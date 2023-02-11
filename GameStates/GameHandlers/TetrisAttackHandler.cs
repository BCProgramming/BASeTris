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
    [HandlerTipText("Tetris Attack, Marathon Mode (WIP!)")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    
    public class TetrisAttackHandler : CascadingPopBlockGameHandler<TetrisAttackStatistics, TetrisAttackGameOptions>, IGameHandlerChooserInitializer, IExtendedGameCustomizationHandler
    {
        //still needed:
        //1. Implement block swapping (deny swapping on the bottom two rows!)
        //2. move active group to start closer to bottom.
        //3. Change Active group visuals- well, this would be more appropriate as part of an appropriate visual theme since we'll want that for Tetris Attack as well!.
        //4. Have the gamefield start with say 3 rows filled in.

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
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.LineSeriesBlock() { BlockColor = Color.Yellow , Popping = true }) { FallSpeed = 0, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip });
        }
    

        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.LineSeriesBlock() { BlockColor = Color.Yellow,Popping=true }) {FallSpeed=0,Flags = Nomino.NominoControlFlags.ControlFlags_DropMove|Nomino.NominoControlFlags.ControlFlags_NoClip }   );
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
            AllowedSpawns = AllowedSpawnsFlags.Spawn_Full;
        }
        public override Nomino[] GetNominos()
        {
            return new Nomino[] { new Duomino.Duomino((a) => new Blocks.StandardColouredBlock() { BlockColor = Color.Yellow }) { FallSpeed = 0, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip } } ;
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
            //
        }
        private ExtendedCustomizationHandlerResult PositiveResult = new ExtendedCustomizationHandlerResult(true);
        public ExtendedCustomizationHandlerResult GameProc(GameplayGameState state, IStateOwner pOwner)
        {
            TimeSpan Elapsed = pOwner.GetElapsedTime();
            //for a first test we'll do every 5 seconds...
            double PercentageMove = (Elapsed.TotalSeconds % 10) / 10;
            state.PlayField.OffsetPaint = -(float)PercentageMove;
            if (LastPercentage > PercentageMove)
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
                        state.PlayField.Contents[r][cc] = state.PlayField.Contents[r + 1][cc];
                    }
                }
                state.PlayField.Contents[state.PlayField.Contents.Length - 1] = GenerateNewColumn(state.PlayField, state.PlayField.ColCount).ToArray();
                //move all ActiveGroups -1...
                foreach (var checkgroup in state.PlayField.GetActiveBlockGroups())
                {
                    checkgroup.Y--;
                }

            }
            state.PlayField.HasChanged = true;
            LastPercentage = PercentageMove;
            return PositiveResult;
        }
        private IEnumerable<NominoBlock> GenerateNewColumn(TetrisField pf,int Count)
        {
            for (int i = 0; i < Count; i++)
            {
                var createBlock = new LineSeriesBlock() { CombiningIndex = TetrisGame.Choose(base.GetValidBlockCombiningTypes()) };
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, createBlock);

                pf.Theme.ApplyTheme(Dummino, this, pf, NominoTheme.ThemeApplicationReason.FieldSet);
                
                yield return createBlock;
            }
        }
        public ExtendedCustomizationHandlerResult HandleGameKey(GameplayGameState state, IStateOwner pOwner, GameState.GameKeys g)
        {

            if (g == GameState.GameKeys.GameKey_Drop)
            {

                //Task: swap the positions of the blocks at the positions of the blocks of the active nomino, then call the state and tell it we changed the field and to process that change.
                return new ExtendedCustomizationHandlerResult(false);
            }

            //throw new NotImplementedException();
            return PositiveResult;
        }
    }
}
