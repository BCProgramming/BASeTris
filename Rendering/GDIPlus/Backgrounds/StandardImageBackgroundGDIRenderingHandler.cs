using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using BASeTris.BackgroundDrawers;

namespace BASeTris.Rendering.GDIPlus.Backgrounds
{
    [RenderingHandler(typeof(StandardImageBackgroundGDI), typeof(Graphics), typeof(BackgroundDrawData))]
    public class StandardImageBackgroundGDIRenderingHandler : BackgroundDrawRenderHandler<Graphics, StandardImageBackgroundGDI, BackgroundDrawData>
    {
        //BackgroundDrawData should be a StandardBackgroundDrawData.

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardImageBackgroundGDI Source, BackgroundDrawData Element)
        {
            
            var sbb = Element as GDIBackgroundDrawData;
            if (sbb != null)
            {
               var Capsule = Source.Data;
               if(Capsule.BackgroundBrush==null)
                {
                    Capsule.ResetState();
                }
                pRenderTarget.FillRectangle(Capsule.BackgroundBrush, sbb.Bounds);
            }
            
        }

    }
    
}
