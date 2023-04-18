using BASeCamp.Rendering;
using BASeTris.Rendering.Adapters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{

    [RenderingHandler(typeof(HotLine), typeof(SKCanvas), typeof(TetrisFieldDrawSkiaParameters))]
    public class TetrisFieldHotLineSkiaRenderingHandler : StandardRenderingHandler<SkiaSharp.SKCanvas, HotLine, TetrisFieldDrawSkiaParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, HotLine Source, TetrisFieldDrawSkiaParameters Element)
        {

            double LineHeight = Element.Bounds.Height;
            //throw new NotImplementedException();
            

            SKColor low = (SKColor)(BCColor)Color.Black;
            SKColor upp = (SKColor)(BCColor)Source.Color;
            String sMultiplierText = String.Format("x {0:#.##}", Source.Multiplier);
            SKShader gradShader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, (int)LineHeight), new SKColor[] { low, upp }, SKShaderTileMode.Repeat);

            var useFont = TetrisGame.GetRetroFont((float)(LineHeight * 0.8f), 1, FontStyle.Regular, GraphicsUnit.Pixel);
            SKPaint ForegroundText = new SKPaint() { TextSize = (float)(LineHeight * 0.33f), Typeface = TetrisGame.RetroFontSK, Color = upp };
            SKPaint BackgroundText = new SKPaint() { TextSize = (float)(LineHeight * 0.33f), Typeface = TetrisGame.RetroFontSK, Color = SKColors.Black };

            

            DrawTextInformationSkia sktext = new DrawTextInformationSkia() {Text = sMultiplierText, ShadowPaint = BackgroundText,ForegroundPaint = ForegroundText,DrawFont = new SKFontInfo(TetrisGame.RetroFontSK,(float)LineHeight*0.2f),
                CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia() { Height = 3 } )  };

            sktext.ShadowOffset = new SKPoint(5, 5);
            sktext.CharacterHandler = new DrawCharacterHandlerSkia();
            SKRect multbounds = new SKRect();
            var Measured = sktext.ForegroundPaint.MeasureText(sMultiplierText, ref multbounds);
            sktext.Position = new SKPoint(Element.Bounds.Left + 5, Element.Bounds.Top + Element.Bounds.Height / 2 + multbounds.Height / 2);
            
            


            using (SKPaint skp = new SKPaint() { Shader = gradShader, Color = new SKColor(0, 0, 0, 128) })
            {
                pRenderTarget.DrawRect(Element.Bounds, skp);
            }
            pRenderTarget.DrawTextSK(sktext);
        }
    }
}
