﻿using BASeTris.Blocks;
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
            int MinX = 0, MinY = 0;
            foreach (var iterate in input)
            {
                if (iterate.X < MinX) MinX = iterate.X;
                if (iterate.Y < MinY) MinY = iterate.Y;
            }
            return (from n in input select new NominoPoint(n.X - MinX, n.Y - MinY)).ToList();
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
        public static List<NominoPoint> RotateCW(List<NominoPoint> input)
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
        public static IEnumerable<List<NominoPoint>> FilterRotations(IEnumerable<List<NominoPoint>> Input, Dictionary<String, List<NominoPoint>> LookupTable)
        {
            if (LookupTable == null) LookupTable = new Dictionary<string, List<NominoPoint>>();
            foreach (var filteritem in Input)
            {
                String CurrentRotation = StringRepresentation(filteritem);

                if (LookupTable.ContainsKey(CurrentRotation))
                    yield return LookupTable[CurrentRotation];
                else
                {
                    String[] OtherRotations = GetOtherRotationStrings(filteritem);
                    var stringrotations = new String[] { CurrentRotation }.Concat(OtherRotations);
                    foreach (var setstrings in stringrotations)
                    {
                        if(!LookupTable.ContainsKey(setstrings))
                            LookupTable.Add(setstrings, filteritem);
                    }
                    yield return filteritem;
                }

                



            }
        }
        public static IEnumerable<List<NominoPoint>> FilterPieces(IEnumerable<List<NominoPoint>> Input)
        {
            HashSet<String> FilteredPieces = new HashSet<string>();
            List<List<NominoPoint>> ProcessedList = new List<List<NominoPoint>>();
            HashSet<String> PreviouslyProcessed = new HashSet<string>();
            foreach (var iteratefilter in Input)
            {
                String DebugAid = StringRepresentation(iteratefilter);
                String sHash = DebugAid;
                if (PreviouslyProcessed.Contains(sHash))
                {
                    continue;
                }
                var CW1 = RotateCW(iteratefilter);
                var CW2 = RotateCW(CW1);
                var CW3 = RotateCW(CW2);
                PreviouslyProcessed.Add(sHash);
                PreviouslyProcessed.Add(StringRepresentation(CW1));
                PreviouslyProcessed.Add(StringRepresentation(CW2));
                PreviouslyProcessed.Add(StringRepresentation(CW3));

                yield return iteratefilter;
            }
        }
        //the "String Representation" is an effective Hash. Of course the Hash only applies to one rotation, so really, a particular Point Set has three Hashes. One Hash can easily be used for any indexing, but testing against the Hash needs to check all three.
        //or rather: if we handle this in the "generator" that creates tetrominoes, it can simply keep track of those it has already generated, and their three hashes. If it tries to give back one that has a hash that matches any of them, it can just give back the original instead.
        private static String[] GetHashPointSet(List<NominoPoint> input)
        {

            var inputCW = RotateCW(input);
            var inputCW2 = RotateCW(inputCW);
            var inputCW3 = RotateCW(inputCW2);

            return (from j in new[] { inputCW, inputCW2, inputCW3 } select StringRepresentation(j)).ToArray();
        }
        private static String GetStringForNomino(Nomino src)
        {
            return StringRepresentation(GetNominoPoints(src));
        }
        private static String[] GetStringsForNomino(Nomino src)
        {
            return GetHashPointSet(GetNominoPoints(src));

        }
        public static String[] GetOtherRotationStrings(List<NominoPoint> Source)
        {
            var np1 = NNominoGenerator.RotateCW(Source);
            var np2 = NNominoGenerator.RotateCW(np1);
            var np3 = NNominoGenerator.RotateCW(np2);
            return new string[] { StringRepresentation(np1), StringRepresentation(np2), StringRepresentation(np3) };
        }
        public static String[] GetOtherRotationStrings(Nomino Source)
        {
            var np = NNominoGenerator.GetNominoPoints(Source);
            var np1 = NNominoGenerator.RotateCW(np);
            var np2 = NNominoGenerator.RotateCW(np1);
            var np3 = NNominoGenerator.RotateCW(np2);

            return new string[] { StringRepresentation(np1), StringRepresentation(np2), StringRepresentation(np3) };

        }
        public static List<NominoPoint> GetNominoPoints(Nomino src)
        {

            List<NominoPoint> result = new List<NominoPoint>();
            foreach (var iterate in src)
            {
                NominoPoint np = new NominoPoint(iterate.X, iterate.Y);
                result.Add(np);
            }

            return ResetTranslation(result);
        }
        
        public static Nomino CreateNomino(List<NominoPoint> src)
        {

            
            src =NNominoGenerator.ResetTranslation(src);
            int MaxX = 0;
            int MaxY = 0;
            foreach (var iterate in src)
            {
                if (Math.Abs(iterate.X) > MaxX) MaxX = Math.Abs(iterate.X);
                if (Math.Abs(iterate.Y) > MaxY) MaxY = Math.Abs(iterate.Y);
            }
            List<NominoElement> Elements = new List<NominoElement>();
            foreach (var iterate in src)
            {
                
                NominoElement ne = new NominoElement(new System.Drawing.Point(iterate.X, iterate.Y), new System.Drawing.Size(MaxX, MaxY), new StandardColouredBlock());
                Elements.Add(ne);
                
            }
            Nomino buildresult = new Nomino(Elements);
            return buildresult;
        }
        private static Dictionary<String, List<NominoPoint>> CachedRotationResults = new Dictionary<string, List<NominoPoint>>();
        public static List<NominoPoint> GetPiece(int BlockCount)
        {
            
            return FilterRotations(FilterPieces(GetPieces(BlockCount, null, NominoPieceGenerationFlags.Flag_Randomize)),CachedRotationResults).FirstOrDefault();
        }
        [Flags]
        public enum NominoPieceGenerationFlags
        {
            Flag_None = 0,
            Flag_Randomize = 1
        }
        public static IEnumerable<List<NominoPoint>> GetPieces(int BlockCount,List<NominoPoint> CurrentBuild = null,NominoPieceGenerationFlags  GenerationFlags = NominoPieceGenerationFlags.Flag_None)
        {
            if (CurrentBuild == null)
            {
                
                CurrentBuild = new List<NominoPoint>() {new NominoPoint(0, 0), new NominoPoint(1, 0) }; //create first two blocks
                var subreturn = GetPieces(BlockCount - 2, CurrentBuild,GenerationFlags); //-2 since we added two blocks.
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
                List<NominoPoint>[] DirectionLists = new List<NominoPoint>[] {new List<NominoPoint>(), new List<NominoPoint>(), new List<NominoPoint>() };
                for (int i = 0; i < CurrentBuild.Count; i++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        DirectionLists[a].Add(CurrentBuild[i]);
                    }
                }

                //copies established. index zero is left (-y,x), 1 is forward (x,y) and 2 is right (y,-x)

                List<NominoPoint> LeftwardList = DirectionLists[0];
                List<NominoPoint> ForwardList = DirectionLists[1];
                List<NominoPoint> RightwardList = DirectionLists[2];

                //what is the coordinate if we move leftward (-Y,X)
                NominoPoint LeftMove = new NominoPoint(Last.X - Direction.Y, Last.Y + Direction.X);
                NominoPoint ForwardMove = new NominoPoint(Last.X + Direction.X, Last.Y + Direction.Y);
                NominoPoint RightwardMove = new NominoPoint(Last.X + Direction.Y, Last.Y - Direction.X);

                List<NominoPoint> MoveList = new List<NominoPoint>() { LeftMove, ForwardMove, RightwardMove };
                int[] ArrayOrder = new int[] { 0, 1, 2 };
                if (GenerationFlags.HasFlag(NominoPieceGenerationFlags.Flag_Randomize)) ArrayOrder = TetrisGame.Shuffle(ArrayOrder).ToArray();
                foreach (int index in ArrayOrder)
                {
                    
                    if (!DirectionLists[index].Contains(MoveList[index]))
                    {
                        DirectionLists[index].Add(MoveList[index]);
                        if (BlockCount - 1 > 0)
                        {
                            var Currresult = GetPieces(BlockCount - 1, DirectionLists[index],GenerationFlags);
                            foreach (var iterate in Currresult)
                                yield return iterate;
                        }
                        else
                        {
                            yield return DirectionLists[index];
                        }
                    }

                }

            }
            
        }
    }
}