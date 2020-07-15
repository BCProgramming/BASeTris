using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(NominoBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics,NominoBlock,TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, NominoBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
      
    }
}