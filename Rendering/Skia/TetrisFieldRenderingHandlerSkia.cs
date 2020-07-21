using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.Rendering.RenderElements;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    public class TetrisFieldDrawSkiaParameters
    {
        public int COLCOUNT;
        public int ROWCOUNT;
        public int VISIBLEROWS;
        public SKRect Bounds;
        public SKRect LastFieldSave;
        public SKBitmap FieldBitmap;
        public int HIDDENROWS
        {
            get { return ROWCOUNT - VISIBLEROWS; }
        }
    }
    [RenderingHandler(typeof(TetrisField), typeof(SKCanvas),typeof(TetrisFieldDrawSkiaParameters))]
    public class TetrisFieldRenderingHandlerSkia : StandardRenderingHandler<SkiaSharp.SKCanvas,TetrisField,TetrisFieldDrawSkiaParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TetrisField Source, TetrisFieldDrawSkiaParameters Element)
        {
            Draw(Source,Element,pOwner,pRenderTarget,Element.Bounds);
        }
        public bool DrawFieldContents(IStateOwner pState, TetrisField Source, TetrisFieldDrawSkiaParameters Element, SKCanvas g, SKRect Bounds,bool animated)
        {
            float BlockWidth = Bounds.Width / Element.COLCOUNT;
            float BlockHeight = Bounds.Height / (Element.VISIBLEROWS); //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
            bool FoundAnimated = false;
            for (int drawRow = Element.HIDDENROWS; drawRow < Element.ROWCOUNT; drawRow++)
            {
                float YPos = (drawRow - Element.HIDDENROWS) * BlockHeight;
                var currRow = Source.Contents[drawRow];


                //also, is there a hotline here?
                /*
                if (Source.Flags.HasFlag(TetrisField.GameFlags.Flags_Hotline) && Source.HotLines.ContainsKey(drawRow))
                {
                    RectangleF RowBounds = new RectangleF(0, YPos, BlockWidth * Element.COLCOUNT, BlockHeight);
                    Brush useFillBrush = null;
                    var HotLine = Source.HotLines[drawRow];
                    if (HotLine.LineBrush != null) useFillBrush = HotLine.LineBrush;
                    else
                    {
                        useFillBrush = GetHotLineTexture((int)RowBounds.Height + 1, HotLines[drawRow].Color);
                    }
                    if (useFillBrush is TextureBrush tb1)
                    {
                        tb1.TranslateTransform(0, YPos);
                    }
                    g.FillRectangle(useFillBrush, RowBounds);
                    if (useFillBrush is TextureBrush tb2)
                    {
                        tb2.ResetTransform();
                    }
                }*/ //hotline drawing needs to be reworked for the rendering providers...
                //for each Tetris Row...
                for (int drawCol = 0; drawCol < Element.COLCOUNT; drawCol++)
                {
                    float XPos = drawCol * BlockWidth;
                    var TetBlock = currRow[drawCol];
                    bool isAnim = false;
                    if (TetBlock != null)
                    {
                        isAnim = Source.Theme.IsAnimated(TetBlock);
                        if (isAnim == animated)
                        {

                            SKRect BlockBounds = new SKRect(XPos, YPos, XPos + BlockWidth, YPos + BlockHeight);
                            TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, BlockBounds, null, pState.Settings);
                            RenderingProvider.Static.DrawElement(pState, tbd.g, TetBlock, tbd);
                        }
                    }
                    FoundAnimated |= isAnim;
                }

            }
            if (FoundAnimated) {; }
            return FoundAnimated;
        }
        bool hadAnimated = false;
        public void Draw(TetrisField Source,TetrisFieldDrawSkiaParameters parms,IStateOwner pState, SKCanvas g, SKRect Bounds)
        {
            //first how big is each block?
            float BlockWidth = Bounds.Width / parms.COLCOUNT;
            float BlockHeight = Bounds.Height / (parms.VISIBLEROWS); //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
            lock (Source)
            {
                if (parms.FieldBitmap == null || !parms.LastFieldSave.Equals(Bounds) || Source.HasChanged)
                {
                    SKImageInfo info = new SKImageInfo((int)Bounds.Width, (int)Bounds.Height);
                    SKBitmap BuildField = new SKBitmap(info, SKBitmapAllocFlags.ZeroPixels);
                    using (SKCanvas gfield = new SKCanvas(BuildField))
                    {
                        gfield.Clear(SKColors.Transparent);
                        hadAnimated = DrawFieldContents(pState, Source, parms, gfield, Bounds,false);
                        if (parms.FieldBitmap != null) parms.FieldBitmap.Dispose();
                        parms.FieldBitmap = BuildField;
                    }
                    parms.LastFieldSave = Bounds;
                    Source.HasChanged = false;
                }
            }
            g.DrawBitmap(parms.FieldBitmap,new SKPoint(0,0));

            if(hadAnimated)
            {
                DrawFieldContents(pState, Source, parms, g, Bounds, true);
            }


            var activegroups = Source.GetActiveBlockGroups();

            lock (activegroups)
            {
                foreach (Nomino bg in activegroups)
                {
                    int BaseXPos = bg.X;
                    int BaseYPos = bg.Y;
                    const float RotationTime = 150;
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

                    PointF doTranslate = new PointF(0, translation);
                    if (!pState.Settings.SmoothFall) doTranslate = new PointF(0, 0);
                    //if (Settings.SmoothFall) g.TranslateTransform(doTranslate.X, -BlockHeight + doTranslate.Y);
                    if (pState.Settings.SmoothFall)
                    {
                        g.Translate(doTranslate.X, -BlockHeight + doTranslate.Y);
                    }
                    if (useAngle != 0 && pState.Settings.SmoothRotate)
                    {
                        int MaxXBlock = (from p in bg select p.X).Max();
                        int MaxYBlock = (from p in bg select p.Y).Max();
                        int MinXBlock = (from p in bg select p.X).Min();
                        int MinYBlock = (from p in bg select p.Y).Min();
                        int BlocksWidth = MaxXBlock - MinXBlock + 1;
                        int BlocksHeight = MaxYBlock - MinYBlock + 1;

                        PointF UsePosition = new PointF((bg.X + MinXBlock) * BlockWidth, (bg.Y - parms.HIDDENROWS + MinYBlock) * BlockHeight);


                        SizeF tetronimosize = new Size((int)BlockWidth * (BlocksWidth), (int)BlockHeight * (BlocksHeight));

                        PointF useCenter = new PointF(UsePosition.X + tetronimosize.Width / 2, UsePosition.Y + tetronimosize.Height / 2);

                        g.RotateDegrees((float)useAngle,useCenter.X,useCenter.Y);

                        //g.TranslateTransform(useCenter.X, useCenter.Y);
                        //g.RotateTransform((float)useAngle);
                        //g.TranslateTransform(-useCenter.X, -useCenter.Y);
                    }




                    foreach (NominoElement bge in bg)
                    {
                        int DrawX = BaseXPos + bge.X;
                        int DrawY = BaseYPos + bge.Y - parms.HIDDENROWS;
                        if (DrawX >= 0 && DrawY >= 0 && DrawX < parms.COLCOUNT && DrawY < parms.ROWCOUNT)
                        {
                            float DrawXPx = DrawX * BlockWidth;
                            float DrawYPx = DrawY * BlockHeight;


                            SKRect BlockBounds = new SKRect(DrawXPx, DrawYPx, DrawXPx+ BlockWidth, DrawYPx + BlockHeight);
                            TetrisBlockDrawParameters tbd = new TetrisBlockDrawSkiaParameters(g, BlockBounds, bg, pState.Settings);
                            RenderingProvider.Static.DrawElement(pState, g, bge.Block, tbd);
                        }
                    }
                    g.ResetMatrix();

                    if (!bg.NoGhost)
                    {
                        var GrabGhost = Source.GetGhostDrop(pState, bg, out int dl);
                        if (GrabGhost != null)
                        {
                            foreach (var iterateblock in bg)
                            {

                                float drawGhostX = BlockWidth * (GrabGhost.X + iterateblock.X);
                                float drawGhostY = BlockHeight * (GrabGhost.Y + iterateblock.Y - 2);

                                SKRect BlockBounds = new SKRect(drawGhostX, drawGhostY, drawGhostX + BlockWidth, drawGhostY + BlockHeight);

                                TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, BlockBounds, GrabGhost, pState.Settings);
                                //ImageAttributes Shade = new ImageAttributes();
                                //SKColorMatrices.GetFader
                                //Shade.SetColorMatrix(ColorMatrices.GetFader(0.5f));
                                //tbd.ApplyAttributes = Shade;
                                //tbd.OverrideBrush = GhostBrush;
                                tbd.ColorFilter = SKColorMatrices.GetFader(0.5f);
                                var GetHandler = RenderingProvider.Static.GetHandler(typeof(SKCanvas), iterateblock.Block.GetType(), typeof(TetrisBlockDrawSkiaParameters));
                                GetHandler.Render(pState, tbd.g, iterateblock.Block, tbd);
                                //iterateblock.Block.DrawBlock(tbd);
                            }


                        }
                    }

                }
            }
        }
    }
}
