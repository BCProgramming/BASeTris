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
using BASeTris.AssetManager;
using BASeTris.Rendering.RenderElements;

namespace BASeTris.TetrisBlocks
{
    public class ImageBlock : TetrisBlock
    {
        internal bool DoRotateTransform = false; //if true, we'll RotateTransform the image based on this blocks rotation.
        internal Image[] _RotationImages; //array of images, indexed based on rotation.
        internal ImageAttributes[] useAttributes; //array of Attributes to apply to the image when drawing. Same indexing as above.

        
        protected virtual void NoImage()
        {
        }

   }

    public class StandardColouredBlock : ImageBlock
    {
        public enum BlockStyle
        {
            Style_Gummy,
            Style_CloudBevel,
            Style_HardBevel,
            Style_Chisel,
            Style_Shine,
            Style_Mottled,
            Style_Custom
        }

        public Image GummyBitmap = null;
        
        public Color _BlockColor = Color.Red;
        public Color _InnerColor = Color.White;
        public BlockStyle DisplayStyle = BlockStyle.Style_Gummy;

        public Color BlockColor
        {
            get { return _BlockColor; }
            set { _BlockColor = value; }
        }

        public Color InnerColor
        {
            get { return _InnerColor; }
            set { _InnerColor = value; }
        }

        public Color BlockOutline = Color.Black;
        private Brush BlockBrush = null;
        private Pen BlockPen = null;
        private static Dictionary<ColouredBlockGummyIndexData, Image> GummyBitmaps = new Dictionary<ColouredBlockGummyIndexData, Image>();

        internal int CurrentImageHash = 0;

        protected override void NoImage()
        {
            RebuildImage();
        }

    

        private void RebuildImage()
        {
            Image AcquiredImage;
            ColouredBlockGummyIndexData IndexData;

            {
                IndexData = new ColouredBlockGummyIndexData(BlockColor, InnerColor, InnerColor != BlockColor);
                if (!GummyBitmaps.ContainsKey(IndexData))
                {
                    if (DisplayStyle != BlockStyle.Style_Gummy)
                    {
                        AcquiredImage = GetBevelImage();
                    }
                    else 
                    {
                        AcquiredImage = GummyImage.GetGummyImage(BlockColor, InnerColor, new Size(256, 256));
                    }

                    GummyBitmaps.Add(IndexData, AcquiredImage);
                }

                GummyBitmap = GummyBitmaps[IndexData];
                _RotationImages = new Image[] {GummyBitmap};
                CurrentImageHash = IndexData.GetHashCode();
            }
        }

        //static Dictionary<Color, Image> StandardColourBlocks = null;
        static Dictionary<BlockStyle, Dictionary<Color, Image>> StandardColourBlocks = null;

        private Image GetBevelImage()
        {
            String baseimage = "block_lightbevel_red";
            if (DisplayStyle == BlockStyle.Style_CloudBevel)
                baseimage = "block_lightbevel_red";
            else if (DisplayStyle == BlockStyle.Style_Shine)
            {
                baseimage = "block_shine_red";
            }
            else if (DisplayStyle == BlockStyle.Style_HardBevel)
                baseimage = "block_std_red";
            else if (DisplayStyle == BlockStyle.Style_Chisel)
                baseimage = "block_chisel_red";

            Size TargetSize = new Size(100, 100);
            if (StandardColourBlocks == null)
            {
                StandardColourBlocks = new Dictionary<BlockStyle, Dictionary<Color, Image>>();
            }

            if (!StandardColourBlocks.ContainsKey(DisplayStyle))
            {
                StandardColourBlocks.Add(DisplayStyle, new Dictionary<Color, Image>());
            }

            if (StandardColourBlocks[DisplayStyle].Count == 0)
            {
                foreach (Color c in new Color[] {Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Blue, Color.Red, Color.Orange})
                {
                    StandardColourBlocks[DisplayStyle].Add(c, ResizeImage(RecolorImage(TetrisGame.Imageman[baseimage], c), TargetSize));
                }
            }

            if (!StandardColourBlocks[DisplayStyle].ContainsKey(BlockColor))
            {
                StandardColourBlocks[DisplayStyle].Add(BlockColor, ResizeImage(RecolorImage(TetrisGame.Imageman[baseimage], BlockColor), TargetSize));
            }


            return StandardColourBlocks[DisplayStyle][BlockColor];
        }

        public static Image RecolorImage(Image Source, Color Target)
        {
            float NormalizedR = (float) Target.R / 255;
            float NormalizedG = (float) Target.G / 255;
            float NormalizedB = (float) Target.B / 255;
            float NormalizedA = (float) Target.A / 255;

            //input image is assumed to use RED as it's dominant colour!
            float[][] mat = new float[][]
            {
                new float[] {NormalizedR, NormalizedG, NormalizedB, NormalizedA, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1},
            };
            ColorMatrix cm = new ColorMatrix(mat);
            ImageAttributes ia = new ImageAttributes();
            ia.SetColorMatrix(cm);
            Bitmap result = new Bitmap(Source.Width, Source.Height, PixelFormat.Format32bppPArgb);
            using (Graphics gg = Graphics.FromImage(result))
            {
                gg.Clear(Color.Transparent);
                lock(Source)
                    gg.DrawImage(Source, new Rectangle(0, 0, Source.Width, Source.Height), 0, 0, Source.Width, Source.Height, GraphicsUnit.Pixel, ia);
            }

            return result;
        }

        private Image ResizeImage(Image Source, Size newSize)
        {
            Bitmap result = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppPArgb);
            using (Graphics bgr = Graphics.FromImage(result))
            {
                bgr.DrawImage(Source, 0, 0, newSize.Width, newSize.Height);
            }

            return result;
        }

        internal class ColouredBlockGummyIndexData
        {
            public readonly Color MainColor;
            private readonly Color _InnerColor;

            public Color InnerColor
            {
                get
                {
                    if (hasInnerColor) return _InnerColor;
                    return MainColor;
                }
            }

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