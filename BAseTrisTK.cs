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
using BASeTris.TetrisBlocks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SkiaSharp;

namespace BASeTris
{
    public class BASeTrisTK : GameWindow,IStateOwner,IGamePresenter
    {
        private static SKColorType DefaultColorType = SKColorType.Rgba8888;
        GameState.DisplayMode CurrentDisplayMode = GameState.DisplayMode.Partitioned;
        private GamePresenter _Present = null;
        private GRContext context;
        private GRBackendRenderTarget renderTarget;
        SKSurface SkiaSurface = null;
        SKCanvas _Canvas = null;

        //helper routine
        public static GRBackendRenderTarget CreateRenderTarget(GameWindow Window)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int framebuffer);
            GL.GetInteger(GetPName.StencilBits, out int stencil);
            GL.GetInteger(GetPName.Samples, out int samples);
            stencil = stencil == 0 ?1:stencil;
            int bufferWidth = 0;
            int bufferHeight = 0;
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);
            
            return new GRBackendRenderTarget(Window.ClientSize.Width,Window.ClientSize.Height,3,stencil,new GRGlFramebufferInfo((uint)framebuffer,DefaultColorType.ToGlSizedFormat()));
           
        }
        public const int DEFAULT_GAME_WIDTH = 520;
        public const int DEFAULT_STAT_WIDTH = (int)405.6;
        public const int DEFAULT_AREA_HEIGHT = (int)1007.5;

        private BCRect _LastDrawBounds;
        public BCRect LastDrawBounds {  get { return _LastDrawBounds; } }
        public BASeTrisTK(int Width,int Height):base(Width,Height,GraphicsMode.Default,"BASeTris",GameWindowFlags.Default)
        {
            
        }
        private void UpdateSize()
        {
            this.ClientSize = new Size((int)(((float)DEFAULT_GAME_WIDTH + (float)DEFAULT_STAT_WIDTH) * ScaleFactor), (int)((float)DEFAULT_AREA_HEIGHT * ScaleFactor));
            //this.renderTarget = CreateRenderTarget(this);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var glInterface = GRGlInterface.CreateNativeGlInterface();
            Debug.Assert(glInterface.Validate());
            this.context = GRContext.Create(GRBackend.OpenGL, glInterface);
            Debug.Assert(this.context.Handle != IntPtr.Zero);
            this.renderTarget = CreateRenderTarget(this);
            CursorVisible = true;

            _Present = new GamePresenter(this);
            StartGame();
            CurrentState = new GenericMenuState(StandardImageBackgroundSkia.GetStandardBackgroundDrawer(), this, new TitleMenuPopulator()) { StateHeader = "BASeTris" };
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
            _Present.IgnoreController = true;
            if (e.Key == Key.G)
            {
                if (_Present.Game.CurrentState is StandardTetrisGameState)
                {
                    StandardTetrisGameState gs = _Present.Game.CurrentState as StandardTetrisGameState;
                    TetrisBlock[][] inserts = new TetrisBlock[4][];
                    for (int i = 0; i < inserts.Length; i++)
                    {
                        inserts[i] = new TetrisBlock[gs.PlayField.ColCount];
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
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Debug.Print("Button released:" + e.Key);
            var translated = _Present.TranslateKey(e.Key);
            if (translated != null)
            {
                _Present.GameKeyUp(translated.Value);
            }
        }
        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (_Present.Game != null && _Present.Game.CurrentState is IDirectKeyboardInputState)
            {
                var Casted = (IDirectKeyboardInputState)_Present.Game.CurrentState;
                Casted.KeyPressed(this, (Keys)e.KeyChar);
            }
            
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _Present.RunNextThreadAction();
            if (_Present.Game.CurrentState != null && !_Present.Game.CurrentState.GameProcSuspended)
            {
                _Present.Game.GameProc();
            }
            _Present.CIS.CheckState();
            base.OnUpdateFrame(e);
            //run update...
        }
        private void GetHorizontalSizeData(float WindowHeight, float WindowWidth, out float FieldSize, out float StatSize)
        {
            FieldSize = WindowHeight * (332f / 641f);
            StatSize = WindowWidth - FieldSize;
        }
       
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Debug.Print("RenderFrame");
            base.OnRenderFrame(e);
            try
            {
                var CurrentGameState = _Present.Game.CurrentState;
                Title = $"State:{CurrentGameState.GetType().Name}  (Vsync: {VSync}) FPS: {1f / e.Time:0}";

                Color4 backColor;
                backColor.A = 1.0f;
                backColor.R = 0.1f;
                backColor.G = 0.1f;
                backColor.B = 0.3f;
                GL.ClearColor(backColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                using (var surface = SKSurface.Create(this.context, this.renderTarget, GRSurfaceOrigin.BottomLeft, DefaultColorType))
                {
                    Debug.Assert(surface != null);
                    Debug.Assert(surface.Handle != IntPtr.Zero);

                    var canvas = surface.Canvas;

                    canvas.Flush();
                    //canvas.Clear(SKColors.Brown);
                    var info = this.renderTarget;
                    
                    //canvas.Clear(SKColors.Beige);
                   /* 
                    using (SKPaint paint = new SKPaint
                    {
                        Style = SKPaintStyle.StrokeAndFill,
                        Color = SKColors.White,
                        StrokeWidth = 1
                    })
                    {
                        //canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
                        //canvas.DrawCircle(200, 200, 150, paint);
                        canvas.DrawText("Greetings", new SKPoint(50, 50),paint);
                    }
                    */
                    
                    
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
                                canvas.Save(); //save state before setting clip to field.
                                _LastDrawBounds = FieldRect;
                                canvas.ClipRect(FieldRect);
                                staterender.Render(this, canvas, CurrentGameState, new GameStateSkiaDrawParameters(FieldRect));
                                canvas.Restore();
                                //now, call rendder to render the stats.
                                canvas.ClipRect(StatsRect);
                                
                                staterender.RenderStats(this, canvas, CurrentGameState, new GameStateSkiaDrawParameters(StatsRect));
                                //TODO: this needs to be optimized; drawing both the stats and the main window is still slower than the GDI+ implementation which is able to separate the drawing.
                                
                                //staterender.RenderStats(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(FieldWidth, 0, FieldWidth + StatWidth, ClientSize.Height)));
                                //staterender.Render(this, skTetrisField, _Present.Game.CurrentState,
                                //    new GameStateSkiaDrawParameters(new SKRect(0, 0, skTetrisFieldBmp.Width, e.Info.Height)));
                                //staterender.RenderStats(this,skStats,_Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, skStatsBmp.Width, e.Info.Height)));

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
                    
                    //RenderingProvider.Static.DrawElement(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)));
                    //canvas.Flush();
                }
                this.context.Flush();
                SwapBuffers();
            }
            catch(Exception exr)
            {
                ;
            }
            finally
            {
                
            }
        }

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

   
        private double _ScaleFactor = 1;
        public double ScaleFactor { get { return (float)this.ClientSize.Height / 950f; } }
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
        public StandardSettings Settings
        {
            get
            {
                return _Present.GameSettings;
            }
        }
        public DateTime GameStartTime
        {
            get => _Present.Game.GameStartTime;
            set => _Present.Game.GameStartTime = value;
        }

        public TimeSpan FinalGameTime
        {
            get => _Present.Game.FinalGameTime;
            set => _Present.Game.FinalGameTime = value;
        }

        public DateTime LastPausedTime
        {
            get => _Present.Game.LastPausedTime;
            set => _Present.Game.LastPausedTime = value;
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
    }
}
