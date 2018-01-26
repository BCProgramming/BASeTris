using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public abstract class GameState
    {
        public enum GameKeys
        {
            GameKey_RotateCW,
            GameKey_RotateCCW,
            GameKey_Drop,
            GameKey_Left,
            GameKey_Right,
            GameKey_Down,
            GameKey_Pause
        }
        public abstract void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void GameProc(IStateOwner pOwner);
        public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void HandleGameKey(IStateOwner pOwner, GameKeys g);
    }

    public class StandardTetrisGameState : GameState
    {


        public Queue<BlockGroup> NextBlocks = new Queue<BlockGroup>();
        public TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public Statistics GameStats = new Statistics();
        public bool GameOvered = false;
        public BlockGroup GetNext()
        {
            if (NextBlocks.Count == 0) return null;
            return NextBlocks.Peek();
        }
        public StandardTetrisGameState(int GarbageRows = 0)
        {
            PlayField = new TetrisField(GarbageRows);
            PlayField.BlockGroupSet += PlayField_BlockGroupSet;
            PlayField.LevelChanged += PlayField_LevelChanged;
        }

        private void PlayField_LevelChanged(object sender, LevelChangeEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void PlayField_BlockGroupSet(object sender, BlockGroupSetEventArgs e)
        {
            if (e._group.Y < 3)
            {
                GameOvered = true;
            }
        }

        public override void GameProc(IStateOwner pOwner)
        {
            PlayField.AnimateFrame();
            foreach (var iterate in PlayField.BlockGroups)
            {
                if ((DateTime.Now - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                {

                    if (MoveGroupDown(iterate))
                    {

                        int result = PlayField.ProcessLines();
                        if (result == 1) GameStats.Score += ((GameStats.LineCount / 10) + 1) * 10;
                        else if (result == 2) GameStats.Score += ((GameStats.LineCount / 10) + 2) * 15;
                        else if (result == 3) GameStats.Score += ((GameStats.LineCount / 10) + 3) * 20;
                        else if (result == 4) GameStats.Score += ((GameStats.LineCount / 10) + 5) * 50;


                    }
                    iterate.LastFall = DateTime.Now;
                }
            }
            if (GameOvered)
            {
                TetrisGame.Soundman.StopMusic();
                pOwner.CurrentState = new GameOverGameState(this);
            }
            if (PlayField.BlockGroups.Count == 0)
            {
                SpawnNewTetromino();
            }
        }
        static Random rgen = new Random();
        private const int BlockQueueLength = 5;
        private void RefillBlockQueue()
        {
            while (BlockQueueLength > NextBlocks.Count)
            {
                var Generated = GenerateTetromino();
                NextBlocks.Enqueue(Generated);
            }
        }
        private void SpawnNewTetromino()
        {
            if (NextBlocks.Count == 0)
            {
                RefillBlockQueue();
            }
            var nextget = NextBlocks.Dequeue();
            PlayField.Theme.ApplyTheme(nextget, PlayField);
            if (NextBlocks.Count < BlockQueueLength)
            {
                RefillBlockQueue();
            }
            nextget.X = (int)(((float)PlayField.ColCount / 2) - ((float)nextget.GroupExtents.Width / 2));
            nextget.Y = 0;

            if (nextget is Tetromino_I)
            {
                GameStats.I_Piece_Count++;
            }
            else if (nextget is Tetromino_J)
                GameStats.J_Piece_Count++;
            else if (nextget is Tetromino_L)
                GameStats.L_Piece_Count++;
            else if (nextget is Tetromino_O)
                GameStats.O_Piece_Count++;
            else if (nextget is Tetromino_S)
                GameStats.S_Piece_Count++;
            else if (nextget is Tetromino_T)
                GameStats.T_Piece_Count++;
            else if (nextget is Tetromino_Z)
                GameStats.Z_Piece_Count++;

            PlayField.AddBlockGroup(nextget);

        }
        public BlockGroup GenerateTetromino()
        {
            Func<BlockGroup> GetTetrominoFunction;
            Func<BlockGroup>[] GeneratorFunctions = new Func<BlockGroup>[]
            {
                () => new Tetromino_Z(),
                () => new Tetromino_I(),
                () => new Tetromino_J(),
                () => new Tetromino_L(),
                () => new Tetromino_O(),
                () => new Tetromino_S(),
                () => new Tetromino_T()

            };
            GetTetrominoFunction = TetrisGame.Choose(GeneratorFunctions);
            //GetTetrominoFunction = BlockGroup.GetTetromino_T;
            BlockGroup newTetromino = GetTetrominoFunction();
            return newTetromino;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            g.Clear(Color.Black);


            var useStats = GameStats;
            Font standardFont = new Font("Arial", 12, FontStyle.Bold);

            String BuildStatString = "Score:" + useStats.Score.ToString() + "\n" +
                                     "Lines:" + PlayField.LineCount + "\n" +
                                     "I Tet:" + useStats.I_Piece_Count + "\n" +
                                     "O Tet:" + useStats.O_Piece_Count + "\n" +
                                     "J Tet:" + useStats.J_Piece_Count + "\n" +
                                     "T Tet:" + useStats.T_Piece_Count + "\n" +
                                     "L Tet:" + useStats.L_Piece_Count + "\n" +
                                     "S Tet:" + useStats.S_Piece_Count + "\n" +
                                     "Z Tet:" + useStats.Z_Piece_Count + "\n";


            g.DrawString(BuildStatString, standardFont, Brushes.White, new Point(5, 5));


        }
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (PlayField != null)
            {
                PlayField.Draw(g, Bounds);
            }
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_RotateCW)
            {
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (PlayField.CanRotate(activeitem, false))
                    {
                        activeitem.Rotate(false);
                        TetrisGame.Soundman.PlaySound("block_rotate");
                        activeitem.Clamp(PlayField.RowCount, PlayField.ColCount);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Down)
            {
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (MoveGroupDown(activeitem))
                    {

                        PlayField.ProcessLines();

                    }
                }
            }
            else if (g == GameKeys.GameKey_Right || g == GameKeys.GameKey_Left)
            {
                int XMove = g == GameKeys.GameKey_Right ? 1 : -1;
                foreach (var ActiveItem in PlayField.BlockGroups)
                {
                    if (PlayField.CanFit(ActiveItem, ActiveItem.X + XMove, ActiveItem.Y))
                    {
                        lastHorizontalMove = DateTime.Now;
                        ActiveItem.X += XMove;
                        TetrisGame.Soundman.PlaySound("block_move");
                    }
                }
            }
            else if (g == GameKeys.GameKey_Pause)
            {
                //pOwner.CurrentState = new PauseGameState(this);
            }
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
                    GameStats.Score += 25 - activeItem.Y;
                    TetrisGame.Soundman.PlaySound("block_place");
                    return true;
                }
            }
            return false;
        } }

        public class GameOverGameState : GameState
        {
            private GameState GameOveredState = null;

            public int CoverBlocks = 0;
            private bool CompleteScroll = false;
            public GameOverGameState(GameState paused)
            {
                GameOveredState = paused;
            }
            public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
            {
                GameOveredState.DrawStats(pOwner, g, Bounds);
            }
            DateTime LastAdvance = DateTime.MinValue;
            public override void GameProc(IStateOwner pOwner)
            {
                if ((DateTime.Now - LastAdvance).TotalMilliseconds > 50)
                {
                    LastAdvance = DateTime.Now;
                    CoverBlocks++;
                    StandardTetrisGameState standardstate = GameOveredState as StandardTetrisGameState;
                    if (standardstate != null)
                    {
                        if (CoverBlocks >= standardstate.PlayField.RowCount)
                        {
                            CoverBlocks = standardstate.PlayField.RowCount;

                            CompleteScroll = true;
                        }
                    }
                }
                //gameproc doesn't pass through!
            }
            Brush useCoverBrush = null;
            public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
            {

                if (GameOveredState is StandardTetrisGameState)
                {
                    StandardTetrisGameState standardgame = GameOveredState as StandardTetrisGameState;
                    SizeF BlockSize = new SizeF(Bounds.Width / (float)standardgame.PlayField.ColCount, Bounds.Height / (float)standardgame.PlayField.RowCount);
                    useCoverBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)Bounds.Width, (int)BlockSize.Height), Color.DarkSlateGray, Color.MintCream, LinearGradientMode.Vertical);
                    GameOveredState.DrawProc(pOwner, g, Bounds);
                    g.FillRectangle(useCoverBrush, 0f, 0f, (float)Bounds.Width, (float)BlockSize.Height * CoverBlocks);
                }

                if (CompleteScroll)
                {
                    Font GameOverFont = new Font("Arial", 24);
                    String GameOverText = "GAME\nOVER";
                    var measured = g.MeasureString(GameOverText, GameOverFont);
                    g.DrawString(GameOverText, GameOverFont, Brushes.White, 5 + (Bounds.Width / 2) - measured.Width / 2, 5 + (Bounds.Height / 2) - measured.Height / 2);
                    g.DrawString(GameOverText, GameOverFont, Brushes.Black, (Bounds.Width / 2) - measured.Width / 2, (Bounds.Height / 2) - measured.Height / 2);
                }


            }

            public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
            {

            }

        }



    }

