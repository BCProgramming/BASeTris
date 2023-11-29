using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class NTrisChooser : Choosers.SingleFunctionChooser
    {
        public int BlockCount { get; set; } = 4;
        public NTrisChooser(int pBlockCount,int pSeed) : base(null,pSeed)
        {
            _Available = new Func<Nomino>[] { ChooserFunction };
            BlockCount = pBlockCount;
        }
        public Nomino ChooserFunction()
        {
            var newpiece = NNominoGenerator.GetPiece(BlockCount);
            var buildNomino = NNominoGenerator.CreateNomino(newpiece);
            return buildNomino;
        }
    }
}
