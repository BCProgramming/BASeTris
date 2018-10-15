using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates.Menu;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using Microsoft.SqlServer.Server;

namespace BASeTris.GameStates
{
    public class StandardTetrisGameState : GameState
    {
        Bitmap useBackground = null;
        public Queue<BlockGroup> NextBlocks = new Queue<BlockGroup>();
        public BlockGroup HoldBlock = null;
        private List<Particle> Particles = new List<Particle>();
        public TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public Choosers.BlockGroupChooser Chooser = null;

        //given a value, translates from an unscaled horizontal coordinate in the default width to the appropriate size of the playing field based on the presented bounds.
        public double GetScaledHorizontal(RectangleF Bounds,double Value)
        {
            return Value * (Bounds.Width / BASeTris.DefaultWidth);
        }
        public double GetScaledVertical(RectangleF Bounds,double Value)
        {
            return Value * (Bounds.Height / BASeTris.DefaultHeight);
        }

        public double GetUnScaledHorizontal(RectangleF Bounds,double ScaledValue)
        {
            return ScaledValue / (Bounds.Width / BASeTris.DefaultWidth);
        }
        public double GetUnscaledVertical(RectangleF Bounds,double ScaledValue)
        {
            return ScaledValue / (Bounds.Height / BASeTris.DefaultHeight);
        }
        public PointF GetScaledPoint(RectangleF Bounds,PointF Value)
        {
            return new PointF((float)(GetUnScaledHorizontal(Bounds,Value.X)),(float)(GetUnscaledVertical(Bounds,Value.Y)));
        }

        public PointF GetUnscaledPoint(RectangleF Bounds,PointF Value)
        {
            return new PointF((float)(GetScaledHorizontal(Bounds, Value.X)), (float)(GetScaledVertical(Bounds, Value.Y)));
        }
        public Statistics GameStats
        {
            get { return PlayField?.GameStats; }
        }
        

        public virtual IHighScoreList<TetrisHighScoreData> GetLocalScores()
        {
            return TetrisGame.ScoreMan["Standard"];
        }

        public virtual int ProcessFieldChange(IStateOwner pOwner, BlockGroup Trigger)
        {
            //process lines for standard game.
            GameProcSuspended = true;
            try
            {
                int rowsfound = 0;
                List<int> CompletedRows = new List<int>();
                List<Action> AfterClearActions = new List<Action>();
                //checks the field contents for lines. If there are lines found, they are removed, and all rows above it are shifted down.
                for (int r = 0; r < PlayField.RowCount; r++)
                {
                    if (PlayField.Contents[r].All((d) => d != null))
                    {
                        Debug.Print("Found completed row at row " + r);
                        CompletedRows.Add(r);
                        rowsfound++;
                        //enqueue an action to perform the clear. We'll be replacing the current state with a clear action state, so this should execute AFTER that state returns control.
                        var r1 = r;
                        AfterClearActions.Add
                        (() =>
                        {
                            for (int g = r1; g > 0; g--)
                            {
                                Debug.Print("Moving row " + (g - 1).ToString() + " to row " + g);

                                for (int i = 0; i < PlayField.ColCount; i++)
                                {
                                    PlayField.Contents[g][i] = PlayField.Contents[g - 1][i];
                                }
                            }
                        });
                    }
                }

                long PreviousLineCount = PlayField.LineCount;
                if (Trigger != null)
                {
                    PlayField.GameStats.AddLineCount(Trigger.GetType(), rowsfound);
                }

                if ((PreviousLineCount % 10) > (PlayField.LineCount % 10))
                {
                    PlayField_LevelChanged(this, new TetrisField.LevelChangeEventArgs((int) PlayField.LineCount / 10));
                    PlayField.GameStats.SetLevelTime(GetElapsedTime(pOwner));

                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.LevelUp);
                    PlayField.SetFieldColors();
                }

                if (rowsfound > 0 && rowsfound < 4)
                {
                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.ClearLine, 2.0f);
                }
                else if (rowsfound == 4)
                {
                    TetrisGame.Soundman.PlaySoundRnd(TetrisGame.AudioThemeMan.ClearTetris, 2.0f);
                }


                int topmost = PlayField.RowCount;
                //find the topmost row with any blocks.
                for (int i = 0; i < PlayField.RowCount; i++)
                {
                    if (PlayField.Contents[i].Any((w) => w != null))
                    {
                        topmost = i;
                        break;
                    }
                }

                topmost = topmost + rowsfound; //subtract the rows that were cleared to get an accurate measurement.
                if (topmost < 9)
                {
                    if (currenttempo == 1)
                    {
                        currenttempo = 68;
                        if (GameOptions.MusicRestartsOnTempoChange)
                        {
                            if (GameOptions.MusicEnabled) TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, 0.75f, true);
                        }

                        var grabbed = TetrisGame.Soundman.GetPlayingMusic_Active();
                        if (grabbed != null)
                        {
                            TetrisGame.Soundman.GetPlayingMusic_Active().Tempo = 75f;
                        }
                    }
                }
                else
                {
                    if (currenttempo != 1)
                    {
                        currenttempo = 1;
                        if (GameOptions.MusicRestartsOnTempoChange)
                            if (GameOptions.MusicEnabled)
                                TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, 1f, true);
                        var grabbed = TetrisGame.Soundman.GetPlayingMusic_Active();
                        if (grabbed != null) grabbed.Tempo = 1f;
                    }
                }

                PlayField.HasChanged = rowsfound > 0;

                if (rowsfound > 0)
                {
                    var ClearState = new FieldLineActionGameState(this, CompletedRows.ToArray(), AfterClearActions);
                    ClearState.ClearStyle = TetrisGame.Choose((FieldLineActionGameState.LineClearStyle[]) (Enum.GetValues(typeof(FieldLineActionGameState.LineClearStyle))));

                    pOwner.CurrentState = ClearState;
                }

                //if(rowsfound > 0) pOwner.CurrentState = new FieldLineActionDissolve(this,CompletedRows.ToArray(),AfterClearActions);
                return rowsfound;
            }
            finally
            {
                GameProcSuspended = false;
            }
        }

        private int currenttempo = 1;


        public bool GameOvered = false;

        public BlockGroup GetNext()
        {
            if (NextBlocks.Count == 0) return null;
            return NextBlocks.Peek();
        }

        public StandardTetrisGameState(BlockGroupChooser pChooser, FieldInitializer pFieldInitializer)
        {
            this.Chooser = pChooser;
            PlayField = new TetrisField();
            if (pFieldInitializer != null) pFieldInitializer.Initialize(PlayField);
            PlayField.BlockGroupSet += PlayField_BlockGroupSet;
        }

        ~StandardTetrisGameState()
        {
            if (Chooser != null) Chooser.Dispose();
        }

        private void PlayField_LevelChanged(object sender, TetrisField.LevelChangeEventArgs e)
        {
            lock (LockTetImageRedraw)
            {
                TetrominoImages = null;
            }

            //throw new NotImplementedException();
        }

        private bool f_RedrawTetrominoImages = false;
        private bool f_RedrawStatusBitmap = false;
        private Dictionary<System.Type, Image> TetrominoImages = null;

        public Image GetTetronimoImage(System.Type TetrominoType)
        {
            return TetrominoImages[TetrominoType];
        }

        public Image[] GetTetronimoImages() => TetrominoImages.Values.ToArray();

        private void RedrawStatusbarTetrominoBitmaps(IStateOwner Owner,RectangleF Bounds)
        {
            lock (LockTetImageRedraw)
            {
                TetrominoImages = TetrisGame.GetTetrominoBitmaps(Bounds, PlayField.Theme, PlayField,(float)Owner.ScaleFactor);
            }
        }

        private void PlayField_BlockGroupSet(object sender, TetrisField.BlockGroupSetEventArgs e)
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
            while (true)
            {
                if (PlayField.CanFit(Duplicator, Duplicator.X, Duplicator.Y + 1))
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

        public void FrameUpdate()
        {
            if (NextAngleOffset != 0)
            {
                double AngleChange = ((Math.PI * 2 / 360)) * 5;
                NextAngleOffset = Math.Sign(NextAngleOffset) * (Math.Abs(NextAngleOffset) - AngleChange);
                if (NextAngleOffset < AngleChange) NextAngleOffset = 0;
            }
        }

        private bool FirstRun = false;

        public override void GameProc(IStateOwner pOwner)
        {
            if (!FirstRun)
            {
                if (GameOptions.MusicEnabled)
                {
                    var musicplay = TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, 0.5f, true);
                    musicplay.Tempo = 1f;
                    FirstRun = true;
                }
            }

            FrameUpdate();
            if (GameStartTime == DateTime.MinValue) GameStartTime = DateTime.Now;
            if (LastPausedTime != DateTime.MinValue)
            {
                GameStartTime += (DateTime.Now - LastPausedTime);
                LastPausedTime = DateTime.MinValue;
            }
            List<Particle> RemoveParticles = new List<Particle>();
            foreach(var iterate in Particles)
            {
                if(iterate.PerformFrame(pOwner))
                {
                    RemoveParticles.Add(iterate);
                }
            }
            foreach(var removeit in RemoveParticles)
            {
                Particles.Remove(removeit);
            }
            //TODO: Animate line clears. Tetris should get some weird flashing thing or something too.
            PlayField.AnimateFrame();
            foreach (var iterate in PlayField.BlockGroups)
            {
                if ((DateTime.Now - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                {
                    if (HandleGroupOperation(iterate))
                    {
                        ProcessFieldChangeWithScore(pOwner, iterate);
                    }

                    iterate.LastFall = DateTime.Now;
                }
            }

            if (GameOvered)
            {
                TetrisGame.Soundman.StopMusic();
                FinalGameTime = DateTime.Now - GameStartTime;
                PlayField.GameStats.TotalGameTime = FinalGameTime;
                NextAngleOffset = 0;
                pOwner.EnqueueAction(() => { pOwner.CurrentState = new GameOverGameState(this); });
            }

            if (PlayField.BlockGroups.Count == 0 && !SpawnWait && !pOwner.CurrentState.GameProcSuspended)
            {
                SpawnWait = true;
                pOwner.EnqueueAction
                (() =>
                {
                    Thread.Sleep(200);
                    SpawnNewTetromino();
                    SpawnWait = false;
                });
            }
        }

        private int LastScoreCalc = 0;
        private int LastScoreLines = 0;

        public virtual void ProcessFieldChangeWithScore(IStateOwner pOwner, BlockGroup Trigger)
        {
            int result = ProcessFieldChange(pOwner, Trigger);
            int AddScore = 0;
            if (result >= 1) AddScore += ((GameStats.LineCount / 10) + 1) * 15;
            if (result >= 2)
                AddScore += ((GameStats.LineCount / 10) + 2) * 30;
            if (result >= 3)
                AddScore += ((GameStats.LineCount / 10) + 3) * 45;
            if (result >= 4)
                AddScore += AddScore + ((GameStats.LineCount / 10) + 5) * 75;

            LastScoreCalc = AddScore;

            if (LastScoreLines == result) //getting the same lines in a row gives added score.
            {
                AddScore *= 2;
            }

            LastScoreLines = result;

            GameStats.AddScore(AddScore);

            pOwner.Feedback(0.9f * (float) result, result * 250);
        }

        static bool SpawnWait = false;
        static Random rgen = new Random();
        public DateTime GameStartTime = DateTime.MinValue;
        public DateTime LastPausedTime = DateTime.MinValue;
        private TimeSpan FinalGameTime = TimeSpan.MinValue;
        private StandardGameOptions GameOptions = new StandardGameOptions();

        private void RefillBlockQueue()
        {
            while (GameOptions.NextQueueSize > NextBlocks.Count)
            {
                var Generated = GenerateTetromino();
                NextBlocks.Enqueue(Generated);
            }
        }

        protected virtual void SpawnNewTetromino()
        {
            BlockHold = false;
            if (NextBlocks.Count == 0)
            {
                RefillBlockQueue();
            }

            var nextget = NextBlocks.Dequeue();
            PlayField.Theme.ApplyTheme(nextget, PlayField);
            if (NextBlocks.Count < GameOptions.NextQueueSize)
            {
                RefillBlockQueue();
            }

            nextget.X = (int) (((float) PlayField.ColCount / 2) - ((float) nextget.GroupExtents.Width / 2));
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

            SetLevelSpeed(nextget);
            NextAngleOffset += Math.PI * 2 / 5;

            PlayField.AddBlockGroup(nextget);
        }

        private void SetLevelSpeed(BlockGroup group)
        {
            group.FallSpeed = Math.Max(1000 - (PlayField.Level * 100), 50);
        }


        public virtual BlockGroup GenerateTetromino()
        {
            return Chooser.GetNext();
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
                        BlockGroup ArbitraryGroup = new BlockGroup();
                        ArbitraryGroup.AddBlock(new Point[] {Point.Empty}, GenerateColorBlock);
                        this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                        TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null);
                        GenerateColorBlock.DrawBlock(tbd);
                    }
                }
            }

            StatisticsBackground = buildbg;
        }

        RectangleF LastDrawStat = Rectangle.Empty;
        private Object LockTetImageRedraw = new Object();

        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            LastDrawStat = Bounds;
            if (StatisticsBackground == null || RedrawsNeeded)
            {
                GenerateStatisticsBackground();
            }

            g.DrawImage(StatisticsBackground, Bounds);
            //g.Clear(Color.Black);
            if (TetrominoImages == null || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(pOwner,Bounds);

            lock (LockTetImageRedraw)
            {
                var useStats = GameStats;
                double Factor = Bounds.Height / 644d;
                int DesiredFontPixelHeight = (int) (Bounds.Height * (23d / 644d));
                Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);
                var TopScore = this.GetLocalScores().GetScores().First().Score;
                int MaxScoreLength = Math.Max(TopScore.ToString().Length, useStats.Score.ToString().Length);

                String CurrentScoreStr = useStats.Score.ToString().PadLeft(MaxScoreLength + 2);
                String TopScoreStr = TopScore.ToString().PadLeft(MaxScoreLength + 2);
                //TODO: redo this segment separately, so we can have the labels left-aligned and the values right-aligned.
               // String BuildStatString = "Time:  " + FormatGameTime(pOwner).ToString().PadLeft(MaxScoreLength + 2) + "\n" +
               //                          "Score: " + CurrentScoreStr + "\n" +
               //                          "Top:   " + TopScoreStr + " \n" +
               //                          "Lines: " + GameStats.LineCount.ToString().PadLeft(MaxScoreLength+2);

                g.FillRectangle(LightenBrush, 0, 5, Bounds.Width, (int)(450 * Factor));
                String[] StatLabels = new string[]{"Time:","Score:","Top:","Lines:"};
                String[] StatValues = new string[]{FormatGameTime(pOwner),useStats.Score.ToString(),TopScore.ToString(), GameStats.LineCount.ToString() };
                Point StatPosition = new Point((int)(7 * Factor), (int)(7 * Factor));

                int CurrentYPosition = StatPosition.Y;
                for(int statindex=0;statindex<StatLabels.Length;statindex++)
                {
                    var MeasureLabel = g.MeasureString(StatLabels[statindex], standardFont);
                    var MeasureValue = g.MeasureString(StatValues[statindex], standardFont);
                    float LargerHeight = Math.Max(MeasureLabel.Height, MeasureValue.Height);

                    //we want to draw the current stat label at position StatPosition.X,CurrentYPosition...

                    TetrisGame.DrawText(g,standardFont,StatLabels[statindex],Brushes.Black,Brushes.White,StatPosition.X,CurrentYPosition);
                    
                    //we want to draw the current stat value at Bounds.Width-ValueWidth.
                    TetrisGame.DrawText(g,standardFont,StatValues[statindex],Brushes.Black,Brushes.White,(float)(Bounds.Width-MeasureValue.Width-(5*Factor)),CurrentYPosition);

                    //add the larger of the two heights to the current Y Position.
                    CurrentYPosition += (int)LargerHeight;
                    CurrentYPosition += 2;

                }




     

                Type[] useTypes = new Type[] {typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z)};
                int[] PieceCounts = new int[] {useStats.I_Piece_Count, useStats.O_Piece_Count, useStats.J_Piece_Count, useStats.T_Piece_Count, useStats.L_Piece_Count, useStats.S_Piece_Count, useStats.Z_Piece_Count};

                int StartYPos = (int) (140 * Factor);
                int useXPos = (int) (30 * Factor);
                ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
                for (int i = 0; i < useTypes.Length; i++)
                {
                    PointF BaseCoordinate = new PointF(useXPos, StartYPos + (int) ((float) i * (40d * Factor)));
                    PointF TextPos = new PointF(useXPos + (int) (100d * Factor), BaseCoordinate.Y);
                    String StatText = "" + PieceCounts[i];
                    SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                    Image TetrominoImage = TetrominoImages[useTypes[i]];
                    PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - TetrominoImage.Height / 2));

                    g.DrawImage(TetrominoImage, ImagePos);
                    g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                    g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
                }

                Point NextDrawPosition = new Point((int) (40f * Factor), (int) (420 * Factor));
                Size NextSize = new Size((int) (200f * Factor), (int) (200f * Factor));
                Point CenterPoint = new Point(NextDrawPosition.X + NextSize.Width / 2, NextDrawPosition.Y + NextSize.Height / 2);
                //now draw the "Next" Queue. For now we'll just show one "next" item.
                if (NextBlocks.Count > 0)
                {
                    var QueueList = NextBlocks.ToArray();
                    Image[] NextTetrominoes = (from t in QueueList select TetrominoImages[t.GetType()]).ToArray();
                    Image DisplayBox = TetrisGame.Imageman["display_box"];
                    //draw it at 40,420. (Scaled).
                    float ScaleDiff = 0;
                    iActiveSoundObject PlayingMusic;
                    if ((PlayingMusic = TetrisGame.Soundman.GetPlayingMusic_Active()) != null)
                        StoredLevels.Enqueue(PlayingMusic.Level);

                    if (StoredLevels.Count >= 4)
                    {
                        ScaleDiff = Math.Min(30, 10 * StoredLevels.Dequeue());
                    }

                    if (!TetrisGame.DJMode)
                    {
                        ScaleDiff = 0;
                    }

                    g.DrawImage
                    (DisplayBox,
                        new Rectangle(new Point((int) (NextDrawPosition.X - ScaleDiff), (int) (NextDrawPosition.Y - ScaleDiff)), new Size((int) (NextSize.Width + (ScaleDiff * 2)), (int) (NextSize.Height + (ScaleDiff * 2)))), 0, 0, DisplayBox.Width, DisplayBox.Height, GraphicsUnit.Pixel);

                    g.FillEllipse(Brushes.Black, CenterPoint.X - 5, CenterPoint.Y - 5, 10, 10);

                    for (int i = NextTetrominoes.Length - 1; i > -1; i--)
                    {
                        double StartAngle = Math.PI;
                        double AngleIncrementSize = (Math.PI * 1.8) / (double) NextTetrominoes.Length;
                        //we draw starting at StartAngle, in increments of AngleIncrementSize.
                        //i is the index- we want to increase the angle by that amount (well, obviously, I suppose...

                        double UseAngleCurrent = StartAngle + AngleIncrementSize * (float) i + NextAngleOffset;

                        double UseXPosition = CenterPoint.X + ((float) ((NextSize.Width) / 2.2) * Math.Cos(UseAngleCurrent));
                        double UseYPosition = CenterPoint.Y + ((float) ((NextSize.Height) / 2.2) * Math.Sin(UseAngleCurrent));


                        var NextTetromino = NextTetrominoes[i];

                        float Deviation = (i - NextTetrominoes.Length / 2);
                        Point Deviate = new Point((int) (Deviation * 20 * Factor), (int) (Deviation * 20 * Factor));

                        Point DrawTetLocation = new Point((int) UseXPosition - (NextTetromino.Width / 2), (int) UseYPosition - NextTetromino.Height / 2);
                        //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                        //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                        double AngleMovePercent = NextAngleOffset / AngleIncrementSize;
                        double NumAffect = NextAngleOffset == 0 ? 0 : AngleIncrementSize / NextAngleOffset;
                        Size DrawTetSize = new Size
                        (
                            (int) ((float) NextTetromino.Width * (0.3 + (1 - ((float) (i) * 0.15f) - .15f * AngleMovePercent))),
                            (int) ((float) NextTetromino.Height * (0.3 + (1 - ((float) (i) * 0.15f) - .15f * AngleMovePercent))));


                        //g.TranslateTransform(CenterPoint.X,CenterPoint.Y);
                        g.TranslateTransform(DrawTetLocation.X + DrawTetSize.Width / 2, DrawTetLocation.Y + DrawTetSize.Width / 2);
                        double DrawTetAngle = UseAngleCurrent;
                        DrawTetAngle += (Math.PI * AngleMovePercent);
                        float useDegrees = 180 + (float) (DrawTetAngle * (180 / Math.PI));

                        g.RotateTransform((float) useDegrees);
                        g.TranslateTransform(-(DrawTetLocation.X + DrawTetSize.Width / 2), -(DrawTetLocation.Y + DrawTetSize.Height / 2));
                        //g.TranslateTransform(-CenterPoint.X,-CenterPoint.Y);


                        if (DrawTetSize.Width > 0 && DrawTetSize.Height > 0)
                        {
                            //ImageAttributes Shade = GetShadowAttributes(1.0f - ((float)i * 0.3f));
                            ImageAttributes Shade = new ImageAttributes();
                            Shade.SetColorMatrix(ColorMatrices.GetFader(1.0f - ((float) i * 0.1f)));


                            g.DrawImage
                            (NextTetromino, new Rectangle((int) DrawTetLocation.X, (int) DrawTetLocation.Y, DrawTetSize.Width, DrawTetSize.Height), 0f, 0f,
                                (float) NextTetromino.Width, (float) NextTetromino.Height, GraphicsUnit.Pixel, Shade);
                        }

                        g.ResetTransform();
                    }
                }

                if (HoldBlock != null)
                {
                    Image HoldTetromino = TetrominoImages[HoldBlock.GetType()];
                    g.DrawImage(HoldTetromino, CenterPoint.X - HoldTetromino.Width / 2, CenterPoint.Y - HoldTetromino.Height / 2);
                }
            }
        }

        private Queue<float> StoredLevels = new Queue<float>();
        double NextAngleOffset = 0; //use this to animate the "Next" ring... Set it to a specific value and GameProc should reduce it to zero over time.
        Brush LightenBrush = new SolidBrush(Color.FromArgb(128, Color.MintCream));

        private TimeSpan GetElapsedTime(IStateOwner powner)
        {
            TimeSpan useCalc = (DateTime.Now - GameStartTime);

            if (FinalGameTime != TimeSpan.MinValue)
            {
                useCalc = FinalGameTime;
            }

            if (powner.CurrentState is PauseGameState || powner.CurrentState is UnpauseDelayGameState)
            {
                useCalc = LastPausedTime - GameStartTime;
            }

            return useCalc;
        }


        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = GetElapsedTime(stateowner);
            return useCalc.ToString(@"hh\:mm\:ss");
        }

        private RectangleF StoredBackground = RectangleF.Empty;

        private void RefreshBackground(RectangleF buildSize)
        {
            StoredBackground = buildSize;
            useBackground = new Bitmap((int) buildSize.Width, (int) buildSize.Height);
            using (Graphics bgg = Graphics.FromImage(useBackground))
            {
                Image drawbg = TetrisGame.Imageman["background"];
                bgg.CompositingQuality = CompositingQuality.AssumeLinear;
                bgg.DrawImage(drawbg, 0, 0, buildSize.Width, buildSize.Height);
            }
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        private Brush GhostBrush = new SolidBrush(Color.FromArgb(75, Color.DarkBlue));
        RectangleF StoredBlockImageRect = RectangleF.Empty;
        Image StoredBlockImage = null;

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (useBackground == null || !StoredBackground.Equals(Bounds))
            {
                RefreshBackground(Bounds);
            }

            g.DrawImage(useBackground, Bounds);


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
                        RectangleF BlockBounds = new RectangleF(BlockWidth * (GrabGhost.X + iterateblock.X), BlockHeight * (GrabGhost.Y + iterateblock.Y - 2), PlayField.GetBlockWidth(Bounds), PlayField.GetBlockHeight(Bounds));
                        TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, BlockBounds, GrabGhost);
                        ImageAttributes Shade = new ImageAttributes();
                        Shade.SetColorMatrix(ColorMatrices.GetFader(0.5f));
                        tbd.ApplyAttributes = Shade;
                        //tbd.OverrideBrush = GhostBrush;
                        iterateblock.Block.DrawBlock(tbd);
                    }
                }
            }
        }

        private void PerformRotation(IStateOwner pOwner, BlockGroup grp, bool ccw)
        {
            grp.Rotate(ccw);
            TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupRotate);
            pOwner.Feedback(0.3f, 100);
            grp.Clamp(PlayField.RowCount, PlayField.ColCount);
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_RotateCW || g == GameKeys.GameKey_RotateCCW)
            {
                bool ccw = g == GameKeys.GameKey_RotateCCW;
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (PlayField.CanRotate(activeitem, ccw))
                    {
                        PerformRotation(pOwner, activeitem, ccw);
                    }
                    else if (this.GameOptions.AllowWallKicks)

                    {
                        //we will add up to 3 and subtract up to 3 to the X coordinate. if any say we can rotate then we proceed with allowing the rotation.

                        int[] checkoffsets = new int[] {1, -1, 2, -2, 3, -3};


                        int OriginalPos = activeitem.X;
                        Boolean revertpos = true;

                        foreach (int currentoffset in checkoffsets)
                        {
                            if (currentoffset == 0) continue;
                            activeitem.X = OriginalPos + currentoffset;
                            if (PlayField.CanRotate(activeitem, ccw))
                            {
                                PerformRotation(pOwner, activeitem, ccw);
                                revertpos = false;
                                break;
                            }
                        }

                        if (revertpos)
                        {
                            activeitem.X = OriginalPos;
                        }
                    }
                }
            }
            else if (g == GameKeys.GameKey_Down)
            {
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (HandleGroupOperation(activeitem))
                    {
                        pOwner.Feedback(0.4f, 100);
                        ProcessFieldChangeWithScore(pOwner, activeitem);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Drop)
            {
                //drop all active groups.
                BlockGroup FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    int dropqty = 0;
                    var ghosted = GetGhostDrop(activeitem, out dropqty, 0);
                    PlayField.SetGroupToField(ghosted);
                    PlayField.RemoveBlockGroup(activeitem);
                    GameStats.AddScore((dropqty * (5 + (GameStats.LineCount / 10))));
                }

                pOwner.Feedback(0.6f, 200);
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupPlace);
                ProcessFieldChangeWithScore(pOwner, FirstGroup);
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
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupMove);
                        pOwner.Feedback(0.1f, 50);
                    }
                    else
                    {
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockStopped);
                        pOwner.Feedback(0.4f,75);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Pause)
            {
                if (g == GameKeys.GameKey_Pause)
                {
                    LastPausedTime = DateTime.Now;
                    pOwner.CurrentState = new PauseGameState(pOwner, this);

                    var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                    playing?.Pause();
                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Pause);
                }

                //pOwner.CurrentState = new PauseGameState(this);
            }
            else if (g == GameKeys.GameKey_Hold)
            {
                if (HoldBlock != null && !BlockHold)
                {
                    //if there is a holdblock, take it and put it into the gamefield and make the first active blockgroup the new holdblock,
                    //then set BlockHold to block it from being used until the next Tetromino is spawned.
                    BlockGroup FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                    if (FirstGroup != null)
                    {
                        PlayField.RemoveBlockGroup(FirstGroup);

                        PlayField.AddBlockGroup(HoldBlock);


                        PlayField.Theme.ApplyTheme(HoldBlock, PlayField);
                        HoldBlock.X = (int) (((float) PlayField.ColCount / 2) - ((float) HoldBlock.GroupExtents.Width / 2));
                        HoldBlock.Y = 0;
                        HoldBlock = FirstGroup;
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Hold);
                        pOwner.Feedback(0.9f, 40);
                        BlockHold = true;
                    }
                }
                else if (!BlockHold)
                {
                    BlockGroup FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                    if (FirstGroup != null)
                    {
                        PlayField.RemoveBlockGroup(FirstGroup);
                        HoldBlock = FirstGroup;
                        BlockHold = true;
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Hold);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Debug1)
            {
                pOwner.CurrentState = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], pOwner.CurrentState);
            }
            else if (g == GameKeys.GameKey_Debug2)
            {
                if (pOwner.CurrentState is StandardTetrisGameState)
                {
                    ((StandardTetrisGameState) pOwner.CurrentState).GameStats.Score += 1000;
                }
            }
            else if(g==GameKeys.GameKey_Debug3)
            {
                int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
                Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);
                Font ItemFont = new Font(TetrisGame.RetroFont, (int)((float)DesiredFontPixelHeight*(3f/4f)), FontStyle.Bold, GraphicsUnit.Pixel);
                //set state to a testing menu state.

                MenuState ms = new MenuState(BackgroundDrawers.StandardImageBackgroundDraw.GetStandardBackgroundDrawer());
                ms.StateHeader = "This is a Menu";
                ms.HeaderFont = standardFont;
                MenuStateTextMenuItem returnitem = new MenuStateTextMenuItem();
                returnitem.Font = ItemFont;
                returnitem.Text = "Return";
                returnitem.BackColor = Color.Transparent;
                returnitem.ForeColor = Color.DarkBlue;
                var OriginalState = pOwner.CurrentState;
                ms.MenuElements.Add(returnitem);

                var scaleitem = new MenuStateScaleMenuItem(pOwner);
                scaleitem.Font = ItemFont;
                ms.MenuElements.Add(scaleitem);

                ms.MenuItemActivated += (obj, e) =>
                {
                    if(e.MenuElement==returnitem)
                    {
                        pOwner.CurrentState = OriginalState;
                    }
                };
                for (int i=0;i<8;i++)
                {
                    MenuStateTextMenuItem mts = new MenuStateTextMenuItem();
                    mts.Font = ItemFont;
                    mts.BackColor = Color.Transparent;
                    mts.ForeColor = Color.Black;
                    mts.Text = "Item " + i.ToString();
                    ms.MenuElements.Add(mts);
                }
                pOwner.CurrentState = ms;
                
            }
        }

        bool BlockHold = false;

        private bool HandleGroupOperation(BlockGroup activeItem)
        {
            if (PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1))
            {
                activeItem.Y++;
            }
            else
            {
                if (GameOptions.MoveResetsSetTimer && (DateTime.Now - lastHorizontalMove).TotalMilliseconds > 250)
                {
                    PlayField.SetGroupToField(activeItem);
                    GameStats.AddScore(25 - activeItem.Y);

                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupPlace);
                    return true;
                }
            }

            return false;
        }
    }
}