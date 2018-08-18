using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.TetrisBlocks;

namespace BASeTris.FieldInitializers
{
    public class GarbageFieldInitializer : FieldInitializer
    {
        private int GarbageRows = 0;
        private Random rgen = null;
        private Func<int, int, TetrisBlock> GenerateBlock = null;
        private TetrominoTheme _Theme;

        private TetrisBlock DefaultGenerateBlock(int x, int y)
        {
            var standardfilled = new StandardColouredBlock();
            standardfilled.BlockColor = Color.FromArgb(rgen.Next(255), rgen.Next(255), rgen.Next(255));
            return standardfilled;
        }

        public GarbageFieldInitializer(Random prgen, TetrominoTheme pTheme, int NumRows)
        {
            rgen = prgen;
            GarbageRows = NumRows;
            GenerateBlock = DefaultGenerateBlock;
        }

        public override void Initialize(TetrisField Target)
        {
            if (GarbageRows > 0)
            {
                for (int i = 0; i < GarbageRows; i++)
                {
                    int CurrRow = Target.RowCount - i - 1;
                    var FillRow = Target.Contents[CurrRow];
                    for (int fillcol = 0; fillcol < Target.ColCount; fillcol++)
                    {
                        if (rgen.NextDouble() > 0.5)
                        {
                            FillRow[fillcol] = GenerateBlock(fillcol, CurrRow);
                        }
                    }
                }
            }
        }
    }
}