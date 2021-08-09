using BASeCamp.Rendering;
using BASeTris.GameStates.GameHandlers.HandlerStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates.HandlerStates
{
    [RenderingHandler(typeof(DrMarioLevelCompleteState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class DrMarioLevelCompleteStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, DrMarioLevelCompleteState, GameStateSkiaDrawParameters>
    {
        SKBitmap CompleteBox = null;
        SKPaint CompletionTextPaint = null;
        SKPaint CompletionTextPaintShadow = null;
        private bool Initialized = false;
        String[][] CompletionLines = new string[][] { new string[]{ "LEVEL COMPLETE", "TRY NEXT" }, new string[]{ "LEVEL COMPLETE", "-TRY NEXT-" } };
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, DrMarioLevelCompleteState Source, GameStateSkiaDrawParameters Element)
        {
            if(!Initialized)
            {
                Initialized = true;
                CompleteBox = TetrisGame.Imageman.GetSKBitmap("Level_Complete_Box");
                CompletionTextPaint = new SKPaint();
                CompletionTextPaint.ApplySizedFont(pOwner, 24, SKColors.White);
                CompletionTextPaint.Color = SKColors.White;
                CompletionTextPaintShadow = new SKPaint();
                CompletionTextPaintShadow.ApplySizedFont(pOwner, 24, SKColors.Black);
            }
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.GetComposite(), Element);
            DrawFadeOverlay(pRenderTarget, Element.Bounds);
            //we want a "LEVEL CLEAR TRY NEXT" thingie.
            var BoxBounds = new SKRect(Element.Bounds.Left, Element.Bounds.Top, Element.Bounds.Left + Element.Bounds.Width, Element.Bounds.Width + Element.Bounds.Top);
            pRenderTarget.DrawBitmap(CompleteBox, BoxBounds);

            SKPoint InitialTextPos = new SKPoint(BoxBounds.Left + (float)((CompletionTextPaint.TextSize)*(pOwner.ScaleFactor * 5)), BoxBounds.Top+ (float)((CompletionTextPaint.TextSize) * (pOwner.ScaleFactor * 5)));
            float CurrentY = InitialTextPos.Y;
            var ChosenCompleteLine = CompletionLines[DateTime.Now.Second % 2];
            foreach(String line in ChosenCompleteLine)
            {
                float sWidth = CompletionTextPaint.MeasureText(line);
                //determine vertical location in boxBounds.
                SKPoint TextDrawPosition = new SKPoint(BoxBounds.Left + (BoxBounds.Width / 2) - (sWidth / 2),CurrentY);
                pRenderTarget.DrawText(line, new SKPoint((float)(TextDrawPosition.X+(5f*pOwner.ScaleFactor)),(float)(TextDrawPosition.Y+(5f*pOwner.ScaleFactor))), CompletionTextPaintShadow);
                pRenderTarget.DrawText(line, TextDrawPosition, CompletionTextPaintShadow);
                CurrentY += CompletionTextPaint.TextSize;

            }
            
            
            
            
        }
        SKPaint fadeBrush = new SKPaint() { Color = new SKColor(0, 0, 0, 200) };
        private void DrawFadeOverlay(SKCanvas g, SKRect Bounds)
        {
            g.DrawRect(Bounds, fadeBrush);
        }
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, DrMarioLevelCompleteState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source.GetComposite(), Element);
        }
    }
}
