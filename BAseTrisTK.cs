using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.Skia;
using BASeTris.Blocks;
using BASeTris.Theme.Audio;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SkiaSharp;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Settings;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic.ApplicationServices;
using BASeTris.Rendering.Skia.GameStates;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common.Input;
using System.Configuration;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using TKKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using BASeCamp.Logging;
using System.Collections.Concurrent;
using BASeTris.AssetManager;
using BASeTris.Rendering.FrameBufferEffects;
namespace BASeTris
{
    public class BASeTrisTK : GameWindow,IStateOwner,IGamePresenter, IBufferMultiHistoryProvider<SKImage>
    {
        
        GameState.DisplayMode CurrentDisplayMode = GameState.DisplayMode.Partitioned;
        private GamePresenter _Present = null;
        private GRContext context;
        private GRBackendRenderTarget renderTarget;
        SKSurface SkiaSurface = null;
        SKCanvas _Canvas = null;
        public event EventHandler<GameClosingEventArgs> GameClosing;
        public GameplayRecord GameRecorder
        {
            get => _Present.Game.GameRecorder;
            set => _Present.Game.GameRecorder = value;
        }
        private void FireGameClosing()
        {
            GameClosing?.Invoke(this, new GameClosingEventArgs(this));
        }
        public AudioThemeManager AudioThemeMan { get { return _Present.AudioThemeMan; } set { _Present.AudioThemeMan = value; } }
        //helper routine
        public GRBackendRenderTarget CreateRenderTarget(GameWindow Window)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int framebuffer);
            GL.GetInteger(GetPName.StencilBits, out int stencil);
            GL.GetInteger(GetPName.Samples, out int samples);

            
            //todo: diagnose issues with Intel graphics
            //we need fallback "sets" of samples and stencil values, I suspect, possibly also the ColorType options.
            //Should additionally be something that can be configured, too.
            return new GRBackendRenderTarget(Window.ClientSize.X, Window.ClientSize.Y, 1, 8, new GRGlFramebufferInfo((uint)framebuffer, GlobalResources.DefaultColorType.ToGlSizedFormat())  );
           
        }
        public const int DEFAULT_GAME_WIDTH = 520;
        public const int DEFAULT_STAT_WIDTH = (int)405.6;
        public const int DEFAULT_AREA_HEIGHT = (int)1007.5;
        private SkiaRenderAssistant _RenderAssist = new SkiaRenderAssistant();

        
        public BCRect LastDrawBounds {  get { return _RenderAssist.LastDrawBounds; } }
        public BASeTrisTK(int Width, int Height) : base(new GameWindowSettings() { }, new NativeWindowSettings() {  Flags = ContextFlags.Default | ContextFlags.Debug, Profile = ContextProfile.Core, Vsync = VSyncMode.Adaptive,Size = new OpenTK.Mathematics.Vector2i((int)(Width),(int)(Height)) })
        {
            
        }
        public Type GetCanvasType()
        {
            return typeof(SKCanvas);
        }
        private Thread SizeUpdaterThread = null;
        private long LastSizeUpdateTickCount = 0;
        private bool BlockDisplay = false;
        private void UpdateSize()
        {
            //this.Location = new Point(140);
            var makesize = new Size((int)(((float)DEFAULT_GAME_WIDTH + (float)DEFAULT_STAT_WIDTH) * ScaleFactor), (int)((float)DEFAULT_AREA_HEIGHT * ScaleFactor));
            Debug.Print($"Scale Size: Factor:{ScaleFactor} - {makesize.ToString()}");
            this.ClientSize = new OpenTK.Mathematics.Vector2i(makesize.Width,makesize.Height);
            LastSizeUpdateTickCount = TetrisGame.GetTickCount();
            BlockDisplay = true; 
            if (SizeUpdaterThread == null)
            {
                SizeUpdaterThread = new Thread(() =>
                {
                    while (TetrisGame.GetTickCount() < LastSizeUpdateTickCount + 100)
                    {
                        Thread.Sleep(0);
                    }
                    _Present.EnqueueAction(() =>
                    {
                        InitializeGraphics();
                        BlockDisplay = false;
                        return false;
                    });
                    SizeUpdaterThread = null;
                });
            }
            SizeUpdaterThread.Start();

            
            //this.renderTarget = CreateRenderTarget(this);
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();
            //GlobalResources.OpenGLInterface = GRGlInterface.CreateNativeGlInterface();
            GlobalResources.OpenGLInterface = GRGlInterface.Create();
            Debug.Assert(GlobalResources.OpenGLInterface.Validate());
            InitializeGraphics();
            this.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
            //CursorVisible = false;

            //Icon = OpenTK.Windowing.Common.Input.WindowIcon.   Properties.Resources.AppIcon;
            //todo: fix this
            //Icon = null; // Properties.Resources.AppIcon;
            var AppIconBitmap = Properties.Resources.AppIcon.ToBitmap();
            var bitmapData = AppIconBitmap.LockBits(new Rectangle(0, 0, AppIconBitmap.Width, AppIconBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite,AppIconBitmap.PixelFormat);
            int length = bitmapData.Stride * bitmapData.Height;
            byte[] bitmapbytes = new byte[length];
            Marshal.Copy(bitmapData.Scan0, bitmapbytes, 0, length);
            Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(AppIconBitmap.Width, AppIconBitmap.Height, bitmapbytes));

            Location = new OpenTK.Mathematics.Vector2i(Location.X, 0);
            _Present = new GamePresenter(this);
            _ScaleFactor = Math.Round(((float)(this.ClientSize.Y) / 950f),1);
            StartGame();

            //should be initialized enough for test code....

            //var testbitmap = TetrominoCollageRenderer.GetBackgroundCollage(new GameBoyTetrominoTheme());
            /* var testbitmap = TetrominoCollageRenderer.GetNominoBitmap(new GameBoyTetrominoTheme());

             using (var data = testbitmap.Encode(SKEncodedImageFormat.Png, 80))
             {
                 using (var writer = new FileStream("T:\\testout.png",FileMode.Create))
                 {
                     data.SaveTo(writer);
                 }
             }*/
            this.Resize += BASeTrisTK_Resize;


            //var useBG = new StarfieldBackgroundSkia(new StarfieldBackgroundSkiaCapsule());
            var useBG = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
            CurrentState = new TitleMenuState(useBG, this);
        }

        private void BASeTrisTK_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            GL.Viewport(0, 0, obj.Width, obj.Height);
        }
        private void InitializeGraphics()
        {
            var oldcontext = this.context;
            var oldtarget = this.renderTarget;
            
            this.context = GRContext.CreateGl(GlobalResources.OpenGLInterface); //GRContext.Create(GRBackend.OpenGL, GlobalResources.OpenGLInterface);
            if (this.context == null)
            {
                var GLError = GL.GetError();
                Debug.Print($"InitializeGraphics failed: GL Error: {GLError.ToString()}");
                Debug.Assert(this.context.Handle != IntPtr.Zero);
            }
            
            this.renderTarget = CreateRenderTarget(this);
            if (oldcontext != null) oldcontext.Dispose();
            if (oldtarget != null) oldtarget.Dispose();
            //CursorVisible = false;
        }
        public void StartGame()
        {
            _Present.StartGame(GamePresenter.GameHandlingConstants.Handle_Manual);
           
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            this.context?.Dispose();
            this.context = null;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            OnClosed();
        }
        
        void OnClosed()
        {
            FireGameClosing();
            if (_Present.GameThread != null)
                _Present.GameThread.Abort();
            if (_Present.InputThread != null)
            {
                //_Present.InputThread.Abort();
            }

            //if (XInput.Wrapper.X.IsAvailable)
            //{
                XInput.Wrapper.X.StopPolling();
            //}

            if (_Present.ai != null) _Present.ai.AbortAI();
            TetrisGame.Soundman.StopMusic();
            //this.Close();
            GameWindow gw;
            
        }


        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            _Present.IgnoreController = true;

            if (_Present.Game != null && _Present.Game.CurrentState is IDirectKeyboardInputState Casted && Casted.AllowDirectKeyboardInput())
            {

                Casted.KeyDown(this, (int)e.Key);

            }
            else
            {

                if (e.Key == TKKey.G)
                {
                    if (_Present.Game.CurrentState is GameplayGameState)
                    {
                        GameplayGameState gs = _Present.Game.CurrentState as GameplayGameState;
                        NominoBlock[][] inserts = new NominoBlock[4][];
                        for (int i = 0; i < inserts.Length; i++)
                        {
                            inserts[i] = new NominoBlock[gs.PlayField.ColCount];
                            for (int c = 1; c < inserts[i].Length; c++)
                            {
                                inserts[i][c] = new StandardColouredBlock() { BlockColor = Color.Red, DisplayStyle = StandardColouredBlock.BlockStyle.Style_CloudBevel };
                            }
                        }

                        InsertBlockRowsActionGameState irs = new InsertBlockRowsActionGameState(gs, 0, inserts, Enumerable.Empty<Action>());
                        CurrentState = irs;
                    }
                }
                else if (e.Key == TKKey.C)
                {
                    if (e.Shift && e.Control)
                    {
                        EnterCheatState cheatstate = new EnterCheatState(CurrentState, _Present.Game, 64);
                        CurrentState = cheatstate;
                    }
                }

                Debug.Print("Button pressed:" + e.Key);
                var translated = _Present.TranslateKey(e.Key);
                if (translated != null)
                {
                    _Present.Game.HandleGameKey(this, translated.Value, TetrisGame.KeyInputSource.Input_HID);
                    _Present.GameKeyDown(translated.Value);

                }
            }
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            if (_Present.Game.CurrentState is IMouseInputState imis)
            {
                imis.MouseDown(this,MouseInputStateHelper.TranslateButton(e.Button), new BCPoint(LastMousePosition.X, LastMousePosition.Y));
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            if (_Present.Game.CurrentState is IMouseInputState imis)
            {
                imis.MouseUp(this,MouseInputStateHelper.TranslateButton(e.Button), new BCPoint(LastMousePosition.X, LastMousePosition.Y));
            }
        }
        OpenTK.Mathematics.Vector2 LastMousePosition = OpenTK.Mathematics.Vector2.Zero;
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            LastMousePosition = e.Position;
            if (_Present.UserInputDisabled) return;
            if (_Present.Game.CurrentState is IMouseInputState imis)
            {
                imis.MouseMove(this,new BCPoint(e.Position.X, e.Position.Y));
            }
        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            Debug.Print("Button released:" + e.Key);
            if (_Present.Game != null && _Present.Game.CurrentState is IDirectKeyboardInputState Casted && Casted.AllowDirectKeyboardInput())
            {

                Casted.KeyUp(this, (int)e.Key);
                

            }
            else
            {
                var translated = _Present.TranslateKey(e.Key);
                if (translated != null)
                {
                    if(_Present.Game.CurrentState.AllowUserGameKey(translated.Value))
                        _Present.GameKeyUp(translated.Value);
                }
            }
        }
        //TODO: fix keypress delegation!
        /*protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            if (_Present.Game != null &&  _Present.Game.CurrentState is IDirectKeyboardInputState Casted && Casted.AllowDirectKeyboardInput())
            {
                Casted.KeyPressed(this, (int)e.KeyChar);
            }
            
        }*/
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _Present.RunNextThreadAction();
            if (_Present.Game.CurrentState != null && !_Present.Game.CurrentState.GameProcSuspended)
            {
                _Present.Game.GameProc();
            }
            //_Present.CIS.CheckState();
            base.OnUpdateFrame(e);
            //run update...
        }
        
        public double FrameTime { get { if (CurrentFrameData == null) return 0; return CurrentFrameData.Value.Time; } }
        private FrameEventArgs? LastFrameData = null;
        private FrameEventArgs? CurrentFrameData = null;
        private IBlockGameCustomizationHandler HandlerTitleSet = null;
        private String sDisplayVersion = null,sDisplayHash = null;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            LastFrameData = CurrentFrameData;
            CurrentFrameData = e;
            if (!TetrisGame.Imageman.ImagePrepped) return;
            base.OnRenderFrame(e);
            try
            {
                var CurrentGameState = _Present.Game.CurrentState;
                //int ParticleCount = CurrentGameState is GameplayGameState ? (CurrentGameState as GameplayGameState).Particles.Count : CurrentGameState is ICompositeState<GameplayGameState>?(CurrentGameState as ICompositeState<GameplayGameState>).GetComposite().Particles.Count :0;
                Title = "BASeTris";
                var handler = (this as IStateOwner).GetHandler();

                

                if (handler!=null && handler!=HandlerTitleSet)
                {
                    HandlerTitleSet = handler;
                    if (CurrentGameState is GameplayGameState)
                    {
                        Title += " - " + handler.Name;
                    }
                    else if (CurrentGameState is PauseGameState p)
                    {
                        Title += " - " + handler.Name + " (Paused)";
                    }
                    else if (CurrentGameState is TemporaryInputPauseGameState)
                    {
                        Title += " - " + handler.Name;
                    }
                    else if (CurrentGameState is ICompositeState<GameplayGameState> comp)
                    {
                        Title += " - " + handler.Name;
                    }


                    
                }
                //Title = $"FPS: {1f / e.Time:0} State:{CurrentGameState.GetType().Name}";
                
                Color4 backColor;
                backColor.A = 1f;//1.0f;
                backColor.R = 0f;// 0.1f;
                backColor.G = 0f;// 0.1f;
                backColor.B = 0f; //0.3f;
                
                GL.ClearColor(backColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                if (BlockDisplay) return;
                //SKSurface.Create(this.context, this.renderTarget, GRSurfaceOrigin.BottomLeft, GlobalResources.DefaultColorType);
                using (var surface = SKSurface.Create(this.context, this.renderTarget, GRSurfaceOrigin.BottomLeft, GlobalResources.DefaultColorType))
                {
                    if (surface == null) return;
                    //Debug.Assert(surface != null);
                    //Debug.Assert(surface.Handle != IntPtr.Zero);

                    var canvas = surface.Canvas;

                    canvas.Flush();
                    //canvas.Clear(SKColors.Brown);
                    var info = this.renderTarget;
                    _RenderAssist.PaintStateSkia(this, CurrentGameState, ClientSize, canvas);
                    //PaintStateSkia(CurrentGameState, canvas);
                    if (FPSPaint == null)
                    {
                        FPSPaint = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = (int)(12 * ScaleFactor), Color = SKColors.Black };
                        FPSShadow = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = (int)(12 * ScaleFactor), Color = SKColors.White };
                    }
                    SKRect FPSBound = new SKRect();
                    double Framerate = (1 / e.Time);
                    String sVersion;
                    String sFPS = String.Format("{0:0.0} FPS", Framerate);
                    FPSPaint.MeasureText(sFPS, ref FPSBound);
                    var FPSPosition = new SKPoint(ClientSize.X - (FPSBound.Width), ClientSize.Y - (FPSBound.Height / 2));
                    //FPSPosition = new SKPoint(50, 50);

                    var asm = typeof(AssemblyInfo).Assembly;
                    var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
                    if (sDisplayVersion == null)
                    {
                        sDisplayVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                        sDisplayHash = attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;

                        if (sDisplayHash != null)
                        {
                            sDisplayVersion += " - " + sDisplayHash;

                        }
                    }
                    canvas.DrawText(sDisplayVersion, new SKPoint(12, FPSPosition.Y), FPSShadow);
                    canvas.DrawText(sDisplayVersion, new SKPoint(9, FPSPosition.Y - 3), FPSPaint);
                    canvas.DrawText(sFPS, FPSPosition, FPSShadow);
                    canvas.DrawText(sFPS, new SKPoint(FPSPosition.X - 3, FPSPosition.Y - 3), FPSPaint);

                    _Present.ProcessRenderTags((r) =>
                    {
                        RenderingProvider.Static.DrawElement(this, canvas, r, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.X, ClientSize.Y)));
                        return r.DoRender();
                    });


                    //if (_BufferEffect == null) _BufferEffect = new GhostlyFrameBufferEffect(this);

                    bool IsEffectFrame = _BufferEffect!=null && _BufferEffect.IsEffectFrame(TetrisGame.GetTickCount());
                    SKImage NewSnapShot = null;
                        
                    

                    
                    //now, render previous frames.
                    int CurrentFrame = 0;
                    if (_BufferEffect!=null && _BufferEffect.NumGhostedFrames > 0 && FrameCount>0)
                    {
                        if (_BufferEffect.InitializationRequired()) _BufferEffect.Initialize();
                        lock (_BufferEffect.GhostObjectLock)
                        {
                            foreach (var iterate in GetFrames().Reverse())
                            {
                                canvas.DrawImage(iterate, new SKRect(0, 0, ClientSize.X, ClientSize.Y), _BufferEffect.PaintItem(CurrentFrame));

                            }
                        }
                        CurrentFrame++;
                    }
                    if (IsEffectFrame)
                        NewSnapShot = surface.Snapshot();
                    if (IsEffectFrame && _BufferEffect!=null)
                    {
                        EFBQueue.AddFrame(NewSnapShot, TetrisGame.GetTickCount());
                        //_BufferEffect.AddFrame(NewSnapShot,TetrisGame.GetTickCount()); //note, as long as the start alpha is low on the ghost paints, adding the snapshot after drawing them can give a wacky dream effect.
                    }
                    //RenderingProvider.Static.DrawElement(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)));
                    //canvas.Flush();
                }
                this.context.Flush();
                SwapBuffers();
            }
            catch(Exception exr)
            {
                Debug.Print("Render Exception:" + exr.ToString());
                ;
            }
            finally
            {
                
            }
        }
        private GhostlyFrameBufferEffect _BufferEffect = null;
       /* long FrameTickDelay = 5;
        ulong LastGhostFrameTick = 0;
        Object GhostObjectLock = new object();
        int GhostAlphaPaintCount = 0;
        SKPaint[] GhostAlphaPaint = null;
        private void PrepareAlphaPaints()
        {
            lock (GhostObjectLock)
            {
                if(GhostAlphaPaint!=null) foreach (var iteratepaint in GhostAlphaPaint)
                {
                    //if (iteratepaint != null) iteratepaint.Dispose();
                }
                GhostAlphaPaint = new SKPaint[NumGhostedFrames];
                for (int i = 0; i < NumGhostedFrames; i++)
                {
                    float UseAlpha = GhostStartAlpha+((GhostStartAlpha - GhostEndAlpha) / (float)NumGhostedFrames) * (float)i;
                    SKPaint BuildAlphaPaint = new SKPaint() { ColorFilter = SKColorMatrices.GetFader(UseAlpha) };
                    GhostAlphaPaint[i] = BuildAlphaPaint;
                }
            }
            GhostAlphaPaintCount = NumGhostedFrames;
        }
        const float GhostStartAlpha = .9f;
        const float GhostEndAlpha = .9f;
        private void AddFrame(SKImage SurfaceSnapshot)
        {
            if (NumGhostedFrames > 0)
            {
                SKImage grabimage = SurfaceSnapshot;
                lock (PreviousFrames)
                {
                    PreviousFrames.Enqueue(grabimage);
                }
                while (PreviousFrames.Count > NumGhostedFrames)
                {
                    SKImage getevicted = null;
                    PreviousFrames.TryDequeue(out getevicted);
                    if (grabimage == getevicted)
                    {
                        ;
                    }
                    getevicted.Dispose();
                }
            }
        }
        public int NumGhostedFrames = 0; //experimental. This probably needs to be changed to be based on time instead of a specific count of frames. 0=disabled.
        ConcurrentQueue<SKImage> PreviousFrames = new ConcurrentQueue<SKImage>();*/
        /*private void PaintStateSkia(GameState CurrentGameState, SKCanvas canvas)
        {
            if (CurrentGameState.SupportedDisplayMode == GameState.DisplayMode.Full)
            {
                canvas.Clear(SKColors.Pink);
                var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), CurrentGameState.GetType(), typeof(GameStateSkiaDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        canvas.Save();
                        var FullRect = new SKRect(0, 0, ClientSize.X, ClientSize.Y);
                        canvas.ClipRect(FullRect);
                        staterender.Render(this, canvas, CurrentGameState,
                            new GameStateSkiaDrawParameters(FullRect));
                        //canvas.DrawLine(new SKPoint(0, 0), new SKPoint(ClientSize.Width, ClientSize.Height), new SKPaint() { Color = SKColors.Black });
                        canvas.Restore();
                        _LastDrawBounds = FullRect;

                    }
                }
            }
            else if (CurrentGameState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
            {
                SKRect LastDraw;
                //StandardTetrisGameStateSkiaRenderingHandler.PaintPartitionedState(this, CurrentGameState, canvas, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)),out LastDraw,out _);
                //_LastDrawBounds = LastDraw;
                PaintPartitionedState(CurrentGameState, canvas);
            }
        }*/
        

        /*private void PaintPartitionedState(GameState PaintState, SKCanvas canvas)
        {
            RenderHelpers.GetHorizontalSizeData(ClientSize.Y, ClientSize.X, out float FieldWidth, out float StatWidth);
            var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), PaintState.GetType(), typeof(GameStateSkiaDrawParameters));
            if (renderer != null)
            {
                if (renderer is IStateRenderingHandler staterender)
                {
                    SKRect FieldRect = new SKRect(0, 0, FieldWidth, ClientSize.Y);
                    SKRect StatsRect = new SKRect(FieldWidth, 0, FieldWidth + StatWidth, ClientSize.Y);
                    _LastDrawBounds = FieldRect;

                    using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(FieldRect);
                        staterender.Render(this, canvas, PaintState, new GameStateSkiaDrawParameters(FieldRect));
                    }
                    using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(StatsRect);
                        staterender.RenderStats(this, canvas, PaintState, new GameStateSkiaDrawParameters(StatsRect));
                    }

                }
                else
                {
                    ;
                }
            }
            else
            {
                ;
            }
        }*/

        SKPaint FPSPaint = null;
        SKPaint FPSShadow = null;
        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            CurrentDisplayMode = pMode;
        }

        public GameState CurrentState
        {
            get { return _Present.Game?.CurrentState; }
            set { _Present.Game.CurrentState = value; }
        }

        public void EnqueueAction(Func<bool> pAction)
        {
            _Present.EnqueueAction(pAction);
        }

        public Rectangle GameArea { get {

                RenderHelpers.GetHorizontalSizeData(ClientSize.Y, ClientSize.X, out float FieldWidth, out float StatWidth);

                return new Rectangle(0,0,(int)FieldWidth,ClientSize.Y);
            } }
        public void Feedback(float Strength, int Length)
        {
            _Present.Feedback(Strength, Length);
        }

        //return ((float)(this.ClientSize.Height) / 950f)*
        private double _ScaleFactor = 1;
        public double ScaleFactor { get { return _ScaleFactor; } }
        public void SetScale(double pScale)
        {
            _ScaleFactor = pScale;
            UpdateSize();
        }
        public event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange
        {
            add => _Present.Game.BeforeGameStateChange += value;
            remove => _Present.Game.BeforeGameStateChange -= value;
        }
        public SettingsManager Settings
        {
            get
            {
                return _Present.GameSettings;
            }
        }

        public Stopwatch GameTime
        {
            get => _Present.Game.GameTime;
            set => _Present.Game.GameTime = value;

        }

        public TimeSpan FinalGameTime
        {
            get => _Present.Game.FinalGameTime;
            set => _Present.Game.FinalGameTime = value;
        }


        public TimeSpan GetElapsedTime()
        {
            return _Present.Game.GetElapsedTime();
        }


        HashSet<Keys> PressedKeys = new HashSet<Keys>();

        public void Present()
        {
            //for OpenTK I don't believe we can really do much here, since we paint every frame.
            //throw new NotImplementedException();
        }

        public GamePresenter GetPresenter()
        {
            return _Present;
        }
        public IEnumerable<SKImage> GetFrames()
        {
            return EFBQueue.Frames;
            
        }

        public SKImage GetLastFrame()
        {
            return EFBQueue.Frames.Last();
        }
        public int FrameCount { get { return EFBQueue.FrameCount; } }
        

        public ulong LastFrameTick { get { return EFBQueue.LastFrameTick; } set { EFBQueue.LastFrameTick = value; } }

        FrameBufferRecorderQueue<SKImage> EFBQueue = new FrameBufferRecorderQueue<SKImage>();

        FrameBufferRecorderQueue<SKImage> EFBBackgroundQueue = new FrameBufferRecorderQueue<SKImage>();


        public void AcceptCallback(GamePresenterCallbackCapsule pCapsule)
        {
        }

    }
}
