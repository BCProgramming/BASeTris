using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using BaseTris;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering.GDIPlus
{
    public class TetrisStandardColouredBlockGDIRenderingHandler : TetrisImageBlockGDIRenderingHandler, IRenderingHandler<Graphics, StandardColouredBlock, TetrisBlockDrawParameters>
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
            else if (Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Pixeled)
            {
                baseimage = "block_pixeled_red";
            }
            else if(Source.DisplayStyle==StandardColouredBlock.BlockStyle.Style_Pixeled_Outline)
            {
                baseimage = "block_pixeled_red_outline";        
            }
            
            else if(Source.DisplayStyle == StandardColouredBlock.BlockStyle.Style_Mottled)
            {
                baseimage = "block_mottled";
            }
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
                    StandardColourBlocks[sBlockKey].Add(c, ResizeImage(GDIPlusHelpers.RecolorImage(TetrisGame.Imageman[baseimage], c), TargetSize));
                }
            }

            if (!StandardColourBlocks[sBlockKey].ContainsKey(Source.BlockColor))
            {
                StandardColourBlocks[sBlockKey].Add(Source.BlockColor, ResizeImage(GDIPlusHelpers.RecolorImage(TetrisGame.Imageman[baseimage], Source.BlockColor), TargetSize));
            }


            return StandardColourBlocks[sBlockKey][Source.BlockColor];
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