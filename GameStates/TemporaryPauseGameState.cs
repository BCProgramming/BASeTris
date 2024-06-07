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
        public delegate bool TemporaryInputGameKeyFilterFunction(IStateOwner pOwner, GameKeys key);
        public TemporaryInputGameKeyFilterFunction FilterFunction = DefaultFilter;
        private static bool DefaultFilter(IStateOwner pOwner, GameKeys key)
        {
            return false;
        }
        public static TemporaryInputGameKeyFilterFunction CreateKeyFilter(params GameKeys[] AllowedKeys)
        {

            return new TemporaryInputGameKeyFilterFunction((o,k)=> { return AllowedKeys.Contains(k); } );

        }
        public TemporaryInputPauseGameState(GameplayGameState pState,int pPauseTicks,Action<IStateOwner> pResumeFunc, TemporaryInputGameKeyFilterFunction KeyFilter = null)
        {
            FilterFunction = KeyFilter;
            PauseTicks = pPauseTicks;
            PausedState = pState;
            ResumeFunc = pResumeFunc;
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
            if (FilterFunction == null) return;
            if (FilterFunction(pOwner, g))
                PausedState.HandleGameKey(pOwner, g);
            //since we are "paused"
        }

        public GameplayGameState GetComposite()
        {
            return PausedState;
        }
    }
}
