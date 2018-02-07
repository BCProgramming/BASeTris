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
using BASeTris.AI;
using BASeTris.AssetManager;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
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
        public void SetScale(double factor)
        {
            mnuScale_Tiny.Checked = mnuScale_Small.Checked = mnuScale_Large.Checked = mnuScale_Biggliest.Checked = false;
            Size = new Size((int)(DefaultWidth*factor),(int)(DefaultHeight*factor));
            
            
        }
        public void Feedback(float Strength,int Length)
        {
            X.Gamepad_1.FFB_Vibrate(Strength,Strength,Length);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            TetrisGame.InitState();
           
            
        }
        BlockGroup testBG = null;
        private void StartGame()
        {
            if (X.IsAvailable)
            {
                X.UpdatesPerSecond = 30;

                X.StartPolling(X.Gamepad_1);
                
            }
            _Game = new TetrisGame(this, new StandardTetrisGameState(Tetromino.BagTetrominoChooser(),new GarbageFieldInitializer(new Random(),new NESTetrominoTheme(),1)));
            
            


            if (GameThread!=null) GameThread.Abort();
            GameThread = new Thread(GameProc);
            GameThread.Start();
            if(InputThread!=null) InputThread.Abort();
            InputThread = new Thread(GamepadInputThread);
            InputThread.Start();
            ai = new TetrisAI(this);
        }
        private TetrisAI ai;
        private Thread GameThread = null;
        private Thread InputThread = null;
        private ConcurrentQueue<Action> ProcThreadActions = new ConcurrentQueue<Action>();
        HashSet<GameState.GameKeys> ActiveKeys = new HashSet<GameState.GameKeys>();
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
        private void GameProc()
        {
            
            while (true)
            {

                if(ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }

                if (_Game.CurrentState!=null && !_Game.CurrentState.GameProcSuspended)
                {
                    _Game.GameProc();
                }
                Invoke((MethodInvoker)(() =>
                {
                    picTetrisField.Invalidate();
                    picTetrisField.Refresh();
                    picStatistics.Invalidate();
                    picStatistics.Refresh();
                }));

                Thread.Sleep(5);
                
            }

        }
        static Random rgen = new Random();
       
      
        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            if (_Game == null) return;
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
            if (PressedKeys.Contains(e.KeyCode)) return;
            PressedKeys.Add(e.KeyCode);
            if(e.KeyCode==Keys.X)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this,GameState.GameKeys.GameKey_RotateCW);
                    
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
            else if(e.KeyCode==Keys.Pause)
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
            SetScale(1.5f);
            ((ToolStripMenuItem)sender).Checked = true;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(1.75f);
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
