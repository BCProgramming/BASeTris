using BASeCamp.Rendering;
using BASeTris.GameStates.GameHandlers.HandlerStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(DrMarioVirusAppearanceState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class PrimaryBlockAppearanceStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, DrMarioVirusAppearanceState, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, DrMarioVirusAppearanceState Source, GameStateSkiaDrawParameters Element)
        {
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.GetComposite(), Element);
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, DrMarioVirusAppearanceState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source.GetComposite(), Element);
        }
    }

}
