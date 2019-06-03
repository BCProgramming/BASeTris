using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.Properties;

namespace BASeTris.GameStates
{
    public class GameOverGameState : GameState
    {
        public GameState GameOveredState = null;

        public int CoverBlocks = 0;
        public bool CompleteSummary = false;
        public bool CompleteScroll = false;
        public int ShowExtraLines = 0;
        public int MaxExtraLines = 7;
        public DateTime CompleteScrollTime = DateTime.MaxValue;
        public DateTime CompleteSummaryTime = DateTime.MaxValue;

        public DateTime InitTime;

        //if the score is a high score, this will be changed to the position after the game stats are displayed.
        public int NewScorePosition = -1;

        public GameOverGameState(GameState paused)
        {
            GameOveredState = paused;
            InitTime = DateTime.Now;
            
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

    

        DateTime LastAdvance = DateTime.MinValue;
        bool PlayedDeathSound = false;
        public override void GameProc(IStateOwner pOwner)
        {
            if(!PlayedDeathSound)
            {
                PlayedDeathSound = true;
                TetrisGame.Soundman.PlaySound("mmdeath", pOwner.Settings.EffectVolume);
            }
            if ((DateTime.Now - CompleteSummaryTime).TotalMilliseconds > 500)
            {
                CompleteSummaryTime = DateTime.MaxValue;
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOver,pOwner.Settings.EffectVolume);
                StandardTetrisGameState standardstate = GameOveredState as StandardTetrisGameState;
                if (standardstate != null)
                {
                    var grabposition = standardstate.GetLocalScores().IsEligible(standardstate.GameStats.Score);
                    if (grabposition > 0)
                    {
                        NewScorePosition = grabposition;
                    }
                }
            }

            if (((DateTime.Now - InitTime)).TotalMilliseconds < 1500) return;
            if ((DateTime.Now - LastAdvance).TotalMilliseconds > 50 && !CompleteScroll)
            {
                LastAdvance = DateTime.Now;
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOverShade, pOwner.Settings.EffectVolume);
                CoverBlocks++;
                StandardTetrisGameState standardstate = GameOveredState as StandardTetrisGameState;
                if (standardstate != null)
                {
                    if (CoverBlocks >= standardstate.PlayField.RowCount)
                    {
                        CoverBlocks = standardstate.PlayField.RowCount;
                        CompleteScrollTime = DateTime.Now;
                        CompleteScroll = true;
                        CompleteSummary = false;
                    }
                }
            }

            if (CompleteScroll && !CompleteSummary)
            {
                int calcresult = (int) ((DateTime.Now - CompleteScrollTime).TotalMilliseconds) / 750;
                if (calcresult > 0)
                {
                    if (ShowExtraLines != calcresult)
                    {
                        TetrisGame.Soundman.PlaySound("block_place_2", pOwner.Settings.EffectVolume);
                    }

                    ShowExtraLines = calcresult;
                }

                if (ShowExtraLines > MaxExtraLines)
                {
                    CompleteSummary = true;
                    CompleteSummaryTime = DateTime.Now;
                }
            }

            //gameproc doesn't pass through!
        }

        Brush useCoverBrush = null;
        public String GameOverText = "GAME\nOVER\n"; //+ ShowExtraLines.ToString();
        
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_RotateCW)
            {
                if (NewScorePosition > -1)
                {
                    if (GameOveredState is StandardTetrisGameState)
                    {
                        EnterHighScoreState ehs = new EnterHighScoreState
                        (GameOveredState, pOwner,
                            ((StandardTetrisGameState) GameOveredState).GetLocalScores(), (n, s) => new XMLScoreEntry<TetrisHighScoreData>(n, s, new TetrisHighScoreData(((StandardTetrisGameState) GameOveredState).GameStats))
                            , ((StandardTetrisGameState) GameOveredState).GameStats);
                        pOwner.CurrentState = ehs;
                        TetrisGame.Soundman.PlayMusic("highscoreentry",pOwner.Settings.MusicVolume,true);
                    }
                }
            }
        }
    }
}