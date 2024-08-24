using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BASeTris.AI
{
    public class ReplayInputInjector : BaseAI //A "replay" is sort of like an AI that just, presses the recorded buttons at the right time. 
    {
        ConcurrentQueue<GameplayRecordElement> DataQueue = null;
        
        public ReplayInputInjector(IStateOwner pOwner,Queue<GameplayRecordElement> ReplayDataQueue):base(pOwner)
        {
            DataQueue = new ConcurrentQueue<GameplayRecordElement>(ReplayDataQueue);
        }
        protected override void AIActionThread()
        {
            while (DataQueue == null)
            {
            }
            while (DataQueue!=null && DataQueue.Any() && AIProcessing)
            {
                DataQueue.TryPeek(out GameplayRecordElement result);
                var Elapsed = _Owner.GetElapsedTime();
                if (result.Elapsed <= Elapsed)
                {
                    DataQueue.TryDequeue(out _);
                    //push it!
                    Debug.Print($"Enqueueing playback action: {result.Elapsed} > {Elapsed}, Key={result.GameKey}");
                    _Owner.EnqueueAction(() => { _Owner.CurrentState.HandleGameKey(_Owner, result.GameKey); return false; });
                }
                Thread.Sleep(0);

            }
        }
    }
}