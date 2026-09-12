using BASeCamp.Logging;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BASeTris.AI
{
    public class ReplayInputInjector : BaseAI //A "replay" is sort of like an AI that just, presses the recorded buttons at the right time. 
    {
        //Note: still issues with this one. It can get to about 5-6 minutes before a desync, though it's unclear if that's specific to that replay, or if it's a general problem

        //Added notes: Was able to observe exact moment of desync; a piece was moved one extra block/tile beyond where it had been. Possibly the drop input was somehow observed before the input to move the piece?
        ConcurrentQueue<GameplayRecordElement> DataQueue = null;
        
       
     
        public ReplayInputInjector(IStateOwner pOwner,Queue<GameplayRecordElement> ReplayDataQueue):base(pOwner)
        {
            DataQueue = new ConcurrentQueue<GameplayRecordElement>(ReplayDataQueue);
            
        }
        private TimeSpan Millisecond = new TimeSpan(0, 0, 0, 0, 1);
        private bool _GameKeyDirect = false;
        protected override void AIActionThread()
        {
            while (DataQueue == null)
            {
            }
            while (DataQueue!=null && DataQueue.Any() && AIProcessing)
            {
                if (_Owner.CurrentState is GameplayGameState) //only insert keys during the Gameplay Game State, since we only record them there
                {

                    //peek at the top of the Queue
                    _ = DataQueue.TryPeek(out GameplayRecordElement? result);

                    var Elapsed = _Owner.GetElapsedTime();
                    //if the games elapsed time is larger than the elapsed time of the element
                    if (result.Elapsed <= Elapsed)
                    {
                        //if the time has elapsed, dequeue this element
                        DataQueue.TryDequeue(out result);

                        //now make it so the next game frame will handle the game key.
                        if (result is GameplayRecordKeyPressElement gpr)
                        {


                            //wild part here: we directly call HandleGameKey.
                            if (_GameKeyDirect)
                            {
                                _Owner.CurrentState.HandleGameKey(_Owner, gpr.GameKey);
                                var gt = _Owner.GetElapsedTime();
                                Debug.Print($"Enqueued playback action: lag = {gt - result.Elapsed}, Key={gpr.GameKey}");
                            }
                            else
                            {
                                _Owner.EnqueueAction(() =>
                                {
                                    _Owner.CurrentState.HandleGameKey(_Owner, gpr.GameKey);
                                    var gt = _Owner.GetElapsedTime();
                                    Debug.Print($"Enqueued playback action: lag = {gt - result.Elapsed}, Key={gpr.GameKey}");
                                    return false;
                                });
                            }


                            //
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
                        //Debug.Print($"Not Enqueueing next replay element. Event={result.Elapsed} GameTime={Elapsed}");
                        //let's sleep based on the time to the next event... we could probably get away without this...
                        //var mssleep = result.Elapsed - Elapsed;
                        //if (mssleep.TotalMilliseconds > 200) 
                        //    Thread.Sleep((int)(mssleep.TotalMilliseconds/4));
                    }
                }
                else
                {
                    DebugLogger.Log.WriteLine("Not processing replay input during Gamestate type:" + _Owner.CurrentState.GetType().Name);
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