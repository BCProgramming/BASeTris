using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class FullRandomChooser : BlockGroupChooser
    {
        private Func<BlockGroup>[] Options = null;
        private Random rgen;
        public FullRandomChooser(Random rgenerator,Func<BlockGroup>[] SelectionFunctions)
        {
            rgen = rgenerator;
            SetOptions(SelectionFunctions);
        }

        public override void SetOptions(Func<BlockGroup>[] pAvailable)
        {
            Options = pAvailable;
        }

        public override BlockGroup GetNext()
        {
            int RandomIndex = rgen.Next(Options.Length);
            return Options[RandomIndex]();
        }
    }
}
