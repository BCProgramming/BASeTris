using BASeTris.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BASeTris.AI.StoredBoardState;

namespace BASeTris.AI
{
    public class StandardTetrisAIScoringHandler : IGameScoringHandler
    {
        public static int GetCompletedLines(NominoBlock[][] _BoardState)
        {
            int countlines = 0;
            foreach (var iterate in _BoardState)
            {
                if (iterate.All((r) => r != null))
                {
                    countlines++;
                }
            }

            return countlines;
        }

        public static int GetHeight(NominoBlock[][] _BoardState, int column)
        {
            int Heightfound = _BoardState.Length;
            for (int i = _BoardState.Length - 1; i > 0; i--)
            {
                if (_BoardState[i][column] != null) Heightfound = i;
            }

            return _BoardState.Length - Heightfound;
        }

        public static int CountColumnHoles(NominoBlock[][] _BoardState, int column)
        {
            int FoundSinceFilled = 0;
            int FoundTotal = 0;
            for (int i = _BoardState.Length - 1; i > 0; i--)
            {
                NominoBlock ThisBlock = _BoardState[i][column];
                if (ThisBlock == null) FoundSinceFilled++;
                else
                {
                    FoundTotal += FoundSinceFilled;
                    FoundSinceFilled = 0;
                }
            }

            return FoundTotal;
        }

        public static int GetAggregateHeight(NominoBlock[][] _BoardState)
        {
            int HeightRunner = 0;
            for (int col = 0; col < _BoardState[0].Length; col++)
            {
                HeightRunner += GetHeight(_BoardState,col);
            }

            return HeightRunner;
        }

        public static int GetHoles(NominoBlock[][] _BoardState)
        {
            int HoleCount = 0;
            for (int c = 0; c < _BoardState[0].Length; c++)
            {
                HoleCount += CountColumnHoles(_BoardState,c);
            }

            return HoleCount;
        }

        public static int GetBumpiness(NominoBlock[][] _BoardState)
        {
            int HeightRunner = 0;
            int lastHeight = -1;
            for (int col = 0; col < _BoardState[0].Length; col++)
            {
                var CurrHeight = GetHeight(_BoardState,col);
                if (lastHeight == -1) lastHeight = CurrHeight;
                HeightRunner += Math.Abs((CurrHeight - lastHeight));
            }

            return HeightRunner;
        }
        public static int GetCrevasses(NominoBlock[][] _BoardState)
        {
            const int MinimumCrevasseHeight = 2;
            //a 'crevasse' is any place where the highest block is at least 3 blocks below the highest block in adjacent columns. The score is calculated as 1 plus the adjacent height above 3
            //convert each column into a number representing the highest block.
            int[] Heights = (from p in Enumerable.Range(0, _BoardState[0].Length - 1) select GetHeight(_BoardState,p)).ToArray();
            int accumScore = 0;
            for (int i = 1; i < Heights.Length - 2; i++)
            {
                int PrevHeight = Heights[i - 1];
                int CurrHeight = Heights[i];
                int NextHeight = Heights[i + 1];
                int CreviceScore = (Math.Max(0, (Math.Abs(CurrHeight - PrevHeight) - MinimumCrevasseHeight))) + Math.Max(0, (Math.Abs(CurrHeight - NextHeight) - MinimumCrevasseHeight));
                accumScore += CreviceScore;


            }

            return accumScore;

        }

        public double CalculateScore(BoardScoringRuleData Data, StoredBoardState state)
        {
            var Rules = Data as TetrisScoringRuleData;
            int Rows = GetCompletedLines(state.State);
            int Aggregate = GetAggregateHeight(state.State);
            int Holes = GetHoles(state.State);
            int Bumpy = GetBumpiness(state.State);
            int Crevice = GetCrevasses(state.State);
            //Debug.Print("Rows=" + Rows + " Aggregate=" + Aggregate + " Holes=" + Holes + " Bumps=" + Bumpy);
            //double a = -0.610066f;
            //double b = 0.760666;
            //double c = -0.55663;
            ////double d = -.184483;
            //double d = -.384483;
            double CreviceScore = (Rules.CrevasseScore * (double)Crevice);

            var ScoreResult = (Rules.AggregateHeightScore * (double)Aggregate) +
                   (Rules.RowScore * (double)Rows) +
                   (Rules.HoleScore * (double)Holes) +
                   (Rules.BumpinessScore * (double)Bumpy);

            return ScoreResult;


        }
    }
}
