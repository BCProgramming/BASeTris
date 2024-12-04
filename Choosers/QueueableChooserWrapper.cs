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

        public Queue<Nomino> GeneratedQueue = new Queue<Nomino>();

        public Nomino Peek()
        {
            return GeneratedQueue.Peek();
        }
        public Queue<Nomino> GetQueue()
        {
            return GeneratedQueue;
        }
        public QueueableChooserWrapper(BlockGroupChooser pComposite) : base(null, 0)
        {
        }
        private void FillQueue()
        {
            while (GeneratedQueue.Count < WaitListSize)
            {
                var compositeresult = Composite.RetrieveNext();
                GeneratedQueue.Enqueue(compositeresult);
            }
        }
        protected override Nomino GetNext()
        {
            FillQueue();
            return GeneratedQueue.Dequeue();
        }




    }
}
