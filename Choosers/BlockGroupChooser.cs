using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{

    public class GeneratedChooser : BlockGroupChooser
    {
        private IList<Nomino> GenerationList = null;
        private int Index = 0;
        public GeneratedChooser(IList<Nomino> PregeneratedItems):base(null,0)
        {
            GenerationList = PregeneratedItems;
            Index = 0;
                
        }
        protected override Nomino GetNext()
        {
            //retrieve the next item and increment the index.
            Nomino retrieveresult = GenerationList[Index];
            Index = (Index + 1) % GenerationList.Count;
            return retrieveresult;
        }
    }

    public abstract class BlockGroupChooser : IDisposable
    {
        /// <summary>
        /// array of Nomino-producing functions. Things like tetris for example provide a function for each Tetromino type. Dr.Mario just gives back a Duomino.
        /// </summary>
        /// 
        public List<Nomino> AllGeneratedNominos = new List<Nomino>();
        protected Func<Nomino>[] _Available;
        public IRandomizer rgen = null;
        /// <summary>
        /// Action delegate that is called that can make additional changes to chosen Nomino's.
        /// used by things like the Dr.Mario handler to change the colours of the blocks in the pill.
        /// </summary>
        public Action<BlockGroupChooser,Nomino> ResultAffector { get; set; } = null;
        private BlockGroupChooser(Func<Nomino>[] pAvailable)
        {
            _Available = pAvailable;
            rgen = RandomHelpers.Construct();
        }

        public virtual void Dispose()
        {
        }

        public BlockGroupChooser(Func<Nomino>[] pAvailable, int Seed)
        {
            _Available = pAvailable;
            rgen = RandomHelpers.Construct(Seed);
        }

        public IEnumerable<Nomino> GetEnumerator()
        {
            while (true)
            {
                yield return GetNext();
            }
        }
        public Nomino RetrieveNext()
        {
            var result = GetNext();
            if (ResultAffector != null) ResultAffector(this,result);
            AllGeneratedNominos.Add(result);
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
        protected abstract Nomino GetNext();
    }
}