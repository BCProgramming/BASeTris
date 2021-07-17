using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace BASeTris.Rendering.GDIPlus
{
    public static class GraphicsExtensions
    {

        public static void DrawImage(this Graphics g,Image DrawImage,RectangleF DestRect,ImageAttributes Attributes)
        {

            PointF[] UsePoints = new PointF[] { new PointF(DestRect.Left, DestRect.Top), new PointF(DestRect.Right, DestRect.Top), new PointF(DestRect.Left, DestRect.Bottom) };
            g.DrawImage(DrawImage, UsePoints,
                new RectangleF(0f, 0f, (float)DrawImage.Width, (float)DrawImage.Height), GraphicsUnit.Pixel, Attributes);
        }
        // Measure the characters in a string with
        // no more than 32 characters.
        private static List<RectangleF> MeasureCharacterSizeInternal(
            Graphics gr, Font font, string text)
        {

            List<RectangleF> result = new List<RectangleF>();

            using (StringFormat string_format = new StringFormat())
            {
                RectangleF FullBound;

                string_format.Alignment = StringAlignment.Near;
                string_format.LineAlignment = StringAlignment.Near;
                string_format.Trimming = StringTrimming.None;
                string_format.FormatFlags =
                    StringFormatFlags.MeasureTrailingSpaces;
                var measurefull = gr.MeasureString(text, font, PointF.Empty, StringFormat.GenericDefault);
                FullBound = new RectangleF(0, 0, measurefull.Width, measurefull.Height);
                CharacterRange[] ranges = new CharacterRange[text.Length];
                for (int i = 0; i < text.Length; i++)
                {
                    ranges[i] = new CharacterRange(i, 1);
                }
                string_format.SetMeasurableCharacterRanges(ranges);

                // Find the character ranges.
                RectangleF rect = new RectangleF(0, 0, 10000, 100);
                Region[] regions =
                    gr.MeasureCharacterRanges(
                        text, font, FullBound,
                        string_format);

                // Convert the regions into rectangles.
                foreach (Region region in regions)
                    result.Add(region.GetBounds(gr));
            }

            return result;
        }
        // Measure the characters in the string.
        private static List<RectangleF> MeasureCharacterSizes(Graphics gr,
            Font font, string text)
        {
            List<RectangleF> results = new List<RectangleF>();

            // The X location for the next character.
            float x = 0;

            // Get the character sizes 31 characters at a time.
            for (int start = 0; start < text.Length; start += 32)
            {
                // Get the substring.
                int len = 32;
                if (start + len >= text.Length) len = text.Length - start;
                string substring = text.Substring(start, len);

                // Measure the characters.
                List<RectangleF> rects =
                    MeasureCharacterSizeInternal(gr, font, substring);

                // Remove lead-in for the first character.
                if (start == 0) x += rects[0].Left;

                // Save all but the last rectangle.
                for (int i = 0; i < rects.Count + 1 - 1; i++)
                {
                    RectangleF new_rect = new RectangleF(
                        x, rects[i].Top,
                        rects[i].Width, rects[i].Height);
                    results.Add(new_rect);

                    // Move to the next character's X position.
                    x += rects[i].Width;
                }
            }

            // Return the results.
            return results;
        }
        public static void DrawText(this Graphics g, DrawTextInformationGDI DrawData)
        {
            if (DrawData.Format == null)
            {
                DrawData.Format = new StringFormat();
            }

            //May 15th 2019- we now draw the string manually. None of this DrawString stuff.
            var characterpositions = MeasureCharacterSizes(g, DrawData.DrawFont, DrawData.Text);

            char[] drawcharacters = DrawData.Text.ToCharArray();
            g.PageUnit = GraphicsUnit.Pixel;
            for (int i = 0; i < drawcharacters.Length; i++)
            {
                //get the dimensions of this character

                char drawcharacter = drawcharacters[i];
                PointF DrawPosition = new PointF(characterpositions[i].Location.X + DrawData.Position.X, characterpositions[i].Location.Y + DrawData.Position.Y);
                DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, characterpositions[i].Size, i, drawcharacters.Length, 1);
            }

            //Draw the foreground
            for (int i = 0; i < drawcharacters.Length; i++)
            {
                //get the dimensions of this character
                char drawcharacter = drawcharacters[i];
                PointF DrawPosition = new PointF(characterpositions[i].Location.X + DrawData.Position.X, characterpositions[i].Location.Y + DrawData.Position.Y);
                DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, characterpositions[i].Size, i, drawcharacters.Length, 2);
            }
            //g.DrawString(DrawData.Text, DrawData.DrawFont, DrawData.ShadowBrush,DrawData.Position.X+DrawData.ShadowOffset.X, DrawData.Position.Y+DrawData.ShadowOffset.Y,DrawData.Format);
        }

    }
}