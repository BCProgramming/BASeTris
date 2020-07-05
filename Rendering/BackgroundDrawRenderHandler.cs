using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;

namespace BASeTris.Rendering
{
    public class BackgroundDrawParameters : Rendering.GDIPlus.BaseDrawParameters
    {
        public BackgroundDrawParameters(RectangleF pRect) :base(pRect)
        {

        }
    }
    //base Background rendering handler implementation.
    public abstract class BackgroundDrawRenderHandler<TARGET, BG, BDD> : StandardRenderingHandler<TARGET, BG, BDD> where BDD : BackgroundDrawData where BG : Background
    {

        
    }
}
