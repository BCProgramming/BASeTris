using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AssetManager;
using BASeTris.Rendering;
using BASeTris.BackgroundDrawers;

namespace BASeTris.BackgroundDrawers
{
    //base class for all Background Draw Data.
    //note that this is part of the Background interface definition. This 
    //is intended to be used for storing any Graphics API specific information. if needed, with subclasses for the appropriate
    //types being implemented as needed for each possible renderer.
    public abstract class BackgroundDrawData
    {

    }
    public class GDIBackgroundDrawData : BackgroundDrawData
    {
        public RectangleF Bounds;
        public GDIBackgroundDrawData(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class NullBackgroundDrawData : BackgroundDrawData
    {

    }

    public abstract class Background<T> : Background, IBackground<T> where T:BackgroundDrawData,new()
    {
        //public abstract void DrawProc(Graphics g, RectangleF Bounds);
        public T Data { get; set; }
    }
    public abstract class Background : IBackground
    {
        public abstract void FrameProc(IStateOwner pState);
    }
    public interface IBackground<T> : IBackground where T:BackgroundDrawData
    {
        /// <summary>
        /// Routine called to perform the drawing task. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        //void DrawProc(Graphics g, RectangleF Bounds);
        //drawing is handled by rendering provider now...

        /// <summary>
        /// Called each Game "tick" to allow the background implementation to perform any necessary state changes.
        /// </summary>
        
    }
    public interface IBackground
    {
        void FrameProc(IStateOwner pState);
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
    public class StandardImageBackground : Background<StandardImageBackgroundDrawGDICapsule> 
    {
        
        public StandardImageBackgroundDrawGDICapsule Capsule { get
            {
                return Data;
            }
            set { Data = value; }
            }

        public override void FrameProc(IStateOwner pOwner)
        {
            StandardImageBackgroundDrawGDICapsule dd = Capsule;
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

        public StandardImageBackground(StandardImageBackgroundDrawGDICapsule sbdd)
        {
            Data = sbdd;
        }
        public static StandardImageBackground GetStandardBackgroundDrawer(float fade=0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            double xpoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            var sib = new StandardImageBackground(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = new PointF((float)xpoint, (float)ypoint) });
            
            return sib;
        }
        public static StandardImageBackground GetStandardBackgroundDrawer(PointF Movement,float fade = 0.4f)
        {
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(fade));
            var sib = new StandardImageBackground(new StandardImageBackgroundDrawGDICapsule() { _BackgroundImage = TetrisGame.StandardTiledTetrisBackground, theAttributes = useBGAttributes, Movement = Movement });

            return sib;
        }
    }
}