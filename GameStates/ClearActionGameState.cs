using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;
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
        //we don't call the main State's GameProc here. We operate on the data (the field contents) to "clear" the given information but we operate on it separate from
        //the standard game state. For example we replace blocks with "intermediate" forms, or remove them altogether, then when finished return control and allow the game to continue.
        //The general approach ought to be to set this state, then enqueue a frameaction to be run for the next frame that "performs" the actual action, such as clearing rows or whatever.

        public override void GameProc(IStateOwner pOwner)
        {
            if(_BaseState is StandardTetrisGameState)
            {
                _BaseState.FrameUpdate();
            }
        }
        
          
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
    //standard "line clear" which clears out a given set of line rows via animation, then calls a set of actions to perform afterwards.
    //This will use a "Tetris flash" when 4 or more rows are being cleared- this is a white overlay that is placed over top (well, for now- might be a gradient or some kind of pattern brush later!)

    public class ClearLineActionGameState : ClearActionGameState
    {
        public enum LineClearStyle
        {
            LineClear_Left_To_Right,
            LineClear_Right_To_Left,
            LineClear_Middle_Out,
            LineClear_Outside_In

        }
        private LineClearStyle _ClearStyle = LineClearStyle.LineClear_Middle_Out;
        public LineClearStyle ClearStyle {  get { return _ClearStyle; } set { _ClearStyle = value; } }
        private int[] RowNumbers = null;
        
        private IEnumerable<Action> AfterClear = Enumerable.Empty<Action>();
        protected bool FlashState = false;
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
            base.GameProc(pOwner);
            if(StartOperation == DateTime.MaxValue)
            {
                StartOperation = LastOperation = DateTime.Now;
            }

            //for now we clear horizontally across...
            if((DateTime.Now - LastOperation).TotalMilliseconds> MSBlockClearTime)
            {
                //clear another block on each row.
                var ClearResult = ClearFrame();
                if(RowNumbers.Length>=4)
                {
                    FlashState = !FlashState;
                }
                LastOperation = DateTime.Now;
              
                if(ClearResult)
                {
                    //reset the original State.
                    pOwner.CurrentState = _BaseState;
                    foreach(var iterate in AfterClear)
                    {
                        pOwner.EnqueueAction(iterate);
                    }
                }
         
            }
        }
        //The actual "Frame" operation. This implementation clears one block and returns true when it clears all the blocks on each line.
        //derived classes can override pretty much just this one to clear the lines in different ways.        
        protected virtual bool ClearFrame()
        {
            switch (_ClearStyle)
            {
                case LineClearStyle.LineClear_Left_To_Right:
                    foreach (int rowClear in RowNumbers)
                    {
                        var GrabRow = _BaseState.PlayField.Contents[rowClear];
                        //find the block, and clear it out if needed.
                        var FindClear = CurrentClearIndex >= GrabRow.Length ? null : GrabRow[CurrentClearIndex];
                        if (FindClear != null)
                        {
                            GrabRow[CurrentClearIndex] = null;
                        }


                    }
                    CurrentClearIndex++;
                    _BaseState.PlayField.HasChanged = true;
                    return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                case LineClearStyle.LineClear_Right_To_Left:
                    foreach (int rowClear in RowNumbers)
                    {
                        var GrabRow = _BaseState.PlayField.Contents[rowClear];
                        int useIndex = GrabRow.Length - CurrentClearIndex-1;
                        //find the block, and clear it out if needed.
                        var FindClear = CurrentClearIndex >= GrabRow.Length ? null : GrabRow[useIndex];
                        if (FindClear != null)
                        {
                            GrabRow[useIndex] = null;
                        }
                    }
                    CurrentClearIndex++;
                    _BaseState.PlayField.HasChanged = true;
                    return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                case LineClearStyle.LineClear_Middle_Out:
                    //middle out
                    foreach (int rowClear in RowNumbers)
                    {
                        var GrabRow = _BaseState.PlayField.Contents[rowClear];
                        
                            int i = GrabRow.Length>>1;
                            int useindex =i+ ((CurrentClearIndex % 2 == 0) ? CurrentClearIndex / 2 : -(CurrentClearIndex / 2 + 1));
                            if (useindex < GrabRow.Length && useindex >= 0)
                                GrabRow[useindex] = null;
                    }
                    CurrentClearIndex++;
                    _BaseState.PlayField.HasChanged = true;
                    return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                case LineClearStyle.LineClear_Outside_In:
                {
                    
                    //middle out
                    foreach (int rowClear in RowNumbers)
                    {
                        
                            var GrabRow = _BaseState.PlayField.Contents[rowClear];
                            int processindex = GrabRow.Length - CurrentClearIndex;
                        int i = GrabRow.Length >> 1;
                        int useindex = i + ((processindex % 2 == 0) ? processindex / 2 : -(processindex / 2 + 1));
                        if (useindex < GrabRow.Length && useindex >= 0)
                            GrabRow[useindex] = null;
                    }
                    CurrentClearIndex++;
                    _BaseState.PlayField.HasChanged = true;
                    return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;

                }

            }
            return true;
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
    public class ClearLineActionDissolve: ClearLineActionGameState
    {
        Queue<Point> ClearBlockList = null;
        int RowClearCount = 0;
        public ClearLineActionDissolve(StandardTetrisGameState _BaseState, int[] ClearRows, IEnumerable<Action> pAfterClearActions) : base(_BaseState, ClearRows, pAfterClearActions)
        {
            List<Point> AllBlockPositions = new List<Point>();
            RowClearCount = ClearRows.Length;
            foreach(int grabrow in ClearRows)
            {
                var rowcontent = _BaseState.PlayField.Contents[grabrow];
                for (int addblock=0;addblock<rowcontent.Length;addblock++)
                {
                    if(rowcontent[addblock]!=null)
                    {
                        AllBlockPositions.Add(new Point(grabrow,addblock));
                    }
                }
            }
            ClearBlockList = new Queue<Point>();
            foreach(var choosernd in BagChooser.Shuffle(new Random(),AllBlockPositions))
            {
                ClearBlockList.Enqueue(choosernd);
            }
            

        }
        protected override bool ClearFrame()
        {
            for (int clearblock = 0; clearblock < RowClearCount; clearblock++)
            {
                if (ClearBlockList.Count == 0) return true;
                var grabnext = ClearBlockList.Dequeue();
                _BaseState.PlayField.Contents[grabnext.X][grabnext.Y] = null;
                _BaseState.PlayField.HasChanged = true;
            }
            
            return false;


        }
    }
    public class InsertBlockRowsActionGameState:ClearActionGameState
    {


        private IEnumerable<Action> AfterClear = Enumerable.Empty<Action>();
        private Queue<TetrisBlock[]> RowInsertions = null;
        //private TetrisBlock[][] InsertionData = null;
        private int InsertionRow;
        
        
        public InsertBlockRowsActionGameState(StandardTetrisGameState pBaseState, int InsertRow,TetrisBlock[][] RowData, IEnumerable<Action> pAfterClearActions) : base(pBaseState)
        {
            RowInsertions = new Queue<TetrisBlock[]>();
            foreach(TetrisBlock[] AddRow in RowData)
            {
                RowInsertions.Enqueue(AddRow);
            }
            InsertionRow = InsertRow;
            AfterClear = pAfterClearActions;
        }

        int MSLineAddTime = 30; //number of ms between blocks being removed/cleared.
        int CurrentClearIndex = 0;
        int InsertedCount = 0;
        DateTime StartOperation = DateTime.MaxValue;
        DateTime LastOperation = DateTime.MaxValue;
        public override void GameProc(IStateOwner pOwner)
        {
            base.GameProc(pOwner);
            if (StartOperation == DateTime.MaxValue)
            {
                StartOperation = LastOperation = DateTime.Now;
            }

            //for now we clear horizontally across...
            if ((DateTime.Now - LastOperation).TotalMilliseconds > MSLineAddTime)
            {
                int InsertionIndex = _BaseState.PlayField.Contents.Length - 1 - InsertionRow-InsertedCount; //since high rows have lower indices, we want to "reverse" it.
                //if the queue is empty, we are now finished.
                if(RowInsertions.Count==0)
                {
                    //reset the original State.
                    _BaseState.PlayField.HasChanged = true;

                    pOwner.CurrentState = _BaseState;
                    foreach (var iterate in AfterClear)
                    {
                        pOwner.EnqueueAction(iterate);
                    }
                }
                else
                {
                    //dequeue the next line of blocks to insert.
                    TetrisBlock[] NextRow = RowInsertions.Dequeue();

                    //insert into the playfield at InsertionIndex.
                    //This means moving All rows from InsertionIndex up one.
                    for(int moverow=0;moverow<InsertionIndex;moverow++)
                    {
                        TetrisBlock[] ThisRow = _BaseState.PlayField.Contents[moverow+1];
                        TetrisBlock[] TargetRow = _BaseState.PlayField.Contents[moverow];

                        for(int copyCol =0;copyCol<ThisRow.Length;copyCol++)
                        {
                            TargetRow[copyCol] = ThisRow[copyCol];
                        }
                    }

                    TetrisBlock[] InsertedRow = _BaseState.PlayField.Contents[InsertionIndex];
                    for(int i=0;i<InsertedRow.Length;i++)
                    {
                        InsertedRow[i] = NextRow[i % NextRow.Length];
                    }
                    InsertedCount++;
                    LastOperation = DateTime.Now;
                }
                


           
                //another way of doing this: we can just use DrawProc and draw the background over top, I suppose?
            }
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
    }
}
