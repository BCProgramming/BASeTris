using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BASeTris.Choosers;
using BASeTris.Replay;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using Microsoft.Win32;

namespace BASeTris.AI
{
    //this is a simple evolver for the AI, which tweaks the Scoring rules.
    //AIEvolver interface. takes a ScoringRuleData interface and mutates it into a new generation. 
    public interface BASetrisAIEvolver<T> where T:StoredBoardState.TetrisScoringRuleData
    {
        IEnumerable<StoredBoardState.TetrisScoringRuleData> Mutate(StoredBoardState.TetrisScoringRuleData Parent, int NumChildren);
    }

    public class SimpleAIEvolver : BASetrisAIEvolver<StoredBoardState.TetrisScoringRuleData>
    {
        //double a = -0.610066f;
        //double b = 0.760666;
        //double c = -0.55663;
        ////double d = -.184483;
        //double d = -.384483;
        private double MutationMinimum = 0.0000001;
        private double MutationMaximum = 0.1;
        public static Random rgen = new Random();
        public IEnumerable<StoredBoardState.TetrisScoringRuleData> Mutate(StoredBoardState.TetrisScoringRuleData Parent,int NumChildren)
        {
            var HeightScore = Parent.AggregateHeightScore - Parent.RowScore;
            var BumpinessScore = Parent.BumpinessScore;
            var HoleScore = Parent.HoleScore;
            var RowScore = Parent.RowScore;
            var CreviceScore = Parent.CrevasseScore;
            for (int i=0;i<NumChildren;i++)
            {
                var MutateHeight = MutateValue(HeightScore, MutationMinimum, MutationMaximum);
                var MutateBumpiness = MutateValue(BumpinessScore, MutationMinimum, MutationMaximum);
                var MutateHole = MutateValue(HoleScore, MutationMinimum, MutationMaximum);
                var MutateRow = MutateValue(RowScore, MutationMinimum, MutationMaximum);
                var MutateCrevice = MutateValue(CreviceScore, MutationMinimum, MutationMaximum);
                StoredBoardState.TetrisScoringRuleData scoreresult = new StoredBoardState.TetrisScoringRuleData() { AggregateHeightScore = MutateHeight, BumpinessScore = MutateBumpiness, HoleScore = MutateHole, RowScore = MutateRow,CrevasseScore = MutateCrevice };
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
        private static ConcurrentQueue<StoredBoardState.TetrisScoringRuleData> ThreadWorkItems = new ConcurrentQueue<StoredBoardState.TetrisScoringRuleData>();
        private static ConcurrentDictionary<StoredBoardState.TetrisScoringRuleData, double> ThreadWorkResults = new ConcurrentDictionary<StoredBoardState.TetrisScoringRuleData, double>();
        
        public static void RunSimulations(int ChildrenPerGeneration = 50)
        {
            int SimulationThreads = 7;
            //"RunSimulations" is my attempt at a single AI Evolver.
            SimpleAIEvolver evolver = new SimpleAIEvolver();
            //we start with the default.
            StoredBoardState.TetrisScoringRuleData ScoreData = new StoredBoardState.TetrisScoringRuleData();
            bool LocalMaxFound = false;
            const int MaxRetries = 5;
            int RetryCount = 0;
            double CurrentBestScore = RunMultipleSimulations(ScoreData);
            StoredBoardState.TetrisScoringRuleData BestScorer = ScoreData;
            while(!LocalMaxFound &&RetryCount<MaxRetries)
            {
                //With the current score Data, create 100 mutations.
                var Mutations = evolver.Mutate(ScoreData, ChildrenPerGeneration);
                Dictionary<double, StoredBoardState.TetrisScoringRuleData> GenerationScores = new Dictionary<double, StoredBoardState.TetrisScoringRuleData>();
                foreach (var iterate in Mutations)
                {
                    ThreadWorkItems.Enqueue(iterate);
                }
                Action ThreadWork = () =>
                {
                    while (!ThreadWorkItems.IsEmpty)
                    {
                        if (ThreadWorkItems.TryDequeue(out StoredBoardState.TetrisScoringRuleData result))
                        {
                            double MutationScore = RunMultipleSimulations(result);
                            ThreadWorkResults.TryAdd(result, MutationScore);
                        }
                    }



                };
                List<Thread> WorkerThreads = new List<Thread>();
                for(int i=0;i < SimulationThreads; i++)
                    {
                    Thread newThread = new Thread(()=> { ThreadWork(); });
                    WorkerThreads.Add(newThread);
                    newThread.Start();
                    }
                int LastCount = ThreadWorkItems.Count;
                while(!ThreadWorkItems.IsEmpty)
                {
                    Thread.Sleep(100);
                    if(ThreadWorkItems.Count!=LastCount)
                    {
                        Debug.Print((LastCount=ThreadWorkItems.Count).ToString() + "queued simulations.");
                    }
                }
                foreach(var iterate in ThreadWorkResults)
                {
                    if(!GenerationScores.ContainsKey(iterate.Value))
                        GenerationScores.Add(iterate.Value,iterate.Key);
                }
                /*foreach(var iterate in Mutations)
                {
                    //run a simulation on this mutation
                    double MutationScore = RunMultipleSimulations(iterate);
                    if(!GenerationScores.ContainsKey(MutationScore))
                        GenerationScores.Add(MutationScore,iterate);
                }*/
                //which ones did better than our current best score?
                var BetterScores = (from g in GenerationScores where g.Key > CurrentBestScore orderby g.Key descending select g).ToList();
                if(!BetterScores.Any())
                {
                    Debug.Print("No better scores found. Retry count:" + RetryCount + ". Settings:");
                    RetryCount++;
                    
                    Debug.Print(BestScorer.ToString());
                    if (RetryCount < MaxRetries) break;
                    LocalMaxFound = true;
                }
                else
                {
                    RetryCount = 0;
                    Debug.Print("Better averages found in mutated runs. Taking top scorer.");
                    //Take the best scorer and we will use it as the basis for a new generation.
                    var BestKVP = BetterScores.First();
                    Debug.Print("Top scorer achieved a score of " + CurrentBestScore);
                    CurrentBestScore = BestKVP.Key;
                    BestScorer = BestKVP.Value;
                    Debug.Print($"new Top Scorer: " + BestScorer.ToString());
                }

            }



        }
        public static double RunMultipleSimulations(StoredBoardState.TetrisScoringRuleData scoredata, int BoardWidth = 20, int BoardHeight = 22,int NumSimulations=10)
        {
            Debug.Print("Running multiple simulations with specified Score Information:");
            List<double> AllScores = new List<double>();
            for(int i=0;i<NumSimulations;i++)
            {
                
                //run one simulation.
                var CurrentSimResult = RunSimulation(scoredata, BoardWidth, BoardHeight);
                AllScores.Add(CurrentSimResult);
            }
            //give back average of all simulations.
            return AllScores.Average();
        }
      
        //Runs a simulation with a given piece of scoring data, and board width and height, and runs one simulated game using that scoring for AI determination and 
        //returns a valuation of the resulting gameplay using the scoring algorithm with those weights.
        public static double RunSimulation(StoredBoardState.TetrisScoringRuleData scoredata,int BoardWidth=10,int BoardHeight=22)
        {
            Debug.Print($"--------Running Simulation " + scoredata.ToString() + "-----");
                
            //create the chooser.
            BlockGroupChooser bgc = new BagChooser(Tetromino.StandardTetrominoFunctions,Environment.TickCount);
            List<Nomino> ChosenNominos = new List<Nomino>();
           

            NominoBlock[][] Contents = new NominoBlock[TetrisField.DEFAULT_ROWCOUNT][];
            for (int row = 0; row < TetrisField.DEFAULT_ROWCOUNT; row++)
            {
                Contents[row] = new NominoBlock[TetrisField.DEFAULT_COLCOUNT];
            }
            int Placedpieces = 0;
            int rowsFinished = 9;
            bool GameEnded = false;
            int nominoCount = 0;

            while(!GameEnded)
            {
                
                
                //get the next Nomino.
                var nextNomino = bgc.RetrieveNext();
                ChosenNominos.Add(nextNomino);
                //Debug.Print("Processing new Nomino:" + nextNomino.SpecialName);
                var PossibleBoardResults = StandardNominoAI.GetPossibleResults(Contents, nextNomino);
                //score each one based on the scoring rules.
                var BestScore = (from p in PossibleBoardResults orderby p.GetScore(typeof(GameStates.GameHandlers.StandardTetrisHandler), scoredata) descending select p).FirstOrDefault();

                if(BestScore!=null)
                {
                    // we go with the best scoring value of course.
                    Contents = BestScore.State;

                    String NewStateString = BestScore.GetBoardString();
                    //Debug.Print("Found best score location for new Nomino. Board State:");
                    //Debug.Print("\n\n" + NewStateString);
                    Placedpieces++;
                }

                int RemoveLines = RemoveFinishedRows(ref Contents);
                rowsFinished += (RemoveLines*RemoveLines); //square it, reason is to give higher scores for AI that manages to land more multi-line clears.
                //if the top line has a block than we will consider it Game Ended.
                if (Contents[0].Any((r) => r != null))
                {
                    GameEnded = true;
                }
            }
            //Debug.Print("Final Score:" +rowsFinished + Placedpieces);
            //Debug.Print($"----------------Completed Simulation Bumpiness:{scoredata.BumpinessScore}, Height:{scoredata.AggregateHeightScore}, Hole:{scoredata.HoleScore}, Row:{scoredata.RowScore} --------");
            return rowsFinished + Placedpieces; //return score for this simulation.

        }

       public static int RemoveFinishedRows(ref NominoBlock[][] Field)
        {
            int removeCount = 0;
            //first, let's find all the finished rows.
            List<int> finishedRows = new List<int>();
            
            for(int r=Field.Length-1;r>=0;r--)
            {
                if(Field[r].All((d)=>d!=null))
                {
                    finishedRows.Add(r);
                    removeCount++;
                }
            }

            if(finishedRows.Count>0)
            {
                foreach(int FinishedRow in finishedRows)
                {

                    

                    for(int g=FinishedRow;g<Field.Length-1;g++)
                    {
                        for(int c=0;c<Field[g].Length-1;c++)
                        {
                            Field[g][c] = Field[g - 1][c];
                        }
                    }
                }
            }
            return removeCount;
        }
    }
}
