using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    class DropWaitState : GameState, ICompositeState<GameplayGameState>
    {
        private IStateOwner pStateOwner = null;
        private GameplayGameState OwnerState = null;
        public DropWaitState(IStateOwner pOwner,GameplayGameState pOwnerState)
        {
            pStateOwner = pOwner;
            OwnerState = pOwnerState;
            OwnerState.PlayField.BlockGroupSet += PlayField_BlockGroupSet;
        }

        private void PlayField_BlockGroupSet(object sender, TetrisField.BlockGroupSetEventArgs e)
        {
            if (OwnerState.PlayField.GetActiveBlockGroups().Count == 0)
            {
                pStateOwner.CurrentState = OwnerState;
            }
        }

       

        public override void GameProc(IStateOwner pOwner)
        {
            OwnerState.GameProc(pOwner);
            
           
        }

        public GameplayGameState GetComposite()
        {
            return OwnerState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //do not allow this to pass through to the composite state, otherwise the BlockGroups will be able to be controlled.
        }
    }
}
