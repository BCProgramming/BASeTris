using BASeCamp.BASeScores;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Tetrominoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Cheats
{
    internal class SetLinesCheat : Cheat
    {

        public override string DisplayName => "Set Lines";

        public override string CheatName => "setlines";

        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            var pOwner = pStateOwner;
            var phandler = pOwner.GetHandler();
            if (phandler is StandardTetrisHandler sth && CheatParameters.Length >= 1 && int.TryParse(CheatParameters[0], out int setlinecount))
            {
                sth.Statistics.SetLineCount(typeof(Tetromino_S), setlinecount);
            }


            return true;
            //TetrisGame.Soundman.PlayMusic("highscoreentry", pOwner.Settings.std.MusicVolume, true);
        }
    }
}
