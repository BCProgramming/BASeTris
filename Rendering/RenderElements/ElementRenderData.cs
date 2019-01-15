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
        public StandardSettings Settings;
        public TetrisBlockDrawParameters(BlockGroup pGroupOwner,StandardSettings pSettings)
        {
            GroupOwner = pGroupOwner;
            Settings = pSettings;
        }
    }

    public class TetrisBlockDrawGDIPlusParameters : TetrisBlockDrawParameters
    {
        public Graphics g;
        public RectangleF region;

        public Brush OverrideBrush = null;
        public ImageAttributes ApplyAttributes = null;


        public TetrisBlockDrawGDIPlusParameters(Graphics pG, RectangleF pRegion, BlockGroup pGroupOwner, StandardSettings pSettings) : base(pGroupOwner,pSettings)
        {
            g = pG;
            region = pRegion;

        }
    }
    
}
