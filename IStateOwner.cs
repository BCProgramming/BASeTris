using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris
{
    public interface IStateOwner
    {
        void SetDisplayMode(GameState.DisplayMode pMode);
        GameState CurrentState { get; set; }
        void EnqueueAction(Action pAction);
        Rectangle GameArea { get; }
        void Feedback(float Strength, int Length);


        double ScaleFactor { get; }
        void SetScale(double pScale);
        event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange;
        DateTime GameStartTime { get; set; }

        TimeSpan FinalGameTime { get; set; }
        DateTime LastPausedTime { get; set; }
        TimeSpan GetElapsedTime();

        BCRect LastDrawBounds { get; }
        StandardSettings Settings { get; }
        event EventHandler<GameClosingEventArgs> GameClosing;
    }
    public class StateOwnerEventArgs :EventArgs
    {
        public IStateOwner _Owner;
        public StateOwnerEventArgs(IStateOwner pOwner)
        {
            _Owner = pOwner;
        }
    }
    public class GameClosingEventArgs : StateOwnerEventArgs
    {
        public GameClosingEventArgs(IStateOwner pOwner) :base(pOwner)
        {

        }
    }
    public interface IGamePresenter
    {
        void Present();
        void StartGame();

        GamePresenter GetPresenter();
    }
    
    public class BeforeGameStateChangeEventArgs : CancelEventArgs
    {
        public GameState PreviousState;
        public GameState NewState;
        public BeforeGameStateChangeEventArgs(GameState pPrevious, GameState pNew)
        {
            PreviousState = pPrevious;
            NewState = pNew;
            Cancel = false;
        }
    }
}