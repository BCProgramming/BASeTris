using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class BasicBagChooser : BagChooser
    {
        public BasicBagChooser(Func<Nomino>[] SelectionFunctions, int Seed) : base(SelectionFunctions, Seed, 1)
        {
            HistoryReroll = false; //the basic bag chooser also disables the history reroll.
        }
        
    }


    [ChooserCompatibility(typeof(StandardTetrisHandler))]
    public class BagChooser : BlockGroupChooser
    {

        ItemBag<Func<Nomino>> WorkBag = null;

        protected bool HistoryReroll = true;
        //Queue<Func<Nomino>> WorkQueue = new Queue<Func<Nomino>>();

        private Func<Nomino>[] OriginalAvailable = null;
        private Dictionary<Func<Nomino>,int> GeneratedDistance = null;
        
        public static IEnumerable<T> Shuffle<T>(IRandomizer rgen, IEnumerable<T> Shufflethese)
        {
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                sl.Add((float) rgen.NextDouble(), iterate);
            }

            return sl.Select(iterator => iterator.Value);
        }

        public BagChooser(Func<Nomino>[] SelectionFunctions, int Seed,int pNumSets = 1) : base(SelectionFunctions,Seed)
        {
            OriginalAvailable = _Available;
            GeneratedDistance = new Dictionary<Func<Nomino>, int>();
            
            foreach (var iterate in OriginalAvailable)
            {
                GeneratedDistance.Add(iterate, 0);
            }
            //if we are to have multiple sets in the bag, duplicate the provided array of functions.
            if (pNumSets > 1)
            {
                _Available = DuplicateSet(_Available, pNumSets).ToArray();
                
            }
            WorkBag = new ItemBag<Func<Nomino>>(_Available);
        }
        private static IEnumerable<T> DuplicateSet<T>(IEnumerable<T> source, int pNumSets)
        {

            for (int i = 0; i < pNumSets; i++)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }
        /*private void RefillQueue()
        {
            var Shuffled = Shuffle(rgen, _Available).ToArray();
            for (int i = 0; i < Shuffled.Length; i++)
            {
                WorkQueue.Enqueue(Shuffled[i]);
            }
        }*/
        internal override Func<Nomino> GetNextFunc()
        {
            //if (WorkQueue.Count == 0) RefillQueue();
            var getresult = WorkBag.Take(rgen);
            //var getresult = WorkQueue.Dequeue();
            int retrycount = 0;
            while (HistoryReroll && retrycount < 6)
            {
                retrycount++;
                if (PieceHistory.Contains(getresult))
                {
                    Debug.Print("Piece func was one of the last 4 called. Attempting to re-roll");
                    getresult = WorkBag.Take(rgen);
                }
            }


            foreach (var increment in GeneratedDistance.Keys)
            {
                GeneratedDistance[increment]++;
            }
            
            GeneratedDistance[getresult] = 0;
            int usemax = -1;

            var maxdistanceitem = GeneratedDistance.OrderByDescending((u) => u.Value).First();
            WorkBag.Add(maxdistanceitem.Key);

            //we could "wrap" the call to this function in a wrapper to do something additional here, eg to set data in this class.
            //however the main reason for that would be operate on the Mino itself, and since we can't create a generated list of distances for every possible result from a function 
            //it is not very useful.
            return getresult;
        }
        
    }


  




    
}