using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class BagChooser : BlockGroupChooser
    {
        Queue<Func<BlockGroup>> WorkQueue = new Queue<Func<BlockGroup>>();

        private Func<BlockGroup>[] AllOptions = null;
        private Random rgen;

        private IEnumerable<T> Shuffle<T>(IEnumerable<T> Shufflethese)
        {
            if (rgen == null) rgen = new Random();
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                sl.Add((float)rgen.NextDouble(), iterate);
            }
            Random rg = new Random();

            return sl.Select(iterator => iterator.Value);

        }
        public BagChooser(Random rgenerator, Func<BlockGroup>[] SelectionFunctions)
        {
            rgen = rgenerator;
            SetOptions(SelectionFunctions);
        }
        public override void SetOptions(Func<BlockGroup>[] pAvailable)
        {
            AllOptions = pAvailable;
        }
        private void RefillQueue()
        {
            var Shuffled = Shuffle(AllOptions).ToArray();
            for(int i=0;i<Shuffled.Length;i++)
            {
                WorkQueue.Enqueue(Shuffled[i]);
            }
        }
        public override BlockGroup GetNext()
        {
            if (WorkQueue.Count == 0) RefillQueue();
            return WorkQueue.Dequeue()();
        }
    }
}
