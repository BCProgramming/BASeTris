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
        public BlockGroup Owner { get; set; }
        public virtual bool IsAnimated { get { return false; } }
        private int _GroupID = 0;
        public int GroupID { get { return _GroupID; } set { _GroupID = value; } }
        private int _Rotation = 0;
        //rotation can be set but if owned by a BlockGroup we use it's rotation.
        public virtual int Rotation
        {
            get
            {
                if(Owner!=null)
                {
                    BlockGroupEntry getbge = Owner.FindEntry(this);
                    if (getbge != null) return getbge.RotationModulo;
                }
                return _Rotation;
            }

            set { _Rotation = value; }
        }
        
        
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
        public Brush OverrideBrush = null;
        public TetrisBlockDrawParameters(Graphics pG,RectangleF pRegion,BlockGroup pGroupOwner)
        {
            g = pG;
            region = pRegion;
            GroupOwner = pGroupOwner;
        }
    }
}
