using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.RenderElements
{

    public abstract class TetrisBlockDrawParameters
    {
        public BlockGroup GroupOwner = null;
        public float FillPercent = 1f;
        public TetrisBlockDrawParameters(BlockGroup pGroupOwner)
        {
            GroupOwner = pGroupOwner;
        }
    }

    public class TetrisBlockDrawGDIPlusParameters : TetrisBlockDrawParameters
    {
        public Graphics g;
        public RectangleF region;

        public Brush OverrideBrush = null;
        public ImageAttributes ApplyAttributes = null;


        public TetrisBlockDrawGDIPlusParameters(Graphics pG, RectangleF pRegion, BlockGroup pGroupOwner) : base(pGroupOwner)
        {
            g = pG;
            region = pRegion;

        }
    }
    
}
