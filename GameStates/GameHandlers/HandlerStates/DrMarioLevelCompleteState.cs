using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerStates
{
    
    //State which is used to display the "level complete" indication when a level completes in Dr.Mario.
    //
    public class DrMarioLevelCompleteState : GameState, ICompositeState<GameplayGameState>
    {
        public String LevelCompleteMusic { get; set; } = "drm_complete";
        private GameplayGameState OriginalState = null;
        Func<GameState> StateProcessionFunction = null;
        public DrMarioLevelCompleteState(GameplayGameState pState,Func<GameState> AdvanceToStateFunc)
        {
            StateProcessionFunction = AdvanceToStateFunc;
            OriginalState = pState;
        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
        private AssetManager.iActiveSoundObject CompletionMusic = null;
        public override void GameProc(IStateOwner pOwner)
        {
            if (CompletionMusic == null)
                //start the victory music... or whatever music we are told to I suppose.
                CompletionMusic = OriginalState.Sounds.PlayMusic(LevelCompleteMusic, 3.0f, false);

        }

        public GameplayGameState GetComposite()
        {
            return OriginalState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if(new GameKeys[] { GameKeys.GameKey_RotateCW,GameKeys.GameKey_RotateCCW}.Contains(g))
            {
                //proceed with procession function.
                var NextState = StateProcessionFunction();
                if (NextState == null) throw new NullReferenceException("DrMarioLevelCompleteState: StateProcessionFunction returned null.");
                if(NextState is GameplayGameState ggst)
                {
                    ggst.FirstRun = false;
                    //ggst.FieldPrepared = true;
                }
                else if(NextState is ICompositeState<GameplayGameState> ggc)
                {
                    ggc.GetComposite().FirstRun = false;
                    //ggc.GetComposite().FieldPrepared = true;
                }
                pOwner.CurrentState = NextState;
                //play the default music...
                
            }
        }
    }
}
