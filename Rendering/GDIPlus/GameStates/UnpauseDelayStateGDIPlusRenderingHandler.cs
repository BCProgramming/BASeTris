using System;
using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(UnpauseDelayGameState), typeof(Graphics), typeof(BaseDrawParameters))]
    public class UnpauseDelayStateGDIPlusRenderingHandler : StandardStateRenderingHandler<Graphics,UnpauseDelayGameState,BaseDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, UnpauseDelayGameState Source, BaseDrawParameters Element)
        {
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,Source._ReturnState,Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
            //draw a centered Countdown

            if (Source.LastSecond != Source.timeremaining.Seconds)
            {
                //emit a sound.
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key, pOwner.Settings.std.EffectVolume);
                Source.LastSecond = Source.timeremaining.Seconds;
                Source.lastMillis = 1000;
            }

            double SecondsLeft = Math.Round(Source.timeremaining.TotalSeconds, 1);
            String sSecondsLeft = Source.timeremaining.ToString("%s");
            double Millis = (double)Source.timeremaining.Milliseconds / 1000d; //millis in percent. We will use this to animate the unpause time left.
            Millis = Math.Min(Millis, Source.lastMillis);
            float useSize = (float)(64f * (1 - (Millis)));
            var SecondsFont = TetrisGame.GetRetroFont(useSize, pOwner.ScaleFactor);
            var MeasureText = g.MeasureString(sSecondsLeft, SecondsFont);

            PointF DrawPosition = new PointF(Bounds.Width / 2 - MeasureText.Width / 2, Bounds.Height / 2 - MeasureText.Height / 2);

            g.DrawString(sSecondsLeft, SecondsFont, Brushes.White, DrawPosition);
            Source.lastMillis = Millis;
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, UnpauseDelayGameState Source, BaseDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._ReturnState,Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(pRenderTarget, Element.Bounds);

            
        }
        private void DrawFadeOverlay(Graphics g, RectangleF Bounds)
        {
            g.FillRectangle(fadeBrush, Bounds);
        }
        Brush fadeBrush = new SolidBrush(Color.FromArgb(200, Color.Black));
    }
}