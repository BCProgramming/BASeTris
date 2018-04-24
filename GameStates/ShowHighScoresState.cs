using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{

    //Displays the current High score listing.
    //This will animate the display by drawing them one by one.
    //Scores can also be highlighted.

    class ShowHighScoresState : GameState
    {
        public GameState RevertState = null; //if set, pressing the appropriate key will set the state back to this one. An integrated 'Menu' state could benefit from this- get rid of the Windows Menu bar and make it more gamey?)
        private int[] HighlightedScorePositions = new int[] { };
        public override GameState.DisplayMode SupportedDisplayMode {  get { return GameState.DisplayMode.Full; } }

        public ShowHighScoresState(GameState ReversionState = null,int[] HighlightPositions = null)
        {
            RevertState = ReversionState;
        }
        //This state Draws the High scores.
        //Note that this state "takes over" the full display- it doesn't use an underlying Standard State to handle drawing aspects like the Status bar.
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
            
            
        }

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {

            //throw new NotImplementedException();
        }

        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //not called at all. Or, it shouldn't be, anyway.
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (RevertState != null) pOwner.CurrentState = RevertState;
        }
    }
}
