using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public class GameOverGameState : GameState
    {
        private GameState GameOveredState = null;

        public int CoverBlocks = 0;
        private bool CompleteScroll = false;
        private DateTime CompleteScrollTime = DateTime.MaxValue;
        private DateTime InitTime;
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
            if ((DateTime.Now - CompleteScrollTime).TotalMilliseconds > 500)
            {
                CompleteScrollTime = DateTime.MaxValue;
                TetrisGame.Soundman.PlaySound("tetris_game_over");
            }
            if (((DateTime.Now - InitTime)).TotalMilliseconds < 1500) return;
            if ((DateTime.Now - LastAdvance).TotalMilliseconds > 50 && !CompleteScroll)
            {
                LastAdvance = DateTime.Now;
                TetrisGame.Soundman.PlaySound("shade_move");
                CoverBlocks++;
                StandardTetrisGameState standardstate = GameOveredState as StandardTetrisGameState;
                if (standardstate != null)
                {
                    if (CoverBlocks >= standardstate.PlayField.RowCount)
                    {
                        CoverBlocks = standardstate.PlayField.RowCount;
                        CompleteScrollTime = DateTime.Now;
                        CompleteScroll = true;
                    }
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
                SizeF BlockSize = new SizeF(Bounds.Width / (float)standardgame.PlayField.ColCount, Bounds.Height / (float)standardgame.PlayField.RowCount);
                useCoverBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)Bounds.Width, (int)BlockSize.Height), Color.DarkSlateGray, Color.MintCream, LinearGradientMode.Vertical);
                GameOveredState.DrawProc(pOwner, g, Bounds);
                g.FillRectangle(useCoverBrush, 0f, 0f, (float)Bounds.Width, (float)BlockSize.Height * CoverBlocks);
            }

            if (CompleteScroll)
            {
                Font GameOverFont = new Font(TetrisGame.RetroFont, 24);
                String GameOverText = "GAME\nOVER";
                var measured = g.MeasureString(GameOverText, GameOverFont);
                g.DrawString(GameOverText, GameOverFont, Brushes.White, 5 + (Bounds.Width / 2) - measured.Width / 2, 5 + (Bounds.Height / 2) - measured.Height / 2);
                g.DrawString(GameOverText, GameOverFont, Brushes.Black, (Bounds.Width / 2) - measured.Width / 2, (Bounds.Height / 2) - measured.Height / 2);
            }


        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {

        }

    }
}
