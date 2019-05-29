using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AssetManager;


namespace BASeTris.BackgroundDrawers
{
    public interface IBackgroundDraw
    {
        /// <summary>
        /// Routine called to perform the drawing task. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        void DrawProc(Graphics g, RectangleF Bounds);

        /// <summary>
        /// Called each Game "tick" to allow the background implementation to perform any necessary state changes.
        /// </summary>
        void FrameProc();
    }

    public class StandardImageBackgroundDraw : IBackgroundDraw
    {
        private Image _BackgroundImage = null;

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
        private ImageAttributes theAttributes = null;
        private TextureBrush BackgroundBrush = null;

        private void ResetState()
        {
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

        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            g.FillRectangle(BackgroundBrush, Bounds);
        }

        public void FrameProc()
        {
            if (!Movement.IsEmpty)
            {
                CurrOrigin = new PointF((CurrOrigin.X + Movement.X) % _BackgroundImage.Width, (CurrOrigin.Y + Movement.Y) % _BackgroundImage.Height);
            }

            if (AngleSpeed > 0) CurrAngle += AngleSpeed;
            BackgroundBrush.ResetTransform();
            BackgroundBrush.TranslateTransform(CurrOrigin.X, CurrOrigin.Y);
            BackgroundBrush.RotateTransform(CurrAngle);
        }

        public StandardImageBackgroundDraw(Image pImage, ImageAttributes useAttributes)
        {
            theAttributes = useAttributes;
            BackgroundImage = pImage;
        }
        public static StandardImageBackgroundDraw GetStandardBackgroundDrawer(float fade=0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            var sib = new StandardImageBackgroundDraw(TetrisGame.StandardTiledTetrisBackground, useBGAttributes);
            double xpoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            sib.Movement = new PointF((float)xpoint, (float)ypoint);
            return sib;
        }
    }
}