using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.GameStates;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Settings;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(GameplayGameState), typeof(Graphics), typeof(BaseDrawParameters))]
    public class StandardTetrisGameStateGDIPlusRenderingHandler : StandardStateRenderingHandler<Graphics, GameplayGameState, BaseDrawParameters>
    {
        public RectangleF LastDrawStat = Rectangle.Empty;
        //private Dictionary<System.Type, Image> TetrominoImages = null;
        Brush LightenBrush = new SolidBrush(Color.FromArgb(128, Color.MintCream));
        
      
        Image StatisticsBackground = null;
        NominoTheme GeneratedImageTheme = null; 
        public void GenerateStatisticsBackground(GameplayGameState Self)
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
                        Self.PlayField.Theme.ApplyRandom(ArbitraryGroup,Self.GameHandler, Self.PlayField);
                        //this.PlayField.Theme.ApplyTheme(ArbitraryGroup, this.PlayField);
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, new RectangleF(DrawBlockX, DrawBlockY, BlockSize.Width, BlockSize.Height), null, new SettingsManager());
                        RenderingProvider.Static.DrawElement(null, tbd.g, GenerateColorBlock, tbd);

                    }
                }
            }

            StatisticsBackground = buildbg;
            GeneratedImageTheme = Self.PlayField.Theme;
        }
        private String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss");
        }
        public object LockTetImageRedraw = new Object();
        public void RedrawStatusbarTetrominoBitmaps(IStateOwner Owner,GameplayGameState State, RectangleF Bounds)
        {
            lock (LockTetImageRedraw)
            {
                
                State.SetTetrominoImages(TetrisGame.GetTetrominoBitmaps(Bounds, State.PlayField.Theme, State.GameHandler, State.PlayField, (float)Owner.ScaleFactor));
            }
        }
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, GameplayGameState Source, BaseDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;

            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            LastDrawStat = Bounds;
            if (StatisticsBackground == null || RedrawsNeeded || GeneratedImageTheme!=Source.PlayField.Theme)
            {
                GenerateStatisticsBackground(Source);
            }

            g.DrawImage(StatisticsBackground, Bounds);
            //g.Clear(Color.Black);
            if (!Source.HasTetrominoImages() || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(pOwner,Source, Bounds);
            
            lock (Source.LockTetImageRedraw)
            {
                var useStats = Source.GameStats;
                double Factor = Bounds.Height / 644d;
                int DesiredFontPixelHeight = (int)(Bounds.Height * (23d / 644d));
                using (Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    var LocalScores = Source.GetLocalScores();
                    var TopScore = LocalScores == null ? 0 : LocalScores.GetScores().First().Score;
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
                    int LineCount = Source.GameStats is TetrisStatistics ? (Source.GameStats as TetrisStatistics).LineCount : 0;
                    String[] StatValues = new string[] { FormatGameTime(pOwner), useStats.Score.ToString(), TopScore.ToString(), LineCount.ToString() };
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

                    int[] PieceCounts = null;

                    if (useStats is TetrisStatistics ts)
                    {
                        PieceCounts = new int[] { ts.I_Piece_Count, ts.O_Piece_Count, ts.J_Piece_Count, ts.T_Piece_Count, ts.L_Piece_Count, ts.S_Piece_Count, ts.Z_Piece_Count };
                    }
                    else
                    {
                        PieceCounts = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    }


                    int StartYPos = (int)(140 * Factor);
                    int useXPos = (int)(30 * Factor);
                    ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
                    if (Source.GameHandler is StandardTetrisHandler)
                    {
                        for (int i = 0; i < useTypes.Length; i++)
                        {
                            PointF BaseCoordinate = new PointF(useXPos, StartYPos + (int)((float)i * (40d * Factor)));
                            PointF TextPos = new PointF(useXPos + (int)(100d * Factor), BaseCoordinate.Y);
                            String StatText = "" + PieceCounts[i];
                            SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                            String sNomTypeKey = Source.PlayField.Theme.GetNominoTypeKey(useTypes[i], Source.GameHandler, Source.PlayField);
                            Image TetrominoImage = TetrisGame.Choose(Source.ImageManager.NominoImages[sNomTypeKey]);
                            PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - TetrominoImage.Height / 2));

                            g.DrawImage(TetrominoImage, ImagePos);
                            g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                            g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
                        }
                    }

                    Point NextDrawPosition = new Point((int)(40f * Factor), (int)(420 * Factor));
                    Size NextSize = new Size((int)(200f * Factor), (int)(200f * Factor));
                    Point CenterPoint = new Point(NextDrawPosition.X + NextSize.Width / 2, NextDrawPosition.Y + NextSize.Height / 2);
                    //now draw the "Next" Queue. For now we'll just show one "next" item.
                    if (Source.NextBlocks.Count > 0)
                    {
                        var QueueList = Source.NextBlocks.ToArray();
                        //(from t in QueueList select Source.GetTetrominoSKBitmap(pOwner,t)).ToArray()
                        Image[] NextTetrominoes = (from t in QueueList select Source.GetTetrominoImage(pOwner, t)).ToArray(); //  TetrisGame.Choose(Source.NominoImages[Source.PlayField.Theme.GetNominoKey(t,Source.GameHandler,Source.PlayField)])).ToArray();
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
                        var GetKey = Source.PlayField.Theme.GetNominoKey(Source.HoldBlock, Source.GameHandler, Source.PlayField);
                        Image HoldTetromino = Source.GetTetrominoImage(pOwner, Source.HoldBlock);
                        g.DrawImage(HoldTetromino, CenterPoint.X - HoldTetromino.Width / 2, CenterPoint.Y - HoldTetromino.Height / 2);
                    }
                }
            }
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, GameplayGameState Source, BaseDrawParameters Element)
        {
            Source._DrawHelper.DrawProc(Source, pOwner, pRenderTarget, Element.Bounds);
            //throw new NotImplementedException();
        }
    }
}