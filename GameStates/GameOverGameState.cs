using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;

namespace BASeTris.GameStates
{
    public class GameOverGameState : GameState
    {
        private GameState GameOveredState = null;

        public int CoverBlocks = 0;
        private bool CompleteSummary = false;
        private bool CompleteScroll = false;
        private int ShowExtraLines = 0;
        private int MaxExtraLines = 7;
        private DateTime CompleteScrollTime = DateTime.MaxValue;
        private DateTime CompleteSummaryTime = DateTime.MaxValue;

        private DateTime InitTime;

        //if the score is a high score, this will be changed to the position after the game stats are displayed.
        int NewScorePosition = -1;

        public GameOverGameState(GameState paused)
        {
            GameOveredState = paused;
            InitTime = DateTime.Now;
            TetrisGame.Soundman.PlaySound("mmdeath");
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            GameOveredState.DrawStats(pOwner, g, Bounds);
        }

        DateTime LastAdvance = DateTime.MinValue;

        public override void GameProc(IStateOwner pOwner)
        {
            if ((DateTime.Now - CompleteSummaryTime).TotalMilliseconds > 500)
            {
                CompleteSummaryTime = DateTime.MaxValue;
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOver);
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
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOverShade);
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
                        TetrisGame.Soundman.PlaySound("block_place_2");
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

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (GameOveredState is StandardTetrisGameState)
            {
                StandardTetrisGameState standardgame = GameOveredState as StandardTetrisGameState;
                SizeF BlockSize = new SizeF(Bounds.Width / (float) standardgame.PlayField.ColCount, Bounds.Height / (float) standardgame.PlayField.RowCount);
                useCoverBrush = new LinearGradientBrush(new Rectangle(0, 0, (int) Bounds.Width, (int) BlockSize.Height), Color.DarkSlateGray, Color.MintCream, LinearGradientMode.Vertical);
                GameOveredState.DrawProc(pOwner, g, Bounds);
                g.FillRectangle(useCoverBrush, 0f, 0f, (float) Bounds.Width, (float) BlockSize.Height * CoverBlocks);
            }

            if (CompleteScroll)
            {
                Font EntryFont = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
                Font GameOverFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
                String GameOverText = "GAME\nOVER\n"; //+ ShowExtraLines.ToString();
                var measured = g.MeasureString(GameOverText, GameOverFont);
                var measuremini = g.MeasureString(GameOverText, EntryFont);
                PointF GameOverPos = new PointF(Bounds.Width / 2 - measured.Width / 2, measured.Height);
                g.DrawString(GameOverText, GameOverFont, Brushes.White, 5 + GameOverPos.X, 5 + GameOverPos.Y);
                g.DrawString(GameOverText, GameOverFont, Brushes.Black, GameOverPos.X, GameOverPos.Y);

                //draw each "line" of summary statistical information based on ShowExtraLines.

                for (int i = 0; i < ShowExtraLines; i++)
                {
                    float XPosition = Bounds.Width * 0.25f;
                    float YPosition = GameOverPos.Y + ((1 + i) * measuremini.Height) + 10;

                    if (i == 0)
                    {
                        var measuredmini = g.MeasureString("---Line Clears---", EntryFont);
                        g.DrawString("---Line Clears---", EntryFont, Brushes.White, Bounds.Width / 2 - measuredmini.Width / 2, GameOverPos.Y + measured.Height);
                        g.DrawString("---Line Clears---", EntryFont, Brushes.Black, Bounds.Width / 2 - measuredmini.Width / 2 - 5, GameOverPos.Y + measured.Height - 5);
                    }

                    if (i == 1) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_I), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 2) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_O), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 3) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_T), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 4) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_J), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 5) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_L), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 6) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_S), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 7) DrawTetrominoStat(typeof(Tetrominoes.Tetromino_Z), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                }

                if (NewScorePosition > -1)
                {
                    //draw the awarded score position as well.
                    float XPosition = Bounds.Width * .25f;
                    float YPosition = GameOverPos.Y + ((1 + MaxExtraLines) * measured.Height) + 10;
                    String ScoreText = "New High Score!";
                    var MeasuredScoreText = g.MeasureString(ScoreText, GameOverFont);


                    g.DrawString(ScoreText, GameOverFont, Brushes.White, Bounds.Width / 2 - MeasuredScoreText.Width / 2, YPosition + measured.Height);
                    g.DrawString(ScoreText, GameOverFont, Brushes.Black, Bounds.Width / 2 - MeasuredScoreText.Width / 2 - 5, YPosition + measured.Height - 5);
                }
            }
        }


        private void DrawTetrominoStat(Type TetronimoType, PointF BasePosition, Graphics Target, RectangleF Bounds, Font GameOverFont)
        {
            StandardTetrisGameState standardgame = GameOveredState as StandardTetrisGameState;
            Image I_Tet = standardgame.GetTetronimoImage(TetronimoType);
            Target.DrawImage(I_Tet, new PointF(BasePosition.X - (float) (I_Tet.Width) / 2, BasePosition.Y));
            PointF TextPos = new PointF(BasePosition.X + Bounds.Width / 2, BasePosition.Y - 10);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.White, 5 + TextPos.X, 5 + TextPos.Y);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.Black, TextPos.X, TextPos.Y);
        }

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
                        TetrisGame.Soundman.PlayMusic("highscoreentry");
                    }
                }
            }
        }
    }
}