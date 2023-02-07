using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{

    public class TetrisAttackHandler : CascadingPopBlockGameHandler<DrMarioStatistics, DrMarioGameOptions>, IGameHandlerChooserInitializer
    {
        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.StandardColouredBlock() { BlockColor = Color.Yellow }) { FallSpeed = 0, Flags = Nomino.NominoControlFlags.ControlFlags_DropMove | Nomino.NominoControlFlags.ControlFlags_NoClip });
        }
    

        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            return new SingleFunctionChooser(() => new Duomino.Duomino((a) => new Blocks.StandardColouredBlock() { BlockColor = Color.Yellow }) {FallSpeed=0,Flags = Nomino.NominoControlFlags.ControlFlags_DropMove|Nomino.NominoControlFlags.ControlFlags_NoClip }   );
        }
       
        public override GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }

        public override string GetName()
        {
            return "Tetris Attack";
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
    }
}
