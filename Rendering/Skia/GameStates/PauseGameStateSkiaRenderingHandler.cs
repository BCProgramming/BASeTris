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
    public class PauseGameStateSkiaRenderingHandler : MenuStateSkiaRenderingHandler // StandardStateRenderingHandler<SKCanvas, PauseGameState, GameStateSkiaDrawParameters>
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
                    pfi.XSpeed = 0;
                    pfi.YSpeed = (float)(rgen.NextDouble() * 5);
                    pfi.AngleSpeed = 0; //(float)(rgen.NextDouble() * 20) - 10;
                    pfi.XPosition = (float)rgen.NextDouble() * (float)Areause.Width;
                    pfi.YPosition = (float)rgen.NextDouble() * (float)Areause.Height;
                    Source.FallImages.Add(pfi);
                }
            }
        }
        static SKPaint GrayBG = new SKPaint() { Color = SKColors.LightBlue,BlendMode = SKBlendMode.HardLight };
        private static SKPaint GameOverTextPaint = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source is PauseGameState pgs)
            {
                Render(pOwner, pRenderTarget, pgs, Element);
            }
            else
            {
                base.Render(pOwner, pRenderTarget, Source, Element);
            }
        }

        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, PauseGameState Source, GameStateSkiaDrawParameters Element)
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
            
            if (Source.PauseGamePlayerState != null)
            {
                RenderingProvider.Static.DrawElement(Source, pRenderTarget, Source.PauseGamePlayerState, Element);
            }
            g.DrawRect(Bounds, GrayBG);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }
            g.ResetMatrix();



            SKPoint DrawPos = new SKPoint(Bounds.Width / 2 - MeasureBounds.Width / 2, Bounds.Height / 2 - MeasureBounds.Height / 2);
            
            g.DrawText(sPauseText, DrawPos, GameOverTextPaint);

            //retrieve the renderer for the MenuState object.

            //var basecall = RenderingProvider.Static.GetHandler(typeof(SKCanvas), typeof(MenuState), typeof(GameStateSkiaDrawParameters));
            base.Render(pOwner, pRenderTarget, Source, Element);
            //basecall?.Render(pOwner, pRenderTarget, Source, Element);


        }
        public override float DrawHeader(IStateOwner pOwner, MenuState Source, SKCanvas Target, SKRect Bounds)
        {
            return (float)Bounds.Height * 0.6f;
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source is PauseGameState pgs)
            {
                RenderStats(pOwner, pRenderTarget, pgs, Element);
            }
            else
            {
                base.Render(pOwner, pRenderTarget, Source, Element);
            }
        }
        public void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, PauseGameState Source, GameStateSkiaDrawParameters Element)
        {
            //delegate...
            var PausedState = Source.PausedState;
            if (PausedState != null)
            {
                RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, PausedState, Element);
            }
        }


    }
}
