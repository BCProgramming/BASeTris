using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.GameHandlers.HandlerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    //[HandlerMenuCategory("Columns")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerOptionsMenu(typeof(DrMarioOptionsHandler))]
    [HandlerTipText("Columns (WIP)")]
    public class ColumnsGameHandler : CascadingPopBlockGameHandler<ColumnsStatistics, ColumnsGameOptions>, IGameHandlerChooserInitializer
    {
        public override FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldColumns = 6,
                FieldRows = 13,
                BottomHiddenFieldRows = 0,
                TopHiddenFieldRows = 2

            };
        }
        
        public ColumnsGameHandler()
        {
            Level = 0;
            base.LevelCompleteWhenMasterCountZero = false;
            base.ClearOrientations = ClearOrientationConstants.Horizontal | 
                ClearOrientationConstants.Vertical |
                ClearOrientationConstants.Diagonal;
        }
        public override GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }
        public override string GetName()
        {
            return "Columns";
        }
        private BlockGroupChooser _Chooser = null;
        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            if (_Chooser != null) return _Chooser;
            _Chooser = CreateSupportedChooser(typeof(SingleFunctionChooser));
            return _Chooser;
        }

        private void ColumnsNominoTweaker(Nomino Source)
        {
            int conIndex = 0;
            Source.AllowRotationAnimations = false;
            //tweak the nomino and set a random combining index.
            foreach (var iterate in Source)
            {
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    lsb.CombiningIndex = TetrisGame.Choose(GetValidBlockCombiningTypes());
                    lsb.ConnectionIndex = conIndex++;
                }
            }


        }
        Nomino[] savedNominoes = null;
        public override Nomino[] GetNominos()
        {
            if(savedNominoes == null)
                savedNominoes = new Nomino[] { CreateColumnNomino() };
            return savedNominoes;
        }
        public override IBlockGameCustomizationHandler NewInstance()
        {
            return new ColumnsGameHandler() { };
        }
        public override void HandleLevelComplete(IStateOwner pOwner, GameplayGameState state)
        {
            //var completionState = new DrMarioLevelCompleteState(state, () => SetupNextLevel(state, pOwner));
            //pOwner.CurrentState = completionState;
        }

        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            if (DesiredChooserType == typeof(SingleFunctionChooser))
            {
                var result = new SingleFunctionChooser(CreateColumnNomino);
                result.ResultAffector = ColumnsNominoTweaker;
                return result;
            }
            return null;
            //throw new NotImplementedException();
        }
        public Nomino CreateColumnNomino()
        {
            return NNominoGenerator.CreateColumnNomino(3, () => new LineSeriesBlock());
        }
        public override void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            state.PlayField.Reset();
        }
    }
}
