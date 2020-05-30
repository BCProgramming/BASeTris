using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{

    [RenderingHandler(typeof(ImageBlock), typeof(SKCanvas),typeof(TetrisBlockDrawParameters))]

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
                int usemodulo = Source.Rotation;
                if (usemodulo < 0) usemodulo = Source._RotationImages.Length - usemodulo;

                Image useImageA = Source._RotationImages[usemodulo % Source._RotationImages.Length];
                SKImage useImage = SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(useImageA));
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

                    DrawPosition = new SKRect(CenterX - desiredWidth / 2, CenterY - desiredHeight / 2, (CenterX - desiredWidth / 2) + desiredWidth, CenterY - desiredHeight / 2 + desiredHeight);
                }
                SKPoint[] UsePoints = new SKPoint[] { new SKPoint(DrawPosition.Left, DrawPosition.Top),
                    new SKPoint(DrawPosition.Right, DrawPosition.Top),
                    new SKPoint(DrawPosition.Left, DrawPosition.Bottom) };
                if (Source.DoRotateTransform)
                {
                    var current = parameters.g.TotalMatrix;
                    parameters.g.ResetMatrix();
                    parameters.g.Concat(ref parameters.ApplyAttributes);
                    parameters.g.RotateDegrees(Degrees, Center.X, Center.Y);
                    if(parameters.ColorFilter==null)
                        parameters.g.DrawImage(useImage, DrawPosition );
                   else
                    {
                        parameters.g.DrawImage(useImage, DrawPosition,new SKPaint() { ColorFilter = parameters.ColorFilter});
                    }
                    parameters.g.SetMatrix(current);
                }
                else
                {

                    lock (useImage)
                    {
                        var current = parameters.g.TotalMatrix;
                        parameters.g.Concat(ref parameters.ApplyAttributes);
                        if(parameters.ColorFilter!=null)
                            parameters.g.DrawImage(useImage, DrawPosition, new SKPaint() { ColorFilter = parameters.ColorFilter });
                        else
                            parameters.g.DrawImage(useImage, DrawPosition);

                        parameters.g.SetMatrix(current);
                        
                    }
                }
                
            }
        }
    }
}