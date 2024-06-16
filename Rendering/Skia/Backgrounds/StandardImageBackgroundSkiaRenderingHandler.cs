using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using SkiaSharp;

namespace BASeTris.Rendering.Skia.Backgrounds
{
    [RenderingHandler(typeof(StandardImageBackgroundSkia), typeof(SKCanvas), typeof(BackgroundDrawData))]
    public class StandardImageBackgroundSkiaRenderingHandler : BackgroundDrawRenderHandler<SKCanvas, StandardImageBackgroundSkia, BackgroundDrawData>
    {
        //BackgroundDrawData should be a StandardBackgroundDrawData.
        SKRect lastBounds = SKRect.Empty;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StandardImageBackgroundSkia Source, BackgroundDrawData Element)
        {
            if (Source.Data == null) return;
            
            if (Source.Underlayer != null)
            {
                //draw "underlayer" first
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.Underlayer, Element);
            }
            
            var sbb = Element as SkiaBackgroundDrawData;
            
            if (sbb != null)
            {
                var Capsule = Source.Data;
                if (Capsule.BackgroundBrush == null || lastBounds!=sbb.Bounds)
                {
                    Capsule.ResetState(sbb.Bounds);
                }
                lastBounds = sbb.Bounds;
                using (SKAutoCanvasRestore sr = new SKAutoCanvasRestore(pRenderTarget, true))
                {
                    
                    pRenderTarget.Scale((float)(Source.Data.Scale));
                    pRenderTarget.DrawRect(sbb.Bounds, Capsule.BackgroundBrush);
                }
            }

            if (Source.Overlayer != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.Overlayer, Element);
            }

        }

    }



    
    [RenderingHandler(typeof(StandardImageBackgroundBorderSkia), typeof(SKCanvas), typeof(BackgroundDrawData))]
    public class StandardImageBackgroundBorderSkiaRenderingHandler : BackgroundDrawRenderHandler<SKCanvas, StandardImageBackgroundBorderSkia, BackgroundDrawData>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StandardImageBackgroundBorderSkia Source, BackgroundDrawData Element)
        {
            SkiaBackgroundDrawData sbdd = Element as SkiaBackgroundDrawData;
            PreparePaints(Source,pOwner,sbdd);
            
            bool HasTopLeft = Source.BorderData.Top_Left_Corner.HasData;
            bool HasTopRight = Source.BorderData.Top_Right_Corner.HasData;
            bool HasBottomRight = Source.BorderData.Bottom_Right_Corner.HasData;
            bool HasBottomLeft = Source.BorderData.Bottom_Left_Corner.HasData;

            SKBitmap TopLeft = Source.BorderData.TopLeftBitmap;
            SKBitmap TopRight = Source.BorderData.TopRightBitmap;
            SKBitmap BottomLeft = Source.BorderData.BottomLeftBitmap;
            SKBitmap BottomRight = Source.BorderData.BottomRightBitmap;
            SKBitmap Left = Source.BorderData.LeftBitmap;
            SKBitmap Right = Source.BorderData.RightBitmap;
            SKBitmap Top = Source.BorderData.TopBitmap;
            SKBitmap Bottom = Source.BorderData.BottomBitmap;

            if (HasTopLeft)
            {
                using (var topLeft = ScaleBitmap(Source.BorderData.TopLeftBitmap, pOwner.ScaleFactor))
                {
                    //if we have a top-left corner, paint it. I mean, obviously, I suppose.
                    pRenderTarget.DrawBitmap(topLeft, new SKPoint(0, 0));
                }


            }
            if (HasTopRight)
            {
                using (var topRight = ScaleBitmap(Source.BorderData.TopRightBitmap, pOwner.ScaleFactor))
                {
                    pRenderTarget.DrawBitmap(topRight, new SKPoint(sbdd.Bounds.Right - topRight.Width, 0));
                }
            }
            if (HasBottomRight)
            {
                using (var bottomRight = ScaleBitmap(Source.BorderData.BottomRightBitmap, pOwner.ScaleFactor))
                {
                    pRenderTarget.DrawBitmap(bottomRight, new SKPoint(sbdd.Bounds.Right - bottomRight.Width, sbdd.Bounds.Bottom - bottomRight.Height));
                }

            }
            if (HasBottomLeft)
            {
                using (var bottomLeft = ScaleBitmap(Source.BorderData.BottomLeftBitmap, pOwner.ScaleFactor))
                {
                    pRenderTarget.DrawBitmap(bottomLeft, new SKPoint(0, sbdd.Bounds.Bottom - bottomLeft.Height));
                }
            }

            //the sides.
            if (Source.BorderData.Left.HasData)
            {


                using (var leftBitmap = ScaleBitmap(Source.BorderData.LeftBitmap, pOwner.ScaleFactor))
                {
                    SKRect LeftSizeBound = GetLeftSideBounds(pOwner, Source, sbdd); //new SKRect(0, HasTopLeft ? ScaleHelper(TopLeft.Height, pOwner.ScaleFactor) : 0, ScaleHelper(Source.BorderData.LeftBitmap.Width, pOwner.ScaleFactor)*2, sbdd.Bounds.Height - (HasBottomLeft ? ScaleHelper(Source.BorderData.BottomLeftBitmap.Height, pOwner.ScaleFactor) : 0));
                                                                                    //pRenderTarget.DrawRect(LeftSizeBound, new SKPaint() { Color = SKColors.Blue,Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f });
                    PaintTile(pOwner,Source.BorderData.Left, pRenderTarget, LeftSizeBound);
                }
                //alternative: paint the images ourselves to create a repeating pattern?
                //not sure how the tile stuff works, can't get it to do what I want here.
                //alternative that just paints repeating manually.
                /*
                using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(pRenderTarget))
                {
                    pRenderTarget.ClipRect(LeftSizeBound);
                    
                    pRenderTarget.DrawRect(LeftSizeBound, LeftPaint);
                }*/
//                PreTileFilter = SKImageFilter.CreateImage(_BackgroundImage);
//            }
//            SKRect Bound = new SKRect(0, 0, _BackgroundImage.Width, _BackgroundImage.Height);
//            PrimaryFilter = SKImageFilter.CreateTile(Bound, new SKRect(-4096, -4096, 4096, 4096), PreTileFilter);


            //left side is influenced by the Top_Left_Corner and Bottom_Left_Corner.



            }


            if (Source.BorderData.Right.HasData)
            {
                using (var rightBitmap = ScaleBitmap(Source.BorderData.RightBitmap, pOwner.ScaleFactor))
                {
                    SKRect RightSizeBound = GetRightSideBounds(pOwner, Source, sbdd);
                    PaintTile(pOwner,Source.BorderData.Right, pRenderTarget, RightSizeBound);
                }
                /*
                using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(pRenderTarget))
                {
                    pRenderTarget.ClipRect(RightSizeBound);
                    pRenderTarget.DrawRect(RightSizeBound, RightPaint);
                }*/
            }

        }
        private void PaintTile(IStateOwner pOwner, StandardImageBackgroundBorderSkia.BorderImageInfo srcinfo, SKCanvas Target, SKRect Region)
        {
            var X = Region.Left;
            var Y = Region.Top;
            int CurrentIndex = 0;
            //ScaleBitmap(Source.BorderData.RightBitmap, pOwner.ScaleFactor)
            SKBitmap useBitmap = srcinfo.ImageBitmaps.FirstOrDefault();
            if (useBitmap == null) return;
            var srcset = srcinfo.ImageBitmaps.Select((s) => ScaleBitmap(s, pOwner.ScaleFactor)).ToArray();
            try
            {
                using (SKAutoCanvasRestore rest = new SKAutoCanvasRestore(Target, true))
                {

                    Target.ClipRect(Region);

                    while (Y < Region.Bottom && X < Region.Right)
                    {
                        var src = srcset[CurrentIndex];
                        float maxWidth = src.Width;
                        float maxHeight = src.Height;
                        if (Region.Right - X < src.Width)
                        {
                            maxWidth = Region.Right - X;
                        }
                        if (Region.Bottom - Y < src.Height)
                        {
                            maxHeight = Region.Bottom - Y;
                        }
                        Target.DrawBitmap(src, new SKRect(0, 0, maxWidth, maxHeight), new SKRect(X, Y, X + maxWidth, Y + maxHeight));

                        X += src.Width;
                        if (X > Region.Right)
                        {
                            X = Region.Left;
                            Y += src.Height;
                        }
                        CurrentIndex = (CurrentIndex + 1) % srcinfo.ImageBitmaps.Length;
                    }
                }
            }
            finally
            {
                //dispose all the bitmaps needed.
                foreach (var disposeit in srcset)
                {
                    disposeit.Dispose();
                }
            }
            
        }

        private SKRect GetLeftSideBounds(IStateOwner pOwner,StandardImageBackgroundBorderSkia Source,SkiaBackgroundDrawData sbdd)
        {
            bool HasTopLeft = Source.BorderData.Top_Left_Corner.HasData;
            bool HasBottomLeft = Source.BorderData.Bottom_Left_Corner.HasData;
            SKBitmap TopLeft = Source.BorderData.TopLeftBitmap;
            SKRect LeftSizeBound = new SKRect(0, HasTopLeft ? ScaleHelper(TopLeft.Height, pOwner.ScaleFactor) : 0, ScaleHelper(Source.BorderData.LeftBitmap.Width, pOwner.ScaleFactor), sbdd.Bounds.Height - (HasBottomLeft ? ScaleHelper(Source.BorderData.BottomLeftBitmap.Height, pOwner.ScaleFactor) : 0));
            return LeftSizeBound;
        }
        private SKRect GetRightSideBounds(IStateOwner pOwner, StandardImageBackgroundBorderSkia Source, SkiaBackgroundDrawData sbdd)
        {
            bool HasTopRight = Source.BorderData.Top_Right_Corner.HasData;
            bool HasBottomRight = Source.BorderData.Bottom_Right_Corner.HasData;

            SKBitmap TopRight = Source.BorderData.TopRightBitmap;
            
                SKRect RightSizeBound = new SKRect(sbdd.Bounds.Right - ScaleHelper(Source.BorderData.LeftBitmap.Width, pOwner.ScaleFactor) ,

                        HasTopRight ? ScaleHelper(TopRight.Height, pOwner.ScaleFactor) : 0,
                        sbdd.Bounds.Right, sbdd.Bounds.Height - (HasBottomRight ? ScaleHelper(Source.BorderData.BottomRightBitmap.Height, pOwner.ScaleFactor) : 0));

                return RightSizeBound;
            
        }

        private SKRect GetTopSideBounds(IStateOwner pOwner, StandardImageBackgroundBorderSkia Source, SkiaBackgroundDrawData sbdd)
        {
            bool HasTopRight = Source.BorderData.Top_Right_Corner.HasData;
            bool HasTopLeft = Source.BorderData.Top_Left_Corner.HasData;
            SKBitmap TopBitmap = Source.BorderData.TopBitmap;
            SKBitmap TopRight = Source.BorderData.TopRightBitmap;
            SKBitmap TopLeft = Source.BorderData.TopLeftBitmap;
            SKRect TopSideBound = new SKRect(HasTopLeft ? ScaleHelper(TopLeft.Width,pOwner.ScaleFactor) : 0, 0, sbdd.Bounds.Width - (HasTopRight ? ScaleHelper(TopRight.Width,pOwner.ScaleFactor) : 0), ScaleHelper(TopBitmap.Height,pOwner.ScaleFactor));
            return TopSideBound;

        }
        private float ScaleHelper(double Value, double Scale)
        {
            return (float)(Value * Scale);
        }
        private SKPaint CreateSidePaint(SKBitmap src,IStateOwner pOwner,SKRect? Bound = null)
        {
            Bound = Bound ?? new SKRect(-4096, -4096, 4096, 4096);
            var scaledbmp = ScaleBitmap(src, pOwner.ScaleFactor);
            var PreTileFilter = SKImageFilter.CreateImage(SKImage.FromBitmap(scaledbmp));
            SKRect BoundUse = new SKRect(0, 0, scaledbmp.Width, scaledbmp.Height);
            
        var PrimaryFilter = SKImageFilter.CreateTile(BoundUse,  Bound.Value, PreTileFilter);

            return new SKPaint() { ImageFilter = PrimaryFilter };


        }



        private double LastScalePreparedPaint = 0;
        private SKPaint LeftPaint, RightPaint, TopPaint, BottomPaint;

        private void PreparePaints(StandardImageBackgroundBorderSkia Source,IStateOwner pOwner,SkiaBackgroundDrawData sdbb)
        {
            if (LastScalePreparedPaint != pOwner.ScaleFactor)
            {
                LastScalePreparedPaint = pOwner.ScaleFactor;
                if (Source.BorderData.Left.HasData)
                {
                    LeftPaint = CreateSidePaint(Source.BorderData.LeftBitmap, pOwner,GetLeftSideBounds(pOwner,Source,sdbb));
                }
                if (Source.BorderData.Right.HasData)
                {
                    RightPaint = CreateSidePaint(Source.BorderData.RightBitmap, pOwner);
                }
                if (Source.BorderData.Top.HasData)
                {
                    TopPaint = CreateSidePaint(Source.BorderData.TopBitmap, pOwner);
                }
                if (Source.BorderData.Bottom.HasData)
                {
                    BottomPaint = CreateSidePaint(Source.BorderData.BottomBitmap, pOwner);
                }


            }
        }

        private SKBitmap ScaleBitmap(SKBitmap src, double ScaleFactor)
        {
            SKPointI newsize = new SKPointI((int)((float)src.Width * ScaleFactor), (int)((float)src.Height * ScaleFactor));
            SKBitmap newbitmap = new SKBitmap(newsize.X,newsize.Y);
            using (SKCanvas skc = new SKCanvas(newbitmap))
            {
                skc.DrawBitmap(src, new SKRect(0, 0, newsize.X, newsize.Y));
            }

            return newbitmap;

        }

    }
}
