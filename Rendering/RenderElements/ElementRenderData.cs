using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Settings;
namespace BASeTris.Rendering.RenderElements
{

    public abstract class TetrisBlockDrawParameters
    {
        public Nomino GroupOwner = null;
        public float FillPercent = 1f;
        public SettingsManager Settings;
        public TetrisBlockDrawParameters(Nomino pGroupOwner, SettingsManager pSettings)
        {
            GroupOwner = pGroupOwner;
            Settings = pSettings;
        }
    }
   
    public class TetrisBlockDrawGDIPlusParameters : TetrisBlockDrawParameters
    {
        public Graphics g;
        public RectangleF region;

        public Brush OverrideBrush = null;
        public ImageAttributes ApplyAttributes = null;


        public TetrisBlockDrawGDIPlusParameters(Graphics pG, RectangleF pRegion, Nomino pGroupOwner, SettingsManager pSettings) : base(pGroupOwner,pSettings)
        {
            g = pG;
            region = pRegion;

        }
    }
    
}
