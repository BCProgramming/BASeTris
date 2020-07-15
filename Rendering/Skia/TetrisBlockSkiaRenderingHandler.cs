using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    [RenderingHandler(typeof(NominoBlock),typeof(SKCanvas),typeof(TetrisBlockDrawParameters))]
    public class TetrisBlockSkiaRenderingHandler : StandardRenderingHandler<SkiaSharp.SKCanvas, NominoBlock, TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SkiaSharp.SKCanvas pRenderTarget, NominoBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
    }
}