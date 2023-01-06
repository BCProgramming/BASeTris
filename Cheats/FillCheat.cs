using BASeTris.Blocks;
using BASeTris.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Cheats
{
    public class FillCheat : Cheat
    {
        public override string DisplayName => "Fill";

        public override string CheatName => "Fill";

        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            if (pStateOwner.CurrentState is GameplayGameState mainstate)
            {
                int usestart = 0;
                for (int r = 0; r < mainstate.PlayField.Contents.Length; r++)
                {
                    if (mainstate.PlayField.Contents[r].Any((a) => a != null))
                    {
                        usestart = r;
                        break;
                    }
                }
                for (int i = usestart; i < mainstate.PlayField.Contents.Length; i++)
                {


                    for (int c = 0; c < mainstate.PlayField.Contents[i].Length; c++)
                    {
                        if (mainstate.PlayField.Contents[i][c] == null)
                            mainstate.PlayField.Contents[i][c] = new StandardColouredBlock();
                    }
                }

                return true;
            }
            return false;
        }
    }
}
