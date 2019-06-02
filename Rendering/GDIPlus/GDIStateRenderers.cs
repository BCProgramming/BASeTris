using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AssetManager;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.RenderElements;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;

namespace BASeTris.Rendering.GDIPlus
{
    /// <summary>
    /// Base DrawElement type
    /// </summary>
    public class GameStateDrawParameters
    {
        public RectangleF Bounds;
        public GameStateDrawParameters(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    
    public class StandardTetrisGameStateRenderingHandler : StandardStateRenderingHandler<Graphics, StandardTetrisGameState, GameStateDrawParameters>
    {
        public RectangleF LastDrawStat = Rectangle.Empty;
        private Dictionary<System.Type, Image> TetrominoImages = null;
        Brush LightenBrush = new SolidBrush(Color.FromArgb(128, Color.MintCream));
        double NextAngleOffset = 0; //use this to animate the "Next" ring... Set it to a specific value and GameProc should reduce it to zero over time.
        public Image GetTetronimoImage(System.Type TetrominoType)
        {
            return TetrominoImages[TetrominoType];
        }
        Image StatisticsBackground = null;
        public Image[] GetTetronimoImages() => TetrominoImages.Values.ToArray();
        public Object LockTetImageRedraw = new Object();
        private void RedrawStatusbarTetrominoBitmaps(StandardTetrisGameState Self,IStateOwner Owner, RectangleF Bounds)
        {
            lock (LockTetImageRedraw)
            {
                TetrominoImages = TetrisGame.GetTetrominoBitmaps(Bounds, Self.PlayField.Theme, Self.PlayField, (float)Owner.ScaleFactor);
            }
        }
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
        
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            Graphics g = pRenderTarget;
            bool RedrawsNeeded = !LastDrawStat.Equals(Bounds);
            LastDrawStat = Bounds;
            if (StatisticsBackground == null || RedrawsNeeded)
            {
                GenerateStatisticsBackground(Source);
            }

            g.DrawImage(StatisticsBackground, Bounds);
            //g.Clear(Color.Black);
            if (TetrominoImages == null || RedrawsNeeded) RedrawStatusbarTetrominoBitmaps(Source,pOwner, Bounds);

            lock (LockTetImageRedraw)
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

                        double UseAngleCurrent = StartAngle + AngleIncrementSize * (float)i + NextAngleOffset;

                        double UseXPosition = CenterPoint.X + ((float)((NextSize.Width) / 2.2) * Math.Cos(UseAngleCurrent));
                        double UseYPosition = CenterPoint.Y + ((float)((NextSize.Height) / 2.2) * Math.Sin(UseAngleCurrent));


                        var NextTetromino = NextTetrominoes[i];

                        float Deviation = (i - NextTetrominoes.Length / 2);
                        Point Deviate = new Point((int)(Deviation * 20 * Factor), (int)(Deviation * 20 * Factor));

                        Point DrawTetLocation = new Point((int)UseXPosition - (NextTetromino.Width / 2), (int)UseYPosition - NextTetromino.Height / 2);
                        //Point DrawTetLocation = new Point(Deviate.X + (int)(NextDrawPosition.X + ((float)NextSize.Width / 2) - ((float)NextTetromino.Width / 2)),
                        //    Deviate.Y + (int)(NextDrawPosition.Y + ((float)NextSize.Height / 2) - ((float)NextTetromino.Height / 2)));
                        double AngleMovePercent = NextAngleOffset / AngleIncrementSize;
                        double NumAffect = NextAngleOffset == 0 ? 0 : AngleIncrementSize / NextAngleOffset;
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
            //render the stats image.
            //throw new NotImplementedException();
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            Source._DrawHelper.DrawProc(Source, pOwner, pRenderTarget, Element.Bounds);
            //throw new NotImplementedException();
        }
    }
    public class PauseGameStateRenderingHandler: StandardStateRenderingHandler<Graphics, PauseGameState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            var FallImages = Source.FallImages;
            //Render the paused state.
            Font usePauseFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }

            g.ResetTransform();
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            TetrisGame.DrawText(g, usePauseFont, sPauseText, Brushes.White, Brushes.Black, DrawPos.X, DrawPos.Y);
            //retrieve the renderer for the MenuState object.
            var basecall = RenderingProvider.Static.GetHandler(typeof(Graphics), typeof(MenuState), typeof(GameStateDrawParameters));
            basecall?.Render(pOwner,pRenderTarget,Source,Element); //draw the menu itself.

        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            //delegate...
            var PausedState = Source.PausedState;
            if(PausedState!=null)
            {
                RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,PausedState,Element);
            }
            
        }

       
    }
    public class MenuStateRenderingHandler :StandardStateRenderingHandler<Graphics,MenuState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            if (Source._BG != null) Source._BG.DrawProc(g, Bounds);
            int CurrentIndex = Source.StartItemOffset;
            float CurrentY = Source.DrawHeader(pOwner, g, Bounds);
            float MaxHeight = 0, MaxWidth = 0;
            //we want to find the widest item.
            foreach (var searchitem in Source.MenuElements)
            {
                if (searchitem is MenuStateSizedMenuItem mss)
                {
                    var grabsize = mss.GetSize(pOwner);
                    if (grabsize.Height > MaxHeight) MaxHeight = grabsize.Height;
                    if (grabsize.Width > MaxWidth) MaxWidth = grabsize.Width;
                }
            }
            //we draw each item at the maximum size.
            SizeF ItemSize = new SizeF(MaxWidth, MaxHeight);
            CurrentY += (float)(pOwner.ScaleFactor * 5);
            for (int menuitemindex = 0; menuitemindex < Source.MenuElements.Count; menuitemindex++)
            {
                var drawitem = Source.MenuElements[menuitemindex];
                Rectangle TargetBounds = new Rectangle((int)(Bounds.Width / 2 - ItemSize.Width / 2) + Source.MainXOffset, (int)CurrentY, (int)(ItemSize.Width), (int)(ItemSize.Height));
                MenuStateMenuItem.StateMenuItemState useState = menuitemindex == Source.SelectedIndex ? MenuStateMenuItem.StateMenuItemState.State_Selected : MenuStateMenuItem.StateMenuItemState.State_Normal;
                drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Height + 5;
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
