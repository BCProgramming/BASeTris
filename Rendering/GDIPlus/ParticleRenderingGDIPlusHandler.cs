using BASeCamp.Rendering;
using BASeTris.GameObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.GDIPlus
{
    //[RenderingHandler(typeof(TetrisBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    //public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics, TetrisBlock, TetrisBlockDrawParameters>
    [RenderingHandler(typeof(List<Particle>), typeof(Graphics), typeof(BaseDrawParameters))]
    public class ParticleRenderingGDIPlusHandler : StandardRenderingHandler<Graphics, List<Particle>, BaseDrawParameters>
    {
        
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, List<Particle> Source, BaseDrawParameters Element)
        {
            foreach(var iterate in Source)
            {
                using (Pen DrawPen = new Pen(iterate.Color, 1))
                {
                    pRenderTarget.DrawRectangle(DrawPen, new Rectangle(iterate.Position, new Size(1, 1)));

                }
            }
        }
    }
}
