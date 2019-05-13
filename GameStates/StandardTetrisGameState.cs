using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.DrawHelper;
using BASeTris.FieldInitializers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Replay;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using Microsoft.SqlServer.Server;

namespace BASeTris.GameStates
{
    public class StandardTetrisGameState : GameState
    {
        private StandardTetrisGameStateDrawHelper _DrawHelper = new StandardTetrisGameStateDrawHelper();
        public Queue<Nomino> NextBlocks = new Queue<Nomino>();
        public Nomino HoldBlock = null;
        private List<Particle> Particles = new List<Particle>();
        public TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public bool DoRefreshBackground = false;
        public Choosers.BlockGroupChooser Chooser = null;


        public override bool GamePlayActive { get{ return true; } }

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

        public virtual int ProcessFieldChange(IStateOwner pOwner, Nomino Trigger,out IList<HotLine> HotLines)
        {

            HotLines = new List<HotLine>();
            //process lines for standard game.
            ReplayData.CreateReplayState(pOwner, this);
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
                        if(PlayField.Flags.HasFlag(TetrisField.GameFlags.Flags_Hotline) && PlayField.HotLines.ContainsKey(r))
                        {
                            Debug.Print("Found hotline row at row " + r);
                            HotLines.Add(PlayField.HotLines[r]);
                        }
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
                    PlayField.GameStats.SetLevelTime(pOwner.GetElapsedTime());

                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.LevelUp, pOwner.Settings.EffectVolume);
                    PlayField.SetFieldColors();
                }

                if (rowsfound > 0 && rowsfound < 4)
                {
                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.ClearLine, pOwner.Settings.EffectVolume*2);
                }
                else if (rowsfound == 4)
                {
                    TetrisGame.Soundman.PlaySoundRnd(TetrisGame.AudioThemeMan.ClearTetris, pOwner.Settings.EffectVolume * 2);
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
                            if (GameOptions.MusicEnabled) TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, pOwner.Settings.MusicVolume, true);
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
                                TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, pOwner.Settings.MusicVolume, true);
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
                ReplayData.CreateReplayState(pOwner, this);
            }
        }

        private int currenttempo = 1;


        public bool GameOvered = false;

        public Nomino GetNext()
        {
            if (NextBlocks.Count == 0) return null;
            return NextBlocks.Peek();
        }

        public StandardTetrisGameState(BlockGroupChooser pChooser, FieldInitializer pFieldInitializer)
        {

            
            this.Chooser = pChooser;
            PlayField = new TetrisField();
            //PlayField.Settings = Settings;
            PlayField.OnThemeChangeEvent += PlayField_OnThemeChangeEvent;
            if (pFieldInitializer != null) pFieldInitializer.Initialize(PlayField);
            PlayField.BlockGroupSet += PlayField_BlockGroupSet;
            PlayField.SetStandardHotLines();
        }

        private void PlayField_OnThemeChangeEvent(object sender, OnThemeChangeEventArgs e)
        {
            lock (LockTetImageRedraw)
            {
                TetrominoImages = null;
                f_RedrawStatusBitmap = true;
                StatisticsBackground = null;
                f_RedrawTetrominoImages = true;
                foreach (var refreshgroup in PlayField.BlockGroups)
                {
                    PlayField.Theme.ApplyTheme(refreshgroup, PlayField);
                    }
            }
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
            //reapply the theme when setting it down. Some themes may want
            //to have different appearances for blocks that are "set" versus those that are still "active".
            var firstBlock = e._group.FirstOrDefault();
            Nomino useGroup = e._group;
            if (firstBlock != null) useGroup = firstBlock.Block.Owner ?? e._group;
            PlayField.Theme.ApplyTheme(useGroup,PlayField);
        }

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _DrawHelper.DrawProc(this,pOwner,g,Bounds);
        }

        public Nomino GetGhostDrop(IStateOwner pOwner,Nomino Source, out int dropLength, int CancelProximity = 3)
        {
            //routine returns the Ghost Drop representor of this Nomino.
            //this function will also return null if the dropped block is CancelProximity or closer to the place it would be dropped.
            Nomino Duplicator = new Nomino(Source);

            dropLength = 0;
            while (true)
            {
                if (PlayField.CanFit(Duplicator, Duplicator.X, Duplicator.Y + 1))
                {
                    dropLength++;
                    Duplicator.SetY(pOwner,Duplicator.Y+1);
                }
                else
                {
                    break;
                }
            }

            if (dropLength < CancelProximity) return null;
            foreach(var iterate in Duplicator)
            {
                iterate.Block.Owner = Source;
            }
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
        private StatefulReplay ReplayData = null;
        public override void GameProc(IStateOwner pOwner)
        {
            if(ReplayData==null) ReplayData = new StatefulReplay();
            if (!FirstRun)
            {
                if (GameOptions.MusicEnabled)
                {
                    var musicplay = TetrisGame.Soundman.PlayMusic(TetrisGame.AudioThemeMan.BackgroundMusic, pOwner.Settings.MusicVolume, true);
                    musicplay.Tempo = 1f;
                    FirstRun = true;
                }
            }

            FrameUpdate();
            if (pOwner.GameStartTime == DateTime.MinValue) pOwner.GameStartTime = DateTime.Now;
            if (pOwner.LastPausedTime != DateTime.MinValue)
            {
                pOwner.GameStartTime += (DateTime.Now - pOwner.LastPausedTime);
                pOwner.LastPausedTime = DateTime.MinValue;
                foreach(var iterate in PlayField.BlockGroups)
                {
                    iterate.LastFall = pOwner.GetElapsedTime();
                    iterate.HighestHeightValue = 0;
                }
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
                if ((pOwner.GetElapsedTime() - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                {
                    if (HandleGroupOperation(pOwner,iterate))
                    {
                        
                        ProcessFieldChangeWithScore(pOwner, iterate);
                        
                    }

                    iterate.LastFall = pOwner.GetElapsedTime();
                }
            }

            if (GameOvered)
            {
                //For testing: write out the replay data as a sequence of little images.
                //ReplayData.WriteStateImages("T:\\ReplayData");
                TetrisGame.Soundman.StopMusic();
                pOwner.FinalGameTime = DateTime.Now - pOwner.GameStartTime;
                PlayField.GameStats.TotalGameTime = pOwner.FinalGameTime;
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

        public virtual void ProcessFieldChangeWithScore(IStateOwner pOwner, Nomino Trigger)
        {
            
            int result = ProcessFieldChange(pOwner, Trigger,out IList<HotLine> HotLines);
            int AddScore = 0;
            if (result >= 1) AddScore += ((GameStats.LineCount / 10) + 1) * 15;
            if (result >= 2)
                AddScore += ((GameStats.LineCount / 10) + 2) * 30;
            if (result >= 3)
                AddScore += ((GameStats.LineCount / 10) + 3) * 45;
            if (result >= 4)
                AddScore += AddScore + ((GameStats.LineCount / 10) + 5) * 75;

            if(HotLines!=null)
            {
                double SumMult = 00;
                foreach(var iterate in HotLines)
                {
                    SumMult += iterate.Multiplier;
                }
                AddScore = (int)((double)AddScore * SumMult);
            }
            
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
            nextget.SetY(null,0);

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

        private void SetLevelSpeed(Nomino group)
        {
            group.FallSpeed = Math.Max(1000 - (PlayField.Level * 100), 50);
        }


        public virtual Nomino GenerateTetromino()
        {
            var nextitem = Chooser.GetNext();
            //add additional processing here- for example Sticky tetris and cascade tetris should
            //modify colouring of blocks.

            return nextitem;
        }
        
        Image StatisticsBackground = null;

        public void GenerateStatisticsBackground()
        {
            Bitmap buildbg = new Bitmap(1120, 2576);
            Size BlockSize = new Size(128, 128);
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
                        Nomino ArbitraryGroup = new Nomino();
                        ArbitraryGroup.AddBlock(new Point[] {Point.Empty}, GenerateColorBlock);
                        this.PlayField.Theme.ApplyRandom(ArbitraryGroup,this.PlayField);
                        //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null,new StandardSettings());
                        RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);
                        
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
                        float useDegrees = 180 + (float)(DrawTetAngle * (180 / Math.PI));

                        g.RotateTransform((float)useDegrees);

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

        


        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss");
        }

     

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        

        

        private void PerformRotation(IStateOwner pOwner, Nomino grp, bool ccw)
        {
            grp.Rotate(ccw);
            TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupRotate, pOwner.Settings.EffectVolume);
            pOwner.Feedback(0.3f, 100);
            grp.Clamp(PlayField.RowCount, PlayField.ColCount);
        }
        private bool HandleBlockGroupKey(IStateOwner pOwner,GameKeys key)
        {
            bool AnyTrue = false;
            foreach(var activeitem in PlayField.BlockGroups)
            {
                AnyTrue |= activeitem.HandleGameKey(pOwner, key);
            }

            return AnyTrue;

        }
        
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {

            if (HandleBlockGroupKey(pOwner, g)) return;


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
                    if (HandleGroupOperation(pOwner,activeitem))
                    {
                        pOwner.Feedback(0.4f, 100);
                        ProcessFieldChangeWithScore(pOwner, activeitem);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Drop)
            {
                //drop all active groups.
                Nomino FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                if (FirstGroup != null)
                {
                    foreach (var activeitem in PlayField.BlockGroups)
                    {
                        int dropqty = 0;
                        var ghosted = GetGhostDrop(pOwner,activeitem, out dropqty, 0);
                        PlayField.SetGroupToField(ghosted);
                        PlayField.RemoveBlockGroup(activeitem);
                        GameStats.AddScore((dropqty * (5 + (GameStats.LineCount / 10))));
                    }

                    pOwner.Feedback(0.6f, 200);
                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupPlace, pOwner.Settings.EffectVolume);
                    ProcessFieldChangeWithScore(pOwner, FirstGroup);
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
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupMove, pOwner.Settings.EffectVolume);
                        pOwner.Feedback(0.1f, 50);
                    }
                    else
                    {
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockStopped, pOwner.Settings.EffectVolume);
                        pOwner.Feedback(0.4f,75);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Pause)
            {
                if (g == GameKeys.GameKey_Pause)
                {
                    pOwner.LastPausedTime = DateTime.Now;
                    pOwner.CurrentState = new PauseGameState(pOwner, this);

                    var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                    playing?.Pause();
                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Pause, pOwner.Settings.EffectVolume);
                }

                //pOwner.CurrentState = new PauseGameState(this);
            }
            else if (g == GameKeys.GameKey_Hold)
            {
                if (HoldBlock != null && !BlockHold)
                {
                    //if there is a holdblock, take it and put it into the gamefield and make the first active blockgroup the new holdblock,
                    //then set BlockHold to block it from being used until the next Tetromino is spawned.
                    Nomino FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                    if (FirstGroup != null)
                    {
                        PlayField.RemoveBlockGroup(FirstGroup);

                        PlayField.AddBlockGroup(HoldBlock);

                        //We probably should set the speed appropriately here for the level. As is it will retain the speed from whe nthe hold block was
                        //held.
                        PlayField.Theme.ApplyTheme(HoldBlock, PlayField);
                        HoldBlock.X = (int) (((float) PlayField.ColCount / 2) - ((float) HoldBlock.GroupExtents.Width / 2));
                        HoldBlock.SetY(pOwner,0);
                        HoldBlock.HighestHeightValue = 0; //reset the highest height as well, so the falling animation doesn't goof
                        HoldBlock = FirstGroup;
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Hold, pOwner.Settings.EffectVolume);
                        pOwner.Feedback(0.9f, 40);
                        BlockHold = true;
                    }
                }
                else if (!BlockHold)
                {
                    Nomino FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                    if (FirstGroup != null)
                    {
                        PlayField.RemoveBlockGroup(FirstGroup);
                        HoldBlock = FirstGroup;
                        BlockHold = true;
                        TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Hold, pOwner.Settings.EffectVolume);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Debug1)
            {
                pOwner.CurrentState = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], pOwner.CurrentState);
            }
            else if (g == GameKeys.GameKey_Debug2)
            {

                OptionsMenuState OptionState = new OptionsMenuState(BackgroundDrawers.StandardImageBackgroundDraw.GetStandardBackgroundDrawer(),
                    pOwner, pOwner.CurrentState);

                pOwner.CurrentState = OptionState;
                /*if (pOwner.CurrentState is StandardTetrisGameState)
                {
                    ((StandardTetrisGameState) pOwner.CurrentState).GameStats.Score += 1000;
                }*/
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
        
        private bool HandleGroupOperation(IStateOwner pOwner,Nomino activeItem)
        {

            if (activeItem.HandleBlockOperation(pOwner)) return true;
            if (PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1))
            {
                activeItem.SetY(pOwner,activeItem.Y+1);
            }
            else
            {
                if (GameOptions.MoveResetsSetTimer && (DateTime.Now - lastHorizontalMove).TotalMilliseconds > pOwner.Settings.LockTime)
                {
                    PlayField.SetGroupToField(activeItem);
                    GameStats.AddScore(25 - activeItem.Y);

                    TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.BlockGroupPlace, pOwner.Settings.EffectVolume);
                    return true;
                }
            }

            return false;
        }
    }
}