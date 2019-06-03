using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
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
    
    public class ViewScoreDetailsStateHandler : StandardStateRenderingHandler<Graphics,ViewScoreDetailsState,GameStateDrawParameters>
    {
        private Pen Separator = new Pen(Color.Black, 3);
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, ViewScoreDetailsState Source, GameStateDrawParameters Element)
        {
            var g = pRenderTarget;
            var Bounds = Element.Bounds;
            Source._BG.DrawProc(g, Bounds);

            Font HeaderFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            Font PlacementFont = TetrisGame.GetRetroFont(10, pOwner.ScaleFactor);
            Font DetailFont = TetrisGame.GetRetroFont(8, pOwner.ScaleFactor);


            //One thing we draw in every case is the "--SCORE DETAILS--" header text. this is positioned at 5% from the top, centered in the middle of our bounds.
            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            var MeasuredHeader = g.MeasureString(Source._DetailHeader, HeaderFont);
            int RotateAmount = (int)(Millipercent * 240);
            Color UseColor1 = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color UseColor2 = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            PointF ScorePosition = new PointF((Bounds.Width / 2) - (MeasuredHeader.Width / 2), Bounds.Height * 0.05f);
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(0, 0, (int)MeasuredHeader.Width, (int)MeasuredHeader.Height), UseColor1, UseColor2, LinearGradientMode.Vertical))
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddString(Source._DetailHeader, HeaderFont, new Point((int)ScorePosition.X, (int)ScorePosition.Y), StringFormat.GenericDefault);
                    g.FillPath(lgb, gp);
                    g.DrawPath(Pens.White, gp);
                }
            }

            //we also show Xth Place - <NAME> centered below the header using the placementfont.
            String sPlacement = TetrisGame.FancyNumber(Source._Position) + " - " + Source.ShowEntry.Name + " - " + Source.ShowEntry.Score.ToString();

            var measureit = g.MeasureString(sPlacement, PlacementFont);

            PointF DrawPlacement = new PointF(Bounds.Width / 2 - measureit.Width / 2, (float)(ScorePosition.Y + MeasuredHeader.Height * 1.1f));

            g.DrawString(sPlacement, PlacementFont, Brushes.Black, DrawPlacement.X + 3, DrawPlacement.Y + 3);
            g.DrawString(sPlacement, PlacementFont, Brushes.White, DrawPlacement.X, DrawPlacement.Y);

            g.DrawLine(Separator, (float)(Bounds.Width * 0.05f), (float)(DrawPlacement.Y + measureit.Height + 5), (float)(Bounds.Width * 0.95), (float)(DrawPlacement.Y + measureit.Height + 5));


            switch (Source.CurrentView)
            {
                case ViewScoreDetailsState.ViewScoreDetailsType.Details_Tetrominoes:
                    DrawTetronimoDetails(Source,g, Bounds);
                    break;
                case ViewScoreDetailsState.ViewScoreDetailsType.Details_LevelTimes:
                    DrawLevelTimesDetails(Source,g, Bounds);
                    break;
            }
        }
        private void DrawTetronimoDetails(ViewScoreDetailsState Source,Graphics g, RectangleF Bounds)
        {
            //draws the tetronimo pictures, the tetronimo stats, and the numberof lines down the screen.
        }

        private void DrawLevelTimesDetails(ViewScoreDetailsState Source, Graphics g, RectangleF Bounds)
        {
            //draw the times each level was achieved.
            //(Possible feature: support paging if  we can't fit them on one screen?)
        }
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, ViewScoreDetailsState Source, GameStateDrawParameters Element)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowHighScoreStateRenderingHandler : StandardStateRenderingHandler<Graphics,ShowHighScoresState,GameStateDrawParameters>
    {
        Pen LinePen = new Pen(new LinearGradientBrush(new Rectangle(0, 0, 5, 25), Color.Black, Color.DarkGray, LinearGradientMode.Vertical), 25);
        private String PointerText = "►";
        Font ScoreFont = null;
        private void DrawBackground(ShowHighScoresState Self, IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //ColorMatrices.GetFader(1.0f - ((float)i * 0.1f))
            g.Clear(Color.White);


            Self._BG.DrawProc(g, Bounds);
        }

        private Brush GetHighlightBrush()
        {
            return DateTime.Now.Millisecond < 500 ? Brushes.Lime : Brushes.Blue;
        }
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, ShowHighScoresState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            float StartY = Bounds.Height * 0.175f;
            float MiddleX = Bounds.Width / 2;
            DrawBackground(Source,pOwner, g, Bounds);
            float TextSize = Bounds.Height / 30f;
            using (ScoreFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                float LineHeight = g.MeasureString("#", ScoreFont).Height + 5;
                //This needs to change based on the actual gameplay area size.)
                if (Source.IncrementedDrawState >= 0)
                {
                    //Draw HIGH SCORES
                    var Measured = g.MeasureString(Source.HeaderText, ScoreFont);
                    PointF DrawPosition = new PointF(MiddleX - (Measured.Width / 2), StartY);
                    g.DrawString(Source.HeaderText, ScoreFont, Brushes.White, new PointF(DrawPosition.X + 2, DrawPosition.Y + 2));
                    g.DrawString(Source.HeaderText, ScoreFont, Brushes.Black, DrawPosition);
                }

                if (Source.IncrementedDrawState >= 1)
                {
                    float LineYPosition = StartY + LineHeight;

                    //draw a line underneath the High scores text
                    g.DrawLine(LinePen, 20, LineYPosition, Bounds.Width - 20, LineYPosition);
                }

                if (Source.IncrementedDrawState >= 2)
                {
                    //draw the high score listing entries.
                    //iterate from 2 to drawstate and draw the high score at position drawstate-2.
                    for (int scoreiterate = 2; scoreiterate < Source.IncrementedDrawState; scoreiterate++)
                    {
                        int CurrentScoreIndex = scoreiterate - 2;
                        int CurrentScorePosition = CurrentScoreIndex + 1;
                        float useYPosition = StartY + (LineHeight * 2.5f) + LineHeight * CurrentScoreIndex;
                        float UseXPosition = Bounds.Width * 0.19f;
                        String sUseName = "N/A";
                        int sUseScore = 0;
                        IHighScoreEntry currentScore = Source.hs.Count > CurrentScoreIndex ? Source.hs[CurrentScoreIndex] : null;
                        if (currentScore != null)
                        {
                            sUseName = currentScore.Name;
                            sUseScore = currentScore.Score;
                        }

                        var MeasureScore = g.MeasureString(sUseScore.ToString(), ScoreFont);
                        var MeasureName = g.MeasureString(sUseName, ScoreFont);
                        float PosXPosition = Bounds.Width * 0.1f;
                        float NameXPosition = Bounds.Width * 0.20f;
                        float ScoreXPositionRight = Bounds.Width * (1 - 0.10f);
                        Brush DrawScoreBrush = Source.HighlightedScorePositions.Contains(CurrentScorePosition) ? GetHighlightBrush() : Brushes.Gray;

                        g.DrawString(CurrentScorePosition.ToString(), ScoreFont, Brushes.Black, PosXPosition + 2, useYPosition + 2);
                        g.DrawString(CurrentScorePosition.ToString(), ScoreFont, DrawScoreBrush, PosXPosition, useYPosition);

                        g.DrawString(sUseName, ScoreFont, Brushes.Black, NameXPosition + 2, useYPosition + 2);
                        g.DrawString(sUseName, ScoreFont, DrawScoreBrush, NameXPosition, useYPosition);

                        float ScoreXPosition = ScoreXPositionRight - MeasureScore.Width;

                        g.DrawString(sUseScore.ToString(), ScoreFont, Brushes.Black, ScoreXPosition + 2, useYPosition + 2);
                        g.DrawString(sUseScore.ToString(), ScoreFont, DrawScoreBrush, ScoreXPosition, useYPosition);

                        g.DrawLine(new Pen(DrawScoreBrush, 3), NameXPosition + MeasureName.Width + 15, useYPosition + LineHeight / 2, ScoreXPosition - 15, useYPosition + LineHeight / 2);

                        if (Source.SelectedScorePosition == CurrentScoreIndex)
                        {
                            //draw the selection arrow to the left of the NamePosition and useYPosition.
                            var MeasureArrow = g.MeasureString(PointerText, ScoreFont);
                            float ArrowX = PosXPosition - MeasureArrow.Width - 5;
                            float ArrowY = useYPosition;
                            g.DrawString(PointerText, ScoreFont, Brushes.Black, ArrowX + 2, ArrowY + 2);
                            g.DrawString(PointerText, ScoreFont, DrawScoreBrush, ArrowX, ArrowY);
                        }
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, ShowHighScoresState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
    public class UnpauseDelayStateRenderingHandler : StandardStateRenderingHandler<Graphics,UnpauseDelayGameState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, UnpauseDelayGameState Source, GameStateDrawParameters Element)
        {
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,Source._ReturnState,Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
            //draw a centered Countdown

            if (Source.LastSecond != Source.timeremaining.Seconds)
            {
                //emit a sound.
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOverShade, pOwner.Settings.EffectVolume);
                Source.LastSecond = Source.timeremaining.Seconds;
                Source.lastMillis = 1000;
            }

            double SecondsLeft = Math.Round(Source.timeremaining.TotalSeconds, 1);
            String sSecondsLeft = Source.timeremaining.ToString("%s");
            double Millis = (double)Source.timeremaining.Milliseconds / 1000d; //millis in percent. We will use this to animate the unpause time left.
            Millis = Math.Min(Millis, Source.lastMillis);
            float useSize = (float)(64f * (1 - (Millis)));
            var SecondsFont = TetrisGame.GetRetroFont(useSize, pOwner.ScaleFactor);
            var MeasureText = g.MeasureString(sSecondsLeft, SecondsFont);

            PointF DrawPosition = new PointF(Bounds.Width / 2 - MeasureText.Width / 2, Bounds.Height / 2 - MeasureText.Height / 2);

            g.DrawString(sSecondsLeft, SecondsFont, Brushes.White, DrawPosition);
            Source.lastMillis = Millis;
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, UnpauseDelayGameState Source, GameStateDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._ReturnState,Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(pRenderTarget, Element.Bounds);

            
        }
        private void DrawFadeOverlay(Graphics g, RectangleF Bounds)
        {
            g.FillRectangle(fadeBrush, Bounds);
        }
        Brush fadeBrush = new SolidBrush(Color.FromArgb(200, Color.Black));
    }
    public class GameOverStateRenderingHandler : StandardStateRenderingHandler<Graphics,GameOverGameState,GameStateDrawParameters>
    {
        Brush useCoverBrush = null;
        private void DrawTetrominoStat(GameOverGameState Self,  Type TetronimoType, PointF BasePosition, Graphics Target, RectangleF Bounds, Font GameOverFont)
        {
            StandardTetrisGameState standardgame = Self.GameOveredState as StandardTetrisGameState;
            Image I_Tet = standardgame.GetTetronimoImage(TetronimoType);
            Target.DrawImage(I_Tet, new PointF(BasePosition.X - (float)(I_Tet.Width) / 2, BasePosition.Y));
            PointF TextPos = new PointF(BasePosition.X + Bounds.Width / 2, BasePosition.Y - 10);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.White, 5 + TextPos.X, 5 + TextPos.Y);
            Target.DrawString(standardgame.GameStats.GetLineCount(TetronimoType).ToString(), GameOverFont, Brushes.Black, TextPos.X, TextPos.Y);
        }
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, GameOverGameState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.GameOveredState is StandardTetrisGameState)
            {
                StandardTetrisGameState standardgame = Source.GameOveredState as StandardTetrisGameState;
                SizeF BlockSize = new SizeF(Bounds.Width / (float)standardgame.PlayField.ColCount, Bounds.Height / (float)standardgame.PlayField.RowCount);
                useCoverBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)Bounds.Width, (int)BlockSize.Height), Color.DarkSlateGray, Color.MintCream, LinearGradientMode.Vertical);
                RenderingProvider.Static.DrawElement(pOwner,g, Source.GameOveredState,Element);
                g.FillRectangle(useCoverBrush, 0f, 0f, (float)Bounds.Width, (float)BlockSize.Height * Source.CoverBlocks);
            }
           
            if (Source. CompleteScroll)
            {
                Font EntryFont = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
                Font GameOverFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);

                var measured = g.MeasureString(Source.GameOverText, GameOverFont);
                var measuremini = g.MeasureString(Source.GameOverText, EntryFont);
                PointF GameOverPos = new PointF(Bounds.Width / 2 - measured.Width / 2, measured.Height / 4);
                g.DrawString(Source.GameOverText, GameOverFont, Brushes.White, 5 + GameOverPos.X, 5 + GameOverPos.Y);
                g.DrawString(Source.GameOverText, GameOverFont, Brushes.Black, GameOverPos.X, GameOverPos.Y);

                //draw each "line" of summary statistical information based on ShowExtraLines.

                for (int i = 0; i < Source.ShowExtraLines; i++)
                {
                    float XPosition = Bounds.Width * 0.25f;
                    float YPosition = GameOverPos.Y + ((1 + i) * measuremini.Height) + measuremini.Height;

                    if (i == 0)
                    {
                        var measuredmini = g.MeasureString("---Line Clears---", EntryFont);
                        TetrisGame.DrawText(g, EntryFont, "---Line Clears---", Brushes.Black, Brushes.White, Bounds.Width / 2 - measuredmini.Width / 2, GameOverPos.Y + measured.Height);
                    }
                    
                    if (i == 1) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_I), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 2) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_O), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 3) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_T), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 4) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_J), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 5) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_L), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 6) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_S), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                    if (i == 7) DrawTetrominoStat(Source, typeof(Tetrominoes.Tetromino_Z), new PointF(XPosition, YPosition), g, Bounds, EntryFont);
                }

                if (Source.NewScorePosition > -1)
                {
                    Font HighScoreEntryFont = new Font(GameOverFont.FontFamily, (float)(8 * pOwner.ScaleFactor), FontStyle.Regular);
                    //draw the awarded score position as well.
                    float XPosition = Bounds.Width * .25f;
                    float YPosition = Bounds.Height - measured.Height - 10;
                    String ScoreText = "New High Score!";
                    var MeasuredScoreText = g.MeasureString(ScoreText, HighScoreEntryFont);
                    using (Brush RainbowBrush = new SolidBrush(TetrisGame.GetRainbowColor(Color.Lime, 0.1d)))
                    {
                        TetrisGame.DrawText(g, GameOverFont, ScoreText, Brushes.Black, RainbowBrush, Bounds.Width / 2 - MeasuredScoreText.Width / 2, YPosition + measuremini.Height);
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, GameOverGameState Source, GameStateDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source.GameOveredState,Element);
        }
    }
    public class FieldActionStateRenderingHandler :StandardStateRenderingHandler<Graphics,FieldActionGameState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {
            if (Source._BaseState != null)
                RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,Source._BaseState,Element);
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {  if(Source._BaseState!=null)
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._BaseState,Element);
        }
    }
    public class StandardTetrisGameStateRenderingHandler : StandardStateRenderingHandler<Graphics, StandardTetrisGameState, GameStateDrawParameters>
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
            if (TetrominoImages == null || RedrawsNeeded) Source.RedrawStatusbarTetrominoBitmaps(pOwner, Bounds);

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
    public class EnterTextStateRenderingHandler : StandardStateRenderingHandler<Graphics, EnterTextState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, EnterTextState Source, GameStateDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.useFont == null) Source.useFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);
            if (Source.EntryFont == null) Source.EntryFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);

            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            int RotateAmount = (int)(Millipercent * 240);

            Color UseBackgroundColor = HSLColor.RotateHue(Color.DarkBlue, RotateAmount);
            Color UseHighLightingColor = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color useLightRain = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            //throw new NotImplementedException();
            Source._BG.DrawProc(g, Bounds);
            int StartYPosition = (int)(Bounds.Height * 0.15f);
            var MeasureBounds = g.MeasureString(Source.EntryPrompt[0], Source.useFont);
            for (int i = 0; i < Source.EntryPrompt.Length; i++)
            {
                //draw this line centered at StartYPosition+Height*i...

                int useYPosition = (int)(StartYPosition + (MeasureBounds.Height + 5) * i);
                int useXPosition = (int)(Bounds.Width / 2 - MeasureBounds.Width / 2);
                g.DrawString(Source.EntryPrompt[i], Source.useFont, Brushes.Black, new PointF(useXPosition + 5, useYPosition + 5));
                g.DrawString(Source.EntryPrompt[i], Source.useFont, new SolidBrush(useLightRain), new PointF(useXPosition, useYPosition));
            }

            float nameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (Source.EntryPrompt.Length + 1);


            var AllCharacterBounds = (from c in Source.NameEntered.ToString().ToCharArray() select g.MeasureString(c.ToString(), Source.useFont)).ToArray();
            float useCharWidth = g.MeasureString("_", Source.EntryFont).Width;
            float TotalWidth;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;

            }
            TotalWidth = (useCharWidth + 5) * Source.NameEntered.Length;
            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Preblank)
            {
                for (int charpos = 0; charpos < Source.NameEntered.Length; charpos++)
                {
                    char thischar = Source.NameEntered[charpos];
                    float useX = NameEntryX + ((useCharWidth + 5) * (charpos));
                    Brush DisplayBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(UseHighLightingColor) : Brushes.NavajoWhite;
                    Brush ShadowBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(useLightRain) : Brushes.Black;
                    g.DrawString(thischar.ToString(), Source.EntryFont, ShadowBrush, new PointF(useX + 2, nameEntryY + 2));
                    g.DrawString(thischar.ToString(), Source.EntryFont, DisplayBrush, new PointF(useX, nameEntryY));
                }
            }
            else if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                //"simpler"- we just draw the trimmed text.
                String TrimEntered = Source.NameEntered.ToString().Trim(' ', '_');



            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, EnterTextState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
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
