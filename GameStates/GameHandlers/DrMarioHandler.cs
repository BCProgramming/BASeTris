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
using BASeTris.FieldInitializers;
using BASeTris.GameObjects;
using BASeTris.GameStates.GameHandlers.HandlerOptions;
using BASeTris.GameStates.GameHandlers.HandlerStates;
using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
using SkiaSharp;

namespace BASeTris.GameStates.GameHandlers
{
    [HandlerMenuCategory("Dr. Mario")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerOptionsMenu(typeof(DrMarioOptionsHandler))]
    [HandlerTipText("Dr.Mario, but with 6 different viruses.")]
    public class DrMarioSixViruses : DrMarioHandler
    {
        public override string GetName()
        {
            return "Dr.Mario (6 Viruses)";
        }
        public DrMarioSixViruses()
        {
            AllowedSpawns = CascadingPopBlockGameHandler<DrMarioStatistics, DrMarioGameOptions>.AllowedSpawnsFlags.Spawn_Full;
        }
    }
    [HandlerMenuCategory("Dr. Mario")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerOptionsMenu(typeof(DrMarioOptionsHandler))]
    [HandlerTipText("Dr.Mario, but with copyright-distinct viruses")]
    public class DrMarioAltViruses : DrMarioHandler
    {
        public override string GetName()
        {
            return "Mr. Mario";
        }
        public DrMarioAltViruses()
        {
            AllowedSpawns = CascadingPopBlockGameHandler<DrMarioStatistics, DrMarioGameOptions>.AllowedSpawnsFlags.Spawn_Alternate;
        }
    }
    [HandlerMenuCategory("Dr. Mario")]
    [GameScoringHandler(typeof(DrMarioAIScoringHandler),typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerOptionsMenu(typeof(DrMarioOptionsHandler))]
    [HandlerTipText("Brightly coloured pills to cure Fever or Chills")]
    public class DrMarioHandler : CascadingPopBlockGameHandler<DrMarioStatistics,DrMarioGameOptions>,IGameHandlerChooserInitializer
    {
        public DrMarioHandler()
        {
            Level = 0;
        }
        public override GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }
        public override string GetName()
        {
            return "Dr.Mario";
        }
        private BlockGroupChooser _Chooser = null;
        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            if (_Chooser != null) return _Chooser;
            _Chooser = CreateSupportedChooser(typeof(SingleFunctionChooser));
            return _Chooser;
        }

        private void DrMarioNominoTweaker(Nomino Source)
        {
            //tweak the nomino and set a random combining index.
            foreach (var iterate in Source)
            {
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    lsb.CombiningIndex = TetrisGame.Choose(GetValidBlockCombiningTypes());
                }
            }


        }
        public override Nomino[] GetNominos()
        {
            return new Nomino[] { new Duomino.Duomino() };
        }
        public override IGameCustomizationHandler NewInstance()
        {
            return new DrMarioHandler() { } ;
        }
        public override void HandleLevelComplete(IStateOwner pOwner, GameplayGameState state)
        {
            var completionState = new DrMarioLevelCompleteState(state, () => SetupNextLevel(state, pOwner));
            pOwner.CurrentState = completionState;
        }

        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            if (DesiredChooserType == typeof(SingleFunctionChooser))
            {
                var result = Duomino.Duomino.DrMarioDuominoChooser();
                result.ResultAffector = DrMarioNominoTweaker;
                return result;
            }
            return null;
            //throw new NotImplementedException();
        }
    }
 
}
