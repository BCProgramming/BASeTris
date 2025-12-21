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
        internal override Func<Nomino> GetNextFunc()
        {
            //retrieve the next item and increment the index.
            Nomino retrieveresult = GenerationList[Index];
            Index = (Index + 1) % GenerationList.Count;
            return ()=>retrieveresult;
        }
    }

    public abstract class BlockGroupChooser : IDisposable
    {
        /// <summary>
        /// array of Nomino-producing functions. Things like tetris for example provide a function for each Tetromino type. Dr.Mario just gives back a Duomino.
        /// </summary>
        
        //This likely defies an expectation here, which would be that the bag is selecting from Minos themselves.
        //The reason for the use of functions is for situations such as larger Minos, where trying to actually generate every possible Mino would require a long time and way too much memory.
        //The downside is that in many ways unless we can figure out a reliable way of indexing those polyminoes we are sort of limited to the randomization process that selects them and cannot make good use of the choosers for those
        //game modes.
        public List<Nomino> AllGeneratedNominos = new List<Nomino>();
        protected Func<Nomino>[] _Available;
        public IRandomizer rgen = null;
        private int MaxHistorySize = 4;
        protected Queue<Func<Nomino>> PieceHistory = new Queue<Func<Nomino>>();

        public void SetHistory(Func<Nomino>[] Values)
        {
            PieceHistory.Clear();
            foreach (var enqueueitem in Values)
            {
                PieceHistory.Enqueue(enqueueitem);
            }
        }
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
        internal abstract Func<Nomino> GetNextFunc();
        public Nomino GetNext()
        {
            var usefunction = GetNextFunc();
            PieceHistory.Enqueue(usefunction);
            while (PieceHistory.Count > MaxHistorySize)
            {
                PieceHistory.Dequeue();
            }
            return usefunction();
        }
    }
}