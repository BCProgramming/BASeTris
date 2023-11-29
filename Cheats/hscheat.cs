using BASeCamp.BASeScores;
using BASeTris.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Cheats
{
    public class HSCheat : Cheat
    {


        /* EnterHighScoreState ehs = new EnterHighScoreState
                            (GameOveredState, pOwner,MenuState,
                                ((GameplayGameState)GameOveredState).GetLocalScores(), (n, s) => new XMLScoreEntry<TetrisHighScoreData>(n, s, new TetrisHighScoreData(useStats as TetrisStatistics))
                                , useStats as TetrisStatistics);
                            pOwner.CurrentState = ehs;
                            TetrisGame.Soundman.PlayMusic("highscoreentry", pOwner.Settings.std.MusicVolume, true);*/
        public override string DisplayName => "High Score Test";

        public override string CheatName => "hs";

        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            var pOwner = pStateOwner;
            var useStats = new TetrisStatistics();
            EnterHighScoreState ehs = new EnterHighScoreState
                            (pStateOwner.CurrentState, pOwner, pStateOwner.CurrentState,
                                new XMLHighScores<TetrisHighScoreData>("TEST",50000), (n, s) => new XMLScoreEntry<TetrisHighScoreData>(n, s, new TetrisHighScoreData(useStats,null))
                                , useStats);
            pOwner.CurrentState = ehs;
            return true;
            //TetrisGame.Soundman.PlayMusic("highscoreentry", pOwner.Settings.std.MusicVolume, true);
        }
    }
}
