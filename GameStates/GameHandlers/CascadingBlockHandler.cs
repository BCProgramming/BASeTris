﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates.GameHandlers.HandlerOptions;
using BASeTris.GameStates.GameHandlers.HandlerStates;
using BASeTris.Particles;
using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using static BASeTris.GameState;

namespace BASeTris.GameStates.GameHandlers
{
    public abstract class CascadingPopBlockGameHandler<STATT, OPTT> : IBlockGameCustomizationHandler, IExtendedGameCustomizationHandler
         where STATT : BaseStatistics, new()
         where OPTT : GameOptions, new()
    {
        [Flags]
        public enum ClearOrientationConstants
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Diagonal = 4
        }
        public ClearOrientationConstants ClearOrientations = ClearOrientationConstants.Horizontal | ClearOrientationConstants.Vertical;
        public String Name { get { return GetName(); } }
        public abstract String GetName();

        public bool LevelCompleteWhenMasterCountZero = true;
        public STATT Statistics { get; set; } = new STATT();
        BaseStatistics IBlockGameCustomizationHandler.Statistics { get { return this.Statistics; } set { this.Statistics = (STATT)value; } }
        public bool AllowFieldImageCache { get { return false; } }
        public OPTT GameOptions { get; } = new OPTT() { AllowWallKicks = false };

        public abstract BlockGroupChooser GetChooser(IStateOwner pOwner);
        public abstract void HandleLevelComplete(IStateOwner pOwner, GameplayGameState state);

        GamePreparerOptions IBlockGameCustomizationHandler.PrepInstance { get { return this.PrepInstance; } set { if(value is CascadingBlockPreparer cbp) this.PrepInstance = cbp; } }
        public CascadingBlockPreparer PrepInstance { get; set; }
        protected CascadingPopBlockGameHandler(CascadingBlockPreparer cbp)
        {
            PrepInstance = cbp;
            Level = (int)cbp.StartingLevel;
            switch ((int)cbp.TypeCount)
            {
                case 2:
                    AllowedSpawns = AllowedSpawnsFlags.Spawn_Red | AllowedSpawnsFlags.Spawn_Yellow;

                    break;
                case 3:
                    AllowedSpawns = AllowedSpawnsFlags.Spawn_Standard;
                    break;
                case 4:
                    AllowedSpawns = AllowedSpawnsFlags.Spawn_Standard | AllowedSpawnsFlags.Spawn_Orange;
                    break;
                case 5:
                    AllowedSpawns = AllowedSpawnsFlags.Spawn_Standard | AllowedSpawnsFlags.Spawn_Orange | AllowedSpawnsFlags.Spawn_Magenta;
                    break;
                case 6:
                    AllowedSpawns = AllowedSpawnsFlags.Spawn_Full;
                    break;
            }
        }
        protected CascadingPopBlockGameHandler()
        {
        }
        public virtual FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows = PrepInstance!=null?(int)PrepInstance.RowCount : TetrisField.DEFAULT_ROWCOUNT,
                BottomHiddenFieldRows = 0,
                TopHiddenFieldRows = 2,
                FieldColumns = PrepInstance!=null?(int)PrepInstance.ColumnCount:TetrisField.DEFAULT_COLCOUNT
            };
        }


       
        
        public abstract GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner);
        
        public abstract Nomino[] GetNominos();
        public virtual LineSeriesBlock.CombiningTypes[] GetValidBlockCombiningTypes()
        {
            List<LineSeriesBlock.CombiningTypes> buildresult = new List<LineSeriesBlock.CombiningTypes>();
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Yellow_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Yellow);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Red_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Red);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Blue_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Blue);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Orange_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Orange);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Magenta_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Magenta);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Green_Block)) buildresult.Add(LineSeriesBlock.CombiningTypes.Green);

            return buildresult.ToArray();
        }

        public virtual LineSeriesBlock.CombiningTypes[] GetValidPrimaryCombiningTypes()
        {
            List<LineSeriesBlock.CombiningTypes> buildresult = new List<LineSeriesBlock.CombiningTypes>();
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Yellow_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Yellow);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Red_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Red);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Blue_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Blue);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Orange_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Orange);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Magenta_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Magenta);
            if (this.AllowedSpawns.HasFlag(AllowedSpawnsFlags.Spawn_Green_Primary)) buildresult.Add(LineSeriesBlock.CombiningTypes.Green);

            return buildresult.ToArray();
        }


    

        public abstract IBlockGameCustomizationHandler NewInstance();

        //finds critical mass excesses starting from the given position.
        //this basically only finds one specific "critical mass"
        private HashSet<Point> FindCriticalMassesHorizontal(GameplayGameState state, IStateOwner pOwner, Point StartPosition,out int MaxMass)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            MaxMass = 0;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
                int FirstCol = StartPosition.X;
                //first, we look to the left until we no longer find a matching block.

                for (int X = StartPosition.X - 1; X > 0; X--)
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
                int MaxCriticalHorz = 1;
                List<Point> Horizontals = new List<Point>();
                Horizontals.Add(StartPosition);
                for (int X = FirstCol; X < state.PlayField.ColCount; X++)
                {
                    var CheckPos = state.PlayField.Contents[StartPosition.Y][X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
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
                if (HorizontalMass > MaxMass) MaxMass = HorizontalMass;
                if (HorizontalMass > MaxCriticalHorz)
                {
                    foreach (var iterate in Horizontals)
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
        private HashSet<Point> FindCriticalMassesDiagonal(GameplayGameState state, IStateOwner pOwner, Point StartPosition, out int MaxMass)
        {
            Func<int,int,int> Subtract = (a, b) => a - b;
            Func<int, int, int> Add = (a, b) => a + b;
            HashSet<Point> FoundPoints = new HashSet<Point>();
            MaxMass = 0;
            foreach (var callfuncs in new (Func<int, int, int>, Func<int, int, int>)[] { (Subtract, Subtract),(Add,Subtract),(Subtract,Add),(Add,Add) })
            {
                var DoOffsetX = callfuncs.Item2;
                var DoOffsetY = callfuncs.Item1;
                
                var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
                if (OurPos == null) return new HashSet<Point>();

                else
                {
                    int FirstRow = StartPosition.Y;
                    int FirstCol = StartPosition.X;
                    int FirstOffset = -1;
                    //first, look up and to the left and find the "last" matching item.

                    for (int Offset = 1; FirstRow - Offset > 0 && FirstCol - Offset > 0; Offset++)
                    {
                        //int CheckRow = FirstRow - Offset;
                        //int CheckCol = FirstCol - Offset;
                        int CheckRow = DoOffsetY(FirstRow, Offset);
                        int CheckCol = DoOffsetX(FirstCol, Offset);
                        if (CheckCol < 0 || CheckCol > state.PlayField.ColCount - 1) break;
                        if (CheckRow < 0 || CheckRow > state.PlayField.RowCount - 1) break;
                        var CheckPos = state.PlayField.Contents[CheckRow][CheckCol] as LineSeriesBlock;
                        if (CheckPos == null) break;
                        if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                            FirstOffset = Offset;
                        else
                            break;

                    }

                    //now, we do the same operation in "reverse" but starting from the offset we found.
                    int DiagonalMass = 0;
                    int MaxCriticalDiag = 1;
                    List<Point> Diagonals = new List<Point>();
                    Diagonals.Add(StartPosition);

                    //starting from firstoffset, we will subtract until we find a non lineseriesblock or a mismatch or are outside the board.
                    int CurrOffset = FirstOffset;
                    while (true)
                    {
                        //retrieve current position.
                        int CurrCol = DoOffsetX(FirstCol, CurrOffset);
                        int CurrRow = DoOffsetY(FirstRow, CurrOffset);
                        //if outside bounds of play field, we are finished.
                        if (CurrCol < 0 || CurrCol > state.PlayField.ColCount - 1) break;
                        if (CurrRow < 0 || CurrRow > state.PlayField.RowCount - 1) break;
                        var checkPos = state.PlayField.Contents[CurrRow][CurrCol] as LineSeriesBlock;
                        if (checkPos == null) break;
                        if (checkPos.CombiningIndex == OurPos.CombiningIndex)
                        {
                            MaxCriticalDiag = Math.Max(MaxCriticalDiag, checkPos.CriticalMass);
                            Diagonals.Add(new Point(CurrCol, CurrRow));
                            DiagonalMass++;
                        }
                        else
                        {
                            break;
                        }
                        CurrOffset--;

                    }
                    if (DiagonalMass > MaxMass) MaxMass = DiagonalMass;
                    if (DiagonalMass >= MaxCriticalDiag)
                    {
                        foreach (var iterate in Diagonals)
                        {
                            if (!FoundPoints.Contains(iterate))
                                FoundPoints.Add(iterate);
                        }
                    }

                }
              
            }
            return FoundPoints;
        }
        private HashSet<Point> FindCriticalMassesVertical(GameplayGameState state, IStateOwner pOwner, Point StartPosition,out int MaxMass)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            MaxMass = 0;
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
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
                int MaxCriticalVert = 1;
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
                if (VerticalMass > MaxMass) MaxMass = VerticalMass;
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
        private HashSet<Point> FindCriticalMasses(GameplayGameState state, IStateOwner pOwner, Point StartPosition,out int MaxMass)
        {
            //if we are below the visible area, than ignore this call, blocks that are hidden cannot participate in critical masses.
            //if (StartPosition.Y > state.PlayField.ColCount - state.PlayField.HIDDENROWS_BOTTOM) return new HashSet<Point>(); 
            bool useHorz = ClearOrientations.HasFlag(ClearOrientationConstants.Horizontal);
            bool useVert = ClearOrientations.HasFlag(ClearOrientationConstants.Vertical);
            bool useDiag = ClearOrientations.HasFlag(ClearOrientationConstants.Diagonal);
            int MaxMassHorizontal = 0, MaxMassVertical = 0, MaxMassDiagonal = 0;
            HashSet<Point> Horizontal = useHorz ? FindCriticalMassesHorizontal(state, pOwner, StartPosition,out MaxMassHorizontal) : new HashSet<Point>();
            HashSet<Point> Vertical = useVert? FindCriticalMassesVertical(state, pOwner, StartPosition,out MaxMassVertical) : new HashSet<Point>();
            HashSet<Point> Diagonal = useDiag? FindCriticalMassesDiagonal(state, pOwner, StartPosition,out MaxMassDiagonal) : new HashSet<Point>();

            MaxMass = Math.Max(MaxMassDiagonal, Math.Max(MaxMassHorizontal, MaxMassVertical));
            foreach (Point verticalpoint in Vertical)
            {
                if (!Horizontal.Contains(verticalpoint)) Horizontal.Add(verticalpoint);
            }
            foreach (Point diagpoint in Diagonal)
                if (!Horizontal.Contains(diagpoint)) Horizontal.Add(diagpoint);

            return Horizontal;

        }
        
        public List<Nomino> ProcessBlockDroppage(GameplayGameState state, int Column, int Row, ref HashSet<Nomino> AdditionalSkipBlocks)
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
                    if (!isPopping && !cb.IsSupported(cb.Owner,Row,Column, state.PlayField, new HashSet<CascadingBlock>(new[] { cb })) && !AdditionalSkipBlocks.Contains(cb.Owner))
                    {
                        //we initialize the list of recursion blocks to the block we are testing, since it cannot support itself.
                        //resurrect this block and other blocks that are in the same nomino.
                        //since we remove busted blocks from the nomino, we can take the Duomino this
                        //block belongs to and add it back to the Active Groups, then remove all the blocks that are in the nomino from the field.

                        if (true && cb.Owner is Duomino.Duomino)
                        {

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
                        else
                        {
                            //can't do the same thing with other types, unfortunately- if you pop a block in the middle you don't want
                            //both sides to be part of the same nomino as they fall.
                        }

                    }
                }

                //now recursively process for the block to our left, the block to our right, and the block above. But only if that block is not part of the same nomino as currentblock or currentblock is null.

                foreach (Point offset in new Point[] { new Point(-1, 0), new Point(0, -1), new Point(1, 0) })
                {
                    var checkblock = state.PlayField.Contents[Row + offset.Y][Column + offset.X];
                    if (checkblock != null && (currentblock == null || !currentblock.Owner.HasBlock(checkblock)))
                    {
                        List<Nomino> CurrResult = ProcessBlockDroppage(state, Column + offset.X, Row + offset.Y, ref AdditionalSkipBlocks);
                        foreach (var iterateresult in CurrResult)
                        {
                            CreateResult.Add(iterateresult);
                        }
                    }

                }

            }


            return CreateResult;
        }
        public TimeSpan LastPopComplete = TimeSpan.Zero;
        public int CriticalMassToPopAllOfSameColor = 5;
        public int Level { get; set; } = 0;
        public int PrimaryBlockCount = 0;
        protected bool IgnoreActiveGroupsForFieldChange = false;
        protected bool ProcessWithoutMasses = false;
        protected bool NoFallInputDelay = false;
        public virtual FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner,Nomino Trigger)
        {
            FieldChangeResult fcr = new FieldChangeResult();
            if (!IgnoreActiveGroupsForFieldChange && state.PlayField.GetActiveBlockGroups().Count() > 0) return new FieldChangeResult() { ScoreResult = 0 };
            //here we would go through the field and handle where the blocks line up to more than the required critical mass. 
            Dictionary<LineSeriesBlock.CombiningTypes, int> MaxMassesByType = new Dictionary<LineSeriesBlock.CombiningTypes, int>();
            //Nomino's have two blocks- usually. But, we should account for more. This handler may be expanded for the Tetris2 handler, (if we ever bother to make one)
            //in any case we want to check all the positions of the trigger nomino and check for critical masses.
            int MasterCount = 0;
            bool LevelCompleted = false;
            HashSet<Point> CriticalMasses = null;
            for (int y = 0; y < state.PlayField.RowCount; y++)
            {
                var currRow = state.PlayField.Contents[y];
                for (int x = 0; x < state.PlayField.ColCount; x++)
                {
                    if (state.PlayField.Contents[y][x] is LineSeriesPrimaryBlock)
                    {
                        MasterCount++;
                    }
                    if (state.PlayField.Contents[y][x] is LineSeriesBlock lsb)
                    {
                        int GetMaximum = 0;
                        var foundmasses = FindCriticalMasses(state, pOwner, new Point(x, y),out GetMaximum);
                        if (!MaxMassesByType.ContainsKey(lsb.CombiningIndex))
                            MaxMassesByType.Add(lsb.CombiningIndex, GetMaximum);
                        else
                        {
                            if (MaxMassesByType[lsb.CombiningIndex] < GetMaximum)
                                MaxMassesByType[lsb.CombiningIndex] = GetMaximum;
                        }
                        foreach (var iterate in foundmasses)
                        {
                            if (CriticalMasses == null) CriticalMasses = new HashSet<Point>(foundmasses);
                            else if (!CriticalMasses.Contains(iterate)) CriticalMasses.Add(iterate);
                        }
                    }

                }
            }
            PrimaryBlockCount = MasterCount;

            //if MasterCount is 0 then we completed this level.
            //if there are no primary blocks left, this level is now complete. We need a "Level complete" screen state with an overlay- we would switch to that state. It should
            //operate similar to the TemporaryInputPauseGameState in that we provide a routine to be called after the user opts to press a button to continue.
            if (MasterCount == 0 && LevelCompleteWhenMasterCountZero)
            {
                LevelCompleted = true;
            }

            if ((CriticalMasses != null && CriticalMasses.Any()) || ProcessWithoutMasses)
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
                var originalstate = state;
               
                int MaxCombo = 0;
                HashSet<Point> ShinyPopped = new HashSet<Point>();
                if (CriticalMasses != null)
                {
                    var PopRegulars = new HashSet<LineSeriesBlock.CombiningTypes>(MaxMassesByType.Where((d) => d.Value > CriticalMassToPopAllOfSameColor).Select((k)=>k.Key));
                    if (PopRegulars.Any())
                    {
                        for (int r = 0; r < state.PlayField.Contents.Length; r++)
                        {
                            var row = state.PlayField.Contents[r];
                            for (int c = 0; c < row.Length; c++)
                            {
                                var cell = row[c];
                                if (cell is LineSeriesBlock lsba && !lsba.Popping && !CriticalMasses.Contains(new Point(r,c)))
                                {
                                    if (!(cell is LineSeriesPrimaryBlock))
                                    {
                                        if (PopRegulars.Contains(lsba.CombiningIndex))
                                        {
                                            ProcessPopBlock(state, pOwner, ref MaxCombo, new Point(c, r), lsba, lsba);
                                            fcr.ScoreResult += 5;
                                            ShinyPopped.Add(new Point(c, r));
                                        }
                                    }

                                }
                            }
                        }
                    }



                    HashSet<Nomino> MassNominoes = new HashSet<Nomino>();
                    
                    foreach (var iterate in CriticalMasses)
                    {
                        fcr.BlocksAffected++;
                        var popItem = state.PlayField.Contents[iterate.Y][iterate.X];

                        if (popItem is LineSeriesBlock lsb)
                        {
                            MaxCombo = ProcessPopBlock(state, pOwner,ref MaxCombo, iterate, popItem, lsb);

                        }
                        if (popItem is LineSeriesPrimaryShinyBlock lsbp)
                        {
                            var FoundShinyPop = false;
                            //go through all cells.
                            for (int r = 0; r < state.PlayField.Contents.Length; r++)
                            {
                                var row = state.PlayField.Contents[r];
                                for (int c = 0; c<row.Length; c++)
                                {
                                    var cell = row[c];



                                    if (cell is LineSeriesPrimaryBlock lsb2 && !lsb2.Popping)
                                    {
                                        if (lsb2.CombiningIndex == lsbp.CombiningIndex)
                                        {
                                            ProcessPopBlock(state, pOwner, ref MaxCombo, new Point(c, r), lsb2, lsb2);
                                            ShinyPopped.Add(new Point(c, r));
                                            fcr.ScoreResult += 10;
                                            FoundShinyPop = true;

                                        }
                                    }
                                    else if (cell is LineSeriesBlock lsba && !lsba.Popping)
                                    {
                                        if (PopRegulars.Contains(lsba.CombiningIndex))
                                        {
                                            ProcessPopBlock(state, pOwner, ref MaxCombo, new Point(c, r), lsba, lsba);
                                            fcr.ScoreResult += 5;
                                            ShinyPopped.Add(new Point(c, r));
                                        }
                                            
                                    }
                                }
                             
                            }

                            if(FoundShinyPop)
                                TetrisGame.Soundman.PlaySound("shiny_clear_tetris_2");
                        }
                        if (popItem.Owner != null)
                            popItem.Owner.RemoveBlock(popItem);
                        state.PlayField.HasChanged = true;
                    }

                    state.Sounds.PlaySound(pOwner.AudioThemeMan.BlockPop.Key);

                }


                //need to determine a way to detect chains here, where we create an active block and then it results in another "pop".



                TemporaryInputPauseGameState tpause = new TemporaryInputPauseGameState(state, (NoFallInputDelay || CriticalMasses==null)?0:1000, (owner) =>
                {
                    //first, remove the CriticalMasses altogether.

                    if (CriticalMasses != null) foreach (var iterate in CriticalMasses.Concat(ShinyPopped))
                    {
                            
                        //clear out the cell at the appropriate position.
                        var popItem = state.PlayField.Contents[iterate.Y][iterate.X];
                            if (popItem == null) continue;
                            if (popItem is LineSeriesBlock lsbb)
                            {
                                fcr.ScoreResult += (5 * Math.Min(1,lsbb.ComboTracker));
                            }
                        state.PlayField.Contents[iterate.Y][iterate.X] = null;
                        //now apply the theme to the specified location
                        if (popItem.Owner != null)
                            state.PlayField.Theme.ApplyTheme(popItem.Owner, this, state.PlayField, NominoTheme.ThemeApplicationReason.Normal);
                        else
                        {
                            //create a "dummy" nomino for the application of the "pop" theme animation.
                            var Dummino = new Nomino() { };
                            Dummino.AddBlock(new Point[] { new Point(0, 0) }, popItem);
                            state.PlayField.Theme.ApplyTheme(Dummino, this, state.PlayField, NominoTheme.ThemeApplicationReason.Normal);
                        }


                    }
                    //algorithm change: instead of going through the entire field, we'll go through all the critical masses.
                    //With Each one:
                    //check the block to the left, to the right, and above.


                    //next, go through the entire field.

                    HashSet<Nomino> ResurrectNominos = null;
                    if (SimplePopHandling && false)
                    {
                        ResurrectNominos = ResurrectLoose(state, pOwner, MaxCombo + 1);
                        
                    }
                    else
                    {
                        
                        ResurrectNominos = ResurrectLoose_Ex(state, pOwner, MaxCombo + 1);
                        
                    }

                    if (ResurrectNominos.Any())
                    {
                        HashSet<Point> AddedPoints = new HashSet<Point>();
                        foreach (var addresurrected in ResurrectNominos)
                        {
                            List<Point> AllPoints = (from b in addresurrected select new Point(b.X + addresurrected.X, b.Y + addresurrected.Y)).ToList();

                            //verify that we removed it from the field...
                            foreach (var checkfield in AllPoints)
                            {
                                //if (addresurrected.Any((d) => d.Block == state.PlayField.Contents[checkfield.Y][checkfield.X]))
                                state.PlayField.Contents[checkfield.Y][checkfield.X] = null; //clear it out.
                            }


                            if (!AllPoints.Any((w) => AddedPoints.Contains(w)))
                            {
                                state.PlayField.Theme.ApplyTheme(addresurrected, state.GameHandler, state.PlayField, NominoTheme.ThemeApplicationReason.Normal);
                                state.PlayField.AddBlockGroup(addresurrected);
                                addresurrected.FallSpeed = 100;
                                foreach (var point in AllPoints)
                                {
                                    AddedPoints.Add(point);
                                }
                            }
                        }
                        //additionally: we actually need to go through and apply the theme to all the blocks on the field, too.
                    }


                    originalstate.NoTetrominoSpawn = false;
                    originalstate.PlayField.HasChanged = true;
                    originalstate.SuspendFieldSet = true;
                    //if we determined the level was completed earlier, 
                    //we need to switch to the level completion state, and from there will need to resume starting with that new level.
                    if (LevelCompleted)
                    {

                        LevelCompleted = false;
                        HandleLevelComplete(owner, state);

                    }
                    else
                    {
                        owner.CurrentState = originalstate;
                    }
                });
                if (NoFallInputDelay) tpause.FilterFunction = TemporaryInputPauseGameState.CreateKeyFilter(GameKeys.GameKey_Down, GameKeys.GameKey_Drop, GameKeys.GameKey_Left, GameKeys.GameKey_Right, GameKeys.GameKey_RotateCW, GameKeys.GameKey_RotateCCW);
                pOwner.CurrentState = tpause;


            }


            if (LevelCompleted)
            {
                LevelCompleted = false;
                var completionState = new PrimaryBlockLevelCompleteState(state, () => SetupNextLevel(state, pOwner));
                pOwner.CurrentState = completionState;
            }

            //Remove those blocks from the field.
            //then, reprocess the field: find any unsupported blocks, and generate new ActiveBlockGroups for them. Add them to the list of active block groups. Set the fallspeed appropriately.
            //if we found any unsupported blocks groups, change the state to the GroupFallState (not defined) which is a composite state that doesn't allow input, and waits for all active block groups to come to rest before
            //continuing.

            //once all block groups come to rest, ProcessFieldChange will be called again.

            //Note: for visual flair eventually we'll want to have a temporary state which does nothing but allow the blocks being destroyed to be indicated for perhaps 250ms, before advancing to the state where blocks
            //will fall
            state.PlayField.VerifySingularBlocks();

            fcr.ScoreResult = 5;
            return fcr;

        }

        private int ProcessPopBlock(GameplayGameState state, IStateOwner pOwner, ref int MaxCombo, Point iterate, NominoBlock popItem, LineSeriesBlock lsb)
        {
            if (lsb.ComboTracker > MaxCombo) MaxCombo = lsb.ComboTracker;
            lsb.Popping = true;
            GeneratePopParticles(pOwner, state, new SKPointI(iterate.X, iterate.Y));
            if (popItem.Owner != null)
                state.PlayField.Theme.ApplyTheme(popItem.Owner, this, state.PlayField, NominoTheme.ThemeApplicationReason.Normal);
            else
            {
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, popItem);
                state.PlayField.Theme.ApplyTheme(Dummino, this, state.PlayField, NominoTheme.ThemeApplicationReason.Normal);
            }

            return MaxCombo;
        }

        private HashSet<Nomino> ResurrectLoose_Ex(GameplayGameState state, IStateOwner pOwner, int Combo)
        {
            //new version of resurrectLoose.

            //1. Iterate through all cells
            //2. if the block in the cell is not supported, create a new Nomino
            //3. To the new nomino, add the block in the cell. Check adjacent cells recursively to find blocks that are part of the same nomino and have the same group index. 
            //  3.(a). The new nomino's position should be the "center" of the square of the size that is the largest of the x or y width of the resurrected nominoes.
            //  3.(b). And the added blocks that are 'resurrected' will of course need appropriate offsets such that when the Nomino is added to the active groups all those blocks "spawn" in the same position as the blocks to which they were based in the field.
            //4. empty out the cells that have been resurrected as part of the new nomino.
            HashSet<NominoBlock> CheckedBlocks = new HashSet<NominoBlock>();
            HashSet<Nomino> ResurrectNominoes = new HashSet<Nomino>();

            for (int row = 0; row < state.PlayField.RowCount; row++)
            {
                for (int column = 0; column < state.PlayField.ColCount; column++)
                {
                    var currentblock = state.PlayField.Contents[row][column];
                    bool IsPopping = false;
                    if (currentblock != null)
                    {
                        if (currentblock is CascadingBlock cb)
                        {
                            if (CheckedBlocks.Contains(currentblock)) continue; //already processed.
                            CheckedBlocks.Add(currentblock);
                            if (currentblock is LineSeriesBlock lsb) IsPopping = lsb.Popping;
                            var BlockSupported = cb.IsSupported(cb.Owner, row, column, state.PlayField);
                            if (!IsPopping && !BlockSupported)
                            {
                                //we want to resurrect this nomino. Or, to be more precise, we want to resurrect all blocks adjacent to this one that were part of the same nomino and have the same group index.
                                var FindElements = GetAdjacent(row, column, state, pOwner, (nb) =>
                                {
                                    if (nb is CascadingBlock cbb)
                                    {
                                        bool Result = false;
                                        if (cbb is LineSeriesBlock lsb)
                                        {
                                            return !lsb.Popping && (cbb.Owner == cb.Owner && cbb.ConnectionIndex == cb.ConnectionIndex);
                                        }
                                        else
                                        {
                                            return (cbb.Owner == cb.Owner && cbb.ConnectionIndex == cb.ConnectionIndex);
                                        }
                                    }

                                    return false;
                                });


                                //create a new Nomino out of currentblock and the blocks in FindElements.
                                List<AdjacentResultInfo> NewNominoContents = FindElements.Concat(new[] { new AdjacentResultInfo(cb, row, column) }).ToList();
                                //add the blocks in the results to the list of blocks we've already processed, so we don't try recreating Nominoes multiple times.
                                foreach (var addcheck in NewNominoContents)
                                {
                                    if (!CheckedBlocks.Contains(addcheck.block))
                                        CheckedBlocks.Add(addcheck.block);

                                }
                                //We need to find the extents of These Nominos, so we need the minimum and maximum X and Y values.
                                int MinX = int.MaxValue, MinY = int.MaxValue, MaxX = int.MinValue, MaxY = int.MinValue;
                                foreach (var findextent in NewNominoContents)
                                {
                                    if (findextent.Column < MinX) MinX = findextent.Column;
                                    if (findextent.Column > MaxX) MaxX = findextent.Column;
                                    if (findextent.Row < MinY) MinY = findextent.Row;
                                    if (findextent.Row > MaxY) MaxY = findextent.Row;

                                }
                                //determine width and height to be used. This is used to evaluate the "center point" of the newly constructed nomino.
                                int NominoWidth = MaxX - MinX;
                                int NominoHeight = MaxY - MinY;
                                //round up to the next multiple of 2, so the center point is an integer.
                                NominoWidth += NominoWidth % 2;
                                NominoHeight += NominoHeight % 2;

                                int CenterPointX = MinX + NominoWidth / 2;
                                int CenterPointY = MinY + NominoHeight / 2;
                                List<NominoElement> newElements = new List<NominoElement>();
                                foreach (var useresult in NewNominoContents)
                                {
                                    useresult.block.Rotation = 0;
                                    newElements.Add(new NominoElement(new Point(useresult.Column - CenterPointX, useresult.Row - CenterPointY), new Size(NominoWidth, NominoHeight), useresult.block));
                                }
                                foreach (var removeoriginal in NewNominoContents)
                                {
                                    removeoriginal.block.Owner.RemoveBlock(removeoriginal.block);
                                    
                                }
                                //construct the new Nomino.
                                //Now, we can go through the NewNominoContents
                                Nomino BuildResult = new Nomino(newElements);
                                foreach (var iterate in BuildResult) iterate.Block.Owner = BuildResult;
                                BuildResult.X = CenterPointX;
                                BuildResult.Y = CenterPointY;
                                ResurrectNominoes.Add(BuildResult);

                            }
                        }
                    }
                }
            }

            

            

            return ResurrectNominoes;
        }




        private class AdjacentResultInfo
        {
            public NominoBlock block { get; set; }
            public int Column { get; set; }
            public int Row { get; set; }
            public AdjacentResultInfo(NominoBlock pBlock, int pRow, int pCol)
            {
                block = pBlock;
                Column = pCol;
                Row = pRow;
            }
        }
        //retrieve all blocks adjacent to the one provided, and adjacent to that one, and so on, that pass the test function.

        private IEnumerable<AdjacentResultInfo> GetAdjacent(int Row,int Column,GameplayGameState state, IStateOwner pOwner, Predicate<NominoBlock> TestFunction,HashSet<Point> RecurseList= null)
        {
            if (RecurseList == null) RecurseList = new HashSet<Point>();
            if (RecurseList.Contains(new Point(Column, Row))) yield break;
            RecurseList.Add(new Point(Column, Row));
            Point[] checkoffsets = new Point[]{
                new(){X=-1,Y=0 },
                new(){X=1,Y=0 },
                new(){X=0,Y=1 },
                new(){X=0,Y=-1 }
            };
            foreach (var pt in checkoffsets)
            {
                int CheckRow = Row + pt.Y;
                int CheckColumn = Column + pt.X;
                if (CheckRow < 0 || CheckColumn < 0 || CheckRow > state.PlayField.RowCount - 1 || CheckColumn > state.PlayField.ColCount - 1) continue;
                if (RecurseList.Contains(new Point(CheckColumn, CheckRow))) continue;
                NominoBlock nb = state.PlayField.Contents[CheckRow][CheckColumn];
                if (nb != null && TestFunction(nb))
                {
                    yield return new AdjacentResultInfo(nb,CheckRow,CheckColumn);
                    foreach (var iterate in GetAdjacent(CheckRow, CheckColumn, state, pOwner, TestFunction, RecurseList))
                    {
                        yield return iterate;
                    }
                }
            }
        }
        protected virtual HashSet<Nomino> ResurrectLoose(GameplayGameState state,IStateOwner pOwner,int Combo)
        {
            List<NominoBlock> CheckedBlocks = new List<NominoBlock>();

            HashSet<CascadingBlock> AddedBlockAlready = new HashSet<CascadingBlock>();
            //working list of the Nominoes that will be added to the Game Field.
            HashSet<Nomino> ResurrectNominos = new HashSet<Nomino>();
            //keep track of the blocks we've examined already.
            for (int row = 0; row < state.PlayField.RowCount; row++)
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
                            bool BlockSupported;

                            BlockSupported = cb.IsSupported(cb.Owner, row, column, state.PlayField);


                            if (!isPopping && !BlockSupported && !ResurrectNominos.Contains(cb.Owner) && !AddedBlockAlready.Contains(cb))
                            {
                                //this needs to be updated to support resurrection of multiple Nominoes for each ConnectionIndex 
                                //Algorithm would create a new Nomino for each unique connection index, then add
                                //the Blocks that are still "alive" with that connection index to the new Nomino, and finally add the resurrected Nominoes to the list.
                                //It might be wise to somehow track that they were originally a different Nomino somehow...

                              
                                    //resurrect this block and other blocks that are in the same nomino. 
                                    //since we remove busted blocks from the nomino, we can take the Duomino this
                                    //block belongs to and add it back to the Active Groups, then remove all the blocks that are in the nomino from the field.
                                    if (cb.Owner == null)
                                    {
                                        //create a new owner.
                                        var Dummino = new Nomino() { };
                                        Dummino.AddBlock(new Point[] { new Point(0, 0) }, cb);
                                        Dummino.X = column;
                                        Dummino.Y = row;

                                        cb.Owner = Dummino;

                                    }
                                    foreach (var iterate in cb.Owner)
                                    {
                                        if (iterate.Block is LineSeriesBlock lsba)
                                        {
                                            lsba.ComboTracker = Combo;
                                        }
                                        var useX = iterate.X + cb.Owner.X;
                                        var useY = iterate.Y + cb.Owner.Y;
                                        state.PlayField.Contents[useY][useX] = null;
                                    }

                                    Nomino resurrect = cb.Owner;
                                    resurrect.Controllable = false;
                                    resurrect.FallSpeed = 100;
                                    resurrect.InitialY = row;
                                    resurrect.LastFall = pOwner.GetElapsedTime();
                                    resurrect.MoveSound = true;
                                    resurrect.PlaceSound = false;
                                    resurrect.NoGhost = true;
                                    ResurrectNominos.Add(resurrect);
                                    AddedBlockAlready.Add(cb);
                              
                            }
                            //state.PlayField.AddBlockGroup(resurrect);
                        }
                    }

                    //now recursively process for the block to our left, the block to our right, and the block above. But only if that block is not part of the same nomino as currentblock or currentblock is null.


                }

            }
            return ResurrectNominos;
        }
        protected bool SimplePopHandling = false;
        const int ParticlesPerPop = 400;
        static BCColor[] RedColors = new BCColor[] { SKColors.Red, SKColors.IndianRed, SKColors.OrangeRed, SKColors.DarkRed };
        static BCColor[] BlueColors = new BCColor[] { SKColors.Blue, SKColors.Navy, SKColors.SkyBlue, SKColors.LightBlue };
        static BCColor[] YellowColors = new BCColor[] { SKColors.Yellow, SKColors.LightYellow, SKColors.LightGoldenrodYellow, SKColors.Goldenrod, SKColors.DarkGoldenrod };
        static BCColor[] OrangeColors = new BCColor[] { SKColors.Orange, SKColors.OrangeRed, SKColors.DarkGoldenrod };
        static BCColor[] MagentaColors = new BCColor[] { SKColors.Magenta, SKColors.Purple, SKColors.Indigo, SKColors.MediumPurple };
        static BCColor[] GreenColors = new BCColor[] { SKColors.Green, SKColors.Olive, SKColors.OliveDrab, SKColors.YellowGreen };
        static BCPoint[] CardinalOptions = new BCPoint[] { new BCPoint(1, 0), new BCPoint(0, 1) };
        const float MAX_SPEED = 0.25f;
        const float MIN_SPEED = 0.35f;
        private Dictionary<LineSeriesBlock.CombiningTypes, BCColor[]> CombiningColorMap = new Dictionary<LineSeriesBlock.CombiningTypes, BCColor[]>()
        {
            {LineSeriesBlock.CombiningTypes.Red,RedColors },
            {LineSeriesBlock.CombiningTypes.Green,GreenColors },
            {LineSeriesBlock.CombiningTypes.Blue,BlueColors },
            {LineSeriesBlock.CombiningTypes.Yellow,YellowColors },
            {LineSeriesBlock.CombiningTypes.Orange,OrangeColors },
            {LineSeriesBlock.CombiningTypes.Magenta,MagentaColors }
            


        };
        //GeneratePopParticles(pOwner, state, iterate);
        public BCColor[] GetCombiningColor(LineSeriesBlock.CombiningTypes ptype)
        {
            if (CombiningColorMap.ContainsKey(ptype)) return CombiningColorMap[ptype];
            return RedColors;
            
        }
        private void GeneratePopParticles(IStateOwner pOwner, GameplayGameState gstate, SKPointI pt)
        {
            var rgen = TetrisGame.StatelessRandomizer;
            var popItem = gstate.PlayField.Contents[pt.Y][pt.X];
            BCColor[] useColor = YellowColors;
            if (popItem is LineSeriesBlock lsb)
            {
                useColor = GetCombiningColor(lsb.CombiningIndex);
                for (int i = 0; i < ParticlesPerPop; i++)
                {
                    PointF Offset = new PointF((float)rgen.NextDouble(), (float)rgen.NextDouble());
                    BCColor selColor = TetrisGame.Choose(useColor);
                    BCPoint Velocity = TetrisGame.Choose(CardinalOptions);
                    float Speed = (float)rgen.NextDouble() * (MAX_SPEED - MIN_SPEED) + MIN_SPEED;
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
        public GameState SetupNextLevel(GameplayGameState mutate, IStateOwner pOwner)
        {
            Level++;
            //PrepareField(mutate, pOwner);
            //the next field will be prepared later.
            mutate.PlayField.Reset();
            mutate.FirstRun = false;
            mutate.FieldPrepared = false;
            return mutate;
            //DrMarioVirusAppearanceState levelstarter = new DrMarioVirusAppearanceState(mutate);
            //return levelstarter;
        }
        [Flags]
        public enum AllowedSpawnsFlags
        {
            Spawn_Invalid = 0,
            Spawn_Red_Primary = 1,
            Spawn_Blue_Primary = 2,
            Spawn_Yellow_Primary = 4,
            Spawn_Red_Block = 8,
            Spawn_Blue_Block = 16,
            Spawn_Yellow_Block = 32,
            Spawn_Orange_Primary = 64,
            Spawn_Magenta_Primary = 128,
            Spawn_Green_Primary = 256,
            Spawn_Orange_Block = 512,
            Spawn_Magenta_Block = 1024,
            Spawn_Green_Block = 2048,
            Spawn_Standard = Spawn_Red_Primary | Spawn_Blue_Primary | Spawn_Yellow_Primary | Spawn_Red_Block | Spawn_Blue_Block | Spawn_Yellow_Block,
            Spawn_Alternate = Spawn_Orange_Primary | Spawn_Magenta_Primary | Spawn_Green_Primary | Spawn_Magenta_Block | Spawn_Green_Block | Spawn_Orange_Block | Spawn_Magenta_Block | Spawn_Green_Block,
            Spawn_4 = Spawn_Standard | Spawn_Orange_Primary | Spawn_Orange_Block,
            Spawn_5 = Spawn_4 | Spawn_Magenta_Primary | Spawn_Magenta_Block,
            Spawn_Full = Spawn_Standard | Spawn_Alternate,
            Spawn_Red = Spawn_Red_Primary | Spawn_Red_Block,
            Spawn_Blue = Spawn_Blue_Primary | Spawn_Blue_Block,
            Spawn_Yellow = Spawn_Yellow_Primary | Spawn_Yellow_Block,
            Spawn_Orange = Spawn_Orange_Primary | Spawn_Orange_Block,
            Spawn_Magenta = Spawn_Magenta_Primary | Spawn_Magenta_Block,
            Spawn_Green = Spawn_Green_Primary | Spawn_Green_Block
                


        }



        public AllowedSpawnsFlags AllowedSpawns { get; set; } = AllowedSpawnsFlags.Spawn_Standard;
        public virtual NominoTheme DefaultTheme { get { return new DrMarioTheme(); } }



        GameOptions IBlockGameCustomizationHandler.GameOptions => this.GameOptions;

        public virtual void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            //likely will need to have stats and stuff abstracted to each Handler.
            state.PlayField.Reset();

            LineSeriesGameFieldInitializerParameters _InitParams = new LineSeriesGameFieldInitializerParameters(Level,GetValidPrimaryCombiningTypes());

            LineSeriesGameFieldInitializer fieldinit = new LineSeriesGameFieldInitializer(this, _InitParams);
            fieldinit.Initialize(state.PlayField);
            PrimaryBlockCount = state.PlayField.AllContents().Count((y) => y != null);
            
            DrMarioVirusAppearanceState appearstate = new DrMarioVirusAppearanceState(state);
            pOwner.CurrentState = appearstate;


        }




        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            return null;
        }

        public virtual IHighScoreList GetHighScores()
        {
            return (IHighScoreList)TetrisGame.ScoreMan[this.Name];
            return null;
            //throw new NotImplementedException();
        }
        private IEnumerable<Particles.BaseParticle> AddParticles(IStateOwner pOwner,int Column, int Row, LineSeriesPrimaryBlock block,int ParticleCountBase = 10)
        {
            var sUseColors = GetCombiningColor(block.CombiningIndex);

            for (int i = 0; i < ParticleCountBase; i++)
            {
                float XPosition = (float)Column + (float)TetrisGame.StatelessRandomizer.NextDouble();
                float YPosition = (float)Row + (float)TetrisGame.StatelessRandomizer.NextDouble();

                var UseVelocity = new BCPoint(((float)TetrisGame.StatelessRandomizer.NextDouble() - 0.5f) / 80f, ((float)TetrisGame.StatelessRandomizer.NextDouble() - 0.5f) / 80f);
                var UsePosition = new BCPoint(XPosition, YPosition);
                ShapeParticle sp = new ShapeParticle(UsePosition, UseVelocity, sUseColors); //sUseColors
                sp.Decay = new BCPoint(1, 1);
                sp.Size = 1;
                sp.TTL = 500;
                yield return sp;
            }
            



        }
        int FrameCount = -1;
        public ExtendedCustomizationHandlerResult GameProc(GameplayGameState state, IStateOwner pOwner)
        {



            List<Func<IEnumerable<BaseParticle>>> Primaries = new List<Func<IEnumerable<BaseParticle>>>();
            FrameCount++;
            if (FrameCount > 50) FrameCount = 0;
            
            if (FrameCount >0) return ExtendedCustomizationHandlerResult.Default;
            
            

            //find all Primary Shiny Blocks.
            for (int r = 0; r < state.PlayField.Contents.Length; r++)
            {
                var Row = state.PlayField.Contents[r];
                for (int c = 0; c < Row.Length; c++)
                {
                    var Cell = Row[c];
                        if (Cell is LineSeriesPrimaryShinyBlock lspb)
                        {
                        var closecol = c;
                        var closerow = r;
                        Primaries.Add(() => AddParticles(pOwner,closecol, closerow - state.PlayField.HIDDENROWS_TOP, lspb));
                        }

                } 
            }

            //now, add particles!
            lock (state.TopParticles)
            {
                foreach (var particlefunc in Primaries)
                {
                    var addparticles = particlefunc();
                    state.TopParticles.AddRange(addparticles);
                }
            }


            return ExtendedCustomizationHandlerResult.Default;
            //throw new NotImplementedException();
        }

        public ExtendedCustomizationHandlerResult HandleGameKey(GameplayGameState state, IStateOwner pOwner, GameKeys g)
        {
            return ExtendedCustomizationHandlerResult.Default;
            //throw new NotImplementedException();
        }
    }
}
