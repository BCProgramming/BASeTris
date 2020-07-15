using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;

namespace BASeTris
{
    public class NominoBuilder
    {

        static T[][] CopyArrayBuiltIn<T>(T[][] source)
        {
            var len = source.Length;
            var dest = new T[len][];

            for (var x = 0; x < len; x++)
            {
                var inner = source[x];
                var ilen = inner.Length;
                var newer = new T[ilen];
                Array.Copy(inner, newer, ilen);
                dest[x] = newer;
            }

            return dest;
        }

        bool[][] NominoMatrix = null;
        public static String NominoToString(bool[][] input)
        {
            StringBuilder buildresult = new StringBuilder();
            foreach(var row in input)
            {
                foreach(var col in row)
                {
                    buildresult.Append(col ? "#" : ".");
                }
                buildresult.AppendLine();
            }
            return buildresult.ToString();
        }
        
        public static IEnumerable<bool[][]> BuildNominoes(int BlockCount,bool[][] BuildingNomino = null, int XPos = 0, int YPos = 0)
        {
            //start case:
            //if our building nomino is null, create a new one.
            if (BuildingNomino == null)
            {
                BuildingNomino = new bool[BlockCount][];
                for (int bn= 0; bn < BlockCount; bn++)
                {
                    BuildingNomino[bn] = new bool[BlockCount];
                }
                //set the top left index...
                BuildingNomino[0][0] = true;
                var Duplicated = CopyArrayBuiltIn(BuildingNomino);
                //recursively call with the two possible directions.
                foreach(var iterate in BuildNominoes(BlockCount-1, Duplicated, 1,0))
                {
                    yield return iterate;
                }
                foreach(var iterate in BuildNominoes(BlockCount-1, Duplicated, 0,1))
                {
                    yield return iterate;
                }
                //that actually covers all possibilities...
            }
            else
            {
                //otherwise:
                //if BblockCount is 0, we are done. set the specified index, and yield the completed nomino matrix.
                if (BlockCount == 0)
                {
                    BuildingNomino[XPos][YPos] = true;
                    yield return BuildingNomino;
                    yield break;
                }
                else
                {
                    foreach (int xoffset in new int[] { -1, 0,1 })
                    {
                        int useX = XPos + xoffset;
                        if (useX < 0) continue;
                        foreach (int yoffset in new int[] { -1,0,1})
                        {
                            if ((xoffset == 0) ^ (yoffset == 0)) //only proceed if one or the other is zero- but not both or neither. (xor)
                            {
                                int useY = YPos + yoffset;
                                //since we make the matrix the "length" of the 'snake', we only need to check that we are < 0 since we cannot be long enough to hit either bottom or right side.

                                if (useY < 0) continue;
                                //if the given position is not occupied already

                                if (!BuildingNomino[useX][useY])
                                {
                                    //set the particular position...
                                    BuildingNomino[XPos][YPos] = true;
                                    var copied = CopyArrayBuiltIn(BuildingNomino);
                                    //now, recursively call for this particular arrangement...
                                    foreach (var iterate in BuildNominoes(BlockCount - 1, copied, useX, useY))
                                    {
                                        yield return iterate;
                                    }
                                }
                            }
                        }
                    }



                }
            }

        }

    }
}
