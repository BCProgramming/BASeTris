using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerStates
{
    //State which is used to display the "level complete" indication when a level completes in Dr.Mario.
    //
    public class DrMarioLevelCompleteState : GameState, ICompositeState<GameplayGameState>
    {
        private GameplayGameState OriginalState = null;
        Func<GameState> StateProcessionFunction = null;
        public DrMarioLevelCompleteState(GameplayGameState pState,Func<GameState> AdvanceToStateFunc)
        {
            StateProcessionFunction = AdvanceToStateFunc;
            OriginalState = pState;
        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
        
        public override void GameProc(IStateOwner pOwner)
        {
            
            //implementation for now, assumes that the caller will change the music, and AdvanceToStateFunc will handle setting the normal music.
            //throw new NotImplementedException();
        }

        public GameplayGameState GetComposite()
        {
            return OriginalState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
        }
    }
}
