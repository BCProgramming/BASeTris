using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseTris;

namespace BASeTris.TetrisBlocks
{
    public class StandardColouredBlock : TetrisBlock
    {
        private Image GummyBitmap = null;
        public Color _BlockColor = Color.Red;
        public Color _InnerColor = Color.White;
        public Color BlockColor { get { return _BlockColor; } set { _BlockColor = value;GummyBitmap = null; } }
        public Color InnerColor {   get { return _InnerColor; } set { _InnerColor = value;GummyBitmap = null; } }
        public Color BlockOutline = Color.Black;
        private Brush BlockBrush = null;
        private Pen BlockPen = null;
        private static Dictionary<ColouredBlockGummyIndexData, Image> GummyBitmaps = new Dictionary<ColouredBlockGummyIndexData, Image>();
        public override void DrawBlock(TetrisBlockDrawParameters parameters)
        {
            ColouredBlockGummyIndexData gummydata = new ColouredBlockGummyIndexData(BlockColor, InnerColor, InnerColor != BlockColor);
            if(GummyBitmap==null)
            {
                if (!GummyBitmaps.ContainsKey(gummydata))
                {
                    GummyBitmaps.Add(gummydata,GummyImage.GetGummyImage(BlockColor,InnerColor, new Size(256, 256)));
                }
                GummyBitmap = GummyBitmaps[gummydata];
            }
            parameters.g.DrawImage(GummyBitmap,parameters.region);
            if (BlockPen == null) BlockPen = new Pen(BlockOutline, 2f);
            parameters.g.DrawRectangle(BlockPen,parameters.region.Left,parameters.region.Top,parameters.region.Width,parameters.region.Height);
            /*if (BlockBrush == null) BlockBrush = new SolidBrush(BlockColor);
            
            parameters.g.FillRectangle(BlockBrush,parameters.region);
            
            parameters.g.DrawRectangle(BlockPen,new Rectangle((int)parameters.region.Left, (int)parameters.region.Top, (int)parameters.region.Width,(int)parameters.region.Height));*/
        }
        private class ColouredBlockGummyIndexData
        {
            public Color MainColor;
            public Color InnerColor;
            public bool hasInnerColor;
            public ColouredBlockGummyIndexData(Color pMain, Color pInner, bool hasInner)
            {
                MainColor = pMain;
                InnerColor = pInner;
                hasInner = hasInnerColor;
            }

            public override int GetHashCode()
            {
                return MainColor.GetHashCode() ^ InnerColor.GetHashCode() ^ hasInnerColor.GetHashCode();
            }
        }
    }
  
    
}
