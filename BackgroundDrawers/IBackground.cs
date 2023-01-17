using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AssetManager;
using BASeTris.Rendering;
using BASeTris.BackgroundDrawers;
using SkiaSharp;
using BASeTris.Theme.Block;

namespace BASeTris.BackgroundDrawers
{
    //base class for all Background Draw Data.
    //note that this is part of the Background interface definition. This 
    //is intended to be used for storing any Graphics API specific information. if needed, with subclasses for the appropriate
    //types being implemented as needed for each possible renderer.
    public abstract class BackgroundDrawData
    {

    }
    public class SkiaBackgroundDrawData :BackgroundDrawData
    {
        public SKRect Bounds;
        public SkiaBackgroundDrawData(SKRect pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class GDIBackgroundDrawData : BackgroundDrawData
    {
        public RectangleF Bounds;
        public GDIBackgroundDrawData(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class NullBackgroundDrawData : BackgroundDrawData
    {

    }

    public abstract class Background<T> : Background, IBackground<T> where T:BackgroundDrawData,new()
    {
        //public abstract void DrawProc(Graphics g, RectangleF Bounds);
        public T Data { get; set; }
    }
    public abstract class Background : IBackground
    {
        public abstract void FrameProc(IStateOwner pState);
        
    }
    public interface IBackground<T> : IBackground where T:BackgroundDrawData
    {
        /// <summary>
        /// Routine called to perform the drawing task. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        //void DrawProc(Graphics g, RectangleF Bounds);
        //drawing is handled by rendering provider now...

        /// <summary>
        /// Called each Game "tick" to allow the background implementation to perform any necessary state changes.
        /// </summary>
        
    }
    public interface IBackground
    {
        void FrameProc(IStateOwner pState);
    }
    public class StandardImageBackgroundDrawSkiaCapsule : BackgroundDrawData
    {

        public SKImage _BackgroundImage = null;
        public SKImage BackgroundImage
        {
            get { return _BackgroundImage; }
           
        }

        public SKPoint CurrOrigin { get; set; } = SKPoint.Empty;
        public float CurrAngle { get; set; } = 0;
        public float AngleSpeed { get; set; } = 0;
        public SKPoint Movement { get; set; } = new SKPoint(0, 0);
        public SKColorFilter theFilter = null;
        public SKPaint BackgroundBrush = null;
        public SKImageFilter PrimaryFilter = null;

        public void ResetState(SKRect DrawBounds)
        {
            if (_BackgroundImage == null) return;
            SKImageFilter PreTileFilter = null;
            if (theFilter != null)
            {
                SKImageFilter si = SKImageFilter.CreateColorFilter(theFilter);
                Rectangle AttribRect = new Rectangle(0, 0, _BackgroundImage.Width, _BackgroundImage.Height);
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
            SKRect Bound = new SKRect(0,0,_BackgroundImage.Width,_BackgroundImage.Height);
            PrimaryFilter = SKImageFilter.CreateTile(Bound, new SKRect(-4096,-4096,4096,4096), PreTileFilter);
            BackgroundBrush = new SKPaint()
            {
                ImageFilter = PrimaryFilter
            };
            //BackgroundBrush.WrapMode = WrapMode.Tile;
        }
    }

    public class StandardImageBackgroundDrawGDICapsule : BackgroundDrawData
    {
        
        public Image _BackgroundImage = null;
        public Image BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                _BackgroundImage = value;
                ResetState();
            }
        }

        public PointF CurrOrigin { get; set; } = PointF.Empty;
        public float CurrAngle { get; set; } = 0;
        public float AngleSpeed { get; set; } = 0;
        public PointF Movement { get; set; } = new PointF(0, 0);
        public ImageAttributes theAttributes = null;
        public TextureBrush BackgroundBrush = null;

        public void ResetState()
        {
            if (_BackgroundImage == null) return;
            if (theAttributes != null)
            {
                Rectangle AttribRect = new Rectangle(0, 0, _BackgroundImage.Width, _BackgroundImage.Height);
                BackgroundBrush = new TextureBrush(_BackgroundImage, AttribRect, theAttributes);
            }
            else
            {
                BackgroundBrush = new TextureBrush(_BackgroundImage);
                ;
            }

            BackgroundBrush.WrapMode = WrapMode.Tile;
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
        public static StandardImageBackgroundSkia GetMenuBackgroundDrawer()
        {
            var _Background = new StandardImageBackgroundSkia();
            var useImage = TetrisGame.Imageman["block_arrangement"];
            SKImage usebg = null;
            if (true || TetrisGame.rgen.NextDouble() > 0.5)
            {
                NominoTheme chosen = TetrisGame.Choose<Func<NominoTheme>>(new Func<NominoTheme>[] { () => new GameBoyTetrominoTheme(), () => new SNESTetrominoTheme(), () => new NESTetrominoTheme(),()=>new StandardTetrominoTheme(),()=>new Tetris2Theme_Standard(),()=>new Tetris2Theme_Enhanced() })();
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
            StandardImageBackgroundDrawSkiaCapsule dd = Data;
            if (dd == null) return;
            if (dd.BackgroundBrush == null)
            {
                dd.ResetState(new SKRect(0, 0, pOwner.GameArea.Width, pOwner.GameArea.Height));
            }
            if (dd.BackgroundBrush == null) return;

            if (!dd.Movement.IsEmpty)
            {
                
                dd.CurrOrigin = new SKPoint((dd.CurrOrigin.X + dd.Movement.X) % dd._BackgroundImage.Width, (dd.CurrOrigin.Y + dd.Movement.Y) % dd._BackgroundImage.Height);
            }

            if (dd.AngleSpeed > 0) dd.CurrAngle += dd.AngleSpeed;
            dd.BackgroundBrush.ImageFilter = SKImageFilter.CreateOffset(dd.CurrOrigin.X,dd.CurrOrigin.Y,dd.PrimaryFilter);
            
            //might need to do something weird for tiling.
        }
    }
    [BackgroundInformation(typeof(Graphics),"STANDARD")]
    public class StandardImageBackgroundGDI : Background<StandardImageBackgroundDrawGDICapsule> 
    {
        
      

        public override void FrameProc(IStateOwner pOwner)
        {
            StandardImageBackgroundDrawGDICapsule dd = Data;
            if (dd == null) return;
            if(dd.BackgroundBrush==null)
            {
                dd.ResetState();
            }
            if (dd.BackgroundBrush == null) return;

            if (!dd.Movement.IsEmpty)
            {
                dd.CurrOrigin = new PointF((dd.CurrOrigin.X + dd.Movement.X) % dd._BackgroundImage.Width, (dd.CurrOrigin.Y + dd.Movement.Y) % dd._BackgroundImage.Height);
            }

            if (dd.AngleSpeed > 0) dd.CurrAngle += dd.AngleSpeed;
            dd.BackgroundBrush.ResetTransform();
            dd.BackgroundBrush.TranslateTransform(dd.CurrOrigin.X, dd.CurrOrigin.Y);
            dd.BackgroundBrush.RotateTransform(dd.CurrAngle);
        }

        public StandardImageBackgroundGDI(StandardImageBackgroundDrawGDICapsule sbdd)
        {
            Data = sbdd;
        }
        public static StandardImageBackgroundGDI GetStandardBackgroundDrawer(float fade=0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            double xpoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            var sib = new StandardImageBackgroundGDI(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = new PointF((float)xpoint, (float)ypoint) });
            
            return sib;
        }
        public static StandardImageBackgroundGDI GetStandardBackgroundDrawer(PointF Movement,float fade = 0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            var sib = new StandardImageBackgroundGDI(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = Movement });

            return sib;
        }
    }
    public class BackgroundInformationAttribute : Attribute
    {
        public Type CanvasType { get; set; }
        public String StyleName { get; set; }
        public BackgroundInformationAttribute(Type pCanvasType,String pStyleName)
        {
            CanvasType = pCanvasType;
            StyleName = pStyleName;
        }
        public static String Style_Standard = "STANDARD";
    }
}