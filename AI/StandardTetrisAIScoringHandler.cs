﻿using BASeTris.Blocks;
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

        public static int OldGetHeight(NominoBlock[][] _BoardState, int column)
        {
            int Heightfound = _BoardState.Length;
            for (int i = _BoardState.Length - 1; i > 0; i--)
            {
                if (_BoardState[i][column] != null) Heightfound = i;
            }

            return _BoardState.Length - Heightfound;
        }
        public static int GetHeight(NominoBlock[][] _BoardState, int column)
        {
            int Heightfound = _BoardState.Length;
            for (int i = 0; i <= _BoardState.Length-1; i++)
            {
                if (_BoardState[i][column] != null)
                {
                    Heightfound = i;
                    break;
                }
            }

            return _BoardState.Length - (Heightfound-1);
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
            int[] Heights = (from p in Enumerable.Range(0, _BoardState[0].Length) select GetHeight(_BoardState,p)).ToArray();
            int accumScore = 0;
            for (int i = 1; i < Heights.Length - 1; i++)
            {
                int PrevHeight = Heights[i - 1];
                int CurrHeight = Heights[i];
                int NextHeight = i==Heights.Length-1?CurrHeight:Heights[i + 1];
                int CreviceScore = (Math.Max(0, (Math.Abs(CurrHeight - PrevHeight) - MinimumCrevasseHeight))) + Math.Max(0, (Math.Abs(CurrHeight - NextHeight) - MinimumCrevasseHeight));
                accumScore += CreviceScore;


            }

            return accumScore;

        }
        //new board scoring: We want to also detect edge "spears", where there's a big line of blocks in the sides. We want to discourage the AI from putting I pieces there.
        public double CalculateScore(BoardScoringRuleData Data, StoredBoardState state)
        {
            //for some reason, t he AI is loving to put pieces on the far right for seemingly no reason on occasion. It's unclear what is causing it but somehow the score evaluation is getting confused.
            var Rules = Data as TetrisScoringRuleData;
            var EvaluateState = state.State;
            int Rows = GetCompletedLines(EvaluateState);
            int Aggregate = GetAggregateHeight(EvaluateState); //we subtract the lines here so that the added height for clearing a line doesn't cause it to tip over to be considered "bad"
            int Holes = GetHoles(EvaluateState);
            int Bumpy = GetBumpiness(EvaluateState);
            int Crevice = GetCrevasses(EvaluateState);
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
                   (Rules.BumpinessScore * (double)Bumpy) + 
                   (Rules.CrevasseScore * (double)Crevice);

            return ScoreResult;


        }
    }
}
