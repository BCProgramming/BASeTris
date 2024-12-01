using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using BASeTris.GameStates;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using System.Reflection;
using BASeCamp.Logging;
using System.Collections.Concurrent;

namespace BASeTris.AI
{
    //AI experiments. highly ungood implementation IMO.
    //Main issue with this AI is that it will only evaluate possibilities involving moving a block, rotating it, and dropping it. This leaves out for example wall-kicks and in the case
    //of cascading styles sliding the blockgroup underneath existing blocks.
    //strictly speaking such capabilities are not outside the purview of the underlying infrastructure, but we'd need
    //a way to figure out those possibilities and how to properly encode them- technically we could do it with down keys, but the fallspeed
    //could mess t hat up- press down X times and who knows if an 'auto-fall' had been triggered too, which would mess it up.
    public class StandardNominoAI : BaseAI
    {
        public StandardNominoAI(IStateOwner pOwner) : base(pOwner)
        {
        }

        public static NominoBlock[][] DuplicateField(NominoBlock[][] Source)
        {
            NominoBlock[][] Copied = new NominoBlock[Source.Length][];
            for (int r = 0; r < Source.Length; r++)
            {
                NominoBlock[] row = Source[r];
                Copied[r] = new NominoBlock[row.Length];
                for (int c = 0; c < row.Length; c++)
                {
                    Copied[r][c] = row[c];
                }
            }

            return Copied;
        }
        //retrieves all possible positions of the given Mino as they could be placed on the board.
        public static IEnumerable<StoredBoardState> GetPossibleResults(NominoBlock[][] Source, Nomino bg)
        {
            //Debug.Print("Calculating possible results:" + Source.Sum((u)=>u.Count((y)=>y!=null)) + " Non null entries.");
            for (int useRotation = 0; useRotation < 4; useRotation++)
            {
                for (int x = -5; x < Source[0].Length + 5; x++)
                {


                    /*if(rules.StupidFactor<1)
                    {
                        if (TetrisGame.StatelessRandomizer.NextDouble() < rules.StupidFactor) continue;
                    }*/
                    Nomino cloneFor = new Nomino(bg);
                    foreach (var resetblock in cloneFor)
                    {
                        resetblock.Block = new StandardColouredBlock();
                    }

                    int XOffset = x - bg.X;
                    StoredBoardState BuildState = new StoredBoardState(Source, cloneFor, XOffset, useRotation);
                    if (!BuildState.InvalidState)
                        yield return BuildState;
                }
            }
        }

        public static IEnumerable<(double, StoredBoardState)> GetDepthScoreResultConsideringNextQueue(NominoBlock[][] InitialState, Nomino CurrentMino, Nomino[] NextQueue, Func<StoredBoardState, double> ScorerFunc,bool UseThreading=false)
        {
            DepthSearchHelper dsh = new DepthSearchHelper();
            return dsh.GetDepthScoreResultConsideringNextQueue(InitialState, CurrentMino, NextQueue, ScorerFunc, UseThreading);

        }




    private Dictionary<Type, StoredBoardState.BoardScoringRuleData> HandlerRuleDataDictionary = new Dictionary<Type, StoredBoardState.BoardScoringRuleData>();
        public StoredBoardState.BoardScoringRuleData ScoringRules { get
            {
                Type HandlerType = null;
                if(_Owner.CurrentState is GameplayGameState gps)
                {
                    HandlerType = gps.PlayField.Handler.GetType();
                }
                else if(_Owner.CurrentState is ICompositeState<GameplayGameState> igps)
                {
                    HandlerType = igps.GetComposite().PlayField.Handler.GetType();
                }

                if(HandlerType!=null)
                {
                    lock (HandlerRuleDataDictionary)
                    {
                        if (!HandlerRuleDataDictionary.ContainsKey(HandlerType))
                        {

                            var foundattrib = HandlerType.GetCustomAttributes(typeof(GameScoringHandlerAttribute));
                            foreach (var iterate in foundattrib)
                            {
                                if (iterate is GameScoringHandlerAttribute gsh)
                                {
                                    HandlerRuleDataDictionary.Add(HandlerType, (StoredBoardState.BoardScoringRuleData)Activator.CreateInstance(gsh.RuleDataType));
                                }
                            }
                        }
                    }
                    return HandlerRuleDataDictionary[HandlerType];
                }
                return null;
            }
        }
        Nomino LastProcessPiece = null;
        public override void AIActionFrame()
        {
            //do our hard thinking here.
            //first we only do stuff with the standard game state.
            if (_Owner == null) return;
            if (_Owner.CurrentState is GameplayGameState)
            {
                GameplayGameState stdState = _Owner.CurrentState as GameplayGameState;
                if (stdState == null) return;
                //next, we only want to do stuff if there is one active blockgroup...
                if (stdState.PlayField.GetActiveBlockGroups().Count == 1)
                {
                    //todo: we want to copy the playfield for our inspection here... we'll want to see what happens based on moving the blockgroup left or right up to each side and dropping it and evaluate the result to select the ideal
                    //then slap those keys into the queue.
                    Nomino ActiveGroup = stdState.PlayField.BlockGroups[0];
                    if (ActiveGroup == LastProcessPiece) return; // we don't want to recalculate when we already processed the place to put a piece. Also, that could cause oddities.
                    PressKeyQueue.Clear(); // if this is not the same piece as last time, it's a new one. We don't want to have any additional buttons left over to be pressed too.
                    LastProcessPiece = ActiveGroup; 
                    Nomino[] NextPieces = stdState.NextBlocks.Take(2).ToArray(); //more t han one processes too slow :(

                    //var PossibleStates = GetPossibleResults(stdState.PlayField.Contents, ActiveGroup).ToList();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var PossibleStates = GetDepthScoreResultConsideringNextQueue(stdState.PlayField.Contents, ActiveGroup, NextPieces, (s) => s.GetScore(stdState.GameHandler.GetType(), ScoringRules),true).ToList();
                    DebugLogger.Log.WriteLine("Processed Depth Search:" + sw.Elapsed.ToString());
                    Debug.Print("Found " + PossibleStates.Count + " possible states...");
                    var Sorted = (ScoringRules.Moronic?PossibleStates.OrderByDescending((w)=>TetrisGame.StatelessRandomizer.Next()):  PossibleStates.OrderByDescending((w) => w.Item1)).Select((d)=>d.Item2).ToList();   //w.GetScore(stdState.GameHandler.GetType(), ScoringRules))

                    //var Scores = (from p in PossibleStates orderby p.GetScore(ScoringRules) descending select new Tuple<StoredBoardState, double>(p, p.GetScore(ScoringRules))).ToArray();
                    /*foreach (var writedebug in Scores)
                    {
                        Debug.Print("Possible State: Move " + writedebug.Item1.XOffset + ", Rotate " + writedebug.Item1.RotationCount + " To get score " + writedebug.Item1.GetScore(ScoringRules));
                        Debug.Print("What it will look like\n" + writedebug.Item1.GetBoardString());
                        Debug.Print("------");
                    }*/

                    var maximumValue = Sorted.FirstOrDefault();
                    Debug.Print("Best Move: Move " + maximumValue.XOffset + ", Rotate " + maximumValue.RotationCount + " To get score " + maximumValue.GetScore(stdState.GameHandler.GetType(), ScoringRules));
                    Debug.Print("What it will look like\n" + maximumValue.GetBoardString());
                    Debug.Print("------");

                    //int randomint = TetrisGame.rgen.Next(Scores.Length);
                    //int randomint2 = TetrisGame.rgen.Next(Scores.Length);
                    //StoredBoardState FirstState = Scores[randomint2].Item1;
                    StoredBoardState IdealState = maximumValue;
                    PushButtonInputs(IdealState);
                    //if(maximumValue!=null)
                    //{
                    //    PushButtonInputs(maximumValue);
                    //}
                }
            }
        }

        private void PushButtonInputs(StoredBoardState sbs)
        {
            for (int i = 0; i < sbs.RotationCount; i++)
            {
                PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_RotateCW);
            }

            if (sbs.XOffset < 0)
            {
                for (int i = 0; i < Math.Abs(sbs.XOffset); i++)
                {
                    PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Left);
                }
            }
            else if (sbs.XOffset > 0)
            {
                for (int i = 0; i < Math.Abs(sbs.XOffset); i++)
                {
                    PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Right);
                }
            }

            for (int i = 0; i < 15; i++) PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Null);
            PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Drop);
        }
    }

    public class DepthSearchHelper
    {

        private List<Thread> WorkerThreads = null;
        private ConcurrentQueue<Func<(double, StoredBoardState)>> FuncCallQueue = null;
        private const int WORKER_THREADS = 4;
        private int ActiveWorkers = 0;
        ConcurrentBag<(double, StoredBoardState)> WorkerResults = new ConcurrentBag<(double, StoredBoardState)>();
        private void QueueWorker()
        {
            while (!FuncCallQueue.IsEmpty)
            {
                if (FuncCallQueue.TryDequeue(out var callitem))
                {
                    var Callresult = callitem();
                    WorkerResults.Add(Callresult);
                }
            }
            ActiveWorkers--;
        }
        public IEnumerable<(double, StoredBoardState)> GetDepthScoreResultConsideringNextQueue(NominoBlock[][] InitialState, Nomino CurrentMino, Nomino[] NextQueue, Func<StoredBoardState, double> ScorerFunc, bool UseThreading = false)
        {
            //Where GetPossibleResults gives all possible results for the next board state,
            //this one builds upon it, instead returning an enumerable tuple where Item1 is the top score for this move at the end of the provided "Next" queue pieces.
            //this also means we don't care about the score UNTIL that many pieces are used.

            //First, we start with the Possible Results of the next piece.
            IEnumerable<StoredBoardState> NextMoves = StandardNominoAI.GetPossibleResults(InitialState, CurrentMino);
            //if no next queue, we just give back the next moves with the high score attached.
            if (NextQueue == null || NextQueue.Length == 0)
            {
                foreach (var yieldval in NextMoves.Select((r) => (ScorerFunc(r), r)))
                {
                    yield return yieldval;
                }
                yield break;
            }
            //our task now is to find the highest score after processing all the Next Queue for each of these board states.
            else
            {
                Func<StoredBoardState, (double, StoredBoardState)> CheckMoveFunction = (s) =>
                {
                    var useNextState = ClearFilledRows(s.State);


                    //recursively call this function, passing in the first element of the nextqueue as the current mino, and the rest of the next queue as the next queue.
                    var Subresults = GetDepthScoreResultConsideringNextQueue(useNextState, NextQueue.First(), NextQueue.Skip(1).ToArray(), ScorerFunc);
                    //the top of these subresults is our result for this item.
                    var ChosenEndResult = Subresults.OrderByDescending((e) => e.Item1).FirstOrDefault();
                    return (ChosenEndResult.Item1, s);
                };


                //TODO: we need to make this stuff work async or with multiple threads
                if (!UseThreading)
                {
                    foreach (var checkmove in NextMoves)
                    {
                        //something we can't forget here: The board state we have actually doesn't have the cleared lines. We want to remove those like the game will, so that good places to put future pieces that get revealed by cleared lines
                        //are properly accounted for.
                       
                        //this is the end score, return this score, but the current nextmove.

                        yield return CheckMoveFunction(checkmove);
                    }
                }
                else
                {
                    FuncCallQueue = new ConcurrentQueue<Func<(double, StoredBoardState)>>();
                    foreach (var checkmove in NextMoves)
                    {
                        FuncCallQueue.Enqueue(() => CheckMoveFunction(checkmove));
                    }
                    WorkerThreads = new List<Thread>();
                    //with them all enqueued we can spin up the workers, then wait for the work queue to empty.
                    for (int i = 0; i < WORKER_THREADS; i++)
                    {
                        Thread WThread = new Thread(QueueWorker);
                        WorkerThreads.Add(WThread);
                        ActiveWorkers++;
                        WThread.Start();
                    }
                    while (!FuncCallQueue.IsEmpty && ActiveWorkers > 0)
                    {
                        //wait for the function call queue to empty out and for all the workers to complete.
                        Thread.Sleep(5);
                    }
                    foreach (var workerresult in WorkerResults)
                    {
                        yield return workerresult;
                    }
                   
                }
            }


        }
        public static NominoBlock[][] ClearFilledRows(NominoBlock[][] BoardState)
        {
            NominoBlock[][] Result = new NominoBlock[BoardState.Length][];
            int CurrentRow = BoardState.Length - 1; //start at the bottom.

            for (int CheckRow = BoardState.Length - 1; CheckRow >= 0; CheckRow--)
            {
                Result[CurrentRow] = new NominoBlock[BoardState[CheckRow].Length];
                if (!BoardState[CheckRow].All((b) => b != null))
                {

                    //they aren't all filled, so not a row. we can fill CurrentRow with this row and decrement it.
                    for (int c = 0; c < BoardState[CheckRow].Length - 1; c++)
                    {
                        Result[CurrentRow][c] = BoardState[CheckRow][c] != null ? new StandardColouredBlock() : null;
                    }
                    CurrentRow--;

                }


            }
            while (CurrentRow >= 0)
            {
                Result[CurrentRow] = Enumerable.Repeat<NominoBlock>(null, BoardState[0].Length).ToArray();
                CurrentRow--;
            }
            return Result;



        }
    }

    public class StoredBoardState
    {
        private Nomino _SourceGroup;
        private NominoBlock[][] _BoardState;

        public NominoBlock[][] State
        {
            get { return _BoardState; }
        }

        public int XOffset; //offset from the BlockGroups current position.
        public int RotationCount; //CW rotation count.
        public bool InvalidState = false;

        public String GetBoardString()
        {
            StringBuilder sb = new StringBuilder();

            for (int r = 0; r < State.Length; r++)
            {
                for (int c = 0; c < State[r].Length; c++)
                {
                    if (State[r][c] == null)
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append("#");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
        public int RowCount { get; private set; }
        public int ColCount { get; private set; }
        public StoredBoardState(NominoBlock[][] InitialState, Nomino pGroup, int pXOffset, int pRotationCount)
        {
            RowCount = InitialState.GetUpperBound(0);
            ColCount = InitialState[0].GetUpperBound(0);
            _BoardState = StandardNominoAI.DuplicateField(InitialState);
            if (pGroup == null) return;
            _SourceGroup = new Nomino(pGroup);
            XOffset = pXOffset;
            RotationCount = pRotationCount;
            foreach (var resetblock in _SourceGroup)
            {
                resetblock.Block = new StandardColouredBlock();
            }


            for (int i = 0; i < RotationCount; i++) _SourceGroup.Rotate(false);
            //move the Nomino by the specified offset...

            _SourceGroup.RecalcExtents();
            
            //if(_SourceGroup.GroupExtents.Left+_SourceGroup.X+XOffset < 0 || _SourceGroup.GroupExtents.Right+XOffset+_SourceGroup.X > _BoardState[0].Length)
            //{
            //    InvalidState = true; //nothing else to do- this is an invalid state as the move puts us "off" the board.
            //}
            //else
            {



                //get our board state...
                try
                {
                    var dbreturn = DropBlock(_SourceGroup, ref _BoardState, pXOffset);
                    if (!dbreturn)
                    {
                        InvalidState = true;
                        return;
                    }
                    else
                    {
                        ;
                    }
                }
                catch (Exception exx)
                {
                    InvalidState = true;
                    return;
                }
            }
        }

        private int GetCompletedLines()
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

        private int GetHeight(int column)
        {
            int Heightfound = _BoardState.Length;
            for (int i = _BoardState.Length - 1; i > 0; i--)
            {
                if (_BoardState[i][column] != null) Heightfound = i;
            }

            return _BoardState.Length - Heightfound;
        }

        private int CountColumnHoles(int column)
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

        private int GetAggregateHeight()
        {
            int HeightRunner = 0;
            for (int col = 0; col < _BoardState[0].Length; col++)
            {
                HeightRunner += GetHeight(col);
            }

            return HeightRunner;
        }

        private int GetHoles()
        {
            int HoleCount = 0;
            for (int c = 0; c < _BoardState[0].Length; c++)
            {
                HoleCount += CountColumnHoles(c);
            }

            return HoleCount;
        }

        private int GetBumpiness()
        {
            int HeightRunner = 0;
            int lastHeight = -1;
            for (int col = 0; col < _BoardState[0].Length; col++)
            {
                var CurrHeight = GetHeight(col);
                if (lastHeight == -1) lastHeight = CurrHeight;
                HeightRunner += Math.Abs((CurrHeight - lastHeight));
            }

            return HeightRunner;
        }
        private int GetCrevasses()
        {
            const int MinimumCrevasseHeight = 2;
            //a 'crevasse' is any place where the highest block is at least 3 blocks below the highest block in adjacent columns. The score is calculated as 1 plus the adjacent height above 3
            //convert each column into a number representing the highest block.
            int[] Heights = (from p in Enumerable.Range(0, _BoardState[0].Length - 1) select GetHeight(p)).ToArray();
            int accumScore = 0;
            for(int i=1;i<Heights.Length-2;i++)
            {
                int PrevHeight = Heights[i-1];
                int CurrHeight = Heights[i];
                int NextHeight = Heights[i + 1];
                int CreviceScore = (Math.Max(0,(Math.Abs(CurrHeight - PrevHeight) - MinimumCrevasseHeight))) + Math.Max(0,(Math.Abs(CurrHeight - NextHeight) - MinimumCrevasseHeight));
                accumScore += CreviceScore;


            }

            return accumScore;

        }

        public double GetScore(Type CustomizationType,  StoredBoardState.BoardScoringRuleData Rules)
        {
            //Gamehandler types should have an attribute that points to another type implementing the IGameScoringHandler interface. That will accept a board state
            //and provide the score.
            
            var scoreattributes = CustomizationType.GetCustomAttributes(typeof(GameScoringHandlerAttribute),true);
            if(scoreattributes.Length > 0)
            {
                //take the first one.
                GameScoringHandlerAttribute attrib = scoreattributes.First() as GameScoringHandlerAttribute;
                return attrib.Handler.CalculateScore(Rules, this);



            }

            throw new TypeLoadException("Failed to find GameScoringHandler for type " + CustomizationType.GetType().ToString());
            //calculate the "value" of this state.
            //we need:
            //number of completed rows/lines
            //Aggregate Height (sum of the height of the highest block in each column)
            //Holes- where we have null blocks with non-null blocks above it.
            //bumpiness: sum of the absolute differences of the heights of each adjacent column
            /*
            int Rows = GetCompletedLines();
            int Aggregate = GetAggregateHeight();
            int Holes = GetHoles();
            int Bumpy = GetBumpiness();
            int Crevice = GetCrevasses();
            //Debug.Print("Rows=" + Rows + " Aggregate=" + Aggregate + " Holes=" + Holes + " Bumps=" + Bumpy);
            //double a = -0.610066f;
            //double b = 0.760666;
            //double c = -0.55663;
            ////double d = -.184483;
            //double d = -.384483;
            double CreviceScore = (Rules.CrevasseScore * (double)Crevice);
            return (Rules.AggregateHeightScore * (double)Aggregate) +
                   (Rules.RowScore * (double)Rows) +
                   (Rules.HoleScore * (double)Holes) +
                   (Rules.BumpinessScore * (double)Bumpy) +
                    CreviceScore;
              */  

            /*a = -0.510066
b = 0.760666
c = -0.35663
d = -0.184483

a+AggregateHeight+b*completelines+c*holes+d*bumpiness*/
        }
        //returns true if successful, false if the piece didn't fit.
        private bool DropBlock(Nomino Source, ref NominoBlock[][] FieldState, int XOffset)
        {
            bool result = true;
            int YOffset = 0;
            bool neverfit = true;
            int ROWCOUNT = FieldState.Length - 1;
            int COLCOUNT = FieldState[0].Length - 1;

            Nomino Duplicator = new Nomino(Source);

            int dropLength = 0;
            while (true)
            {
                if (CanFit(Duplicator, FieldState, Duplicator.Y + 1, Duplicator.X + XOffset))
                {
                    dropLength++;
                    Duplicator.SetY(null,Duplicator.Y+1);
                }
                else
                {
                    break;
                }
            }
            if (dropLength == 0) return false;
            foreach (var iterate in Duplicator)
            {
                int Row = iterate.Y + Duplicator.Y;
                int Col = iterate.X + Duplicator.X + XOffset;
                if (Row > RowCount  || Col > ColCount  || Row < 0 || Col < 0)
                {

                }
                else
                {
                    //if (FieldState[Row][Col] != null) throw new ArgumentException("Invalid state...");
                    FieldState[Row][Col] = iterate.Block;
                }
            }
            return true;
            
        }

        private bool CanFit(Nomino Source, NominoBlock[][] Field, int Y, int X)
        {
            bool result = false;
            int ROWCOUNT = Field.Length - 1;
            int COLCOUNT = Field[0].Length - 1;
            foreach (var checkblock in Source)
            {
                int CheckRow = Y + checkblock.Y;
                int CheckCol = X + checkblock.X;
                if (CheckCol < 0)
                {
                    return false;
                }
                else if (CheckRow > ROWCOUNT || CheckCol > COLCOUNT)
                {
                    return false;
                }
                else
                {
                    var grabpos = Field[CheckRow][CheckCol];
                    if (grabpos != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public abstract class BoardScoringRuleData
        {
            public bool Moronic { get; set; } = false;
            public float StupidFactor { get; set; } = 1.0f;
        }
        public class MasterBlockScoringRuleData : BoardScoringRuleData
        {
            public double MasterBlockColumnMultiplier { get; set; } = 1.25f;
            public double MasterBlockMassValue { get; set; } = 4f;
        }
        public class DrMarioScoringRuleData : MasterBlockScoringRuleData
        {
        

        }
        public class Tetris2ScoringRuleData : MasterBlockScoringRuleData
        {
        
        }
        public class TetrisScoringRuleData:BoardScoringRuleData
        {
            //Height:-3.56765739215024,Row:0.627171085947044,Hole:-0.717625853214083,Bumpiness:-0.49091002708371,Crevasse:-0.40348813763272
            //Bump:-0.46105559660822,Height:-2.87825892656684,Hole:-0.631400211245207,Row:0.680445781380349   
            public double AggregateHeightScore { get; set; } = - 0.510066;
            public double RowScore { get; set; } = 0.760666d;
            public double HoleScore { get; set; } = -0.35663d;
            public double BumpinessScore { get; set; } = -0.184483;

            public double CrevasseScore { get; set; } = -0.40348813763272;
            public override string ToString()
            {
                return $"Height:{AggregateHeightScore},Row:{RowScore},Hole:{HoleScore},Bumpiness:{BumpinessScore},Crevasse:{CrevasseScore}";
            }
            
            
        }

        public class DepthSearchInfo
        {
            public List<DepthSearchInfo> Children { get; set; }
            public StoredBoardState CurrentState { get; set; }


            
            public bool IsFinalMove { get { return Children == null || Children.Count == 0; } }

            public IEnumerable<DepthSearchInfo> GetYoungest()
            {
                if (IsFinalMove)
                {
                    yield return this;
                }
                else
                {
                    foreach (var iterate in Children)
                    {
                        foreach (var child in iterate.GetYoungest())
                        {
                            yield return child;
                        }
                    }
                }
            }
            public void PopulateChildren(Nomino NextBlock)
            {
                //populates the children of this node, using "NextBlock" to evaluate the next move.
                //The Mino position shouldn't matter: realistically, we only need it for the current piece, but we are using this to evaluate where to put that piece, not how to move later pieces.
                Children = StandardNominoAI.GetPossibleResults(CurrentState._BoardState, NextBlock).Select((r) => new DepthSearchInfo() { CurrentState = r }).ToList();
            }

        }
    }
}