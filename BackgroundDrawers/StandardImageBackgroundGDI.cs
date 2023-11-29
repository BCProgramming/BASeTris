using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BASeTris.AssetManager;

namespace BASeTris.BackgroundDrawers
{
    [BackgroundInformation(typeof(Graphics),"STANDARD")]
    public class StandardImageBackgroundGDI : Background<StandardImageBackgroundDrawGDICapsule> 
    {
        
      

        public override void FrameProc(IStateOwner pOwner)
        {
            StandardImageBackgroundDrawGDICapsule dd = Data;
            if (dd == null) return;
            if(dd.BackgroundBrush==null)
            {
                dd.ResetState();
            }
            if (dd.BackgroundBrush == null) return;

            if (!dd.Movement.IsEmpty)
            {
                dd.CurrOrigin = new PointF((dd.CurrOrigin.X + dd.Movement.X) % dd._BackgroundImage.Width, (dd.CurrOrigin.Y + dd.Movement.Y) % dd._BackgroundImage.Height);
            }

            if (dd.AngleSpeed > 0) dd.CurrAngle += dd.AngleSpeed;
            dd.BackgroundBrush.ResetTransform();
            dd.BackgroundBrush.TranslateTransform(dd.CurrOrigin.X, dd.CurrOrigin.Y);
            dd.BackgroundBrush.RotateTransform(dd.CurrAngle);
        }

        public StandardImageBackgroundGDI(StandardImageBackgroundDrawGDICapsule sbdd)
        {
            Data = sbdd;
        }
        public static StandardImageBackgroundGDI GetStandardBackgroundDrawer(float fade=0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            double xpoint = 1 + TetrisGame.StatelessRandomizer.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.StatelessRandomizer.NextDouble() * 2;
            var sib = new StandardImageBackgroundGDI(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = new PointF((float)xpoint, (float)ypoint) });
            
            return sib;
        }
        public static StandardImageBackgroundGDI GetStandardBackgroundDrawer(PointF Movement,float fade = 0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            var sib = new StandardImageBackgroundGDI(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = Movement });

            return sib;
        }
    }
    public class StandardImageBackgroundDrawGDICapsule : BackgroundDrawData
    {

        public Image _BackgroundImage = null;
        public Image BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                _BackgroundImage = value;
                ResetState();
            }
        }

        public PointF CurrOrigin { get; set; } = PointF.Empty;
        public float CurrAngle { get; set; } = 0;
        public float AngleSpeed { get; set; } = 0;
        public PointF Movement { get; set; } = new PointF(0, 0);
        public ImageAttributes theAttributes = null;
        public TextureBrush BackgroundBrush = null;

        public void ResetState()
        {
            if (_BackgroundImage == null) return;
            if (theAttributes != null)
            {
                Rectangle AttribRect = new Rectangle(0, 0, _BackgroundImage.Width, _BackgroundImage.Height);
                BackgroundBrush = new TextureBrush(_BackgroundImage, AttribRect, theAttributes);
            }
            else
            {
                BackgroundBrush = new TextureBrush(_BackgroundImage);
                ;
            }

            BackgroundBrush.WrapMode = WrapMode.Tile;
        }
    }
}