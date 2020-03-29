using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;
using BASeTris.Tetrominoes;
using Microsoft.Win32;

namespace BASeTris.AI
{
    //this is a simple evolver for the AI, which tweaks the Scoring rules.
    //AIEvolver interface. takes a ScoringRuleData interface and mutates it into a new generation. 
    public interface BASetrisAIEvolver<T> where T:StoredBoardState.AIScoringRuleData
    {
        IEnumerable<StoredBoardState.AIScoringRuleData> Mutate(StoredBoardState.AIScoringRuleData Parent, int NumChildren);
    }

    public class SimpleAIEvolver : BASetrisAIEvolver<StoredBoardState.AIScoringRuleData>
    {
        //double a = -0.610066f;
        //double b = 0.760666;
        //double c = -0.55663;
        ////double d = -.184483;
        //double d = -.384483;
        private double MutationMinimum = 0.0000001;
        private double MutationMaximum = 0.00001;
        public static Random rgen = new Random();
        public IEnumerable<StoredBoardState.AIScoringRuleData> Mutate(StoredBoardState.AIScoringRuleData Parent,int NumChildren)
        {
            var HeightScore = Parent.AggregateHeightScore - Parent.RowScore;
            var BumpinessScore = Parent.BumpinessScore;
            var HoleScore = Parent.HoleScore;
            var RowScore = Parent.RowScore;
            for (int i=0;i<NumChildren;i++)
            {
                var MutateHeight = MutateValue(HeightScore, MutationMinimum, MutationMaximum);
                var MutateBumpiness = MutateValue(BumpinessScore, MutationMinimum, MutationMaximum);
                var MutateHole = MutateValue(HoleScore, MutationMinimum, MutationMaximum);
                var MutateRow = MutateValue(RowScore, MutationMinimum, MutationMaximum);
                StoredBoardState.AIScoringRuleData scoreresult = new StoredBoardState.AIScoringRuleData() { AggregateHeightScore = MutateHeight, BumpinessScore = MutateBumpiness, HoleScore = MutateHole, RowScore = MutateRow };
                //yield this mutation
                yield return scoreresult;
            }
        }
        private double MutateValue(double OriginalValue,double MinimumMutation,double MaximumMutation)
        {
            double MutationAmount = rgen.NextDouble(MinimumMutation, MaximumMutation);
            var Direction = (double)(rgen.Next(1) == 1 ? 1 : -1);
            return OriginalValue + (MutationAmount * Direction);



        }

        public static void RunSimulations()
        {
            //create a new TetrisField, we'll use the contents of this
            
        }

        public static double RunSimulation(StoredBoardState.AIScoringRuleData scoredata)
        {
            //create the chooser.
            BlockGroupChooser bgc = new BagChooser(Tetromino.StandardTetrominoFunctions);
            
            TetrisField tf = new TetrisField();
            bool GameEnded = false;
            int nominoCount = 0;

            while(!GameEnded)
            {





            }

            return 0;

        }

       
    }
}
