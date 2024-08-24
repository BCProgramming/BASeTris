using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering.RenderElements;
using BASeTris.Settings;

namespace BASeTris.Rendering.Skia
{
  
    //
    class SkiaRenderingProvider
    {
    }
    /// <summary>
    /// Draw Parameter information for SkiaSharp drawing.
    /// </summary>
    public class TetrisBlockDrawSkiaParameters : TetrisBlockDrawParameters
    {
        public SkiaSharp.SKCanvas g;
        public SkiaSharp.SKRect region;
        public SkiaSharp.SKPaint OverrideBrush = null;
        public SkiaSharp.SKMatrix ApplyAttributes = SkiaSharp.SKMatrix.MakeIdentity();
        public SkiaSharp.SKColorFilter ColorFilter = null;
        public SettingsManager Settings = null;
        public TetrisBlockDrawSkiaParameters(SkiaSharp.SKCanvas pG, SkiaSharp.SKRect pRegion, Nomino pGroupOwner, SettingsManager pSettings) : base(pGroupOwner, pSettings)
        {
            Settings = pSettings;
            g = pG;
            region = pRegion;
        }
    }
}
