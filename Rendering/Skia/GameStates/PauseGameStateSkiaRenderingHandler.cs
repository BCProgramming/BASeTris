using BASeTris.GameStates;
using SkiaSharp;
using BASeCamp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(PauseGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class PauseGameStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, PauseGameState, GameStateSkiaDrawParameters>
    {
        private void InitDrawData(IStateOwner pOwner, PauseGameState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source.PausedState is StandardTetrisGameState std)
            {
                var rgen = new Random();
                SKBitmap[] availableImages = std.GetTetrominoSKBitmaps();
                var Areause = pOwner.GameArea;
                Source.FallImages = new List<PauseGameState.PauseFallImageBase>();
                for (int i = 0; i < PauseGameState.NumFallingItems; i++)
                {
                    
                    PauseGameState.PauseFallImageSkiaSharp pfi = new PauseGameState.PauseFallImageSkiaSharp();
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
        static SKPaint GrayBG = new SKPaint() { Color = SKColors.Gray };
        private static SKPaint GameOverTextPaint = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, PauseGameState Source, GameStateSkiaDrawParameters Element)
        {

            if(!Source.DrawDataInitialized)
            {
                InitDrawData(pOwner, Source, Element);
                Source.DrawDataInitialized = true;
            }

            if(GameOverTextPaint ==null)
            {
                GameOverTextPaint = new SKPaint();
                GameOverTextPaint.Color = SKColors.Black;
                GameOverTextPaint.TextSize = 24;
                GameOverTextPaint.IsAntialias = true;
                GameOverTextPaint.Typeface = TetrisGame.RetroFontSK;
            }
            String sPauseText = "Pause";
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            var FallImages = Source.FallImages;
            SKRect MeasureBounds = new SKRect();
            var measureresult = GameOverTextPaint.MeasureText(sPauseText, ref MeasureBounds);
            //render the paused state.
            //TetrisGame.RetroFontSK
            g.DrawRect(Bounds, GrayBG);

            foreach(var iterate in FallImages)
            {
                iterate.Draw(g);
            }
            g.ResetMatrix();



            SKPoint DrawPos = new SKPoint(Bounds.Width - 2 - MeasureBounds.Width / 2, Bounds.Height / 2 - MeasureBounds.Height / 2);
            
            g.DrawText(sPauseText, DrawPos, GameOverTextPaint);

            //retrieve the renderer for the MenuState object.
            var basecall = RenderingProvider.Static.GetHandler(typeof(SKCanvas), typeof(MenuState), typeof(GameStateSkiaDrawParameters));
            basecall?.Render(pOwner, pRenderTarget, Source, Element);

          
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
