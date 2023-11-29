using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BASeTris.AI;
using BASeTris.GameStates;

namespace BASeTris.Choosers.AIChoosers
{
    public abstract class BaseAIChooser : BlockGroupChooser
    {
        protected GameplayGameState _State;

        public BaseAIChooser(GameplayGameState _StandardState, Func<Nomino>[] pAvailable,int pSeed) : base(pAvailable,pSeed)
        {
            _State = _StandardState;
        }

        ~BaseAIChooser()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (AIWorker != null)
            {
                AIWorker.Abort();
                AIWorker = null;
            }
        }

        bool Caughtup = false;

        private void AIChooserWorker()
        {
            try
            {
                while (AIWorker != null)
                {
                    while (WorkQueue.Count < _MaxElements)
                    {
                        var Grabnext = PerformGetNext();
                        WorkQueue.Enqueue(Grabnext);
                    }

                    if (!Caughtup)
                    {
                        Caughtup = true;
                        AIWorker.Priority = ThreadPriority.Lowest;
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException tae)
            {
            }
        }


        Thread AIWorker = null;
        private int _MaxElements = 15;
        protected ConcurrentQueue<Nomino> WorkQueue = new ConcurrentQueue<Nomino>();
        public abstract Nomino PerformGetNext();

        internal override Nomino GetNext()
        {
            if (AIWorker == null)
            {
                AIWorker = new Thread(AIChooserWorker);
                AIWorker.Priority = ThreadPriority.Normal;
                AIWorker.Start();
            }

            while (WorkQueue.IsEmpty)
            {
                Thread.Sleep(5);
            }

            Nomino getresult = null;
            while (!WorkQueue.TryDequeue(out getresult))
            {
                Thread.Sleep(15);
            }

            return getresult;
        }
    }
}