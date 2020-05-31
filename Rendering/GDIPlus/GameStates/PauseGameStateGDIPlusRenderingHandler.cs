using System;
using System.Collections.Generic;
using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(PauseGameState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class PauseGameStateGDIPlusRenderingHandler: StandardStateRenderingHandler<Graphics, PauseGameState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            if(!Source.DrawDataInitialized)
            {
                InitDrawData(pOwner, Source, Element);
                Source.DrawDataInitialized = true;
            }
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
        private void InitDrawData(IStateOwner pOwner,PauseGameState Source, GameStateDrawParameters Element)
        {
            if (Source.PausedState is StandardTetrisGameState std)
            {
                var rgen = new Random();
                //TODO: well this clearly shouldn't be here...
                Image[] availableImages = std.GetTetronimoImages();
                var Areause = pOwner.GameArea;
                Source.FallImages = new List<PauseGameState.PauseFallImageBase>();
                for (int i = 0; i < PauseGameState.NumFallingItems; i++)
                {
                    PauseGameState.PauseFallImageGDIPlus pfi = new PauseGameState.PauseFallImageGDIPlus();
                    pfi.OurImage = TetrisGame.Choose(availableImages);
                    pfi.XSpeed = (float)(rgen.NextDouble() * 10) - 5;
                    pfi.YSpeed = (float)(rgen.NextDouble() * 10) - 5;
                    pfi.AngleSpeed = (float)(rgen.NextDouble() * 20) - 10;
                    pfi.XPosition = (float)rgen.NextDouble() * (float)Areause.Width;
                    pfi.YPosition = (float)rgen.NextDouble() * (float)Areause.Height;
                    Source.FallImages.Add(pfi);
                }
            }
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