using BASeTris.GameStates.GameHandlers;
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
                //peek at the top of the Queue
                DataQueue.TryPeek(out GameplayRecordElement result);

                var Elapsed = _Owner.GetElapsedTime();
                //if the games elapsed time is larger than the elapsed time of the element
                if (result.Elapsed <= Elapsed)
                {
                    //if the time has elapsed, dequeue this element
                    DataQueue.TryDequeue(out result);
                    //now make it so the next game frame will handle the game key.
                    if (result is GameplayRecordKeyPressElement gpr)
                    {
                        Debug.Print($"Enqueueing playback action: keytime={result.Elapsed} > gametime={Elapsed}, Key={gpr.GameKey}");
                        _Owner.EnqueueAction(() => { _Owner.CurrentState.HandleGameKey(_Owner, gpr.GameKey); return false; });
                    }
                    else
                    {
                        //if it is another type, then we need to pass it to the handler.
                        //This is for stuff like say the Dr.Mario game which generates a new level.
                        var _handler = _Owner.GetHandler();
                        if (_handler is IReplayInputAcceptor iria)
                        {
                            iria.AcceptElement(result);
                        }
                    }
                }
                else
                {
                    Debug.Print($"Not Enqueueing next replay element. Event={result.Elapsed} GameTime={Elapsed}");
                    //let's sleep based on the time to the next event.
                    var mssleep = result.Elapsed - Elapsed;
                    Thread.Sleep((int)(mssleep.TotalMilliseconds/2));
                }
                Thread.Sleep(0);

            }
        }
    }

    public interface IReplayInputAcceptor
    {
        void AcceptElement(GameplayRecordElement Element);
    }
}