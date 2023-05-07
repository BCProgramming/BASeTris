using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Settings;
using BASeTris.Particles;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(GameplayGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class StandardTetrisGameStateSkiaRenderingHandler :   StandardStateRenderingHandler<SKCanvas,GameplayGameState,GameStateSkiaDrawParameters>
    {
        private StandardImageBackgroundSkia _Background = null;
        private NominoTheme CurrentTheme = null;
        public SKRect LastDrawStat = SKRect.Empty;
        
        private void BuildBackground(GameplayGameState Self,SKRect Size)
        {
            var bgInfo = Self.PlayField.Theme.GetThemePlayFieldBackground(Self.PlayField,Self.GameHandler);
            
            _Background = new StandardImageBackgroundSkia();
            Bitmap bmp = new Bitmap(ImageManager.ReduceImage(bgInfo.BackgroundImage,
                new Size((int)(Size.Width + 0.5f), (int)(Size.Height + 0.5f))));
            
            SKImage usebg = SkiaSharp.Views.Desktop.Extensions.ToSKImage(bmp);
            _Background.Data = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = usebg, Movement = new SKPoint(0, 0) };
            if(bgInfo.TintColor!=Color.Transparent)
            {
                _Background.Data.theFilter = SKColorMatrices.GetColourizer(bgInfo.TintColor.R, bgInfo.TintColor.G, bgInfo.TintColor.B, bgInfo.TintColor.A);
            }
            Self.DoRefreshBackground = false;
        }
        SKImage StatisticsBackground = null;
        //redraws the StatisticsBackground SKImage.
        public void GenerateStatisticsBackground(GameplayGameState Self,IStateOwner pOwner)
        {
            using (SKBitmap sourcebit = new SKBitmap(new SKImageInfo(1120, 2576, SKColorType.Rgba8888)))
            {
                Size BlockSize = new Size(128, 128);
                int ColumnCount = (sourcebit.Width / BlockSize.Width) + 1;
                int RowCount = (sourcebit.Height / BlockSize.Height) + 1;

                using (SKCanvas g = new SKCanvas(sourcebit))
                {
                    g.Clear(Color.Black.ToSKColor());
                    for (int col = 0; col < ColumnCount; col++)
                    {
                        for (int row = 0; row < RowCount; row++)
                        {
                            int DrawBlockX = col * BlockSize.Width;
                            int DrawBlockY = row * BlockSize.Height;
                            StandardColouredBlock GenerateColorBlock = new StandardColouredBlock();
                            Nomino ArbitraryGroup = new Nomino();
                            ArbitraryGroup.AddBlock(new Point[] { Point.Empty }, GenerateColorBlock);
                            Self.PlayField.Theme.ApplyRandom(ArbitraryGroup,Self.GameHandler, Self.PlayField);
                            //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                            TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, new SKRect(DrawBlockX, DrawBlockY, DrawBlockX + BlockSize.Width, DrawBlockY + BlockSize.Height), null,   pOwner.Settings);
                            RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);

                        }
                    }
                }

                StatisticsBackground = SKImage.FromBitmap(sourcebit);
                LastStatisticsTheme = Self.PlayField.Theme;
                Self.f_RedrawStatusBitmap = false;
            }
        }


        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, GameplayGameState Source, GameStateSkiaDrawParameters Element)
        {
            GamePlayGameStateDataTagInfo tagData = Element.TagData as GamePlayGameStateDataTagInfo;

            bool SkipParticles = tagData != null && tagData.SkipParticlePaint;

            //testing code for background.
            if (Source.DoRefreshBackground ||  _Background==null || CurrentTheme!=Source.PlayField.Theme)
            {
                BuildBackground(Source,Element.Bounds);
                CurrentTheme = Source.PlayField.Theme;
            }
            _Background.FrameProc(pOwner);
            
            RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,_Background, new SkiaBackgroundDrawData(Element.Bounds));
            //draw particles.
            
            if(!SkipParticles) RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.Particles, Element);
            var PlayField = Source.PlayField;
            if (PlayField != null)
            {
                Object grabdata = RenderingProvider.Static.GetExtendedData(PlayField.GetType(), PlayField,
                    (o) =>
                        new TetrisFieldDrawSkiaParameters()
                        {
                            Bounds = Element.Bounds,
                            COLCOUNT = PlayField.ColCount,
                            ROWCOUNT = PlayField.RowCount,
                            HIDDENBOTTOMROWS = PlayField.HIDDENROWS_BOTTOM,
                            HIDDENTOPROWS = PlayField.HIDDENROWS_TOP,
                            FieldBitmap = null,
                            LastFieldSave = SKRect.Empty,
                            VISIBLEROWS = PlayField.VisibleRows
                        });
                
                TetrisFieldDrawSkiaParameters parameters = (TetrisFieldDrawSkiaParameters)grabdata;
                parameters.LastFieldSave = Element.Bounds;
                //if (!Source.GameHandler.AllowFieldImageCache) parameters.FieldBitmap = null;
                RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,PlayField,parameters);
            }
            var particledrawer = RenderingProvider.Static.GetHandler(typeof(List<BaseParticle>), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters));
            Element.Offset = new Adapters.BCPoint(0, PlayField.OffsetPaint);
            if(Source.TopParticles!=null && Source.TopParticles.Count > 0)
            {
                if (!SkipParticles) RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.TopParticles, Element);
            }
            

        }
        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss");
        }
        public object LockTetImageRedraw = new Object();
        bool CalledRedraw = false;
        public void RedrawStatusbarTetrominoBitmaps(IStateOwner Owner, GameplayGameState State, SKRect Bounds)
        {
            if(CalledRedraw)
            {
                ;
            }
            CalledRedraw = true;
            lock (LockTetImageRedraw)
            {

                State.SetTetrominoSKBitmaps(TetrisGame.GetTetrominoBitmapsSK(Bounds, State.PlayField.Theme,State.GameHandler, State.PlayField, (float)Owner.ScaleFactor));
            }
        }
        SKPaint BlackBrush = new SKPaint() { Color = SKColors.Black, Style = SKPaintStyle.StrokeAndFill };
        SKPaint WhiteBrush = new SKPaint() { Color = SKColors.White, Style = SKPaintStyle.StrokeAndFill };
        SKPaint LightenBrush = new SKPaint() { Color = Color.FromArgb(128, Color.MintCream).ToSKColor(),Style=SKPaintStyle.Fill };
        SKPaint StandardText = null;

        Graphics gscale = Graphics.FromImage(new Bitmap(1, 1, PixelFormat.Format32bppArgb));
        private double PixelsToPoints(double pPixels)
        {
            return pPixels * 72 / gscale.DpiX;
        }
        NominoTheme LastStatisticsTheme = null;
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, GameplayGameState Source, GameStateSkiaDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            
            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            try
            {
                if (StatisticsBackground == null || RedrawsNeeded || Source.f_RedrawStatusBitmap ||  LastStatisticsTheme!=Source.PlayField.Theme)
                {
                    GenerateStatisticsBackground(Source,pOwner);
                }

                g.DrawImage(StatisticsBackground, Bounds);
                //g.Clear(Color.Black);
                if (!Source.HasTetrominoSKBitmaps() || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(pOwner, Source, Bounds);

                lock (Source.LockTetImageRedraw)
                {
                    var useStats = Source.GameStats;
                    double Factor = Bounds.Height / 644d;
                    var DesiredFontPixelHeight = 22d; //  PixelsToPoints((int)(Bounds.Height * (30d / 644d)));
                    float DesiredFontSize = (float)(DesiredFontPixelHeight * pOwner.ScaleFactor);
                    SKTypeface standardFont = TetrisGame.RetroFontSK;
                    //Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);

                    var TopScore = Source.GetLocalScores()==null?0:Source.GetLocalScores().GetScores().First().Score;
                    int MaxScoreLength = Math.Max(TopScore.ToString().Length, useStats.Score.ToString().Length);

                    String CurrentScoreStr = useStats.Score.ToString().PadLeft(MaxScoreLength + 2);
                    String TopScoreStr = TopScore.ToString().PadLeft(MaxScoreLength + 2);
                    //TODO: redo this segment separately, so we can have the labels left-aligned and the values right-aligned.
                    // String BuildStatString = "Time:  " + FormatGameTime(pOwner).ToString().PadLeft(MaxScoreLength + 2) + "\n" +
                    //                          "Score: " + CurrentScoreStr + "\n" +
                    //                          "Top:   " + TopScoreStr + " \n" +
                    //                          "Lines: " + GameStats.LineCount.ToString().PadLeft(MaxScoreLength+2);
                    //SKPaint skp = new SKPaint(){Style = SKPaintStyle.Fill,Color=;
                    //g.DrawRect();
                    g.DrawRect(Bounds.Left, Bounds.Top + 5, Bounds.Width-10, (int)(450 * Factor), LightenBrush);
                    //String[] StatLabels = new string[] { "Time:", "Score:", "Top:", "Lines:" };
                    //int LineCount = Source.GameStats is TetrisStatistics ? (Source.GameStats as TetrisStatistics).LineCount : 0;
                    //String[] StatValues = new string[] { FormatGameTime(pOwner), useStats.Score.ToString(), TopScore.ToString(), LineCount.ToString() };
                    
                    WhiteBrush.TextSize = BlackBrush.TextSize = DesiredFontSize;
                    WhiteBrush.Typeface = BlackBrush.Typeface = TetrisGame.RetroFontSK;
                    SKRect MeasureLabel = new SKRect(), MeasureValue = new SKRect();
                    BlackBrush.MeasureText("#", ref MeasureLabel);
                    BlackBrush.MeasureText("#", ref MeasureValue);
                    SKPoint StatPosition = new SKPoint(Bounds.Left + (int)(7 * Factor), Bounds.Top + MeasureLabel.Height + (int)(14 * Factor));
                    float CurrentYPosition = StatPosition.Y+MeasureLabel.Height;
                    var StatData = useStats.GetDisplayStatistics(pOwner, Source);

                    int MaxLabelLength = 0;
                    int MaxValueLength = 0;
                    foreach(var statkvp in StatData)
                    {
                        if(MaxLabelLength < statkvp.Key.Length)
                        {
                            MaxLabelLength = statkvp.Key.Length;
                        }
                        if(MaxValueLength < statkvp.Value.Length)
                        {
                            MaxValueLength = statkvp.Value.Length;
                        }
                    }
                    ///TODO: improve to allow any number of entries, as stuff like Pentris will add loads of entries here.
                    foreach(var statkvp in StatData)
                    //for (int statindex = 0; statindex < StatLabels.Length; statindex++)
                    {
                        String Label = statkvp.Key;
                        String Value = statkvp.Value.PadLeft(MaxValueLength,' ');
                       float LabelWidth = BlackBrush.MeasureText(Label.Replace(" ", "_") + "###" , ref MeasureLabel);
                       float ValueWidth = BlackBrush.MeasureText(Value.Replace(" ","_")+ "###" , ref MeasureValue);

                        //var MeasureLabel = g.MeasureString(StatLabels[statindex], standardFont);
                        //var MeasureValue = g.MeasureString(StatValues[statindex], standardFont);
                        float LargerHeight = Math.Max(MeasureLabel.Height, MeasureValue.Height);

                        //we want to draw the current stat label at position StatPosition.X,CurrentYPosition...

                        // TetrisGame.DrawTextSK(g, StatLabels[statindex], new SKPoint(StatPosition.X, CurrentYPosition),
                        //     TetrisGame.RetroFontSK, SKColors.Black, DesiredFontSize, (float)pOwner.ScaleFactor);
                        

                        //draw labels...
                        g.DrawText(Label, new SKPoint(StatPosition.X + 5, CurrentYPosition + 5), WhiteBrush);
                        g.DrawText(Label, new SKPoint(StatPosition.X, CurrentYPosition), BlackBrush);


                        SKPoint ValuePosition = new SKPoint((float)(Bounds.Right - ((MeasureValue.Width) + (7 * Factor))), CurrentYPosition);

                        g.DrawText(Value, new SKPoint(ValuePosition.X + 5, ValuePosition.Y + 5), WhiteBrush);
                        g.DrawText(Value, new SKPoint(ValuePosition.X , ValuePosition.Y ), BlackBrush);
                        //TetrisGame.DrawTextSK(g, PaintInfo);


                        //TetrisGame.DrawTextSK(g, PaintInfo);

                        //we want to draw the current stat value at Bounds.Width-ValueWidth.
                        //TetrisGame.DrawTextSK(g, StatValues[statindex], new SKPoint((float)(Bounds.Right - ((MeasureValue.Width) + (5 * Factor))), CurrentYPosition), TetrisGame.RetroFontSK, SKColors.Black, DesiredFontSize, (float)pOwner.ScaleFactor);

                        //add the larger of the two heights to the current Y Position.
                        CurrentYPosition += (int)LargerHeight;
                        CurrentYPosition += 10;

                    }


                    var findRenderer = Source.GameHandler.GetStatAreaRenderer<SKCanvas, GameStateSkiaDrawParameters>();
                    if (findRenderer != null)
                    {
                        //bounds of stat area.
                        var LeftBound = Bounds.Left + (int)(30 * Factor);
                        var TopBound = Bounds.Top + (int)(140 * Factor);
                        SKRect useBounds = new SKRect(LeftBound, TopBound, Bounds.Right, (float)(420 * Factor));
                        GameStateSkiaDrawParameters parameters = new GameStateSkiaDrawParameters(useBounds);
                        findRenderer.Render(pOwner, pRenderTarget, Source, parameters);

                    }
                    else
                    {
                    }
                    SKPoint NextDrawPosition = new SKPoint(Bounds.Left + (int)(40f * Factor), Bounds.Top + (int)(420 * Factor));
                    Size NextSize = new Size((int)(200f * Factor), (int)(200f * Factor));
                    SKPoint CenterPoint = new SKPoint(NextDrawPosition.X + NextSize.Width / 2, NextDrawPosition.Y + NextSize.Height / 2);
                    
                    if (Source.NextBlocks.Count > 0 && Source.DrawNextQueue)
                    {
                        var QueueList = Source.NextBlocks.ToArray();
                        SKBitmap[] NextTetrominoes = (from t in QueueList select Source.GetTetrominoSKBitmap(pOwner,t)).ToArray();
                        SKBitmap DisplayBox = TetrisGame.Imageman.GetSKBitmap("display_box");
                        //draw it at 40,420. (Scaled).
                        float ScaleDiff = 0;
                        iActiveSoundObject PlayingMusic;
                        if ((PlayingMusic = TetrisGame.Soundman.GetPlayingMusic_Active()) != null)
                            Source.StoredLevels.Enqueue(PlayingMusic.Level);

                        if (Source.StoredLevels.Count >= 4)
                        {
                            ScaleDiff = Math.Min(30, 10 * Source.StoredLevels.Dequeue());
                        }

                        if (!TetrisGame.DJMode)
                        {
                            ScaleDiff = 0;
                        }


                        SKRect useRectangle = new SKRect(NextDrawPosition.X - ScaleDiff, NextDrawPosition.Y - ScaleDiff,
                            NextDrawPosition.X - ScaleDiff + NextSize.Width + ScaleDiff * 2,
                            NextDrawPosition.Y - ScaleDiff + NextSize.Height + ScaleDiff * 2);





                        g.DrawBitmap(DisplayBox, useRectangle);

                        g.DrawCircle(CenterPoint.X - 5, CenterPoint.Y - 5, 5, BlackBrush);

                        //new "wiggling" mode. add Sin(timer*(5*index))*30 to the angle.

                        for (int i = NextTetrominoes.Length - 1; i > -1; i--)
                        {
                            var ThisNomino = QueueList[i];
                            var baseval = ((double)(DateTime.Now.Ticks + (250000 * i)) / 5000000);
                            //var AngleAdd = Math.Sin(baseval) * 10; //old approach.
                            var AngleAdd =(Math.Sin(baseval)*Math.Cos(baseval*2)) * 7; //new approach, tries to be a little bit different...

                            //var AngleAdd = Math.Sin(((double)DateTime.Now.Millisecond / 166)) * 15;


                            double StartAngle = Math.PI;
                            double AngleIncrementSize = (Math.PI * 1.8) / (double)NextTetrominoes.Length;
                            //we draw starting at StartAngle, in increments of AngleIncrementSize.
                            //i is the index- we want to increase the angle by that amount (well, obviously, I suppose...

                            double UseAngleCurrent = StartAngle + AngleIncrementSize * (float)i + Source.NextAngleOffset;

                            double UseXPosition = CenterPoint.X + ((float)((NextSize.Width) / 2.2) * Math.Cos(UseAngleCurrent));
                            double UseYPosition = CenterPoint.Y + ((float)((NextSize.Height) / 2.2) * Math.Sin(UseAngleCurrent));


                            var NextTetromino = NextTetrominoes[i];
                            if (NextTetromino == null) continue;
                            float Deviation = (i - NextTetrominoes.Length / 2);
                            Point Deviate = new Point((int)(Deviation * 20 * Factor), (int)(Deviation * 20 * Factor));

                            var AngleRotateLocation = DateTime.Now.Ticks / 5000000;
                            double AngleMovePercent = Source.NextAngleOffset / AngleIncrementSize;
                            Size DrawTetSize = new Size
                           (
                               (int)((float)(NextTetromino.Width * 1.5f) * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))),
                               (int)((float)(NextTetromino.Height * 1.5f) * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))));

                            var SizeVal = Math.Max(ThisNomino.GroupExtents.Width, ThisNomino.GroupExtents.Height);
                            if (SizeVal > 5)
                            {
                                float reduceFactor = 5 / (float)SizeVal;
                                DrawTetSize = new Size((int)((float)DrawTetSize.Width * reduceFactor), (int)((float)DrawTetSize.Height * reduceFactor));

                            }
                            Point DrawTetLocation = new Point((int)UseXPosition - (DrawTetSize.Width / 2), (int)UseYPosition - DrawTetSize.Height / 2);
                            //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                            //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                            
                            double NumAffect = Source.NextAngleOffset == 0 ? 0 : AngleIncrementSize / Source.NextAngleOffset;
                           
                            

                            //g.TranslateTransform(CenterPoint.X,CenterPoint.Y);
                            double DrawTetAngle = UseAngleCurrent;
                            DrawTetAngle += (Math.PI * AngleMovePercent);
                            float useDegrees = 180 + (float)(DrawTetAngle * (180 / Math.PI));
                            useDegrees = (float)(useDegrees + AngleAdd);
                            g.RotateDegrees(useDegrees, DrawTetLocation.X + DrawTetSize.Width / 2, DrawTetLocation.Y + DrawTetSize.Width / 2);

                            var DrawAdjustmentAngle = MathHelper.mod((int)(((double)DateTime.Now.Ticks / 400) * (5f*i)), 360);



                            if (DrawTetSize.Width > 0 && DrawTetSize.Height > 0)
                            {
                                //ImageAttributes Shade = GetShadowAttributes(1.0f - ((float)i * 0.3f));
                                //ImageAttributes Shade = new ImageAttributes();
                                //Shade.SetColorMatrix(ColorMatrices.GetFader(1.0f - ((float)i * 0.1f)));

                                SKRect DrawBound = new SKRect(DrawTetLocation.X, DrawTetLocation.Y, DrawTetLocation.X + DrawTetSize.Width, DrawTetLocation.Y + DrawTetSize.Height);

                                //for the shade we would need to deal with SKPaint, I feel. Want to get it working somewhat first though.
                                g.DrawBitmap(NextTetromino, DrawBound, null);

                            }

                            g.ResetMatrix();

                        }
                    }

                    if (Source.HoldBlocks.Any())
                    {
                        int holdoffsetcount = Source.HoldBlocks.Count()-1;
                        double XOffset = 0;
                        double YOffset = 0;
                        foreach (var HoldBlock in Source.HoldBlocks.Reverse())
                        {
                            SKBitmap HoldTetromino = Source.GetTetrominoSKBitmap(pOwner, HoldBlock);
                            if (HoldTetromino != null)
                                g.DrawBitmap(HoldTetromino, (float)(XOffset+ CenterPoint.X - HoldTetromino.Width / 2), (float)(YOffset + CenterPoint.Y - HoldTetromino.Height / 2));

                            XOffset += HoldTetromino.Width;
                            YOffset += Math.Sqrt(HoldTetromino.Height);
                            holdoffsetcount--;
                        }
                    }
                }
            }
            finally
            {
                LastDrawStat = Bounds;
            }

        }
    }

 
}
