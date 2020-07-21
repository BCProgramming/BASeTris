using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.GameStates;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    


    [RenderingHandler(typeof(FieldActionGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class FieldActionStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, FieldActionGameState, GameStateSkiaDrawParameters>
    {
        SKPaint FlashBrush = new SKPaint() { ColorFilter = SKColorFilter.CreateBlendMode(SKColors.HotPink, SKBlendMode.Luminosity) };
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, FieldActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            var g = pRenderTarget;
            if (Source._BaseState != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._BaseState, Element);
                if (Source is FieldLineActionGameState linestate)
                {
                    if (linestate.FlashState)
                    {
                        g.DrawRect(Element.Bounds, FlashBrush);
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, FieldActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source._BaseState != null)
            {
                
                RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source._BaseState, Element);
            }
        }
    }
}
