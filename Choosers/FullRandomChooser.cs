using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class FullRandomChooser : BlockGroupChooser
    {
        public FullRandomChooser(Func<Nomino>[] SelectionFunctions) : base(SelectionFunctions)
        {
        }


        protected override Nomino GetNext()
        {
            int RandomIndex = rgen.Next(_Available.Length);
            return _Available[RandomIndex]();
        }
    }
}