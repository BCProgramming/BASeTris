using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;

namespace BASeTris.GameStates
{
    public class UnpauseDelayGameState : GameState
    {
        public GameState _ReturnState = null;
        DateTime InitialUnpauseTime = DateTime.MinValue;
        TimeSpan PauseDelay = new TimeSpan(0, 0, 0, 5);
        Action ReturnFunc = null;
        public override bool GamePlayActive { get { return false; } }
        public UnpauseDelayGameState(GameState ReturnToState, Action pReturnFunc = null)
        {
            _ReturnState = ReturnToState;
            ReturnFunc = pReturnFunc;
        }

     
        public int LastSecond;
        public TimeSpan timeremaining = TimeSpan.Zero;

        public override void GameProc(IStateOwner pOwner)
        {
            //don't call GameProc of original State
            if (InitialUnpauseTime == DateTime.MinValue)
            {
                InitialUnpauseTime = DateTime.Now;
                LastSecond = PauseDelay.Seconds;
            }


            timeremaining = PauseDelay - (DateTime.Now - InitialUnpauseTime);
            if (timeremaining.Ticks < 0)
            {
                ReturnFunc?.Invoke();
                pOwner.CurrentState = _ReturnState;
                return;
            }
        }

        public double lastMillis = 1000;

        
        Brush fadeBrush = new SolidBrush(Color.FromArgb(200, Color.Black));

        private void DrawFadeOverlay(Graphics g, RectangleF Bounds)
        {
            g.FillRectangle(fadeBrush, Bounds);
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
        }

      
    }
}