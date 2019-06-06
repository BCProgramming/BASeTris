using System.Drawing;
using System.Text;
using System.Threading.Tasks;

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
}
