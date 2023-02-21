using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Properties;

namespace BASeTris.GameStates
{
    public class GameOverGameState : GameState
    {
        public GameState GameOveredState = null;

        public int CoverBlocks = 0;
        public bool CompleteSummary = false;
        public bool CompleteScroll = false;
        public int CurrentLinesDisplay = 0;
        public int LineMaxIndex = 7;
        public DateTime CompleteScrollTime = DateTime.MaxValue;
        public DateTime CompleteSummaryTime = DateTime.MaxValue;



        public GameOverStatistics GameOverInfo { get; set; } = null;

        public DateTime InitTime;

        //if the score is a high score, this will be changed to the position after the game stats are displayed.
        public int NewScorePosition = -1;

        public GameOverGameState(GameState paused,GameOverStatistics StatInfo)
        {
            GameOverInfo = StatInfo;
            GameOveredState = paused;
            InitTime = DateTime.Now;
            LineMaxIndex = StatInfo==null?0:StatInfo.Statistics.Count-1;
        }

       

    

        DateTime LastAdvance = DateTime.MinValue;
        bool PlayedDeathSound = false;
        public override void GameProc(IStateOwner pOwner)
        {
            if(!PlayedDeathSound)
            {
                PlayedDeathSound = true;
                TetrisGame.Soundman.PlaySound("mmdeath", pOwner.Settings.std.EffectVolume);
            }
            if ((DateTime.Now - CompleteSummaryTime).TotalMilliseconds > 500)
            {
                CompleteSummaryTime = DateTime.MaxValue;
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOver.Key, pOwner.Settings.std.EffectVolume);
                GameplayGameState standardstate = GameOveredState as GameplayGameState;
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
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key, pOwner.Settings.std.EffectVolume);
                CoverBlocks++;
                GameplayGameState standardstate = GameOveredState as GameplayGameState;
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
                    if (CurrentLinesDisplay != calcresult)
                    {
                        TetrisGame.Soundman.PlaySound("block_place_2", pOwner.Settings.std.EffectVolume);
                    }

                    CurrentLinesDisplay = calcresult;
                }

                if (CurrentLinesDisplay > LineMaxIndex)
                {
                    CompleteSummary = true;
                    CompleteSummaryTime = DateTime.Now;
                }
            }

            //gameproc doesn't pass through!
        }

        Brush useCoverBrush = null;
        public String GameOverText = "GAME    OVER"; //+ ShowExtraLines.ToString();
        
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_RotateCW)
            {
                if (NewScorePosition > -1)
                {
                    if (GameOveredState is GameplayGameState)
                    {
                        var useStats = ((GameplayGameState)GameOveredState).GameStats;
                        var MenuState = ((GameplayGameState)GameOveredState).MainMenuState;
                        if (useStats is TetrisStatistics)
                        {

                            EnterHighScoreState ehs = new EnterHighScoreState
                            (GameOveredState, pOwner,MenuState,
                                ((GameplayGameState)GameOveredState).GetLocalScores(), (n, s) => new XMLScoreEntry<TetrisHighScoreData>(n, s, new TetrisHighScoreData(useStats as TetrisStatistics))
                                , useStats as TetrisStatistics);
                            pOwner.CurrentState = ehs;
                            TetrisGame.Soundman.PlayMusic("highscoreentry", pOwner.Settings.std.MusicVolume, true);
                        }
                        else
                        {

                        }
                    }
                    else if (CompleteSummary)
                    {
                        IBackground bg = null;
                        if (pOwner is BASeTris bt)
                        {
                            bg = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();
                        }
                        else if (pOwner is BASeTrisTK)
                        {
                            bg = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                        }
                        //GenericMenuState TitleMenu = new GenericMenuState(bg, pOwner, new TitleMenuPopulator());
                        pOwner.CurrentState = new TitleMenuState(bg, pOwner);
                    }
                }
                else if (CompleteSummary)
                {
                    IBackground bg = null;
                    if (pOwner is BASeTris bt)
                    {
                        bg = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();
                    }
                    else if (pOwner is BASeTrisTK)
                    {
                        bg = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                        
                    }
                    //GenericMenuState TitleMenu = new GenericMenuState(bg, pOwner, new TitleMenuPopulator());
                    pOwner.CurrentState = new TitleMenuState(bg, pOwner);
                }

            }
           
        }
    }
}