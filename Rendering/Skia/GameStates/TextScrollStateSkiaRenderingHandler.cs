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






    [RenderingHandler(typeof(TextScrollState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TextScrollStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, TextScrollState, GameStateSkiaDrawParameters> // StandardStateRenderingHandler<SKCanvas, PauseGameState, GameStateSkiaDrawParameters>
    {
        StarfieldStarData[] Stars = null;
        SKPaint Foreground = null;
        SKPaint Shadow = null;
        
        private void GenerateStars(float CenterX,float CenterY,SKRect Bounds)
        {
            float[] AvailableFactors = new float[] { 1f, 0.5f, 0.25f,1.1f,1.25f,0.1f };
            float[] AvailableScales = new float[] { 0.25f, .5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f, 5f };
            float[] chooseweights = new float[] { 20, 30, 40, 100, 40, 30, 20, 1, 1 };
            Stars = new StarfieldStarData[265];
            for (int i = 0; i < Stars.Length; i++)
            {
                float sx = (float)(CenterX + (TetrisGame.StatelessRandomizer.NextDouble() - 0.5) * Bounds.Width);
                float sy = (float)(CenterY + (TetrisGame.StatelessRandomizer.NextDouble() - 0.5) * Bounds.Height);
                
                Stars[i] = new StarfieldStarData(sx, sy);
                Stars[i].SpeedFactor = TetrisGame.Choose(AvailableFactors)/3;
                Stars[i].SizeMultiplier = 1f; // RandomHelper.Select(chooseweights, AvailableFactors)/10;
            }
        }
    public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TextScrollState Source, GameStateSkiaDrawParameters Element)
        {

            float MiddleX = Element.Bounds.Width / 2 + Element.Bounds.Left;
            float MiddleY = Element.Bounds.Height / 2 + Element.Bounds.Top;
            if (Foreground == null)
            {
                Foreground = new SKPaint()
                {
                    Color = SKColors.LightBlue,
                    TextSize = (float)(24 * pOwner.ScaleFactor),
                    Typeface = TetrisGame.CreditFontSK   

                };
                Shadow = new SKPaint()
                {
                    Color = SKColors.Navy,
                    TextSize = (float)(24 * pOwner.ScaleFactor),
                    Typeface = TetrisGame.CreditFontSK   
                };

            }

            if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Element.Bounds));
            }
            /*
            if (Stars == null)
            {
                GenerateStars(MiddleX,MiddleY,Element.Bounds);
            }
            var g = pRenderTarget;
            g.Clear(SKColors.Black);

            //draw stars.

            foreach (var stardraw in Stars)
            {

                var x = stardraw.X;
                var y = stardraw.Y;
                var r = stardraw.SizeMultiplier* ((pOwner.ScaleFactor)* ( 0.001 * (Math.Sqrt(Math.Pow(x - MiddleX, 2) + Math.Pow(y - MiddleY, 2)))));

                g.DrawCircle(new SKPoint(x, y), (float)r, stardraw.StarPaint);

                //update star position now.
                stardraw.X = (float)(stardraw.X + ((stardraw.X - MiddleX) * 0.025)*(stardraw.SpeedFactor * Source.WarpFactor) + Source.DirectionAdd.X);
                stardraw.Y = (float)(stardraw.Y + ((stardraw.Y - MiddleY) * 0.025)*(stardraw.SpeedFactor*Source.WarpFactor) + Source.DirectionAdd.Y);


                if (stardraw.X < Element.Bounds.Left - 50 || stardraw.X > Element.Bounds.Right + 50 ||
                    stardraw.Y < Element.Bounds.Top - 50 || stardraw.Y > Element.Bounds.Bottom + 50)
                {
                    float sx = (float)(MiddleX+ (TetrisGame.rgen.NextDouble()-0.5)*(Element.Bounds.Width/3) );
                    float sy = (float)(MiddleY + (TetrisGame.rgen.NextDouble()-0.5)*(Element.Bounds.Width / 3));
                    stardraw.X = sx;
                    stardraw.Y = sy;
                }
                

            }
            */

            if (Source.CurrentItem != null)
            {
                if (Source.CurrentItem is TextScrollEntry tse)
                {

                    String[] sRenderText = tse.Text;
                    SKRect[] RenderBounds = new SKRect[sRenderText.Length];

                    for (int i = 0; i < RenderBounds.Length; i++)
                    {
                        Foreground.MeasureText(sRenderText[i], ref RenderBounds[i]);
                    }

                    float TotalHeight = RenderBounds.Sum((a) => a.Height);
                    float MaxWidth = RenderBounds.Max((a) => a.Width);
                    float MaxHeight = RenderBounds.Max((a) => a.Height);


                    float CurrentY = Element.Bounds.Height / 2 - (TotalHeight);


                    for (int i = 0; i < sRenderText.Length; i++)
                    {
                        String sRender = sRenderText[i];
                        SKRect useBounds = RenderBounds[i];
                        var usePosition = new SKPoint(MiddleX - useBounds.Width/2, CurrentY);
                        pRenderTarget.DrawText(sRender, new SKPoint((float)(usePosition.X + 5 * pOwner.ScaleFactor), (float)(usePosition.Y + 5 * pOwner.ScaleFactor)), Shadow);
                        pRenderTarget.DrawText(sRender, new SKPoint(usePosition.X , usePosition.Y ), Foreground);

                        CurrentY += MaxHeight;


                    }


                    

                    


                }


            }


        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, TextScrollState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
