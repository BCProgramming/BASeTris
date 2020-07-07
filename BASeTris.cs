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
using BASeTris.GameStates.Menu;
using BASeTris.BackgroundDrawers;
using BASeTris.Rendering.Adapters;

namespace BASeTris
{
    
    //IStateOwner in the form itself.
    //This is necessary so we can also have an implementation via the OpenTK GameWindow. 
    public partial class BASeTris : Form, IStateOwner,IGamePresenter
    {
        private GamePresenter _Present;
        //delegate the BeforeGameStateChange event...
        public GamePresenter GetPresenter() => _Present;
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

        private double current_factor = 1.3;

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

           
            _Present = new GamePresenter(this);
            
            //XMLHighScores<NoSpecialInfo> TestScores = new XMLHighScores<NoSpecialInfo>(35000,(r)=>new NoSpecialInfo());
            //int Position1 = TestScores.IsEligible(12000);
            //int Position2 = TestScores.IsEligible(3000);
            
            menuStrip1.Font = new Font(menuStrip1.Font.FontFamily, 14, FontStyle.Regular);
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
            StartGame();

            GenericMenuState TitleMenu = new GenericMenuState(StandardImageBackgroundGDI.GetStandardBackgroundDrawer(), this, new TitleMenuPopulator());

            CurrentState = TitleMenu;
            SetScale(1.33333d);

        }
        SKBitmap SkiaBitmap = null;
        
        
        bool XPolling = false;
        Nomino testBG = null;

        public void StartGame()
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
            RectangleF useBounds;
            var renderer = RenderingProvider.Static.GetHandler(typeof(Graphics), _Present.Game.CurrentState.GetType(), typeof(BaseDrawParameters));
            if(renderer!=null)
            {
                if(renderer is IStateRenderingHandler staterender)
                {
                    useBounds = new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height);
                    staterender.Render(this, e.Graphics, _Present.Game.CurrentState,
                        
                        new BaseDrawParameters(useBounds));


                    return;
                }
            }
            //if the above doesn't go through....
            useBounds = new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height);
            _Present.Game.DrawProc(e.Graphics, useBounds);
            _LastDrawBounds = useBounds;
        }
       
     


        private void picStatistics_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentState != null)
            {
                
                if (picStatistics.Visible == false) return;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;

                var renderer = RenderingProvider.Static.GetHandler(typeof(Graphics), _Present.Game.CurrentState.GetType(), typeof(BaseDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        staterender.RenderStats(this, e.Graphics, _Present.Game.CurrentState,
                            new BaseDrawParameters(picStatistics.ClientRectangle));
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
        private BCRect _LastDrawBounds;
        public BCRect LastDrawBounds => _LastDrawBounds;

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

        }
        //public RendererMode ActiveRenderMode = RendererMode.Renderer_GDIPlus;
        public RendererMode ActiveRenderMode = RendererMode.Renderer_GDIPlus;
        

        

    

     

      

      
        
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