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

    public class StandardImageBackgroundRenderingHandler : BackgroundDrawRenderHandler<Graphics, StandardImageBackgroundDraw, BackgroundDrawData>
    {
        //BackgroundDrawData should be a StandardBackgroundDrawData.

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardImageBackgroundDraw Source, BackgroundDrawData Element)
        {
            var sbb = Element;
            if (sbb != null)
            {
                var Capsule = Source.Capsule;
                pRenderTarget.FillRectangle(Capsule.BackgroundBrush, Element.Bounds);
            }
            
        }

        public override void FrameProc(StandardImageBackgroundDraw pBG, BackgroundDrawData BackgroundData)
        {


            StandardImageBackgroundDrawGDICapsule dd = pBG.Capsule;
            if (dd == null) return;
            if (!dd.Movement.IsEmpty)
            {
                dd.CurrOrigin = new PointF((dd.CurrOrigin.X + dd.Movement.X) % dd._BackgroundImage.Width, (dd.CurrOrigin.Y + dd.Movement.Y) % dd._BackgroundImage.Height);
            }

            if (dd.AngleSpeed > 0) dd.CurrAngle += dd.AngleSpeed;
            dd.BackgroundBrush.ResetTransform();
            dd.BackgroundBrush.TranslateTransform(dd.CurrOrigin.X, dd.CurrOrigin.Y);
            dd.BackgroundBrush.RotateTransform(dd.CurrAngle);
        
    }
    }
    
}
