using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AI;
using BASeTris.GameStates;
using BASeTris.Blocks;

namespace BASeTris.Choosers.AIChoosers
{
    //The Mean Chooser tries to give the player pieces that work as poorly as possible for their situation. You won't see a lot of I pieces here.
    //This utilizes the routines found in the TetrisAI routines. How convenient- I already wrote those!
    public class MeanChooser : BaseAIChooser
    {
        StoredBoardState.TetrisScoringRuleData AIRules = new StoredBoardState.TetrisScoringRuleData();
        public MeanChooser(GameplayGameState _StandardState, Func<Nomino>[] pAvailable) : base(_StandardState, pAvailable)
        {
        }

        //when we evaluate the next group, we evaluate it based on it giving the worst results. This StoredBoardState
        //will represent the best outcome from that piece- we will use that to evaluate the next piece, which will store it's best possibility here, and so on.
        //so, we basically give the worst possible options, and base our next choice on that worst possible option being used in the most effective way.
        private StoredBoardState BestCaseScenario = null;


        public override Nomino PerformGetNext()
        {
            //First, we need to see what we CAN choose from.
            //We are slightly limited- if the functions give back varied results or something then it might act weird.
            //Take all the available groups and turn it into a Nomino.
            var availablegroups = from b in _Available select b();
            NominoBlock[][] CurrentState = BestCaseScenario != null ? BestCaseScenario.State : _State.PlayField.Contents;
            //alrighty. Now, we take those available groups and get available board states for each one.
            Dictionary<Nomino, IEnumerable<StoredBoardState>> StateEvaluation = new Dictionary<Nomino, IEnumerable<StoredBoardState>>();

            foreach (Nomino b in availablegroups)
            {
                StateEvaluation.Add(b, StandardNominoAI.GetPossibleResults(CurrentState, b,AIRules));
            }

            Dictionary<Nomino, double> FinalScores = new Dictionary<Nomino, double>();
            Dictionary<Nomino, StoredBoardState> BestStates = new Dictionary<Nomino, StoredBoardState>();
            Dictionary<Nomino, StoredBoardState> WorstStates = new Dictionary<Nomino, StoredBoardState>();
            //OK, now we need to evaluate the possibilities returned by each. Basically turn them into an array of scores and create the appropriate data in the Dictionaries.
            foreach (var kvp in StateEvaluation)
            {
                List<double> AllScores = new List<double>();
                double Maximum = double.MinValue;
                double Minimum = double.MaxValue;
                StoredBoardState CurrentMaximum = null;
                StoredBoardState CurrentMinimum = null;
                foreach (var iterate in kvp.Value)
                {
                    double GrabScore = iterate.GetScore(typeof(GameStates.GameHandlers.StandardTetrisHandler), AIRules);
                    if (GrabScore > Maximum)
                    {
                        Maximum = GrabScore;
                        CurrentMaximum = iterate;
                    }

                    if (GrabScore < Minimum)
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
                    FinalScores.Add(kvp.Key, Maximum);
                    //FinalScores.Add(kvp.Key, new double[] { Average, Maximum, Minimum }.Average());
                }
            }

            var ordered = FinalScores.OrderBy((o) => o.Value).ToList();
            //select the "winning" FinalScore.
            var crappiest = ordered.FirstOrDefault(); //the lowest value. This is the block with the lowest "maximum" in terms of positive aspects.
            if (crappiest.Key == null)
            {
                BestCaseScenario = null;
                return TetrisGame.Choose(availablegroups);
            }
            else
            {
                Nomino ChosenGroup = crappiest.Key;

                //now, what was the best possible board state possible with this crappiest one?
                BestCaseScenario = BestStates[ChosenGroup];

                return ChosenGroup;
            }
        }
    }
}