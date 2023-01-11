using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering.RenderElements;
using BASeTris.Settings;

namespace BASeTris.Rendering.Skia
{
    //Heavy WIP. Not usable, not even close.
    //Basically a SkiaSharp implementation for drawing. However, currently, other classes need to be changed as they have things like ImageAttributes and Image class instances.
    //It looks like this "conversion" is going to probably need to be full-hog- that is all the GDI+ stuff transformed into SkiaSharp. (SKMatrix over DrawAttributes, SKPaint instead of Brush, etc.
    //I guess that kind of makes the whole "abstraction" idea pointless, though SkiaSharp is also cross-platform so, hooray, maybe?
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
