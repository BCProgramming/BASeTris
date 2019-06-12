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

    public class StandardImageBackgroundRenderingHandler : BackgroundDrawRenderHandler<Graphics, StandardImageBackground, BackgroundDrawData>
    {
        //BackgroundDrawData should be a StandardBackgroundDrawData.

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardImageBackground Source, BackgroundDrawData Element)
        {
            
            var sbb = Element as GDIBackgroundDrawData;
            if (sbb != null)
            {
               var Capsule = Source.Capsule;
               if(Capsule.BackgroundBrush==null)
                {
                    Capsule.ResetState();
                }
                pRenderTarget.FillRectangle(Capsule.BackgroundBrush, sbb.Bounds);
            }
            
        }

    }
    
}
