using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates
{
    public class ViewScoreDetailsState : GameState
    {
        public override DisplayMode SupportedDisplayMode
        {
            get { return DisplayMode.Full; }
        }

        public ShowHighScoresState _Owner = null;
        public IHighScoreEntry ShowEntry = null;

        public enum ViewScoreDetailsType
        {
            Details_Tetrominoes,
            Details_LevelTimes
        }

        
        public ViewScoreDetailsType CurrentView = ViewScoreDetailsType.Details_Tetrominoes;
        public int _Position;

        public ViewScoreDetailsState(ShowHighScoresState pOwner, IHighScoreEntry pShowEntry, IBackground useBG, int DetailPosition)
        {
            _Position = DetailPosition;
            _BG = useBG; //so it is the same as the "main" show score state and looks "seamless".
            _Owner = pOwner;
            ShowEntry = pShowEntry;
        }

     

        public override void GameProc(IStateOwner pOwner)
        {
            
            //For flair we'll have some gubbins or whatever in the background.
            //throw new NotImplementedException();
        }

        public String _DetailHeader = "---SCORE DETAILS---";

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            switch (g)
            {
                case GameKeys.GameKey_Left:
                    break;
                case GameKeys.GameKey_Right:
                    break;
                case GameKeys.GameKey_RotateCCW:
                    pOwner.CurrentState = _Owner;
                    break;
            }
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
        }
    }
}