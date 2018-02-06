using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.TetrisBlocks;

namespace BASeTris.GameStates
{
    //operates solely on a StandardTetrisGameState (or derived class- really anything with an appropriate TetrisField.
    //This is itself a base class for "Clear" actions. This would include actions like clearing a line in Tetris, clearing a set of blocks in Dr Mario or Tetris 2, etc.
    //also this could be used to ADD stuff to the field- so it's not strictly for CLEAR but "ActionGameState" seemed a bit silly as a name.
    public abstract class ClearActionGameState : GameState
    {
        protected StandardTetrisGameState _BaseState;

        public ClearActionGameState(StandardTetrisGameState pBaseState)
        {
            _BaseState = pBaseState;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _BaseState.DrawStats(pOwner,g,Bounds);
        }

        public abstract override void GameProc(IStateOwner pOwner);
        
            //we don't call the main State's GameProc here. We operate on the data (the field contents) to "clear" the given information but we operate on it separate from
            //the standard game state. For example we replace blocks with "intermediate" forms, or remove them altogether, then when finished return control and allow the game to continue.
            //The general approach ought to be to set this state, then enqueue a frameaction to be run for the next frame that "performs" the actual action, such as clearing rows or whatever.

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _BaseState.DrawProc(pOwner,g,Bounds);
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //Should we allow Game keys here? probably. We'll block them for the moment though. There shouldn't be an active BlockGroup during this process
            //so really there shouldn't be anything to control- Aside, of course, from pause for example.
            //throw new NotImplementedException();
        }
    }

    public class ClearLineActionGameState : ClearActionGameState
    {
        private int[] RowNumbers = null;
        private IEnumerable<Action> AfterClear = Enumerable.Empty<Action>();
        bool FlashState = false;
        public ClearLineActionGameState(StandardTetrisGameState _BaseState,int[] ClearRows,IEnumerable<Action> pAfterClearActions):base(_BaseState)
        {
            AfterClear = pAfterClearActions;
            RowNumbers = ClearRows;

            

        }
        int MSBlockClearTime = 20; //number of ms between blocks being removed/cleared.
        int CurrentClearIndex = 0;
        DateTime StartOperation = DateTime.MaxValue;
        DateTime LastOperation = DateTime.MaxValue;
        public override void GameProc(IStateOwner pOwner)
        {
            if(StartOperation == DateTime.MaxValue)
            {
                StartOperation = LastOperation = DateTime.Now;
            }

            //for now we clear horizontally across...
            if((DateTime.Now - LastOperation).TotalMilliseconds> MSBlockClearTime)
            {
                //clear another block on each row.
                foreach(int rowClear in RowNumbers)
                {
                    var GrabRow = _BaseState.PlayField.Contents[rowClear];
                    //find the block, and clear it out if needed.
                    var FindClear = CurrentClearIndex >= GrabRow.Length?null:GrabRow[CurrentClearIndex];
                    if(FindClear!=null)
                    {
                        GrabRow[CurrentClearIndex] = null;
                    }
                    
                    
                }
                if(RowNumbers.Length>=4)
                {
                    FlashState = !FlashState;
                }
                LastOperation = DateTime.Now;
                CurrentClearIndex++;
                _BaseState.PlayField.HasChanged = true;
                if(CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length+2)
                {
                    //reset the original State.
                    pOwner.CurrentState = _BaseState;
                    foreach(var iterate in AfterClear)
                    {
                        pOwner.EnqueueAction(iterate);
                    }
                }
                //another way of doing this: we can just use DrawProc and draw the background over top, I suppose?
            }
        }
        SolidBrush FlashBrush = new SolidBrush(Color.FromArgb(128,Color.White));

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (FlashState)
            {
                g.FillRectangle(FlashBrush, Bounds);
            }
        }

        
    }
}
