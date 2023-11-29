using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class GameBoyChooser : BlockGroupChooser
    {
        int id, roll;

        public GameBoyChooser(Func<Nomino>[] pSelectionFunctions,int pSeed) : base(pSelectionFunctions,pSeed)
        {
            id = rgen.Next(_Available.Length);
            roll = 6 * _Available.Length - 3;
        }

        internal override Nomino GetNext()
        {
            id = (id + (rgen.Next(roll) / 5) + 1) % _Available.Length;
            return _Available[id]();
        }
    }
}