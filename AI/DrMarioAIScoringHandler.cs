using BASeTris.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.AI
{
    class DrMarioAIScoringHandler : IGameScoringHandler
    {
        public class MassItem
        {
            public List<LineSeriesBlock> MassContents { get; private set; }
            public int MassSize { get { return MassContents.Count; } }
            public MassItem(IEnumerable<LineSeriesBlock> Blocks)
            {
                MassContents = new List<LineSeriesBlock>(Blocks);
            }
        }
        
        public double GetFieldColumnScore(StoredBoardState.DrMarioScoringRuleData data, StoredBoardState state)
        {
            List<List<double>> AllColumnRuns = new List<List<double>>();
            for (int c = 0; c < state.ColCount; c++)
            {
                List<double> ColumnRuns = new List<double>();
                int CurrentRun = 0;
                int MasterCount = 0;
                int Interfacecount = 0; //number of changes.
                bool ResetHandled = false;
                LineSeriesBlock.CombiningTypes? currentType = null;
                for (int r = 0; r < state.RowCount; r++)
                {
                    ResetHandled = false;
                    if (state.State[r][c] is LineSeriesBlock lsb)
                    {
                        if (state.State[r][c] is LineSeriesPrimaryBlock)
                        {
                            MasterCount++;
                        }
                        if (currentType == null)
                        {
                            currentType = lsb.CombiningIndex;
                        }
                        if (currentType == lsb.CombiningIndex)
                        {

                            CurrentRun++;
                        }
                        else if (currentType != lsb.CombiningIndex)
                        {
                            Interfacecount++;
                            ResetHandled = true;
                            ColumnRuns.Add(CurrentRun * ((MasterCount + 1) * data.MasterBlockColumnMultiplier));
                            CurrentRun = 1;
                            MasterCount = 0;
                            currentType = lsb.CombiningIndex;
                        }
                    }



                }
                if (!ResetHandled)
                {
                    ColumnRuns.Add(CurrentRun * ((MasterCount + 1) * data.MasterBlockColumnMultiplier));
                }

                AllColumnRuns.Add(ColumnRuns);


            }
            //want longer runs, but, we also want to avoid stacking colours on top of unmatched colours, so we give weight to
            //the number of scores we accumulated in the row.
            return (from co in AllColumnRuns select (co.Sum()-(co.Count*co.Count))).Sum();
        }

        private IEnumerable<LineSeriesBlock> FindHorizontalMass(StoredBoardState.DrMarioScoringRuleData data, StoredBoardState state, int pRow, int pCol, LineSeriesBlock.CombiningTypes MatchingIndex,List<LineSeriesBlock> InConsiderables)
        {
            int CurrR = pRow, CurrC = pCol;
           //look to the left
            while (state.State[CurrR][CurrC] is LineSeriesBlock lsb && lsb.CombiningIndex == MatchingIndex && CurrC > 0) CurrC--;
            int FirstC = CurrC;
            int CheckC = FirstC;
            if (CurrC != FirstC)
            {
                //we have a horizontal group. let's grab the blocks.
                while (state.State[CurrR][CheckC] is LineSeriesBlock lsb && lsb.CombiningIndex == MatchingIndex && CurrC < state.ColCount - 1)
                {
                    if (InConsiderables.Contains(lsb)) break;
                    CheckC++;
                    yield return lsb;
                }
            }



        }

        private IEnumerable<LineSeriesBlock> FindVerticalMass(StoredBoardState.DrMarioScoringRuleData data, StoredBoardState state, int pRow, int pCol,LineSeriesBlock.CombiningTypes MatchingIndex, List<LineSeriesBlock> InConsiderables)
        {
            int CurrR = pRow, CurrC = pCol;
            //look above.
            while (state.State[CurrR][CurrC] is LineSeriesBlock lsb && lsb.CombiningIndex == MatchingIndex && CurrR > 0) CurrR--;
            int FirstR = CurrR;
            int CheckR = FirstR;
            
            if(CurrR!=FirstR)
            {
                //we have a vertical group. Lets see what blocks. So from the starting point,
                //check each block until air, or until we see a different coloured block OR we reach the other side of the stage.
                while(state.State[CheckR][CurrC] is LineSeriesBlock lsb && lsb.CombiningIndex == MatchingIndex && CurrC < state.ColCount - 1)
                {
                    if (InConsiderables.Contains(lsb)) break;
                    CheckR++;
                    yield return lsb;
                }
            }

        }
       
        //DrMario Scoring:
        //Lines are worthless, obviously
        //Critical Masses add lots of value
        //groups of the same colour add value
        //additionally, columns of a colour with only air in-between as well as possible colour drops 
        //(if the bottom colour of a stuck stack fell as far as it could would line up with other of the same colour

        public IEnumerable<MassItem> FindMasses(StoredBoardState.DrMarioScoringRuleData data, StoredBoardState state)
        {
           
            List<LineSeriesBlock> ConsideredResults = new List<LineSeriesBlock>();


            for(int r = 0;r<state.RowCount;r++)
            {
                for(int c=0;c<state.ColCount;c++)
                {
                    if(state.State[r][c] is LineSeriesBlock lsb)
                    {
                        //check for a mass here.
                        var HorizontalMass = FindHorizontalMass(data, state, r, c, lsb.CombiningIndex, ConsideredResults).ToList();
                        var VerticalMass = FindVerticalMass(data, state, r, c, lsb.CombiningIndex, ConsideredResults).ToList();
                        ConsideredResults.Add(lsb);
                        if(HorizontalMass.Count > 1)
                        {
                            
                            MassItem HorzMass = new MassItem(HorizontalMass);
                            yield return HorzMass;
                        }
                        if(VerticalMass.Count > 1)
                        {
                           
                            MassItem VertMass = new MassItem(VerticalMass);
                            yield return VertMass;
                        }
                        
                    }
                }
            }


           



        }




        public double CalculateScore(StoredBoardState.BoardScoringRuleData data, StoredBoardState state)
        {
            StoredBoardState.DrMarioScoringRuleData dat = data as StoredBoardState.DrMarioScoringRuleData;

            var FieldColumnScore = GetFieldColumnScore(dat, state);
            Debug.Print("Field Column Score:" + FieldColumnScore);
            double MassScore = 0;
            foreach(var iterate in FindMasses(dat,state))
            {
                int VirusCount = (from c in iterate.MassContents where c is LineSeriesPrimaryBlock select c).Count();
                double AddScore = ((double)VirusCount) * dat.MasterBlockMassValue + iterate.MassSize;
                MassScore += AddScore;
            }
            return FieldColumnScore+MassScore; //basically random, I think.
            //throw new NotImplementedException();
        }
    }
    
}
