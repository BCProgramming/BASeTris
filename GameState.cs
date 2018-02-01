using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BASeTris.AssetManager;
using BASeTris.TetrisBlocks;
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

        Bitmap useBackground = null;
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
            TetrominoImages = null;
            //throw new NotImplementedException();
        }
        private bool f_RedrawTetrominoImages = false;
        private bool f_RedrawStatusBitmap = false;
        private Dictionary<System.Type, Image> TetrominoImages = null;

        private void RedrawStatusbarTetrominoBitmaps(RectangleF Bounds)
        {
            TetrominoImages = new Dictionary<Type, Image>();
            float useSize = 18 * ((float)Bounds.Height / 644f);
            SizeF useTetSize = new SizeF(useSize,useSize);
            Tetromino_I TetI = new Tetromino_I();
            Tetromino_J TetJ = new Tetromino_J();
            Tetromino_L TetL = new Tetromino_L();
            Tetromino_O TetO = new Tetromino_O();
            Tetromino_S TetS = new Tetromino_S();
            Tetromino_T TetT = new Tetromino_T();
            Tetromino_Z TetZ = new Tetromino_Z();

            
            PlayField.Theme.ApplyTheme(TetI,PlayField);
            PlayField.Theme.ApplyTheme(TetJ, PlayField);
            PlayField.Theme.ApplyTheme(TetL, PlayField);
            PlayField.Theme.ApplyTheme(TetO, PlayField);
            PlayField.Theme.ApplyTheme(TetS, PlayField);
            PlayField.Theme.ApplyTheme(TetT, PlayField);
            PlayField.Theme.ApplyTheme(TetZ, PlayField);
            Image Image_I = TetI.GetImage(useTetSize);
            Image Image_J = TetJ.GetImage(useTetSize);
            Image Image_L = TetL.GetImage(useTetSize);
            Image Image_O = TetO.GetImage(useTetSize);
            Image Image_S = TetS.GetImage(useTetSize);
            Image Image_T = TetT.GetImage(useTetSize);
            Image Image_Z = TetZ.GetImage(useTetSize);

            TetrominoImages.Add(typeof(Tetromino_I),Image_I);
            TetrominoImages.Add(typeof(Tetromino_J), Image_J);
            TetrominoImages.Add(typeof(Tetromino_L), Image_L);
            TetrominoImages.Add(typeof(Tetromino_O), Image_O);
            TetrominoImages.Add(typeof(Tetromino_S), Image_S);
            TetrominoImages.Add(typeof(Tetromino_T), Image_T);
            TetrominoImages.Add(typeof(Tetromino_Z), Image_Z);

        }
        private void PlayField_BlockGroupSet(object sender, BlockGroupSetEventArgs e)
        {
            if (e._group.Y < 1)
            {
                GameOvered = true;
            }
        }
        public BlockGroup GetGhostDrop(BlockGroup Source, out int dropLength, int CancelProximity = 3)
        {
            //routine returns the Ghost Drop representor of this BlockGroup.
            //this function will also return null if the dropped block is CancelProximity or closer to the place it would be dropped.
            BlockGroup Duplicator = new BlockGroup(Source);

            dropLength = 0;
            while (true) { 
                if (PlayField.CanFit(Duplicator, Duplicator.X, Duplicator.Y+1))
                {
                    dropLength++;
                    Duplicator.Y++;
                }
                else
                {
                    break;
                }
        }
            if (dropLength < CancelProximity) return null;
            return Duplicator;
            


            
        }
        public override void GameProc(IStateOwner pOwner)
        {
            if(NextAngleOffset != 0)
            {
                double AngleChange = ((Math.PI * 2 / 360)) * 5;
                NextAngleOffset = Math.Sign(NextAngleOffset) * (Math.Abs(NextAngleOffset) - AngleChange);
                if (NextAngleOffset < AngleChange) NextAngleOffset = 0;
            }
            if (GameStartTime == DateTime.MinValue) GameStartTime = DateTime.Now;
            if(LastPausedTime!=DateTime.MinValue)
            {
                GameStartTime += (DateTime.Now - LastPausedTime);
                LastPausedTime = DateTime.MinValue;
            }
            //TODO: Animate line clears. Tetris should get some weird flashing thing or something too.
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
                FinalGameTime = DateTime.Now - GameStartTime;
                pOwner.CurrentState = new GameOverGameState(this);
            }
            if (PlayField.BlockGroups.Count == 0 && !SpawnWait)
            {
                SpawnWait = true;
                pOwner.EnqueueAction(()=>{
                Thread.Sleep(200);
                SpawnNewTetromino();
                SpawnWait = false;
                });
            }
        }
        static bool SpawnWait = false;
        static Random rgen = new Random();
        private DateTime GameStartTime = DateTime.MinValue;
        private DateTime LastPausedTime = DateTime.MinValue;
        private TimeSpan FinalGameTime = TimeSpan.MinValue;
        private const int BlockQueueLength = 5;
        private void RefillBlockQueue()
        {
            while (BlockQueueLength > NextBlocks.Count)
            {
                var Generated = GenerateTetromino();
                NextBlocks.Enqueue(Generated);
            }
        }
        protected virtual void SpawnNewTetromino()
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
            //FallSpeed is 1000 -50 for each level. Well, for now.
            nextget.FallSpeed = Math.Max(1000 - (PlayField.Level * 100), 50);
            NextAngleOffset += Math.PI * 2 / 5;

            PlayField.AddBlockGroup(nextget);

        }
        private ImageAttributes GetShadowAttributes(float ShadowBrightness = 0.1f)
        {
            float brt = ShadowBrightness;
            ImageAttributes resultAttr = new ImageAttributes();
            System.Drawing.Imaging.ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[]{brt,0f,0f,0f,0f},
                new float[]{0f,brt,0f,0f,0f},
                new float[]{0f,0f,brt,0f,0f},
                new float[]{0f,0f,0f,1f,0f},
                new float[]{0f,0f,0f,0f,1f}

            });
            resultAttr.SetColorMatrix(cm);
            return resultAttr;
        }


        public virtual BlockGroup GenerateTetromino()
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
        Image StatisticsBackground = null;
        public void GenerateStatisticsBackground()
        {
            Bitmap buildbg = new Bitmap(1120, 2576);
            Size BlockSize = new Size(128, 128);
            Color[] usePossibleColours = NESTetrominoTheme.AllThemeColors;
            int ColumnCount = (buildbg.Width / BlockSize.Width) + 1;
            int RowCount = (buildbg.Height / BlockSize.Height) + 1;
            using (Graphics g = Graphics.FromImage(buildbg))
            {
                g.Clear(Color.Black);
                for (int col = 0; col < ColumnCount; col++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        int DrawBlockX = col * BlockSize.Width;
                        int DrawBlockY = row * BlockSize.Height;
                        StandardColouredBlock GenerateColorBlock = new StandardColouredBlock();
                        Color MainColor = TetrisGame.Choose(usePossibleColours);
                        GenerateColorBlock.BlockColor = MainColor;
                        if (TetrisGame.rgen.NextDouble() > 0.75)
                        {
                            GenerateColorBlock.InnerColor = Color.MintCream;
                        }
                        else
                        {

                            GenerateColorBlock.InnerColor = GenerateColorBlock.BlockColor;
                        }
                        TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null);
                        GenerateColorBlock.DrawBlock(tbd);

                    }
                }
            }
            StatisticsBackground = buildbg;

        }

        RectangleF LastDrawStat = Rectangle.Empty;
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {

            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            LastDrawStat = Bounds;
            if(StatisticsBackground==null || RedrawsNeeded)
            {
                GenerateStatisticsBackground();
            }
            
            g.DrawImage(StatisticsBackground,Bounds);
            //g.Clear(Color.Black);
            if (TetrominoImages == null || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(Bounds);

            var useStats = GameStats;
            double Factor = Bounds.Height / 644d;
            int DesiredFontPixelHeight = (int)(Bounds.Height * (23d / 644d));

            Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold,GraphicsUnit.Pixel);

            String BuildStatString = "Time:" + FormatGameTime(pOwner) + "\n" +

                "Score: " + useStats.Score.ToString() + "\n" +
                                     "Lines: " + PlayField.LineCount + "\n";
                                    

            var measured = g.MeasureString(BuildStatString,standardFont);




            g.FillRectangle(LightenBrush, 0, 5, Bounds.Width, (int)(450*Factor));
            g.DrawString(BuildStatString, standardFont, Brushes.White, new Point((int)(7*Factor), (int)(7*Factor)));
            g.DrawString(BuildStatString, standardFont, Brushes.Black, new Point((int)(5*Factor), (int)(5*Factor)));

            Type[] useTypes = new Type[]{typeof(Tetromino_I),typeof(Tetromino_O),typeof(Tetromino_J),typeof(Tetromino_T),typeof(Tetromino_L),typeof(Tetromino_S),typeof(Tetromino_Z)};
            int[] PieceCounts = new int[] { useStats.I_Piece_Count, useStats.O_Piece_Count, useStats.J_Piece_Count, useStats.T_Piece_Count, useStats.L_Piece_Count, useStats.S_Piece_Count, useStats.Z_Piece_Count };

            int StartYPos = (int)(140*Factor);
            int useXPos = (int)(30*Factor);
            ImageAttributes ShadowTet = GetShadowAttributes();
            for(int i=0;i<useTypes.Length;i++)
            {
                PointF BaseCoordinate = new PointF(useXPos,StartYPos + (int)((float)i*(40d*Factor)));
                PointF TextPos = new PointF(useXPos+(int)(100d*Factor),BaseCoordinate.Y);
                String StatText = "" + PieceCounts[i];
                SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                Image TetrominoImage = TetrominoImages[useTypes[i]];
                PointF ImagePos = new PointF(BaseCoordinate.X,BaseCoordinate.Y+(StatTextSize.Height/2 - TetrominoImage.Height/2));
                int offset = 3;
                foreach(Point shadowblob in new Point[] { new Point(offset,offset),new Point(-offset,offset),new Point(offset,-offset), new Point(-offset,-offset)})
                {
                    g.DrawImage(TetrominoImage, new Rectangle((int)ImagePos.X + shadowblob.X, (int)ImagePos.Y + shadowblob.Y, TetrominoImage.Width, TetrominoImage.Height), 0f, 0f, (float)TetrominoImage.Width, (float)TetrominoImage.Height, GraphicsUnit.Pixel, ShadowTet);
                }

                
                g.DrawImage(TetrominoImage, ImagePos);
                g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X+4,TextPos.Y+4));
                g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
            }

            //now draw the "Next" Queue. For now we'll just show one "next" item.
            if (NextBlocks.Count > 0)
            {
                var QueueList = NextBlocks.ToArray();
                Image[] NextTetrominoes = (from t in QueueList select TetrominoImages[t.GetType()]).ToArray();
                Image DisplayBox = TetrisGame.Imageman["display_box"];
                //draw it at 40,420. (Scaled).
                Point NextDrawPosition = new Point((int)(40f * Factor), (int)(420 * Factor));
                Size NextSize = new Size((int)(200f * Factor), (int)(200f * Factor));
                g.DrawImage(DisplayBox, new Rectangle(NextDrawPosition, NextSize), 0, 0, DisplayBox.Width, DisplayBox.Height, GraphicsUnit.Pixel);
                for (int i = NextTetrominoes.Length-1; i > -1 ; i--)
                {

                    Point CenterPoint = new Point(NextDrawPosition.X+NextSize.Width/2,NextDrawPosition.Y+NextSize.Height/2);

                    double StartAngle = Math.PI;
                    double AngleIncrementSize = (Math.PI * 1.75) / (double)NextTetrominoes.Length;
                    //we draw starting at StartAngle, in increments of AngleIncrementSize.
                    //i is the index- we want to increase the angle by that amount (well, obviously, I suppose...

                    double UseAngleCurrent = StartAngle + AngleIncrementSize * (float)i + NextAngleOffset;

                    Double UseXPosition = CenterPoint.X + ((float)(NextSize.Width) / 3 * Math.Cos(UseAngleCurrent));
                    double UseYPosition = CenterPoint.Y + ((float)(NextSize.Height) / 3 * Math.Sin(UseAngleCurrent));
                    


                    var NextTetromino = NextTetrominoes[i];
                    
                    float Deviation = (i - NextTetrominoes.Length / 2);
                    Point Deviate = new Point((int)(Deviation * 20*Factor), (int)(Deviation * 20*Factor));
                    
                    Point DrawTetLocation = new Point((int)UseXPosition,(int)UseYPosition);
                    //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                    //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                    Size DrawTetSize = new Size(
                        (int)((float)NextTetromino.Width * (1 - ((float)i * 0.1f))),
                        (int)((float)NextTetromino.Height * (1 - ((float)i * 0.1f))));
                    ImageAttributes Shade = GetShadowAttributes(1.0f - ((float)i * 0.3f));
                    int offset = 3;
                    foreach (Point shadowblob in new Point[] { new Point(offset, offset), new Point(-offset, offset), new Point(offset, -offset), new Point(-offset, -offset) })
                    {
                        g.DrawImage(NextTetromino, new Rectangle((int)DrawTetLocation.X + shadowblob.X, (int)DrawTetLocation.Y+shadowblob.Y, DrawTetSize.Width, DrawTetSize.Height), 0f, 0f,
                            (float)NextTetromino.Width, (float)NextTetromino.Height, GraphicsUnit.Pixel, ShadowTet);
                    }


                        g.DrawImage(NextTetromino, new Rectangle((int)DrawTetLocation.X, (int)DrawTetLocation.Y, DrawTetSize.Width, DrawTetSize.Height), 0f, 0f,
                        (float)NextTetromino.Width, (float)NextTetromino.Height, GraphicsUnit.Pixel, Shade);

                    
                }
            }
            //"I Tet: " + useStats.I_Piece_Count + "\n" +
                //"O Tet: " + useStats.O_Piece_Count + "\n" +
                //"J Tet: " + useStats.J_Piece_Count + "\n" +
                //"T Tet: " + useStats.T_Piece_Count + "\n" +
                //"L Tet: " + useStats.L_Piece_Count + "\n" +
                //"S Tet: " + useStats.S_Piece_Count + "\n" +
                //"Z Tet: " + useStats.Z_Piece_Count + "\n";




        }
        double NextAngleOffset = 0; //use this to animate the "Next" ring... Set it to a specific value and GameProc should reduce it to zero over time.
        Brush LightenBrush = new SolidBrush(Color.FromArgb(128, Color.MintCream));
        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = (DateTime.Now - GameStartTime);

            if(FinalGameTime !=TimeSpan.MinValue)
            {
                useCalc = FinalGameTime;
            }
            if(stateowner.CurrentState is PauseGameState)
            {
                useCalc = LastPausedTime - GameStartTime;
            }

            return useCalc.ToString(@"hh\:mm\:ss");
        }
        private RectangleF StoredBackground = RectangleF.Empty;
        private void RefreshBackground(RectangleF buildSize)
        {
            StoredBackground = buildSize;
            useBackground = new Bitmap((int)buildSize.Width, (int)buildSize.Height);
            using (Graphics bgg = Graphics.FromImage(useBackground))
            {
                Image drawbg = TetrisGame.Imageman["background"];
                bgg.CompositingQuality = CompositingQuality.AssumeLinear;
                bgg.DrawImage(drawbg, 0, 0, buildSize.Width, buildSize.Height);
            }
        }


        private Brush GhostBrush = new SolidBrush(Color.FromArgb(75, Color.DarkBlue));
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {

            if(useBackground==null || !StoredBackground.Equals(Bounds))
            {
                RefreshBackground(Bounds);
            }
            
            g.DrawImage(useBackground,Bounds);
            

            if (PlayField != null)
            {
                PlayField.Draw(g, Bounds);
            }

            foreach (var activeblock in PlayField.BlockGroups)
            {
                int dl = 0;
                var GrabGhost = GetGhostDrop(activeblock, out dl, 3);
                if (GrabGhost != null)
                {
                    var BlockWidth = PlayField.GetBlockWidth(Bounds);
                    var BlockHeight = PlayField.GetBlockHeight(Bounds);

                    foreach (var iterateblock in activeblock)
                    {
                        RectangleF BlockBounds = new RectangleF(BlockWidth * (GrabGhost.X + iterateblock.X), BlockHeight * (GrabGhost.Y + iterateblock.Y-2), PlayField.GetBlockWidth(Bounds), PlayField.GetBlockHeight(Bounds));
                        TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, BlockBounds, GrabGhost);
                        tbd.OverrideBrush = GhostBrush;
                        iterateblock.Block.DrawBlock(tbd);
                    }
                }
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
            else if(g==GameKeys.GameKey_Drop)
            {
                //drop all active groups.
                foreach(var activeitem in PlayField.BlockGroups)
                {
                    int dropqty = 0;
                    var ghosted = GetGhostDrop(activeitem, out dropqty, 0);
                    PlayField.SetGroupToField(ghosted);
                    PlayField.RemoveBlockGroup(activeitem);
                    GameStats.Score += (dropqty * (5 + (GameStats.LineCount / 10)));
                }
                TetrisGame.Soundman.PlaySound("block_place");
                PlayField.ProcessLines();
                
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
                if (g == GameKeys.GameKey_Pause)
                {
                    LastPausedTime = DateTime.Now;
                    pOwner.CurrentState = new PauseGameState(this);

                    var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                    playing.Pause();
                    TetrisGame.Soundman.PlaySound("pause");


                }
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
        private DateTime CompleteScrollTime = DateTime.MaxValue; 
            private DateTime InitTime;
            public GameOverGameState(GameState paused)
            {
                GameOveredState = paused;
                InitTime = DateTime.Now;
            TetrisGame.Soundman.PlaySound("mmdeath");
            }
            public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
            {
                GameOveredState.DrawStats(pOwner, g, Bounds);
            }
            DateTime LastAdvance = DateTime.MinValue;
            public override void GameProc(IStateOwner pOwner)
            {
                if((DateTime.Now-CompleteScrollTime).TotalMilliseconds>500)
                {
                CompleteScrollTime = DateTime.MaxValue;
                TetrisGame.Soundman.PlaySound("tetris_game_over");
                }
            if (((DateTime.Now - InitTime)).TotalMilliseconds < 1500) return;
                if ((DateTime.Now - LastAdvance).TotalMilliseconds > 50 && !CompleteScroll)
                {
                    LastAdvance = DateTime.Now;
                    TetrisGame.Soundman.PlaySound("shade_move");
                CoverBlocks++;
                    StandardTetrisGameState standardstate = GameOveredState as StandardTetrisGameState;
                    if (standardstate != null)
                    {
                        if (CoverBlocks >= standardstate.PlayField.RowCount)
                        {
                            CoverBlocks = standardstate.PlayField.RowCount;
                        CompleteScrollTime = DateTime.Now;
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
                    Font GameOverFont = new Font(TetrisGame.RetroFont, 24);
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

    public class PauseGameState:GameState
    {
        private GameState PausedState = null;
        public PauseGameState(GameState pPausedState)
        {
            PausedState = pPausedState;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            PausedState.DrawStats(pOwner,g,Bounds);
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //no op!
        }

        Font usePauseFont = new Font(TetrisGame.RetroFont, 24);
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray,Bounds);
            PointF DrawPos = new PointF(Bounds.Width/2-Measured.Width/2,Bounds.Height/2-Measured.Height/2);
            g.DrawString(sPauseText,usePauseFont,Brushes.White,DrawPos);
            
            


        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if(g==GameKeys.GameKey_Pause)
            {
                pOwner.CurrentState = PausedState;

                var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                playing.UnPause();
                TetrisGame.Soundman.PlaySound("pause");

            }
        }
    }

    }

