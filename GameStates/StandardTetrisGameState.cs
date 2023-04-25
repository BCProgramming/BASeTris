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
using BASeTris.Settings;
using BASeTris.Theme;
using BASeTris.Particles;

namespace BASeTris.GameStates
{
    public class GameplayGameState : GameState
    {
        [Flags]
        public enum GameplayStateFlags
        {
            Normal,
            Paused = 1
        }
        internal StandardTetrisGameStateDrawHelper _DrawHelper = new StandardTetrisGameStateDrawHelper();
        public bool DrawNextQueue { get; set; } = true;
        public Queue<Nomino> NextBlocks = new Queue<Nomino>();
        public Nomino HoldBlock = null;
        public List<BaseParticle> TopParticles = new List<BaseParticle>();
        public List<BaseParticle> Particles = new List<BaseParticle>();
        public TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public bool DoRefreshBackground = false;
        public GameplayStateFlags Flags { get; set; } = GameplayStateFlags.Normal;
        BlockGroupChooser overrideChooser = null;

        public Choosers.BlockGroupChooser GetChooser(IStateOwner pOwner)
            {
            if (overrideChooser != null) return overrideChooser;
            return GameHandler.GetChooser(pOwner);
            }
        public void SetChooser(BlockGroupChooser value)
        {
            overrideChooser = value;
        }

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


        public virtual IHighScoreList GetLocalScores()
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
        public GameplayGameState(IStateOwner pOwner,IGameCustomizationHandler Handler, FieldInitializer pFieldInitializer, IAudioHandler pAudio,MenuState MainMenu)
        {

            Sounds = pAudio;
            GameHandler = Handler;
            MainMenuState = MainMenu;

            var GetFieldData = Handler.GetFieldInfo();


            int Columns = GetFieldData.FieldColumns;
            int Rows = GetFieldData.FieldRows; 
            int HiddenRows = GetFieldData.TopHiddenFieldRows;

            var GetSettingsTheme = NominoTheme.GetNewThemeInstanceByName(pOwner.Settings.GetSettings(Handler.Name).Theme, Handler.GetType());

            PlayField = new TetrisField(GetSettingsTheme, Handler,Rows,Columns,HiddenRows,GetFieldData.BottomHiddenFieldRows);
            PlayField.HIDDENROWS_BOTTOM = GetFieldData.BottomHiddenFieldRows;
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

                ImageManager.Reset();
                f_RedrawStatusBitmap = true;
                StatisticsBackground = null;
                f_RedrawTetrominoImages = true;
                foreach (var refreshgroup in PlayField.BlockGroups)
                {
                    PlayField.Theme.ApplyTheme(refreshgroup, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Theme_Changed);
                }
            }
        }

        ~GameplayGameState()
        {
            
        }
        public void InvokePlayFieldLevelChanged(object sender, TetrisField.LevelChangeEventArgs e)
        {
            PlayField_LevelChanged(sender, e);
        }
        private void PlayField_LevelChanged(object sender, TetrisField.LevelChangeEventArgs e)
        {
            lock (LockTetImageRedraw)
            {
                ImageManager.Reset();
            }

            //throw new NotImplementedException();
        }

        public bool f_RedrawTetrominoImages = false;
        public bool f_RedrawStatusBitmap = false;

        //public Dictionary<String, List<Image>> NominoImages { protected set; get; } = null;

        //private Dictionary<String, List<SKBitmap>> NominoSKBitmaps = null;

        private TetrominoImageManager _ImageManager = null;
        public TetrominoImageManager ImageManager
        {
            get
            {
                if (_ImageManager == null) _ImageManager = new TetrominoImageManager(GameHandler, PlayField);
                return _ImageManager;
            }
        }
        public SKBitmap GetTetrominoSKBitmap(IStateOwner pOwner, String pStringRep)
        {
            return GetTetrominoSKBitmap(pOwner, NNominoGenerator.CreateNomino(NNominoGenerator.FromString(pStringRep).ToList()));
        }
        public SKBitmap GetTetrominoSKBitmap(IStateOwner pOwner, List<NNominoGenerator.NominoPoint> Points)
        {
            Nomino genNom = NNominoGenerator.CreateNomino(Points);
            return GetTetrominoSKBitmap(pOwner, genNom);
        }
        public SKBitmap GetTetrominoSKBitmap(IStateOwner pOwner,Nomino nom)
        {
            return ImageManager.GetTetrominoSKBitmap(pOwner, nom);
            /*String GetKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if (!NominoSKBitmaps.ContainsKey(GetKey))
            {
                return AddTetrominoBitmapSK(pOwner, nom);
            }
            return GetTetrominoSKBitmap(GetKey);*/
        }
        public SKBitmap GetTetrominoSKBitmap(Type sType)
        {
            return ImageManager.GetTetrominoSKBitmap(sType);
            /*String GetKey = PlayField.Theme.GetNominoTypeKey(sType, GameHandler, PlayField);
            return GetTetrominoSKBitmap(GetKey);*/
        }
        public SKBitmap GetTetrominoSKBitmap(String Source)
        {
            return ImageManager.GetTetrominoSKBitmap(Source);
            /*if (NominoSKBitmaps == null) NominoSKBitmaps = new Dictionary<String, List<SKBitmap>>();
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
            */
        }

        public bool HasTetrominoSKBitmaps() => ImageManager.HasTetrominoSKBitmaps();
        public void SetTetrominoSKBitmaps(Dictionary<String, List<SKBitmap>> bitmaps)
        {
            ImageManager.SetTetrominoSKBitmaps(bitmaps);
        }
        public bool HasTetrominoImages() => ImageManager.HasTetrominoImages();
        public Image AddTetrominoImage(IStateOwner pOwner,Nomino Source)
        {
            return ImageManager.AddTetrominoImage(pOwner, Source);
            /*
            String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SizeF useTetSize = new SizeF(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Normal);

            Image buildBitmap = TetrisGame.OutLineImage(Source.GetImage(useTetSize));
            if (!NominoImages.ContainsKey(sAddKey))
                NominoImages.Add(sAddKey, new List<Image>() { buildBitmap });


            return buildBitmap;
            */
        }
        public SKBitmap AddTetrominoBitmapSK(IStateOwner pOwner, Nomino Source)
        {
            return ImageManager.AddTetrominoBitmapSK(pOwner, Source);
            /*String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SKSize useTetSize = new SKSize(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Normal);

            SKBitmap buildBitmap = TetrisGame.OutlineImageSK(Source.GetImageSK(useTetSize));
            if (!NominoSKBitmaps.ContainsKey(sAddKey))
                NominoSKBitmaps.Add(sAddKey, new List<SKBitmap>() { buildBitmap });


            return buildBitmap;*/
        }
        public Image GetTetrominoImage(IStateOwner pOwner,Nomino nom)
        {
            return ImageManager.GetTetrominoImage(pOwner, nom);   
            /*String sKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if(!NominoImages.ContainsKey(sKey))
            {
                return AddTetrominoImage(pOwner, nom);
            }
            return GetTetrominoImage(sKey);*/
        }
        public Image GetTetrominoImage(Type pType)
        {
            return ImageManager.GetTetrominoImage(pType);
            /*String sKey = PlayField.Theme.GetNominoTypeKey(pType, GameHandler, PlayField);
            return GetTetrominoImage(sKey);*/
        }
        public Image GetTetrominoImage(String TetrominoType)
        {
            return ImageManager.GetTetrominoImage(TetrominoType);
            //return TetrisGame.Choose(NominoImages[TetrominoType]);
        }
        public SKBitmap[] GetTetrominoSKBitmaps() => ImageManager.GetTetrominoSKBitmaps();
        public Image[] GetTetronimoImages() => ImageManager.GetTetronimoImages();
            




        public void SetTetrominoImages(Dictionary<String,List<Image>> images)
        {
            ImageManager.SetTetrominoImages(images);
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

            //TODO: for Dr Mario theme, the block set is using the "original" rotation when setting.
            //I suspect setting it to the field must be resetting the rotation of the blocks,
            
            foreach (var group in e._groups)
            {
                var firstBlock = group.FirstOrDefault();
                Nomino useGroup = group;
                if (firstBlock != null) useGroup = firstBlock.Block.Owner ?? group;
                
                PlayField.Theme.ApplyTheme(useGroup, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.FieldSet);
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
        internal bool FieldPrepared = false;
        internal bool FirstRun = false;
        private StatefulReplay ReplayData = null;
       
        private void HandleActiveGroups(IStateOwner pOwner,bool ForceFall = false)
        {
            bool reprocess = true;
            while (reprocess)
            {
                reprocess = false;
                List<Nomino> HandledGroups = new List<Nomino>();

                var AnyMoved = false;

                //go through each active block group, starting from the lowest to the highest.
                //TODO: can we optimize this better so we aren't sorting the groups constantly?
                foreach (var iterate in from abg in PlayField.BlockGroups orderby abg.Max((i) => i.Y) ascending select abg)
                {
                    //if this blockgroup is empty (somehow) we don't need to waste time with the rest of it.
                    if (iterate.Count() == 0) continue;
                    //if forcing a fall (down key was pressed for example) Or if we need to make it fall naturally because the appropriate fall speed elapsed, do so.
                    if (ForceFall || (pOwner.GetElapsedTime() - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                    {
                        if (HandleGroupOperation(pOwner, iterate)==GroupOperationResult.Operation_Success)
                        {
                            if (!SuspendFieldSet)
                            {
                                GameHandler.ProcessFieldChange(this, pOwner, iterate);
                                HandledGroups.Add(iterate);
                                reprocess = true;
                                continue;
                            }
                        }
                        else
                        {
                            AnyMoved = true;

                            if (iterate.MoveSound && !SuspendFieldSet)
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
        }
        public override void GameProc(IStateOwner pOwner)
        {
            if(ReplayData==null) ReplayData = new StatefulReplay();
            if (!FirstRun)
            {
                if (GameOptions.MusicEnabled)
                {
                    iActiveSoundObject musicplay = null;
                    if(pOwner.Settings.std.MusicOption =="<RANDOM>")
                    {
                        musicplay = Sounds.PlayMusic(pOwner.AudioThemeMan.BackgroundMusic.Key, pOwner.Settings.std.MusicVolume, true);
                    }
                    else
                    {
                        musicplay = Sounds.PlayMusic(pOwner.Settings.std.MusicOption, pOwner.Settings.std.MusicVolume, true);
                    }

                    
                    if (musicplay != null)
                    {
                        musicplay.Tempo = 1f;
                    }
                    FirstRun = true;
                    
                    GameHandler.PrepareField(this, pOwner);
                    pOwner.GameTime.Start();

                }
            }

            //update particles.
            if (GameHandler is IExtendedGameCustomizationHandler iextend)
            {
                
                iextend.GameProc(this, pOwner);
            }
            FrameUpdate();
            
          
            
            PlayField.AnimateFrame();
            HandleActiveGroups(pOwner);


            var currmusic = Sounds.GetPlayingMusic_Active();
            //if(currmusic!=null)
            //    currmusic.Pitch = TetrisGame.rgen.Next(2) - 1;
            if (GameOvered)
            {
                //For testing: write out the replay data as a sequence of little images.
                //ReplayData.WriteStateImages("T:\\ReplayData");
                Sounds.StopMusic();
                pOwner.GameTime.Stop();
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


        public GameOptions GameOptions
        {
            get
            {
                return this.GameHandler.GameOptions;        
            }
        }



        public void RefillBlockQueue(IStateOwner pOwner)
        {
            while (GameOptions.NextQueueSize > NextBlocks.Count)
            {
                var Generated = NominoTetromino(pOwner);
                NextBlocks.Enqueue(Generated);
            }
        }
        public bool SuspendFieldSet { get; set; } = false;
        public bool NoTetrominoSpawn { get; set; } = false;
        protected virtual void SpawnNewTetromino(IStateOwner pOwner)
        {
            //TODO (Possibly)- could we animate the next nomino in about 250ms from the position it is in in the "next" circle group, to above the playfield? 
            //Maybe we can implement that as part of the drawing code instead? it can record when the last nomino dropped and use that as a basis for
            //tweening between the "last" position of the next nomino and the middle top of the playfield.
            

            if (NoTetrominoSpawn) return;
            BlockHold = false;
            if (NextBlocks.Count == 0)
            {
                RefillBlockQueue(pOwner);
            }

            var nextget = NextBlocks.Dequeue();
            
            if (NextBlocks.Count < GameOptions.NextQueueSize)
            {
                RefillBlockQueue(pOwner);
            }

            nextget.X = (int) (((float) PlayField.ColCount / 2) - ((float) nextget.GroupExtents.Width / 2));
            nextget.SetY(null,0);
            if (GameStats is TetrisStatistics ts)
            {

                ts.IncrementPieceCount(nextget);
                /*
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
                */
                //FallSpeed is 1000 -50 for each level. Well, for now.
            }
            SetLevelSpeed(nextget);
            if (pOwner.GetHandler() is TetrisAttackHandler)
            {
                nextget.Y = 6;
                nextget.NoGhost = true;
                nextget.Flags = Nomino.NominoControlFlags.ControlFlags_NoClip | Nomino.NominoControlFlags.ControlFlags_DropMove;
            }
            NextAngleOffset += Math.PI * 2 / 5;
            nextget.LastFall = pOwner.GetElapsedTime().Add(new TimeSpan(0,0,0,0,100));
            PlayField.AddBlockGroup(nextget);
            PlayField.Theme.ApplyTheme(nextget,GameHandler, PlayField, NominoTheme.ThemeApplicationReason.NewNomino);
        }

        private void SetLevelSpeed(Nomino group)
        {
            var Level = (GameHandler.Statistics is TetrisStatistics ts) ? ts.Level : 0;

            group.FallSpeed = GameHandler is TetrisAttackHandler?16777216: Math.Max(1000 - (Level * 100), 50);
        }


        public virtual Nomino NominoTetromino(IStateOwner pOwner)
        {
            //HACK!
            //var hexpiece = NNominoGenerator.GetPiece(6);
            //var buildNomino = NNominoGenerator.CreateNomino(hexpiece);
            //return buildNomino;
            var useChooser = GetChooser(pOwner);
            var nextitem = useChooser.RetrieveNext();
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
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null,new SettingsManager());
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
            return useCalc.ToString(@"hh\:mm\:ss\:ff");
        }

     
        

        

        private void PerformRotation(IStateOwner pOwner, Nomino grp, bool ccw)
        {
            if (!grp.Controllable) return;
            grp.Rotate(ccw);
            Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupRotate.Key, pOwner.Settings.std.EffectVolume);
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

            if (GameHandler is IExtendedGameCustomizationHandler iextend)
            {

                var extendresult = iextend.HandleGameKey(this, pOwner, g);
                if (!extendresult.ContinueDefault) return;
            }


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
                    bool SomeDropped = false;

                    foreach (var activeitem in PlayField.BlockGroups)
                    {
                        if (!activeitem.Controllable) continue;
                        
                        if (activeitem.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_DropMove))
                        {
                            //"drop" should be interpreted as moving up instead.
                            HandleActiveMove(pOwner, 0, -1, activeitem);
                        }
                        else
                        {
                            SomeDropped = true;
                            List<Tuple<BCPoint, NominoElement>> StartBlockPositions = new List<Tuple<BCPoint, NominoElement>>();
                            List<Tuple<BCPoint, NominoElement>> EndBlockPositions = new List<Tuple<BCPoint, NominoElement>>();
                            foreach (var element in activeitem)
                            {
                                StartBlockPositions.Add(new Tuple<BCPoint, NominoElement>(new BCPoint(activeitem.X + element.X, activeitem.Y + element.Y), element));
                            }
                            int dropqty = 0;
                            var ghosted = GetGhostDrop(pOwner, activeitem, out dropqty, 0);
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
                    }
                    if (SomeDropped)
                    {
                        pOwner.Feedback(0.6f, 200);
                        Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupPlace.Key, pOwner.Settings.std.EffectVolume);
                        GameHandler.ProcessFieldChange(this, pOwner, FirstGroup);
                    }
                    //ProcessFieldChangeWithScore(pOwner, FirstGroup);
                }
            }
            else if (g == GameKeys.GameKey_Right || g == GameKeys.GameKey_Left)
            {
                int XMove = g == GameKeys.GameKey_Right ? 1 : -1;
                foreach (var ActiveItem in PlayField.BlockGroups)
                {
                    if (!ActiveItem.Controllable) continue;
                    HandleActiveMove(pOwner, XMove,0, ActiveItem);
                }
            }
            else if (g == GameKeys.GameKey_Pause)
            {
                if (g == GameKeys.GameKey_Pause)
                {
                    
                    pOwner.CurrentState = new PauseGameState(pOwner, this);
                    pOwner.GameTime.Stop();
                    var playing = Sounds.GetPlayingMusic_Active();
                    playing?.Pause();
                    Sounds.PlaySound(pOwner.AudioThemeMan.Pause.Key, pOwner.Settings.std.EffectVolume);
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
                            PlayField.Theme.ApplyTheme(HoldBlock, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Normal);
                            HoldBlock.X = (int)(((float)PlayField.ColCount / 2) - ((float)HoldBlock.GroupExtents.Width / 2));
                            HoldBlock.SetY(pOwner, 0);
                            HoldBlock.HighestHeightValue = 0; //reset the highest height as well, so the falling animation doesn't goof
                            HoldBlock = FirstGroup;
                            Sounds.PlaySound(pOwner.AudioThemeMan.Hold.Key, pOwner.Settings.std.EffectVolume);
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
                            Sounds.PlaySound(pOwner.AudioThemeMan.Hold.Key, pOwner.Settings.std.EffectVolume);
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

                OptionsMenuSettingsSelectorState OptionState = new OptionsMenuSettingsSelectorState(BackgroundDrawers.StandardImageBackgroundGDI.GetStandardBackgroundDrawer(),
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
                        _Present.ai = new StandardNominoAI(pOwner);
                    }
                else
                {
                        _Present.ai.AbortAI();
                        _Present.ai = null;
                    }
                }
            }
        }

        private void HandleActiveMove(IStateOwner pOwner, int XMove,int YMove, Nomino ActiveItem)
        {
            var FitResult = PlayField.CanFit(ActiveItem, ActiveItem.X + XMove, ActiveItem.Y + YMove, false);
            if (FitResult.Result == CanFitResults.CanFitResultConstants.CanFit || ActiveItem.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip))
            {
                if (ActiveItem.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip)) //CanFit won't be true fhere, however we might be working with a NoClip Nomino. In that case we want to block going off the edges.
                {

                    if (ActiveItem.Any((b) => ActiveItem.X + b.X + XMove < 0 || ActiveItem.X + b.X + XMove > PlayField.ColCount || ActiveItem.Y + b.Y + YMove < 0 || ActiveItem.Y + b.Y + YMove > PlayField.RowCount))
                    {
                        Sounds.PlaySound(pOwner.AudioThemeMan.BlockStopped.Key, pOwner.Settings.std.EffectVolume);
                        pOwner.Feedback(0.4f, 75);
                    }
                    else
                    {
                        ActiveItem.Y += YMove;
                        ActiveItem.X += XMove;
                        Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupMove.Key, pOwner.Settings.std.EffectVolume);
                        pOwner.Feedback(0.1f, 75);
                    }
                }
                else
                {
                    lastHorizontalMove = DateTime.Now;
                    ActiveItem.X += XMove;
                    ActiveItem.Y += YMove;
                    Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupMove.Key, pOwner.Settings.std.EffectVolume);
                    pOwner.Feedback(0.1f, 50);
                }
            }
            else
            {
                Sounds.PlaySound(pOwner.AudioThemeMan.BlockStopped.Key, pOwner.Settings.std.EffectVolume);
                pOwner.Feedback(0.4f, 75);
            }
        }

        const int ParticlesPerBlock = 15;
        private void GenerateDropParticles(List<Tuple<BCPoint, NominoElement>> StartCoordinates,List<Tuple<BCPoint, NominoElement>> EndCoordinates)
        {
            for(int index=0;index<StartCoordinates.Count;index++)
            {
                
                var Original = StartCoordinates[index];
                var Drop = EndCoordinates[index];
                var distancedropped = Math.Abs(Drop.Item1.Y - Original.Item1.Y);
                Bitmap ColorSource = null;
                if(Original.Item2.Block is ImageBlock)
                {
                    var casted = (Original.Item2.Block as ImageBlock);
                    var ImageBase = casted._RotationImages[MathHelper.mod(casted.Rotation, casted._RotationImages.Length)];
                    ColorSource = new Bitmap(ImageBase);
                }
                for (float y = Original.Item1.Y; y<Drop.Item1.Y-1;y++)
                {
                    GenerateDropParticles(new BCPoint(Original.Item1.X, y), (int)(Math.Min(1,10*(distancedropped/20))) , () => new BCPoint(0f, (float)((rgen.NextDouble() * 0.2f) + 0.4f)),
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
                    float genX;
                    float genY;


                    for (genX = 0; genX <= 0 || genX >=1; genX = (float)rgen.NextDouble()) ;
                    for (genY = 0; genY <= 0 || genY >=1; genY = (float)rgen.NextDouble()) ;
                    BCPoint Genpos = new BCPoint(Location.X + genX, Location.Y + genY);
                    BCColor ChosenColor = ColorFunc(genX, genY);
                    BaseParticle MakeParticle = new BaseParticle(Genpos, VelocityGenerator(), ChosenColor);
                    Particles.Add(MakeParticle);
                }
            }
        }
        bool BlockHold = false;
        public enum GroupOperationResult
        {
            Operation_Success,
            Operation_Error
        }
        
        private GroupOperationResult HandleGroupOperation(IStateOwner pOwner,Nomino activeItem)
        {

            if (activeItem.HandleBlockOperation(pOwner)) return GroupOperationResult.Operation_Success;
            var fitresult = PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1, false);
            if (fitresult.CanFit)
            {
                activeItem.SetY(pOwner,activeItem.Y+1);
            }
            else if(fitresult.CantFit_Active)
            {

            }
            else if (fitresult.CantFit_Field)
            {

                if (activeItem.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip))
                {
                    HandleActiveMove(pOwner, 0, 1, activeItem);
                    lastHorizontalMove = DateTime.Now;
                    //ignore!
                    //return GroupOperationResult.Operation_Success;
                }


                if (GameOptions.MoveResetsSetTimer && (DateTime.Now - lastHorizontalMove).TotalMilliseconds > pOwner.Settings.std.LockTime)
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


                        Dictionary<int, List<NominoElement>> ConnectionIndices = new Dictionary<int, List<NominoElement>>();
                        foreach (var checkblock in activeItem)
                        {
                            if (checkblock.Block is CascadingBlock cb)
                            {
                                if (!ConnectionIndices.ContainsKey(cb.ConnectionIndex))
                                {
                                    ConnectionIndices.Add(cb.ConnectionIndex, new List<NominoElement>());
                                }
                                ConnectionIndices[cb.ConnectionIndex].Add(checkblock);
                            }
                        }


                        

                        //Another concern is that we should "re-center" the Nominoes we create here.
                        if (true || ConnectionIndices.Count == 1)
                        {
                            PlayField.SetGroupToField(activeItem);
                            GameStats.AddScore(25 - activeItem.Y);
                            if (activeItem.PlaceSound)
                                Sounds.PlaySound(pOwner.AudioThemeMan.BlockGroupPlace.Key, pOwner.Settings.std.EffectVolume);
                            return GroupOperationResult.Operation_Success;
                        }
                        else
                        {
                            //special consideration needed for Blocks that have multiple connectionindices. Basically we would want to find all unsupported blocks in the nomino, and then create a new Nomino for each unique ConnectedIndex within them. 
                            //we would leave the activeItem with only the remaining supported blocks.
                        }
                    }
                }
            }
            

            return GroupOperationResult.Operation_Error;
        }
    }
}