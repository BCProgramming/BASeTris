using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;

namespace BASeTris
{
    /// <summary>
    /// Interface implemented by objects that have a location. bloody near everything, ideally.
    /// </summary>
    public interface iLocatable
    {
        PointF Location { get; set; }
    }


    public class ParticlePrecedenceComparer : IComparer<Particle>
    {
        public int Compare(Particle x, Particle y)
        {
            return x.Precedence.CompareTo(y.Precedence);
        }
    }


    public abstract class Particle : BaseParticle
    {
        //Particle adds gravity to particles...
        public bool Important { get; set; }
        private Object _Tag;
        protected PointF _GravityAcceleration = PointF.Empty;
        protected PointF _VelocityDecay = new PointF(1f, 1f); //default is to not decay at all.

        public Object Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        public PointF VelocityDecay
        {
            get { return _VelocityDecay; }
            set { _VelocityDecay = value; }
        }

        protected Particle(PointF plocation) : base(plocation)
        {
            Location = plocation;
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            float speedmult = 1;
            Velocity = new PointF(Velocity.X + (_GravityAcceleration.X) * speedmult, Velocity.Y + _GravityAcceleration.Y * speedmult);
            TrigFunctions.IncrementLocation(gamestate, ref _Location, Velocity);
            //Location = new PointF(Location.X+Velocity.X,Location.Y+Velocity.Y);
            Velocity = new PointF(Velocity.X * VelocityDecay.X, Velocity.Y * VelocityDecay.Y);
            return false;
        }

        public abstract override void Draw(Graphics g);
    }

    public abstract class BaseParticle : iLocatable
    {
        protected int _Precedence = 0; //higher means less important to draw first, order-wise.

        protected PointF _Location, _Velocity;

        public PointF Location
        {
            get { return _Location; }
            set { _Location = value; }
        }

        public PointF Velocity
        {
            get { return _Velocity; }
            set { _Velocity = value; }
        }

        /// <summary>
        /// Precedence of this Particle in the draw order. a Higher value means it should be drawn later (thus appearing on top).
        /// </summary>
        public int Precedence
        {
            get { return _Precedence; }
            set { _Precedence = value; }
        }

        /// <summary>
        /// Draw this particle.
        /// </summary>
        /// <param name="g"></param>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// performs a single frame of this particles animation,
        /// </summary>
        /// <param name="gamestate"></param>
        /// <returns>true to indicate that this particles "life" is over. false otherwise.</returns>
        public abstract bool PerformFrame(IStateOwner gamestate);


        protected BaseParticle(PointF plocation)
        {
            Location = plocation;
        }
    }

    public abstract class SizedParticle : Particle
    {
        protected SizeF _ParticleSize;
        protected PointF _SizeAnimation = PointF.Empty;

        public PointF SizeAnimation
        {
            get { return _SizeAnimation; }
            set { _SizeAnimation = value; }
        }

        public virtual SizeF ParticleSize
        {
            get { return _ParticleSize; }
            set { _ParticleSize = value; }
        }


        protected SizedParticle(PointF pLocation, SizeF pParticleSize) : base(pLocation)
        {
            ParticleSize = pParticleSize;
        }

        protected SizedParticle(PointF pLocation)
            : this(pLocation, new SizeF(16, 16))
        {
        }

        public abstract override void Draw(Graphics g);

        public override bool PerformFrame(IStateOwner gamestate)
        {
            Rectangle centered = new Rectangle((int) (Location.X - _ParticleSize.Width / 2), (int) (Location.Y - _ParticleSize.Height / 2), (int) _ParticleSize.Width, (int) _ParticleSize.Height);


            _ParticleSize = new SizeF(_ParticleSize.Width + _SizeAnimation.X, _ParticleSize.Height + _SizeAnimation.Y);
            base.PerformFrame(gamestate);
            return !gamestate.GameArea.Contains(centered);
        }
    }

    //a animated particle. Can be set to repeat the image frames, or it can be set to die when it 
    //goes through all the frames.
    public class AnimatedImageParticle : SizedParticle
    {
        protected Image[] _ImageFrames;
        protected int _TTL = 45;
        protected ImageAttributes _DrawAttributes;
        private int _LiveTime;


        public Image[] ImageFrames
        {
            get { return _ImageFrames; }
            set { _ImageFrames = value; }
        }

        public int TTL
        {
            get { return _TTL; }
            set { _TTL = value; }
        }

        public ImageAttributes DrawAttributes
        {
            get { return _DrawAttributes; }
            set { _DrawAttributes = value; }
        }

        private int getCurrentFrame()
        {
            return TrigFunctions.ClampValue
            ((int) ((((float) _LiveTime) / (float) _TTL) * _ImageFrames.Length)
                , 0, _ImageFrames.Length - 1);
        }

        public override SizeF ParticleSize
        {
            get { return _ImageFrames[getCurrentFrame()].Size; }
            set
            {
                //cannot set size...
            }
        }

        public AnimatedImageParticle()
            : this(PointF.Empty)
        {
        }

        public AnimatedImageParticle(PointF pLocation)
            : this(pLocation, TetrisGame.Imageman.getImageFrames("smoke"), 45)
        {
        }

        public AnimatedImageParticle(PointF pLocation, Image[] ImageFrames, int TTL) : base(pLocation)
        {
            _ImageFrames = ImageFrames;
            _TTL = TTL;
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            _LiveTime++;


            return _LiveTime > _TTL || base.PerformFrame(gamestate);
        }

        public override void Draw(Graphics g)
        {
            //retrieve current image...
            Image useimage = _ImageFrames[getCurrentFrame()];
            SizeF usesize = useimage.Size;
            Rectangle drawLocation = new Rectangle
            ((int) (base.Location.X - usesize.Width / 2),
                (int) (base.Location.Y - usesize.Height / 2), (int) usesize.Width, (int) usesize.Height);
            g.DrawImage(useimage, drawLocation, 0, 0, useimage.Width, useimage.Height, GraphicsUnit.Pixel, _DrawAttributes);
        }
    }

    public class ImageParticle : SizedParticle
    {
        protected Image _ParticleImage = null;

        public Image ParticleImage
        {
            get { return _ParticleImage; }
            set { _ParticleImage = value; }
        }

        public ImageParticle(PointF pLocation, Image pParticleImage, SizeF useSize) : base(pLocation, useSize)
        {
            ParticleImage = pParticleImage;
        }

        public ImageParticle(PointF pLocation, Image pParticleImage) : this(pLocation, pParticleImage, pParticleImage.Size)
        {
        }

        public override void Draw(Graphics g)
        {
            PointF uselocation = new PointF(Location.X - _ParticleSize.Width / 2, Location.Y - _ParticleSize.Height / 2);
            g.DrawImage
            (_ParticleImage, new RectangleF(uselocation, _ParticleSize),
                new RectangleF
                (0f, 0f, _ParticleSize.Width,
                    _ParticleSize.Height), GraphicsUnit.Pixel);
        }
    }


    public class BubbleParticle : ImageParticle
    {
        protected static Image _BubbleImage = TetrisGame.Imageman.getLoadedImage("bubble");

        public BubbleParticle(PointF Location) : base(Location, _BubbleImage, new SizeF(16, 16))
        {
            //Velocity = new PointF(0,(float)TetrisGame.rgen.NextDouble()*-0.5f);
            _SizeAnimation = new PointF(0.05f, 0.05f);
        }

        public BubbleParticle(PointF Location, PointF velocity)
            : base(Location, _BubbleImage, new SizeF(16, 16))
        {
            Velocity = velocity;
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            Velocity = new PointF(Velocity.X * .95f, Velocity.Y * 1.05f);


            return base.PerformFrame(gamestate);
        }
    }

    public class CharacterDebris : PolyDebris
    {
        private char _UseCharacter = '*';
        private Font _useFont = new Font(TetrisGame.GetMonospaceFont(), 14);

        public CharacterDebris(PointF pLocation, PointF pVelocity, Color puseColor, double pSizeMin, double pSizeMax)
            : base(pLocation, pVelocity, puseColor, pSizeMin, pSizeMax, 5, 10)
        {
        }

        protected override void GenPoly(double RadiusMin, double RadiusMax, int minpoints, int maxpoints)
        {
            //Override for CharacterDebris.
            //base.GenPoly(RadiusMin, RadiusMax, minpoints, maxpoints);
            //create PolyPoints from the Polygon representation of a character.
            GraphicsPath gpfont = new GraphicsPath();
            Font buildfont = new Font(_useFont.Name, (float) TetrisGame.rgen.NextDouble(RadiusMin, RadiusMax));
            gpfont.AddString(_UseCharacter.ToString(), buildfont, new PointF(0, 0), StringFormat.GenericDefault);
            gpfont.Flatten(); //flatten to a series of lines...
            //acquire the point data...
            PolyPoints = gpfont.PathPoints;


            //now, we need to acquire the Center of the character...
            PointF gotcenter = TrigFunctions.CenterPoint(PolyPoints);

            //now, offset each point by -gotcenter...


            for (int i = 0; i < PolyPoints.Length; i++)
            {
                PolyPoints[i] = new PointF(PolyPoints[i].X - gotcenter.X, PolyPoints[i].Y - gotcenter.Y);
            }

            //and blam, problem solved.
        }
    }


    /// <summary>
    /// "BrickDebris" is a piece of debris that is
    /// intended to mimic the behaviour of bricks breaking in super mario brothers.
    /// for the most part, it's just a quad that shows a specific part of a object's texture.
    /// </summary>
    public class BrickDebris : PolyDebris
    {
        private TextureBrush useBrickBrush = null;

        /// <summary>
        /// Similar to GenerateQuadBricks, but more general
        /// </summary>
        /// <param name="cols">Number of Columns to generate</param>
        /// <param name="rows">Number of Rows to generate</param>
        /// <param name="sourceblock">Block to generate bricks from.</param>
        /// <param name="BallSpeed">Speed of any ball that is causing the "break". Null if no ball is hitting it.</param>
        /// <param name="RandomFactor">factor of Random speed added to all blocks</param>
        /// <returns></returns>
        public static BrickDebris[] GenerateBricksFromBlock(int cols, int rows, RectangleF BlockPosition, TetrisBlock sourceblock, PointF? BallSpeed, float RandomFactor)
        {
            //parameter checks.
            if (BallSpeed == null) BallSpeed = PointF.Empty;
            //First step: create the bitmap.
            Bitmap BlockImage;
            Graphics usecanvas;
            List<RectangleF> OriginRect = new List<RectangleF>();
            RectangleF templocation = BlockPosition;
            Bitmap BuildImage = new Bitmap((int) BlockPosition.Width, (int) BlockPosition.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(BuildImage))
            {
                TetrisBlockDrawGDIPlusParameters tbdp = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(0, 0, templocation.Width, templocation.Height), null,new StandardSettings());
                RenderingProvider.Static.DrawElement(null, tbdp.g, sourceblock, tbdp);
                

                //now we need to chop it into 'cols' columns and 'rows' rows.
                //first, how big will each "piece" be?
                if (cols > templocation.Width) cols = (int) templocation.Width;
                if (rows > templocation.Height) rows = (int) templocation.Height;
                if (cols == 0 || rows == 0) return null;
                float Xsize = templocation.Width / (float) cols;
                float ysize = templocation.Height / (float) rows;
                SizeF usesize = new SizeF(Xsize, ysize);

                for (int currcol = 0; currcol < cols; currcol++)
                {
                    //calculate X coordinate to use.
                    float useX = usesize.Width * (float) currcol;

                    for (int currrow = 0; currrow < rows; currrow++)
                    {
                        float useY = usesize.Height * (float) currrow;
                        OriginRect.Add(new RectangleF(new PointF(useX, useY), usesize));
                    }
                }

                Random rg = TetrisGame.rgen;
                var properoffset = (from m in OriginRect select new RectangleF(m.Left + templocation.Left, m.Top + templocation.Top, m.Width, m.Height)).ToArray();
                var creationarray = OriginRect.ToArray();
                BrickDebris[] createdebris = new BrickDebris[creationarray.Length];
                for (int i = 0; i < creationarray.Length; i++)
                {
                    PointF usevel = TrigFunctions.GetRandomVelocity(RandomFactor);


                    createdebris[i] = new BrickDebris(creationarray[i], BuildImage, new PointF(properoffset[i].Left, properoffset[i].Top), usevel, (float) (rg.NextDouble() - 0.5));
                }

                return createdebris;
            }
        }

        public static BrickDebris[] GenerateQuadBricks(IStateOwner pState,TetrisBlock sourceblock, RectangleF BlockRect, PointF BallSpeed)
        {
            return GenerateQuadBricks(pState,sourceblock, BlockRect, BallSpeed, 2);
        }


        /// <summary>
        /// Task: creates four BrickDebris blocks, one in each corner, with the appropriate textured images for that location.
        /// the upper left and lower right,brick will rotate counter clockwise (negative), upper right and lower left will rotate clockwise.
        /// Image is created from the Block itself, by creating a bitmap and drawing the block to it.
        /// </summary>
        /// <param name="sourceblock">Block to create Bricks on</param>
        /// <param name="BallSpeed">Speed of ball that destroyed the block, or null.</param>
        /// <returns>the four BrickDebris objects created</returns>
        public static BrickDebris[] GenerateQuadBricks(IStateOwner pState,TetrisBlock sourceblock, RectangleF BlockRect, PointF? BallSpeed, float RandomFactor)
        {
            if (BallSpeed == null) BallSpeed = PointF.Empty;


            //First step: create the bitmap.
            Bitmap BlockImage;
            Graphics usecanvas;
            RectangleF templocation = BlockRect;
            BlockImage = new Bitmap((int) BlockRect.Width, (int) BlockRect.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(BlockImage))
            {
                TetrisBlockDrawGDIPlusParameters tb = new TetrisBlockDrawGDIPlusParameters(g, BlockRect, null,new StandardSettings());
                RenderingProvider.Static.DrawElement(pState,g,sourceblock,tb);
            }


            //use BlockImage. Now create 4 BrickDebris objects.
            //their locations are the four quadrants of this block.

            float halfw = templocation.Width / 2;
            float halfh = templocation.Height / 2;

            RectangleF upperleft = new RectangleF(templocation.Left, templocation.Top, halfw, halfh);
            RectangleF upperright = new RectangleF(templocation.Left + halfw, templocation.Top, halfw, halfh);
            RectangleF lowerleft = new RectangleF(templocation.Left, templocation.Top + halfh, halfw, halfh);
            RectangleF lowerright = new RectangleF(templocation.X + halfw, templocation.Top + halfh, halfw, halfh);
            //                                              0              1           2           3
            RectangleF[] posrectangles = new RectangleF[] {upperleft, upperright, lowerleft, lowerright};
            //            PointF[] Velocitymultiplier = new PointF[] { new PointF(-1, -1), new PointF(1, -1), new PointF(-1, 1), new PointF(1, 1) }; 
            PointF[] Velocitymultiplier = new PointF[] {new PointF(-2, -1), new PointF(2, -1), new PointF(-1, -0.5f), new PointF(1, -0.5f)};
            BrickDebris[] builddebris = new BrickDebris[4];

            PointF DoAddVelocity = new PointF(Math.Abs(BallSpeed.Value.X * 0.2f), Math.Abs(BallSpeed.Value.Y * 0.2f));
            for (int i = 0; i < posrectangles.Length; i++)
            {
                PointF randomvel = TrigFunctions.GetRandomVelocity(RandomFactor);
                PointF usevelocity = new PointF(2, 2);
                usevelocity = new PointF(Velocitymultiplier[i].X * usevelocity.X * DoAddVelocity.X, Velocitymultiplier[i].Y * usevelocity.Y * DoAddVelocity.Y);
                usevelocity = new PointF(usevelocity.X + randomvel.X, usevelocity.Y + randomvel.Y);
                float rotatespeed = 2f; //2 degrees
                //counter-clockwise for upperleft and lowerright.
                if (i == 0 || i == 3)
                    rotatespeed *= -1;


                builddebris[i] = new BrickDebris(posrectangles[i], BlockImage, posrectangles[i].Location, usevelocity, rotatespeed);
            }

            return builddebris;
        }

        public BrickDebris(RectangleF DebrisPosition, Bitmap SourceImage, PointF sourceimageOrigin, PointF pVelocity, double pRotationSpeed)
        {
            mAliveFrames = 5000;
            useBrickBrush = new TextureBrush(SourceImage);
            //useBrickBrush.TranslateTransform(sourceimageOrigin.X,sourceimageOrigin.Y);

            initRect(DebrisPosition);
            Velocity = pVelocity;
            VelocityDecay = new PointF(1f, 1f); //y get's bigger... it falls.
            RotationSpeed = pRotationSpeed;
            useFillTexture = useBrickBrush;
            DrawBrush = useFillTexture;
            TextureOrigin = sourceimageOrigin;
            DrawPen = new Pen(DrawBrush, 1); //use the same brush for the pen, so there isn't a border between adjacent blocks.
            mTTL = 50; //by that time, it should be long gone from the stage/level.
        }

        private int BottomBounceTimes = 0;

        public override bool PerformFrame(IStateOwner gamestate)
        {
            Velocity = new PointF(Velocity.X, Velocity.Y + 0.15f); //gravity...


            //"bounce" off sides and top.

            //check for top and bottom...
            if (Location.Y < gamestate.GameArea.Top)
            {
                Velocity = new PointF(Velocity.X, Velocity.Y * -0.8f);
                Location = new PointF(Location.X, gamestate.GameArea.Top);
            }
            else if (Location.Y > gamestate.GameArea.Bottom && BottomBounceTimes < 4)
            {
                BottomBounceTimes++;
                Velocity = new PointF(Velocity.X, Velocity.Y * -0.8f);
                Location = new PointF(Location.X, gamestate.GameArea.Bottom);
            }

            //sides.
            if (Location.X < gamestate.GameArea.Left)
            {
                Velocity = new PointF(Velocity.X * -0.8f, Velocity.Y);
                Location = new PointF(gamestate.GameArea.Left, Location.Y);
            }
            else if (Location.X > gamestate.GameArea.Right)
            {
                Velocity = new PointF(Velocity.X * -0.8f, Velocity.Y);
            }


            //gamestate.GameArea.Contains(PolyPoints)
            return base.PerformFrame(gamestate) || !gamestate.GameArea.Contains(new Point((int) Location.X, (int) Location.Y));
            // return base.PerformFrame(gamestate) || !gamestate.GameArea.Contains(PolyPoints);
        }

        private void initRect(RectangleF rectangleobj)
        {
            Location = TrigFunctions.CenterPoint(rectangleobj);
            SizeF oursize = rectangleobj.Size;
            PointF UpperLeft = new PointF(-oursize.Width / 2, -oursize.Height / 2);
            PointF UpperRight = new PointF(oursize.Width / 2, -oursize.Height / 2);
            PointF LowerLeft = new PointF(-oursize.Width / 2, oursize.Height / 2);
            PointF LowerRight = new PointF(oursize.Width / 2, oursize.Height / 2);
            PolyPoints = new PointF[] {UpperLeft, UpperRight, LowerRight, LowerLeft};
        }
    }

    /// <summary>
    /// Polydebris: similar to the DebrisParticle, but uses a "vector" polygon rather then a image.
    /// </summary>
    public class PolyDebris : Particle
    {
        //changes that could be made:

        /*
         * change code so that it doesn't draw the polygon each time, but instead, draws a bitmap on init, and this bitmap is what is drawn.
         * 
         * 
         * */


        protected int mAliveFrames = 0;
        protected int mTTL = 135;
        public PointF[] PolyPoints; //centered around 0,0

        public double currentRotation = 0;
        public double RotationSpeed = 2;
        public double expansionfactor = 1;
        public double expansionamount = 0;
        public SizeF SizeScale = new SizeF(1, 1);

        private Color _PenColor;
        private Color _BrushColor;


        protected TextureBrush useFillTexture = null;
        protected PointF TextureOrigin = PointF.Empty;

        public int TTL
        {
            get { return mTTL; }
            set { mTTL = value; }
        }

        public Color PenColor
        {
            get { return _PenColor; }
            set
            {
                _PenColor = value;
                DrawPen = new Pen(_PenColor);
            }
        }

        public Color BrushColor
        {
            get { return _BrushColor; }
            set
            {
                _BrushColor = value;
                DrawBrush = new SolidBrush(_BrushColor);
            }
        }


        public Brush DrawBrush { get; set; }
        public Pen DrawPen { get; set; }
        private Polygon _Poly = null;

        protected virtual void GenPoly(double RadiusMin, double RadiusMax, int minpoints, int maxpoints)
        {
            int PointCount = TetrisGame.rgen.Next(minpoints, maxpoints);

            PolyPoints = new PointF[PointCount];
            float Angle = (float) ((Math.PI * 2) / PointCount);
            for (int i = 0; i < PointCount; i++)
            {
                //generate this point...
                float RadiusUse = (float) ((TetrisGame.rgen.NextDouble() * (RadiusMax - RadiusMin)) + RadiusMin);
                float useangle = Angle * i;
                PointF newPoint = new PointF((float) Math.Sin(useangle) * RadiusUse, (float) Math.Cos(useangle) * RadiusUse);

                PolyPoints[i] = newPoint;
            }

            _Poly = new Polygon(PolyPoints);
        }

        private static PointF getRandomVelocity(double speed)
        {
            double angle = TetrisGame.rgen.NextDouble() * (Math.PI * 2);
            return new PointF((float) (Math.Cos(angle) * speed), (float) (Math.Sin(angle) * speed));
        }

        public PolyDebris(PointF pLocation, double speed, Color useColor)
            : this(pLocation, getRandomVelocity(speed), useColor)
        {
        }

        public PolyDebris(PointF pLocation, double speed, Color usecolor, double RadiusMin, double RadiusMax, int MinPoints, int MaxPoints)
            : this
            (pLocation, getRandomVelocity(speed), usecolor,
                RadiusMin, RadiusMax, MinPoints, MaxPoints)
        {
        }

        public PolyDebris(PointF pLocation, double speed, Image pieceimage, double RadiusMin, double RadiusMax, int MinPoints, int MaxPoints)
            : this
            (pLocation, getRandomVelocity(speed), pieceimage,
                RadiusMin, RadiusMax, MinPoints, MaxPoints, null, null)
        {
        }

        public PolyDebris(PointF pLocation, double speed, Image pieceimage) : this(pLocation, getRandomVelocity(speed), pieceimage, 2, 6, 3, 8, null, null)
        {
        }

        public PolyDebris(PointF pLocation, PointF Velocity, Image pieceimage)
            : this(pLocation, Velocity, pieceimage, 2, 6, 3, 8, null, null)
        {
        }

        public PolyDebris(PointF pLocation, float speed, Image ppieceimage, double RadiusMin, double RadiusMax, int MinPoints, int MaxPoints, Point? ClipTopLeft, Size? ClipSize)
            : this(pLocation, TrigFunctions.GetRandomVelocity(speed), ppieceimage, RadiusMin, RadiusMax, MinPoints, MaxPoints, ClipTopLeft, ClipSize)
        {
        }

        /// <summary>
        /// Creates a PolyDebris Object that uses the given image.
        /// </summary>
        /// <param name="pLocation"></param>
        /// <param name="pVelocity"></param>
        /// <param name="pieceimage"></param>
        public PolyDebris(PointF pLocation, PointF pVelocity, Image ppieceimage, double RadiusMin, double RadiusMax, int MinPoints, int MaxPoints, Point? ClipTopLeft, Size? ClipSize)
            : base(pLocation)
        {
            mAliveFrames = mTTL;
            Velocity = pVelocity;
            //cut the image.
            Image pieceimage = null;


            if (ClipTopLeft != null && ClipSize != null)
            {
                pieceimage = ppieceimage.ClipImage(ClipTopLeft.Value, ClipSize.Value);
            }
            else
            {
                pieceimage = ppieceimage;
            }


            //create the texturebrush...
            if (pieceimage != null)
            {
                lock (pieceimage)
                {
                    try
                    {
                        useFillTexture = new TextureBrush(pieceimage, WrapMode.Tile);
                    }
                    catch (InvalidOperationException eex)
                    {
                        //Object in use elsewhere... maybe clone it?
                        Image duplicate = (Image) pieceimage.Clone();
                        useFillTexture = new TextureBrush(duplicate, WrapMode.Tile);
                    }
                }
            }
            else
            {
                Bitmap maketexturemap = new Bitmap(15, 15, PixelFormat.Format32bppPArgb);
                Graphics maketexturegraphics = Graphics.FromImage(maketexturemap);
                maketexturegraphics.Clear(Color.Blue);
                pieceimage = maketexturemap; //bugfix: pieceimage was null below for the PointF() constructor and caused of course a nullreferenceexception.
                useFillTexture = new TextureBrush(maketexturemap, WrapMode.Tile);
            }

            //choose a random origin point within the image.
            Random rr = TetrisGame.rgen;
            TextureOrigin = new PointF((float) (pieceimage.Width * rr.NextDouble()), (float) (pieceimage.Height * rr.NextDouble()));
            useFillTexture.TranslateTransform(TextureOrigin.X, TextureOrigin.Y);
            DrawBrush = useFillTexture;
            DrawPen = new Pen(Color.Black);
            //generate the polygon points.
            /*
             *     const int minpoints = 3, maxpoints = 8;
            const double RadiusMin = 2, RadiusMax = 6;
             * */
            GenPoly(RadiusMin, RadiusMax, MinPoints, MaxPoints);
        }

        public PolyDebris(PointF pLocation, PointF pVelocity, Color UseColor)
            : this(pLocation, pVelocity, UseColor, 2, 6, 3, 8)
        {
        }

        public PolyDebris(PointF pLocation, PointF pVelocity, Color UseColor, double RadiusMin, double RadiusMax, int MinPoints, int MaxPoints)
            : base(pLocation)

        {
            BrushColor = UseColor;
            PenColor = UseColor;
            DrawPen = new Pen(UseColor);
            DrawBrush = new SolidBrush(UseColor);
            mAliveFrames = mTTL;
            Velocity = pVelocity;

            //generate the polygon points.
            GenPoly(RadiusMin, RadiusMax, MinPoints, MaxPoints);
        }

        protected PolyDebris() : base(PointF.Empty)
        {
        }

        public static explicit operator Polygon(PolyDebris src)
        {
            return src._Poly;
        }

        //static method that draws the given GameObject as it currently is and creates numfragments "pieces" of that GameObject, returned
        //as PolyDebris. Somewhat similar to the code that does this for blocks.
        public static IEnumerable<PolyDebris> Fragment(iImagable source, int numfragments, Func<float> SpeedFunc)
        {
            //step one, we need a bitmap to work with. Use the size of the GameObject.
            //This has a minor disadvantage in that the Draw() method might actually
            //want to draw more than the size of the object, but should work well enough.
            //Most effects are created using other particles or game objects anyway.
            //an additional concern is that the source.Location might not correspond to the upper left corner.
            //solution would be to use getRectangle and set the location to the the negative of the left and top values.


            if (SpeedFunc == null) SpeedFunc = (() => (float) ((TetrisGame.rgen.NextDouble() * 2) + 1));
            Bitmap createimage = new Bitmap((int) source.Size.Width, source.Size.Height, PixelFormat.Format32bppPArgb);
            Graphics useg = Graphics.FromImage(createimage);
            //draw it...
            lock (source)
            {
                Point tempgrab = source.Location;
                source.Location = Point.Empty;
                var gotrect = source.getRectangle();

                Point newoffset = new Point(-gotrect.Left, -gotrect.Top);

                source.Location = newoffset;
                source.Draw(useg);
                //revert to original location.
                source.Location = tempgrab;
            }

            Image useimage = TrigFunctions.ScaleImage(createimage, 3);
            //now, we create the pieces.
            float useradius = Math.Max(source.Size.Width, source.Size.Height) / 9;
            double minradius = Math.Min(1, useradius - 2);
            double maxradius = useradius + Math.Min(source.Size.Width, source.Size.Height) / 3;
            PointF usevelocity = TrigFunctions.GetRandomVelocity(SpeedFunc());
            for (int i = 0; i < numfragments; i++)
            {
                int startrange = Math.Min(5, (int) maxradius / 5);
                int endrange = Math.Max(5, (int) maxradius / 5);
                int genWidth = source.Size.Width;
                int genHeight = source.Size.Height;
                Size ClipSize = new Size(genWidth, genHeight);
                Point clipspot = new Point(TetrisGame.rgen.Next((int) (source.Size.Width / 2)), TetrisGame.rgen.Next((int) (source.Size.Width / 2)));
                PolyDebris newdebris = new PolyDebris(TrigFunctions.CenterPoint(source.getRectangle()), usevelocity, useimage, minradius, maxradius, 3, 8, new Point?(clipspot), new Size?(ClipSize));
                newdebris.DrawPen = Pens.Transparent;
                yield return newdebris;
            }
        }

        Matrix rotationmatrix = new Matrix();

        public override void Draw(Graphics g)
        {
            //throw new NotImplementedException();

            int Alpha = (int) (((float) mAliveFrames / (float) mTTL) * 255);


            Matrix cached = g.Transform;
            rotationmatrix.Reset();
            rotationmatrix.Scale(SizeScale.Width, SizeScale.Height);
            rotationmatrix.RotateAt((float) currentRotation, PointF.Empty, MatrixOrder.Append);
            rotationmatrix.Translate(Location.X, Location.Y, MatrixOrder.Append);
            try
            {
                //rotationmatrix.Scale(SizeScale.Width, SizeScale.Height);
                g.Transform = rotationmatrix;
                //g.DrawImage(drawthis, new Rectangle((int)Location.X, (int)Location.Y, (int)truesize.Width, (int)truesize.Height),
                //   0, 0, drawthis.Width, drawthis.Height, GraphicsUnit.Pixel);

                if (useFillTexture == null)
                {
                    Alpha = TrigFunctions.ClampValue(Alpha, 0, 255);
                    Color bcolor = Color.FromArgb(Alpha, BrushColor);
                    Color pcolor = Color.FromArgb(Alpha, PenColor);
                    DrawBrush = new SolidBrush(bcolor);
                    DrawPen = new Pen(DrawBrush);
                }
                else
                {
                    //tricky! we need to change the Alpha of the TextureBrush itself.
                    //we can do this by creating a new TextureBrush, using the same image, but passing in an Alpha-modified
                    //ImageAttributes.
#if EXPERIMENTALPOLYDEBRIS
                    var tweakalpha = ColorMatrices.GetFader(Alpha);
                    var dAttributes = new ImageAttributes();
                    dAttributes.SetColorMatrix(tweakalpha);
                    //store the original, so that we can dispose it manually to avoid piling up generational objects, or exhausting the GDI+ heap.
                    var oldBrush = useFillTexture;
                    //create a new one.
                    useFillTexture = new TextureBrush(oldBrush.Image, new Rectangle(Point.Empty, oldBrush.Image.Size), dAttributes);
                    //tada!
#endif
                }

                g.FillPolygon(DrawBrush, PolyPoints);
                g.DrawPolygon(DrawPen, PolyPoints);
            }
            catch (ArgumentException p)
            {
                Debug.Print("argexcept:" + p.Message + " Trace:" + p.StackTrace);
            }
            finally
            {
                g.Transform = cached;
            }
        }

        private void ScalePoly(double expandfactor, double increaseamount)
        {
            PointF CenterSpot = _Poly.Center;
            //scale from the center.
            for (int i = 0; i < PolyPoints.Length; i++)
            {
                //get x and y difference from this point to the center...
                PointF difference = new PointF
                (
                    (float) expandfactor * (PolyPoints[i].X - CenterSpot.X),
                    (float) expandfactor * (PolyPoints[i].Y - CenterSpot.Y));


                //increase the difference by the factor.
                PolyPoints[i] = new PointF(CenterSpot.X + difference.X, CenterSpot.Y + difference.Y);
            }
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            base.PerformFrame(gamestate);
            currentRotation += RotationSpeed;

            if (Math.Abs(expansionfactor - 1) > 0.0002d)
            {
                //scale the Polygon from the center.
                ScalePoly(expansionfactor, expansionamount);


                //this.PolyPoints 
            }

            Velocity = new PointF(Velocity.X * 0.97f, Velocity.Y * 0.97f);
            mAliveFrames--;


            return (0 >= mAliveFrames);
        }
    }


    //LineParticle is a "particle" that holds two aggregate particles; It will call the 
    //appropriate "PerformFrame" implementations. Also, it can be set to draw one, both, or neither of it's aggregate particles.


    public class LineParticle : Particle
    {
        [Flags]
        public enum LineParticleDrawModeConstants
        {
            LPDM_Line = 2,
            LPDM_ParticleA = 4,
            LPDM_ParticleB = 8
        }

        //defaults to drawing it all.
        private LineParticleDrawModeConstants LineParticleDrawMode = LineParticleDrawModeConstants.LPDM_Line | LineParticleDrawModeConstants.LPDM_ParticleA | LineParticleDrawModeConstants.LPDM_ParticleB;
        private Particle[] _EndPoints = new Particle[2];

        public Particle ParticleA
        {
            get { return _EndPoints[0]; }
            set { _EndPoints[0] = value; }
        }

        public Particle ParticleB
        {
            get { return _EndPoints[1]; }
            set { _EndPoints[1] = value; }
        }

        private Pen _linePen = new Pen(Color.Black); //may as well default to something.
        private int _TTL = 10000; //ms

        public int TTL
        {
            get { return TTL; }
            set { _TTL = value; }
        }

        private DateTime? LiveTime;

        private TimeSpan getAliveTime
        {
            get
            {
                if (LiveTime == null) return new TimeSpan();
                else return DateTime.Now - LiveTime.Value;
            }
        }

        private Color mPenColor;
        private bool usecolor = false;

        public Pen LinePen
        {
            get { return _linePen; }
            set { _linePen = value; }
        }


        private int CalculateAlpha()
        {
            //get percentage of our TTL we have been alive...
            float percentage = ((float) getAliveTime.TotalMilliseconds) / (float) _TTL;
            return TrigFunctions.ClampValue(255 - (int) (255f * percentage), 0, 255);
        }

        public LineParticle(Particle EndPointA, Particle EndPointB, Color linecolor)
            : base(TrigFunctions.MidPoint(EndPointA.Location, EndPointB.Location))
        {
            ParticleA = EndPointA;
            ParticleB = EndPointB;
            usecolor = true;
            mPenColor = linecolor;
        }

        public LineParticle(Particle EndPointA, Particle EndPointB, Pen LinePen) : base(TrigFunctions.MidPoint(EndPointA.Location, EndPointB.Location))
        {
            ParticleA = EndPointA;
            ParticleB = EndPointB;
            _linePen = LinePen;
        }


        public override void Draw(Graphics g)
        {
            if ((LineParticleDrawMode & LineParticleDrawModeConstants.LPDM_ParticleA) == LineParticleDrawModeConstants.LPDM_ParticleA)
            {
                //particleA...
                ParticleA.Draw(g);
            }

            if ((LineParticleDrawMode & LineParticleDrawModeConstants.LPDM_ParticleB) == LineParticleDrawModeConstants.LPDM_ParticleB)
            {
                ParticleB.Draw(g);
            }

            if ((LineParticleDrawMode & LineParticleDrawModeConstants.LPDM_Line) == LineParticleDrawModeConstants.LPDM_Line)
            {
                if (usecolor)
                {
                    _linePen = new Pen(Color.FromArgb(CalculateAlpha(), mPenColor), 10);
                }

                g.DrawLine(_linePen, ParticleA.Location, ParticleB.Location);
            }
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            if (LiveTime == null) LiveTime = DateTime.Now;
            ParticleA.PerformFrame(gamestate);
            ParticleB.PerformFrame(gamestate);
            Location = TrigFunctions.MidPoint(ParticleA.Location, ParticleB.Location);
            return getAliveTime.TotalMilliseconds > _TTL;
        }
    }

    /// <summary>
    /// DebrisParticle: a particle of debris, starts with Location, Velocity, Rotation Angle and Rotation speed; and a few other values.
    /// </summary>
    public class DebrisParticle : Particle
    {
        protected int mAliveFrames = 0;
        public Image[] DebrisImageFrames;

        public int currframe = 0;

        //public PointF Velocity { get; set; }
        public double currentRotation = 0;
        public double RotationSpeed = 0.5;
        public SizeF SizeScale = new SizeF(.25f, .25f);


        public DebrisParticle(Image[] ImageFrames, PointF pLocation, PointF pVelocity, double pRotation, double pRotationSpeed) : base(pLocation)
        {
            mAliveFrames = 750;
            DebrisImageFrames = ImageFrames;
            Velocity = pVelocity;
            currentRotation = pRotation;
            RotationSpeed = pRotationSpeed;
        }


        protected Image GetCurrentFrame()
        {
            return DebrisImageFrames[currframe];
        }

        protected void IncrementFrame()
        {
            if (currframe == DebrisImageFrames.Length) currframe = 0;
            else currframe++;
        }

        private PointF CenterPoint()
        {
            Image currframe = GetCurrentFrame();

            SizeF actualsize = new SizeF(currframe.Width * SizeScale.Width, currframe.Height * SizeScale.Height);


            return new PointF(Location.X + (actualsize.Width / 2), Location.Y + (actualsize.Height / 2));
        }

        //TODO: add ImageAttributes...
        Matrix rotationmatrix = new Matrix();

        public override void Draw(Graphics g)
        {
            //draw the image; here we should also set image attributes so it fades out.
            Image drawthis = GetCurrentFrame();
            SizeF truesize = new SizeF((float) SizeScale.Width * drawthis.Width, (float) SizeScale.Height * drawthis.Height);

            Matrix cached = g.Transform;
            rotationmatrix.Reset();
            rotationmatrix.RotateAt((float) currentRotation, CenterPoint());
            //rotationmatrix.Scale(SizeScale.Width, SizeScale.Height);
            g.Transform = rotationmatrix;
            g.DrawImage
            (drawthis, new Rectangle((int) Location.X, (int) Location.Y, (int) truesize.Width, (int) truesize.Height),
                0, 0, drawthis.Width, drawthis.Height, GraphicsUnit.Pixel);
            g.Transform = cached;
            //FIX THIS DAMMIT.
            /*
            SizeF scaledSize = new SizeF(drawthis.Width * SizeScale.Width, drawthis.Height * SizeScale.Height);
            
            g.TranslateTransform(Location.X, Location.Y);
            
            g.TranslateTransform((scaledSize.Width/2), (scaledSize.Height/2));
            
            g.RotateTransform((float)currentRotation);
            g.ScaleTransform(SizeScale.Width, SizeScale.Height);
            //draw here...
           
           
             * 
             * 
             * 
            g.DrawImage(drawthis,0,0);

            g.ResetTransform();
            
            //g.TranslateTransform(-Location.X, -Location.Y);
            //g.ScaleTransform(1/SizeScale.Width, 1/SizeScale.Height);
            //g.RotateTransform((float)-currentRotation);
             */
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            base.PerformFrame(gamestate);
            currentRotation += RotationSpeed;
            //Location = new PointF(Location.X+Velocity.X,Location.Y+Velocity.Y);
            Velocity = new PointF(Velocity.X * 0.9f, Velocity.Y * 0.9f);
            mAliveFrames--;
            return (0 >= mAliveFrames);
        }
    }


    //water particle
    //descends from starting position to bottom of playing area.
    public class WaterParticle : Particle
    {
        private static Color DefBaseColour = Color.Blue;
        private Color BaseColour;
        private Color ParticleColour;
        private static Random rGen = TetrisGame.rgen;
        private PointF mVelocity;
        private Brush ParticleBrush;
        private long mTTL, InitialTTL;
        private float macceleration = 0;

        public WaterParticle(PointF ParticleLocation, Color PColor)
            : this(ParticleLocation, PointF.Empty, PColor)
        {
        }

        public WaterParticle(PointF ParticleLocation, PointF InitialVelocity, Color PColor)
            : base(ParticleLocation)
        {
            BaseColour = PColor;
            ParticleColour = Color.FromArgb(255, PColor);
            mVelocity = InitialVelocity;
            ParticleBrush = new SolidBrush(ParticleColour);
            mTTL = 75;
            InitialTTL = mTTL;
        }

        public WaterParticle(PointF ParticleLocation, PointF InitialVelocity) : this(ParticleLocation, InitialVelocity, DefBaseColour)
        {
        }


        public override void Draw(Graphics g)
        {
            if (ParticleBrush == null) ParticleBrush = new SolidBrush(ParticleColour);
            //g.FillRectangle(ParticleBrush,new RectangleF(Location.X-1,Location.Y-1,2,2));
            g.FillEllipse(ParticleBrush, new RectangleF(Location.X - 1, Location.Y - 1, 2, 2));
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            base.PerformFrame(gamestate);
            //select a new alpha based on the value of mTTL; basically, a percentage of InitialTTL:
            int newalpha = (int) ((((float) mTTL) / InitialTTL) * 255);

            if (newalpha < 0) newalpha = 0;
            if (newalpha > 255) newalpha = 255;

            ParticleColour = Color.FromArgb(newalpha, BaseColour);
            ParticleColour = BaseColour;
            mTTL--;
            ParticleBrush = new SolidBrush(ParticleColour);


            //IStateOwner.IncrementLocation(gamestate, ref Location, mVelocity);

            //particle "animation"... strictly, interaction with other elements. slows everything down, though...
            /*
            if (gamestate.ClientObject != null)
            {

                Color canvascolor = gamestate.ClientObject.GetCanvasPixel((int)newLocation.X, (int)newLocation.Y);
                Color BGColor = gamestate.ClientObject.getBackgroundBitmap().GetPixel((int)newLocation.X, (int)newLocation.Y);

                if (canvascolor != BGColor)
                {
                    mVelocity.Y = 0;
                    macceleration *= 0.8f;

                }
            }
             */

            //mVelocity = new PointF(mVelocity.X*0.99f,mVelocity.Y+macceleration);
            _GravityAcceleration = new PointF(_GravityAcceleration.X, _GravityAcceleration.Y + 0.1f);
            return (mTTL < 0);
        }
    }

    //basic implementation (used for testing by being generated by balls)

    public class DustParticle : Particle
    {
        private Color mDustColor = Color.Brown;

        //public PointF Velocity { get; set; }
        //private PointF Velocity;
        private int minitialAliveFrames = 45;
        private int mAliveFrames = 45;

        public int TTL
        {
            set
            {
                minitialAliveFrames = value;
                mAliveFrames = value;
            }
        }

        private static Random rGen
        {
            get { return TetrisGame.rgen; }
        }

        public DustParticle(PointF pLocation, int TTL) : this(pLocation)
        {
            minitialAliveFrames = TTL;
            mAliveFrames = TTL;
        }

        public DustParticle(PointF pLocation, float maxspeed, int TTL, Color dustcolor) : this(pLocation, maxspeed)
        {
            minitialAliveFrames = TTL;
            mAliveFrames = TTL;
            mDustColor = dustcolor;
        }

        public DustParticle(PointF pLocation, float maxspeed) : base(pLocation)
        {
            float halfmax = maxspeed / 2;
            Velocity = new PointF((float) (TetrisGame.rgen.NextDouble() * maxspeed) - halfmax, (float) (TetrisGame.rgen.NextDouble() * maxspeed) - halfmax);
        }

        public DustParticle() : base(PointF.Empty)
        {
        }

        public DustParticle(PointF pLocation) : base(pLocation)
        {
            Velocity = new PointF((float) (TetrisGame.rgen.NextDouble() * 2) - 1, (float) (TetrisGame.rgen.NextDouble() * 2) - 1);
        }


        public DustParticle(PointF plocation, PointF pVelocity) : base(plocation)
        {
            Velocity = pVelocity;
        }

        public override void Draw(Graphics g)
        {
            //SetPixel(g.GetHdc(),(int)Location.X,(int)Location.Y);
            //g.ReleaseHdc();
            g.FillRectangle(new SolidBrush(mDustColor), Location.X - 1, Location.Y - 1, 2, 2);
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            Location = new PointF(Location.X + Velocity.X, Location.Y + Velocity.Y);

            //Velocity = new PointF((float)Math.Sqrt(Math.Abs(Velocity.X)) * Math.Sign(Velocity.X),
            //    (float)Math.Sqrt(Math.Abs(Velocity.Y)) * Math.Sign(Velocity.Y));

            Velocity = new PointF(Velocity.X * 0.9f, Velocity.Y * 0.9f);

            mAliveFrames--;
            if (mDustColor.A >= 4)
                mDustColor = Color.FromArgb(mDustColor.A - 4, mDustColor);

            return (0 >= mAliveFrames);
        }
    }

    public class Sparkle : Particle
    {
        //protected PointF _Location;
        //protected PointF _Velocity;

        public static readonly Brush DefaultSparkleBrush = new SolidBrush(Color.Yellow);
        protected Brush _SparkleBrush = DefaultSparkleBrush;
        protected Pen _SparklePen;
        protected TimeSpan _LifeTime = new TimeSpan(0, 0, 0, 1); //default life of 1 second. Short and fleeting.
        protected long LifeTicks = 0;
        protected float currentRadius = 0;
        protected DateTime StartLife; //start of this sparkle's life.

        /*
        public PointF Location
        {
            get { return _Location; }
            private set { _Location = value; }
        }

        public PointF Velocity
        {
            get { return _Velocity; }
            set { _Velocity = value; }
        }
        */


        public Brush SparkleBrush
        {
            get { return _SparkleBrush; }
            set
            {
                _SparkleBrush = value;
                _SparklePen = new Pen(_SparkleBrush);
            }
        }

        public TimeSpan LifeTime
        {
            get { return _LifeTime; }
            set
            {
                _LifeTime = value;
                LifeTicks = _LifeTime.Ticks;
            }
        }


        public Sparkle(PointF Pos)
            : this(Pos, new PointF(0, 0))
        {
        }

        public Sparkle(PointF pLocation, PointF pVelocity)
            : this(pLocation, pVelocity, DefaultSparkleBrush)
        {
        }

        public Sparkle(PointF pLocation, PointF pVelocity, Color sparklecolor)
            : this(pLocation, pVelocity, new SolidBrush(sparklecolor))
        {
        }

        public Sparkle(PointF Pos, PointF pVelocity, Brush pSparkleBrush) : base(Pos)
        {
            _Location = Pos;
            _Velocity = pVelocity;
            SparkleBrush = pSparkleBrush;
            StartLife = DateTime.Now;
            LifeTime = _LifeTime;
        }

        /// <summary>
        /// used to perform a single frame of this gameobjects animation.
        /// </summary>
        /// <param name="gamestate">Game State object</param>
        /// <param name="AddObjects">out parameter, populate with any new GameObjects that will be added to the game.</param>
        /// <param name="removeobjects">otu parameter, populate with gameobjects that should be deleted.</param>
        /// <returns>true to indicate that this gameobject should be removed. False otherwise.</returns>
        /// 
        private float _maxradius = 12;

        public float MaxRadius
        {
            get { return _maxradius; }
            set { _maxradius = value; }
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            _Location = new PointF(_Location.X + _Velocity.X, _Location.Y + _Velocity.Y);
            //get total ticks...
            long livingticks = (DateTime.Now - StartLife).Ticks;
            float percentage = ((float) livingticks) / (float) LifeTicks;
            if (percentage < 0.5f)
                currentRadius = percentage * 2 * _maxradius;
            else
                currentRadius = (1 - (Math.Abs(0.5f - percentage))) * _maxradius;


            _Velocity = new PointF(_Velocity.X * _VelocityDecay.X, _Velocity.Y * _VelocityDecay.Y);
            return (DateTime.Now - StartLife) > LifeTime;
        }

        public override void Draw(Graphics g)
        {
            if (currentRadius < 1) return;
            //throw new NotImplementedException();
            g.DrawLine(_SparklePen, Location.X - currentRadius / 2, Location.Y, Location.X + currentRadius / 2, Location.Y);
            g.DrawLine(_SparklePen, Location.X, Location.Y - currentRadius, Location.X, Location.Y + currentRadius);
        }
    }


    public class FireParticle : Particle
    {
        public int mFrameCount = 0;
        public int TTL = 15;

        public FireParticle(PointF Location) : base(Location)
        {
            Velocity = new PointF((float) (TetrisGame.rgen.NextDouble() * .4f) - .2f, (float) (TetrisGame.rgen.NextDouble() * .4f) - .2f);
        }

        public Color MixColor(Color ColorA, Color ColorB, float percentage)
        {
            float[] ColorAValues = new float[] {(float) ColorA.A, (float) ColorA.R, (float) ColorA.G, (float) ColorA.B};
            float[] ColorBValues = new float[] {(float) ColorB.A, (float) ColorB.R, (float) ColorB.G, (float) ColorB.B};
            float[] ColorCValues = new float[4];


            for (int i = 0; i <= 3; i++)
            {
                ColorCValues[i] = (ColorAValues[i] * percentage) + (ColorBValues[i] * (1 - percentage));
            }


            return Color.FromArgb((int) ColorCValues[0], (int) ColorCValues[1], (int) ColorCValues[2], (int) ColorCValues[3]);
        }

        public Color GetFireColor()
        {
            //stages:
            //firecolor starts out blue:
            //0% BLUE (alpha=95)
            //10%YELLOW (alpha=90)
            //25%RED (alpha=90)
            //35%RED (alpha=95)
            //80% (BLACK (alpha=100%))
            //100% BLACK, 0 ALPHA.

            Color[] firecolors = new Color[]
            {
                Color.Blue,
                Color.FromArgb(230, Color.Yellow),
                Color.FromArgb(200, Color.Orange),
                Color.FromArgb(150, Color.DarkOrange),
                Color.FromArgb(150, Color.Red),
                Color.FromArgb(0, Color.DarkRed)
            };


            float currentpercentage = (float) mFrameCount / (float) TTL;
            Blend x = new Blend();
            float innerpercent = 0;
            Color ColorA = Color.Red, ColorB = Color.Black;
            if (currentpercentage <= 0.1f)
            {
                //blue to yellow, depending on
                innerpercent = currentpercentage / 0.1f;
                ColorA = firecolors[0];
                ColorB = firecolors[1];
            }
            else if (currentpercentage <= .25f)
            {
                innerpercent = ((currentpercentage - 0.1f) / .15f);
                ColorA = firecolors[1];
                ColorB = firecolors[2];
            }
            else if (currentpercentage <= .35f)
            {
                innerpercent = ((currentpercentage - 0.25f) / .10f);
                ColorA = firecolors[2];
                ColorB = firecolors[3];
            }
            else if (currentpercentage <= .8f)
            {
                innerpercent = ((currentpercentage - 0.35f) / .45f);
                ColorA = firecolors[3];
                ColorB = firecolors[4];
            }
            else if (currentpercentage == 1)
            {
                innerpercent = ((currentpercentage - .8f) / .2f);
                ColorA = firecolors[4];
                ColorB = firecolors[5];
            }

            return MixColor(ColorA, ColorB, innerpercent);
            //mix ColorA and ColorB, with "innerpercent" of ColorB.
        }


        public override void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(GetFireColor()), Location.X - 1, Location.Y - 1, 2, 2);
        }

        public override bool PerformFrame(IStateOwner gamestate)
        {
            mFrameCount++;

            Location = new PointF(Location.X + Velocity.X, Location.Y + Velocity.Y);

            return (mFrameCount >= TTL);
        }
    }

    /// <summary>
    /// EmitterParticle: a "dummy" particle that emits other particles during it's lifetime.
    /// </summary>
    public class EmitterParticle : Particle
    {
        protected const int LiveTime = 20;
        protected int TTL = LiveTime;
        protected int FrameNum = 0;
        protected int GenerateDelay = 5;

        public delegate Particle EmissionRoutine(IStateOwner gstate, EmitterParticle Source, int FrameNum, int TTL);

        private Random rGen = TetrisGame.rgen;
        protected EmissionRoutine EmissionFunction;


        public EmitterParticle(PointF pLocation, EmissionRoutine emitroutine) : base(pLocation)
        {
            EmissionFunction = emitroutine;
        }

        public override void Draw(Graphics g)
        {
            //throw new NotImplementedException();
            //nothing.
        }

        private bool proxyaddparticle(ProxyObject sourceobject, IStateOwner gamestate)
        {
            Particle addit = sourceobject.Tag as Particle;
            //if (addit != null)
                //gamestate.  AddParticle(addit);


            return true;
        }

        public override bool PerformFrame(IStateOwner pOwner)
        {
            TTL--;
            FrameNum++;
            Particle addme = EmissionFunction(pOwner, this, FrameNum, TTL);
            ProxyObject addproxy = new ProxyObject(proxyaddparticle, null);
            addproxy.Tag = addme;

            pOwner.AddGameObject(addproxy);


            return base.PerformFrame(pOwner) || TTL < 0;
        }

        private Particle DefaultEmissionRoutine(IStateOwner pStateOwner, EmitterParticle Source, int FrameNum, int TTL)
        {
            if (FrameNum % GenerateDelay == 0)
            {
                return new DustParticle(Source.Location, 3, 50, Color.Red);
            }

            return null;
        }
    }

    /// <summary>
    /// Generic particle class that accepts a particle as a type argument, derives from EmitterParticle, and 
    /// creates that particle.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericEmitter<T> : EmitterParticle where T : Particle
    {
        private Particle DefaultEmissionRoutine(IStateOwner gs, EmitterParticle Source, int FrameNum, int TTL)
        {
            if (FrameNum % GenerateDelay == 0)
            {
                return (T) Activator.CreateInstance(typeof(T), new object[] {Source.Location});
            }

            return null;
        }

        public GenericEmitter(PointF pLocation)
            : base(pLocation, null)
        {
            EmissionFunction = DefaultEmissionRoutine;
        }

        public override void Draw(Graphics g)
        {
            //base.Draw(g);
        }

        public override bool PerformFrame(IStateOwner pStateOwner)
        {
            return base.PerformFrame(pStateOwner);
        }
    }

    public class LightOrb : Particle
    {
        //'lightorb' is a (attempt) to simulate a "light". Since GameObjects are drawn last... (iirc) it seemed reasonable to do it this way.
        //the basic idea is the light is just a radial gradient going from 50% alpha of the colour to 100% alpha (transparent) on the edges, with the given radius.


        private float _Radius;
        private Color _LightColor;
        private PathGradientBrush drawbrush = null;
        private int _TTL = 140;
        private int LiveTime = 0;
        private const int MaxAlpha = 100;
        private const int MinAlpha = 0;
        private float _RadiusDecay = 0.99f;

        public float RadiusDecay
        {
            get { return _RadiusDecay; }
            set { _RadiusDecay = value; }
        }

        public int TTL
        {
            get { return _TTL; }
            set { _TTL = value; }
        }

        public LightOrb(PointF pLocation, Color LightColor, float LightRadius)
            : base(pLocation)
        {
            _Radius = LightRadius;
            _LightColor = LightColor;
        }

        private Rectangle LightRect()
        {
            return new Rectangle((int) (Location.X - _Radius), (int) (Location.Y - _Radius), (int) (_Radius * 2), (int) (_Radius * 2));
        }

        public override bool PerformFrame(IStateOwner StateOwner)
        {
            //decay the colour...
            LiveTime++;
            _Radius *= _RadiusDecay;
            return base.PerformFrame(StateOwner) || LiveTime >= TTL;
        }

        public static Image DrawLightOrb(Size DrawSize, Color usecolor)
        {
            //cheat... create a new LightOrb object...
            LightOrb lo = new LightOrb(new PointF(DrawSize.Width / 2, DrawSize.Height / 2), usecolor, Math.Min(DrawSize.Width, DrawSize.Height) / 2);
            Bitmap usebitmap = new Bitmap(DrawSize.Width, DrawSize.Height, PixelFormat.Format32bppPArgb);
            Graphics useg = Graphics.FromImage(usebitmap);
            lo.Draw(useg);
            return usebitmap;
        }

        public override void Draw(Graphics g)
        {
            float LivePercent = (float) LiveTime / (float) TTL;
            GraphicsPath gp = new GraphicsPath();

            Rectangle glr = LightRect();
            if (glr.Width == 0 || glr.Height == 0) return;
            gp.AddEllipse(glr);
            PathGradientBrush pgb;
            try
            {
                pgb = new PathGradientBrush(gp);
            }
            catch (OutOfMemoryException ome)
            {
                Debug.Print("Out of Memory...");
                return;
            }

            pgb.CenterPoint = TrigFunctions.CenterPoint(glr);
            pgb.CenterColor = Color.FromArgb((int) (MaxAlpha * (1 - LivePercent)), _LightColor);
            pgb.SurroundColors = new Color[] {Color.FromArgb(0, _LightColor)};

            g.FillPath(pgb, gp);
            pgb.Dispose();
            gp.Dispose();
        }
    }
}