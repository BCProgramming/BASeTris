using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using BASeTris.GameStates;
using BASeTris.TetrisBlocks;

namespace BASeTris.AI
{
    //AI experiments. highly ungood implementation IMO.
    public class TetrisAI : BaseAI
    {
        public TetrisAI(IStateOwner pOwner) : base(pOwner)
        {
        }

        public static TetrisBlock[][] DuplicateField(TetrisBlock[][] Source)
        {
            TetrisBlock[][] Copied = new TetrisBlock[Source.Length][];
            for (int r = 0; r < Source.Length; r++)
            {
                TetrisBlock[] row = Source[r];
                Copied[r] = new TetrisBlock[row.Length];
                for (int c = 0; c < row.Length; c++)
                {
                    Copied[r][c] = row[c];
                }
            }

            return Copied;
        }

        public static IEnumerable<StoredBoardState> GetPossibleResults(TetrisBlock[][] Source, BlockGroup bg)
        {
            //Debug.Print("Calculating possible results:" + Source.Sum((u)=>u.Count((y)=>y!=null)) + " Non null entries.");
            for (int useRotation = 0; useRotation < 4; useRotation++)
            {
                for (int x = -5; x < Source[0].Length + 5; x++)
                {
                    BlockGroup cloneFor = new BlockGroup(bg);
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

        public override void AIActionFrame()
        {
            //do our hard thinking here.
            //first we only do stuff with the standard game state.
            if (_Owner == null) return;
            if (_Owner.CurrentState is StandardTetrisGameState)
            {
                StandardTetrisGameState stdState = _Owner.CurrentState as StandardTetrisGameState;
                //next, we only want to do stuff if there is one active blockgroup...
                if (stdState.PlayField.BlockGroups.Count == 1)
                {
                    //todo: we want to copy the playfield for our inspection here... we'll want to see what happens based on moving the blockgroup left or right up to each side and dropping it and evaluate the result to select the ideal
                    //then slap those keys into the queue.
                    BlockGroup ActiveGroup = stdState.PlayField.BlockGroups[0];
                    var PossibleStates = GetPossibleResults(stdState.PlayField.Contents, ActiveGroup).ToList();
                    Debug.Print("Found " + PossibleStates.Count + " possible states...");
                    var Sorted = PossibleStates.OrderByDescending((w) => w.GetScore()).ToList();
                    var Scores = (from p in PossibleStates orderby p.GetScore() descending select new Tuple<StoredBoardState, double>(p, p.GetScore())).ToArray();
                    foreach (var writedebug in Scores)
                    {
                        Debug.Print("Possible State: Move " + writedebug.Item1.XOffset + ", Rotate " + writedebug.Item1.RotationCount + " To get score " + writedebug.Item1.GetScore());
                        Debug.Print("What it will look like\n" + writedebug.Item1.GetBoardString());
                        Debug.Print("------");
                    }

                    var maximumValue = Sorted.FirstOrDefault();
                    Debug.Print("Best Move: Move " + maximumValue.XOffset + ", Rotate " + maximumValue.RotationCount + " To get score " + maximumValue.GetScore());
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

            for (int i = 0; i < 5; i++) PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Null);
            PressKeyQueue.Enqueue(GameState.GameKeys.GameKey_Drop);
        }
    }

    public class StoredBoardState
    {
        private BlockGroup _SourceGroup;
        private TetrisBlock[][] _BoardState;

        public TetrisBlock[][] State
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

        public StoredBoardState(TetrisBlock[][] InitialState, BlockGroup pGroup, int pXOffset, int pRotationCount)
        {
            _BoardState = TetrisAI.DuplicateField(InitialState);
            _SourceGroup = new BlockGroup(pGroup);
            XOffset = pXOffset;
            RotationCount = pRotationCount;
            foreach (var resetblock in _SourceGroup)
            {
                resetblock.Block = new StandardColouredBlock();
            }


            for (int i = 0; i < RotationCount; i++) _SourceGroup.Rotate(false);
            //move the BlockGroup by the specified offset...

            _SourceGroup.RecalcExtents();
            /*
            if(_SourceGroup.GroupExtents.Left+_SourceGroup.X+XOffset < 0 || _SourceGroup.GroupExtents.Right+XOffset+_SourceGroup.X > _BoardState[0].Length)
            {
                InvalidState = true; //nothing else to do- this is an invalid state as the move puts us "off" the board.
            }
            else*/
            {
                //get our board state...
                try
                {
                    DropBlock(_SourceGroup, ref _BoardState, pXOffset);
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
                TetrisBlock ThisBlock = _BoardState[i][column];
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

        public double GetScore()
        {
            //calculate the "value" of this state.
            //we need:
            //number of completed rows/lines
            //Aggregate Height (sum of the height of the highest block in each column)
            //Holes- where we have null blocks with non-null blocks above it.
            //bumpiness: sum of the absolute differences of the heights of each adjacent column

            int Rows = GetCompletedLines();
            int Aggregate = GetAggregateHeight();
            int Holes = GetHoles();
            int Bumpy = GetBumpiness();
            Debug.Print("Rows=" + Rows + " Aggregate=" + Aggregate + " Holes=" + Holes + " Bumps=" + Bumpy);
            double a = -0.510066f;
            double b = 0.760666;
            double c = -0.35663;
            double d = -.184483;
            return (a * (double) Aggregate) +
                   (b * (double) Rows) +
                   (c * (double) Holes) +
                   (d * (double) Bumpy);

            /*a = -0.510066
b = 0.760666
c = -0.35663
d = -0.184483

a+AggregateHeight+b*completelines+c*holes+d*bumpiness*/
        }

        private void DropBlock(BlockGroup Source, ref TetrisBlock[][] FieldState, int XOffset)
        {
            bool result = true;
            int YOffset = 0;
            bool neverfit = true;
            int ROWCOUNT = FieldState.Length - 1;
            int COLCOUNT = FieldState[0].Length - 1;

            BlockGroup Duplicator = new BlockGroup(Source);

            int dropLength = 0;
            while (true)
            {
                if (CanFit(Duplicator, FieldState, Duplicator.Y + 1, Duplicator.X + XOffset))
                {
                    dropLength++;
                    Duplicator.Y++;
                }
                else
                {
                    break;
                }
            }

            foreach (var iterate in Duplicator)
            {
                int Row = iterate.Y + Duplicator.Y;
                int Col = iterate.X + Duplicator.X + XOffset;
                if (FieldState[Row][Col] != null) throw new ArgumentException("Invalid state...");
                FieldState[Row][Col] = iterate.Block;
            }
        }

        private bool CanFit(BlockGroup Source, TetrisBlock[][] Field, int Y, int X)
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
    }
}