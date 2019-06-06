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
    }
}