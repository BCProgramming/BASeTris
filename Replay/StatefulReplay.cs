using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates;

namespace BASeTris.Replay
{
    /// <summary>
    /// A 'stateful' replay records the state of the game board. This records board state paired with elapsed time when changes occur.
    /// Current suggestion: Create a state for the board when a block group is "solidified", and after rows are cleared.
    /// </summary>
    public class StatefulReplay
    {
    }

    public class StatefulReplayState
    {
        //represents a single state of a stateful replay.

        public int Rows;
        public int Columns;
        public StatefulReplayStateBlockInformation[][] BoardState = null;

        public StatefulReplayState(StandardTetrisGameState Source)
        {

        }


    }
    public class StatefulReplayStateBlockInformation
    {
        public enum BlockInformation
        {
            Block_Empty,
            Block_Occupied,
            Block_New,
            Block_Active
        }
    }

}
