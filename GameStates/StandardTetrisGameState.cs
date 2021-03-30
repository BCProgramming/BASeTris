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
using BASeTris.AI;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.DrawHelper;
using BASeTris.FieldInitializers;
using BASeTris.GameObjects;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Replay;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using Microsoft.SqlServer.Server;
using SkiaSharp;

namespace BASeTris.GameStates
{
    public class GameplayGameState : GameState
    {
        internal StandardTetrisGameStateDrawHelper _DrawHelper = new StandardTetrisGameStateDrawHelper();
        public Queue<Nomino> NextBlocks = new Queue<Nomino>();
        public Nomino HoldBlock = null;
        public List<BaseParticle> Particles = new List<BaseParticle>();
        public TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public bool DoRefreshBackground = false;
        BlockGroupChooser overrideChooser = null;
        public Choosers.BlockGroupChooser Chooser { get { return overrideChooser == null ? GameHandler.Chooser : overrideChooser; } set { overrideChooser = value; } }


        public override bool GamePlayActive { get { return true; } }

        //given a value, translates from an unscaled horizontal coordinate in the default width to the appropriate size of the playing field based on the presented bounds.
        public double GetScaledHorizontal(RectangleF Bounds, double Value)
        {
            return Value * (Bounds.Width / BASeTris.DefaultWidth);
        }
        public double GetScaledVertical(RectangleF Bounds, double Value)
        {
            return Value * (Bounds.Height / BASeTris.DefaultHeight);
        }

        public double GetUnScaledHorizontal(RectangleF Bounds, double ScaledValue)
        {
            return ScaledValue / (Bounds.Width / BASeTris.DefaultWidth);
        }
        public double GetUnscaledVertical(RectangleF Bounds, double ScaledValue)
        {
            return ScaledValue / (Bounds.Height / BASeTris.DefaultHeight);
        }
        public PointF GetScaledPoint(RectangleF Bounds, PointF Value)
        {
            return new PointF((float)(GetUnScaledHorizontal(Bounds, Value.X)), (float)(GetUnscaledVertical(Bounds, Value.Y)));
        }

        public PointF GetUnscaledPoint(RectangleF Bounds, PointF Value)
        {
            return new PointF((float)(GetScaledHorizontal(Bounds, Value.X)), (float)(GetScaledVertical(Bounds, Value.Y)));
        }
        public BaseStatistics GameStats
        {
            get { return GameHandler.Statistics; }
        }


        public virtual IHighScoreList<TetrisHighScoreData> GetLocalScores()
        {
            return GameHandler.GetHighScores();
        }



        public int currenttempo = 1;


        public bool GameOvered = false;

        public Nomino GetNext()
        {
            if (NextBlocks.Count == 0) return null;
            return NextBlocks.Peek();
        }
        public MenuState MainMenuState = null; //if this gameplay was spawned by the menu, this should be the top-level menu state.
        public IAudioHandler Sounds = null;
        public GameplayGameState(IGameCustomizationHandler Handler, FieldInitializer pFieldInitializer, IAudioHandler pAudio,MenuState MainMenu)
        {

            Sounds = pAudio;
            GameHandler = Handler;
            MainMenuState = MainMenu;

            PlayField = new TetrisField(Handler.DefaultTheme, Handler);
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
                NominoImages = null;
                NominoSKBitmaps = null;
                f_RedrawStatusBitmap = true;
                StatisticsBackground = null;
                f_RedrawTetrominoImages = true;
                foreach (var refreshgroup in PlayField.BlockGroups)
                {
                    PlayField.Theme.ApplyTheme(refreshgroup, GameHandler, PlayField);
                }
            }
        }

        ~GameplayGameState()
        {
            if (Chooser != null) Chooser.Dispose();
        }
        public void InvokePlayFieldLevelChanged(object sender, TetrisField.LevelChangeEventArgs e)
        {
            PlayField_LevelChanged(sender, e);
        }
        private void PlayField_LevelChanged(object sender, TetrisField.LevelChangeEventArgs e)
        {
            lock (LockTetImageRedraw)
            {
                NominoImages = null;
                NominoSKBitmaps = null;
            }

            //throw new NotImplementedException();
        }

        public bool f_RedrawTetrominoImages = false;
        public bool f_RedrawStatusBitmap = false;

        public Dictionary<String, List<Image>> NominoImages { protected set; get; } = null;

        private Dictionary<String, List<SKBitmap>> NominoSKBitmaps = null;

      
        public SKBitmap GetTetrominoSKBitmap(IStateOwner pOwner,Nomino nom)
        {
            String GetKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if (!NominoSKBitmaps.ContainsKey(GetKey))
            {
                return AddTetrominoBitmapSK(pOwner, nom);
            }
            return GetTetrominoSKBitmap(GetKey);
        }
        public SKBitmap GetTetrominoSKBitmap(Type sType)
        {
            String GetKey = PlayField.Theme.GetNominoTypeKey(sType, GameHandler, PlayField);
            return GetTetrominoSKBitmap(GetKey);
        }
        public SKBitmap GetTetrominoSKBitmap(String Source)
        {
            if (NominoSKBitmaps == null) NominoSKBitmaps = new Dictionary<String, List<SKBitmap>>();
            if (!NominoSKBitmaps.ContainsKey(Source))
            {
                if (NominoImages != null && NominoImages.ContainsKey(Source))
                {
                    NominoSKBitmaps.Add(Source, new List<SKBitmap>());
                    foreach (var copyGDI in NominoImages[Source])
                    {
                        NominoSKBitmaps[Source].Add(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(copyGDI)));
                    }
                }
                else
                {
                    return null;
                }

            }

            return TetrisGame.Choose(NominoSKBitmaps[Source]);

        }

        public bool HasTetrominoSKBitmaps() => NominoSKBitmaps != null;
        public void SetTetrominoSKBitmaps(Dictionary<String, List<SKBitmap>> bitmaps)
        {
            NominoSKBitmaps = bitmaps;
        }
        public bool HasTetrominoImages() => NominoImages != null;
        public Image AddTetrominoImage(IStateOwner pOwner,Nomino Source)
        {
            String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SizeF useTetSize = new SizeF(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField);

            Image buildBitmap = TetrisGame.OutLineImage(Source.GetImage(useTetSize));
            if (!NominoImages.ContainsKey(sAddKey))
                NominoImages.Add(sAddKey, new List<Image>() { buildBitmap });


            return buildBitmap;

        }
        public SKBitmap AddTetrominoBitmapSK(IStateOwner pOwner, Nomino Source)
        {
            String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SKSize useTetSize = new SKSize(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField);

            SKBitmap buildBitmap = TetrisGame.OutlineImageSK(Source.GetImageSK(useTetSize));
            if (!NominoSKBitmaps.ContainsKey(sAddKey))
                NominoSKBitmaps.Add(sAddKey, new List<SKBitmap>() { buildBitmap });


            return buildBitmap;
        }
        public Image GetTetrominoImage(IStateOwner pOwner,Nomino nom)
        {
            
            String sKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if(!NominoImages.ContainsKey(sKey))
            {
                return AddTetrominoImage(pOwner, nom);
            }
            return GetTetrominoImage(sKey);
        }
        public Image GetTetrominoImage(Type pType)
        {
            String sKey = PlayField.Theme.GetNominoTypeKey(pType, GameHandler, PlayField);
            return GetTetrominoImage(sKey);
        }
        public Image GetTetrominoImage(String TetrominoType)
        {
            return TetrisGame.Choose(NominoImages[TetrominoType]);
        }
        public SKBitmap[] GetTetrominoSKBitmaps() => TetrisGame.Coalesce(NominoSKBitmaps);
        public Image[] GetTetronimoImages() => TetrisGame.Coalesce(NominoImages);
            




        public void SetTetrominoImages(Dictionary<String,List<Image>> images)
        {
            NominoImages = images;
        }
        public void InvokeBlockGroupSet(object sender, TetrisField.BlockGroupSetEventArgs e)
        {
            PlayField_BlockGroupSet(sender, e);
        }
        private void PlayField_BlockGroupSet(object sender, TetrisField.BlockGroupSetEventArgs e)
        {
            if (e._groups.All((n)=>n.Y < 1))
            {
                GameOvered = true;
            }
            //reapply the theme when setting it down. Some themes may want
            //to have different appearances for blocks that are "set" versus those that are still "active".
            foreach (var group in e._groups)
            {
                var firstBlock = group.FirstOrDefault();
                Nomino useGroup = group;
                if (firstBlock != null) useGroup = firstBlock.Block.Owner ?? group;
                PlayField.Theme.ApplyTheme(useGroup, GameHandler, PlayField);
            }
        }

       

        public Nomino GetGhostDrop(IStateOwner pOwner,Nomino Source, out int dropLength, int CancelProximity = 3)
        {
            
            return PlayField.GetGhostDrop(pOwner, Source, out dropLength, CancelProximity);
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
       
        private void HandleActiveGroups(IStateOwner pOwner,bool ForceFall = false)
        {
        reprocess:
            List<Nomino> HandledGroups = new List<Nomino>();
            if(PlayField.BlockGroups.Count > 1)
            {
                ;
            }
            var AnyMoved = false;
            foreach (var iterate in from b in PlayField.BlockGroups orderby b.Y,PlayField.ColCount-b.X ascending select b)
            {
                if (iterate.Count() == 0) continue;
                if (ForceFall || (pOwner.GetElapsedTime() - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                {
                    if (HandleGroupOperation(pOwner, iterate))
                    {
                        if (!SuspendFieldSet)
                        {
                            GameHandler.ProcessFieldChange(this, pOwner, iterate);
                            //ProcessFieldChangeWithScore(pOwner, iterate);
                            HandledGroups.Add(iterate);
                            goto reprocess;
                        }
                    }
                    else 
                    {
                        AnyMoved = true;

                        if(iterate.MoveSound && !SuspendFieldSet)
                        {
                            //Make a movement sound as we fall.
                            Sounds.PlaySound(pOwner.AudioThemeMan.BlockFalling.Key);
                        }
                        iterate.LastFall = pOwner.GetElapsedTime();
                    }
                }
            }
            if (SuspendFieldSet && !AnyMoved) SuspendFieldSet = false; //all blocks fell...
        }
        public override void GameProc(IStateOwner pOwner)
        {
            if(ReplayData==null) ReplayData = new StatefulReplay();
            if (!FirstRun)
            {
                if (GameOptions.MusicEnabled)
                {
                    iActiveSoundObject musicplay = null;
                    if(pOwner.Settings.MusicOption=="<RANDOM>")
                    {
                        musicplay = Sounds.PlayMusic(pOwner.AudioThemeMan.BackgroundMusic.Key, pOwner.Settings.MusicVolume, true);
                    }
                    else
                    {
                        musicplay = Sounds.PlayMusic(pOwner.Settings.MusicOption, pOwner.Settings.MusicVolume, true);
                    }

                    
                    if (musicplay != null)
                    {
                        musicplay.Tempo = 1f;
                    }
                    FirstRun = true;
                    GameHandler.PrepareField(this, pOwner);
                }
            }

            //update particles.
            
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
          
            
            PlayField.AnimateFrame();
            HandleActiveGroups(pOwner);

            if (GameOvered)
            {
                //For testing: write out the replay data as a sequence of little images.
                //ReplayData.WriteStateImages("T:\\ReplayData");
                Sounds.StopMusic();
                pOwner.FinalGameTime = DateTime.Now - pOwner.GameStartTime;
                GameHandler.Statistics.TotalGameTime = pOwner.FinalGameTime;
                NextAngleOffset = 0;
                pOwner.EnqueueAction(() => { pOwner.CurrentState = new GameOverGameState(this,GameHandler.GetGameOverStatistics(this,pOwner)); });
            }

            if (PlayField.BlockGroups.Count == 0 && !SpawnWait && !pOwner.CurrentState.GameProcSuspended && !NoTetrominoSpawn)
            {
                SpawnWait = true;
                pOwner.EnqueueAction
                (() =>
                {
                    
                    SpawnNewTetromino(pOwner);
                    SpawnWait = false;
                });
            }
        }

        private int LastScoreCalc = 0;
        private int LastScoreLines = 0;
        public IGameCustomizationHandler GameHandler { get; set; } = new StandardTetrisHandler();
        /*public virtual void ProcessFieldChangeWithScore(IStateOwner pOwner, Nomino Trigger)
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
        }*/

        bool SpawnWait = false;
        static Random rgen = new Random();
        
        
        public StandardGameOptions GameOptions { get; set; } = new StandardGameOptions();

        public void RefillBlockQueue()
        {
            while (GameOptions.NextQueueSize > NextBlocks.Count)
            {
                var Generated = NominoTetromino();
                NextBlocks.Enqueue(Generated);
            }
        }
        public bool SuspendFieldSet { get; set; } = false;
        public bool NoTetrominoSpawn { get; set; } = false;
        protected virtual void SpawnNewTetromino(IStateOwner pOwner)
        {
            if (NoTetrominoSpawn) return;
            BlockHold = false;
            if (NextBlocks.Count == 0)
            {
                RefillBlockQueue();
            }

            var nextget = NextBlocks.Dequeue();
            
            if (NextBlocks.Count < GameOptions.NextQueueSize)
            {
                RefillBlockQueue();
            }

            nextget.X = (int) (((float) PlayField.ColCount / 2) - ((float) nextget.GroupExtents.Width / 2));
            nextget.SetY(null,0);
            if (GameStats is TetrisStatistics ts)
            {
                if (nextget is Tetromino_I)
                {
                    ts.I_Piece_Count++;
                }
                else if (nextget is Tetromino_J)
                    ts.J_Piece_Count++;
                else if (nextget is Tetromino_L)
                    ts.L_Piece_Count++;
                else if (nextget is Tetromino_O)
                    ts.O_Piece_Count++;
                else if (nextget is Tetromino_S)
                    ts.S_Piece_Count++;
                else if (nextget is Tetromino_T)
                    ts.T_Piece_Count++;
                else if (nextget is Tetromino_Z)
                    ts.Z_Piece_Count++;
                //FallSpeed is 1000 -50 for each level. Well, for now.
            }
            SetLevelSpeed(nextget);
            NextAngleOffset += Math.PI * 2 / 5;
            nextget.LastFall = pOwner.GetElapsedTime().Add(new TimeSpan(0,0,0,0,100));
            PlayField.AddBlockGroup(nextget);
            PlayField.Theme.ApplyTheme(nextget,GameHandler, PlayField);
        }

        private void SetLevelSpeed(Nomino group)
        {
            var Level = (GameHandler.Statistics is TetrisStatistics ts) ? ts.Level : 0;

            group.FallSpeed = Math.Max(1000 - (Level * 100), 50);
        }


        public virtual Nomino NominoTetromino()
        {
            var nextitem = Chooser.RetrieveNext();
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
                        this.PlayField.Theme.ApplyRandom(ArbitraryGroup,GameHandler,this.PlayField);
                        //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null,new StandardSettings());
                        RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);
                        
                    }
                }
            }

            StatisticsBackground = buildbg;
        }

        public RectangleF LastDrawStat = Rectangle.Empty;
        public Object LockTetImageRedraw = new Object();

       

        internal Queue<float> StoredLevels = new Queue<float>();
        public double NextAngleOffset = 0; //use this to animate the "Next" ring... Set it to a specific value and GameProc should reduce it to zero over time.
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
            if (!grp.Controllable) return;
            grp.Rotate(ccw);
            Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupRotate.Key, pOwner.Settings.EffectVolume);
            pOwner.Feedback(0.3f, 100);
            grp.Clamp(PlayField.RowCount, PlayField.ColCount);
        }
        private bool HandleBlockGroupKey(IStateOwner pOwner,GameKeys key)
        {
            bool AnyTrue = false;
            foreach(var activeitem in PlayField.BlockGroups)
            {
                if (activeitem.Controllable)
                {
                    AnyTrue |= activeitem.HandleGameKey(pOwner, key);
                }
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
                    if (!activeitem.Controllable) continue;
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
                HandleActiveGroups(pOwner, true);
               
            }
            else if (g == GameKeys.GameKey_Drop)
            {
                //drop all active groups.
                Nomino FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                


                if (FirstGroup != null)
                {
                    //store the block positions for each block in the nomino.
                    

                    foreach (var activeitem in PlayField.BlockGroups)
                    {
                        if (!activeitem.Controllable) continue;
                        List<Tuple<BCPoint,NominoElement>> StartBlockPositions = new List<Tuple<BCPoint, NominoElement>>();
                        List<Tuple<BCPoint, NominoElement>> EndBlockPositions = new List<Tuple<BCPoint, NominoElement>>();
                        foreach (var element in activeitem)
                        {
                            StartBlockPositions.Add(new Tuple<BCPoint, NominoElement>(new BCPoint(activeitem.X + element.X, activeitem.Y + element.Y),element));
                        }
                        int dropqty = 0;
                        var ghosted = GetGhostDrop(pOwner,activeitem, out dropqty, 0);
                        foreach (var element in ghosted)
                        {
                            EndBlockPositions.Add(new Tuple<BCPoint, NominoElement>(new BCPoint(ghosted.X + element.X, ghosted.Y + element.Y), element));
                        }
                        GenerateDropParticles(StartBlockPositions, EndBlockPositions);
                        activeitem.X = ghosted.X;
                        activeitem.SetY(pOwner, ghosted.Y);
                        PlayField.SetGroupToField(activeitem);
                        PlayField.RemoveBlockGroup(activeitem);
                        if (GameStats is TetrisStatistics ts)
                        {
                            GameStats.AddScore((dropqty * (5 + (ts.LineCount / 10))));
                        }
                        
                    }

                    pOwner.Feedback(0.6f, 200);
                    Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupPlace.Key, pOwner.Settings.EffectVolume);
                    GameHandler.ProcessFieldChange(this, pOwner, FirstGroup);
                    //ProcessFieldChangeWithScore(pOwner, FirstGroup);
                }
            }
            else if (g == GameKeys.GameKey_Right || g == GameKeys.GameKey_Left)
            {
                int XMove = g == GameKeys.GameKey_Right ? 1 : -1;
                foreach (var ActiveItem in PlayField.BlockGroups)
                {
                    if (!ActiveItem.Controllable) continue;
                    if (PlayField.CanFit(ActiveItem, ActiveItem.X + XMove, ActiveItem.Y,false)==TetrisField.CanFitResultConstants.CanFit)
                    {
                        lastHorizontalMove = DateTime.Now;
                        ActiveItem.X += XMove;
                        Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupMove.Key, pOwner.Settings.EffectVolume);
                        pOwner.Feedback(0.1f, 50);
                    }
                    else
                    {
                        Sounds.PlaySound(pOwner.AudioThemeMan.BlockStopped.Key, pOwner.Settings.EffectVolume);
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

                    var playing = Sounds.GetPlayingMusic_Active();
                    playing?.Pause();
                    Sounds.PlaySound(pOwner.AudioThemeMan.Pause.Key, pOwner.Settings.EffectVolume);
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
                        if (FirstGroup.Controllable)
                        {
                            PlayField.RemoveBlockGroup(FirstGroup);

                            PlayField.AddBlockGroup(HoldBlock);

                            //We probably should set the speed appropriately here for the level. As is it will retain the speed from whe nthe hold block was
                            //held.
                            PlayField.Theme.ApplyTheme(HoldBlock, GameHandler, PlayField);
                            HoldBlock.X = (int)(((float)PlayField.ColCount / 2) - ((float)HoldBlock.GroupExtents.Width / 2));
                            HoldBlock.SetY(pOwner, 0);
                            HoldBlock.HighestHeightValue = 0; //reset the highest height as well, so the falling animation doesn't goof
                            HoldBlock = FirstGroup;
                            Sounds.PlaySound(pOwner.AudioThemeMan.Hold.Key, pOwner.Settings.EffectVolume);
                            pOwner.Feedback(0.9f, 40);
                            BlockHold = true;
                        }
                    }
                }
                else if (!BlockHold)
                {
                    Nomino FirstGroup = PlayField.BlockGroups.FirstOrDefault();
                    if (FirstGroup != null)
                    {
                        if (FirstGroup.Controllable)
                        {
                            PlayField.RemoveBlockGroup(FirstGroup);
                            HoldBlock = FirstGroup;
                            BlockHold = true;
                            Sounds.PlaySound(pOwner.AudioThemeMan.Hold.Key, pOwner.Settings.EffectVolume);
                        }
                    }
                }
            }
            else if (g == GameKeys.GameKey_Debug1)
            {
                pOwner.CurrentState = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], pOwner.CurrentState);
            }
            else if (g == GameKeys.GameKey_Debug2)
            {

                OptionsMenuState OptionState = new OptionsMenuState(BackgroundDrawers.StandardImageBackgroundGDI.GetStandardBackgroundDrawer(),
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

                MenuState ms = new MenuState(BackgroundDrawers.StandardImageBackgroundGDI.GetStandardBackgroundDrawer());
                ms.StateHeader = "This is a Menu";
                ms.HeaderTypeface = standardFont.Name;
                ms.HeaderTypeSize = standardFont.Size;
                
                MenuStateTextMenuItem returnitem = new MenuStateTextMenuItem();
                returnitem.FontFace = ItemFont.Name;
                returnitem.FontSize = ItemFont.Size;
                returnitem.Text = "Return";
                returnitem.BackColor = Color.Transparent;
                returnitem.ForeColor = Color.DarkBlue;
                var OriginalState = pOwner.CurrentState;
                ms.MenuElements.Add(returnitem);

                var scaleitem = new MenuStateScaleMenuItem(pOwner);
                scaleitem.FontFace = ItemFont.Name;
                scaleitem.FontSize = ItemFont.Size;
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
                    mts.FontFace = ItemFont.Name;
                    mts.FontSize = ItemFont.Size;
                    mts.BackColor = Color.Transparent;
                    mts.ForeColor = Color.Black;
                    mts.Text = "Item " + i.ToString();
                    ms.MenuElements.Add(mts);
                }
                pOwner.CurrentState = ms;
                
            }
            else if(g==GameKeys.GameKey_Debug4)
            {
                if (pOwner is IGamePresenter gp)
                {
                    var _Present = gp.GetPresenter();
                    if (_Present.ai == null)
                {
                        _Present.ai = new TetrisAI(pOwner);
                    }
                else
                {
                        _Present.ai.AbortAI();
                        _Present.ai = null;
                    }
                }
            }
        }
        const int ParticlesPerBlock = 15;
        private void GenerateDropParticles(List<Tuple<BCPoint, NominoElement>> StartCoordinates,List<Tuple<BCPoint, NominoElement>> EndCoordinates)
        {
            for(int index=0;index<StartCoordinates.Count;index++)
            {
                
                var Original = StartCoordinates[index];
                var Drop = EndCoordinates[index];
                Bitmap ColorSource = null;
                if(Original.Item2.Block is ImageBlock)
                {
                    var casted = (Original.Item2.Block as ImageBlock);
                    var ImageBase = casted._RotationImages[MathHelper.mod(casted.Rotation, casted._RotationImages.Length)];
                    ColorSource = new Bitmap(ImageBase);
                }
                for (float y = Original.Item1.Y; y<Drop.Item1.Y-1;y++)
                {
                    GenerateDropParticles(new BCPoint(Original.Item1.X, y), 30, () => new BCPoint(0f, (float)((rgen.NextDouble() * 0.2f) + 0.4f)),
                        (xget,yget)=> {
                            var xposget = xget * ColorSource.Width;
                            var yposget = yget * ColorSource.Height;
                            var grabcolor = ColorSource.GetPixel((int)xposget, (int)yposget);
                            return grabcolor;

                        }  );
                }


            }





        }

        private void GenerateDropParticles(BCPoint Location,int NumGenerate,Func<BCPoint> VelocityGenerator,Func<float,float,BCColor> ColorFunc)
        {
            
            lock (Particles)
            {
                for (int i = 0; i < NumGenerate; i++)
                {
                    var genX = (float)rgen.NextDouble();
                    var genY = (float)rgen.NextDouble();
                    BCPoint Genpos = new BCPoint(Location.X + genX, Location.Y + genY);
                    BCColor ChosenColor = ColorFunc(genX, genY);
                    BaseParticle MakeParticle = new BaseParticle(Genpos, VelocityGenerator(), ChosenColor);
                    Particles.Add(MakeParticle);
                }
            }
        }
        bool BlockHold = false;
        
        private bool HandleGroupOperation(IStateOwner pOwner,Nomino activeItem)
        {

            if (activeItem.HandleBlockOperation(pOwner)) return true;
            var fitresult = PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1, false);
            if (fitresult == TetrisField.CanFitResultConstants.CanFit)
            {
                activeItem.SetY(pOwner,activeItem.Y+1);
            }
            else if (fitresult==TetrisField.CanFitResultConstants.CantFit_Field)
            {
                if (GameOptions.MoveResetsSetTimer && (DateTime.Now - lastHorizontalMove).TotalMilliseconds > pOwner.Settings.LockTime)
                {
                    var elapsed = pOwner.GetElapsedTime();
                    //any and all blockgroups in the field that are set not to allow input must have not moved in the last 750ms before we allow any groups to set.

                    var allgroups = PlayField.GetActiveBlockGroups();
                    var Applicable = allgroups.All((f) =>
                    {
                        return !f.Controllable || (elapsed - f.LastFall).TotalMilliseconds > 750;
                    });
                    Applicable = true;
                    if (Applicable)
                    {




                        PlayField.SetGroupToField(activeItem);
                        GameStats.AddScore(25 - activeItem.Y);
                        if (activeItem.PlaceSound)
                            Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupPlace.Key, pOwner.Settings.EffectVolume);
                        return true;
                    }
                }
            }
            

            return false;
        }
    }
}