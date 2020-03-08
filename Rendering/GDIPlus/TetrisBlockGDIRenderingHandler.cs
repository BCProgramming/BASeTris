using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(TetrisBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics,TetrisBlock,TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
      
    }
}