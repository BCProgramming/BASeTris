using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    public static class SkiaExtensions
    {
        public static RectangleF ToRectangleF(this SKRect Source)
        {
            return new RectangleF(Source.Left, Source.Top, Source.Width, Source.Height);
        }

}
    
}
