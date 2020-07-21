using System.Drawing;
using System.Drawing.Imaging;
using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(ImageBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    public class TetrisImageBlockGDIRenderingHandler : TetrisBlockGDIRenderingHandler, IRenderingHandler<Graphics,ImageBlock,TetrisBlockDrawParameters>
    {
        protected virtual void NoImage()
        {
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, NominoBlock Source, TetrisBlockDrawParameters Element)
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
                //if (usemodulo < 0) usemodulo = Source._RotationImages.Length - usemodulo;

                Image useImage = null;

                if (Source.SpecialImageFunctionSK != null)
                {
                    if (Source is LineSeriesMasterBlock)
                    {
                        ;
                    }
                    useImage =  SkiaSharp.Views.Desktop.Extensions.ToBitmap(Source.SpecialImageFunctionSK(Source));
                }
                else
                {
                    int usemoduloin = Source.Rotation;
                    if (usemoduloin != 0) {; }
                    //if (usemodulo < 0) usemodulo = Source._RotationImages.Length - Math.Abs(usemodulo);

                    useImage = Source._RotationImages[MathHelper.mod(usemoduloin, Source._RotationImages.Length)];
                }



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
                    parameters.g.Transform = original;
                }
                else
                {
                    //inset the region by the specified amount of percentage.

                    lock (useImage)
                    {
                        parameters.g.DrawImage(useImage, UsePoints,
                            new RectangleF(0f, 0f, (float)useImage.Width, (float)useImage.Height), GraphicsUnit.Pixel, useAttrib);
                    }
                    //parameters.g.DrawImage(useImage, new Rectangle((int)DrawPosition.Left, (int)DrawPosition.Top, (int)DrawPosition.Width, (int)DrawPosition.Height), 0, 0, useImage.Width, useImage.Height, GraphicsUnit.Pixel, useAttrib);
                }
            }
        }
    }
}