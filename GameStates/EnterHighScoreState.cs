using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates
{
    public class EnterHighScoreState : EnterTextState
    {
        TetrisStatistics GameStatistics = null;
        IHighScoreList ScoreListing = null;
        Func<string, int, IHighScoreEntry> ScoreToEntryFunc = null;

        private int AchievedPosition;

        //private IBackground _BG = null;
        public override DisplayMode SupportedDisplayMode
        {
            get { return DisplayMode.Full; }
        }

        public EnterHighScoreState(GameState pOriginalState, IStateOwner pStateOwner, IHighScoreList ScoreList, Func<string, int, IHighScoreEntry> ScoreFunc, TetrisStatistics SourceStats)
            : base(pStateOwner, 10)
        {
            ScoreListing = ScoreList;
            ScoreToEntryFunc = ScoreFunc; //function which takes the score and gives back an appropriate IHighScoreEntry implementation.
            GameStatistics = SourceStats;
            AchievedPosition = ScoreListing.IsEligible(GameStatistics.Score);


            EntryPrompt = (" Congratulations, your score is\n at position " + AchievedPosition + " \n Enter your name.").Split('\n');
        }

     

        


        public override bool ValidateEntry(IStateOwner pOwner, string sCurrentEntry)
        {
            return true;
        }

        public override void CommitEntry(IStateOwner pOwner, string sCurrentEntry)
        {
            var submitscore = ScoreToEntryFunc(sCurrentEntry.ToString().Replace("_", " ").Trim(), GameStatistics.Score);
            ScoreListing.Submit(submitscore);
            TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.ClearTetris.Key, pOwner.Settings.EffectVolume);
            TetrisGame.Soundman.PlayMusic("high_score_list");
            pOwner.CurrentState = new ShowHighScoresState(ScoreListing, null, new int[] {AchievedPosition});
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
    }
}