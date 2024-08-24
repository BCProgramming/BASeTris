using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using BaseTris;
using BASeTris.AssetManager;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeCamp.Elementizer;
using SkiaSharp;

namespace BASeTris.Blocks
{
    public class ImageBlock : NominoBlock
    {
        internal bool DoRotateTransform = false; //if true, we'll RotateTransform the image based on this blocks rotation.
        
        private Image[] RotationImagesCache = null;
        internal Image[] _RotationImages
        {
            get
            {
                if(RotationImagesCache==null)
                {
                    RotationImagesCache = (from p in _RotationImagesSK select SkiaSharp.Views.Desktop.Extensions.ToBitmap(p)).ToArray();
                }
                return RotationImagesCache;
            }
            private set
            {
                RotationImagesCache = value;
            }
        }

        internal SKImage[] _RotationImagesSK = null;
        [Obsolete]
        internal ImageAttributes[] useAttributes = null; //array of Attributes to apply to the image when drawing. Same indexing as above.
        //internal Func<ImageBlock, Image> SpecialImageFunction = RotationFunc;
        internal Func<ImageBlock, SKImage> SpecialImageFunctionSK = RotationFuncSK;


        public ImageBlock() : base()
        {
        }
        public override XElement GetXmlData(String pNodeName, Object pPersistenceData)
        {
            //get the XElement from the base class.
            XElement useResult = base.GetXmlData(pNodeName, pPersistenceData);
            //add our data here.
            useResult.Add(new XAttribute(nameof(DoRotateTransform), DoRotateTransform));

            //This might not be needed and of course can use up lots of space. The idea would be that the visual theme, applied after, would initialize all of this.

            //var ImageElement = BASeCamp.Elementizer.StandardHelper.SaveArray(_RotationImages, nameof(_RotationImages), pPersistenceData);
            //useResult.Add(ImageElement); //note that since SKImage array is our "primary" we will need to do a conversion when we try to pull this in and convert the array into SKImage instead.

            //we cannot persist the attributes, but those are pretty much unused now.

            //SpecialImageFunctionSK is another fly in the ointment. For now we'll just have it default to RotationFunkSK. It should be fine as long as a theme is applied after loading.
            return useResult;


        }
        public ImageBlock(XElement src, Object pContext):base(src,pContext)
        {
            DoRotateTransform = src.GetAttributeBool(nameof(DoRotateTransform));
            //_RotationImages = (Image[])src.ReadArray<Image>(nameof(_RotationImages), null, pContext);
            //_RotationImagesSK = _RotationImages.Select((d) => SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(d))).ToArray();



        }

        private static Image RotationFunc(ImageBlock ib)
        {

            SKImage result = RotationFuncSK(ib);
            return SkiaSharp.Views.Desktop.Extensions.ToBitmap(result);

        }
        
        private static SKImage RotationFuncSK(ImageBlock ib)
        {
            int usemodulo = ib.Rotation;
            SKImage UseImageA = ib._RotationImagesSK[MathHelper.mod(usemodulo, ib._RotationImagesSK.Length)];
            return UseImageA;
        }

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
            Style_Pixeled,
            Style_Pixeled_Outline,
            Style_Grain,
            Style_Custom,
            Style_Custom_NoColorize
        }

        public Image GummyBitmap = null;
        
        public Color _BlockColor = Color.Red;
        public Color _InnerColor = Color.White;
        public BlockStyle DisplayStyle = BlockStyle.Style_CloudBevel;

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

        public override XElement GetXmlData(string pNodeName, object pPersistenceData)
        {
            var result = base.GetXmlData(pNodeName, pPersistenceData);
            DisplayStyle = BlockStyle.Style_Chisel; //a default one. This should all be handled by a theme.... we won't even save the "gummy bitmap".
            return result;
        }
        public StandardColouredBlock():base()
        {
        }
        public StandardColouredBlock(XElement src, Object pPersistenceData) : base(src, pPersistenceData)
        {
            //no special logic....
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