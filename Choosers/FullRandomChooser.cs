using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class FullRandomChooser : BlockGroupChooser
    {
        public FullRandomChooser(Func<Nomino>[] SelectionFunctions) : base(SelectionFunctions)
        {
        }


        internal override Nomino GetNext()
        {
            int RandomIndex = rgen.Next(_Available.Length);
            return _Available[RandomIndex]();
        }
    }
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class SequentialChooser : BlockGroupChooser
    {
        int currentindex = 0;
        public SequentialChooser(Func<Nomino>[] SelectionFunctions) : base(SelectionFunctions)
        {
        }


        internal override Nomino GetNext()
        {
            if (currentindex == _Available.Length) currentindex = 0;
            
            return _Available[currentindex++]();
            
        }
    }
}