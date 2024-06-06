using System;
using System.Drawing;
using BASeTris.AssetManager;
using BASeTris.Rendering;
using SkiaSharp;
using BASeTris.Theme.Block;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BASeTris.BackgroundDrawers
{
    public class StandardImageBackgroundDrawSkiaCapsule : BackgroundDrawData
    {
        public SKImage _BackgroundImage = null;
        public SKImage BackgroundImage
        {
            get { return _BackgroundImage; }

        }
        public double Scale { get; set; } = 1f;
        public SKPoint CurrOrigin { get; set; } = SKPoint.Empty;
        public float CurrAngle { get; set; } = 0;
        public float AngleSpeed { get; set; } = 0;
        public SKPoint Movement { get; set; } = new SKPoint(0, 0);
        public SKColorFilter theFilter = null;
        public SKPaint BackgroundBrush = null;
        public SKImageFilter PrimaryFilter = null;
        public IVectorMutator VelocityMutator { get; set; } = new CompositeVectorMutator(new RandomVectorMutator(0)) {AdvanceType = CompositeVectorMutator.MutatorAdvancementType.Random };  //= new RandomVectorMutator(2000);
        //public Func<SKPoint,SKPoint> ChangeVelocityFunction = null;

        
        public void ResetState(SKRect DrawBounds)
        {
            if (_BackgroundImage == null) return;
            SKImageFilter PreTileFilter = null;
            if (theFilter != null)
            {
                SKImageFilter si = SKImageFilter.CreateColorFilter(theFilter);
                Rectangle AttribRect = new Rectangle(0, 0, (int)((float)_BackgroundImage.Width), (int)((float)_BackgroundImage.Height));
                PreTileFilter = SKImageFilter.CreateBlendMode(SKBlendMode.DstIn, SKImageFilter.CreateImage(_BackgroundImage), si);
                //BackgroundBrush = new SKPaint
                //{
                //    ImageFilter = SKImageFilter.CreateBlendMode(SKBlendMode.DstIn, SKImageFilter.CreateImage(_BackgroundImage), si)
                //};

            }
            else
            {
                PreTileFilter = SKImageFilter.CreateImage(_BackgroundImage);
                //BackgroundBrush = new SKPaint()
                //{
                //    ImageFilter = SKImageFilter.CreateImage(_BackgroundImage)
                //};
            }
            SKRect Bound = new SKRect(0, 0, _BackgroundImage.Width, _BackgroundImage.Height);
            PrimaryFilter = SKImageFilter.CreateTile(Bound, new SKRect(-4096, -4096, 4096, 4096), PreTileFilter);
            BackgroundBrush = new SKPaint()
            {
                ImageFilter = PrimaryFilter
            };
            //BackgroundBrush.WrapMode = WrapMode.Tile;
        }
    }

    [BackgroundInformation(typeof(SKCanvas), "STANDARD")]
    public class StandardImageBackgroundBorderSkia : Background<StandardImageBackgroundDrawSkiaCapsule>
    {

        public class BorderImageKeyData
        {
            public enum BorderImagePartConstants
            {
                TopLeftCorner,
                TopRightCorner,
                BottomLeftCorner,
                BottomRightCorner,
                LeftSide,
                RightSide,
                TopSide,
                BottomSide
            }

            private String[] BorderImageKeys = new string[(int)BorderImagePartConstants.BottomSide+1];
            private SKBitmap[] BorderImages = new SKBitmap[(int)BorderImagePartConstants.BottomSide+1];

            private T GetPartElement<T>(T[] source, BorderImagePartConstants part)
            {
                return source[(int)part];
            }
            private void SetPartElement<T>(T[] source,BorderImagePartConstants part,T value)
            {
                source[(int)part] = value;
            }

            public String GetPartKey(BorderImagePartConstants part)
            {
                return GetPartElement(BorderImageKeys, part);
            }
            public void SetPartKey(BorderImagePartConstants part, String pValue)
            {
                SetPartElement(BorderImageKeys, part, pValue);
            }

            public SKBitmap GetPartImage(BorderImagePartConstants part)
            {
                return GetPartElement(BorderImages, part);
            }
            public void SetPartImage(BorderImagePartConstants part, SKBitmap pValue)
            {
                SetPartElement(BorderImages, part, pValue);
            }
            public String Top_Left_Corner { get { return GetPartKey(BorderImagePartConstants.TopLeftCorner); } set { SetPartKey(BorderImagePartConstants.TopLeftCorner, value); } }
            public String Top_Right_Corner { get { return GetPartKey(BorderImagePartConstants.TopRightCorner); } set { SetPartKey(BorderImagePartConstants.TopRightCorner, value); } }
            public String Bottom_Left_Corner { get { return GetPartKey(BorderImagePartConstants.BottomLeftCorner); } set { SetPartKey(BorderImagePartConstants.BottomLeftCorner, value); } }
            public String Bottom_Right_Corner { get { return GetPartKey(BorderImagePartConstants.BottomRightCorner); } set { SetPartKey(BorderImagePartConstants.BottomRightCorner, value); } }

            public String Left { get { return GetPartKey(BorderImagePartConstants.LeftSide); } set { SetPartKey(BorderImagePartConstants.LeftSide, value); } }
            public String Top { get { return GetPartKey(BorderImagePartConstants.TopSide); } set { SetPartKey(BorderImagePartConstants.TopSide, value); } }
            public String Bottom { get { return GetPartKey(BorderImagePartConstants.BottomSide); } set { SetPartKey(BorderImagePartConstants.BottomSide, value); } }
            public String Right { get { return GetPartKey(BorderImagePartConstants.RightSide); } set { SetPartKey(BorderImagePartConstants.RightSide, value); } }


            public SKBitmap TopLeftBitmap { get { return GetPartImage(BorderImagePartConstants.TopLeftCorner); } }

            public SKBitmap TopRightBitmap { get { return GetPartImage(BorderImagePartConstants.TopRightCorner); } }
            public SKBitmap BottomLeftBitmap { get { return GetPartImage(BorderImagePartConstants.BottomLeftCorner); } }

            public SKBitmap BottomRightBitmap { get { return GetPartImage(BorderImagePartConstants.BottomRightCorner); } }


            public SKBitmap LeftBitmap { get { return GetPartImage(BorderImagePartConstants.LeftSide); } }
            public SKBitmap TopBitmap { get { return GetPartImage(BorderImagePartConstants.TopSide); } }
            public SKBitmap RightBitmap { get { return GetPartImage(BorderImagePartConstants.RightSide); } }

            public SKBitmap BottomBitmap { get { return GetPartImage(BorderImagePartConstants.BottomSide); } }

            public BorderImageKeyData(String pCorners, String pLeft, String pTop, String pBottom, String pRight) : this(pCorners, pCorners, pCorners, pCorners, pLeft, pTop, pBottom, pRight)
            {
            }
            private void PreparePartImages()
            {
                foreach (BorderImagePartConstants part in Enum.GetValues(typeof(BorderImagePartConstants)))
                {
                    String sGetKey = GetPartKey(part);
                    if (!String.IsNullOrEmpty(sGetKey))
                    {
                        if (TetrisGame.Imageman.HasSKBitmap(sGetKey))
                            SetPartImage(part, TetrisGame.Imageman.GetSKBitmap(sGetKey));
                    }
                }


            }
            public BorderImageKeyData(String pTopLeft, String pTopRight, String pBottomLeft, String pBottomRight, String pLeft, String pTop, String pBottom, String pRight)
            {
                Top_Left_Corner = pTopLeft;
                Top_Right_Corner = pTopRight;
                Bottom_Left_Corner = pBottomLeft;
                Bottom_Right_Corner = pBottomRight;
                Left = pLeft;
                Top = pTop;
                Right = pRight;
                Bottom = pBottom;
                PreparePartImages();
            }
        }
        public BorderImageKeyData BorderData { get; set; } = null;
        public StandardImageBackgroundBorderSkia(BorderImageKeyData pBorderData)
        {
            BorderData = pBorderData;
        }

        public override void FrameProc(IStateOwner pState)
        {
            //throw new NotImplementedException();
        }
    }



    [BackgroundInformation(typeof(SKCanvas), "STANDARD")]
    public class StandardImageBackgroundSkia : Background<StandardImageBackgroundDrawSkiaCapsule>
    {

        

        public static StandardImageBackgroundSkia  GetStandardBackgroundDrawer()
        {
            var _Background = new StandardImageBackgroundSkia();
            var useImage = TetrisGame.Imageman["background"];
            Bitmap bmp = new Bitmap(ImageManager.ReduceImage(useImage, new Size(useImage.Width / 2, useImage.Height / 2)));
                

            SKImage usebg = SkiaSharp.Views.Desktop.Extensions.ToSKImage(bmp);
            _Background.Data = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = usebg, Movement = new SKPoint(0, 0) };
            return _Background;
        }
        static Dictionary<SKColor, SKImage> singlecolorbitmaps = new Dictionary<SKColor, SKImage>();
        public static StandardImageBackgroundSkia GetSolidBackground(SKColor color)
        {
            if (!singlecolorbitmaps.ContainsKey(color))
            {
                //draw 1x1 pixel of the given color.
                SKBitmap skb = new SKBitmap(1, 1);
                using (SKCanvas skc = new SKCanvas(skb))
                {
                    skc.Clear(color);
                }
                singlecolorbitmaps.Add(color, SKImage.FromBitmap(skb));
            }
            return GetImageBackground(singlecolorbitmaps[color]);


        }
        public static StandardImageBackgroundSkia GetImageBackground(SKImage pImage)
        {
            return GetImageBackground(pImage, new SKPoint(0, 0));
        }
        public static StandardImageBackgroundSkia GetImageBackground(SKImage pImage, SKPoint pMovement)
        {
            return GetImageBackground(pImage, pMovement, null);
        }
        public static StandardImageBackgroundSkia GetImageBackground(SKImage pImage, SKPoint pMovement, SKColorFilter filter)
        {
            var _background = new StandardImageBackgroundSkia();
            _background.Data = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = pImage, Movement = pMovement };
            if(filter!=null) _background.Data.theFilter = filter;

            return _background;
        }

        public static StandardImageBackgroundSkia GetMenuBackgroundDrawer()
        {
            var _Background = new StandardImageBackgroundSkia();


            
            var useImage = TetrisGame.Imageman["block_arrangement"];
            SKImage usebg = null;
            if (true || TetrisGame.StatelessRandomizer.NextDouble() > 0.5)
            {
                NominoTheme chosen = TetrisGame.Choose<Func<NominoTheme>>(NominoTheme.GetVisualizationThemes())();
                SKBitmap skb = TetrominoCollageRenderer.GetBackgroundCollage(chosen);

                Bitmap bmp = SkiaSharp.Views.Desktop.Extensions.ToBitmap(skb);

                bmp = new Bitmap(ImageManager.ReduceImage(bmp, new Size(bmp.Width / 8, bmp.Width / 8)));

                usebg = SkiaSharp.Views.Desktop.Extensions.ToSKImage(bmp);


            }
            else
            {
                Bitmap bmp = new Bitmap(ImageManager.ReduceImage(useImage, new Size(useImage.Width / 8, useImage.Height / 8)));


                usebg = SkiaSharp.Views.Desktop.Extensions.ToSKImage(bmp);
            }

            _Background.Data = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = usebg, Movement = new SKPoint(5, 5) };
            _Background.Data.theFilter = SKColorMatrices.GetFader(0.5f);
            
            return _Background;
        }
        public override void FrameProc(IStateOwner pOwner)
        {
            double movementscale = 1;
            if (pOwner is IGamePresenter igp)
            {
                movementscale = Math.Max(1,((1d / 60d) / igp.FrameTime));
                if (double.IsInfinity(movementscale)) movementscale = 1;
            }
            StandardImageBackgroundDrawSkiaCapsule dd = Data;

            if (Underlayer != null) Underlayer.FrameProc(pOwner);
            if (Overlayer != null) Overlayer.FrameProc(pOwner);

            if (dd == null) return;

            //if (dd.UnderLayer != null) dd.UnderLayer.FrameProc(pOwner);

            if (dd.BackgroundBrush == null)
            {
                dd.ResetState(new SKRect(0, 0, pOwner.GameArea.Width, pOwner.GameArea.Height));
            }
            if (dd.BackgroundBrush == null) return;

            if (!dd.Movement.IsEmpty)
            {
                
                dd.CurrOrigin = new SKPoint((dd.CurrOrigin.X + (float)(dd.Movement.X/movementscale)) % dd._BackgroundImage.Width, (dd.CurrOrigin.Y + (float)(dd.Movement.Y/movementscale)) % dd._BackgroundImage.Height);
                if (dd.CurrOrigin.X == float.NaN) dd.CurrOrigin = new SKPoint(0, dd.CurrOrigin.Y);
                if (dd.CurrOrigin.Y == float.NaN) dd.CurrOrigin = new SKPoint(dd.CurrOrigin.X, 0);
            }
            
            if (dd.AngleSpeed > 0) dd.CurrAngle += dd.AngleSpeed;
            dd.BackgroundBrush.ImageFilter = SKImageFilter.CreateOffset(dd.CurrOrigin.X,dd.CurrOrigin.Y,dd.PrimaryFilter);
            if (dd.VelocityMutator != null) dd.Movement = dd.VelocityMutator.Mutate(dd.Movement);
            //might need to do something weird for tiling.
        }
    }
}