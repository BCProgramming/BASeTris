using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
    [HandlerTipText("Tetris Attack, 6 Types")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerMenuCategory("Tetris Attack")]
    public class TetrisAttackExpandedHandler : TetrisAttackHandler
    {
        public override FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows =  20,
                BottomHiddenFieldRows = 1,
                TopHiddenFieldRows = 2,
                FieldColumns = 10
            };
        }
        public override string GetName()
        {
            return "Tetris Attack (Expanded)"; 
        }
        public TetrisAttackExpandedHandler()
        {
            AllowedSpawns = CascadingPopBlockGameHandler<TetrisAttackStatistics, TetrisAttackGameOptions>.AllowedSpawnsFlags.Spawn_Full;
        }
    }
    [HandlerTipText("Tetris Attack, 5 Types")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerMenuCategory("Tetris Attack")]
    public class TetrisAttackHandler : CascadingPopBlockGameHandler<TetrisAttackStatistics, TetrisAttackGameOptions>, IGameHandlerChooserInitializer, IExtendedGameCustomizationHandler
    {
        //still needed:
        //1. Implement block swapping (deny swapping on the bottom two rows!)
        //2. move active group to start closer to bottom.
        //3. Change Active group visuals- well, this would be more appropriate as part of an appropriate visual theme since we'll want that for Tetris Attack as well!.
        //4. Have the gamefield start with say 3 rows filled in.
        [Flags]
        public enum TetrisAttackExpandedCombiningTypes
        {
            None = 0,
            Seventh = 1,
            Eighth = 2,
            Ninth = 4,
            Tenth = 8
        }
        public override FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows = 15,
                BottomHiddenFieldRows = 1,
                TopHiddenFieldRows = 2,
                FieldColumns = 7
            };
        }

        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.LineSeriesBlock() { BlockColor = Color.Yellow , Popping = true }) { NoGhost = true, FallSpeed = 0, Y = 6, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip });
        }
    

        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.LineSeriesBlock() { BlockColor = Color.Yellow, Popping = true }) { NoGhost = true, FallSpeed = 0, Y = 6, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip }) ;
        }
       
        public override GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }

        public override string GetName()
        {
            return "Tetris Attack";
        }
       
        public TetrisAttackHandler()
        {
            SimplePopHandling = true;
            IgnoreActiveGroupsForFieldChange = true;
            AllowedSpawns = CascadingPopBlockGameHandler<TetrisAttackStatistics, TetrisAttackGameOptions>.AllowedSpawnsFlags.Spawn_5;
            LevelCompleteWhenMasterCountZero = true;
            ProcessWithoutMasses = true;
        }
        public override Nomino[] GetNominos()
        {
            return new Nomino[] { new Duomino.Duomino((a) => new Blocks.StandardColouredBlock() { BlockColor = Color.Yellow }) { FallSpeed = 0,Y=6, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip } } ;
        }

        public override void HandleLevelComplete(IStateOwner pOwner, GameplayGameState state)
        {
            //throw new NotImplementedException();
        }
        
        public override IGameCustomizationHandler NewInstance()
        {
            return new TetrisAttackHandler();
        }
        double LastPercentage = 0;

        public override void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            for (int i = 0; i < 3; i++)
                state.PlayField.Contents[state.PlayField.RowCount - 1 - i] = GenerateNewColumn(state.PlayField, state.PlayField.RowCount - 1 - i,state.PlayField.ColCount).ToArray();

            
        }
        TimeSpan onesecond = new TimeSpan(0, 0, 1);
        bool MultiGroups = false;
        private ExtendedCustomizationHandlerResult PositiveResult = new ExtendedCustomizationHandlerResult(true);
        public ExtendedCustomizationHandlerResult GameProc(GameplayGameState state, IStateOwner pOwner)
        {
            
            state.DrawNextQueue = false;
            //TODO: Fix this up, speed needs to be variable and additionally, we need to not move when the game is not actively being played. (We should verify that the gamestate is the GamePlayGameState).

            TimeSpan Elapsed = pOwner.GetElapsedTime();
            //for a first test we'll do every 5 seconds...
            double PercentageMove = 0;
            if (state.PlayField.GetActiveBlockGroups().Count() > 1)
            {
                MultiGroups = true;
            }
            else if(state.PlayField.GetActiveBlockGroups().Count() == 1 && MultiGroups))
            {
                MultiGroups = false;
                LastPopComplete = pOwner.GetElapsedTime();
            }

            if (state.PlayField.GetActiveBlockGroups().Count() > 1 || !(pOwner.CurrentState is GameplayGameState) || pOwner.GetElapsedTime()-LastPopComplete < onesecond)
            {
            }

            else if (pOwner is IGamePresenter gp)
            {
                PercentageMove = (1d / 1500d) * ((1d / 60d) / gp.FrameTime);
            }
            //PercentageMove = 0;
            state.PlayField.OffsetPaint = state.PlayField.OffsetPaint - (float)PercentageMove;
            if (state.PlayField.OffsetPaint <= -1) state.PlayField.OffsetPaint = state.PlayField.OffsetPaint + 1;
            if (LastPercentage < state.PlayField.OffsetPaint)
            {
                //move all blocks in the playfield up one line, then generate a line of blocks at the bottom.

                //first, check current top visible row. If it has any blocks, Game Over...
                var TopRow = state.PlayField.Contents[state.PlayField.HIDDENROWS_TOP];
                if ((TopRow.Any((a) => a != null)))
                {
                    state.GameOvered = true;
                    return PositiveResult;
                }
                for (int r = state.PlayField.HIDDENROWS_TOP + 1; r < state.PlayField.Contents.Length-1; r++)
                {
                    for (int cc = 0; cc < state.PlayField.ColCount; cc++)
                    {
                        //state.PlayField.Contents[r][cc] = state.PlayField.Contents[r + 1][cc];
                        if(state.PlayField.Contents[r][cc]!=null)
                            state.PlayField.Contents[r][cc].Owner.Y--;
                    }

                    Array.Copy(state.PlayField.Contents[r + 1], state.PlayField.Contents[r], state.PlayField.ColCount);

                }
                var arraynew = GenerateNewColumn(state.PlayField, state.PlayField.Contents.Length - 1, state.PlayField.ColCount).ToArray();

                Array.Copy(arraynew, state.PlayField.Contents[state.PlayField.Contents.Length - 1], state.PlayField.ColCount);
                //move all ActiveGroups -1...
                foreach (var checkgroup in state.PlayField.GetActiveBlockGroups())
                {
                    checkgroup.Y--;
                }
                var changecall =  base.ProcessFieldChange(state, pOwner, null);

            }
            state.PlayField.HasChanged = true;
            LastPercentage = state.PlayField.OffsetPaint;
            return PositiveResult;
        }
        public override LineSeriesBlock.CombiningTypes[] GetValidBlockCombiningTypes()
        {
            var result = base.GetValidBlockCombiningTypes();

            return result;
        }
        private IEnumerable<NominoBlock> GenerateNewColumn(TetrisField pf,int row,int Count)
        {
            NominoBlock previouslyyielded = null;
            //TODO: generated lines should not create matches at the time they generate.
            //To accomplish this:
            //No block can match the block already at the bottom of the field
            //No block can match the block that was just generated.
            //Basically: the new line of blocks will create no "lines" of equal blocks.
            for (int i = 0; i < Count; i++)
            {
                var combinecheck = GetValidBlockCombiningTypes().ToList();
                //remove the type of the block immediately above, and if this is not the first block, the last block we yielded.
                var checkblocks = new[] { pf.Contents[row - 1][i],previouslyyielded };
                foreach (var blockadjacent in checkblocks)
                {
                    if (blockadjacent is LineSeriesBlock lsb)
                    {
                        combinecheck.Remove(lsb.CombiningIndex);
                    }
                }
                var createBlock = new LineSeriesPrimaryBlock() { Fixed=false, CombiningIndex = TetrisGame.Choose(combinecheck) };
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, createBlock);
                Dummino.Y = row;
                Dummino.X = i;
                pf.Theme.ApplyTheme(Dummino, this, pf, NominoTheme.ThemeApplicationReason.FieldSet);
                createBlock.Owner = Dummino;
                yield return (previouslyyielded = createBlock);
            }
        }
        public ExtendedCustomizationHandlerResult HandleGameKey(GameplayGameState state, IStateOwner pOwner, GameState.GameKeys g)
        {
            if (state.PlayField.GetActiveBlockGroups().Count() > 1 || !(pOwner.CurrentState is GameplayGameState))
            {
                return new ExtendedCustomizationHandlerResult(false);
            }
            if (g == GameState.GameKeys.GameKey_Hold)
            {
                LastPercentage = -1;
                return new ExtendedCustomizationHandlerResult(false);
            }
            if (g == GameState.GameKeys.GameKey_RotateCW || g == GameState.GameKeys.GameKey_RotateCCW)
            {

                //Task: swap the positions of the blocks at the positions of the blocks of the active nomino, then call the state and tell it we changed the field and to process that change.
                //return new ExtendedCustomizationHandlerResult(false);
                foreach (var iterate in state.PlayField.GetActiveBlockGroups())
                {
                    if(iterate.Y > 0)
                        if (iterate.Count == 2)
                        {
                            var FirstBlock = iterate.First();
                            var LastBlock = iterate.Last();
                            Point FirstPos = new Point(iterate.X + FirstBlock.X, iterate.Y + FirstBlock.Y);
                            Point SecondPos = new Point(iterate.X + LastBlock.X, iterate.Y + LastBlock.Y);
                            if (FirstPos.X > state.PlayField.ColCount - 1 || FirstPos.X < 0 ||
                                FirstPos.Y > state.PlayField.RowCount - 1 || FirstPos.Y < 0 ||
                                SecondPos.X > state.PlayField.ColCount - 1 || SecondPos.X < 0 ||
                                SecondPos.Y > state.PlayField.RowCount - 1 || SecondPos.Y < 0)
                                continue;
                            (state.PlayField.Contents[SecondPos.Y][SecondPos.X], state.PlayField.Contents[FirstPos.Y][FirstPos.X]) =
                                (state.PlayField.Contents[FirstPos.Y][FirstPos.X], state.PlayField.Contents[SecondPos.Y][SecondPos.X]);


                            state.PlayField.HasChanged = true;
                            NominoBlock FirstNBlock, SecondNBlock;
                            TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.BlockGroupRotate.Key);

                            //swap the owners positions. as well.

                            if (state.PlayField.Contents[FirstPos.Y][FirstPos.X]?.Owner != null)
                            {
                                state.PlayField.Contents[FirstPos.Y][FirstPos.X].Owner.X = FirstPos.X;
                                state.PlayField.Contents[FirstPos.Y][FirstPos.X].Owner.Y = FirstPos.Y;
                            }

                            if (state.PlayField.Contents[SecondPos.Y][SecondPos.X]?.Owner != null)
                            {
                                state.PlayField.Contents[SecondPos.Y][SecondPos.X].Owner.X = FirstPos.X;
                                state.PlayField.Contents[SecondPos.Y][SecondPos.X].Owner.Y = FirstPos.Y;
                            }

                           // (state.PlayField.Contents[FirstPos.Y][FirstPos.X].Owner, state.PlayField.Contents[SecondPos.Y][SecondPos.X].Owner) =
                           //     (state.PlayField.Contents[SecondPos.Y][SecondPos.X].Owner, state.PlayField.Contents[FirstPos.Y][FirstPos.X].Owner);

                           


                            base.ProcessFieldChange(state, pOwner, null);
                            

                        }



                }
                return new ExtendedCustomizationHandlerResult(false);

            }

            //throw new NotImplementedException();
            return PositiveResult;
        }
        
    }
}
