﻿using System;
using System.Drawing;
using BASeTris.AssetManager;
using BASeTris.Rendering;
using SkiaSharp;
using BASeTris.Theme.Block;

namespace BASeTris.BackgroundDrawers
{
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
                NominoTheme chosen = TetrisGame.Choose<Func<NominoTheme>>(new Func<NominoTheme>[] { () => new GameBoyTetrominoTheme(), () => new SNESTetrominoTheme(), () => new NESTetrominoTheme(),()=>new StandardTetrominoTheme(),()=>new Tetris2Theme_Standard(),()=>new Tetris2Theme_Enhanced() ,()=>new GameBoyMottledTheme()})();
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
}