﻿using BASeTris.Blocks;
using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            public NominoElement Source;
            public int  X;
            public int Y;
            public NominoPoint(int pX, int pY, NominoElement pSource = null)
            {
                X = pX;
                Y = pY;
                Source = pSource;
                    
            }
            public static NominoPoint Create(int pX, int pY,NominoElement pSource = null)
            {
                return new NominoPoint(pX, pY,pSource);
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
                return X == np.X && Y == np.Y && Source == np.Source;
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


        private static readonly char[] BaseChars =
         "LFR".ToCharArray();
        private static readonly Dictionary<char, int> CharValues = BaseChars
                   .Select((c, i) => new { Char = c, Index = i })
                   .ToDictionary(c => c.Char, c => c.Index);

        private static string IndexToDirectionString(long value, long index)
        {
            long targetBase = BaseChars.Length;
            // Determine exact number of characters to use.
            char[] buffer = new char[Math.Max(
                       (int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = buffer.Length;
            do
            {
                buffer[--i] = BaseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            return new string(buffer, i, buffer.Length - i);
        }

        private static long DirectionStringToIndex(string DirectionString)
        {
            char[] chrs = DirectionString.ToCharArray();
            int m = chrs.Length - 1;
            int n = BaseChars.Length, x;
            long result = 0;
            for (int i = 0; i < chrs.Length; i++)
            {
                x = CharValues[chrs[i]];
                result += x * (long)Math.Pow(n, m--);
            }
            return result;
        }
        public static long GetIndex(List<NominoPoint> Input)
        {
            return DirectionStringToIndex(GetDirectionString(Input));
            
        }
        //getIndex function. By getting the directionstring, we can basically treat it like a trinary number:
        //Left is 0
        //Forward is 1
        //Right is 2

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
            return (from n in input select new NominoPoint(n.X - MinX, n.Y - MinY,n.Source)).ToList();
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
            BuildList = (from p in input select new NominoPoint(p.Y, -p.X,p.Source)).ToList();
            return ResetTranslation(BuildList);

        }
        
        static System.Text.RegularExpressions.Regex ReplaceParens = new System.Text.RegularExpressions.Regex("\\(.*\\)", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        public static IEnumerable<NominoPoint> FromString(String src)
        {
            //because of the change to allow additional data for each item, we need to strip out that extra info in parenthesis using regex.
            if(src.IndexOf("(") > -1)
                src = ReplaceParens.Replace(src, "");


            String[] linesplit = src.Split('\n');
            for (int currrow = 0; currrow < linesplit.Length; currrow++)
            {
                for (int currcol = 0; currcol < linesplit[currrow].Length; currcol++)
                {
                    if (linesplit[currrow][currcol] != ' ')
                    {
                        NominoPoint np = new NominoPoint(currcol, currrow);
                        yield return np;
                    }
                }
            }
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
                    var gotpoint =  Points.Any((p) => (p.X == xcoord && p.Y == ycoord));
                    if (gotpoint)
                    {
                        var getPoint = Points.First((p) => (p.X == xcoord && p.Y == ycoord));
                        sb.Append("#");
                        if (getPoint.Source != null && getPoint.Source.Block is LineSeriesBlock lsb)
                        {
                            sb.Append("(" + lsb.CombiningIndex + ")");
                        }
                    }
                    else
                        sb.Append(" ");
                }
                sb.AppendLine("");
            }


            return sb.ToString();
        }
        Dictionary<char, int[][]> SimpleCharacters = new Dictionary<char, int[][]>()
        {
            {'A',
                new int[][]{
                    new int []{0,1,0 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'B',
                new int[][]{
                    new int []{0,1,1 },
                    new int []{1,0,1 },
                    new int []{1,0,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'C',
                new int[][]{
                    new int []{0,1,1 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{0,1,1 },
                } },
                {'D',
                new int[][]{
                    new int []{1,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,0 },
                } },
                {'E',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,0 },
                    new int []{1,1,0 },
                    new int []{1,0,0 },
                    new int []{1,1,1 },
                } },
                {'F',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,0 },
                    new int []{1,1,0 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                } },
                {'G',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'H',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'I',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{1,1,1 },
                } },
                {'J',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{1,0,0 },
                } },
                {'K',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'L',
                new int[][]{
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                    new int []{1,1,1 },
                } },
                {'M',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'N',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,1,1 },
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'O',
                new int[][]{
                    new int []{0,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{0,1,0 },
                } },
                {'P',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{1,0,0 },
                    new int []{1,0,0 },
                } },
                {'Q',
                new int[][]{
                    new int []{0,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{0,1,1 },
                } },
                {'R',
                new int[][]{
                    new int []{1,1,0 },
                    new int []{1,0,1 },
                    new int []{1,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'S',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,0 },
                    new int []{1,1,1 },
                    new int []{0,0,1 },
                    new int []{1,1,1 },
                } },
                {'T',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                } },
                {'U',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                } },
                {'V',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{0,1,0 },
                } },
                {'W',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                } },
                {'X',
                new int[][]{
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                    new int []{0,1,0 },
                    new int []{1,0,1 },
                    new int []{1,0,1 },
                } },
                {'Y',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{1,0,1 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                    new int []{0,1,0 },
                } },
                {'Z',
                new int[][]{
                    new int []{1,1,1 },
                    new int []{0,0,1 },
                    new int []{0,1,0 },
                    new int []{1,0,0 },
                    new int []{1,1,1 },
                } }

        };
        public static IEnumerable<NominoPoint> FromLetter(String src)
        {
            var useFont = new Font("Small Fonts", 4);
            SizeF MeasuredText = new SizeF();
            using (Bitmap b = new Bitmap(1, 1))
            {
                Graphics g = Graphics.FromImage(b);
                MeasuredText = g.MeasureString(src, useFont);
            }
            using (Bitmap buildg = new Bitmap((int)(Math.Ceiling(MeasuredText.Width)), (int)(Math.Ceiling(MeasuredText.Height))))
            {
                using (Graphics g = Graphics.FromImage(buildg))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    g.Clear(Color.Transparent);
                    g.DrawString(src, useFont, Brushes.Black, new PointF(0, 0));
                }

                for (int x = 0; x < buildg.Width; x++)
                {
                    for (int y = 0; y < buildg.Height; y++)
                    {
                        if (buildg.GetPixel(x, y).A > 0)
                        {
                            NominoPoint np = new NominoPoint(x, y);
                            yield return np;

                        }
                    }
                }


            }



        }
        public static Nomino NominoFromLetter(String src)
        {
            return CreateNomino(FromLetter(src).ToList());
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
        public static IEnumerable<List<NominoPoint>> FilterHolePieces(IEnumerable<List<NominoPoint>> Input)
        {

            return Input.Where((s) => !IsHolePiece(s));

        }
        public static bool IsHolePiece(List<NominoPoint> TestItem)
        {
            if (TestItem.Count <= 6) return false; //need at least 7 blocks to even form a hole
            //a "hole piece" is a piece where there is an enclosed hole. These might be undesirable.
            //first iterate through each position to build a HashSet of tuples.
            //while we are at it store max and min coordinates.
            HashSet<(int x, int y)> Occupied = new HashSet<(int x, int y)>();
            BCPointI MaxPos = BCPointI.MinValue;
            BCPointI MinPos = BCPointI.MaxValue;
            foreach (var position in TestItem)
            {
                Occupied.Add((position.X, position.Y));
                if (MaxPos.X < position.X) MaxPos.X = position.X;
                if (MaxPos.Y < position.Y) MaxPos.Y = position.Y;
                if (MinPos.X > position.X) MinPos.X = position.X;
                if (MinPos.Y > position.Y) MinPos.Y = position.Y;
            }
            HashSet<(int x, int y)> ProcessPos = new HashSet<(int x, int y)>();
            bool HoleTest = false;
            for (int x = MinPos.X; x <= MaxPos.X; x++)
            {
                for (int y = MinPos.Y; y <= MaxPos.Y; y++)
                {
                    HoleTest = (IsHolePosition((x, y), Occupied, ProcessPos, MinPos, MaxPos));
                    if (HoleTest) return true;
                        
                }
            }
            return false;


        }
        private static bool IsHolePosition((int x, int y) Coordinates, HashSet<(int x, int y)> NominoSpots, HashSet<(int x, int y)> Processpositions,BCPointI MinPos,BCPointI MaxPos, HashSet<(int x, int y)> IgnorePositions = null )
        {
            IgnorePositions = IgnorePositions ?? new HashSet<(int x, int y)>();
            //a hole position is a position that:
            //= is surrounded on all four sides not present in processedpositions by either occupied coordinates, or by empty positions that are a hole position.
            //any blank position at the edge of the minimum/maximum positions is not a hole position.

            if (NominoSpots.Contains(Coordinates)) return false;
            //check if we're on the edge.
            if (Coordinates.x == MinPos.X || Coordinates.y == MinPos.Y || Coordinates.x == MaxPos.X || Coordinates.y == MaxPos.Y)
            {
                Processpositions.Add(Coordinates);
                return false;
            }
            else
            {
                (int x, int y)[] OffsetsArray = new (int x, int y)[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
                

                IgnorePositions.Add(Coordinates);

                foreach (var checkoffset in OffsetsArray)
                {
                    
                    var CheckCoordinate = (Coordinates.x + checkoffset.x, Coordinates.y + checkoffset.y);
                    if (IgnorePositions.Contains(CheckCoordinate)) continue; //disregard to prevent recursion.
                    if (NominoSpots.Contains(CheckCoordinate)) continue; //if this is a block in the mino, the also continue.
                    //at this point we know this coordinate is not occupied. Check if it can reach the edge of the Mino.
                    if (!IsHolePosition(CheckCoordinate, NominoSpots, Processpositions, MinPos, MaxPos, IgnorePositions)) return false;
                    

                }



            }


            return true;

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
                NominoPoint np = new NominoPoint(iterate.X, iterate.Y,iterate);
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

        public static List<NominoPoint>? GetPiece(int BlockCount,IRandomizer rgen)
        {
            
            return FilterRotations(FilterPieces(GetPieces(BlockCount,NominoPieceGenerationFlags.Flag_Randomize,rgen)),CachedRotationResults).FirstOrDefault();
        }
        [Flags]
        public enum NominoPieceGenerationFlags
        {
            Flag_None = 0,
            Flag_Randomize = 1
        }


        //"New" mini generation code.
        // To generate all piece of size N:
        // Take all peices of Size N-1.
        // For all block positions, add a block to every free square adjacent to that piece.
        // This is the generated Mino. If a duplicate piece has not already been generated (based on the rotation hash checks we already have), than it is a valid result.
        // Mino if size 1 is the end case, of course.

        //The selection can be randomized of course by randomizing the selection of the block to add onto and which of the available adjacent cells to add.

        //TODO: we can have a sequence number that dictates a specific Nomino, 
        public static IEnumerable<List<NominoPoint>> GetPieces(int BlockCount, NominoPieceGenerationFlags GenerationFlags = NominoPieceGenerationFlags.Flag_None, IRandomizer? rgen = null)
        {
            if (BlockCount == 1)
            {
                //End case. block count of 1 is just, well, one block.
                yield return new List<NominoPoint>() { new NominoPoint(0, 0, null) };
            }
            else
            {
                //for all other counts, grab all pieces from the N-1...
                var LowerSet = FilterRotations(GetPieces(BlockCount - 1, GenerationFlags, rgen), null);

                foreach (var Npiece in LowerSet)
                {
                    //first let's create a HashSet of the x/y tuples.
                    HashSet<(int, int)> Positions = new HashSet<(int, int)>();
                    foreach (var block in Npiece)
                    {
                        Positions.Add((block.X, block.Y));
                    }
                    //now, iterate through each block in the piece.
                    (int x, int y)[] OffsetsArray = new (int x, int y)[] { (-1, 0), (0, 1), (1, 0), (0, -1) };

                    var iteratelist = (GenerationFlags == NominoPieceGenerationFlags.Flag_Randomize) ?RandomHelpers.Static.Shuffle(Npiece, rgen): Npiece;
                    foreach (var block in iteratelist)
                    {
                        //if set to randomize, randomize the offset order for this block's sub piece choices.
                        if (GenerationFlags == NominoPieceGenerationFlags.Flag_Randomize) OffsetsArray = RandomHelpers.Static.Shuffle(OffsetsArray, rgen).ToArray();
                        foreach (var offsetuse in OffsetsArray)
                        {
                            int useX = block.X + offsetuse.x;
                            int useY = block.Y + offsetuse.y;
                            //check if this coordinate is in the Positions hash
                            if (!Positions.Contains((useX, useY)))
                            {
                                NominoPoint Append = new NominoPoint(useX, useY);
                                //yield the lowerset with this point stapled on.
                                yield return Npiece.Concat(new[] { Append }).ToList();
                            }
                        }
                    }
                }

            }
        }
        


        //--a revised Nomino Builder Algorithm--
        //-Basic algorithm is similar to the Logo approach of the existing GetPieces method.
        //-we need two new things. First is a way of dictating that we are to branch in multiple directions from a particular position.
        //-the other aspect is that we either need a way of indicating "how much" that part should branch (how many blocks for each one). If we only do half for each or by roughly three, then we won't be able to generate "all possibilities". 

        //we'd still be better off, arguably, than the previous implementation, however.

        //alternative idea:
        //If there is more than 1 block left to generate, in addition to generating as it does now in one direction, also recursively generate



        //This algorithm has a flaw, in that it won't be able to generate "branched" pieces. For example, the T piece in Tetris would not be generated through this.
        //Adding this capability is beyond the details of this algorithm, however, so we'd need to effectively reimplement this routine.
        //Effectively we'd need a way to indicate multiple directions for the same path. I'd suggest a new "3" place value which indicate that, so 23132 would mean to move Right, then branch both Forward (1) and right from that position recursively.


        public static IEnumerable<List<NominoPoint>> GetPieces_Snake(int BlockCount,List<NominoPoint> CurrentBuild = null,NominoPieceGenerationFlags  GenerationFlags = NominoPieceGenerationFlags.Flag_None,IRandomizer rgen = null)
        {
            rgen = rgen ?? RandomHelpers.Construct();
            if (CurrentBuild == null) //Starting case, we make two blocks, then recursively call to add to it if needed.
            {
                
                CurrentBuild = new List<NominoPoint>() {new NominoPoint(0, 0), new NominoPoint(1, 0) }; //create first two blocks
                var subreturn = GetPieces_Snake(BlockCount - 2, CurrentBuild,GenerationFlags); //-2 since we added two blocks.
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
                //if set to randomize, randomize the order of the direction
                if (GenerationFlags.HasFlag(NominoPieceGenerationFlags.Flag_Randomize)) ArrayOrder = RandomHelpers.Static.Shuffle(ArrayOrder,rgen).ToArray();
                foreach (int index in ArrayOrder)
                {
                    
                    if (!DirectionLists[index].Contains(MoveList[index]))
                    {
                        DirectionLists[index].Add(MoveList[index]);
                        if (BlockCount - 1 > 0)
                        {
                            var Currresult = GetPieces_Snake(BlockCount - 1, DirectionLists[index],GenerationFlags);
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
        
        public static Nomino CreateColumnNomino(int size,Func<NominoBlock> generateblockfunc)
        {
            //For Columns, we are able to "exploit" the design of the BlockGroups, whereby "rotations" are actually just generalized.
            ////Instead of setting rotation positions such that they actually rotate, we just have them move positions within the column. We can also have only three states, and "rotate" 
            //goes between them. 
            List<NominoElement> items = new List<NominoElement>();
            var rotpoints = Enumerable.Range(0, size).Select((w) => new Point(0, w)).ToArray();
            for (int y = 0; y < size; y++)
            {
                Point[] usepart = new Point[rotpoints.Length];
                int startposition = y;
                for (int i = 0; i < rotpoints.Length; i++)
                {
                    var currindex = ((startposition + i) % rotpoints.Length);
                    usepart[i] = rotpoints[currindex];

                }
                NominoElement makeelement = new NominoElement(usepart, generateblockfunc());
                
                
                items.Add(makeelement);

            }
            return new Nomino(items);

        }


        public static T GetNominoData<T>(Dictionary<string, T> Source, Nomino Group, Func<T> ChooseValueFunc)
        {
            var cw = GetNominoPoints(Group);
            String sHash = StringRepresentation(cw);
            if (!Source.ContainsKey(sHash))
            {
                T chosenresult = ChooseValueFunc();
                var cw2 = RotateCW(cw);
                var cw3 = RotateCW(cw2);
                var cw4 = RotateCW(cw3);
                Source[sHash] = chosenresult;
                Source[StringRepresentation(cw2)] = chosenresult;
                Source[StringRepresentation(cw3)] = chosenresult;
                Source[StringRepresentation(cw4)] = chosenresult;
            }
            return Source[sHash];

        }


        //related concept: "backpacking" pieces onto a set size grid.
        //A recursive algorithm might work here. We can start with an algorithm that accepts all possible Minos that it wants to tile.

        //Take a piece- (at random perhaps?) and try to place place it in each unoccupied position at a random location on the grid where it fits.
        //If it doesn't fit, try each rotation. if it still doesn't fit try a different piece, and start over searching. If we fail to find a place to position any available piece at any possible rotation, we have failed.
        //If it does fit, we now create a a copy of the field, and recursively call to try to fill in the rest of the field the same way.
        //If the recursive call succeeds, we're done and have a tiled representation that fills the grid.
        //otherwise, we go back to step one and proceed with choosing another position, rotation, or piece.
        



    }
}
