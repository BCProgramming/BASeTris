using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AI;
using BASeTris.GameStates;
using BASeTris.TetrisBlocks;

namespace BASeTris.Choosers.AIChoosers
{
    //The Mean Chooser tries to give the player pieces that work as poorly as possible for their situation. You won't see a lot of I pieces here.
    //This utilizes the routines found in the TetrisAI routines. How convenient- I already wrote those!
    public class MeanChooser : BaseAIChooser
    {
        public MeanChooser(StandardTetrisGameState _StandardState, Func<BlockGroup>[] pAvailable) : base(_StandardState, pAvailable)
        {
        }
        //when we evaluate the next group, we evaluate it based on it giving the worst results. This StoredBoardState
        //will represent the best outcome from that piece- we will use that to evaluate the next piece, which will store it's best possibility here, and so on.
        //so, we basically give the worst possible options, and base our next choice on that worst possible option being used in the most effective way.
        private StoredBoardState BestCaseScenario = null; 
        public override BlockGroup GetNext()
        {
            //First, we need to see what we CAN choose from.
            //We are slightly limited- if the functions give back varied results or something then it might act weird.
            //Take all the available groups and turn it into a BlockGroup.
            var availablegroups = from b in _Available select b();
            TetrisBlock[][] CurrentState = BestCaseScenario != null ? BestCaseScenario.State : _State.PlayField.Contents;
            //alrighty. Now, we take those available groups and get available board states for each one.
            Dictionary<BlockGroup, IEnumerable<StoredBoardState>> StateEvaluation = new Dictionary<BlockGroup, IEnumerable<StoredBoardState>>();
            
            foreach(BlockGroup b in availablegroups)
            {

                StateEvaluation.Add(b,TetrisAI.GetPossibleResults(CurrentState,b));
            }
            
            Dictionary<BlockGroup, double> FinalScores = new Dictionary<BlockGroup, double>();
            Dictionary<BlockGroup, StoredBoardState> BestStates = new Dictionary<BlockGroup, StoredBoardState>();
            Dictionary<BlockGroup, StoredBoardState> WorstStates = new Dictionary<BlockGroup, StoredBoardState>();
            //OK, now we need to evaluate the possibilities returned by each. Basically turn them into an array of scores and create the appropriate data in the Dictionaries.
            foreach(var kvp in StateEvaluation)
            {

                List<double> AllScores = new List<double>();
                double Maximum = double.MinValue;
                double Minimum = double.MaxValue;
                StoredBoardState CurrentMaximum = null;
                StoredBoardState CurrentMinimum = null;
                foreach(var iterate in kvp.Value)
                {
                    double GrabScore = iterate.GetScore();
                    if(GrabScore > Maximum)
                    {
                        Maximum = GrabScore;
                        CurrentMaximum = iterate;
                    }
                    if(GrabScore < Minimum)
                    {
                        Minimum = GrabScore;
                        CurrentMinimum = iterate;
                    }
                    AllScores.Add(GrabScore);
                }
                if (AllScores.Count > 0)
                {
                    BestStates[kvp.Key] = CurrentMaximum;
                    WorstStates[kvp.Key] = CurrentMinimum;
                    var Average = AllScores.Average();

                    FinalScores.Add(kvp.Key, new double[] { Average, Maximum, Minimum }.Average());
                }
            }

            var ordered = FinalScores.OrderBy((o) => o.Value).ToList();
            //select the "winning" FinalScore.
            var crappiest = ordered.First();  //the lowest value.
            BlockGroup ChosenGroup = crappiest.Key;

            //now, what was the best possible board state possible with this crappiest one?
            BestCaseScenario = BestStates[ChosenGroup];

            return ChosenGroup;
           
        }
    }
}
