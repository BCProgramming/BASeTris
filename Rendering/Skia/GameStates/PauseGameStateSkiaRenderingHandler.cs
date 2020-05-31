using BASeTris.GameStates;
using SkiaSharp;
using BASeCamp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(PauseGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class PauseGameStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, PauseGameState, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, PauseGameState Source, GameStateSkiaDrawParameters Element)
        {
           /* Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            var FallImages = Source.FallImages;
            //Render the paused state.
            Font usePauseFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }

            g.ResetTransform();
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            TetrisGame.DrawText(g, usePauseFont, sPauseText, Brushes.White, Brushes.Black, DrawPos.X, DrawPos.Y);
            //retrieve the renderer for the MenuState object.
            var basecall = RenderingProvider.Static.GetHandler(typeof(Graphics), typeof(MenuState), typeof(GameStateSkiaDrawParameters));
            basecall?.Render(pOwner, pRenderTarget, Source, Element); //draw the menu itself.
            */
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, PauseGameState Source, GameStateSkiaDrawParameters Element)
        {
            /*
            //delegate...
            var PausedState = Source.PausedState;
            if (PausedState != null)
            {
                RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, PausedState, Element);
            }
            */

        }


    }
}
