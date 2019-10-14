using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(GameOverGameState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class GameOverStateRenderingHandler : StandardStateRenderingHandler<Graphics,GameOverGameState,GameStateDrawParameters>
    {
        Brush useCoverBrush = null;
        private void DrawTetrominoStat(GameOverGameState Self,  Type TetronimoType, PointF BasePosition, Graphics Target, RectangleF Bounds, Font GameOverFont)
        {
            StandardTetrisGameState standardgame = Self.GameOveredState as StandardTetrisGameState;
            Image I_Tet = standardgame.GetTetronimoImage(TetronimoType);
            Target.DrawImage(I_Tet, new PointF(BasePosition.X - (float)(I_Tet.Width) / 2, BasePosition.Y));
            PointF TextPos = new PointF(BasePosition.X + Bounds.Width / 2, BasePosition.Y - 10);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.White, 5 + TextPos.X, 5 + TextPos.Y);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.Black, TextPos.X, TextPos.Y);
        }
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, GameOverGameState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.GameOveredState is StandardTetrisGameState)
            {
                StandardTetrisGameState standardgame = Source.GameOveredState as StandardTetrisGameState;
                SizeF BlockSize = new SizeF(Bounds.Width / (float)standardgame.PlayField.ColCount, Bounds.Height / (float)standardgame.PlayField.RowCount);
                useCoverBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)Bounds.Width, (int)BlockSize.Height), Color.DarkSlateGray, Color.MintCream, LinearGradientMode.Vertical);
                RenderingProvider.Static.DrawElement(pOwner,g, Source.GameOveredState,Element);
                g.FillRectangle(useCoverBrush, 0f, 0f, (float)Bounds.Width, (float)BlockSize.Height * Source.CoverBlocks);
            }
           
            if (Source. CompleteScroll)
            {
                Font EntryFont = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
                Font GameOverFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);

                var measured = g.MeasureString(Source.GameOverText, GameOverFont);
                var measuremini = g.MeasureString(Source.GameOverText, EntryFont);
                PointF GameOverPos = new PointF(Bounds.Width / 2 - measured.Width / 2, measured.Height / 4);
                g.DrawString(Source.GameOverText, GameOverFont, Brushes.White, 5 + GameOverPos.X, 5 + GameOverPos.Y);
                g.DrawString(Source.GameOverText, GameOverFont, Brushes.Black, GameOverPos.X, GameOverPos.Y);

                //draw each "line" of summary statistical information based on ShowExtraLines.

                for (int i = 0; i < Source.ShowExtraLines; i++)
                {
                    float XPosition = Bounds.Width * 0.25f;
                    float YPosition = GameOverPos.Y + ((1 + i) * measuremini.Height) + measuremini.Height;

                    if (i == 0)
                    {
                        var measuredmini = g.MeasureString("---Line Clears---", EntryFont);
                        TetrisGame.DrawText(g, EntryFont, "---Line Clears---", Brushes.Black, Brushes.White, Bounds.Width / 2 - measuredmini.Width / 2, GameOverPos.Y + measured.Height);
                    }
                    
                    if (i == 1) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_I), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 2) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_O), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 3) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_T), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 4) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_J), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 5) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_L), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 6) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_S), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 7) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_Z), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                }

                if (Source.NewScorePosition > -1)
                {
                    Font HighScoreEntryFont = new Font(GameOverFont.FontFamily, (float)(8 * pOwner.ScaleFactor), FontStyle.Regular);
                    //draw the awarded score position as well.
                    float XPosition = Bounds.Width * .25f;
                    float YPosition = Bounds.Height - measured.Height - 10;
                    String ScoreText = "New High Score!";
                    var MeasuredScoreText = g.MeasureString(ScoreText, HighScoreEntryFont);
                    using (Brush RainbowBrush = new SolidBrush(TetrisGame.GetRainbowColor(Color.Lime, 0.1d)))
                    {
                        TetrisGame.DrawText(g, GameOverFont, ScoreText, Brushes.Black, RainbowBrush, Bounds.Width / 2 - MeasuredScoreText.Width / 2, YPosition + measuremini.Height);
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, GameOverGameState Source, GameStateDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source.GameOveredState,Element);
        }
    }
}