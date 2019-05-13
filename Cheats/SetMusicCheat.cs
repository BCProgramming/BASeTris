using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Cheats
{
    class SetMusicCheat : Cheat
    {
        public override string DisplayName { get { return "Set Music"; } }
        public override string CheatName { get { return "setmusic"; } }
        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            if (CheatParameters.Any())
            {
                String sMusic = CheatParameters[0];
                if (TetrisGame.Soundman.HasSound(sMusic))
                {
                    TetrisGame.Soundman.PlayMusic(sMusic);
                }
                else
                {
                    return false;
                }
            }
            else {
                return false;
            }
            return true;
        
        }
    }
}
