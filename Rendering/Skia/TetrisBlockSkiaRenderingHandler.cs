using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    [RenderingHandler(typeof(TetrisBlock),typeof(SKCanvas),typeof(TetrisBlockDrawParameters))]
    public class TetrisBlockSkiaRenderingHandler : StandardRenderingHandler<SkiaSharp.SKCanvas, TetrisBlock, TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SkiaSharp.SKCanvas pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
    }
}