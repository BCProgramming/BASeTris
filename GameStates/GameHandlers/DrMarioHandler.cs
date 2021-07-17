using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameObjects;
using BASeTris.GameStates.GameHandlers.HandlerStates;
using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
using SkiaSharp;

namespace BASeTris.GameStates.GameHandlers
{
    //placeholder attribute: a Dr Mario scoring handler should be implemented...
    [GameScoringHandler(typeof(DrMarioAIScoringHandler),typeof(StoredBoardState.DrMarioScoringRuleData))]
    public class DrMarioHandler : IGameCustomizationHandler
    {
        public String Name { get { return "Dr. Mario"; } }
        public DrMarioStatistics Statistics { get; private set; } = new DrMarioStatistics();
        BaseStatistics IGameCustomizationHandler.Statistics { get { return this.Statistics; }  }
        public bool AllowFieldImageCache { get { return false; } }
        public StandardGameOptions GameOptions { get; } = new StandardGameOptions() {AllowWallKicks = false } ;
        public BlockGroupChooser Chooser  {
            get {
                var result = Duomino.Duomino.BagTetrominoChooser();
                result.ResultAffector = DrMarioNominoTweaker;
                return result;
    } }
        public GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }
        public Nomino[] GetNominos()
        {
            return new Nomino[] { new Duomino.Duomino() };
        }
        private void DrMarioNominoTweaker(Nomino Source)
        {
            //tweak the nomino and set a random combining index.
            foreach (var iterate in Source)
            {
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    lsb.CombiningIndex = TetrisGame.Choose(new LineSeriesBlock.CombiningTypes[] { LineSeriesBlock.CombiningTypes.Yellow, LineSeriesBlock.CombiningTypes.Red, LineSeriesBlock.CombiningTypes.Blue });
                }
            }
            
            
        }
        public IHighScoreList<TetrisHighScoreData> GetHighScores()
        {
            return null;
        }

        public IGameCustomizationHandler NewInstance()
        {
            return new DrMarioHandler();
        }
    
        //finds critical mass excesses starting from the given position.
        //this basically only finds one specific "critical mass"
        private HashSet<Point> FindCriticalMassesHorizontal(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
               int CurrentMass = 0;
                int FirstCol = StartPosition.X;
               //first, we look to the left until we no longer find a matching block.

               for(int X=StartPosition.X-1;X > 0;X--)
                {
                    var CheckPos = state.PlayField.Contents[StartPosition.Y][X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                    {
                        FirstCol = X;
                    }
                    else
                        break;
                }

                //starting from FirstCol we will work through and find all matching combining indices....
                int HorizontalMass = 1; //start at one since we know we have one block.
                int MaxCriticalHorz = 4;
                List<Point> Horizontals = new List<Point>();
                Horizontals.Add(StartPosition);
            for(int X = FirstCol;X<state.PlayField.ColCount;X++)
                {
                    var CheckPos = state.PlayField.Contents[StartPosition.Y][X] as LineSeriesBlock;
                    if(CheckPos==null) break;
                    if(CheckPos.CombiningIndex==OurPos.CombiningIndex)
                    {
                        MaxCriticalHorz = Math.Max(MaxCriticalHorz, CheckPos.CriticalMass);
                        Horizontals.Add(new Point(X, StartPosition.Y));
                        HorizontalMass++;

                    }
                    else
                    {
                        break;
                    }
                }
            
                if(HorizontalMass > MaxCriticalHorz)
                {
                    foreach(var iterate in Horizontals)
                    {
                        if(!FoundPoints.Contains(iterate))
                        {
                            FoundPoints.Add(iterate);
                        }
                    }
                }
            }
            return FoundPoints;
        }
        private HashSet<Point> FindCriticalMassesVertical(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
                int CurrentMass = 0;
                int FirstRow = StartPosition.Y;
                //first, we look up until we no longer find a matching block.

                for (int Y = StartPosition.Y - 1; Y > 0; Y--)
                {
                    var CheckPos = state.PlayField.Contents[Y][StartPosition.X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                        FirstRow = Y;
                    else
                        break;
                }

                


                //starting from FirstCol we will work through and find all matching combining indices....
                int VerticalMass = 0; //start at one since we know we have one block.
                int MaxCriticalVert = 4;
                List<Point> Verticals = new List<Point>();
                Verticals.Add(StartPosition);
                for (int Y = FirstRow; Y < state.PlayField.RowCount; Y++)
                {
                    var CheckPos = state.PlayField.Contents[Y][StartPosition.X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                    {
                        MaxCriticalVert = Math.Max(MaxCriticalVert, CheckPos.CriticalMass);
                        Verticals.Add(new Point(StartPosition.X, Y));
                        VerticalMass++;

                    }
                    else
                    {
                        break;
                    }
                }

                if (VerticalMass >= MaxCriticalVert)
                {
                    foreach (var iterate in Verticals)
                    {
                        if (!FoundPoints.Contains(iterate))
                        {
                            FoundPoints.Add(iterate);
                        }
                    }
                }
            }
            return FoundPoints;
        }
        /// <summary>
        /// Searches the Field to find Critical masses. These are cases where the necessary number of blocks are lined up.
        /// returns the position (X=Column, Y=Row) in the gamefield that these were found.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pOwner"></param>
        /// <param name="StartPosition"></param>
        /// <returns></returns>
        private HashSet<Point> FindCriticalMasses(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {
           
            HashSet<Point> Horizontal = FindCriticalMassesHorizontal(state, pOwner, StartPosition);
            HashSet<Point> Vertical = FindCriticalMassesVertical(state, pOwner, StartPosition);
            foreach(Point verticalpoint in Vertical)
            {
                if (!Horizontal.Contains(verticalpoint)) Horizontal.Add(verticalpoint);
            }
            return Horizontal;

        }
        public List<Nomino> ProcessBlockDroppage(GameplayGameState state, int Column,int Row, ref HashSet<Nomino> AdditionalSkipBlocks)
        {
            List<Nomino> CreateResult = new List<Nomino>();
            var currentblock = state.PlayField.Contents[Row][Column];
            bool isPopping = false;
            if (currentblock != null)
            {
                if (currentblock is CascadingBlock cb)
                {
                    if (currentblock is LineSeriesBlock lsb)
                    {
                        isPopping = lsb.Popping;  //blocks that are popping shouldn't be resurrected.
                    }
                    if (!cb.IsSupported(cb.Owner, state.PlayField,new[] { cb }.ToList()) && !AdditionalSkipBlocks.Contains(cb.Owner))
                    {
                        //we initialize the list of recursion blocks to the block we are testing, since it cannot support itself.
                        //resurrect this block and other blocks that are in the same nomino.
                        //since we remove busted blocks from the nomino, we can take the Duomino this
                        //block belongs to and add it back to the Active Groups, then remove all the blocks that are in the nomino from the field.
                        foreach (var iterate in cb.Owner)
                        {
                            var useX = iterate.X + cb.Owner.X;
                            var useY = iterate.Y + cb.Owner.Y;
                            state.PlayField.Contents[useY][useX] = null;
                        }

                        Nomino resurrect = cb.Owner;
                        resurrect.Controllable = false;
                        resurrect.FallSpeed = 250;
                        resurrect.MoveSound = true;
                        resurrect.PlaceSound = false;
                        resurrect.NoGhost = true;
                        AdditionalSkipBlocks.Add(resurrect);
                        CreateResult.Add(resurrect);
                        
                    }
                }
            
            //now recursively process for the block to our left, the block to our right, and the block above. But only if that block is not part of the same nomino as currentblock or currentblock is null.

            foreach(Point offset in new Point[] { new Point(-1,0),new Point(0,-1),new Point(1,0)})
            {
                var checkblock = state.PlayField.Contents[Row + offset.Y][Column + offset.X];
                if(checkblock!=null && (currentblock==null || !currentblock.Owner.HasBlock(checkblock)))
                {
                    List<Nomino> CurrResult = ProcessBlockDroppage(state, Column + offset.X, Row + offset.Y, ref AdditionalSkipBlocks);
                    foreach(var iterateresult in CurrResult)
                        {
                            CreateResult.Add(iterateresult);
                        }
                }
                
            }

            }


            return CreateResult;
        }
        public int Level { get; set; } = 0;
        public int VirusCount = 0;
        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            if (state.PlayField.GetActiveBlockGroups().Count() > 0) return new FieldChangeResult() { ScoreResult=0};
            //here we would go through the field and handle where the blocks line up to more than the required critical mass. 
            
            //Nomino's have two blocks- usually. But, we should account for more. This handler may be expanded for the Tetris2 handler, (if we ever bother to make one)
            //in any case we want to check all the positions of the trigger nomino and check for critical masses.
            int MasterCount = 0;
            bool LevelCompleted = false;
            HashSet<Point> CriticalMasses = null;
            for (int y = 0; y < state.PlayField.RowCount ; y++)
            {
                var currRow = state.PlayField.Contents[y];
                for (int x =0;x<state.PlayField.ColCount;x++)
                {
                    if(state.PlayField.Contents[y][x] is LineSeriesMasterBlock)
                    {
                        MasterCount++;
                    }
                    if (state.PlayField.Contents[y][x] is LineSeriesBlock)
                    {
                        var foundmasses = FindCriticalMasses(state, pOwner, new Point(x, y));
                        foreach (var iterate in foundmasses)
                        {
                            if (CriticalMasses == null) CriticalMasses = new HashSet<Point>(foundmasses);
                            else if (!CriticalMasses.Contains(iterate)) CriticalMasses.Add(iterate);
                        }
                    }

                }
            }
            VirusCount = MasterCount;

            //if MasterCount is 0 then we completed this level.
            //if there are no viruses left, this level is now complete. We need a "Level complete" screen state with an overlay- we would switch to that state. It should
            //operate similar to the TemporaryInputPauseGameState in that we provide a routine to be called after the user opts to press a button to continue.
            if(MasterCount==0)
            {
                LevelCompleted = true;
            }

            if (CriticalMasses!=null && CriticalMasses.Any())
            {
                state.NoTetrominoSpawn = true;

                //process the critical masses.
                //first: we need to switch the blocks in question to "pop" them.
                //then we need to switch to a temporary state that allows them to display as "popped" for a moment or so, without processing drops or other actions.

                //after the delay expires, the state will then process the critical mass blocks, changing the underlying nomino to remove the deleted block, so that if only part of a nomino is cleared
                //the other parts are separated from it.

                //then it will check the full field again, changing unsupported field blocks into active groups and removing them from the field. 
                // Blocks that are part of a nomino will be resurrected with the other blocks that are part of that nomino.)

                //check the field again and change unsupported field blocks back into active groups.

                HashSet<Nomino> MassNominoes = new HashSet<Nomino>();
                foreach(var iterate in CriticalMasses)
                {
                    
                    var popItem = state.PlayField.Contents[iterate.Y][iterate.X];
                    
                    if (popItem is LineSeriesBlock lsb)
                    {
                        lsb.Popping = true;
                        GeneratePopParticles(pOwner, state, new SKPointI(iterate.X, iterate.Y));
                        if (popItem.Owner!=null)
                            state.PlayField.Theme.ApplyTheme(popItem.Owner, this, state.PlayField);
                        else
                        {
                            var Dummino = new Nomino() { };
                            Dummino.AddBlock(new Point[] { new Point(0, 0) }, popItem);
                            state.PlayField.Theme.ApplyTheme(Dummino, this, state.PlayField);
                        }
                        
                        
                        
                    }
                    if (popItem.Owner != null)
                        popItem.Owner.RemoveBlock(popItem);
                    state.PlayField.HasChanged = true;
                }
                var originalstate = state;
                state.Sounds.PlaySound(pOwner.AudioThemeMan.BlockPop.Key);


                
                
                //need to determine a way to detect chains here, where we create an active block and then it results in another "pop".

                
                
                TemporaryInputPauseGameState tpause = new TemporaryInputPauseGameState(state, 1000, (owner) =>
                {
                    //first, remove the CriticalMasses altogether.
                    foreach (var iterate in CriticalMasses)
                    {
                        //clear out the cell at the appropriate position.
                        var popItem = state.PlayField.Contents[iterate.Y][iterate.X];
                        state.PlayField.Contents[iterate.Y][iterate.X] = null;
                        //now apply the theme to the specified location
                        if (popItem.Owner != null)
                            state.PlayField.Theme.ApplyTheme(popItem.Owner, this, state.PlayField);
                        else
                        {
                            //create a "dummy" nomino for the application of the "pop" theme animation.
                            var Dummino = new Nomino() { };
                            Dummino.AddBlock(new Point[] { new Point(0, 0) }, popItem);
                            state.PlayField.Theme.ApplyTheme(Dummino, this, state.PlayField);
                        }


                    }
                    //algorithm change: instead of going through the entire field, we'll go through all the critical masses.
                    //With Each one:
                    //check the block to the left, to the right, and above.


                    //next, go through the entire field.
                    List<NominoBlock> CheckedBlocks = new List<NominoBlock>();
                    HashSet<Nomino> ResurrectNominos = new HashSet<Nomino>();
                    HashSet<CascadingBlock> AddedBlockAlready = new HashSet<CascadingBlock>();
                    
                    //keep track of the blocks we've examined already.
                    for(int row =0;row<state.PlayField.RowCount;row++)
                    {
                        for (int column = 0; column < state.PlayField.ColCount; column++)
                        {

                            var currentblock = state.PlayField.Contents[row][column];
                            bool isPopping = false;
                            if (currentblock != null)
                            {
                                if (currentblock is CascadingBlock cb)
                                {
                                    if (currentblock is LineSeriesBlock lsb)
                                    {
                                        isPopping = lsb.Popping;  //blocks that are popping shouldn't be resurrected.
                                    }
                                    if (!cb.IsSupported(cb.Owner, state.PlayField) && !ResurrectNominos.Contains(cb.Owner) && !AddedBlockAlready.Contains(cb))
                                    {
                                        //resurrect this block and other blocks that are in the same nomino.
                                        //since we remove busted blocks from the nomino, we can take the Duomino this
                                        //block belongs to and add it back to the Active Groups, then remove all the blocks that are in the nomino from the field.
                                        foreach (var iterate in cb.Owner)
                                        {
                                            var useX = iterate.X + cb.Owner.X;
                                            var useY = iterate.Y + cb.Owner.Y;
                                            state.PlayField.Contents[useY][useX] = null;
                                        }

                                        Nomino resurrect = cb.Owner;
                                        resurrect.Controllable = false;
                                        resurrect.FallSpeed = 250;
                                        resurrect.LastFall = pOwner.GetElapsedTime();
                                        resurrect.MoveSound = true;
                                        resurrect.PlaceSound = false;
                                        resurrect.NoGhost = true;
                                        ResurrectNominos.Add(resurrect);
                                        AddedBlockAlready.Add(cb);
                                        //state.PlayField.AddBlockGroup(resurrect);
                                    }
                                }

                                //now recursively process for the block to our left, the block to our right, and the block above. But only if that block is not part of the same nomino as currentblock or currentblock is null.


                            }

                        }

                    }

                    if (ResurrectNominos.Any())
                    {
                        HashSet<Point> AddedPoints = new HashSet<Point>();
                        foreach (var addresurrected in ResurrectNominos)
                        {
                            List<Point> AllPoints = (from b in addresurrected select new Point(b.X + addresurrected.X, b.Y + addresurrected.Y)).ToList();

                            if (!AllPoints.Any((w) => AddedPoints.Contains(w)))
                            {
                                state.PlayField.AddBlockGroup(addresurrected);
                                foreach(var point in AllPoints)
                                {
                                    AddedPoints.Add(point);
                                }
                            }
                        }
                    }


                    originalstate.NoTetrominoSpawn = false;
                    originalstate.PlayField.HasChanged = true;
                    originalstate.SuspendFieldSet = true;
                    //if we determined the level was completed earlier, 
                    //we need to switch to the level completion state, and from there will need to resume starting with that new level.
                    if (LevelCompleted)
                    {
                        LevelCompleted = false;
                        var completionState = new DrMarioLevelCompleteState(state, () => SetupNextLevel(state, pOwner));
                        owner.CurrentState = completionState;
                    }
                    else
                    {
                        owner.CurrentState = originalstate;
                    }
                });
                pOwner.CurrentState = tpause;


            }


            if (LevelCompleted)
            {
                LevelCompleted = false;
                var completionState = new DrMarioLevelCompleteState(state, () => SetupNextLevel(state, pOwner));
                pOwner.CurrentState = completionState;
            }

            //Remove those blocks from the field.
            //then, reprocess the field: find any unsupported blocks, and generate new ActiveBlockGroups for them. Add them to the list of active block groups. Set the fallspeed appropriately.
            //if we found any unsupported blocks groups, change the state to the GroupFallState (not defined) which is a composite state that doesn't allow input, and waits for all active block groups to come to rest before
            //continuing.

            //once all block groups come to rest, ProcessFieldChange will be called again.

            //Note: for visual flair eventually we'll want to have a temporary state which does nothing but allow the blocks being destroyed to be indicated for perhaps 250ms, before advancing to the state where blocks
            //will fall



            return new FieldChangeResult() { ScoreResult = 5 };

        }
        const int ParticlesPerPop = 400;
        static BCColor[] RedColors = new BCColor[] { SKColors.Red, SKColors.IndianRed, SKColors.OrangeRed, SKColors.DarkRed };
        static BCColor[] BlueColors = new BCColor[] { SKColors.Blue, SKColors.Navy, SKColors.SkyBlue, SKColors.LightBlue };
        static BCColor[] YellowColors = new BCColor[] { SKColors.Yellow, SKColors.LightYellow, SKColors.LightGoldenrodYellow, SKColors.Goldenrod, SKColors.DarkGoldenrod };
        static BCPoint[] CardinalOptions = new BCPoint[] { new BCPoint(1, 0), new BCPoint(0, 1) };
        const float MAX_SPEED = 0.25f;
        const float MIN_SPEED = 0.35f;
        //GeneratePopParticles(pOwner, state, iterate);
        private void GeneratePopParticles(IStateOwner pOwner,GameplayGameState gstate,SKPointI pt)
        {
            var rgen = TetrisGame.rgen;
            var popItem = gstate.PlayField.Contents[pt.Y][pt.X];
            BCColor[] useColor = YellowColors;
            if(popItem is LineSeriesBlock lsb)
            {
                switch(lsb.CombiningIndex)
                {
                    case LineSeriesBlock.CombiningTypes.Red:
                        useColor = RedColors;
                        break;
                    case LineSeriesBlock.CombiningTypes.Blue:
                        useColor = BlueColors;
                        break;
                    case LineSeriesBlock.CombiningTypes.Yellow:
                        useColor = YellowColors;
                        break;
                }
                for(int i=0;i<ParticlesPerPop;i++)
                {
                    PointF Offset = new PointF((float)rgen.NextDouble(),(float)rgen.NextDouble());
                    BCColor selColor = TetrisGame.Choose(useColor);
                    BCPoint Velocity = TetrisGame.Choose(CardinalOptions);
                    float Speed = (float)rgen.NextDouble() * (MAX_SPEED-MIN_SPEED) + MIN_SPEED;
                    float Sign = TetrisGame.Choose(new float[] { -1f, 1f });

                    BCPoint VelocityUse = new BCPoint(Velocity.X * Speed * Sign, Velocity.Y * Speed * Sign);

                    BaseParticle bp = new BaseParticle(new BCPoint(pt.X + Offset.X, pt.Y + Offset.Y), VelocityUse, selColor);
                    gstate.Particles.Add(bp);
                    
                }


            }

            /*for (int i=0;i<ParticlesPerPop;i++)
            {

            }*/


        }
        /// <summary>
        /// mutates the given GameplayGameState to prepare it for a new level, and returns it to the caller.
        /// </summary>
        /// <param name="mutate"></param>
        /// <returns></returns>
        public GameState SetupNextLevel(GameplayGameState mutate,IStateOwner pOwner)
        {
            Level++;
            PrepareField(mutate, pOwner);
            ViriiAppearanceState levelstarter = new ViriiAppearanceState(mutate);
            return levelstarter;
        }
        public TetrominoTheme DefaultTheme { get { return new DrMarioTheme(); } }
        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            //likely will need to have stats and stuff abstracted to each Handler.
            state.PlayField.Reset();
            
            HashSet<SKPointI> usedPositions = new HashSet<SKPointI>();
            //virus count is based on out level.
            int numViruses = (int)((Level * 1.25f) + 4);
            for(int i=0;i<numViruses;i++)
            {
                //choose a random virus type.
                var chosentype = TetrisGame.Choose(new LineSeriesBlock.CombiningTypes[] { LineSeriesBlock.CombiningTypes.Yellow, LineSeriesBlock.CombiningTypes.Red, LineSeriesBlock.CombiningTypes.Blue });
                LineSeriesMasterBlock lsmb = new LineSeriesMasterBlock() { CombiningIndex = chosentype };
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, lsmb);
                state.PlayField.Theme.ApplyTheme(Dummino,this, state.PlayField);
                lsmb.CriticalMass = 4;
                
                int RandomXPos = TetrisGame.rgen.Next(state.PlayField.ColCount);
                int RandomYPos = state.PlayField.RowCount - 1 -TetrisGame.rgen.Next(state.PlayField.RowCount / 2);
                SKPointI randomPos = new SKPointI(RandomXPos, RandomYPos);
                while(usedPositions.Contains(randomPos))
                {
                    int rndXPos = TetrisGame.rgen.Next(state.PlayField.ColCount);
                    int rndYPos = state.PlayField.RowCount - 1 - TetrisGame.rgen.Next(state.PlayField.RowCount / 2);
                     randomPos = new SKPointI(rndXPos, rndYPos);
                }
                state.PlayField.Contents[RandomYPos][RandomXPos] = lsmb;
                VirusCount++;



            }
            ViriiAppearanceState appearstate = new ViriiAppearanceState(state);
            pOwner.CurrentState = appearstate;


        }

        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            return null;
        }
    }
}
