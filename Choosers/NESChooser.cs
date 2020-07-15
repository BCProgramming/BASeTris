using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class NESChooser : BlockGroupChooser
    {
        /*public class NintendoRandomizer extends Randomizer {

	int prev;
	int roll;

	public NintendoRandomizer() {
		super();
	}

	public NintendoRandomizer(boolean[] pieceEnable, long seed) {
		super(pieceEnable, seed);
	}

	public void init() {
		prev = pieces.length;
		roll = pieces.length+1;
	}

	public int next() {
		int id = r.nextInt(roll);
		if (id == prev || id == pieces.length) {
			id = r.nextInt(pieces.length);
		}
		prev = id;
		return pieces[id];
	}

}*/
        int prev, roll;

        public NESChooser(Func<Nomino>[] pAvailable) : base(pAvailable)
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