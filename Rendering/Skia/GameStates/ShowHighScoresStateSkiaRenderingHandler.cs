using BASeCamp.BASeScores;
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
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
            //g.Clear(SKColors.White);

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
            if(Source.BG==null)
            {



                StandardImageBackgroundSkia sk = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                sk.Data.Movement = new SKPoint(3, 3);
                Source.BG = sk;

            }
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            
            float StartY = (Bounds.Height * 0.175f);
            var CurrentY = StartY;
            float MiddleX = Bounds.Width / 2;
            DrawBackground(Source, pOwner, g, Bounds);
            float TextSize = Bounds.Height / 30f;
            var ScoreFont = TetrisGame.RetroFontSK; //point size 24.
            SKPaint MainScoreFont = new SKPaint() {Typeface = ScoreFont,TextSize = (float)(24*pOwner.ScaleFactor),Color=SKColors.Black };
            SKPaint ShadowScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor),Color=SKColors.White };
            SKPaint ListingFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.Black };
            float PercentThroughSecond = (float)DateTime.Now.Millisecond / 1000f;
            SKPaint ListingFontRainbow = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColor.FromHsl(PercentThroughSecond*240,240,120)};
            SKPaint ListingFontShadow = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.White };
            SKPaint ListingFontArrow = new SKPaint() { Typeface = TetrisGame.ArialFontSK, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColor.FromHsl(PercentThroughSecond * 240, 240, 120) };
            SKPaint ListingFontArrowShadow = new SKPaint() { Typeface = TetrisGame.ArialFontSK, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.White };
            SKRect resultitem = new SKRect();
            float LineHeight = MainScoreFont.MeasureText("#", ref resultitem);
            var useShader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, Bounds.Height), new SKColor[] { SKColors.Red, SKColors.Orange, SKColors.Yellow, SKColors.Green, SKColors.Blue, SKColors.Indigo, SKColors.Violet }, null, SKShaderTileMode.Mirror);

            SKPaint LinePaint = new SKPaint() { BlendMode = SKBlendMode.ColorBurn, StrokeWidth = 24, Shader = useShader };
            g.DrawRect(new SKRect(0, 0, Bounds.Width, Bounds.Height), LinePaint);
            //g.DrawRect(new SKRect((int)(Bounds.Width * (1 / 7)), CurrentY, (float)(Bounds.Width - (Bounds.Width * (1 / 7))), (float)(CurrentY + (LineHeight * 2.5) + (LineHeight * 3) * 12)),LinePaint);


            
            if (Source.IncrementedDrawState >= 0)
            {
                //draw "HIGH SCORES" header text.
                SKRect MeasuredRect = new SKRect();
                MainScoreFont.MeasureText(Source.HeaderText, ref MeasuredRect);
                SKPoint DrawPosition = new SKPoint(MiddleX - (MeasuredRect.Width / 2), StartY);
                g.DrawText(Source.HeaderText, new SKPoint(DrawPosition.X + 2, DrawPosition.Y + 2), ShadowScoreFont);
                g.DrawText(Source.HeaderText, DrawPosition, MainScoreFont);
                CurrentY = StartY + MeasuredRect.Height + 10;
            }
            
            if(Source.IncrementedDrawState >=1)
            {
              //maybe a line under the header.
            }

            if(Source.IncrementedDrawState >= 2)
            {
                //draw the high score listing entries.
                //iterate from 2 to drawstate and draw the high score at position drawstate-2.
                for (int scoreiterate = 2;scoreiterate < Source.IncrementedDrawState; scoreiterate++)
                {
                    int CurrentScoreIndex = scoreiterate - 2;
                    int CurrentScorePosition = CurrentScoreIndex + 1;
                    double useYPosition = StartY + (LineHeight * 2.5) + (LineHeight*3) * CurrentScoreIndex;
                    double useXPosition = Bounds.Width * 0.19d;
                    String sUseName = "N/A";
                    int sUseScore = 0;
                    IHighScoreEntry currentScore = Source.hs.Count > CurrentScoreIndex ? Source.hs[CurrentScoreIndex] : null;
                    if (currentScore != null)
                    {
                        sUseName = currentScore.Name;
                        sUseScore = currentScore.Score;
                    }
                    SKRect MeasureName= new SKRect(), MeasureScore = new SKRect();
                    ListingFont.MeasureText(sUseName, ref MeasureName);
                    ListingFont.MeasureText(sUseScore.ToString(), ref MeasureScore);
                    float PosXPosition = Bounds.Width * 0.1f;
                    float NameXPosition = Bounds.Width * 0.20f;
                    float ScoreXPositionRight = Bounds.Width * (1 - 0.10f);
                    var useForegroundPaint = Source.HighlightedScorePositions.Contains(CurrentScorePosition) ? ListingFontRainbow : ListingFont;

                    //draw position
                    g.DrawText(CurrentScorePosition.ToString() + ".", new SKPoint(PosXPosition + 2, (float)useYPosition + 2), ListingFontShadow);
                    g.DrawText(CurrentScorePosition.ToString() + ".", new SKPoint(PosXPosition, (float)useYPosition), useForegroundPaint);
                    //draw high score name
                    g.DrawText(sUseName, new SKPoint(PosXPosition + 2 + Math.Abs(resultitem.Height)*2.25f, (float)useYPosition + 2), ListingFontShadow);
                    g.DrawText(sUseName, new SKPoint(PosXPosition + Math.Abs(resultitem.Height) * 2.25f, (float)useYPosition), useForegroundPaint);

                    //draw the high score
                    float ScoreXPosition = ScoreXPositionRight - MeasureScore.Width;

                    g.DrawText(sUseScore.ToString(), new SKPoint(ScoreXPosition + 2, (float)useYPosition + 2), ListingFontShadow);
                    g.DrawText(sUseScore.ToString(), new SKPoint(ScoreXPosition , (float)useYPosition ), useForegroundPaint);
                    useForegroundPaint.StrokeWidth = 6;

                    g.DrawLine(new SKPoint(NameXPosition + MeasureName.Width + 15, (float)useYPosition + LineHeight / 2),new SKPoint(ScoreXPosition - 15, (float)useYPosition + LineHeight / 2),useForegroundPaint);

                    if(Source.SelectedScorePosition == CurrentScoreIndex)
                    {
                        //draw selection indicator if needed
                        SKRect MeasureArrow = new SKRect();
                        useForegroundPaint.MeasureText(PointerText, ref MeasureArrow);
                        float ArrowX = PosXPosition - MeasureArrow.Width - 5;
                        float ArrowY = (float)useYPosition;
                        g.DrawText(PointerText, new SKPoint(ArrowX + 2, ArrowY + 2), ListingFontArrowShadow);
                        g.DrawText(PointerText, new SKPoint(ArrowX , ArrowY ), ListingFontArrow);
                    }
                }
                
            }
           

                }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, ShowHighScoresState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
