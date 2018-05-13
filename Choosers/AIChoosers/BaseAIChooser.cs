using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AI;
using BASeTris.GameStates;

namespace BASeTris.Choosers.AIChoosers
{
    public abstract class BaseAIChooser : BlockGroupChooser 
    {
        protected StandardTetrisGameState _State;
        public BaseAIChooser(StandardTetrisGameState _StandardState, Func<BlockGroup>[] pAvailable) : base(pAvailable)
        {
            _State = _StandardState;
        }

        public double[] GetScores(BlockGroup WithGroup)
        {


            var boardstate = TetrisAI.GetPossibleResults(_State.PlayField.Contents, WithGroup);
            
            //Task is to take this block group, and get <all> the "AI Score" of all possible placement positions for it.
            //we return that in an array.

            return Enumerable.Empty<double>().ToArray();
        }


        public abstract override BlockGroup GetNext();
        
    }
}
