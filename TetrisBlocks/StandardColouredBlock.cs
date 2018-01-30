using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using BaseTris;

namespace BASeTris.TetrisBlocks
{
    public class ImageBlock : TetrisBlock
    {
        protected Image[] _RotationImages; //array of images, indexed based on rotation.
        protected ImageAttributes[] useAttributes; //array of Attributes to apply to the image when drawing. Same indexing as above.
        protected virtual void NoImage(){}
        public override void DrawBlock(TetrisBlockDrawParameters parameters)
        {
            if(_RotationImages==null) NoImage();
            if (parameters.OverrideBrush != null)
            {
                parameters.g.FillRectangle(parameters.OverrideBrush, parameters.region);
                return;
            }
            int usemodulo = Rotation;
            Image useImage = _RotationImages[usemodulo % _RotationImages.Length];
            ImageAttributes useAttrib = useAttributes == null ? null : useAttributes[usemodulo % useAttributes.Length];
            parameters.g.DrawImage(useImage,new Rectangle((int)parameters.region.Left,(int)parameters.region.Top,(int)parameters.region.Width,(int)parameters.region.Height),0,0,useImage.Width,useImage.Height,GraphicsUnit.Pixel,useAttrib);

        }
    }
    public class StandardColouredBlock : ImageBlock
    {
        private Image GummyBitmap = null;
        public Color _BlockColor = Color.Red;
        public Color _InnerColor = Color.White;
        public Color BlockColor { get { return _BlockColor; } set { _BlockColor = value; } }
        public Color InnerColor {   get { return _InnerColor; } set { _InnerColor = value; } }
        public Color BlockOutline = Color.Black;
        private Brush BlockBrush = null;
        private Pen BlockPen = null;
        private static Dictionary<ColouredBlockGummyIndexData, Image> GummyBitmaps = new Dictionary<ColouredBlockGummyIndexData, Image>();

        private int CurrentImageHash = 0;
        protected override void NoImage()
        {
            RebuildImage();
        }

        public override void DrawBlock(TetrisBlockDrawParameters parameters)
        {
            ColouredBlockGummyIndexData gummydata = new ColouredBlockGummyIndexData(BlockColor, InnerColor, InnerColor != BlockColor);
            if(CurrentImageHash!=gummydata.GetHashCode())
            {
                RebuildImage();
            }
            
            base.DrawBlock(parameters);
        }

        private void RebuildImage()
        {
           
                ColouredBlockGummyIndexData gummydata = new ColouredBlockGummyIndexData(BlockColor, InnerColor, InnerColor != BlockColor);
                if (!GummyBitmaps.ContainsKey(gummydata))
                {
                    GummyBitmaps.Add(gummydata, GummyImage.GetGummyImage(BlockColor, InnerColor, new Size(256, 256)));
                }
                GummyBitmap = GummyBitmaps[gummydata];
                _RotationImages = new Image[] { GummyBitmap };
                CurrentImageHash = gummydata.GetHashCode();


        }
        
        private class ColouredBlockGummyIndexData
        {
            public readonly Color MainColor;
            private readonly Color _InnerColor;
            public Color InnerColor { get { if (hasInnerColor) return _InnerColor; return MainColor; } }
            public readonly bool hasInnerColor;
            public ColouredBlockGummyIndexData(Color pMain, Color pInner, bool hasInner)
            {
                MainColor = pMain;
                _InnerColor = pInner;
                hasInnerColor = hasInner;
                
            }

            public override int GetHashCode()
            {
                return (MainColor.A.ToString() + MainColor.R.ToString() + MainColor.G.ToString() +
                       InnerColor.A.ToString() + InnerColor.R.ToString() + InnerColor.G.ToString() + (hasInnerColor ? "Y" : "N")).GetHashCode();
                
            }
        }
    }
  
    
}
