using BASeCamp.BASeScores;
using BASeCamp.Rendering;
using BASeTris.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(ShowHighScoresState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ShowHighScoreStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, ShowHighScoresState, GameStateSkiaDrawParameters>
    {
        SKPaint LinePen = new SKPaint() { Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(25, 0), new SKColor[] { SKColors.Black, SKColors.DarkGray }, null, SKShaderTileMode.Mirror) };
        
        private String PointerText = "►";
        
        private void DrawBackground(ShowHighScoresState Self, IStateOwner pOwner, SKCanvas g, SKRect Bounds)
        {
            //ColorMatrices.GetFader(1.0f - ((float)i * 0.1f))
            g.Clear(SKColors.White);

            if (Self.BG != null)
                RenderingProvider.Static.DrawElement(pOwner, g, Self.BG, new BackgroundDrawers.SkiaBackgroundDrawData(Bounds));

        }

        SKPaint[] HighlightPaints = new SKPaint[]
        {
            new SKPaint() {Color=SKColors.Lime},
            new SKPaint() {Color=SKColors.Blue}
        };

        private SKPaint GetHighlightBrush()
        {
            return DateTime.Now.Millisecond < 500 ? HighlightPaints[0] : HighlightPaints[1];
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ShowHighScoresState Source, GameStateSkiaDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            float StartY = Bounds.Height * 0.175f;
            float MiddleX = Bounds.Width / 2;
            DrawBackground(Source, pOwner, g, Bounds);
            float TextSize = Bounds.Height / 30f;
            var ScoreFont = TetrisGame.RetroFontSK;
            
            /*
            float LineHeight = g.MeasureString("#", ScoreFont).Height + 5;
            //This needs to change based on the actual gameplay area size.)
            if (Source.IncrementedDrawState >= 0)
            {
                //Draw HIGH SCORES
                var Measured = g.MeasureString(Source.HeaderText, ScoreFont);
                PointF DrawPosition = new PointF(MiddleX - (Measured.Width / 2), StartY);
                g.DrawString(Source.HeaderText, ScoreFont, Brushes.White, new PointF(DrawPosition.X + 2, DrawPosition.Y + 2));
                g.DrawString(Source.HeaderText, ScoreFont, Brushes.Black, DrawPosition);
            }

            if (Source.IncrementedDrawState >= 1)
            {
                float LineYPosition = StartY + LineHeight;

                //draw a line underneath the High scores text
                g.DrawLine(LinePen, 20, LineYPosition, Bounds.Width - 20, LineYPosition);
            }

            if (Source.IncrementedDrawState >= 2)
            {
                //draw the high score listing entries.
                //iterate from 2 to drawstate and draw the high score at position drawstate-2.
                for (int scoreiterate = 2; scoreiterate < Source.IncrementedDrawState; scoreiterate++)
                {
                    int CurrentScoreIndex = scoreiterate - 2;
                    int CurrentScorePosition = CurrentScoreIndex + 1;
                    float useYPosition = StartY + (LineHeight * 2.5f) + LineHeight * CurrentScoreIndex;
                    float UseXPosition = Bounds.Width * 0.19f;
                    String sUseName = "N/A";
                    int sUseScore = 0;
                    IHighScoreEntry currentScore = Source.hs.Count > CurrentScoreIndex ? Source.hs[CurrentScoreIndex] : null;
                    if (currentScore != null)
                    {
                        sUseName = currentScore.Name;
                        sUseScore = currentScore.Score;
                    }

                    var MeasureScore = g.MeasureString(sUseScore.ToString(), ScoreFont);
                    var MeasureName = g.MeasureString(sUseName, ScoreFont);
                    float PosXPosition = Bounds.Width * 0.1f;
                    float NameXPosition = Bounds.Width * 0.20f;
                    float ScoreXPositionRight = Bounds.Width * (1 - 0.10f);
                    Brush DrawScoreBrush = Source.HighlightedScorePositions.Contains(CurrentScorePosition) ? GetHighlightBrush() : Brushes.Gray;

                    g.DrawString(CurrentScorePosition.ToString(), ScoreFont, Brushes.Black, PosXPosition + 2, useYPosition + 2);
                    g.DrawString(CurrentScorePosition.ToString(), ScoreFont, DrawScoreBrush, PosXPosition, useYPosition);

                    g.DrawString(sUseName, ScoreFont, Brushes.Black, NameXPosition + 2, useYPosition + 2);
                    g.DrawString(sUseName, ScoreFont, DrawScoreBrush, NameXPosition, useYPosition);

                    float ScoreXPosition = ScoreXPositionRight - MeasureScore.Width;

                    g.DrawString(sUseScore.ToString(), ScoreFont, Brushes.Black, ScoreXPosition + 2, useYPosition + 2);
                    g.DrawString(sUseScore.ToString(), ScoreFont, DrawScoreBrush, ScoreXPosition, useYPosition);

                    g.DrawLine(new Pen(DrawScoreBrush, 3), NameXPosition + MeasureName.Width + 15, useYPosition + LineHeight / 2, ScoreXPosition - 15, useYPosition + LineHeight / 2);

                    if (Source.SelectedScorePosition == CurrentScoreIndex)
                    {
                        //draw the selection arrow to the left of the NamePosition and useYPosition.
                        var MeasureArrow = g.MeasureString(PointerText, ScoreFont);
                        float ArrowX = PosXPosition - MeasureArrow.Width - 5;
                        float ArrowY = useYPosition;
                        g.DrawString(PointerText, ScoreFont, Brushes.Black, ArrowX + 2, ArrowY + 2);
                        g.DrawString(PointerText, ScoreFont, DrawScoreBrush, ArrowX, ArrowY);
                    }
                }
            }*/
           
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, ShowHighScoresState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
