using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.TetrisBlocks
{
    public abstract class TetrisBlock
    {
        public virtual bool IsAnimated { get { return false; } }
        public abstract void DrawBlock(TetrisBlockDrawParameters parameters);
        public virtual void AnimateFrame()
        {
            //nothing by default. Well, for now anyway....
        }
    }
    public class TetrisBlockDrawParameters
    {
        public Graphics g;
        public RectangleF region;
        public BlockGroup GroupOwner = null;
        public TetrisBlockDrawParameters(Graphics pG,RectangleF pRegion,BlockGroup pGroupOwner)
        {
            g = pG;
            region = pRegion;
            GroupOwner = pGroupOwner;
        }
    }
}
