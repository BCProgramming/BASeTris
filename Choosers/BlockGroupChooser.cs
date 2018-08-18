using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public abstract class BlockGroupChooser : IDisposable
    {
        protected Func<BlockGroup>[] _Available;
        protected Random rgen = null;

        public BlockGroupChooser(Func<BlockGroup>[] pAvailable)
        {
            _Available = pAvailable;
            rgen = new Random();
        }

        public virtual void Dispose()
        {
        }

        public BlockGroupChooser(Func<BlockGroup>[] pAvailable, int Seed)
        {
            _Available = pAvailable;
            rgen = new Random(Seed);
        }

        public IEnumerable<BlockGroup> GetEnumerator()
        {
            while (true)
            {
                yield return GetNext();
            }
        }

        public abstract BlockGroup GetNext();
    }
}