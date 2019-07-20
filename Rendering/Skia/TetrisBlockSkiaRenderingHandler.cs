using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering.Skia
{
    public class TetrisBlockSkiaRenderingHandler : StandardRenderingHandler<SkiaSharp.SKCanvas, TetrisBlock, TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SkiaSharp.SKCanvas pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
    }
}