using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.GDIPlus
{
    /// <summary>
    /// Base DrawElement type
    /// </summary>
    public class GameStateDrawParameters
    {
        public RectangleF Bounds;
        public GameStateDrawParameters(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class GameStateSkiaDrawParameters
    {
        public SKRect Bounds;
        public GameStateSkiaDrawParameters(SKRect pBounds)
        {
            Bounds = pBounds;
        }
}
}
