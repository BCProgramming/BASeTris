using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public abstract class BlockGroupChooser : IDisposable
    {
        protected Func<Nomino>[] _Available;
        protected Random rgen = null;
        public Action<Nomino> ResultAffector { get; set; } = null;
        public BlockGroupChooser(Func<Nomino>[] pAvailable)
        {
            _Available = pAvailable;
            rgen = new Random();
        }

        public virtual void Dispose()
        {
        }

        public BlockGroupChooser(Func<Nomino>[] pAvailable, int Seed)
        {
            _Available = pAvailable;
            rgen = new Random(Seed);
        }

        public IEnumerable<Nomino> GetEnumerator()
        {
            while (true)
            {
                yield return GetNext();
            }
        }
        public virtual Nomino RetrieveNext()
        {
            var result = GetNext();
            if (ResultAffector != null) ResultAffector(result);
            return result;
        }
        
        protected abstract Nomino GetNext();
    }
}