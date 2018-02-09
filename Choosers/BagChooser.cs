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

        
        

        public static IEnumerable<T> Shuffle<T>(Random rgen,IEnumerable<T> Shufflethese)
        {
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                sl.Add((float)rgen.NextDouble(), iterate);
            }
            return sl.Select(iterator => iterator.Value);

        }
        public BagChooser(Func<BlockGroup>[] SelectionFunctions) : base(SelectionFunctions)
        {

        }
        private void RefillQueue()
        {
            var Shuffled = Shuffle(rgen,_Available).ToArray();
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
