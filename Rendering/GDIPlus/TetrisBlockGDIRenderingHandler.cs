using System.Drawing;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering.GDIPlus
{
    public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics,TetrisBlock,TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
      
    }
}