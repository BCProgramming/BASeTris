using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    //Class which generates N-Nominos, that is to say: it generates the possible pieces when told how many blocks are present.

    public class NNominoGenerator
    {
        public struct NominoPoint
        {
            public int  X;
            public int Y;
            public NominoPoint(int pX, int pY)
            {
                X = pX;
                Y = pY;
                    
            }
            public static NominoPoint Create(int pX, int pY)
            {
                return new NominoPoint(pX, pY);
            }
            public override int GetHashCode()
            {
                return X + (Y + (((X + 1) / 2) * ((X + 1) / 2))); //bijective algorithm...
            }
            public override bool Equals(object obj)
            {
                if (obj is NominoPoint np)
                    return Equals(np);
                return false;
            }
            public bool Equals(NominoPoint np)
            {
                return X == np.X && Y == np.Y;
            }
            public static bool operator ==(NominoPoint A, NominoPoint B)
            {
                return A.Equals(B);
            }
            public static bool operator !=(NominoPoint A, NominoPoint B)
            {
                return !A.Equals(B);
            }
            public override string ToString()
            {
                return $"NP:({X},{Y})";
            }
        }
        //given a set of nomino points, retrieves a list of "Turtle" directions that create said nomino.
        //The purpose of this is to detect (some) duplicate nominoes.
        public static String GetDirectionString(List<NominoPoint> input)
        {
            NominoPoint? Previous = null;
            NominoPoint? Current = null;
            NominoPoint? ForwardDirection = new NominoPoint(0,1);
            String BuildResult = "";
            for (int i = 0; i < input.Count; i++)
            {
                Previous = Current;
                Current = input[i];
                if (!Previous.HasValue) continue;


                

                NominoPoint Diff = new NominoPoint(Current.Value.X - Previous.Value.X, Current.Value.Y - Previous.Value.Y);

                if (ForwardDirection == null)
                {
                    ForwardDirection = Diff;
                    continue;
                }

                //we need to decide to add L, R, F (or, B- backwards) based on how the Diff relates to the ForwardDirection.

                //if they are equal, then that is forward....
                if (ForwardDirection.Value == Diff)
                    BuildResult= BuildResult += "F";
                else if (ForwardDirection.Value == new NominoPoint(Diff.Y, -Diff.X))
                { //right
                    BuildResult += "L";
                    
                }
                else if (ForwardDirection.Value == new NominoPoint(-Diff.Y, Diff.X))
                {
                    //left
                    BuildResult += "R";
                }
                else if (ForwardDirection.Value == new NominoPoint(-Diff.Y, -Diff.X))
                {
                    BuildResult += "B";
                }

                ForwardDirection = Diff;

            }
            return BuildResult;

            
        }
        private static List<NominoPoint> ResetTranslation(List<NominoPoint> input)
        {
            //translate all coordinates so that the upper-left corner is at 0,0.

            //Find minimum X and maximum Y.
            int MinX = 0, MaxY = 0;
            foreach (var iterate in input)
            {
                if (iterate.X < MinX) MinX = iterate.X;
                if (iterate.Y > MaxY) MaxY = iterate.Y;
            }
            return (from n in input select new NominoPoint(n.X - MinX, n.Y - MaxY)).ToList();
        }
        private static bool IsEqual(List<NominoPoint> A, List<NominoPoint> B)
        {
            var ResetA = ResetTranslation(A);
            var ResetB = ResetTranslation(B);
            //equality which doesn't care about order.
            //if the same points are in both, regardless of order, they are considered equal.
            if (A.Count != B.Count) return false; //different numbers means not equal.

            foreach (var iterateA in ResetA)
            {
                if (!ResetB.Any((b) => iterateA.Equals(b)))
                    return false;


            }

            return true;
        }
        private static List<NominoPoint> RotateCW(List<NominoPoint> input)
        {

            List<NominoPoint> BuildList = new List<NominoPoint>();
            BuildList = (from p in input select new NominoPoint(p.Y, -p.X)).ToList();
            return ResetTranslation(BuildList);

        }
        public static String StringRepresentation(List<NominoPoint> Points)
        {

            int MinX = int.MaxValue, MaxX = int.MinValue;
            int MinY = int.MaxValue, MaxY = int.MinValue;

            foreach (var iteratepoint in Points)
            {
                if (iteratepoint.X < MinX) MinX = iteratepoint.X;
                if (iteratepoint.X > MaxX) MaxX = iteratepoint.X;
                if (iteratepoint.Y < MinY) MinY = iteratepoint.Y;
                if (iteratepoint.Y > MaxY) MaxY = iteratepoint.Y;

            }
            StringBuilder sb = new StringBuilder();
            for (int ycoord = MaxY; ycoord >= MinY; ycoord--)
            {
                for (int xcoord = MinX; xcoord <= MaxX; xcoord++)
                {
                    if (Points.Any((p) => (p.X == xcoord && p.Y == ycoord)))
                        sb.Append("#");
                    else
                        sb.Append(" ");
                }
                sb.AppendLine("");
            }


            return sb.ToString();
        }

        public static IEnumerable<List<NominoPoint>> FilterPieces(IEnumerable<List<NominoPoint>> Input)
        {
            HashSet<String> FilteredPieces = new HashSet<string>();
            List<List<NominoPoint>> ProcessedList = new List<List<NominoPoint>>();
            foreach (var iteratefilter in Input)
            {

                if (ProcessedList.Any((p) => IsEqual(p, iteratefilter))) continue;
                //ProcessedList.Add(iteratefilter);
                var CW1 = RotateCW(iteratefilter);
                var CW2 = RotateCW(CW1);
                var CW3 = RotateCW(CW2);
                ProcessedList.AddRange(new[] { iteratefilter, CW1, CW2, CW3 });
                var GetStr = GetDirectionString(iteratefilter);
                if (!FilteredPieces.Contains(GetStr))
                {
                    FilteredPieces.Add(GetStr);
                    yield return iteratefilter;
                }

            }
        }
        public static IEnumerable<List<NominoPoint>> GetPieces(int BlockCount,List<NominoPoint> CurrentBuild = null)
        {
            if (CurrentBuild == null)
            {
                
                CurrentBuild = new List<NominoPoint>() {new NominoPoint(0, 0), new NominoPoint(1, 0) }; //create first two blocks
                var subreturn = GetPieces(BlockCount - 2, CurrentBuild); //-2 since we added two blocks.
                foreach (var yieldit in subreturn)
                {
                    yield return yieldit;
                }
            }
            else
            {
                //determine forward direction. There should be at least two tuples in the list.
                var Last = CurrentBuild[CurrentBuild.Count - 1];
                var NextToLast = CurrentBuild[CurrentBuild.Count - 2];
                var Direction = new NominoPoint(Last.X - NextToLast.X, Last.Y - NextToLast.Y);
                //Create three copies of the current List.
                List<NominoPoint>[] Copies = new List<NominoPoint>[] {new List<NominoPoint>(), new List<NominoPoint>(), new List<NominoPoint>() };
                for (int i = 0; i < CurrentBuild.Count; i++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        Copies[a].Add(CurrentBuild[i]);
                    }
                }

                //copies established. index zero is left (-y,x), 1 is forward (x,y) and 2 is right (y,-x)

                List<NominoPoint> LeftwardList = Copies[0];
                List<NominoPoint> ForwardList = Copies[1];
                List<NominoPoint> RightwardList = Copies[2];

                //what is the coordinate if we move leftward (-Y,X)
                NominoPoint LeftMove = new NominoPoint(Last.X - Direction.Y, Last.Y + Direction.X);
                NominoPoint ForwardMove = new NominoPoint(Last.X + Direction.X, Last.Y + Direction.Y);
                NominoPoint RightwardMove = new NominoPoint(Last.X + Direction.Y, Last.Y - Direction.X);

                if (!LeftwardList.Contains(LeftMove))
                {
                    LeftwardList.Add(LeftMove);
                    if (BlockCount-1 > 0)
                    {
                        var LeftResult = GetPieces(BlockCount - 1, LeftwardList);
                        foreach (var iterate in LeftResult)
                        {
                            yield return iterate;
                        }
                    }
                    else
                    {
                        yield return LeftwardList;
                    }
                }
                if (!ForwardList.Contains(ForwardMove))
                {
                    ForwardList.Add(ForwardMove);
                    if (BlockCount-1 > 0)
                    {
                        var ForwardResult = GetPieces(BlockCount - 1, ForwardList);
                        foreach (var iterate in ForwardResult)
                        {
                            yield return iterate;
                        }
                    }
                    else
                    {
                        yield return ForwardList;
                    }
                }
                if (!RightwardList.Contains(RightwardMove))
                {
                    RightwardList.Add(RightwardMove);
                    if (BlockCount-1 > 0)
                    {
                        var RightResult = GetPieces(BlockCount-1, RightwardList);
                        foreach (var iterate in RightResult)
                        {
                            yield return iterate;
                        }
                    }
                    else
                    {
                        yield return RightwardList;
                    }
                }

            }

            
        }
    }
}
