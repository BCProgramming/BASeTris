using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates
{

    //Displays the current High score listing.
    //This will animate the display by drawing them one by one.
    //Scores can also be highlighted.

    public class ShowHighScoresState : GameState
    {
        Pen LinePen = new Pen(new LinearGradientBrush(new Rectangle(0, 0, 5, 25), Color.Black, Color.DarkGray, LinearGradientMode.Vertical), 25);
        public GameState RevertState = null; //if set, pressing the appropriate key will set the state back to this one. An integrated 'Menu' state could benefit from this- get rid of the Windows Menu bar and make it more gamey?)
        private int[] HighlightedScorePositions = new int[] { };
        private IHighScoreList _ScoreList = null;
        private String PointerText = "►";
        private int SelectedScorePosition = 0;
        private bool ScrollCompleted = false;
        DateTime LastIncrementTime = DateTime.MinValue;
        TimeSpan IncrementTimediff = new TimeSpan(0,0,0,0,300);
        private String HeaderText = "HIGH SCORES";
        List<IHighScoreEntry> hs = null;
        Font ScoreFont = null;
        int IncrementedDrawState = -1;

        //the increment Draw State goes from 0, where the screen hasn't had any additional foreground information drawn, to the size of the high score list + 2.
        //"The added two lines are HIGH SCORES and the header line, which are also drawn".
        
        public override GameState.DisplayMode SupportedDisplayMode {  get { return GameState.DisplayMode.Full; } }
        private IBackgroundDraw _BG = null;
        public ShowHighScoresState(IHighScoreList ScoreList,GameState ReversionState = null,int[] HighlightPositions = null)
        {
            
            _ScoreList = ScoreList;
            hs = _ScoreList.GetScores().ToList();
            HighlightedScorePositions = HighlightPositions??new int[] { };
            SelectedScorePosition = HighlightPositions == null || HighlightPositions.Length == 0 ? 1 : HighlightPositions.First()-1;
            RevertState = ReversionState;
            ImageAttributes useBGAttributes = new ImageAttributes();
            useBGAttributes.SetColorMatrix(ColorMatrices.GetFader(0.25f));
            var sib = new StandardImageBackgroundDraw(TetrisGame.StandardTiledTetrisBackground, useBGAttributes);
            double xpoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            sib.Movement = new PointF((float)xpoint,(float)ypoint);
            _BG = sib;
            
            
        }
        //This state Draws the High scores.
        //Note that this state "takes over" the full display- it doesn't use an underlying Standard State to handle drawing aspects like the Status bar.
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
        
        private void DrawBackground(IStateOwner pOwner,Graphics g,RectangleF Bounds)
        {
           
            //ColorMatrices.GetFader(1.0f - ((float)i * 0.1f))
            g.Clear(Color.White);
          
            
            _BG.DrawProc(g,Bounds);
        }
        private Brush GetHighlightBrush()
        {
            return DateTime.Now.Millisecond < 500 ? Brushes.Lime:Brushes.Blue;
        }
        
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            ;
            float StartY = Bounds.Height * 0.175f;
            float MiddleX = Bounds.Width / 2;
            DrawBackground(pOwner,g, Bounds);
            float TextSize = Bounds.Height / 30f;
            using (ScoreFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor,FontStyle.Bold,GraphicsUnit.Pixel))
            {
                float LineHeight = g.MeasureString("#", ScoreFont).Height + 5;
                //This needs to change based on the actual gameplay area size.)
                if (IncrementedDrawState >= 0)
                {
                    //Draw HIGH SCORES
                    var Measured = g.MeasureString(HeaderText, ScoreFont);
                    PointF DrawPosition = new PointF(MiddleX - (Measured.Width / 2), StartY);
                    g.DrawString(HeaderText, ScoreFont, Brushes.White, new PointF(DrawPosition.X + 2, DrawPosition.Y + 2));
                    g.DrawString(HeaderText, ScoreFont, Brushes.Black, DrawPosition);

                }
                if (IncrementedDrawState >= 1)
                {
                    float LineYPosition = StartY + LineHeight;

                    //draw a line underneath the High scores text
                    g.DrawLine(LinePen, 20, LineYPosition, Bounds.Width - 20, LineYPosition);
                }

                if (IncrementedDrawState >= 2)
                {
                    //draw the high score listing entries.
                    //iterate from 2 to drawstate and draw the high score at position drawstate-2.
                    for (int scoreiterate = 2; scoreiterate < IncrementedDrawState; scoreiterate++)
                    {
                        int CurrentScoreIndex = scoreiterate - 2;
                        int CurrentScorePosition = CurrentScoreIndex + 1;
                        float useYPosition = StartY + (LineHeight * 2.5f) + LineHeight * CurrentScoreIndex;
                        float UseXPosition = Bounds.Width * 0.19f;
                        String sUseName = "N/A";
                        int sUseScore = 0;
                        IHighScoreEntry currentScore = hs.Count  > CurrentScoreIndex ? hs[CurrentScoreIndex] : null;
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
                        Brush DrawScoreBrush = HighlightedScorePositions.Contains(CurrentScorePosition) ? GetHighlightBrush() : Brushes.Gray;

                        g.DrawString(CurrentScorePosition.ToString(), ScoreFont, Brushes.Black, PosXPosition + 2, useYPosition + 2);
                        g.DrawString(CurrentScorePosition.ToString(), ScoreFont, DrawScoreBrush, PosXPosition, useYPosition);

                        g.DrawString(sUseName, ScoreFont, Brushes.Black, NameXPosition + 2, useYPosition + 2);
                        g.DrawString(sUseName, ScoreFont, DrawScoreBrush, NameXPosition, useYPosition);

                        float ScoreXPosition = ScoreXPositionRight - MeasureScore.Width;

                        g.DrawString(sUseScore.ToString(), ScoreFont, Brushes.Black, ScoreXPosition + 2, useYPosition + 2);
                        g.DrawString(sUseScore.ToString(), ScoreFont, DrawScoreBrush, ScoreXPosition, useYPosition);

                        g.DrawLine(new Pen(DrawScoreBrush, 3), NameXPosition + MeasureName.Width + 15, useYPosition + LineHeight / 2, ScoreXPosition - 15, useYPosition + LineHeight / 2);

                        if(SelectedScorePosition==CurrentScoreIndex)
                        {
                            //draw the selection arrow to the left of the NamePosition and useYPosition.
                            var MeasureArrow = g.MeasureString(PointerText, ScoreFont);
                            float ArrowX = PosXPosition - MeasureArrow.Width - 5;
                            float ArrowY = useYPosition;
                            g.DrawString(PointerText,ScoreFont,Brushes.Black,ArrowX+2,ArrowY+2);
                            g.DrawString(PointerText, ScoreFont,DrawScoreBrush, ArrowX , ArrowY);
                        }
                        

                    }

                }
            }
        }

        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //not called at all. Or, it shouldn't be, anyway.
        }

        public override void GameProc(IStateOwner pOwner)
        {

            _BG.FrameProc();
            if(DateTime.Now-LastIncrementTime > IncrementTimediff  && !ScrollCompleted)
            {
                IncrementedDrawState++;
                LastIncrementTime = DateTime.Now;
                //Maybe twiddle the Timediff a bit? I dunno
                TetrisGame.Soundman.PlaySound("switch_inactive");
                if(IncrementedDrawState==_ScoreList.MaximumSize+2)
                {
                    ScrollCompleted = true;
                }
            }

        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            GameKeys[] handledKeys = new GameKeys[] { GameKeys.GameKey_Down, GameKeys.GameKey_Drop, GameKeys.GameKey_RotateCW };
            if (!ScrollCompleted) IncrementTimediff = new TimeSpan(0, 0, 0, 0, 50);

            else if (ScrollCompleted && handledKeys.Contains(g))
            {
                if (g == GameKeys.GameKey_Drop)
                {
                    //move up...
                    SelectedScorePosition--;
                    if (SelectedScorePosition < 0) SelectedScorePosition = _ScoreList.MaximumSize;
                }
                else if (g == GameKeys.GameKey_Down)
                {
                    SelectedScorePosition++;
                    if (SelectedScorePosition > _ScoreList.MaximumSize) SelectedScorePosition = 0;
                }
                else if (g == GameKeys.GameKey_RotateCW)
                {
                    //This is where we will enter a "HighscoreDetails" state passing along this one specific high score.
                    var SelectedScore = _ScoreList.GetScores().ToArray()[SelectedScorePosition];
                    ViewScoreDetailsState vsd = new ViewScoreDetailsState(this, SelectedScore, _BG, SelectedScorePosition+1);
                    pOwner.CurrentState = vsd;
                }
                
            }

            else if (RevertState != null) pOwner.CurrentState = RevertState;
        }
    }
}
