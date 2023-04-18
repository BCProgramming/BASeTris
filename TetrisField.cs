using BASeTris.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using SkiaSharp;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Adapters;
using static BASeTris.CanFitResults;

namespace BASeTris
{
    public class TetrisField
    {
        //standard tetris field is 10 blocks wide, and 22 blocks tall- (the top 2 rows aren't drawn usually).
        //The "tetris field" here just represents the "permanent" blocks. eg, blocks that have been dropped, and "set" in place.
        //these get evaluated to check for lines when a new block is placed, for example.

            [Flags]
            public enum GameFlags
            {
                Flags_None=0, //Standard tetris.
                Flags_Hotline = 1, //perform Hotline drawing/logic
                Flags_Cascade = 2, //Perform "Cascade" logic. 
                Flags_Sticky = 4, //usually goes with Cascade but isn't required. This needs special tetromino handling, as well.
            }

        //public BaseStatistics GameStats = new TetrisStatistics();
        public event EventHandler<BlockGroupSetEventArgs> BlockGroupSet;
        
        private NominoBlock[][] FieldContents;

     
        public GameFlags _GameFlags = GameFlags.Flags_None;
        public GameFlags Flags { get { return _GameFlags; } set { _GameFlags = value; }}
        public Dictionary<int, HotLine> HotLines = new Dictionary<int, HotLine>();
        public event EventHandler<OnThemeChangeEventArgs> OnThemeChangeEvent;
        public event EventHandler<OnNewLineRowScroll> OnNewLineScroll;
        Dictionary<int, Dictionary<Color, TextureBrush>> HotLineTextures = new Dictionary<int, Dictionary<Color, TextureBrush>>();

        //used (or will be used...) by Tetris Attack mode.



        public float OffsetPaint { get; set; } //offset paint, this is in "blocks" and should be  >=0 and <1.
        

        public TextureBrush GetHotLineTexture(int pHeight, Color pColor)
        {
            if (!HotLineTextures.ContainsKey(pHeight))
            {
                HotLineTextures.Add(pHeight, new Dictionary<Color, TextureBrush>());
            }
            if (!HotLineTextures[pHeight].ContainsKey(pColor))
            {
                HotLineTextures[pHeight].Add(pColor, BuildHotLineTexture(pHeight, pColor));
            }
            return HotLineTextures[pHeight][pColor];
        }
        private TextureBrush BuildHotLineTexture(int pHeight, Color pColor)
        {

            Bitmap buildimage = new Bitmap(1, pHeight);
            using (Graphics buildg = Graphics.FromImage(buildimage))
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(0, 0, 1, pHeight), pColor, RenderHelpers.MixColor(pColor, Color.FromArgb(150, Color.Black)), LinearGradientMode.Vertical))
                {

                    buildg.FillRectangle(lgb, new Rectangle(0, 0, 1, pHeight));
                }
            }
            return new TextureBrush(buildimage, new Rectangle(0, 0, 1, pHeight));

        }
        public Nomino GetGhostDrop(IStateOwner pOwner, Nomino Source, out int dropLength, int CancelProximity = 3)
        {
            //routine returns the Ghost Drop representor of this Nomino.
            //this function will also return null if the dropped block is CancelProximity or closer to the place it would be dropped.
            Nomino Duplicator = new Nomino(Source);

            dropLength = 0;
            while (true)
            {
                var fitresult = CanFit(Duplicator, Duplicator.X, Duplicator.Y + 1, true, new Nomino[] { Source });
                //ghost drops will show up "through" any active blocks, since active blocks don't actual set blocks..
                if (fitresult.CanFit || fitresult.CantFit_Active )
                {
                    dropLength++;
                    Duplicator.SetY(pOwner, Duplicator.Y + 1);
                }
                else
                {
                    break;
                }
            }

            if (dropLength < CancelProximity) return null;
            foreach (var iterate in Duplicator)
            {
                iterate.Block.Owner = Source;
            }
            return Duplicator;
        }

      
        private NominoTheme _Theme = null;


        public NominoTheme Theme
        {
            get
            {
                return _Theme;
            }
            set
            {
                _Theme = value;
                SetFieldColors(_Handler);
                OnThemeChangeEvent?.Invoke(this,new OnThemeChangeEventArgs());
            }
        }

        //public TetrominoTheme Theme = new NESTetrominoTheme();
        //public TetrominoTheme Theme = new GameBoyTetrominoTheme();
        private List<Nomino> ActiveBlockGroups = new List<Nomino>();

        public IList<Nomino> GetActiveBlockGroups() => ActiveBlockGroups;
       
        public const int DEFAULT_ROWCOUNT = 22; //22;
        public const int DEFAULT_COLCOUNT = 10;//10;
        public const int DEFAULT_TOPHIDDENROWS = 2;
        public const int DEFAULT_BOTTOMHIDDENROWS = 0;
        public const int DEFAULT_VISIBLEROWS = 20; //20;
        private int _VisibleRows = DEFAULT_VISIBLEROWS;
        private int _HiddenRowsTop = DEFAULT_TOPHIDDENROWS;
        private int _HiddenRowsBottom = DEFAULT_BOTTOMHIDDENROWS;
        public int RowCount { get; set; } = DEFAULT_ROWCOUNT;
        public int ColCount { get; set; } = DEFAULT_COLCOUNT;

        public int VisibleRows { get { return _VisibleRows; } set { _VisibleRows = value; RecalcRows(); } }
        public int HIDDENROWS_TOP { get { return _HiddenRowsTop; } set { _HiddenRowsTop = value; RecalcRows(); } } 
        public int HIDDENROWS_BOTTOM { get { return _HiddenRowsBottom; } set { _HiddenRowsBottom = value;RecalcRows(); } } 

        private void RecalcRows()
        {
            RowCount = _VisibleRows + HIDDENROWS_BOTTOM + HIDDENROWS_TOP;
        }
        //const int ROWCOUNT = 44;
        //const int COLCOUNT = 20;
        Random rg = new Random();

     

        public IList<Nomino> BlockGroups
        {
            get { return new List<Nomino>(ActiveBlockGroups); }
        }


        public NominoBlock[][] Contents
        {
            get { return FieldContents; }
        }
        public IEnumerable<NominoBlock> AllContents()
        {

            for(int r=0;r<RowCount;r++)
            {
                for(int c = 0;c<ColCount;c++)
                {
                    yield return Contents[r][c];
                }
            }



        }
        /// <summary>
        /// sweeps through all blocks/cells and if that cell contains a Block which has an owner and that owner has only a single Nomino, then set the coordinate of that Nomino to the location of the block in the cell.
        /// </summary>
        public void VerifySingularBlocks()
        {

            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {
                    var checkblock = Contents[r][c];
                    if (checkblock != null)
                    {
                        if (checkblock.Owner!=null && checkblock.Owner.Count == 1)
                        {
                            if (checkblock.Owner.X != c)
                            {
                                checkblock.Owner.X = c;
                            }
                            if (checkblock.Owner.Y != r)
                            {
                                checkblock.Owner.Y = r;
                            }
                        }
                    }
                }
            }
        }


        public Dictionary<NominoBlock,Point> FindBlockLocations(IList<NominoBlock> Target)
        {
            Dictionary<NominoBlock, Point> constructresult = new Dictionary<NominoBlock, Point>();
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColCount; c++)
                {
                    var checkblock = Contents[r][c];
                    if(Target.Any((t)=>checkblock==t))
                    {
                        constructresult.Add(checkblock, new Point(c, r));
                    }
                }
            }
            return constructresult;
        }
       
        public void ClearContents()
        {
            lock (ActiveBlockGroups)
            {
                ActiveBlockGroups.Clear();
                foreach (var row in FieldContents)
                {
                    for (int i = 0; i < ColCount; i++)
                    {
                        row[i] = null;
                    }
                }
            }
        }

        public void AddBlockGroup(Nomino newGroup)
        {
            Debug.Print("Added:" + newGroup.ToString());
            lock (ActiveBlockGroups)
            {
                ActiveBlockGroups.Add(newGroup);
            }
        }
        public void ClearActiveBlockGroups()
        {
            lock(ActiveBlockGroups)
            {
                ActiveBlockGroups.Clear();
            }
        }
        public void RemoveBlockGroup(Nomino oldGroup)
        {
            lock (ActiveBlockGroups)
            {
                ActiveBlockGroups.Remove(oldGroup);
            }
        }
        public IEnumerable<int> FindCompleteRows()
        {
            for(int r = 0;r<Contents.Length-1;r++)
            {
                var FieldRow = Contents[r];
                if(FieldRow.All((n)=>n!=null))
                {
                    yield return r;
                }
            }

        }
        private IGameCustomizationHandler _Handler = null;
        public IGameCustomizationHandler Handler {  get { return _Handler; } }

        public int Level {
            get { return (Handler == null ? 0 : (Handler.Statistics is TetrisStatistics ts) ? ts.Level : 0); } }

        public int LineCount
        {
            get {  return (Handler.Statistics is TetrisStatistics ts) ? ts.LineCount : 0; }
        }

        public TetrisField(NominoTheme theme, IGameCustomizationHandler Handler, int pRowCount = DEFAULT_ROWCOUNT,int pColCount = DEFAULT_COLCOUNT,int pHiddenRowCount=2,int pHiddenRowCountBottom = 0)
        {
            _Theme = theme;
            _Handler = Handler;
            _VisibleRows = pRowCount - pHiddenRowCountBottom - pHiddenRowCount;
            this.RowCount = pRowCount;
            this.ColCount = pColCount;
            HIDDENROWS_BOTTOM = pHiddenRowCountBottom;
            HIDDENROWS_TOP = pHiddenRowCount;

            Reset();
        }
        public void Reset()
        {
            FieldContents = new NominoBlock[RowCount][];
            for (int row = 0; row < RowCount; row++)
            {
                FieldContents[row] = new NominoBlock[ColCount];
            }
            ActiveBlockGroups.Clear();

        }
        public void SetStandardHotLines()
        {
            HotLines.Clear();
            HotLines.Add(17, new HotLine(Color.Goldenrod, 1.25, 17));
            HotLines.Add(12, new HotLine(Color.Yellow, 1.5, 12));
            HotLines.Add(8, new HotLine(Color.Orange, 2, 8));
            HotLines.Add(5, new HotLine(Color.Red, 3, 5));
            HotLines.Add(3, new HotLine(Color.Purple, 5, 3));
            HotLines.Add(2, new HotLine(Color.Blue, 10, 2));
        }
        private bool _HasChanged = false;

        public bool HasChanged
        {
            get { return _HasChanged; }
            set {
                if(value==false)
                {
                    ;
                }

                _HasChanged = value; }
        }

        public void AnimateFrame()
        {
            lock (this)
            {
                for (int drawRow = HIDDENROWS_TOP; drawRow < RowCount; drawRow++)
                {
                    var currRow = FieldContents[drawRow];
                    //for each Tetris Row...
                    for (int drawCol = 0; drawCol < ColCount; drawCol++)
                    {
                        var TetBlock = currRow[drawCol];
                        if (TetBlock != null)
                        {
                            if (TetBlock.IsAnimated)
                            {
                                TetBlock.AnimateFrame();
                                HasChanged = true;
                            }
                        }
                    }
                }
            }
            foreach (var bg in from abg in ActiveBlockGroups orderby abg.Max((i)=>i.Y) descending select abg)
            {
                foreach (var iterateblock in bg)
                {
                    if (iterateblock.Block != null && iterateblock.Block.IsAnimated)
                    {
                        iterateblock.Block.AnimateFrame();
                    }
                }
            }
        }

        DateTime LastSetGroup = DateTime.MinValue;

        public IList<Point> SetGroupToField(params Nomino[] groups)
        {
            List<Point> Result = new List<Point>();
            lock (this)
            {
                lock (ActiveBlockGroups)
                {
                    LastSetGroup = DateTime.Now;

                    foreach (var bg in groups)
                    {
                        if (bg.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip)) continue;
                        Debug.Print("Setting Nomino to Field:" + bg.ToString());
                        foreach (var groupblock in bg)
                        {
                            int RowPos = groupblock.Y + bg.Y;
                            int ColPos = groupblock.X + bg.X;
                            //Investigate: is this resetting the Rotation?
                            //is it losing it's owner or something? Dr.Mario theme gets confused when setting a block to the field.
                            try
                            {
                                if (FieldContents[RowPos][ColPos] == null)
                                {
                                    FieldContents[RowPos][ColPos] = groupblock.Block;
                                    Result.Add(new Point(ColPos, RowPos));
                                }
                                else
                                {


                                }
                            }
                            catch (IndexOutOfRangeException e)
                            {
                            }
                        }
                    
                    if (ActiveBlockGroups.Contains(bg)) RemoveBlockGroup(bg);
                    }

                    BlockGroupSet?.Invoke(this, new BlockGroupSetEventArgs(groups));
                    HasChanged = true;
                }
            }
            return Result;
        }
       
        public CanFitResults CanFit(Nomino bg, int X, int Y,bool SkipActiveChecks,Nomino[] AdditionalIgnores = null)
        {
            Dictionary<Point, Nomino> GroupData = new Dictionary<Point, Nomino>();
            HashSet<Point> ActiveBlocks = new HashSet<Point>();
            Point Diff = new Point(X - bg.X, Y - bg.Y);
            //this routine handles other Block Groups as well, allowing multiple to exist at once in the play field, and be moved.
            //eg you cannot rotate or move an Active Group such that it will interfere with another active Group.
            //One consideration here is that the order of the groups will matter, in the sense that if one is processed to move first then it can be blocked even if it would be able to move
            //after a later group moves, which may create as few unusual side-effects.
            //let's start off by creating a HashSet of Point structs listing the field positions of other active groups than the specified Nomino.
            foreach (var active in ActiveBlockGroups)
            {
                if (active.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip)) continue;
                if (active != bg && (AdditionalIgnores==null|| !AdditionalIgnores.Contains(active)))
                {
                    
                    //can we alter this to also CanFit the other nomino in some way?
                  //if this one can move the difference provided, then we will skip it. The assumption being that if it can move in that direction, then it will move in that direction and won't actually be blocking us.

                    //buggy: this needs to be altered, we shouldn't be canfit-ing every single other Active Block Group. We should be only doing so if we decide that it would otherwise block is.

                    /*if (CanFit(active, Diff.X, Diff.Y, false, 
                        AdditionalIgnores==null?new[] { active }:AdditionalIgnores.Concat(new[] { active }).ToArray()).Result==CanFitResultConstants.CanFit) continue;*/
                    foreach (var check in active)
                    {
                        
                        var pcheck = new Point(active.X + check.X, active.Y + check.Y);
                        if(!ActiveBlocks.Contains(pcheck)) ActiveBlocks.Add(pcheck);
                        if(!GroupData.ContainsKey(pcheck)) GroupData.Add(pcheck, active);

                    }
                }
            }
            IList<NominoElement> Contacts = new List<NominoElement>();
            bool result = true;
            bool ActiveTouched = false;
            foreach (var checkblock in bg)
            {
                int CheckRow = Y +  checkblock.Y;
                int CheckCol = X +  checkblock.X;
                if (CheckRow < 0 || CheckCol < 0)
                {
                    result = false;
                    Contacts.Add(checkblock);
                }
                else if (CheckRow >= RowCount || CheckCol >= ColCount)
                {
                    result = false;
                    Contacts.Add(checkblock);
                }
                else
                {
                    var grabpos = FieldContents[CheckRow][CheckCol];
                    bool touchesactive = false;
                    if (!SkipActiveChecks && ActiveBlocks.Contains(new Point(CheckCol, CheckRow)))
                    {
                        if (CheckRow == RowCount-1)
                        {
                            ;
                        }
                        var grabNomino = GroupData[new Point(CheckCol, CheckRow)];
                        if (CanFit(grabNomino,grabNomino.X + Diff.X,grabNomino.Y + Diff.Y,false,AdditionalIgnores).Result==CanFitResultConstants.CanFit) continue;
                        //note: here we can CanFit to see if that other ActiveBlock can move in the direction we want to check.
                        touchesactive = true;
                    }
                    if(touchesactive)
                    {
                        ActiveTouched |= touchesactive;
                    }
                    if (grabpos != null || touchesactive)
                    {
                        result = false;
                        Contacts.Add(checkblock);
                    }
                }
            }
            if (ActiveTouched)
            {
                return new CanFitResults(CanFitResultConstants.CantFit_Active);
            }
            else if (result)
            {
                return new CanFitResults(CanFitResults.CanFitResultConstants.CanFit);
            }
            else
            {
                return new CanFitResults(CanFitResultConstants.CantFit_Field);
            }
            
        }

        public bool CanRotate(Nomino bg, bool ccw)
        {
            
            Nomino duped = new Nomino(bg);
            duped.Rotate(ccw);
            duped.Clamp(RowCount, ColCount);
            //we need to pass in bg for the additional argument this time, since we duplicated to a new nomino it will incorrectly get blocked by the original by CanFit otherwise.
            var fitresult = CanFit(duped, bg.X, bg.Y, false, new Nomino[] { bg });
            if (fitresult.Result == CanFitResultConstants.CantFit_Active) return bg.Flags.HasFlag(Nomino.NominoControlFlags.ControlFlags_NoClip);
            return fitresult.Result == CanFitResultConstants.CanFit;
        }

        public float GetBlockWidth(RectangleF ForBounds)
        {
            return ForBounds.Width / ColCount;
        }

        public float GetBlockHeight(RectangleF ForBounds)
        {
            return ForBounds.Height / (VisibleRows);
        }
        public float GetBlockWidth(SKRect ForBounds)
        {
            return ForBounds.Width / ColCount;
        }
        public float GetBlockHeight(SKRect ForBounds)
        {
            return ForBounds.Height / (VisibleRows);
        }
        public BCRect GetBlockBounds(IStateOwner pOwner,SKRect Bounds,BCPointI TopLeft,BCPointI BottomRight)
        {
            float BlockWidth = (float)(GetBlockWidth(Bounds));
            float BlockHeight = (float)(GetBlockHeight(Bounds));


            BCPoint TopLeftF = new BCPoint((float)TopLeft.X * BlockWidth, (float)TopLeft.Y * BlockHeight);
            BCPoint BottomRightF = new BCPoint((float)BottomRight.X + BlockWidth + BlockWidth, (float)TopLeft.Y * BlockHeight + BlockHeight);
            return new BCRect(TopLeftF.X, TopLeftF.Y, BottomRightF.X, BottomRightF.Y);


        }
        Pen LinePen = new Pen(Color.Black, 1) {DashPattern = new float[] {4, 1, 3, 1, 2, 1, 3, 1}};
        public RectangleF LastFieldSave = RectangleF.Empty;
        Image FieldBitmap = null;

        public void DrawFieldContents(IStateOwner pState,Graphics g, RectangleF Bounds)
        {
            float BlockWidth = Bounds.Width / ColCount;
            float BlockHeight = Bounds.Height / (VisibleRows); //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
#if false
            for (int drawCol = 0; drawCol < COLCOUNT; drawCol++)
            {
                float XPos = drawCol * BlockWidth;
                g.DrawLine(LinePen, XPos, 0, XPos, Bounds.Height);
            }
            for (int drawRow = HIDDENROWS; drawRow < ROWCOUNT; drawRow++)
            {
                float YPos = (drawRow - HIDDENROWS) * BlockHeight;
                g.DrawLine(LinePen, 0, YPos, Bounds.Width, YPos);
            }
#endif
            for (int drawRow = HIDDENROWS_TOP; drawRow < RowCount; drawRow++)
            {
                float YPos = (drawRow - HIDDENROWS_TOP) * BlockHeight;
                var currRow = FieldContents[drawRow];


                //also, is there a hotline here?
                if (Flags.HasFlag(TetrisField.GameFlags.Flags_Hotline) && HotLines.ContainsKey(drawRow))
                {
                    RectangleF RowBounds = new RectangleF(0, YPos, BlockWidth * ColCount, BlockHeight);
                    Brush useFillBrush = null;
                    var HotLine = HotLines[drawRow];
                    if (HotLine.LineBrush != null) useFillBrush = HotLine.LineBrush;
                    else
                    {
                        useFillBrush = GetHotLineTexture((int)RowBounds.Height + 1, HotLines[drawRow].Color);
                    }
                    if(useFillBrush is TextureBrush tb1)
                    {
                        tb1.TranslateTransform(0,YPos);
                    }
                    g.FillRectangle(useFillBrush, RowBounds);
                    if (useFillBrush is TextureBrush tb2)
                    {
                        tb2.ResetTransform();
                    }
                }
                //for each Tetris Row...
                for (int drawCol = 0; drawCol < ColCount; drawCol++)
                {
                    float XPos = drawCol * BlockWidth;
                    var TetBlock = currRow[drawCol];
                    if (TetBlock != null)
                    {
                        RectangleF BlockBounds = new RectangleF(XPos, YPos, BlockWidth, BlockHeight);
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, BlockBounds, null,pState.Settings);
                        RenderingProvider.Static.DrawElement(pState,tbd.g,TetBlock,tbd);
                    }
                }
               
            }
        }
        double HighestHeightValue = 0;
        public void Draw(IStateOwner pState,Graphics g, RectangleF Bounds)
        {
            //first how big is each block?
            float BlockWidth = Bounds.Width / ColCount;
            float BlockHeight = Bounds.Height / (VisibleRows); //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
            lock (this)
            {
                if (FieldBitmap == null || !LastFieldSave.Equals(Bounds) || HasChanged)
                {
                    Bitmap BuildField = new Bitmap((int)Bounds.Width, (int)Bounds.Height, PixelFormat.Format32bppPArgb);
                    using (Graphics gfield = Graphics.FromImage(BuildField))
                    {
                        gfield.CompositingQuality = CompositingQuality.HighSpeed;
                        gfield.SmoothingMode = SmoothingMode.HighSpeed;
                        gfield.InterpolationMode = InterpolationMode.NearestNeighbor;
                        gfield.Clear(Color.Transparent);
                        DrawFieldContents(pState, gfield, Bounds);
                        if (FieldBitmap != null) FieldBitmap.Dispose();
                        FieldBitmap = BuildField;
                    }

                    HasChanged = false;
                }
            }
            g.DrawImageUnscaled(FieldBitmap, 0, 0);


            lock (ActiveBlockGroups)
            {
                foreach (Nomino bg in ActiveBlockGroups)
                {
                    int BaseXPos = bg.X;
                    int BaseYPos = bg.Y;
                    const float RotationTime = 100;
                    double useAngle = 0;
                    TimeSpan tsRotate = DateTime.Now - bg.GetLastRotation();
                    if (tsRotate.TotalMilliseconds > 0 && tsRotate.TotalMilliseconds < RotationTime)
                    {
                        if (!bg.LastRotateCCW)
                            useAngle = -90 + ((tsRotate.TotalMilliseconds / RotationTime) * 90);
                        else
                        {
                            useAngle = 90 - ((tsRotate.TotalMilliseconds / RotationTime) * 90);
                        }
                    }

                    var translation = bg.GetHeightTranslation(pState, BlockHeight);
                   
                   
                        float BlockPercent = translation / BlockHeight;
                        float CalcValue = BlockPercent + (float)bg.Y;

                        if (CalcValue > bg.HighestHeightValue)
                        {
                            bg.HighestHeightValue = CalcValue;
                        }
                        else
                        {
                            translation = (bg.HighestHeightValue - (float)bg.Y) * BlockHeight;
                        }
                   
                    PointF doTranslate = new PointF(0,translation);
                    if(!pState.Settings.std.SmoothFall) doTranslate = new PointF(0,0);
                    //if (Settings.SmoothFall) g.TranslateTransform(doTranslate.X, -BlockHeight + doTranslate.Y);
                    if(pState.Settings.std.SmoothFall) g.TranslateTransform(doTranslate.X,-BlockHeight + doTranslate.Y);
                    if (useAngle != 0 && pState.Settings.std.SmoothRotate)
                    {
                        int MaxXBlock = (from p in bg select p.X).Max();
                        int MaxYBlock = (from p in bg select p.Y).Max();
                        int MinXBlock = (from p in bg select p.X).Min();
                        int MinYBlock = (from p in bg select p.Y).Min();
                        int BlocksWidth = MaxXBlock - MinXBlock + 1;
                        int BlocksHeight = MaxYBlock - MinYBlock + 1;

                        PointF UsePosition = new PointF((bg.X + MinXBlock) * BlockWidth, (bg.Y - HIDDENROWS_TOP + MinYBlock) * BlockHeight);


                        SizeF tetronimosize = new Size((int) BlockWidth * (BlocksWidth), (int) BlockHeight * (BlocksHeight));

                        PointF useCenter = new PointF(UsePosition.X + tetronimosize.Width / 2, UsePosition.Y + tetronimosize.Height / 2);

                        g.TranslateTransform(useCenter.X, useCenter.Y);
                        g.RotateTransform((float) useAngle);
                        g.TranslateTransform(-useCenter.X, -useCenter.Y);
                    }

                    


                    foreach (NominoElement bge in bg)
                    {
                        int DrawX = BaseXPos + bge.X;
                        int DrawY = BaseYPos + bge.Y - HIDDENROWS_TOP;
                        if (DrawX >= 0 && DrawY >= 0 && DrawX < ColCount && DrawY < RowCount)
                        {
                            float DrawXPx = DrawX * BlockWidth;
                            float DrawYPx = DrawY * BlockHeight;


                            RectangleF BlockBounds = new RectangleF(DrawXPx, DrawYPx, BlockWidth, BlockHeight);
                            TetrisBlockDrawParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, BlockBounds, bg, pState.Settings);
                            RenderingProvider.Static.DrawElement(pState,g,bge.Block,tbd);
                        }
                    }

                    g.ResetTransform();
                }
            }
        }

        public void SetFieldColors(IGameCustomizationHandler handler)
        {
            lock (this)
            { 
                foreach (var iteraterow in FieldContents)
                {
                    foreach (var iteratecell in iteraterow)
                    {
                        if (iteratecell != null && iteratecell.Owner != null)
                            Theme.ApplyTheme(iteratecell.Owner, handler,this, NominoTheme.ThemeApplicationReason.Normal);
                    }
                }

                HasChanged = true;
            }
        }


        public class LevelChangeEventArgs : EventArgs
        {
            private int LevelNumber = 0;

            public LevelChangeEventArgs(int newLevel = 0)
            {
            }
        }

        public class BlockGroupSetEventArgs : EventArgs
        {
            public List<Nomino> _groups = null;

            public BlockGroupSetEventArgs(params Nomino[] bg)
            {
                _groups = bg.ToList();
            }
        }
    }
    public class OnThemeChangeEventArgs :EventArgs
    {

    }
    //event fired when a "new" Row scrolls into view.
    public class OnNewLineRowScroll : EventArgs
    {
        public int RowIndex { get; set; }
        public OnNewLineRowScroll(int pRowIndex)
        {
            RowIndex = pRowIndex;
        }
    }
    public class OnRemoveActiveBlockGroupEventArgs : EventArgs
    {
        public Nomino BlockGroupRemove;
        public OnRemoveActiveBlockGroupEventArgs(Nomino RemovingGroup)
        {
            BlockGroupRemove = RemovingGroup;
        }
    }
    public class CanFitResults
    {
        public enum CanFitResultConstants
        {
            /// <summary>
            /// The nomino Can fit
            /// </summary>
            CanFit,
            /// <summary>
            /// The nomino cannot fit, because it is blocked by a fixed block on the field.
            /// </summary>
            CantFit_Field,
            /// <summary>
            /// The Nomino can't fit because it is blocked by an active nomino.
            /// </summary>
            CantFit_Active
        }
        public bool CanFit
        {
            get
            {
                return Result == CanFitResultConstants.CanFit;
            }
        }
        public bool CantFit_Field
        {
            get
            {
                return Result == CanFitResultConstants.CantFit_Field;
            }
        }
        public bool CantFit_Active
        {
            get
            {
                return Result == CanFitResultConstants.CantFit_Active;
            }
        }
        public CanFitResultConstants Result { get; set; }
        public CanFitResults(CanFitResultConstants pResult)
        {
            Result = pResult;
        }
    }
}