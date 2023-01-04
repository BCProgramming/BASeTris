using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public abstract class BlockGroupChooser : IDisposable
    {
        /// <summary>
        /// array of Nomino-producing functions. Things like tetris for example provide a function for each Tetromino type. Dr.Mario just gives back a Duomino.
        /// </summary>
        /// 

        protected Func<Nomino>[] _Available;
        protected Random rgen = null;
        /// <summary>
        /// Action delegate that is called that can make additional changes to chosen Nomino's.
        /// used by things like the Dr.Mario handler to change the colours of the blocks in the pill.
        /// </summary>
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
        public static Type ChooserTypeFromString(String strName)
        {
            if (strName == "Default") return typeof(BagChooser);
            foreach (var iteratetype in Program.DITypes[typeof(BlockGroupChooser)].GetManagedTypes())
            {

                if (String.Equals(iteratetype.Name, strName, StringComparison.OrdinalIgnoreCase))
                    return iteratetype;

            }
            return null;
        }
        internal abstract Nomino GetNext();
    }
}