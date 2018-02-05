using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public abstract class BlockGroupChooser
    {
        public abstract void SetOptions(Func<BlockGroup>[] pAvailable);

        public IEnumerable<BlockGroup> GetEnumerator()
        {
            while(true)
            {
                yield return GetNext();
            }
        }

        public abstract BlockGroup GetNext();

    }
   
}
