using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates;

namespace BASeTris.Cheats
{
    public class DebugCheat : Cheat
    {
        public override string DisplayName
        {
            get { return "Debugging Cheat"; }
        }

        public override string CheatName
        {
            get { return "Cheat"; }
        }

        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            if (CheatParameters.Length == 0) return false;
            int parsednumber;
            if (int.TryParse(CheatParameters[0], out parsednumber))
            {
                switch (parsednumber)
                {
                    case 0:
                        ShowHighScoresState shs = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], pStateOwner.CurrentState, null);
                        pStateOwner.CurrentState = shs;
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}