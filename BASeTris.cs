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
    public partial class BASeTris : Form
    {
        private TetrisField PlayField = null;
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
            PlayField = new TetrisField(1);
           
            if(GameThread!=null) GameThread.Abort();
            GameThread = new Thread(GameProc);
            GameThread.Start();
        }

        private Thread GameThread = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        private ConcurrentQueue<Action> ProcThreadActions = new ConcurrentQueue<Action>();
        private void GameProc()
        {
            
            while (true)
            {

                if(ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }


                PlayField.AnimateFrame();
                foreach(var iterate in PlayField.BlockGroups)
                {
                    if ((DateTime.Now - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                    {
                        
                            if(MoveGroupDown(iterate))
                            {
                                ProcThreadActions.Enqueue(() =>
                                {
                                    PlayField.ProcessLines();
                                });
                            }
                            iterate.LastFall = DateTime.Now;
                    }
                }

                if(PlayField.BlockGroups.Count==0)
                {
                    SpawnNewTetromino();
                }

                Invoke((MethodInvoker)(() =>
                {
                    picTetrisField.Invalidate();
                    picTetrisField.Refresh();
                }));

                Thread.Sleep(5);
                
            }

        }
        static Random rgen = new Random();
        public static T Choose<T>(IEnumerable<T> ChooseArray)
        {
            if (rgen == null) rgen = new Random();
            SortedList<double, T> sorttest = new SortedList<double, T>();
            foreach (T loopvalue in ChooseArray)
            {
                double rgg;
                do
                {
                    rgg = rgen.NextDouble();
                }
                while (sorttest.ContainsKey(rgg));
                sorttest.Add(rgg, loopvalue);

            }

            //return the first item.
            return sorttest.First().Value;
        }
        private void SpawnNewTetromino()
        {
            Func<BlockGroup> GetTetrominoFunction;
            Func<BlockGroup>[] GeneratorFunctions = new Func<BlockGroup>[]
            {
                BlockGroup.GetTetromino_Z,
                BlockGroup.GetTetromino_I,
                BlockGroup.GetTetromino_J,
                BlockGroup.GetTetromino_L,
                BlockGroup.GetTetromino_O,
                BlockGroup.GetTetromino_S,
                BlockGroup.GetTetromino_T
            };
            GetTetrominoFunction = Choose(GeneratorFunctions);
            //GetTetrominoFunction = BlockGroup.GetTetromino_T;
            BlockGroup newTetromino = GetTetrominoFunction();
            newTetromino.X = 5-newTetromino.GroupExtents.Width/2;
            newTetromino.Y = 0;
            PlayField.AddBlockGroup(newTetromino);
            
        }
        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            if(PlayField!=null)
            {
                PlayField.Draw(e.Graphics,new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height));
            }
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
        private bool MoveGroupDown(BlockGroup activeItem)
        {
            
                if (PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1))
                {
                    activeItem.Y++;
                }
                else
                {
                    if ((DateTime.Now - lastHorizontalMove).TotalMilliseconds > 250)
                    {
                        PlayField.SetGroupToField(activeItem);
                        return true;
                    }
                }
            return false;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.X)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    foreach (var activeitem in PlayField.BlockGroups)
                    {
                        if (PlayField.CanRotate(activeitem, false))
                        {
                            activeitem.Rotate(false);
                            activeitem.Clamp(PlayField.RowCount, PlayField.ColCount);
                        }
                    }
                });
            }
            else if(e.KeyCode==Keys.Down)
            {
                ProcThreadActions.Enqueue(() =>
                {
                    foreach (var activeitem in PlayField.BlockGroups)
                    {
                        if (MoveGroupDown(activeitem))
                        {
                            ProcThreadActions.Enqueue(() =>
                            {
                                PlayField.ProcessLines();
                            });
                        }
                    }
                });
            }
            else if(e.KeyCode ==Keys.Right|| e.KeyCode == Keys.Left)
            {
                
                int XMove = e.KeyCode == Keys.Right ? 1 : -1;
                ProcThreadActions.Enqueue(() =>
                {
                    foreach (var ActiveItem in PlayField.BlockGroups)
                    {
                        if (PlayField.CanFit(ActiveItem, ActiveItem.X + XMove, ActiveItem.Y))
                        {
                            lastHorizontalMove = DateTime.Now;
                            ActiveItem.X += XMove;
                        }
                    }
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
    }
}
