using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseTris;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering.GDIPlus
{
    public class RenderingProvider : IRenderingProvider 
    {
        public static RenderingProvider Static = new RenderingProvider();
        private Dictionary<Type, Dictionary<Type, IRenderingHandler>> handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler>>();
        bool InitProviderDictionary = false; 
        public IRenderingHandler GetHandler(Type ClassType, Type DrawType, Type DrawDataType)
        {
            if(!InitProviderDictionary)
            {
                InitProviderDictionary = true;
                handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler>>();


                handlerLookup.Add(typeof(Graphics), new Dictionary<Type, IRenderingHandler>()
                  { { typeof(TetrisBlock),new TetrisBlockRenderingHandler()},
                    { typeof(ImageBlock),new TetrisImageBlockRenderingHandler()},
                    { typeof(StandardColouredBlock),new TetrisStandardColouredBlockRenderingHandler() }
                    });

                
            }
            if(handlerLookup.ContainsKey(ClassType))
            {
                if(handlerLookup[ClassType].ContainsKey(DrawType))
                {
                    return handlerLookup[ClassType][DrawType];
                }
            }
            return null;
        }
        public void DrawElement(IStateOwner pOwner,Object Target,Object Element,Object ElementData)
        {
            var Handler = GetHandler(Target.GetType(), Element.GetType(), ElementData.GetType());
            Handler.Render(pOwner,Target,Element,ElementData);
        }
    }
    public class TetrisBlockRenderingHandler : StandardRenderingHandler<Graphics,TetrisBlock,TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
      
    }

    public class TetrisImageBlockRenderingHandler : TetrisBlockRenderingHandler, IRenderingHandler<Graphics,ImageBlock,TetrisBlockDrawParameters>
    {
        protected virtual void NoImage()
        {
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            if(Source is ImageBlock)
                this.Render(pOwner,pRenderTarget,(ImageBlock)Source,Element);
        }

        public virtual void Render(IStateOwner pOwner, Graphics pRenderTarget, ImageBlock Source, TetrisBlockDrawParameters Element)
        {
            var drawparameters = Element;
            if (drawparameters is TetrisBlockDrawGDIPlusParameters)
            {
                var parameters = (TetrisBlockDrawGDIPlusParameters)drawparameters;
                base.Render(pOwner,pRenderTarget,Source,Element);
                if (Source._RotationImages == null) NoImage();
                /*if (parameters.OverrideBrush != null)
                {
                    parameters.g.FillRectangle(parameters.OverrideBrush, parameters.region);
                    return;
                }*/
                int usemodulo = Source.Rotation;
                if (usemodulo < 0) usemodulo = Source._RotationImages.Length - usemodulo;
                Image useImage = Source._RotationImages[usemodulo % Source._RotationImages.Length];
                ImageAttributes useAttrib = parameters.ApplyAttributes ?? (Source.useAttributes == null ? null : Source.useAttributes[usemodulo % Source.useAttributes.Length]);

                float Degrees = usemodulo * 90;
                PointF Center = new PointF(parameters.region.Left + (float)(parameters.region.Width / 2), parameters.region.Top + (float)(parameters.region.Height / 2));

                RectangleF DrawPosition = parameters.region;
                if (parameters.FillPercent != 1)
                {
                    float totalWidth = parameters.region.Width;
                    float totalHeight = parameters.region.Height;
                    float CenterX = DrawPosition.Width / 2 + DrawPosition.Left;
                    float CenterY = DrawPosition.Height / 2 + DrawPosition.Top;

                    float desiredWidth = totalWidth * parameters.FillPercent;
                    float desiredHeight = totalHeight * parameters.FillPercent;

                    DrawPosition = new RectangleF(CenterX - desiredWidth / 2, CenterY - desiredHeight / 2, desiredWidth, desiredHeight);
                }
                PointF[] UsePoints = new PointF[] { new PointF(DrawPosition.Left, DrawPosition.Top), new PointF(DrawPosition.Right, DrawPosition.Top), new PointF(DrawPosition.Left, DrawPosition.Bottom) };
                if (Source.DoRotateTransform)
                {
                    var original = parameters.g.Transform;
                    parameters.g.TranslateTransform(Center.X, Center.Y);
                    parameters.g.RotateTransform(Degrees);
                    parameters.g.TranslateTransform(-Center.X, -Center.Y);
                    
                    parameters.g.DrawImage(useImage, UsePoints,
                        new RectangleF(0f, 0f, (float)useImage.Width, (float)useImage.Height), GraphicsUnit.Pixel, useAttrib);
                    //parameters.g.DrawImage(useImage,parameters.region,new RectangleF(0f,0f, (float)useImage.Width, (float)useImage.Height),GraphicsUnit.Pixel,useAttrib );
                    //parameters.g.DrawImage(useImage, parameters.region, 0f, 0f, (float)useImage.Width, (float)useImage.Height, GraphicsUnit.Pixel, useAttrib);
                    parameters.g.Transform = original;
                }
                else
                {
                    //inset the region by the specified amount of percentage.
                   

                    parameters.g.DrawImage(useImage, UsePoints,
                        new RectangleF(0f, 0f, (float)useImage.Width, (float)useImage.Height), GraphicsUnit.Pixel, useAttrib);
                    //parameters.g.DrawImage(useImage, new Rectangle((int)DrawPosition.Left, (int)DrawPosition.Top, (int)DrawPosition.Width, (int)DrawPosition.Height), 0, 0, useImage.Width, useImage.Height, GraphicsUnit.Pixel, useAttrib);
                }
            }
        }
    }

    public class TetrisStandardColouredBlockRenderingHandler : TetrisImageBlockRenderingHandler, IRenderingHandler<Graphics, StandardColouredBlock, TetrisBlockDrawParameters>
    {
        public static Dictionary<String, Dictionary<Color, Image>> StandardColourBlocks = null;
        private static Dictionary<StandardColouredBlock.ColouredBlockGummyIndexData, Image> GummyBitmaps = new Dictionary<StandardColouredBlock.ColouredBlockGummyIndexData, Image>();
        
        private void RebuildImage(StandardColouredBlock Source)
        {
            Image AcquiredImage;
            StandardColouredBlock.ColouredBlockGummyIndexData IndexData;

            {
                IndexData = new StandardColouredBlock.ColouredBlockGummyIndexData(Source.BlockColor, Source.InnerColor, Source.InnerColor != Source.BlockColor);
                if (!GummyBitmaps.ContainsKey(IndexData))
                {
                    if (Source.DisplayStyle != StandardColouredBlock.BlockStyle.Style_Gummy)
                    {
                        AcquiredImage = GetBevelImage(Source);
                    }
                    else
                    {
                        AcquiredImage = GummyImage.GetGummyImage(Source.BlockColor, Source.InnerColor, new Size(256, 256));
                    }

                    GummyBitmaps.Add(IndexData, AcquiredImage);
                }

                Source.GummyBitmap = GummyBitmaps[IndexData];
                Source._RotationImages = new Image[] { Source.GummyBitmap };
                Source.CurrentImageHash = IndexData.GetHashCode();
            }
        }
        private Image GetBevelImage(StandardColouredBlock Source)
        {
            String baseimage = "block_lightbevel_red";
            if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_CloudBevel)
                baseimage = "block_lightbevel_red";
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Shine)
            {
                baseimage = "block_shine_red";
            }
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_HardBevel)
                baseimage = "block_std_red";
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Chisel)
                baseimage = "block_chisel_red";
            Size TargetSize = new Size(100, 100);
            String sBlockKey = baseimage;
            if (StandardColourBlocks == null)
            {
                StandardColourBlocks = new Dictionary<String, Dictionary<Color, Image>>();
            }

            if (!StandardColourBlocks.ContainsKey(sBlockKey))
            {
                StandardColourBlocks.Add(sBlockKey, new Dictionary<Color, Image>());
            }

            if (StandardColourBlocks[sBlockKey].Count == 0)
            {
                foreach (Color c in new Color[] { Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Blue, Color.Red, Color.Orange })
                {
                    StandardColourBlocks[sBlockKey].Add(c, ResizeImage(RecolorImage(TetrisGame.Imageman[baseimage], c), TargetSize));
                }
            }

            if (!StandardColourBlocks[sBlockKey].ContainsKey(Source.BlockColor))
            {
                StandardColourBlocks[sBlockKey].Add(Source.BlockColor, ResizeImage(RecolorImage(TetrisGame.Imageman[baseimage], Source.BlockColor), TargetSize));
            }


            return StandardColourBlocks[sBlockKey][Source.BlockColor];
        }

        private Image RecolorImage(Image Source, Color Target)
        {
            float NormalizedR = (float)Target.R / 255;
            float NormalizedG = (float)Target.G / 255;
            float NormalizedB = (float)Target.B / 255;
            float NormalizedA = (float)Target.A / 255;

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
            Bitmap result = new Bitmap(Source.Width, Source.Height,PixelFormat.Format32bppPArgb);
            using (Graphics gg = Graphics.FromImage(result))
            {
                gg.Clear(Color.Transparent);
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

        
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, ImageBlock Source, TetrisBlockDrawParameters Element)
        {
            if(Source is StandardColouredBlock)
                this.Render(pOwner,pRenderTarget,(StandardColouredBlock)Source,Element);
        }


        public void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardColouredBlock Source, TetrisBlockDrawParameters Element)
        {
            if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Custom)
            {
                base.Render(pOwner, pRenderTarget, Source, Element);
            }
            else
            {
                StandardColouredBlock.ColouredBlockGummyIndexData gummydata = new StandardColouredBlock.ColouredBlockGummyIndexData(Source.BlockColor, Source.InnerColor, Source.InnerColor != Source.BlockColor);
                if (Source.CurrentImageHash != gummydata.GetHashCode())
                {
                    RebuildImage(Source);
                }
                base.Render(pOwner, pRenderTarget, Source, Element);
            }
        }
    }
}
