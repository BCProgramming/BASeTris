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

namespace BASeTris
{
    public class BASeTrisTK : GameWindow,IStateOwner,IGamePresenter
    {
        
        GameState.DisplayMode CurrentDisplayMode = GameState.DisplayMode.Partitioned;
        private GamePresenter _Present = null;
        private GRContext context;
        private GRBackendRenderTarget renderTarget;
        SKSurface SkiaSurface = null;
        SKCanvas _Canvas = null;
        public event EventHandler<GameClosingEventArgs> GameClosing;
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
            stencil = stencil == 0 ?1:stencil;
            int bufferWidth = 0;
            int bufferHeight = 0;
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);
            samples = 1;
            //todo: diagnose issues with Intel graphics
            return new GRBackendRenderTarget(Window.ClientSize.Width, Window.ClientSize.Height, samples, stencil, new GRGlFramebufferInfo((uint)framebuffer, GlobalResources.DefaultColorType.ToGlSizedFormat())  );
           
        }
        public const int DEFAULT_GAME_WIDTH = 520;
        public const int DEFAULT_STAT_WIDTH = (int)405.6;
        public const int DEFAULT_AREA_HEIGHT = (int)1007.5;

        private BCRect _LastDrawBounds;
        public BCRect LastDrawBounds {  get { return _LastDrawBounds; } }
        public BASeTrisTK(int Width,int Height):base(Width,Height,GraphicsMode.Default,"BASeTris",GameWindowFlags.FixedWindow)
        {
            
        }
        public Type GetCanvasType()
        {
            return typeof(SKCanvas);
        }
        private void UpdateSize()
        {
            //this.Location = new Point(140);
            var makesize = new Size((int)(((float)DEFAULT_GAME_WIDTH + (float)DEFAULT_STAT_WIDTH) * ScaleFactor), (int)((float)DEFAULT_AREA_HEIGHT * ScaleFactor));
            Debug.Print($"Scale Size: Factor:{ScaleFactor} - {makesize.ToString()}");
            this.ClientSize = makesize;
            InitializeGraphics();
            //this.renderTarget = CreateRenderTarget(this);
        }
     
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //GlobalResources.OpenGLInterface = GRGlInterface.CreateNativeGlInterface();
            GlobalResources.OpenGLInterface = GRGlInterface.Create();
            Debug.Assert(GlobalResources.OpenGLInterface.Validate());
            InitializeGraphics();
            CursorVisible = false;

            Icon = Properties.Resources.AppIcon;
            Location = new Point(Location.X, 0);
            _Present = new GamePresenter(this);
            _ScaleFactor = Math.Round(((float)(this.ClientSize.Height) / 950f),1);
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
            


            //var useBG = new StarfieldBackgroundSkia(new StarfieldBackgroundSkiaCapsule());
            var useBG = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
            CurrentState = new TitleMenuState(useBG, this);
        }
        private void InitializeGraphics()
        {
            var oldcontext = this.context;
            var oldtarget = this.renderTarget;
            this.context = GRContext.CreateGl(GlobalResources.OpenGLInterface); //GRContext.Create(GRBackend.OpenGL, GlobalResources.OpenGLInterface);
            Debug.Assert(this.context.Handle != IntPtr.Zero);
            this.renderTarget = CreateRenderTarget(this);
            if (oldcontext != null) oldcontext.Dispose();
            if (oldtarget != null) oldtarget.Dispose();
            CursorVisible = false;
        }
        public void StartGame()
        {
            _Present.StartGame(GamePresenter.GameHandlingConstants.Handle_Manual);
           
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            this.context?.Dispose();
            this.context = null;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            FireGameClosing();
            if (_Present.GameThread != null)
                _Present.GameThread.Abort();
            if (_Present.InputThread != null)
            {
                _Present.InputThread.Abort();
            }

            if (XInput.Wrapper.X.IsAvailable)
            {
                XInput.Wrapper.X.StopPolling();
            }

            if (_Present.ai != null) _Present.ai.AbortAI();
            TetrisGame.Soundman.StopMusic();
            Exit();
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
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

                if (e.Key == Key.G)
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
                else if (e.Key == Key.C)
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
                imis.MouseDown(this,MouseInputStateHelper.TranslateButton(e.Button), new BCPoint(e.Position.X,e.Position.Y));
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            if (_Present.Game.CurrentState is IMouseInputState imis)
            {
                imis.MouseUp(this,MouseInputStateHelper.TranslateButton(e.Button), new BCPoint(e.Position.X, e.Position.Y));
            }
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
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
        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (_Present.UserInputDisabled) return;
            if (_Present.Game != null &&  _Present.Game.CurrentState is IDirectKeyboardInputState Casted && Casted.AllowDirectKeyboardInput())
            {
                Casted.KeyPressed(this, (int)e.KeyChar);
            }
            
        }
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
        private void GetHorizontalSizeData(float WindowHeight, float WindowWidth, out float FieldSize, out float StatSize)
        {
            FieldSize = WindowHeight * (332f / 641f);
            StatSize = WindowWidth - FieldSize;
        }
        public double FrameTime { get { if (CurrentFrameData == null) return 0; return CurrentFrameData.Time; } }
        private FrameEventArgs LastFrameData = null;
        private FrameEventArgs CurrentFrameData = null;
        private IBlockGameCustomizationHandler HandlerTitleSet = null;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            LastFrameData = CurrentFrameData;
            CurrentFrameData = e;
            if (!TetrisGame.Imageman.ImagePrepped) return;
            base.OnRenderFrame(e);
            try
            {
                var CurrentGameState = _Present.Game.CurrentState;
                if(CurrentGameState is FieldLineActionGameState)
                {
                    ;
                }
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
                using (var surface = SKSurface.Create(this.context, this.renderTarget, GRSurfaceOrigin.BottomLeft, GlobalResources.DefaultColorType))
                {
                    Debug.Assert(surface != null);
                    Debug.Assert(surface.Handle != IntPtr.Zero);

                    var canvas = surface.Canvas;

                    canvas.Flush();
                    //canvas.Clear(SKColors.Brown);
                    var info = this.renderTarget;
                    
                    if (CurrentGameState.SupportedDisplayMode == GameState.DisplayMode.Full)
                    {
                        canvas.Clear(SKColors.Pink);
                        var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), CurrentGameState.GetType(), typeof(GameStateSkiaDrawParameters));
                        if (renderer != null)
                        {
                            if (renderer is IStateRenderingHandler staterender)
                            {
                                canvas.Save();
                                var FullRect = new SKRect(0, 0, ClientSize.Width, ClientSize.Height);
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
                        GetHorizontalSizeData(ClientSize.Height, ClientSize.Width, out float FieldWidth, out float StatWidth);
                        var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), CurrentGameState.GetType(), typeof(GameStateSkiaDrawParameters));
                        if (renderer != null)
                        {
                            if (renderer is IStateRenderingHandler staterender)
                            {
                                SKRect FieldRect = new SKRect(0, 0, FieldWidth, ClientSize.Height);
                                SKRect StatsRect = new SKRect(FieldWidth, 0, FieldWidth + StatWidth, ClientSize.Height);
                                //canvas.Clear(SKColors.Blue);
                                //canvas.Save(); //save state before setting clip to field.
                                _LastDrawBounds = FieldRect;
                                
                                using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                                {
                                    canvas.ClipRect(FieldRect);
                                    staterender.Render(this, canvas, CurrentGameState, new GameStateSkiaDrawParameters(FieldRect));
                                }
                                //canvas.Restore();
                                
                                //now, call rendder to render the stats.
                                
                                using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                                {
                                    canvas.ClipRect(StatsRect);
                                    staterender.RenderStats(this, canvas, CurrentGameState, new GameStateSkiaDrawParameters(StatsRect));
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
                    }
                    if (FPSPaint == null)
                    {
                        FPSPaint = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = (int)(12 * ScaleFactor), Color = SKColors.Black };
                        FPSShadow = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize =(int)(12*ScaleFactor), Color = SKColors.White };
                    }
                    SKRect FPSBound = new SKRect();
                    double Framerate = (1 / e.Time);
                    String sVersion;
                    String sFPS = String.Format("{0:0.0} FPS", Framerate);
                    FPSPaint.MeasureText(sFPS, ref FPSBound);
                    var FPSPosition = new SKPoint(ClientSize.Width - (FPSBound.Width ), ClientSize.Height - (FPSBound.Height/2 ));
                    //FPSPosition = new SKPoint(50, 50);

                    var asm = typeof(AssemblyInfo).Assembly;
                    var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
                    String sDisplayVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    String sHash = attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;

                    if (sHash != null)
                    {
                        sDisplayVersion += " - " + sHash;
                        
                    }
                    canvas.DrawText(sDisplayVersion, new SKPoint(12, FPSPosition.Y), FPSShadow);
                    canvas.DrawText(sDisplayVersion, new SKPoint(9, FPSPosition.Y - 3), FPSPaint);
                    canvas.DrawText(sFPS, FPSPosition, FPSShadow);
                    canvas.DrawText(sFPS, new SKPoint(FPSPosition.X - 3, FPSPosition.Y - 3), FPSPaint);

                    _Present.ProcessRenderTags((r) =>
                    {
                        RenderingProvider.Static.DrawElement(this, canvas, r, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)));
                        return r.DoRender();
                    });
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

        public void EnqueueAction(Action pAction)
        {
            _Present.EnqueueAction(pAction);
        }

        public Rectangle GameArea { get {

                GetHorizontalSizeData(ClientSize.Height, ClientSize.Width, out float FieldWidth, out float StatWidth);

                return new Rectangle(0,0,(int)FieldWidth,ClientSize.Height);
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
    }
}
