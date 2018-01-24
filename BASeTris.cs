using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris
{
    public partial class BASeTris : Form, IStateOwner
    {
        private TetrisGame _Game;
        
        public BASeTris()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartGame();
        }
        BlockGroup testBG = null;
        private void StartGame()
        {
            _Game = new TetrisGame(this);
            
          
            if(GameThread!=null) GameThread.Abort();
            GameThread = new Thread(GameProc);
            GameThread.Start();
        }

        private Thread GameThread = null;
        
        private ConcurrentQueue<Action> ProcThreadActions = new ConcurrentQueue<Action>();
        private void GameProc()
        {
            
            while (true)
            {

                if(ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }


               _Game.GameProc();

                Invoke((MethodInvoker)(() =>
                {
                    picTetrisField.Invalidate();
                    picTetrisField.Refresh();
                }));

                Thread.Sleep(5);
                
            }

        }
        static Random rgen = new Random();
       
      
        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            _Game.DrawProc(e.Graphics, new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height));
            
        }

        private void picTetrisField_Resize(object sender, EventArgs e)
        {
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }

        private void picTetrisField_Click(object sender, EventArgs e)
        {
            testBG.Rotate(false);
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }
       
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.X)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    _Game.HandleGameKey(this,GameState.GameKeys.GameKey_RotateCW);
                    
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
            GameThread.Abort();
        }

        public GameState CurrentState { get { return _Game.CurrentState; } set{ _Game.CurrentState = value; } }
        public void EnqueueAction(Action pAction)
        {
            ProcThreadActions.Enqueue(pAction);
        }
    }
}
