using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class NESChooser : BlockGroupChooser
    {
        
        int prev, roll;

        protected NESChooser(Func<Nomino>[] pAvailable) : base(pAvailable,Environment.TickCount)
        {
        }

        public NESChooser(Func<Nomino>[] pAvailable, int Seed) : base(pAvailable, Seed)
        {
        }

        private bool IsInitialized = false;

        private void Init()
        {
            prev = _Available.Length;
            roll = _Available.Length + 1;
        }

        protected override Nomino GetNext()
        {
            int id = rgen.Next(roll);
            if (id == prev || id == _Available.Length)
            {
                id = rgen.Next(_Available.Length);
            }

            prev = id;
            return _Available.ElementAt(id)();
        }
    }
}