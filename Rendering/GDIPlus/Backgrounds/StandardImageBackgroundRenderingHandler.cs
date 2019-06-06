using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;

namespace BASeTris.Rendering.GDIPlus.Backgrounds
{

    public class StandardImageBackgroundRenderingHandler : BackgroundDrawRenderHandler<Graphics, StandardImageBackgroundDraw, BackgroundDrawData>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardImageBackgroundDraw Source, BackgroundDrawData Element)
        {
            throw new NotImplementedException();
        }

    }
    
}
