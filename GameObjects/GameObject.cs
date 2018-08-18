using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    /// <summary>
    /// interface implemented by explodable things.
    /// </summary>
    public interface iImagable
    {
        void Draw(Graphics g);
        Size Size { get; set; }
        Point Location { get; set; }
        Rectangle getRectangle();
    }

    public abstract class GameObject
    {
        public delegate void GameObjectFrameFunction(GameObject sourceobject, IStateOwner gamestate);

        public event GameObjectFrameFunction ObjectFrame;

        protected void InvokeFrameEvent(IStateOwner gamestate)
        {
            var copied = ObjectFrame;
            if (copied != null)
                copied(this, gamestate);
        }

        protected GameObject()
        {
        }


        public override string ToString()
        {
            return base.ToString() + "\n" +
                   "Frozen:" + this.Frozen + "\n";
        }


        //Note: had to do a bit of hackney-ing here to figure out how best to do this. Ball and Blocks have parameters indicating objects to remove,, so I designed it that way for the 
        //GameObjects from the start.
        /// <summary>
        /// used to perform a single frame of this gameobjects animation.
        /// </summary>
        /// <param name="gamestate">Game State object</param>
        /// <returns>true to indicate that this gameobject should be removed. False otherwise.</returns>
        public virtual bool PerformFrame(IStateOwner gamestate)
        {
            InvokeFrameEvent(gamestate);
            return false;
        }

        private bool _frozen = false;

        public virtual bool getFrozen()
        {
            return _frozen;
        }

        public virtual void setFrozen(bool newvalue)
        {
            _frozen = newvalue;
        }

        /// <summary>
        /// if True, means this Object will not animate while the game is paused (enemies, for example).
        /// false means it will, which could be desirable for other effects. Derived classes can hook get/set access by overriding
        /// the virtual setFrozen and getFrozen methods.
        /// </summary>
        /// <returns></returns>
        public bool Frozen
        {
            get { return getFrozen(); }
            set { setFrozen(value); }
        }

        public abstract void Draw(Graphics g);

        public static double Angle(double px1, double py1, double px2, double py2)
        {
            // Negate X and Y values
            double pxRes = px2 - px1;

            double pyRes = py2 - py1;
            double angle = 0.0;


            double drawangle = 0;
            const double drawangleincrement = Math.PI / 20;

            // Calculate the angle
            if (pxRes == 0.0)
            {
                if (pxRes == 0.0)

                    angle = 0.0;
                else if (pyRes > 0.0) angle = System.Math.PI / 2.0;

                else
                    angle = System.Math.PI * 3.0 / 2.0;
            }
            else if (pyRes == 0.0)
            {
                if (pxRes > 0.0)

                    angle = 0.0;

                else
                    angle = System.Math.PI;
            }

            else
            {
                if (pxRes < 0.0)

                    angle = System.Math.Atan(pyRes / pxRes) + System.Math.PI;
                else if (pyRes < 0.0) angle = System.Math.Atan(pyRes / pxRes) + (2 * System.Math.PI);

                else
                    angle = System.Math.Atan(pyRes / pxRes);
            }

            // Convert to degrees
            return angle;
        }
    }

    public abstract class SizeableGameObject : GameObject, iLocatable
    {
        protected PointF _Location;
        public SizeF Size { get; set; }

        public PointF Location
        {
            get { return _Location; }
            set { _Location = value; }
        }

        protected SizeableGameObject(PointF pLocation, SizeF objectsize)
        {
            Size = objectsize;
            Location = pLocation;
        }

        public PointF CenterPoint()
        {
            return new PointF(Location.X + (Size.Width / 2), Location.Y + (Size.Height / 2));
        }

        public RectangleF getRectangle()
        {
            return new RectangleF(Location, Size);
        }
    }

    public class AnimatedImageObject : SizeableGameObject
    {
        public Image[] ObjectImages;
        protected int frameadvancedelay = 3;
        protected int countframe = 0;
        protected int currimageframe = 0;
        protected PointF _Velocity;

        public PointF Velocity
        {
            get { return _Velocity; }
            set { _Velocity = value; }
        }

        protected VelocityChanger _VelocityChange = new VelocityChangerLinear();

        public VelocityChanger VelocityChange
        {
            get { return _VelocityChange; }
            set { _VelocityChange = value; }
        }

        public int CurrentFrame
        {
            get { return currimageframe; }
            set { currimageframe = value; }
        }

        public Image CurrentFrameImage
        {
            get
            {
                try
                {
                    return ObjectImages[CurrentFrame];
                }
                catch (IndexOutOfRangeException erange)
                {
                    Debug.Print("stop");
                }

                return null;
            }
        }


        public AnimatedImageObject(PointF Location, SizeF ObjectSize, Image[] pObjectImages, int pframeadvancedelay)
            : base(Location, ObjectSize)
        {
            ObjectImages = pObjectImages;
            frameadvancedelay = pframeadvancedelay;
        }

        public AnimatedImageObject(PointF Location, SizeF ObjectSize, Image[] pObjectImages)
            : this(Location, ObjectSize, pObjectImages, 3)
        {
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            //throw new NotImplementedException();
            Location = _VelocityChange.PerformFrame(gamestate, Location);
            if (ObjectImages.Length == 1) return false;
            if (frameadvancedelay > 0)
            {
                countframe++;
                if (countframe >= frameadvancedelay)
                {
                    currimageframe++;
                    if (currimageframe > ObjectImages.Length - 1) currimageframe = 0;
                    countframe = 0;
                }
            }

            return false;
        }

        public override void Draw(Graphics g)
        {
            g.DrawImage(ObjectImages[currimageframe], Location.X, Location.Y, Size.Width, Size.Height);
        }
    }


    public class GameImageObject : SizeableGameObject
    {
        public Image ObjectImage = null;

        public GameImageObject(PointF Location, SizeF ObjectSize, Image ImageUse)
            : base(Location, ObjectSize)
        {
            ObjectImage = ImageUse;
        }

        public GameImageObject(PointF Location, Image ImageUse)
            : base(Location, ImageUse.Size)
        {
            ObjectImage = ImageUse;
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            return false;
        }

        public override void Draw(Graphics g)
        {
            g.DrawImage(ObjectImage, Location.X, Location.Y, Size.Width, Size.Height);
        }
    }
}