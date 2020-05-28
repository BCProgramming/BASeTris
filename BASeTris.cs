using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseTris;
using BASeCamp.BASeScores;
using BASeTris.AI;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.Choosers.AIChoosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Rendering.Skia;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using OpenGL;
using OpenTK.Platform;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using XInput.Wrapper;

namespace BASeTris
{
    
    //IStateOwner in the form itself.
    //This is necessary so we can also have an implementation via the OpenTK GameWindow. 
    public partial class BASeTris : Form, IStateOwner,IGamePresenter
    {
        private GamePresenter _Present;
        //delegate the BeforeGameStateChange event...
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

        public BASeTris()
        {
            InitializeComponent();
        }

        public void AddGameObject(GameObject go)
        {
            _Present.GameObjects.Add(go);
        }

      

        public double ScaleFactor
        {
            get { return current_factor; }
        }

        public Rectangle GameArea
        {
            get { return picTetrisField.ClientRectangle; }
        }

        public static readonly double DefaultWidth = 643d;
        public static readonly double DefaultHeight = 734d;

        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            if (InvokeRequired) Invoke((MethodInvoker)(() => SetDisplayMode(pMode)));
            if (ActiveRenderMode == RendererMode.Renderer_GDIPlus)
            {
                if (picFullSize.Visible == (pMode == GameState.DisplayMode.Full)) return; //if full size visibility matches the passed state being full, we are already in that state.
                picFullSize.Visible = pMode == GameState.DisplayMode.Full;
                picTetrisField.Visible = pMode == GameState.DisplayMode.Partitioned;
                picStatistics.Visible = pMode == GameState.DisplayMode.Partitioned;
            }
            else
            {
                picFullSize.Visible = picTetrisField.Visible = picStatistics.Visible = false;
                
            }
        }

        private double current_factor = 1;

        public void SetScale(double factor)
        {
            current_factor = factor;
            
            Invoke((MethodInvoker)(()=>
            {
                mnuScale_Tiny.Checked = mnuScale_Small.Checked = mnuScale_Large.Checked = mnuScale_Biggliest.Checked = false;
                Size = new Size((int) (DefaultWidth * factor), (int) (DefaultHeight * factor));
            }));
        }

        
        

        private void BASeTris_Load(object sender, EventArgs e)
        {

            PrepareSkia(); //add the Skia Controls.
            _Present = new GamePresenter(this);
            
            //XMLHighScores<NoSpecialInfo> TestScores = new XMLHighScores<NoSpecialInfo>(35000,(r)=>new NoSpecialInfo());
            //int Position1 = TestScores.IsEligible(12000);
            //int Position2 = TestScores.IsEligible(3000);
            
            menuStrip1.Font = new Font(menuStrip1.Font.FontFamily, 28, FontStyle.Regular);
            Win10MenuRenderer buildrender = new Win10MenuRenderer(null, true);

            menuStrip1.Renderer = buildrender;
            menuStrip1.BackColor = SystemColors.Control;
            TetrisGame.InitState();
            
            /*SkiaBitmap = new SKBitmap(new SKImageInfo(picTetrisField.Width, picTetrisField.Height, SKColorType.Rgba8888));
            SKCanvas SkiaCanvas = new SKCanvas(SkiaBitmap);

            StandardColouredBlock scb = new StandardColouredBlock() { DisplayStyle = StandardColouredBlock.BlockStyle.Style_Mottled,BlockColor = Color.Yellow };
            TetrisBlockDrawSkiaParameters tt = new TetrisBlockDrawSkiaParameters(SkiaCanvas, new SKRect(0, 0, 64, 64), null, Settings);
            scb.Rotation = 0;
            scb.DoRotateTransform = true;
            
            RenderingProvider.Static.DrawElement(this, SkiaCanvas, scb, tt);
            SKTypeface st = TetrisGame.RetroFontSK;
            TetrisGame.DrawTextSK(SkiaCanvas, "Testing 1 2 3", new SKPoint(15, 15), st, SkiaSharp.Views.Desktop.Extensions.ToSKColor(Color.Blue), 18, 1d);
            */
            
            

           
        }
        SKBitmap SkiaBitmap = null;
        
        
        bool XPolling = false;
        Nomino testBG = null;

        private void StartGame()
        {

            _Present.StartGame();
            
        }

        public void Present()
        {
            Invoke
            ((MethodInvoker)(() =>
            {
                if (ActiveRenderMode == RendererMode.Renderer_GDIPlus)
                {
                    if (_Present.Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
                    {

                        picTetrisField.Invalidate();
                        picTetrisField.Refresh();
                        picStatistics.Invalidate();
                        picStatistics.Refresh();
                    }
                    else if (_Present.Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Full)
                    {
                        picFullSize.Invalidate();
                        picFullSize.Refresh();
                    }
                }
                else if (ActiveRenderMode == RendererMode.Renderer_SkiaSharp)
                {
                    if (_Present.Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
                    {
                        skFullSize.Invalidate();
                    }
                }
            }));
        }


        
        

        static Random rgen = new Random();

        private void picFullSize_Paint(object sender, PaintEventArgs e)
        {
            
            if (_Present.Game == null) return;
            if (CurrentState.SupportedDisplayMode == GameState.DisplayMode.Full)
            {
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                
                _Present.Game.DrawProc(e.Graphics, new RectangleF(picFullSize.ClientRectangle.Left, picFullSize.ClientRectangle.Top, picFullSize.ClientRectangle.Width, picFullSize.ClientRectangle.Height));
            }
        }

        //this needs to be moved to a new OpenTK GameWindow Presenter implementation
        private void SkFullSize_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_Present.Game == null) return;
            if(CurrentState.SupportedDisplayMode==GameState.DisplayMode.Full)
            {
                e.Surface.Canvas.Clear();
                var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        staterender.Render(this, e.Surface.Canvas, _Present.Game.CurrentState,
                            new GameStateSkiaDrawParameters(new SKRect(0,0,e.Info.Width,e.Info.Height)));
                        return;
                    }
                }
            }
            else if (CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
            {
                //SKControls mess up with multiple, so we need to basically draw the stats as well as the main display onto the same thing.
                //we will paint both to a separate canvas, then merge them together.
                GetHorizontalSizeData(skFullSize.Height,skFullSize.Width,out float FieldWidth,out float StatWidth);
                if(skTetrisField==null) ResetSK();
                skTetrisField.Clear();
                skStats.Clear();
                //note: this approach is too slow. What it needs to do is call both renderers but have it paint directly to the target surface, with the bounds
                //assigned appropriately for each call. Painting the two SKCanvas objects to the main one is too slow, it seems.
                var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        staterender.Render(this,e.Surface.Canvas,_Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, FieldWidth, e.Info.Height)));
                        //TODO: this needs to be optimized; drawing both the stats and the main window is still slower than the GDI+ implementation which is able to separate the drawing.
                        
                        staterender.RenderStats(this, e.Surface.Canvas, _Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(FieldWidth, 0, FieldWidth+StatWidth, e.Info.Height)));
                        //staterender.Render(this, skTetrisField, _Present.Game.CurrentState,
                        //    new GameStateSkiaDrawParameters(new SKRect(0, 0, skTetrisFieldBmp.Width, e.Info.Height)));
                        //staterender.RenderStats(this,skStats,_Present.Game.CurrentState, new GameStateSkiaDrawParameters(new SKRect(0, 0, skStatsBmp.Width, e.Info.Height)));

                    }
                }

                //e.Surface.Canvas.DrawBitmap(skTetrisFieldBmp,0,0);
                //e.Surface.Canvas.DrawBitmap(skStatsBmp,skFullSize.Width-StatWidth,0);

                //with this sizing information we can paint the two canvases.


            }
        }
       
        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            /*e.Graphics.DrawImage(SkiaSharp.Views.Desktop.Extensions.ToBitmap(SkiaBitmap), Point.Empty);
            
            return;*/
            if (_Present.Game == null) return;
            if (picTetrisField.Visible == false) return;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            if(_Present.Game.CurrentState is FieldActionGameState)
            {
                ;
            }
            var renderer = RenderingProvider.Static.GetHandler(typeof(Graphics), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
            if(renderer!=null)
            {
                if(renderer is IStateRenderingHandler staterender)
                {
                    staterender.Render(this, e.Graphics, _Present.Game.CurrentState,
                        new GameStateDrawParameters(new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height)));
                    return;
                }
            }
            //if the above doesn't go through....
            _Present.Game.DrawProc(e.Graphics, new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height));
            
        }
       
     


        private void picStatistics_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentState != null)
            {
                
                if (picStatistics.Visible == false) return;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;

                var renderer = RenderingProvider.Static.GetHandler(typeof(Graphics), _Present.Game.CurrentState.GetType(), typeof(GameStateDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        staterender.RenderStats(this, e.Graphics, _Present.Game.CurrentState,
                            new GameStateDrawParameters(picStatistics.ClientRectangle));
                        return;
                    }
                }

            }
        }

      

        private void picTetrisField_Resize(object sender, EventArgs e)
        {
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }

        private void picTetrisField_Click(object sender, EventArgs e)
        {
            //testBG.Rotate(false);
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }

        private void BASeTris_KeyDown(object sender, KeyEventArgs e)
        {
            _Present.IgnoreController = true;
            if (e.KeyCode == Keys.G)
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
                            inserts[i][c] = new StandardColouredBlock() {BlockColor = Color.Red, DisplayStyle = StandardColouredBlock.BlockStyle.Style_CloudBevel};
                        }
                    }

                    InsertBlockRowsActionGameState irs = new InsertBlockRowsActionGameState(gs, 0, inserts, Enumerable.Empty<Action>());
                    CurrentState = irs;
                }
            }
            else if (e.KeyCode == Keys.C)
            {
                if (e.Shift && e.Control)
                {
                    EnterCheatState cheatstate = new EnterCheatState(CurrentState, _Present.Game, 64);
                    CurrentState = cheatstate;
                }
            }

            Debug.Print("Button pressed:" + e.KeyCode);
            var translated = _Present.TranslateKey(e.KeyCode);
            if (translated != null)
            {
                _Present.Game.HandleGameKey(this, translated.Value, TetrisGame.KeyInputSource.Input_HID);
                _Present.GameKeyDown(translated.Value);
                
            }
        }

        

        
        

        private void BASeTris_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Present.GameThread != null)
                _Present.GameThread.Abort();
            if (_Present.InputThread != null)
            {
                _Present.InputThread.Abort();
            }

            if (X.IsAvailable)
            {
                X.StopPolling();
            }
            
            if (_Present.ai != null) _Present.ai.AbortAI();
            TetrisGame.Soundman.StopMusic();
            Application.Exit();
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

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartGame();
        }

     
        public enum RendererMode
        {
            Renderer_GDIPlus,
            Renderer_SkiaSharp
        }
        //public RendererMode ActiveRenderMode = RendererMode.Renderer_GDIPlus;
        public RendererMode ActiveRenderMode = RendererMode.Renderer_GDIPlus;
        SKControl skFullSize;

        
        SKBitmap skTetrisFieldBmp;
        SKCanvas skTetrisField;
        SKCanvas skStats;
        SKBitmap skStatsBmp;
    

        private void PrepareSkia()
        {/*
            skFullSize = new SKControl();
            skFullSize.Location = picFullSize.Location;
            skFullSize.Size = picFullSize.Size;
            //            skTetrisField.Location = picTetrisField.Location;
            //            skTetrisField.Size = picTetrisField.Size;
            //            skStats.Location = picStatistics.Location;
            //            skStats.Size = picStatistics.Size;
            skFullSize.KeyDown += SkFullSize_KeyDown;
            skFullSize.KeyUp += SkFullSize_KeyUp;
            skFullSize.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
//            skTetrisField.Anchor = picTetrisField.Anchor;
//            skStats.Anchor = picStatistics.Anchor;
            Controls.Add(skFullSize);
            //            Controls.Add(skTetrisField);
            //            Controls.Add(skStats);
            
            //paint routine handlers for Skia.
            skFullSize.PaintSurface += SkFullSize_PaintSurface;
            skFullSize.Resize += SkFullSize_Resize;
            //skTetrisField.PaintSurface += SkTetrisField_PaintSurface;
            //skStats.PaintSurface += SkStats_PaintSurface;
           */ 
        }

        private void SkFullSize_KeyUp(object sender, KeyEventArgs e)
        {
            BASeTris_KeyUp(sender,e);
        }

       

        private void SkFullSize_Resize(object sender, EventArgs e)
        {

            ResetSK();
                //SKCanvas skTetrisField;
                //SKCanvas skStats;
            

        }
        private void ResetSK()
        {
            GetHorizontalSizeData(skFullSize.Height, skFullSize.Width, out float FieldWidth, out float StatWidth);

            SKBitmap Field = new SKBitmap((int)FieldWidth, skFullSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKBitmap Stats = new SKBitmap((int)StatWidth, skFullSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
         
            skTetrisField = new SKCanvas(Field);
      
            skStats = new SKCanvas(Stats);
           
            skTetrisFieldBmp = Field;
            skStatsBmp = Stats;
        }
        private void BASeTris_KeyUp(object sender, KeyEventArgs e)
        {
            Debug.Print("Button released:" + e.KeyCode);
            var translated = _Present.TranslateKey(e.KeyCode);
            if (translated != null)
            {
                _Present.GameKeyUp(translated.Value);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(0.5f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetScale(1);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetScale(1.3f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(1.6f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void BASeTris_ResizeEnd(object sender, EventArgs e)
        {
        }
        private void GetHorizontalSizeData(float WindowHeight,float WindowWidth,out float FieldSize,out float StatSize)
        {
            FieldSize = WindowHeight * (332f / 641f);
            StatSize = WindowWidth - FieldSize;
        }
        private void BASeTris_SizeChanged(object sender, EventArgs e)
        {
            picTetrisField.Width = (int) (picTetrisField.Height * (332f / 641f));
            int statright = picStatistics.Right;
            picStatistics.Left = picTetrisField.Right + 6;
            picStatistics.Width = statright - picStatistics.Left;
        }

        private void aIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aIToolStripMenuItem.Checked = !aIToolStripMenuItem.Checked;
            if (_Present.ai == null)
            {
                _Present.ai = new TetrisAI(this);
            }
            else
            {
                _Present.ai.AbortAI();
                _Present.ai = null;
            }
        }
        public void Feedback(float Strength, int Length)
        {
            _Present.Feedback(Strength, Length);
        }
        private void BASeTris_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_Present.Game != null && _Present.Game.CurrentState is IDirectKeyboardInputState)
            {
                var Casted = (IDirectKeyboardInputState) _Present.Game.CurrentState;
                Casted.KeyPressed(this, (Keys) e.KeyChar);
            }
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void picTetrisField_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Right)
            {
                ContextMenuStrip cms = new ContextMenuStrip();
                for(int i=0;i<10;i++)
                {
                    ToolStripMenuItem tsm = new ToolStripMenuItem("Item " + i);
                    cms.Items.Add(tsm);
                }
                cms.Show();
            }
        }

        private void BASeTris_Enter(object sender, EventArgs e)
        {
            _Present.GameThreadPaused = false;
        }

        private void BASeTris_Leave(object sender, EventArgs e)
        {
            _Present.GameThreadPaused = true;
        }
    }
}