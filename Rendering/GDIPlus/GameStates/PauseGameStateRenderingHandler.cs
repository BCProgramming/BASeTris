using System;
using System.Drawing;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(PauseGameState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class PauseGameStateRenderingHandler: StandardStateRenderingHandler<Graphics, PauseGameState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            Graphics g = pRenderTarget;
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
            var basecall = RenderingProvider.Static.GetHandler(typeof(Graphics), typeof(MenuState), typeof(GameStateDrawParameters));
            basecall?.Render(pOwner,pRenderTarget,Source,Element); //draw the menu itself.

        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            //delegate...
            var PausedState = Source.PausedState;
            if(PausedState!=null)
            {
                RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,PausedState,Element);
            }
            
        }

       
    }
}