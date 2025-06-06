﻿using System;
using System.Collections.Generic;
using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;
using SkiaSharp;
using BASeTris.Theme.Block;
//using SkiaSharp.Views.Desktop;

namespace BASeTris.Rendering.Skia
{
    [RenderingHandler(typeof(StandardColouredBlock),typeof(SKCanvas),typeof(TetrisBlockDrawParameters))]
    public class TetrisStandardColouredBlockSkiaRenderingHandler : TetrisImageBlockSkiaRenderingHandler, IRenderingHandler<SKCanvas, StandardColouredBlock, TetrisBlockDrawParameters>
    {
        public static Dictionary<String, Dictionary<SKColor, Dictionary<int, SKImage>>> StandardColourBlocks = null;
        
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
                Source._RotationImagesSK = new SKImage[] { GummyBitmaps[IndexData] };
                Source.CurrentImageHash = IndexData.GetHashCode();
            }
        }
        private SKImage GetBevelImage(StandardColouredBlock Source,int Alpha=255)
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
                StandardColourBlocks = new Dictionary<string, Dictionary<SKColor, Dictionary<int, SKImage>>>();
            }

            if (!StandardColourBlocks.ContainsKey(sBlockKey))
            {
                StandardColourBlocks.Add(sBlockKey,new Dictionary<SKColor, Dictionary<int, SKImage>>());
            }

            if (StandardColourBlocks[sBlockKey].Count == 0)
            {
                foreach (Color c in new Color[] { Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Blue, Color.Red, Color.Orange })
                {

                    Image GDIPImage = TetrisGame.Imageman[baseimage];
                    Bitmap GDIBit = new Bitmap(GDIPImage);
                    SKImage ski = SkiaSharp.Views.Desktop.Extensions.ToSKImage(GDIBit);
                    var BuildImage = ResizeImage(RecolorImage(ski, c.ToSKColor()), TargetSize);
                    if (Alpha < 255) BuildImage = FadeImage(BuildImage, Alpha);
                    if (!StandardColourBlocks.ContainsKey(sBlockKey)) StandardColourBlocks.Add(sBlockKey, new Dictionary<SKColor, Dictionary<int, SKImage>>());
                    if (!StandardColourBlocks[sBlockKey].ContainsKey(c.ToSKColor()))
                        StandardColourBlocks[sBlockKey].Add(c.ToSKColor(), new Dictionary<int, SKImage>());
                    StandardColourBlocks[sBlockKey][c.ToSKColor()].Add(Alpha,BuildImage );
                }
            }

            if (!StandardColourBlocks[sBlockKey].ContainsKey(Source.BlockColor.ToSKColor()))
            {
                Image GDIPImage = TetrisGame.Imageman[baseimage];
                Bitmap GDIBit = new Bitmap(GDIPImage);
                SKImage ski = SkiaSharp.Views.Desktop.Extensions.ToSKImage(GDIBit);
                var BuildImage = ResizeImage(RecolorImage(ski, Source.BlockColor.ToSKColor()), TargetSize);
                if (Alpha < 255) BuildImage = FadeImage(BuildImage, Alpha);

                if (!StandardColourBlocks[sBlockKey].ContainsKey(Source.BlockColor.ToSKColor()))
                    StandardColourBlocks[sBlockKey].Add(Source.BlockColor.ToSKColor(), new Dictionary<int, SKImage>());
                StandardColourBlocks[sBlockKey][Source.BlockColor.ToSKColor()].Add(Alpha, BuildImage);


            }
            

            return StandardColourBlocks[sBlockKey][Source.BlockColor.ToSKColor()][Alpha];
        }
        public static float[] GetColorizationMatrix(SKColor Target)
        {
            float NormalizedR = (float)Target.Red / 255;
            float NormalizedG = (float)Target.Green / 255;
            float NormalizedB = (float)Target.Blue / 255;
            float NormalizedA = (float)Target.Alpha / 255;
            NormalizedR /= 1.33f;
            NormalizedG /= 1.33f;
            NormalizedB /= 1.33f;
            NormalizedA /= 1.33f;

            float[] mat = new float[] {
                NormalizedR, 0.72f, 0.07f, 0, 0,
                NormalizedG, 0.72f, 0.07f, 0f, 0f,
                NormalizedB, 0.72f, 0.07f, 0f, 0f,
                NormalizedA,    0f,    0f,    1f, 0f};
            return mat;
        }
        public static SKImage RemapImageColors(SKImage Source, Dictionary<SKColor, SKColor> TargetData)
        {
            //Naive implementation, we do the remapping ourself. 

            SKImageInfo ski = new SKImageInfo(Source.Width, Source.Height, Source.ColorType, Source.AlphaType, Source.ColorSpace);
            SKPixmap skp = Source.PeekPixels();
            SKBitmap newbitmap = new SKBitmap(ski);

         
                
                for (int x = 0; x < Source.Width; x++)
                {
                    for (int y = 0; y < Source.Height; y++)
                    {
                        SKColor SourceColor = skp.GetPixelColor(x, y);
                        SKColor TargetColor = SourceColor;
                        if (TargetData.ContainsKey(SourceColor)) TargetColor = TargetData[SourceColor];
                        newbitmap.SetPixel(x, y, TargetColor);          
                    }
                }


            return SKImage.FromBitmap(newbitmap);
            



        }
        public static SKImage ProcessBlockImageInfo(SKImage Source, BlockColorInformation data)
        {
            SKImage OperatingResult = null;
            if (data.ColorMapping != null)
            {
                OperatingResult = RemapImageColors(Source, data.ColorMapping);
            }

            if(data.SolidColor!=null)
            {
               return RecolorImage(OperatingResult??Source, (data.SolidColor)??SKColors.Red);
            }
            return OperatingResult;
            
        }
     
        public static SKImage RecolorImage(SKImage Source, SKColor Target)
        {
            SKCanvas c;
            SKPaint p;
            var mat = GetColorizationMatrix(Target);

            return ApplyMatrix(Source, mat);
        }
        public static SKImage FadeImage(SKImage Source,float Alpha)
        {
            var mat = GetFaderMatrix(Alpha);
            return ApplyMatrix(Source, mat);
        }
        public static SKImage ApplyMatrix(SKImage Source,float[] mat)
        {

            SKRectI irect = new SKRectI((int)0, (int)0, Source.Width, Source.Height);
            var result = Source.ApplyImageFilter(SKImageFilter.CreateColorFilter(SKColorFilter.CreateColorMatrix(mat)), irect, irect, out SKRectI active, out SKPoint activeclip);
            return result;
        }
        public static float[] GetFaderMatrix(float Alpha)
        {

            
            float[] mat = new float[] {
                1, 0f, 0f, 0, 0,
                0f, 1f, 0f, 0f, 0f,
                0f, 0f, 1f, 0f, 0f,
                0f,    0f,    0f,    Alpha, 1f};

            return mat;
        }


        private SKImage ResizeImage(SKImage Source, Size newSize)
        {

            using (SKBitmap result = new SKBitmap(newSize.Width, newSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul))
            {

                using (SKCanvas bgr = new SKCanvas(result))
                {
                    bgr.DrawImage(Source, new SKRect(0, 0, newSize.Width, newSize.Height));

                }
                return SKImage.FromBitmap(result);
            }
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