using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BASeTris.GameStates;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Rendering.Skia;
using BASeTris.Blocks;
using SkiaSharp;
using BASeTris.Settings;

namespace BASeTris
{
    //TODO: change it so that Nomino's can accept the gamekey input. Will have to refactor what we have already...
    public class Nomino : IEnumerable<NominoElement>
    {
        public String SpecialName { get; set; }

        public bool Controllable { get; set; } = true;
        public bool PlaceSound { get; set; } = true;
        public bool MoveSound { get; set; } = false;
        public int FallSpeed { get; set; } = 250; //Higher is slower, number of ms between movements.
        public int InitialY { get; set; } = 0;
        public int X { get; set; }
        private int _Y = 0;
        public int Y { get { return _Y; } private set{ _Y = value; } }
        public bool NoGhost { get; set; } = false;
        public void SetY(IStateOwner pOwner,int Value)
        {
            if(pOwner!=null) LastFall = pOwner.GetElapsedTime();
            Y = Value;
        }
        private int XMin, XMax, YMin, YMax;
        private Rectangle _GroupExtents = Rectangle.Empty;
        public TimeSpan LastFall = TimeSpan.Zero;
        public float HighestHeightValue = 0;
        
        public float GetHeightTranslation(IStateOwner pOwner,float BlockHeight)
        {
            if (!pOwner.CurrentState.GamePlayActive)
            {
                return 0;
            }
            else
            {
                double Percent = ((pOwner.GetElapsedTime() - LastFall).TotalMilliseconds) / (double)FallSpeed;
                if (Percent > 1) Percent = 1; if (Percent < 0) Percent = 0;
                return (float)(((float)BlockHeight) * (Percent));
            }
        }
        public Rectangle GroupExtents
        {
            get { return _GroupExtents; }
        }

        protected List<NominoElement> BlockData = new List<NominoElement>();
        private Dictionary<NominoBlock, NominoElement> _DataLookup = null;
        public IList<NominoElement> GetBlockData()
        {
            return BlockData.AsReadOnly();
        }
        public int IndexOf(NominoElement findItem)
        {
            return BlockData.IndexOf(findItem);
        }
        public Dictionary<NominoBlock, NominoElement> BlockDataLookup
        {
            get
            {
                if (_DataLookup == null)
                {
                    _DataLookup = new Dictionary<NominoBlock, NominoElement>();
                    foreach (var addelement in this)
                    {
                        _DataLookup.Add(addelement.Block, addelement);
                    }
                }

                return _DataLookup;
            }
        }

        private DateTime LastRotationCall = DateTime.MinValue;
        
        /// <summary>
        /// called repeatedly for active groups, normally at a rate determined by the current level.
        /// return true to cancel standard handling. (eg. Block falling)
        /// </summary>
        /// <param name="pOwner"></param>
        /// <returns></returns>
        public virtual bool HandleBlockOperation(IStateOwner pOwner)
        {
            return false;
        }

        /// <summary>
        /// Called before default handling of a key by the game. return true if the key was handled to stop the default handling of said key.
        /// Note that this is called on all the blockGroups in play, so returning true won't stop other blockgroups from receiving the key themselves.
        /// </summary>
        /// <param name="pOwner"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool HandleGameKey(IStateOwner pOwner,GameState.GameKeys key)
        {
            return false;
        }
        public void RecalcExtents()
        {
            foreach (var iterateentry in this)
            {
                if (iterateentry.X < XMin) XMin = iterateentry.X;
                if (iterateentry.X > XMax) XMax = iterateentry.X;
                if (iterateentry.Y < YMin) YMin = iterateentry.Y;
                if (iterateentry.Y > YMax) YMax = iterateentry.Y;
            }

            _GroupExtents = new Rectangle(XMin, YMin, XMax - XMin, YMax - YMin);
        }

        protected void SetBlockOwner()
        {
            foreach (var loopentry in BlockData)
            {
                loopentry.Block.Owner = this;
            }
        }

        public override string ToString()
        {
            return "Nomino:" + BlockData.Count + " Blocks ";
        }

        public bool IsActive(TetrisField pField)
        {
            return pField.BlockGroups.Any((b) => b == this);
        }
        
        public Image GetImage(SizeF BlockSize)
        {
            RecalcExtents();

            Size BitmapSize = new Size((int) BlockSize.Width * (_GroupExtents.Width + 1), (int) BlockSize.Height * (_GroupExtents.Height + 1));

            //generate a new image.
            Bitmap BuiltRepresentation = new Bitmap(BitmapSize.Width, BitmapSize.Height, PixelFormat.Format32bppPArgb);
            using (Graphics DrawRep = Graphics.FromImage(BuiltRepresentation))
            {
                DrawRep.CompositingQuality = CompositingQuality.HighSpeed;
                DrawRep.InterpolationMode = InterpolationMode.NearestNeighbor;
                DrawRep.SmoothingMode = SmoothingMode.HighSpeed;
                foreach (NominoElement bge in this)
                {
                    RectangleF DrawPos = new RectangleF(BlockSize.Width * (bge.X - _GroupExtents.X), BlockSize.Height * (bge.Y - _GroupExtents.Y), BlockSize.Width, BlockSize.Height);
                    TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(DrawRep, DrawPos, this,new SettingsManager());
                    RenderingProvider.Static.DrawElement(null,tbd.g,bge.Block,tbd);
                    //bge.Block.DrawBlock(tbd);
                    
                }
            }

            return BuiltRepresentation;
        }
        //SkiaSharp implementation of GetImage.
        public SKBitmap GetImageSK(SKSize BlockSize)
        {
            RecalcExtents();
            Size BitmapSize = new Size((int)BlockSize.Width * (_GroupExtents.Width + 1), (int)BlockSize.Height * (_GroupExtents.Height + 1));
            SKImageInfo info = new SKImageInfo(BitmapSize.Width,BitmapSize.Height,SKColorType.Rgba8888, SKAlphaType.Premul);
            SKBitmap BuiltRepresentation = new SKBitmap(info,SKBitmapAllocFlags.ZeroPixels);
            using (SKCanvas DrawRep = new SKCanvas(BuiltRepresentation))
            {
                DrawRep.Clear(SKColors.Transparent);
                foreach(NominoElement bge in this)
                {
                    //RectangleF DrawPos = new RectangleF(BlockSize.Width * (bge.X - _GroupExtents.X), BlockSize.Height * (bge.Y - _GroupExtents.Y), BlockSize.Width, BlockSize.Height);
                    var Left = BlockSize.Width * (bge.X - _GroupExtents.X);
                    var Top = BlockSize.Height * (bge.Y - _GroupExtents.Y);
                    SKRect DrawPos = new SKRect(Left, Top, Left + BlockSize.Width, Top + BlockSize.Height);
                    TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(DrawRep,DrawPos,this,new SettingsManager());
                    RenderingProvider.Static.DrawElement(null,tbd.g,bge.Block,tbd);
                }
            }
            return BuiltRepresentation;
        }

        public Nomino(Nomino sourcebg)
        {
            FallSpeed = sourcebg.FallSpeed;
            X = sourcebg.X;
            Y = sourcebg.Y;
            foreach (var cloneentry in sourcebg.BlockData)
            {
                AddBlock(new NominoElement(cloneentry));
            }
        }

        public Nomino()
        {
            XMin = YMin = int.MaxValue;
            XMax = YMax = int.MinValue;
        }

        private void AddBlock(NominoElement bge)
        {
            if (bge.X < XMin) XMin = bge.X;
            if (bge.X > XMax) XMax = bge.X;
            if (bge.Y < YMin) YMin = bge.Y;
            if (bge.Y > YMax) YMax = bge.Y;
            _GroupExtents = new Rectangle(XMin, YMin, XMax - XMin, YMax - YMin);
            BlockData.Add(bge);
        }
        public bool HasBlock(NominoBlock block)
        {
            return BlockDataLookup.ContainsKey(block);
        }
        public void AddBlock(Point[] RotationPoints, NominoBlock tb)
        {
            NominoElement bge = new NominoElement(RotationPoints, tb);
            AddBlock(bge);
        }
        public void RemoveBlock(NominoBlock tb)
        {
            BlockData.Remove(FindEntry(tb));
            //_DataLookup = null;
        }

        public NominoElement FindEntry(NominoBlock findBlock)
        {
            return BlockDataLookup[findBlock];
        }

        public void Clamp(int RowCount, int ColCount)
        {
            //check X Coordinate.
            int MinimumX = int.MaxValue, MinimumY = int.MaxValue;
            int MaximumX = int.MinValue, MaximumY = int.MinValue;
            foreach (var iterateentry in this)
            {
                if (iterateentry.X + X < MinimumX) MinimumX = iterateentry.X + X;
                if (iterateentry.X + X > MaximumX) MaximumX = iterateentry.X + X;
                if (iterateentry.Y + Y < MinimumY) MinimumY = iterateentry.Y + Y;
                if (iterateentry.Y + Y > MaximumY) MaximumY = iterateentry.Y + Y;
            }

            if (MinimumX < 0) X = X + Math.Abs(MinimumX);
            if (MaximumX > ColCount) X = X - (MaximumX - ColCount);

            if (MinimumY < 0) Y = Y + Math.Abs(MinimumY);
            if (MaximumY > RowCount) Y = Y - (MaximumY - RowCount);
        }

        public IEnumerator<NominoElement> GetEnumerator()
        {
            return BlockData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Nomino GetTetromino_Array(Point[][] Source, String pName)
        {
            Nomino bg = new Nomino();
            bg.SpecialName = pName;
            foreach (var bge in GetNominoEntries(Source))
            {
                bge.Block.Owner = bg;
                bg.AddBlock(bge);
            }

            return bg;
        }
        
        public static IEnumerable<NominoElement> GetNominoEntries(Point[] Source, Size AreaSize,Func<int,NominoBlock> BuildBlock = null)
        {
            if (BuildBlock == null) BuildBlock = (i) => new StandardColouredBlock();
            //assumes a "single" set of blocks, we rotate it with the NominoElement Constructor for the needed rotation points.
            int index = 0;
            foreach (Point BlockPos in Source)
            {
                NominoBlock CreateBlock = BuildBlock(index);
                var ne = new NominoElement(BlockPos, AreaSize, CreateBlock);
                yield return ne;
                index++;
            }
        }

        public static IEnumerable<NominoElement> GetNominoEntries(Point[][] Source, Func<int,NominoBlock> BuildBlock = null)
        {
            if (BuildBlock == null) BuildBlock = (i) => new StandardColouredBlock();
            int index = 0;
            foreach (Point[] loopposdata in Source)
            {
                NominoBlock CreateBlock = BuildBlock(index);
                yield return new NominoElement(loopposdata, CreateBlock);
                index++;
            }
        }

        public static double GetAngle(PointF PointA, PointF PointB)
        {
            return Math.Atan2(PointB.Y - PointA.Y, PointB.X - PointA.X);
        }

        
        public DateTime GetLastRotation()
        {
            return LastRotationCall;
        }

        public bool LastRotateCCW = false;

        public void Rotate(bool CCW)
        {
            foreach (var iterateblock in BlockData)
            {
                if (CCW)
                {
                    iterateblock.RotationModulo--;
                }
                else
                    iterateblock.RotationModulo++;

                
                if (iterateblock.RotationModulo == 0)
                {
                    Debug.Print("Err");
                }
            }

            LastRotateCCW = CCW;
            LastRotationCall = DateTime.Now;
        }

        public static float Distance(PointF PointA, PointF PointB)
        {
            return (float) Math.Sqrt(Math.Pow(PointB.X - PointA.X, 2) + Math.Pow(PointB.Y - PointA.Y, 2));
        }
    }

    public class NominoElement
    {
        //Represents a single block within a group. the RotationPoints represent the positions this specific block will rotate/change to when rotated.
        public static int sMod(int A, int B)
        {
            return (A % B + B) % B;
        }

        public int X
        {
            get { return Positions[sMod(RotationModulo, Positions.Length)].X; }
        }

        public int Y
        {
            get { return Positions[sMod(RotationModulo, Positions.Length)].Y; }
        }

        Point[] Positions = null;

        public static Point RotatePoint(Point pSource, Size AreaSize)
        {
            return new Point(AreaSize.Width - pSource.Y, pSource.X);
        }

        public static Point[] GetRotations(Point InitialPoint, Size AreaSize)
        {
            Point[] result = new Point[4];
            result[0] = InitialPoint;
            result[1] = RotatePoint(InitialPoint, AreaSize);
            result[2] = RotatePoint(result[1], AreaSize);
            result[3] = RotatePoint(result[2], AreaSize);

            return result;
        }

        
        //each rotationmodule
        public int RotationModulo = 0;

        public NominoBlock Block;

        public NominoElement(Point Point, Size AreaSize, NominoBlock pBlock)
        {
            Positions = GetRotations(Point, AreaSize);
            Block = pBlock;
        }

        public NominoElement(Point[] RotationPoints, NominoBlock pBlock)
        {
            if (RotationPoints.Length == 0) throw new ArgumentException("RotationPoints");

            Positions = RotationPoints;
            Block = pBlock;
        }

        public NominoElement(NominoElement clonesource)
        {
            RotationModulo = clonesource.RotationModulo;
            Positions = new Point[clonesource.Positions.Length];
            clonesource.Positions.CopyTo(Positions, 0);
            Block = clonesource.Block;
            Block.Rotation = clonesource.RotationModulo;
        }
    }
    
}