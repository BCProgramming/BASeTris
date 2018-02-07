using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BASeTris.AI
{
    //AI Base class.
    //the AI Base Class works by queuing up game key presses based on it's AI, then calling the current game state to handle those keys.
    //for... obvious reasons this AI class itself doesn't actually DO anything- it's the base class for other implementations.
    public class BaseAI
    {
        protected IStateOwner _Owner;
        private bool DetachAI = false;
        private Thread AIThread = null;
        protected Queue<GameState.GameKeys> PressKeyQueue = new Queue<GameState.GameKeys>();
        protected BaseAI(IStateOwner pOwner)
        {
            _Owner = pOwner;
            //spin up an AI thread
            AIThread = new Thread(AIActionThread);
            AIThread.Start();
        }
        bool AIProcessing = true;
        protected void AIActionThread()
        {
            //the meat and potatoes...
            while(AIProcessing)
            {
                Thread.Sleep(25);
                //if there are keys to press, we do NOT evaluate the AI Action Frame...
                //if  there are keys to press, press the next one in the queue. We only do ONE per frame though- the idea is that
                //the AI should at least pretend to be sort of human in it's limitations.
                if (PressKeyQueue.Count > 0)
                {
                    var keypress = PressKeyQueue.Dequeue();
                    if(keypress!= GameState.GameKeys.GameKey_Null)
                        _Owner.EnqueueAction(() => { _Owner.CurrentState.HandleGameKey(_Owner, keypress); });
                }
                else
                {
                    Thread.Sleep(200);
                    AIActionFrame();
                   
                }
            }

        }
        public virtual void AIActionFrame()
        {
            //This is where the thinking happens.
            //The AI evaluates the state of the game, and adds any needed keys to press to the queue.
            //generally if the AI finds no active BlockGroup, or more than one.... it should probably skip out.
        }
        public void AbortAI()
        {
            //Cancels AI processing.
            AIProcessing = false;
        }

    }
}
