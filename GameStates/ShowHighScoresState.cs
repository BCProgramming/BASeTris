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
        
        public GameState RevertState = null; 
        public int[] HighlightedScorePositions = new int[] { };
        private int[] InitialHightedPositions = null;
        public IHighScoreList _ScoreList = null;
        private IHighScoreList _InitialList = null;
        public int SelectedScorePosition = 0;
        private bool ScrollCompleted = false;
        DateTime LastIncrementTime = DateTime.MinValue;
        TimeSpan IncrementTimediff = new TimeSpan(0, 0, 0, 0, 300);
        public String HeaderText = "HIGH SCORES";
        public List<IHighScoreEntry> hs = null;
        public int IncrementedDrawState = -1;
        public event EventHandler<EventArgs> BeforeRevertState;

        private int ScoreSetIndex = 0;
        private String[] ScoreKeys = null;


        //the increment Draw State goes from 0, where the screen hasn't had any additional foreground information drawn, to the size of the high score list + 2.
        //"The added two lines are HIGH SCORES and the header line, which are also drawn".

        public String GetDisplayingSet()
        {
            return ScoreKeys[ScoreSetIndex];
        }
        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        private void UpdateScoreSet()
        {
            _ScoreList = TetrisGame.ScoreMan[ScoreKeys[ScoreSetIndex]];
            hs = _ScoreList.GetScores().ToList();
            if (_InitialList == _ScoreList)
            {
                HighlightedScorePositions = InitialHightedPositions;

            }
            else
            {
                HighlightedScorePositions = new int[] { };
            }

        }

        public ShowHighScoresState(IHighScoreList ScoreList, GameState ReversionState = null, int[] HighlightPositions = null)
        {
            _InitialList = ScoreList;
            _ScoreList = ScoreList;
            hs = _ScoreList.GetScores().ToList();
            HighlightedScorePositions = HighlightPositions ?? new int[] { };
            InitialHightedPositions = HighlightedScorePositions;
            SelectedScorePosition = HighlightPositions == null || HighlightPositions.Length == 0 ? 1 : HighlightPositions.First() - 1;
            RevertState = ReversionState;


            ScoreKeys = TetrisGame.ScoreMan.GetKeys();

            for (int i = 0; i < ScoreKeys.Length; i++)
            {
                if (TetrisGame.ScoreMan[ScoreKeys[i]] == ScoreList)
                {
                    ScoreSetIndex = i;
                    break;
                }

            }



        }

      
     

    

        public override void GameProc(IStateOwner pOwner)
        {
            
            if (DateTime.Now - LastIncrementTime > IncrementTimediff && !ScrollCompleted)
            {
                IncrementedDrawState++;
                LastIncrementTime = DateTime.Now;
                //Maybe twiddle the Timediff a bit? I dunno
                TetrisGame.Soundman.PlaySound("switch_inactive", pOwner.Settings.std.EffectVolume);
                if (IncrementedDrawState == _ScoreList.MaximumSize + 2)
                {
                    ScrollCompleted = true;
                }
            }
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            GameKeys[] handledKeys = new GameKeys[] {GameKeys.GameKey_Down, GameKeys.GameKey_Drop, GameKeys.GameKey_RotateCW,GameKeys.GameKey_Left,GameKeys.GameKey_Right};
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
                else if (g == GameKeys.GameKey_Left)
                {
                    ScoreSetIndex = MathHelper.mod((ScoreSetIndex - 1), ScoreKeys.Length);
                    UpdateScoreSet();
                }
                else if (g == GameKeys.GameKey_Right)
                {
                    ScoreSetIndex = MathHelper.mod((ScoreSetIndex - 1), ScoreKeys.Length);
                    UpdateScoreSet();
                }
            }

            else if (RevertState != null)
            {
                BeforeRevertState?.Invoke(this, new EventArgs());
                pOwner.CurrentState = RevertState;
            }
        }
    }
}