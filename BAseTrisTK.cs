﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
        public static GRBackendRenderTarget CreateRenderTarget()
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int framebuffer);
            GL.GetInteger(GetPName.StencilBits, out int stencil);
            GL.GetInteger(GetPName.Samples, out int samples);
            int bufferWidth = 0;
            int bufferHeight = 0;
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);

            return new GRBackendRenderTarget(bufferWidth,bufferHeight,samples,stencil,new GRGlFramebufferInfo((uint)framebuffer,DefaultColorType.ToGlSizedFormat()));
           
        }

        public BASeTrisTK():base(800,600,GraphicsMode.Default,"BASeTris",GameWindowFlags.Default)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var glInterface = GRGlInterface.CreateNativeGlInterface();
            Debug.Assert(glInterface.Validate());
            this.context = GRContext.Create(GRBackend.OpenGL, glInterface);
            Debug.Assert(this.context.Handle != IntPtr.Zero);
            this.renderTarget = CreateRenderTarget();
            CursorVisible = true;

            _Present = new GamePresenter(this);
            _Present.StartGame(GamePresenter.GameHandlingConstants.Handle_Manual);
            
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            this.context?.Dispose();
            this.context = null;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_Present.Game.CurrentState != null && !_Present.Game.CurrentState.GameProcSuspended)
            {
                _Present.Game.GameProc();
            }
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
            base.OnRenderFrame(e);

            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.1f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            
            //this.renderTarget.Width = this.Width;
            //this.renderTarget.Height = this.Height;

            using (var surface = SKSurface.Create(this.context, this.renderTarget,GRSurfaceOrigin.TopLeft,DefaultColorType))
            {
                Debug.Assert(surface != null);
                Debug.Assert(surface.Handle != IntPtr.Zero);

                var canvas = surface.Canvas;

                canvas.Flush();

                var info = this.renderTarget;

                //canvas.Clear(SKColors.Beige);

                /*using (SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 25
                })
                {
                    canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = SKColors.Blue;
                    canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
                }
                */
                //RenderingProvider.Static.DrawElement(this, g, CurrentGameState, new GameStateDrawParameters(Bounds));
                if (CurrentState.SupportedDisplayMode == GameState.DisplayMode.Full)
                {
                    var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
                    if (renderer != null)
                    {
                        if (renderer is IStateRenderingHandler staterender)
                        {
                            staterender.Render(this, canvas, _Present.Game.CurrentState,
                                new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)));
                            return;
                        }
                    }
                }
                else if (CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
                {
                    GetHorizontalSizeData(ClientSize.Height, ClientSize.Width, out float FieldWidth, out float StatWidth);
                    var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
                    if (renderer != null)
                    {
                        if (renderer is IStateRenderingHandler staterender)
                        {
                            canvas.Clear(SKColors.Blue);
                            staterender.Render(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, FieldWidth, ClientSize.Height)));
                            //TODO: this needs to be optimized; drawing both the stats and the main window is still slower than the GDI+ implementation which is able to separate the drawing.

                            //staterender.RenderStats(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(FieldWidth, 0, FieldWidth + StatWidth, ClientSize.Height)));
                            //staterender.Render(this, skTetrisField, _Present.Game.CurrentState,
                            //    new GameStateSkiaDrawParameters(new SKRect(0, 0, skTetrisFieldBmp.Width, e.Info.Height)));
                            //staterender.RenderStats(this,skStats,_Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, skStatsBmp.Width, e.Info.Height)));

                        }
                    }
                }
                //RenderingProvider.Static.DrawElement(this, canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)));
                canvas.Flush();
            }
            this.context.Flush();
            SwapBuffers();
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

        public Rectangle GameArea { get { return this.ClientRectangle; } }
        public void Feedback(float Strength, int Length)
        {
            _Present.Feedback(Strength, Length);
        }

        public void AddGameObject(GameObject Source)
        {
            _Present.GameObjects.Add(Source);
        }
        private double _ScaleFactor = 1;
        public double ScaleFactor { get { return _ScaleFactor; } }
        public void SetScale(double pScale)
        {
            _ScaleFactor = pScale;
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