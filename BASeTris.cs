using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using XInput.Wrapper;

namespace BASeTris
{
    public partial class BASeTris : Form, IStateOwner
    {
        private List<Particle> ActiveParticles = new List<Particle>();
        private List<GameObject> GameObjects = new List<GameObject>();
        private TetrisGame _Game;
        HashSet<Keys> PressedKeys = new HashSet<Keys>();
        public BASeTris()
        {
            InitializeComponent();
        }
        public void AddGameObject(GameObject go)
        {
            GameObjects.Add(go);
        }
        public void AddParticle(Particle p)
        {
            ActiveParticles.Add(p);
        }

        public Rectangle GameArea
        {
            get
            {
                return picTetrisField.ClientRectangle;
            }
        }

        double DefaultWidth = 643d;
        double DefaultHeight = 734d;
        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            if (picFullSize.Visible == (pMode == GameState.DisplayMode.Full)) return; //if full size visibility matches the passed state being full, we are already in that state.
            picFullSize.Visible = pMode==GameState.DisplayMode.Full;
            picTetrisField.Visible = pMode == GameState.DisplayMode.Partitioned;
            picStatistics.Visible = pMode == GameState.DisplayMode.Partitioned;


        }
        public void SetScale(double factor)
        {
            mnuScale_Tiny.Checked = mnuScale_Small.Checked = mnuScale_Large.Checked = mnuScale_Biggliest.Checked = false;
            Size = new Size((int)(DefaultWidth*factor),(int)(DefaultHeight*factor));
            
            
        }
        public void Feedback(float Strength,int Length)
        {
            if (IgnoreController) return;
            X.Gamepad_1.FFB_Vibrate(Strength,Strength,Length);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //XMLHighScores<NoSpecialInfo> TestScores = new XMLHighScores<NoSpecialInfo>(35000,(r)=>new NoSpecialInfo());
            //int Position1 = TestScores.IsEligible(12000);
            //int Position2 = TestScores.IsEligible(3000);

            menuStrip1.Font = new Font(menuStrip1.Font.FontFamily, 14, FontStyle.Regular);
            Win10MenuRenderer buildrender = new Win10MenuRenderer(null,true);
            
            menuStrip1.Renderer = buildrender;
            menuStrip1.BackColor = SystemColors.Control;
            TetrisGame.InitState();
           
            
        }
        bool XPolling = false;
        BlockGroup testBG = null;
        private void StartGame()
        {
            if (X.IsAvailable && !XPolling)
            {
                XPolling = true;
                X.UpdatesPerSecond = 30;
                X.StartPolling(X.Gamepad_1);
                
            }
            _Game = new TetrisGame(this, new StandardTetrisGameState(Tetromino.BagTetrominoChooser(), new GarbageFieldInitializer(new Random(),new NESTetrominoTheme(),1)));
            
            

            TetrisGame.AudioThemeMan.ResetTheme();
            if (GameThread!=null) GameThread.Abort();
            GameThread = new Thread(GameProc);
            GameThread.Start();
            if(InputThread!=null) InputThread.Abort();
            InputThread = new Thread(GamepadInputThread);
            InputThread.Start();
        }
        private TetrisAI ai;
        private Thread GameThread = null;
        private Thread InputThread = null;
        private ConcurrentQueue<Action> ProcThreadActions = new ConcurrentQueue<Action>();
        HashSet<GameState.GameKeys> ActiveKeys = new HashSet<GameState.GameKeys>();
        private bool IgnoreController = true;
        private void CheckInputs()
        {

            
            
                HandleKey(GameState.GameKeys.GameKey_RotateCW,X.Gamepad_1.A_down,X.Gamepad_1.A_up);
                HandleKey(GameState.GameKeys.GameKey_RotateCCW, X.Gamepad_1.X_down, X.Gamepad_1.X_up);
                HandleKey(GameState.GameKeys.GameKey_Left, X.Gamepad_1.Dpad_Left_down, X.Gamepad_1.Dpad_Left_up);
                HandleKey(GameState.GameKeys.GameKey_Right, X.Gamepad_1.Dpad_Right_down, X.Gamepad_1.Dpad_Right_up);
                HandleKey(GameState.GameKeys.GameKey_Down, X.Gamepad_1.Dpad_Down_down, X.Gamepad_1.Dpad_Down_up);
                HandleKey(GameState.GameKeys.GameKey_Drop, X.Gamepad_1.Dpad_Up_down, X.Gamepad_1.Dpad_Up_up);
                HandleKey(GameState.GameKeys.GameKey_Pause, X.Gamepad_1.Start_down, X.Gamepad_1.Start_up );
                HandleKey(GameState.GameKeys.GameKey_Hold, X.Gamepad_1.RBumper_down, X.Gamepad_1.RBumper_up);
            


        }
        private void HandleKey(GameState.GameKeys key,bool DownState,bool UpState)
        {
            
            if(ActiveKeys.Contains(key) && !DownState)
            {
                ActiveKeys.Remove(key);
                return;
            }
            if (ActiveKeys.Contains(key)) return;
            if (!DownState) return;
            ActiveKeys.Add(key);
            IgnoreController = false;
            _Game.HandleGameKey(this,key);
            
        }
        private void GamepadInputThread()
        {
            while(true)
            {
                Thread.Sleep(3);
                CheckInputs();
            }



        }
        private GameState LastFrameState = null;
        private void GameProc()
        {
            
            while (true)
            {
                if(!IsHandleCreated || IsDisposed)
                {
                    
                    if(!this.Focused)
                    {
                        Thread.Sleep(250);
                        continue;
                    }
                }
                if(ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }
                if(LastFrameState!=_Game.CurrentState)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        SetDisplayMode(_Game.CurrentState.SupportedDisplayMode);
                    }));
                }
                if (_Game.CurrentState!=null && !_Game.CurrentState.GameProcSuspended)
                {
                    
                    _Game.GameProc();
                }
                Invoke((MethodInvoker)(() =>
                {
                    if (_Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
                    {
                        picTetrisField.Invalidate();
                        picTetrisField.Refresh();
                        picStatistics.Invalidate();
                        picStatistics.Refresh();
                    }
                    else if(_Game.CurrentState.SupportedDisplayMode ==GameState.DisplayMode.Full)
                    {
                        picFullSize.Invalidate();
                        picFullSize.Refresh();
                    }
                }));

                Thread.Sleep(5);
                
            }

        }
        static Random rgen = new Random();

        private void picFullSize_Paint(object sender, PaintEventArgs e)
        {
            if (_Game == null) return;
            if (CurrentState.SupportedDisplayMode==GameState.DisplayMode.Full)
            {
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                _Game.DrawProc(e.Graphics, new RectangleF(picFullSize.ClientRectangle.Left, picFullSize.ClientRectangle.Top, picFullSize.ClientRectangle.Width, picFullSize.ClientRectangle.Height));
            }
        }
        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            if (_Game == null) return;
            if (picTetrisField.Visible == false) return;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            _Game.DrawProc(e.Graphics, new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height));

            

            
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
       
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            IgnoreController = true;
            if(e.KeyCode==Keys.G)
            {
                if(_Game.CurrentState is StandardTetrisGameState)
                {
                    StandardTetrisGameState gs = _Game.CurrentState as StandardTetrisGameState;
                    TetrisBlock[][] inserts = new TetrisBlock[4][];
                    for(int i=0;i<inserts.Length;i++)
                    {
                        inserts[i] = new TetrisBlock[gs.PlayField.ColCount];
                        for(int c=1;c<inserts[i].Length;c++)
                        {
                            inserts[i][c] = new StandardColouredBlock() { BlockColor = Color.Red, DisplayStyle = StandardColouredBlock.BlockStyle.Style_CloudBevel };
                        }

                    }
                    InsertBlockRowsActionGameState irs = new InsertBlockRowsActionGameState(gs,0,inserts,Enumerable.Empty<Action>());
                    CurrentState = irs;
                }
                
            }

            if (PressedKeys.Contains(e.KeyCode)) return;
            PressedKeys.Add(e.KeyCode);
            if(e.KeyCode==Keys.X)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this,GameState.GameKeys.GameKey_RotateCW);
                    
                });
            }
            else if(e.KeyCode==Keys.Z)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_RotateCCW);

                });
            }
            else if (e.KeyCode==Keys.Up)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Drop);
                });
            }
            else if(e.KeyCode==Keys.Down)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this,GameState.GameKeys.GameKey_Down);
                });
            }
            else if(e.KeyCode ==Keys.Right)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this,GameState.GameKeys.GameKey_Right);
                });
            }
            else if(e.KeyCode == Keys.Left)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Left);
                });
            }
            else if(e.KeyCode==Keys.Pause || e.KeyCode == Keys.P)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Pause);
                });
            }
            else if(e.KeyCode==Keys.Space)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Hold);
                });
            }
            else if(e.KeyCode==Keys.F2)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Debug1);
                });
            }
            else if(e.KeyCode==Keys.F7)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this, GameState.GameKeys.GameKey_Debug2);
                });
            }
            ProcThreadActions.Enqueue(() =>
            {
                Invoke((MethodInvoker)(() =>
            {
                picTetrisField.Refresh();
                picTetrisField.Invalidate();
            }));
                    });
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(GameThread!=null)
                GameThread.Abort();
            if(InputThread!=null)
            {
                InputThread.Abort();
            }
            if(X.IsAvailable)
            {
                X.StopPolling();
            }
            if(ai!=null) ai.AbortAI();
            TetrisGame.Soundman.StopMusic();
            Application.Exit();
        }

        public GameState CurrentState { get { return _Game?.CurrentState; } set{ _Game.CurrentState = value; } }
        public void EnqueueAction(Action pAction)
        {
            ProcThreadActions.Enqueue(pAction);
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartGame();
        }
        
        private void picStatistics_Paint(object sender, PaintEventArgs e)
        {
            if(CurrentState!=null)
            {
                if (picStatistics.Visible == false) return;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                CurrentState.DrawStats(this,e.Graphics,picStatistics.ClientRectangle);
            }
        }

        private void BASeTris_KeyUp(object sender, KeyEventArgs e)
        {
            if (PressedKeys.Contains(e.KeyCode))
            {
                PressedKeys.Remove(e.KeyCode);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(0.5f);
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetScale(1);
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetScale(1.3f);
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(1.6f);
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void BASeTris_ResizeEnd(object sender, EventArgs e)
        {
           
        }

        private void BASeTris_SizeChanged(object sender, EventArgs e)
        {
            picTetrisField.Width = (int)(picTetrisField.Height * (332f / 641f));
            int statright = picStatistics.Right;
            picStatistics.Left = picTetrisField.Right + 6;
            picStatistics.Width = statright - picStatistics.Left;
        }

        private void aIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aIToolStripMenuItem.Checked = !aIToolStripMenuItem.Checked;
            if(ai==null)
            {
                ai = new TetrisAI(this);
            }
            else
            {
                ai.AbortAI();
                ai = null;
            }
        }

        
    }
}
