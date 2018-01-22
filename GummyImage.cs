using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BaseTris
{
    internal class GummyImage
    {
        public static Image GetGummyImage(Color usecolor,Color useInnerColor, Size RectDrawSize)
        {
            Bitmap DrawGummy = new Bitmap(RectDrawSize.Width, RectDrawSize.Height);
            Graphics GummyCanvas = Graphics.FromImage(DrawGummy);
            GummyCanvas.SmoothingMode = SmoothingMode.HighQuality;
            GummyCanvas.InterpolationMode = InterpolationMode.Bicubic;

            //first ellipse.
            // Base ellipse
            Rectangle BaseEllipse = new Rectangle(new Point(0, 0), new Size(RectDrawSize.Width, RectDrawSize.Height));
            Rectangle ReflectionEllipse = new Rectangle(BaseEllipse.Location, new Size(BaseEllipse.Size.Width - 2, BaseEllipse.Size.Height - 2));

            GraphicsPath ReflectionPath = new GraphicsPath(FillMode.Winding);
            ReflectionPath.AddRectangle(ReflectionEllipse);
            PathGradientBrush ReflectionGradient = new PathGradientBrush(ReflectionPath);
            ReflectionGradient.CenterColor = usecolor;
            ReflectionGradient.SurroundColors = new[] {Color.FromArgb(255, Color.Black)};
            ReflectionGradient.CenterPoint = new PointF((float) (BaseEllipse.Width / 1.5), BaseEllipse.Top - Convert.ToInt16(BaseEllipse.Height * 2));

            Blend ReflectionBlend = new Blend(5);
            ReflectionBlend.Factors = new[] {0.5f, 1.0f, 1.0f, 1.0f, 1.0f};
            ReflectionBlend.Positions = new[] {0.0f, 0.05f, 0.5f, 0.75f, 1.0f};
            ReflectionGradient.Blend = ReflectionBlend;

          

            GummyCanvas.FillPath(ReflectionGradient, ReflectionPath);

            if (useInnerColor != usecolor)
            {
                int outlinewidth = Math.Min(BaseEllipse.Size.Width / 8, BaseEllipse.Size.Height / 8);
                Rectangle inset = new Rectangle(BaseEllipse.Left + outlinewidth, BaseEllipse.Top + outlinewidth, BaseEllipse.Width - (outlinewidth * 2), BaseEllipse.Height - (outlinewidth * 2));

                LinearGradientBrush lgb = new LinearGradientBrush(inset, useInnerColor, Color.LightGray, LinearGradientMode.Horizontal);

                GummyCanvas.FillRectangle(lgb,inset);

            }


            ReflectionGradient.Dispose();
            ReflectionPath.Dispose();

            // 1st highlight ellipse
            int HighlightWidth = Convert.ToInt16(ReflectionEllipse.Width * 1f);
            int HighlightHeight = Convert.ToInt16(ReflectionEllipse.Height * 0.6);

            int HighlightX = (ReflectionEllipse.Width / 2) - (HighlightWidth / 2);
            int HighlightY = ReflectionEllipse.Top + 1;

            Rectangle HighlightEllipse = new Rectangle
            (
                new Point(HighlightX, HighlightY),
                new Size(HighlightWidth, HighlightHeight));

            Color HighlightColour = Color.White;
            Color HighlightFade = Color.Transparent;

        

            LinearGradientBrush HighlightBrush = new LinearGradientBrush(HighlightEllipse, HighlightColour, HighlightFade, 90);
            HighlightBrush.WrapMode = WrapMode.TileFlipX;
            GummyCanvas.FillRectangle(HighlightBrush, HighlightEllipse);

            HighlightBrush.Dispose();

            // 2nd hilite ellipse
            int Highlight2Width = Convert.ToInt16(ReflectionEllipse.Width * 0.3);
            int Highlight2Height = Convert.ToInt16(ReflectionEllipse.Height * 0.2);

            int Highlight2X = (ReflectionEllipse.Width / 2) - (Highlight2Width / 2);
            int Highlight2Y = ReflectionEllipse.Top + Convert.ToInt16(ReflectionEllipse.Height * 0.2);

            Rectangle Highlight2Ellipse = new Rectangle
            (
                new Point(-(Highlight2Width / 2), -(Highlight2Height / 2)),
                new Size(Highlight2Width, Highlight2Height));

            LinearGradientBrush br3 = new LinearGradientBrush(Highlight2Ellipse, HighlightColour, HighlightFade, 90, true);
            GummyCanvas.TranslateTransform(Highlight2X, Highlight2Y);
            GummyCanvas.RotateTransform(-30);
            GummyCanvas.FillEllipse(br3, Highlight2Ellipse);

            br3.Dispose();

            return DrawGummy;
        }
    }
}