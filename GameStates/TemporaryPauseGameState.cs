using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{

    public class TemporaryInputPauseGameState : GameState, ICompositeState<GameplayGameState>
    {
        public GameplayGameState PausedState = null;
        private int PauseTicks = 0;
        Action<IStateOwner> ResumeFunc = null;
        public TemporaryInputPauseGameState(GameplayGameState pState,int pPauseTicks,Action<IStateOwner> pResumeFunc)
        {
            PauseTicks = pPauseTicks;
            PausedState = pState;
            ResumeFunc = pResumeFunc;
        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            PausedState.DrawForegroundEffect(pOwner, g, Bounds);
        }

        uint FirstTick = 0;
        public void Resume(IStateOwner pOwner)
        {
            if(ResumeFunc!=null)
            {
                ResumeFunc(pOwner);
            }
            else
            {
                pOwner.CurrentState = PausedState;
            }
        }
        
        public override void GameProc(IStateOwner pOwner)
        {
            if (FirstTick == 0) FirstTick = TetrisGame.GetTickCount();
            PausedState.GameProc(pOwner);
            if (((TetrisGame.GetTickCount()-FirstTick) > PauseTicks))
            {
                Resume(pOwner);
            }
            
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //since we are "paused"
        }

        public GameplayGameState GetComposite()
        {
            return PausedState;
        }
    }
}
