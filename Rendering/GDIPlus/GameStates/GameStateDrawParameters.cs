using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.GDIPlus
{
    /// <summary>
    /// Base DrawElement type
    /// </summary>
    public class BaseDrawParameters
    {
        public RectangleF Bounds;
        public BaseDrawParameters(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
}

