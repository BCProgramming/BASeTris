using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using SkiaSharp;

namespace BASeTris.Rendering.Skia.Backgrounds
{
    [RenderingHandler(typeof(StandardImageBackgroundSkia), typeof(SKCanvas), typeof(BackgroundDrawData))]
    public class StandardImageBackgroundSkiaRenderingHandler : BackgroundDrawRenderHandler<SKCanvas, StandardImageBackgroundSkia, BackgroundDrawData>
    {
        //BackgroundDrawData should be a StandardBackgroundDrawData.
        SKRect lastBounds = SKRect.Empty;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StandardImageBackgroundSkia Source, BackgroundDrawData Element)
        {

            if (Source.Data.UnderLayer != null)
            {
                //draw "underlayer" first
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.Data.UnderLayer, Element);
            }


            var sbb = Element as SkiaBackgroundDrawData;
            if (sbb != null)
            {
                var Capsule = Source.Data;
                if (Capsule.BackgroundBrush == null || lastBounds!=sbb.Bounds)
                {
                    Capsule.ResetState(sbb.Bounds);
                }
                lastBounds = sbb.Bounds;
                pRenderTarget.DrawRect(sbb.Bounds,Capsule.BackgroundBrush);
            }

        }

    }
}
