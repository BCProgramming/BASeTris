using BASeTris.GameStates.GameHandlers.HandlerStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates.HandlerStates
{
    public class DrMarioLevelCompleteStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, DrMarioLevelCompleteState, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, DrMarioLevelCompleteState Source, GameStateSkiaDrawParameters Element)
        {
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.GetComposite(), Element);
            DrawFadeOverlay(pRenderTarget, Element.Bounds);
            //we want a "LEVEL CLEAR TRY NEXT" thingie.
            
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
