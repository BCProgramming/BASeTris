using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class QueueableChooserWrapper:BlockGroupChooser
    {
        
        private BlockGroupChooser Composite { get; set; } = null;

        public int WaitListSize { get; set; } = 50;

        public Queue<Func<Nomino>> GeneratedQueue = new Queue<Func<Nomino>>();

        /*public Nomino Peek()
        {
            return GeneratedQueue.Peek();
        }*/
        /*public Queue<Nomino> GetQueue()
        {
            return GeneratedQueue;
        }*/
        public QueueableChooserWrapper(BlockGroupChooser pComposite) : base(null, 0)
        {
        }
        private void FillQueue()
        {
            while (GeneratedQueue.Count < WaitListSize)
            {
                var compositeresult = Composite.GetNextFunc();
                GeneratedQueue.Enqueue(compositeresult);
            }
        }
        internal override Func<Nomino> GetNextFunc()
        {
            FillQueue();
            return GeneratedQueue.Dequeue();
        }




    }
}
