﻿using System;
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
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(StandardTetrisGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class StandardTetrisGameStateSkiaRenderingHandler :   StandardStateRenderingHandler<SKCanvas,StandardTetrisGameState,GameStateSkiaDrawParameters>
    {
        private StandardImageBackgroundSkia _Background = null;
        public SKRect LastDrawStat = SKRect.Empty;
        
        private void BuildBackground(SKRect Size)
        {
            _Background = new StandardImageBackgroundSkia();
            Bitmap bmp = new Bitmap(ImageManager.ReduceImage(TetrisGame.Imageman["background"],
                new Size((int)(Size.Width + 0.5f), (int)(Size.Height + 0.5f))));
            
            SKImage usebg = SkiaSharp.Views.Desktop.Extensions.ToSKImage(bmp);
            _Background.Data = new StandardImageBackgroundDrawSkiaCapsule() { _BackgroundImage = usebg, Movement = new SKPoint(0, 0) };
        }
        SKImage StatisticsBackground = null;
        //redraws the StatisticsBackground SKImage.
        public void GenerateStatisticsBackground(StandardTetrisGameState Self)
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
                            Self.PlayField.Theme.ApplyRandom(ArbitraryGroup, Self.PlayField);
                            //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                            TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, new SKRect(DrawBlockX, DrawBlockY, DrawBlockX + BlockSize.Width, DrawBlockY + BlockSize.Height), null, new StandardSettings());
                            RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);

                        }
                    }
                }

                StatisticsBackground = SKImage.FromBitmap(sourcebit);
            }
        }


        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StandardTetrisGameState Source, GameStateSkiaDrawParameters Element)
        {
            //testing code for background.
            if (_Background==null)
            {
                BuildBackground(Element.Bounds);
            }
            _Background.FrameProc(pOwner);
            
            RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,_Background, new SkiaBackgroundDrawData(Element.Bounds));
            var PlayField = Source.PlayField;
            if (PlayField != null)
            {
                Object grabdata = RenderingProvider.Static.GetExtendedData(PlayField.GetType(), PlayField,
                    (o) =>
                        new TetrisFieldDrawSkiaParameters()
                        {
                            Bounds = Element.Bounds,
                            COLCOUNT = TetrisField.COLCOUNT,
                            ROWCOUNT = TetrisField.ROWCOUNT,
                            FieldBitmap = null,
                            LastFieldSave = SKRect.Empty,
                            VISIBLEROWS = TetrisField.VISIBLEROWS
                        });
                
                TetrisFieldDrawSkiaParameters parameters = (TetrisFieldDrawSkiaParameters)grabdata;
                parameters.LastFieldSave = Element.Bounds;

                RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,PlayField,parameters);
            }
            //draw the active block groups...
            /*
            foreach (var activeblock in PlayField.BlockGroups)
            {
                int dl = 0;
                var GrabGhost = pState.GetGhostDrop(pOwner, activeblock, out dl, 3);
                if (GrabGhost != null)
                {
                    var BlockWidth = PlayField.GetBlockWidth(Bounds);
                    var BlockHeight = PlayField.GetBlockHeight(Bounds);

                    foreach (var iterateblock in activeblock)
                    {
                        RectangleF BlockBounds = new RectangleF(BlockWidth * (GrabGhost.X + iterateblock.X), BlockHeight * (GrabGhost.Y + iterateblock.Y - 2), PlayField.GetBlockWidth(Bounds), PlayField.GetBlockHeight(Bounds));
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, BlockBounds, GrabGhost, pOwner.Settings);
                        ImageAttributes Shade = new ImageAttributes();
                        Shade.SetColorMatrix(ColorMatrices.GetFader(0.5f));
                        tbd.ApplyAttributes = Shade;
                        //tbd.OverrideBrush = GhostBrush;
                        var GetHandler = RenderingProvider.Static.GetHandler(typeof(Graphics), iterateblock.Block.GetType(), typeof(TetrisBlockDrawGDIPlusParameters));
                        GetHandler.Render(pOwner, tbd.g, iterateblock.Block, tbd);
                        //iterateblock.Block.DrawBlock(tbd);
                    }
                }
            }*/


            //TetrisGame.DrawTextSK(pRenderTarget,"Greetings!",new SKPoint(50,50),TetrisGame.RetroFontSK,new SKColor(0,0,255,128),28,pOwner.ScaleFactor);

        }
        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss");
        }
        public object LockTetImageRedraw = new Object();
        bool CalledRedraw = false;
        public void RedrawStatusbarTetrominoBitmaps(IStateOwner Owner, StandardTetrisGameState State, SKRect Bounds)
        {
            if(CalledRedraw)
            {
                ;
            }
            CalledRedraw = true;
            lock (LockTetImageRedraw)
            {

                State.SetTetrominoSKBitmaps(TetrisGame.GetTetrominoBitmapsSK(Bounds, State.PlayField.Theme, State.PlayField, (float)Owner.ScaleFactor));
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
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, StandardTetrisGameState Source, GameStateSkiaDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            
            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            try
            {
                if (StatisticsBackground == null || RedrawsNeeded)
                {
                    GenerateStatisticsBackground(Source);
                }

                g.DrawImage(StatisticsBackground, Bounds);
                //g.Clear(Color.Black);
                if (!Source.HasTetrominoSKBitmaps() || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(pOwner, Source, Bounds);

                lock (Source.LockTetImageRedraw)
                {
                    var useStats = Source.GameStats;
                    double Factor = Bounds.Height / 644d;
                    var DesiredFontPixelHeight = PixelsToPoints((int)(Bounds.Height * (23d / 644d)));
                    float DesiredFontSize = (float)DesiredFontPixelHeight;
                    SKPaint skp;
                    SKTypeface standardFont = TetrisGame.RetroFontSK;
                    //Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);

                    var TopScore = Source.GetLocalScores().GetScores().First().Score;
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
                    String[] StatLabels = new string[] { "Time:", "Score:", "Top:", "Lines:" };
                    String[] StatValues = new string[] { FormatGameTime(pOwner), useStats.Score.ToString(), TopScore.ToString(), Source.GameStats.LineCount.ToString() };
                    
                    BlackBrush.TextSize = (float)DesiredFontPixelHeight;
                    BlackBrush.Typeface = TetrisGame.RetroFontSK;
                    SKRect MeasureLabel = new SKRect(), MeasureValue = new SKRect();
                    BlackBrush.MeasureText("#", ref MeasureLabel);
                    BlackBrush.MeasureText("#", ref MeasureValue);
                    SKPoint StatPosition = new SKPoint(Bounds.Left + (int)(7 * Factor), Bounds.Top + MeasureLabel.Height + (int)(14 * Factor));
                    float CurrentYPosition = StatPosition.Y;
                    for (int statindex = 0; statindex < StatLabels.Length; statindex++)
                    {
                       float LabelWidth = BlackBrush.MeasureText(StatLabels[statindex] + "##", ref MeasureLabel);
                       float ValueWidth = BlackBrush.MeasureText(StatValues[statindex] + "##", ref MeasureValue);

                        //var MeasureLabel = g.MeasureString(StatLabels[statindex], standardFont);
                        //var MeasureValue = g.MeasureString(StatValues[statindex], standardFont);
                        float LargerHeight = Math.Max(MeasureLabel.Height, MeasureValue.Height);

                        //we want to draw the current stat label at position StatPosition.X,CurrentYPosition...

                        TetrisGame.DrawTextSK(g, StatLabels[statindex], new SKPoint(StatPosition.X, CurrentYPosition),
                            TetrisGame.RetroFontSK, SKColors.Black, DesiredFontSize, (float)pOwner.ScaleFactor);

                        //we want to draw the current stat value at Bounds.Width-ValueWidth.
                        TetrisGame.DrawTextSK(g, StatValues[statindex], new SKPoint(Bounds.Left + (float)(Bounds.Width - (MeasureValue.Width + (5 * Factor))), CurrentYPosition), TetrisGame.RetroFontSK, SKColors.Black, DesiredFontSize, (float)pOwner.ScaleFactor);

                        //add the larger of the two heights to the current Y Position.
                        CurrentYPosition += (int)LargerHeight;
                        CurrentYPosition += 10;

                    }






                    Type[] useTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z) };
                    int[] PieceCounts = new int[] { useStats.I_Piece_Count, useStats.O_Piece_Count, useStats.J_Piece_Count, useStats.T_Piece_Count, useStats.L_Piece_Count, useStats.S_Piece_Count, useStats.Z_Piece_Count };

                    float StartYPos = Bounds.Top + (int)(140 * Factor);
                    float useXPos = Bounds.Left + (int)(30 * Factor);
                    //ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
                    for (int i = 0; i < useTypes.Length; i++)
                    {
                        BlackBrush.TextSize = DesiredFontSize;
                        WhiteBrush.TextSize = DesiredFontSize;
                        SKPoint BaseCoordinate = new SKPoint(useXPos, StartYPos + (int)((float)i * (40d * Factor)));
                        SKPoint TextPos = new SKPoint(useXPos + (int)(100d * Factor), BaseCoordinate.Y);
                        String StatText = "" + PieceCounts[i];
                        SKRect StatTextSize = new SKRect();
                        BlackBrush.MeasureText(StatText, ref StatTextSize);
                        //SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                        SKBitmap TetrominoImage = Source.GetTetrominoSKBitmap(useTypes[i]);
                        PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - TetrominoImage.Height / 2));

                        g.DrawBitmap(TetrominoImage, ImagePos.X, ImagePos.Y, null);

                        TetrisGame.DrawTextSK(g, StatText, new SKPoint(Bounds.Left + TextPos.X + 4, Bounds.Top + TextPos.Y + 4), standardFont, Color.White.ToSKColor(), DesiredFontSize, pOwner.ScaleFactor);
                        TetrisGame.DrawTextSK(g, StatText, TextPos, standardFont, Color.Black.ToSKColor(), DesiredFontSize, pOwner.ScaleFactor);
                        //g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                        //g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
                    }

                    SKPoint NextDrawPosition = new SKPoint(Bounds.Left + (int)(40f * Factor), Bounds.Top + (int)(420 * Factor));
                    Size NextSize = new Size((int)(200f * Factor), (int)(200f * Factor));
                    SKPoint CenterPoint = new SKPoint(NextDrawPosition.X + NextSize.Width / 2, NextDrawPosition.Y + NextSize.Height / 2);
                    //now draw the "Next" Queue. For now we'll just show one "next" item.
                    if (Source.NextBlocks.Count > 0)
                    {
                        var QueueList = Source.NextBlocks.ToArray();
                        SKBitmap[] NextTetrominoes = (from t in QueueList select Source.GetTetrominoSKBitmap(t.GetType())).ToArray();
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

                        for (int i = NextTetrominoes.Length - 1; i > -1; i--)
                        {
                            double StartAngle = Math.PI;
                            double AngleIncrementSize = (Math.PI * 1.8) / (double)NextTetrominoes.Length;
                            //we draw starting at StartAngle, in increments of AngleIncrementSize.
                            //i is the index- we want to increase the angle by that amount (well, obviously, I suppose...

                            double UseAngleCurrent = StartAngle + AngleIncrementSize * (float)i + Source.NextAngleOffset;

                            double UseXPosition = CenterPoint.X + ((float)((NextSize.Width) / 2.2) * Math.Cos(UseAngleCurrent));
                            double UseYPosition = CenterPoint.Y + ((float)((NextSize.Height) / 2.2) * Math.Sin(UseAngleCurrent));


                            var NextTetromino = NextTetrominoes[i];

                            float Deviation = (i - NextTetrominoes.Length / 2);
                            Point Deviate = new Point((int)(Deviation * 20 * Factor), (int)(Deviation * 20 * Factor));

                            Point DrawTetLocation = new Point((int)UseXPosition - (NextTetromino.Width / 2), (int)UseYPosition - NextTetromino.Height / 2);
                            //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                            //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                            double AngleMovePercent = Source.NextAngleOffset / AngleIncrementSize;
                            double NumAffect = Source.NextAngleOffset == 0 ? 0 : AngleIncrementSize / Source.NextAngleOffset;
                            Size DrawTetSize = new Size
                            (
                                (int)((float)NextTetromino.Width * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))),
                                (int)((float)NextTetromino.Height * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))));


                            //g.TranslateTransform(CenterPoint.X,CenterPoint.Y);
                            double DrawTetAngle = UseAngleCurrent;
                            DrawTetAngle += (Math.PI * AngleMovePercent);
                            float useDegrees = 180 + (float)(DrawTetAngle * (180 / Math.PI));

                            g.RotateDegrees(useDegrees, DrawTetLocation.X + DrawTetSize.Width / 2, DrawTetLocation.Y + DrawTetSize.Width / 2);


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

                    if (Source.HoldBlock != null)
                    {
                        SKBitmap HoldTetromino = Source.GetTetrominoSKBitmap(Source.HoldBlock.GetType());
                        g.DrawBitmap(HoldTetromino, CenterPoint.X - HoldTetromino.Width / 2, CenterPoint.Y - HoldTetromino.Height / 2);
                    }
                }
            }
            finally
            {
                LastDrawStat = Bounds;
            }

        }
    }

    /*public class StandardTetrisGameStateRenderingHandler : StandardStateRenderingHandler<Graphics, StandardTetrisGameState, GameStateDrawParameters>
    {
        public RectangleF LastDrawStat = Rectangle.Empty;
        private Dictionary<System.Type, Image> TetrominoImages = null;
        Brush LightenBrush = new SolidBrush(Color.FromArgb(128, Color.MintCream));
        
      
        Image StatisticsBackground = null;
        
        public void GenerateStatisticsBackground(StandardTetrisGameState Self)
        {
            Bitmap buildbg = new Bitmap(1120, 2576);
            Size BlockSize = new Size(128, 128);
            int ColumnCount = (buildbg.Width / BlockSize.Width) + 1;
            int RowCount = (buildbg.Height / BlockSize.Height) + 1;
            using (Graphics g = Graphics.FromImage(buildbg))
            {
                g.Clear(Color.Black);
                for (int col = 0; col < ColumnCount; col++)
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        int DrawBlockX = col * BlockSize.Width;
                        int DrawBlockY = row * BlockSize.Height;
                        StandardColouredBlock GenerateColorBlock = new StandardColouredBlock();
                        Nomino ArbitraryGroup = new Nomino();
                        ArbitraryGroup.AddBlock(new Point[] { Point.Empty }, GenerateColorBlock);
                        Self.PlayField.Theme.ApplyRandom(ArbitraryGroup, Self.PlayField);
                        //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null, new StandardSettings());
                        RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);

                    }
                }
            }

            StatisticsBackground = buildbg;
        }
        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss");
        }
        public object LockTetImageRedraw = new Object();
        public void RedrawStatusbarTetrominoBitmaps(IStateOwner Owner,StandardTetrisGameState State, RectangleF Bounds)
        {
            lock (LockTetImageRedraw)
            {
                TetrominoImages = TetrisGame.GetTetrominoBitmaps(Bounds, State.PlayField.Theme, State.PlayField, (float)Owner.ScaleFactor);
            }
        }
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;

            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            LastDrawStat = Bounds;
            if (StatisticsBackground == null || RedrawsNeeded)
            {
                GenerateStatisticsBackground(Source);
            }

            g.DrawImage(StatisticsBackground, Bounds);
            //g.Clear(Color.Black);
            if (TetrominoImages == null || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(pOwner,Source, Bounds);
            Process p;
            
            lock (Source.LockTetImageRedraw)
            {
                var useStats = Source.GameStats;
                double Factor = Bounds.Height / 644d;
                int DesiredFontPixelHeight = (int)(Bounds.Height * (23d / 644d));
                Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);
                var TopScore = Source.GetLocalScores().GetScores().First().Score;
                int MaxScoreLength = Math.Max(TopScore.ToString().Length, useStats.Score.ToString().Length);

                String CurrentScoreStr = useStats.Score.ToString().PadLeft(MaxScoreLength + 2);
                String TopScoreStr = TopScore.ToString().PadLeft(MaxScoreLength + 2);
                //TODO: redo this segment separately, so we can have the labels left-aligned and the values right-aligned.
                // String BuildStatString = "Time:  " + FormatGameTime(pOwner).ToString().PadLeft(MaxScoreLength + 2) + "\n" +
                //                          "Score: " + CurrentScoreStr + "\n" +
                //                          "Top:   " + TopScoreStr + " \n" +
                //                          "Lines: " + GameStats.LineCount.ToString().PadLeft(MaxScoreLength+2);

                g.FillRectangle(LightenBrush, 0, 5, Bounds.Width, (int)(450 * Factor));
                String[] StatLabels = new string[] { "Time:", "Score:", "Top:", "Lines:" };
                String[] StatValues = new string[] { FormatGameTime(pOwner), useStats.Score.ToString(), TopScore.ToString(), Source.GameStats.LineCount.ToString() };
                Point StatPosition = new Point((int)(7 * Factor), (int)(7 * Factor));

                int CurrentYPosition = StatPosition.Y;
                for (int statindex = 0; statindex < StatLabels.Length; statindex++)
                {
                    var MeasureLabel = g.MeasureString(StatLabels[statindex], standardFont);
                    var MeasureValue = g.MeasureString(StatValues[statindex], standardFont);
                    float LargerHeight = Math.Max(MeasureLabel.Height, MeasureValue.Height);

                    //we want to draw the current stat label at position StatPosition.X,CurrentYPosition...

                    TetrisGame.DrawText(g, standardFont, StatLabels[statindex], Brushes.Black, Brushes.White, StatPosition.X, CurrentYPosition);

                    //we want to draw the current stat value at Bounds.Width-ValueWidth.
                    TetrisGame.DrawText(g, standardFont, StatValues[statindex], Brushes.Black, Brushes.White, (float)(Bounds.Width - MeasureValue.Width - (5 * Factor)), CurrentYPosition);

                    //add the larger of the two heights to the current Y Position.
                    CurrentYPosition += (int)LargerHeight;
                    CurrentYPosition += 2;

                }






                Type[] useTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z) };
                int[] PieceCounts = new int[] { useStats.I_Piece_Count, useStats.O_Piece_Count, useStats.J_Piece_Count, useStats.T_Piece_Count, useStats.L_Piece_Count, useStats.S_Piece_Count, useStats.Z_Piece_Count };

                int StartYPos = (int)(140 * Factor);
                int useXPos = (int)(30 * Factor);
                ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
                for (int i = 0; i < useTypes.Length; i++)
                {
                    PointF BaseCoordinate = new PointF(useXPos, StartYPos + (int)((float)i * (40d * Factor)));
                    PointF TextPos = new PointF(useXPos + (int)(100d * Factor), BaseCoordinate.Y);
                    String StatText = "" + PieceCounts[i];
                    SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                    Image TetrominoImage = TetrominoImages[useTypes[i]];
                    PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - TetrominoImage.Height / 2));

                    g.DrawImage(TetrominoImage, ImagePos);
                    g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                    g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
                }

                Point NextDrawPosition = new Point((int)(40f * Factor), (int)(420 * Factor));
                Size NextSize = new Size((int)(200f * Factor), (int)(200f * Factor));
                Point CenterPoint = new Point(NextDrawPosition.X + NextSize.Width / 2, NextDrawPosition.Y + NextSize.Height / 2);
                //now draw the "Next" Queue. For now we'll just show one "next" item.
                if (Source.NextBlocks.Count > 0)
                {
                    var QueueList = Source.NextBlocks.ToArray();
                    Image[] NextTetrominoes = (from t in QueueList select TetrominoImages[t.GetType()]).ToArray();
                    Image DisplayBox = TetrisGame.Imageman["display_box"];
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

                    g.DrawImage
                    (DisplayBox,
                        new Rectangle(new Point((int)(NextDrawPosition.X - ScaleDiff), (int)(NextDrawPosition.Y - ScaleDiff)), new Size((int)(NextSize.Width + (ScaleDiff * 2)), (int)(NextSize.Height + (ScaleDiff * 2)))), 0, 0, DisplayBox.Width, DisplayBox.Height, GraphicsUnit.Pixel);

                    g.FillEllipse(Brushes.Black, CenterPoint.X - 5, CenterPoint.Y - 5, 10, 10);

                    for (int i = NextTetrominoes.Length - 1; i > -1; i--)
                    {
                        double StartAngle = Math.PI;
                        double AngleIncrementSize = (Math.PI * 1.8) / (double)NextTetrominoes.Length;
                        //we draw starting at StartAngle, in increments of AngleIncrementSize.
                        //i is the index- we want to increase the angle by that amount (well, obviously, I suppose...

                        double UseAngleCurrent = StartAngle + AngleIncrementSize * (float)i + Source.NextAngleOffset;

                        double UseXPosition = CenterPoint.X + ((float)((NextSize.Width) / 2.2) * Math.Cos(UseAngleCurrent));
                        double UseYPosition = CenterPoint.Y + ((float)((NextSize.Height) / 2.2) * Math.Sin(UseAngleCurrent));


                        var NextTetromino = NextTetrominoes[i];

                        float Deviation = (i - NextTetrominoes.Length / 2);
                        Point Deviate = new Point((int)(Deviation * 20 * Factor), (int)(Deviation * 20 * Factor));

                        Point DrawTetLocation = new Point((int)UseXPosition - (NextTetromino.Width / 2), (int)UseYPosition - NextTetromino.Height / 2);
                        //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                        //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                        double AngleMovePercent = Source.NextAngleOffset / AngleIncrementSize;
                        double NumAffect = Source.NextAngleOffset == 0 ? 0 : AngleIncrementSize / Source.NextAngleOffset;
                        Size DrawTetSize = new Size
                        (
                            (int)((float)NextTetromino.Width * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))),
                            (int)((float)NextTetromino.Height * (0.3 + (1 - ((float)(i) * 0.15f) - .15f * AngleMovePercent))));


                        //g.TranslateTransform(CenterPoint.X,CenterPoint.Y);

                        g.TranslateTransform(DrawTetLocation.X + DrawTetSize.Width / 2, DrawTetLocation.Y + DrawTetSize.Width / 2);


                        double DrawTetAngle = UseAngleCurrent;
                        DrawTetAngle += (Math.PI * AngleMovePercent);
                        float useDegrees = 180 + (float)(DrawTetAngle * (180 / Math.PI));

                        g.RotateTransform((float)useDegrees);

                        g.TranslateTransform(-(DrawTetLocation.X + DrawTetSize.Width / 2), -(DrawTetLocation.Y + DrawTetSize.Height / 2));
                        //g.TranslateTransform(-CenterPoint.X,-CenterPoint.Y);


                        if (DrawTetSize.Width > 0 && DrawTetSize.Height > 0)
                        {
                            //ImageAttributes Shade = GetShadowAttributes(1.0f - ((float)i * 0.3f));
                            ImageAttributes Shade = new ImageAttributes();
                            Shade.SetColorMatrix(ColorMatrices.GetFader(1.0f - ((float)i * 0.1f)));


                            g.DrawImage
                            (NextTetromino, new Rectangle((int)DrawTetLocation.X, (int)DrawTetLocation.Y, DrawTetSize.Width, DrawTetSize.Height), 0f, 0f,
                                (float)NextTetromino.Width, (float)NextTetromino.Height, GraphicsUnit.Pixel, Shade);
                        }

                        g.ResetTransform();
                    }
                }

                if (Source.HoldBlock != null)
                {
                    Image HoldTetromino = TetrominoImages[Source.HoldBlock.GetType()];
                    g.DrawImage(HoldTetromino, CenterPoint.X - HoldTetromino.Width / 2, CenterPoint.Y - HoldTetromino.Height / 2);
                }
            }
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            Source._DrawHelper.DrawProc(Source, pOwner, pRenderTarget, Element.Bounds);
            //throw new NotImplementedException();
        }
    }*/
}