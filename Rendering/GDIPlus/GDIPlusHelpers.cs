using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseTris;
using BASeTris.GameStates;
using BASeTris.Blocks;

namespace BASeTris.Rendering.GDIPlus
{
    public class GDIPlusHelpers
    {
        static Dictionary<StandardColouredBlock.BlockStyle, Dictionary<Color, Image>> StandardColourBlocks = null;

        public static Image GetGummyImage(Color pColor,Color pInnerColor,Size pSize)
        {
            return GummyImage.GetGummyImage(pColor, pInnerColor, pSize);
        }

        public static Image GetBevelImage(StandardColouredBlock.BlockStyle DisplayStyle,Color DisplayColor)
        {
            String baseimage = "block_lightbevel_red";
            if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_CloudBevel)
                baseimage = "block_lightbevel_red";
            else if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_Shine)
            {
                baseimage = "block_shine_red";
            }
            else if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_HardBevel)
                baseimage = "block_std_red";
            else if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_Chisel)
                baseimage = "block_chisel_red";
            else if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_Pixeled)
            {
                baseimage = "block_pixeled_red";
            }
            else if (DisplayStyle == StandardColouredBlock.BlockStyle.Style_Grain)
            {
                baseimage = "block_grain_red";
            }

            Size TargetSize = new Size(100, 100);
            if (StandardColourBlocks == null)
            {
                StandardColourBlocks = new Dictionary<StandardColouredBlock.BlockStyle, Dictionary<Color, Image>>();
            }

            if (!StandardColourBlocks.ContainsKey(DisplayStyle))
            {
                StandardColourBlocks.Add(DisplayStyle, new Dictionary<Color, Image>());
            }

            if (StandardColourBlocks[DisplayStyle].Count == 0)
            {
                foreach (Color c in new Color[] { Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Blue, Color.Red, Color.Orange })
                {
                    StandardColourBlocks[DisplayStyle].Add(c, ResizeImage(GDIPlusHelpers.RecolorImage(TetrisGame.Imageman[baseimage], c), TargetSize));
                }
            }

            if (!StandardColourBlocks[DisplayStyle].ContainsKey(DisplayColor))
            {
                StandardColourBlocks[DisplayStyle].Add(DisplayColor, ResizeImage(GDIPlusHelpers.RecolorImage(TetrisGame.Imageman[baseimage], DisplayColor), TargetSize));
            }


            return StandardColourBlocks[DisplayStyle][DisplayColor];
        }
        public static Image ResizeImage(Image Source, Size newSize)
        {
            Bitmap result = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppPArgb);
            using (Graphics bgr = Graphics.FromImage(result))
            {
                bgr.DrawImage(Source, 0, 0, newSize.Width, newSize.Height);
            }

            return result;
        }
        public static Image RecolorImage(Image Source, Color Target)
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
            Bitmap result = new Bitmap(Source.Width, Source.Height, PixelFormat.Format32bppPArgb);
            using (Graphics gg = Graphics.FromImage(result))
            {
                gg.Clear(Color.Transparent);
                gg.DrawImage(Source, new Rectangle(0, 0, Source.Width, Source.Height), 0, 0, Source.Width, Source.Height, GraphicsUnit.Pixel, ia);
            }

            return result;
        }
    }
}
