using BASeCamp.Rendering;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(GameOverGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class GameOverStateRenderingHandler : StandardStateRenderingHandler<SKCanvas, GameOverGameState, GameStateSkiaDrawParameters>
    {
        SKPaint useCoverBrush = null;

        
        static GameOverStateRenderingHandler()
            {
        
           
            }
    private void DrawTetrominoStat(GameOverGameState Self, Type TetronimoType, SKPoint BasePosition, SKCanvas Target, SKRect Bounds,SKPaint paint)
        {
            
            


            GameplayGameState standardgame = Self.GameOveredState as GameplayGameState;
            var useStats = standardgame.GameStats as TetrisStatistics;
            SKBitmap I_Tet = standardgame.GetTetrominoSKBitmap(TetronimoType);
            Target.DrawBitmap(I_Tet, new SKPoint(BasePosition.X - (float)(I_Tet.Width) / 2, BasePosition.Y));
            
            SKPoint TextPos = new SKPoint(BasePosition.X + Bounds.Width / 2, BasePosition.Y - 10);
            String LineCount = useStats.GetLineCount(TetronimoType).ToString();
            paint.Color = SKColors.White;
            Target.DrawText(LineCount, TextPos.X+5, TextPos.Y+5, paint);
            paint.Color = SKColors.Black;
            Target.DrawText(LineCount, TextPos.X, TextPos.Y, paint);
        }
        
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, GameOverGameState Source, GameStateSkiaDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.GameOveredState is GameplayGameState)
            {
                GameplayGameState standardgame = Source.GameOveredState as GameplayGameState;
                SKPoint BlockSize = new SKPoint(Bounds.Width / (float)standardgame.PlayField.ColCount, Bounds.Height / (float)standardgame.PlayField.RowCount);
                
                if (useCoverBrush == null)
                {
                    
                    useCoverBrush = new SKPaint();
                    useCoverBrush.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, BlockSize.Y), new SKColor[] { SkiaSharp.Views.Desktop.Extensions.ToSKColor(Color.DarkSlateGray),
                    SkiaSharp.Views.Desktop.Extensions.ToSKColor(Color.MintCream)}, null, SKShaderTileMode.Repeat);

                }
                //draw the game overed state...
                RenderingProvider.Static.DrawElement(pOwner, g, Source.GameOveredState, Element);

                g.DrawRect(new SKRect(0,0,(float)Bounds.Width, (float)BlockSize.Y * Source.CoverBlocks), useCoverBrush);

            }
            float YPosition = 0;
            float XPosition = 0;
            if(Source.CompleteScroll)
            {
                //draw each line of summary statistical info. Only draw the number of lines specified by Source.ShowExtraLines.

                SKPaint GameOverTitlePaint = new SKPaint() { TextSize = (float)(48 * pOwner.ScaleFactor),Color=SKColors.White, Typeface = TetrisGame.RetroFontSK };
                SKPaint GameOverEntryPaint = new SKPaint() { TextSize = (float)(28 * pOwner.ScaleFactor),Color=SKColors.White,Typeface = TetrisGame.RetroFontSK };
                SKRect measured = new SKRect(), measuremini = new SKRect();
                //we primarily want text heights for the later calcs.
                GameOverTitlePaint.MeasureText(Source.GameOverText,ref measured);
                GameOverEntryPaint.MeasureText(Source.GameOverText, ref measuremini);
                SKPoint GameOverPos = new SKPoint(Bounds.Width / 2 - measured.Width / 2, measured.Height + measured.Height/2);
                //draw title.
                g.DrawText(Source.GameOverText, new SKPoint(GameOverPos.X+5,GameOverPos.Y+5), GameOverTitlePaint);
                GameOverTitlePaint.Color = SKColors.Black;
                g.DrawText(Source.GameOverText, new SKPoint(GameOverPos.X, GameOverPos.Y), GameOverTitlePaint);
                var useStats = Source.GameOverInfo;
                SKRect DesiredRect = new SKRect(Bounds.Width * 0.18f, 20, Bounds.Width * (1 - .18f), Bounds.Height - 20);
                XPosition = DesiredRect.Left;
                if (useStats != null)
                {
                    for (int i = 0; i < Source.CurrentLinesDisplay; i++)
                    {
                        var currentLine = useStats.Statistics[i];

                        SKPoint CellSize = new SKPoint(DesiredRect.Width / currentLine.ColumnData.Count(), DesiredRect.Height / useStats.Statistics.Count - 1);


                        YPosition = GameOverPos.Y + ((2 + i) * (measured.Height * 2.25f)) + measuremini.Height * 2;
                        float MaxYSize = 0;
                        for (int columnindex = 0; columnindex < currentLine.ColumnData.Count; columnindex++)
                        {
                            var currentcolumn = currentLine.ColumnData[columnindex];

                            SKRect CellPosition = new SKRect(XPosition, YPosition, XPosition + CellSize.X, YPosition + CellSize.Y);

                            switch (currentcolumn.InfoType)
                            {
                                case GameOverStatisticColumnData.CellType.Image:

                                    SKPoint DrawPosition;
                                    SKImage useimage = currentcolumn.CellImage;
                                    var targetposition = GameOverStatisticColumnData.AlignRect(CellPosition, new SKPoint(useimage.Width, useimage.Height),
                                        currentcolumn.ContentAlignmentHorizontal, currentcolumn.ContentAlignmentVertical);
                                    pRenderTarget.DrawImage(useimage, targetposition);
                                    if (useimage.Height > MaxYSize) MaxYSize = useimage.Height;
                                    break;
                                case GameOverStatisticColumnData.CellType.Text:

                                    //measure the text for this item.
                                    //TODO: custom font support?
                                    SKRect TextSize = new SKRect();
                                    GameOverEntryPaint.MeasureText(currentcolumn.CellText, ref TextSize);
                                    var targetpositiontext = GameOverStatisticColumnData.AlignRect(CellPosition, new SKPoint(TextSize.Width, TextSize.Height),
                                        currentcolumn.ContentAlignmentHorizontal, currentcolumn.ContentAlignmentVertical);
                                    SKPoint TargetPos = new SKPoint(targetpositiontext.Left, targetpositiontext.Top - TextSize.Height); //add height because Skia paints from text baseline.

                                    //paint the text shadow...
                                    GameOverEntryPaint.Color = SKColors.White;
                                    pRenderTarget.DrawText(currentcolumn.CellText, new SKPoint(TargetPos.X + 5, TargetPos.Y + 5), GameOverEntryPaint);
                                    GameOverEntryPaint.Color = SKColors.Black;
                                    pRenderTarget.DrawText(currentcolumn.CellText, TargetPos, GameOverEntryPaint);
                                    if (targetpositiontext.Height > MaxYSize) MaxYSize = targetpositiontext.Height;

                                    break;

                            }


                        }


                        /*
                        Type[] TetTypes = new Type[] {typeof(Tetrominoes.Tetromino_I),
                                typeof(Tetrominoes.Tetromino_I) ,
                                typeof(Tetrominoes.Tetromino_O) ,
                                typeof(Tetrominoes.Tetromino_T) ,
                                typeof(Tetrominoes.Tetromino_J) ,
                                typeof(Tetrominoes.Tetromino_L) ,
                                typeof(Tetrominoes.Tetromino_S),
                            typeof(Tetrominoes.Tetromino_Z)};
                        XPosition = Bounds.Width * 0.25f;
                        YPosition = GameOverPos.Y + ((2 + i) * (measured.Height*2.25f)) + measuremini.Height * 2;
                        if(i==0)
                        {
                            SKRect measuredmini = new SKRect();
                            GameOverEntryPaint.MeasureText("---Line Clears---", ref measuredmini);
                            SKPoint ChosenPosition = new SKPoint(Bounds.Width / 2 - measuredmini.Width / 2, GameOverPos.Y + measured.Height * 2);
                            //draw shadow...
                            GameOverEntryPaint.Color = SKColors.White;
                            g.DrawText("---Line Clears---", new SKPoint(ChosenPosition.X + 5, ChosenPosition.Y + 5), GameOverEntryPaint);
                            GameOverEntryPaint.Color = SKColors.Black;
                            g.DrawText("---Line Clears---", ChosenPosition, GameOverEntryPaint);
                        }
                        if (i >= 2)
                        {
                            DrawTetrominoStat(Source, TetTypes[i - 1], new SKPoint(XPosition, YPosition), g, Bounds, GameOverEntryPaint);
                        }*/


                    }
                }
                if (Source.NewScorePosition > -1)
                {
                    using (SKPaint HighScoreEligible = new SKPaint()
                    {
                        Color = SKColors.Black,
                        Typeface = TetrisGame.RetroFontSK,
                        TextSize = (float)(24 * pOwner.ScaleFactor)
                    })
                    {
                        SKPaint ShadowPaint = new SKPaint()
                        {
                            Color = SKColors.DarkGray,
                            Typeface = TetrisGame.RetroFontSK,
                            TextSize = (float)(24 * pOwner.ScaleFactor)
                        };
                        float XPos = Bounds.Width * .25f;
                        float YPos = Bounds.Height - measured.Height - 10;
                        String ScoreText = "New High Score!";
                        SKRect MeasuredScoreText = new SKRect();
                        HighScoreEligible.MeasureText(ScoreText, ref MeasuredScoreText);
                        var useStyle = new DrawTextInformationSkia()
                        {
                            Text = ScoreText,
                            BackgroundPaint = new SKPaint() { Color = SKColors.Transparent },
                            DrawFont = new Adapters.SKFontInfo(TetrisGame.RetroFontSK, (float)(24 * pOwner.ScaleFactor)),
                            ForegroundPaint = HighScoreEligible,
                            ShadowPaint = new SKPaint(),
                            Position = new SKPoint(XPos, YPos + MeasuredScoreText.Height / 2),
                            ShadowOffset = new SKPoint(5f, 5f),
                        };
                        
                        HighScoreEligible.Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(TetrisGame.GetRainbowColor(Color.Lime, 0.1d));
                        useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculatorSkia());
                        pRenderTarget.DrawTextSK(useStyle);


                        //draw the awarded score position as well.

                        


                        //g.DrawText(ScoreText, new SKPoint(5 + Bounds.Width / 2 - MeasuredScoreText.Width / 2, 5 + YPosition + measuremini.Height * 2), HighScoreEligible);
                        //HighScoreEligible.Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(TetrisGame.GetRainbowColor(Color.Lime, 0.1d));
                        //g.DrawText(ScoreText, new SKPoint(5 + Bounds.Width / 2 - MeasuredScoreText.Width / 2, 5 + YPosition + measuremini.Height * 2), HighScoreEligible);
                    }
                }
            }

            /*
            if (Source.CompleteScroll)
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
            }*/
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, GameOverGameState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source.GameOveredState, Element);
        }
    }
}
