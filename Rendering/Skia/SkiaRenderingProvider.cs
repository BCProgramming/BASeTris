using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace BASeTris.Rendering.Skia
{
    //Heavy WIP. Not usable, not even close.
    //Basically a SkiaSharp implementation for drawing. However, currently, other classes need to be changed as they have things like ImageAttributes and Image class instances.
    //It looks like this "conversion" is going to probably need to be full-hog- t hat is all the GDI+ stuff transformed into SkiaSharp. (SKMatrix over DrawAttributes, SKPaint instead of Brush, etc.
    //I guess that kind of makes the whole "abstraction" idea pointless, though SkiaSharp is also cross-platform so, hooray, maybe?
    //
    class SkiaRenderingProvider
    {
    }
    /// <summary>
    /// Draw Parameter information for SkiaSharp drawing.
    /// </summary>
    public class TetrisBlockDrawSkiaParameters : TetrisBlockDrawParameters
    {
        public SkiaSharp.SKCanvas g;
        public SkiaSharp.SKRect region;
        public SkiaSharp.SKPaint OverrideBrush = null;
        public SkiaSharp.SKMatrix ApplyAttributes = SkiaSharp.SKMatrix.MakeIdentity();
        public TetrisBlockDrawSkiaParameters(SkiaSharp.SKCanvas pG, SkiaSharp.SKRect pRegion, Nomino pGroupOwner, StandardSettings pSettings) : base(pGroupOwner, pSettings)
        {
            g = pG;
            region = pRegion;
        }
    }
    public class TetrisBlockSkiaRenderingHandler : StandardRenderingHandler<SkiaSharp.SKCanvas, TetrisBlock, TetrisBlockDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SkiaSharp.SKCanvas pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            Source.InvokeBeforeDraw(Element);
        }
    }

    public class TetrisImageBlockSkiaRenderingHandler : TetrisBlockSkiaRenderingHandler, IRenderingHandler<SkiaSharp.SKCanvas, ImageBlock, TetrisBlockDrawParameters>
    {
        protected virtual void NoImage()
        {
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TetrisBlock Source, TetrisBlockDrawParameters Element)
        {
            
            if(Source is ImageBlock)
                this.Render(pOwner, pRenderTarget, (ImageBlock)Source, Element);

        }

        public virtual void Render(IStateOwner pOwner,SKCanvas pRenderTarget,ImageBlock Source, TetrisBlockDrawParameters Element)
        {
            var drawparameters = Element;
            if (drawparameters is TetrisBlockDrawSkiaParameters)
            {
                var parameters = (TetrisBlockDrawSkiaParameters)drawparameters;
                base.Render(pOwner, pRenderTarget, Source, Element);
                if (Source._RotationImages == null) NoImage();
                /*if (parameters.OverrideBrush != null)
                {
                    parameters.g.FillRectangle(parameters.OverrideBrush, parameters.region);
                    return;
                }*/
                int usemodulo = Source.Rotation;
                if (usemodulo < 0) usemodulo = Source._RotationImages.Length - usemodulo;

                Image useImageA = Source._RotationImages[usemodulo % Source._RotationImages.Length];
                


                SKImage useImage = SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(useImageA));

                SKMatrix useAttributes = SKMatrix.MakeIdentity(); //unimplemented...
                //ImageAttributes useAttrib = parameters.ApplyAttributes ?? (Source.useAttributes == null ? null : Source.useAttributes[usemodulo % Source.useAttributes.Length]);
                
                float Degrees = usemodulo * 90;
                PointF Center = new PointF(parameters.region.Left + (float)(parameters.region.Width / 2), parameters.region.Top + (float)(parameters.region.Height / 2));

                SKRect DrawPosition = parameters.region;
                if (parameters.FillPercent != 1)
                {
                    float totalWidth = parameters.region.Width;
                    float totalHeight = parameters.region.Height;
                    float CenterX = DrawPosition.Width / 2 + DrawPosition.Left;
                    float CenterY = DrawPosition.Height / 2 + DrawPosition.Top;

                    float desiredWidth = totalWidth * parameters.FillPercent;
                    float desiredHeight = totalHeight * parameters.FillPercent;

                    DrawPosition = new SKRect(CenterX - desiredWidth / 2, CenterY - desiredHeight / 2, desiredWidth, desiredHeight);
                }
                SKPoint[] UsePoints = new SKPoint[] { new SKPoint(DrawPosition.Left, DrawPosition.Top),
                    new SKPoint(DrawPosition.Right, DrawPosition.Top),
                    new SKPoint(DrawPosition.Left, DrawPosition.Bottom) };
                if (Source.DoRotateTransform)
                {
                    var original = parameters.g.TotalMatrix;
                    parameters.g.ResetMatrix();
                    parameters.g.RotateDegrees(Degrees, Center.X, Center.Y);

                    parameters.g.DrawImage(useImage, DrawPosition);
                    /*parameters.g.DrawImage(useImage, UsePoints,
                        new RectangleF(0f, 0f, (float)useImage.Width, (float)useImage.Height), GraphicsUnit.Pixel, useAttrib);*/
                    //parameters.g.DrawImage(useImage,parameters.region,new RectangleF(0f,0f, (float)useImage.Width, (float)useImage.Height),GraphicsUnit.Pixel,useAttrib );
                    //parameters.g.DrawImage(useImage, parameters.region, 0f, 0f, (float)useImage.Width, (float)useImage.Height, GraphicsUnit.Pixel, useAttrib);
                }
                else
                {
                    //inset the region by the specified amount of percentage.

                    lock (useImage)
                    {
                        parameters.g.DrawImage(useImage, DrawPosition);
                        
                    }
                    //parameters.g.DrawImage(useImage, new Rectangle((int)DrawPosition.Left, (int)DrawPosition.Top, (int)DrawPosition.Width, (int)DrawPosition.Height), 0, 0, useImage.Width, useImage.Height, GraphicsUnit.Pixel, useAttrib);
                }
                
            }
        }
    }
    public class TetrisStandardColouredBlockSkiaRenderingHandler : TetrisImageBlockSkiaRenderingHandler, IRenderingHandler<SKCanvas, StandardColouredBlock, TetrisBlockDrawParameters>
    {
        public static Dictionary<String, Dictionary<SKColor, SKImage>> StandardColourBlocks = null;
        private static Dictionary<StandardColouredBlock.ColouredBlockGummyIndexData, SKImage> GummyBitmaps = new Dictionary<StandardColouredBlock.ColouredBlockGummyIndexData, SKImage>();

        private void RebuildImage(StandardColouredBlock Source)
        {
            SKImage AcquiredImage=null;
            StandardColouredBlock.ColouredBlockGummyIndexData IndexData;

            {
                IndexData = new StandardColouredBlock.ColouredBlockGummyIndexData(Source.BlockColor, Source.InnerColor, Source.InnerColor != Source.BlockColor);
                if (!GummyBitmaps.ContainsKey(IndexData))
                {
                    if (Source.DisplayStyle != StandardColouredBlock.BlockStyle.Style_Gummy)
                    {
                        AcquiredImage = GetBevelImage(Source);
                    }
                    /*else
                    {
                        AcquiredImage = GummyImage.GetGummyImage(Source.BlockColor, Source.InnerColor, new Size(256, 256));
                    }*/

                    GummyBitmaps.Add(IndexData, AcquiredImage);
                }

                Source.GummyBitmap =  SkiaSharp.Views.Desktop.Extensions.ToBitmap(GummyBitmaps[IndexData]);
                Source._RotationImages = new Image[] { Source.GummyBitmap };
                Source.CurrentImageHash = IndexData.GetHashCode();
            }
        }
        private SKImage GetBevelImage(StandardColouredBlock Source)
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
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Pixeled)
            {
                baseimage = "block_pixeled_red";
            }
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Pixeled_Outline)
            {
                baseimage = "block_pixeled_red_outline";
            }

            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Mottled)
            {
                baseimage = "block_mottled";
            }
            Size TargetSize = new Size(100, 100);
            String sBlockKey = baseimage;
            if (StandardColourBlocks == null)
            {
                StandardColourBlocks = new Dictionary<String, Dictionary<SKColor, SKImage>>();
            }

            if (!StandardColourBlocks.ContainsKey(sBlockKey))
            {
                StandardColourBlocks.Add(sBlockKey, new Dictionary<SKColor, SKImage>());
            }

            if (StandardColourBlocks[sBlockKey].Count == 0)
            {
                foreach (Color c in new Color[] { Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Blue, Color.Red, Color.Orange })
                {

                    Image GDIPImage = TetrisGame.Imageman[baseimage];
                    Bitmap GDIBit = new Bitmap(GDIPImage);
                    SKImage ski = SkiaSharp.Views.Desktop.Extensions.ToSKImage(GDIBit);
                    StandardColourBlocks[sBlockKey].Add(c.ToSKColor(), ResizeImage(RecolorImage(ski, c.ToSKColor()), TargetSize));
                }
            }

            if (!StandardColourBlocks[sBlockKey].ContainsKey(Source.BlockColor.ToSKColor()))
            {
                Image GDIPImage = TetrisGame.Imageman[baseimage];
                Bitmap GDIBit = new Bitmap(GDIPImage);
                SKImage ski = SkiaSharp.Views.Desktop.Extensions.ToSKImage(GDIBit);
                
                StandardColourBlocks[sBlockKey].Add(Source.BlockColor.ToSKColor(), ResizeImage(RecolorImage(ski, Source.BlockColor.ToSKColor()), TargetSize));
            }
            

            return StandardColourBlocks[sBlockKey][Source.BlockColor.ToSKColor()];
        }

        private SKImage RecolorImage(SKImage Source, SKColor Target)
        {
            return Source;
            /*
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
            Bitmap result = new Bitmap(Source.Width, Source.Height, PixelFormat.Format32bppPArgb);
            using (Graphics gg = Graphics.FromImage(result))
            {
                gg.Clear(Color.Transparent);
                gg.DrawImage(Source, new Rectangle(0, 0, Source.Width, Source.Height), 0, 0, Source.Width, Source.Height, GraphicsUnit.Pixel, ia);
            }

            return result;*/
        }

        private SKImage ResizeImage(SKImage Source, Size newSize)
        {
            
            SKBitmap result = new SKBitmap(newSize.Width, newSize.Height, SKColorType.Rgba8888,SKAlphaType.Premul);
            
            using (SKCanvas bgr = new SKCanvas(result))
            {
                bgr.DrawImage(Source, new SKRect(0, 0, newSize.Width, newSize.Height));
                
            }
            return SKImage.FromBitmap(result);
            
        }


        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ImageBlock Source, TetrisBlockDrawParameters Element)
        {
            if (Source is StandardColouredBlock)
                this.Render(pOwner, pRenderTarget, (StandardColouredBlock)Source, Element);
        }


        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StandardColouredBlock Source, TetrisBlockDrawParameters Element)
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
