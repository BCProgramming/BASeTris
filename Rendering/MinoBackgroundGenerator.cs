using BASeTris.Blocks;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using XInput.Wrapper;

namespace BASeTris.Rendering
{
    public class MinoTileGenerator
    {

        //we'll need to do a bit of overlap with the HolePiece detection.
        //what we will want to do is have a function that can find the smallest Contiguous area. The reason for this is that we would want to cancel attempt to continue processing a option that already has an impossible to satisfy situation of trying to fit tetrominoes into non-divisible by 4 contiguous areas.


        public static int[] FindContiguousSections(bool[][] State, Func<int, int, bool> TestPosition)
        {
            List<List<(int x, int y)>> FindContiguousSections = new List<List<(int x, int y)>>();

            for (int y = 0; y < State.Length; y++)
            {
                var Rowval = State[y];
                for (int x = 0; x < Rowval.Length; x++)
                {
                    if (!State[y][x] && !TestPosition(x, y))
                    {
                        //is an empty section. Is it adjacent to any of the existing parts we are trying to grow?
                        if (FindContiguousSections.Count == 0)
                        {
                            FindContiguousSections.Add(new List<(int x, int y)>() { (x, y) });
                        }
                        else
                        {
                            bool FoundMatch = false;
                            foreach (var checkcontig in FindContiguousSections)
                            {
                                if (checkcontig.Any((a) => IsAdjacentTo(a, (x, y))))
                                {
                                    FoundMatch = true;
                                    checkcontig.Add((x, y));
                                }
                            }
                            if (!FoundMatch) FindContiguousSections.Add(new List<(int x, int y)>() {( x, y) });

                        }

                    }


                }
            }
            if (FindContiguousSections.Count == 0) return new int[] { };
            return FindContiguousSections.Select((a) => a.Count).ToArray();







        }

        private static bool IsAdjacentTo((int x, int y) PointA, (int x, int y) PointB)
        {
            return !(PointA.x==PointB.x && PointA.y==PointB.y) && 
                
                
                
                (
                (Math.Abs(PointA.x - PointB.x) == 1 && PointA.y == PointB.y)
                || (Math.Abs(PointA.y - PointB.y)==1 && PointA.x == PointB.x))
                
                ;


        }
        private String StateToString(bool[][] Input)
        {

            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Input.Length; r++)
            {
                var CurrRow = Input[r];
                for (int c = 0; c < CurrRow.Length; c++)
                {
                    sb.Append(CurrRow[c] ? "#": " ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        

        public Nomino[]? Generate(bool[][] InitialState,List<Nomino> CurrentMinos, Func<Nomino>[] GeneratorFunctions, Point[] FreePositions = null,IRandomizer rgen = null,Func<List<Nomino>,bool> ProgressAction = null)
        {
            //TODO: instead of a Generator function- which is random and causes issues, we should take a list of Mino types, or possibly a list of Mino "templates" to be used. Or at least a List of Generator functions for specific types I suppose. Basically we need to make it predictable so we can
            //go through every possible position and rotation for each piece, as right now it's completely random what pieces pop out.
            Debug.Print("Minos:" + (CurrentMinos!=null?CurrentMinos.Count.ToString():"NULL"));
            if (rgen == null) rgen = RandomHelpers.Construct();
            if (CurrentMinos == null) CurrentMinos = new List<Nomino>();
            //InitialState is a true/false as to whether that position is occupied.
            //Task: from the provided initial state
            //1. If FreePositions is empty, find all empty rows and columns in the provided array and create a new array to represent them.
            


            var resultinit = FindContiguousSections(InitialState, (x, y) => CurrentMinos.Any((m) => m.Any((b) => b.X + m.X == x && b.Y + m.Y == y)));
            if (resultinit.Any((a) => a % 4 != 0))
            {
                ;
            }
            int MaxRecursiveAttempts = resultinit.Sum(); //maximum recursive calls is number of unoccupied blocks we have free.
            int CurrentRecurseCount = 0;
                if (FreePositions == null || FreePositions.Length == 0)
            {
                List<Point> FreePositionList = new List<Point>();
                for (int y = -4; y < InitialState.Length+4; y++) //row major order
                {

                    //if (y >= 0 && y < InitialState.Length)
                    //{
                        var Rowval = (y > 0 && y < InitialState.Length?InitialState[y]:InitialState[0]);
                        for (int x = -4; x < Rowval.Length + 4; x++)
                        {
                            //if (Rowval[x] == false) //removed: free positions don't necessarily reflect where a piece of the minos will actually be, so placing them only in free positions will restrict placement.
                                FreePositionList.Add(new Point(x, y));

                        }
                    //}
                    //else
                    //{

                    //    FreePositionList.Add(new Point(x, y));
                    //}
                    
                }
                FreePositions = FreePositionList.ToArray();

            }


            


            if (FreePositions.Length == 0)
            {
                //Mission accomplished. (?)
                return CurrentMinos.ToArray();
            }

            (Point, int)[] FreeData = FreePositions.Select((p) => (p, 0))
                                        .Concat(FreePositions.Select((p) => (p, 1)))
                                        .Concat(FreePositions.Select((p) => (p, 2)))
                                        .Concat(FreePositions.Select((p) => (p, 3))).ToArray();

            FreeData = RandomHelpers.Static.Shuffle(FreeData, rgen).ToArray();
            //no max place attempts, we go through all free positions and the rotations instead now. Shuffled so we get a randomized result (ideally!)

            foreach(var DataPoints in FreeData)
            {
                //2. Generate a new Nomino using the generation function
                //for (int i = 0; i < MaxPlaceAttempts; i++)
                GeneratorFunctions = RandomHelpers.Static.Shuffle(GeneratorFunctions,rgen).ToArray();
                foreach (var UseFunction in GeneratorFunctions)
                {
                    Nomino PlaceAttemptMino = UseFunction();

                    if (FreePositions.Length < PlaceAttemptMino.Count) continue;
                    //3. Choosing a random free position and rotation,
                    int Rotation = DataPoints.Item2;
                    var ChosenPosition = DataPoints.Item1;
                    //int Rotation = rgen.Next(4)%4;
                    //var ChosenPosition = TetrisGame.Choose(FreePositions,rgen);
                    PlaceAttemptMino.SetRotation(Rotation);
                    PlaceAttemptMino.X = ChosenPosition.X;
                    PlaceAttemptMino.Y = ChosenPosition.Y;

                    // determine if the Mino fits in that location.
                    foreach (var testblock in PlaceAttemptMino)
                    {
                        int testX = testblock.X + PlaceAttemptMino.X;
                        int testY = testblock.Y + PlaceAttemptMino.Y;
                        if (testY == 0)
                        {
                            ;
                        }
                        //test bounds of array. row-major so we'll test Y first.
                        if (testY < 0 || testY >= InitialState.Length) goto NextAttempt;
                        //now check x, using the Y position. Though it really ought not be a jagged array, may as well.
                        if (testX < 0 || testX >= InitialState[testY].Length) goto NextAttempt;

                        //Alright. Now- remembering the state is in row-major order, test if this position is occupied.

                        if (InitialState[testY][testX])
                        {
                            //it's already occupied >:(
                            goto NextAttempt;
                        }
                    }
                    //if we get this far, it's not occupied! Make a copy of the InitialState array.

                    bool[][] StateCopy = new bool[InitialState.Length][];

                    for (int c = 0; c < InitialState.Length; c++)
                    {
                        StateCopy[c] = InitialState[c].Select((b) => b).ToArray();
                    }

                    //now, with the copy, set the positions of the Mino.

                    foreach (var addblock in PlaceAttemptMino)
                    {
                        if (addblock.Y + PlaceAttemptMino.Y == 0)
                        {
                            ;
                        }
                        //remember: Row major order!
                        StateCopy[addblock.Y + PlaceAttemptMino.Y][addblock.X + PlaceAttemptMino.X] = true;
                    }
                    var result = FindContiguousSections(StateCopy, (x, y) => CurrentMinos.Concat(new[] { PlaceAttemptMino }).Any((m) => m.Any((b) => b.X + m.X == x && b.Y + m.Y == y)));
                    if (result.Any((a) => a % 4 != 0))
                    {
                        //TODO: test and verify FindContiguous sections!
                        Debug.Print("contiguous space(s) found that that can't fit a Tetromino:" + String.Join(",",result.Where((a)=>a%4!=0)));
                        goto NextAttempt;
                    }

                    


                    //create copy of FreePositions to pass along, excluding the positions of the Mino we placed.

                    Point[] PassFreePositions = FreePositions;

                    var NewMinos = CurrentMinos.Concat(new[] { PlaceAttemptMino }).ToList();
                    if (result.Length == 0)
                    {
                        //no contiguous spaces at all. This means we were successful!
                        return NewMinos.ToArray();
                    }
                    /*if (PassFreePositions.Length == 0)
                    {
                        //no need for a recursion here. We're done!
                        return NewMinos.ToArray();
                    }*/
                    //now, call recursively, add the mino we generated to the listing, of course.
                    if (ProgressAction != null)
                    {
                        if (ProgressAction(NewMinos)) throw new TransactionAbortedException();
                    }
                    CurrentRecurseCount++;
                    //Added idea: queue up these calls and process them in parallel?
                    var RecursiveResult = Generate(StateCopy, NewMinos, GeneratorFunctions, PassFreePositions, rgen, ProgressAction);

                    if (RecursiveResult != null)
                    {
                        return RecursiveResult;
                    }
                    else if (CurrentRecurseCount > MaxRecursiveAttempts) return null;
                NextAttempt:;
                }

            
            }
            return null;
            //4. If it does, create a deep copy of the InitialState array, then add the Mino into that array, and perform a recursive call.
            //If the recursive call returns a non-null value, we have succeeded. Return that result.
            //5. If the Mino does not fit, or if the #4 call fails, go back to step 3.



        }

        Thread[] CallQueueHandlers = null;
        Nomino[] CompletedResult = null;
        bool GenerationCancelled = false;
        public void MinoCallQueueProc()
        {
            while (CompletedResult==null && !GenerationCancelled)
            {
                RecursiveMinoCallQueue.TryDequeue(out var nextfunc);
                var plausibleresult = nextfunc();
                if (plausibleresult != null)
                {
                    CompletedResult = plausibleresult;
                    
                }
            }
        }

        ConcurrentQueue<Func<Nomino[]>> RecursiveMinoCallQueue = new ConcurrentQueue<Func<Nomino[]>>();




    }
    
    public class MinoTileGenerationResult
    {
        bool Successful { get; set; }
        Nomino[] ResultPieces { get; set; }
        private MinoTileGenerationResult()
        {
            Successful = false;
            ResultPieces = null;
        }
        private MinoTileGenerationResult(Nomino[] Pieces)
        {
            ResultPieces = Pieces;
            Successful = true;
        }
        public static MinoTileGenerationResult Success(Nomino[] Items)
        {
            return new MinoTileGenerationResult(Items);
        }
        public static MinoTileGenerationResult Fail()
        {
            return new MinoTileGenerationResult();
        }
    }
}
