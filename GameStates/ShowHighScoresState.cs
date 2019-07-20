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
        
        public GameState RevertState = null; //if set, pressing the appropriate key will set the state back to this one. An integrated 'Menu' state could benefit from this- get rid of the Windows Menu bar and make it more gamey?)
        public int[] HighlightedScorePositions = new int[] { };
        public IHighScoreList _ScoreList = null;
        
        public int SelectedScorePosition = 0;
        private bool ScrollCompleted = false;
        DateTime LastIncrementTime = DateTime.MinValue;
        TimeSpan IncrementTimediff = new TimeSpan(0, 0, 0, 0, 300);
        public String HeaderText = "HIGH SCORES";
        public List<IHighScoreEntry> hs = null;
        public int IncrementedDrawState = -1;

        //the increment Draw State goes from 0, where the screen hasn't had any additional foreground information drawn, to the size of the high score list + 2.
        //"The added two lines are HIGH SCORES and the header line, which are also drawn".

        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        

        public ShowHighScoresState(IHighScoreList ScoreList, GameState ReversionState = null, int[] HighlightPositions = null)
        {
            _ScoreList = ScoreList;
            hs = _ScoreList.GetScores().ToList();
            HighlightedScorePositions = HighlightPositions ?? new int[] { };
            SelectedScorePosition = HighlightPositions == null || HighlightPositions.Length == 0 ? 1 : HighlightPositions.First() - 1;
            RevertState = ReversionState;
            double xpoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            double ypoint = 1 + TetrisGame.rgen.NextDouble() * 2;
            var sib = StandardImageBackgroundGDI.GetStandardBackgroundDrawer(new PointF((float)xpoint, (float)ypoint));
            _BG = sib;
        }

        //This state Draws the High scores.
        //Note that this state "takes over" the full display- it doesn't use an underlying Standard State to handle drawing aspects like the Status bar.
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

     

    

        public override void GameProc(IStateOwner pOwner)
        {
            
            if (DateTime.Now - LastIncrementTime > IncrementTimediff && !ScrollCompleted)
            {
                IncrementedDrawState++;
                LastIncrementTime = DateTime.Now;
                //Maybe twiddle the Timediff a bit? I dunno
                TetrisGame.Soundman.PlaySound("switch_inactive", pOwner.Settings.EffectVolume);
                if (IncrementedDrawState == _ScoreList.MaximumSize + 2)
                {
                    ScrollCompleted = true;
                }
            }
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            GameKeys[] handledKeys = new GameKeys[] {GameKeys.GameKey_Down, GameKeys.GameKey_Drop, GameKeys.GameKey_RotateCW};
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
                    ViewScoreDetailsState vsd = new ViewScoreDetailsState(this, SelectedScore, _BG, SelectedScorePosition + 1);
                    pOwner.CurrentState = vsd;
                }
            }

            else if (RevertState != null) pOwner.CurrentState = RevertState;
        }
    }
}