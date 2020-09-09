using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerStates
{
    public class ViriiAppearanceState : GameState,ICompositeState<GameplayGameState>
    {
        private GameplayGameState StandardState = null;
        Queue<Blocks.LineSeriesMasterBlock> AppearanceBlocks = null;
        uint LastAppearanceTick = 0;
        uint AppearanceTimeDifference = 500; //aiming for 50ms here
        public ViriiAppearanceState(GameplayGameState startupState)
        {
            SortedList<Guid, Blocks.LineSeriesMasterBlock> appearanceshuffler = new SortedList<Guid, Blocks.LineSeriesMasterBlock>();
            StandardState = startupState;
            //set all Viruses to invisible and force a redraw.

            var field = StandardState.PlayField.Contents;
            for(int x=0;x<StandardState.PlayField.ColCount;x++)
            { 
                for(int y=0;y<StandardState.PlayField.RowCount;y++)
                {
                    var block = field[y][x];
                    if (block is Blocks.LineSeriesMasterBlock lsmb)
                    {
                        lsmb.Visible = false;
                        appearanceshuffler.Add(Guid.NewGuid(), lsmb);
                    }
                }

            }
            AppearanceBlocks = new Queue<Blocks.LineSeriesMasterBlock>(appearanceshuffler.Values);
            StandardState.PlayField.HasChanged = true; //since we made them all invisible I'd say that counts as a change!
            LastAppearanceTick = TetrisGame.GetTickCount();

        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
           // throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //if the timeout has elapsed, make another one visible.
            var currtick = TetrisGame.GetTickCount();
            var tickdiff = currtick - LastAppearanceTick;
            if(tickdiff > AppearanceTimeDifference)
            {
                if(AppearanceBlocks.Count == 0)
                {
                    pOwner.CurrentState = StandardState;
                    return;
                }
                var nextAppear = AppearanceBlocks.Dequeue();
                nextAppear.Visible = true;
                StandardState.PlayField.HasChanged = true;

            }
            
        }

        public GameplayGameState GetComposite()
        {
            return StandardState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //do nothing- we don't allow control here.
        }
    }
}
