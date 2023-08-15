using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    public static class SkiaExtensions
    {

        public static void SaveToFile(this SKImage bitmap,String sFileName)
        {
            SKEncodedImageFormat imageFormat = SKEncodedImageFormat.Png;
            int quality = (int)100;

            using (MemoryStream memStream = new MemoryStream())
            {

                SKData skd = bitmap.Encode(imageFormat, quality);
                skd.SaveTo(memStream);
                byte[] data = memStream.ToArray();

                if (data == null)
                {
                    return;
                }
                else if (data.Length == 0)
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(new FileStream(sFileName, FileMode.Create)))
                    {
                        sw.BaseStream.Write(data, 0, data.Length);
                    }
                }
            }
            
        }
        public static Color ToColor(this SKColor src)
        {
            return Color.FromArgb(src.Alpha, src.Red, src.Green, src.Blue);
        }
        public static SKColor ToSKColor(this Color src)
        {
            return new SKColor(src.R, src.G, src.B, src.A);
        }
        public static RectangleF ToRectangleF(this SKRect Source)
        {
            return new RectangleF(Source.Left, Source.Top, Source.Width, Source.Height);
        }
        public static void ApplySizedFont(this SKPaint paint, IStateOwner pOwner,float desiredSize,SKColor pColor)
        {
            
            float useSize = desiredSize * (float)pOwner.ScaleFactor;
            var CurrentColor = SKColors.White;
            paint.Typeface = TetrisGame.RetroFontSK;
            paint.TextSize = useSize;
            paint.Color = pColor;
        }
        public static void DrawTextSK(this SKCanvas g, DrawTextInformationSkia DrawData)
        {

            var characterpositions = MeasureCharacterSizes(DrawData.ForegroundPaint, DrawData.Text);
            char[] drawcharacters = DrawData.Text.ToCharArray();
            foreach (int pass in new[] { 1, 2 })
            {
                for (int i = 0; i < drawcharacters.Length; i++)
                {
                    char drawcharacter = drawcharacters[i];
                    SKPoint DrawPosition = new SKPoint(characterpositions[i].Left + DrawData.Position.X, characterpositions[i].Top + DrawData.Position.Y);
                    DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, new SKPoint(characterpositions[i].Size.Width, characterpositions[i].Size.Height), i, drawcharacters.Length, pass);
                }
            }


        }
        public static void DrawTextSK(this SKCanvas Target, String pText, SKPoint Position, SKTypeface typeface, SKColor Color, float DesiredSize, double ScaleFactor)
        {
            var rBytes = UTF8Encoding.Default.GetBytes(pText.ToCharArray());
            using (SKPaint skp = new SKPaint())
            {
                skp.Color = Color;
                /*skp.ColorFilter = SKColorFilter.CreateBlendMode(
                    SkiaSharp.Views.Desktop.Extensions.ToSKColor(
                    System.Drawing.Color.FromArgb(255, 255, 0, 0)),SKBlendMode.Screen);*/
                skp.TextSize = (float)(DesiredSize * ScaleFactor);
                skp.Typeface = typeface;
                skp.FilterQuality = SKFilterQuality.High;
                skp.SubpixelText = true;
                Target.DrawText(rBytes, Position.X, Position.Y, skp);
            }

            //SkiaCanvas.DrawText(UTF8Encoding.Default.GetBytes("testing".ToCharArray()), 67, 67, skp);



        }
        private static List<SKRect> MeasureCharacterSizes(SKPaint skp, String text)
        {
            List<SKRect> results = new List<SKRect>();
            float xpos = 0;
            for (int index = 0; index < text.Length; index++)
            {
                SKRect measuredchar = new SKRect();
                skp.MeasureText("█", ref measuredchar);
                measuredchar = new SKRect(xpos, measuredchar.Top, xpos + measuredchar.Width, measuredchar.Bottom);
                xpos += measuredchar.Width;
                results.Add(measuredchar);

            }
            return results;
        }
       
 

    }
    
}
