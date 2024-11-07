using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.CustomOptionPreparers
{

    //Option Preparer state that mimics the Tetris introduction from NES Tetris. Why? No reason. get over it.
    public class TetrisOptionPreparerState : GameState
    {
        public override void GameProc(IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            throw new NotImplementedException();
        }
    }
}
