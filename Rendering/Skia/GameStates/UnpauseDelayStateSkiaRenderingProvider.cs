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
    [RenderingHandler(typeof(UnpauseDelayGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]

    public class UnpauseDelayStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, UnpauseDelayGameState, GameStateSkiaDrawParameters>
    {
        private SKPaint SecondsPaint = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, UnpauseDelayGameState Source, GameStateSkiaDrawParameters Element)
        {

            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._ReturnState, Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
            //draw a centered Countdown

            if (Source.LastSecond != Source.timeremaining.Seconds)
            {
                //emit a sound.
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key, pOwner.Settings.EffectVolume);
                Source.LastSecond = Source.timeremaining.Seconds;
                Source.lastMillis = 1000;
            }

            double SecondsLeft = Math.Round(Source.timeremaining.TotalSeconds, 1);
            String sSecondsLeft = Source.timeremaining.ToString("%s");
            double Millis = (double)Source.timeremaining.Milliseconds / 1000d; //millis in percent. We will use this to animate the unpause time left.
            Millis = Math.Min(Millis, Source.lastMillis);
            float useSize = (float)(64f * (1 - (Millis)))*(float)pOwner.ScaleFactor;
            var CurrentColor = SKColors.White;
            if(SecondsPaint==null)
            {
                SecondsPaint = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = useSize, Color = CurrentColor };
            }
            SecondsPaint.TextSize = useSize;
            SKRect MeasureText = new SKRect();
            SecondsPaint.MeasureText(sSecondsLeft, ref MeasureText);

            SKPoint DrawPosition = new SKPoint(Bounds.Width / 2 - MeasureText.Width / 2, Bounds.Height / 2 + MeasureText.Height / 2);

            g.DrawText(sSecondsLeft, DrawPosition, SecondsPaint);
            Source.lastMillis = Millis;
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, UnpauseDelayGameState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source._ReturnState, Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(pRenderTarget, Element.Bounds);


        }
        SKPaint fadeBrush = new SKPaint() { Color = new SKColor(0, 0, 0, 200) };
        private void DrawFadeOverlay(SKCanvas g, SKRect Bounds)
        {
            g.DrawRect(Bounds, fadeBrush);
        }
    }
}
