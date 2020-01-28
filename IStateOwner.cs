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

        void AddGameObject(GameObject Source);
        double ScaleFactor { get; }
        void SetScale(double pScale);
        event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange;
        DateTime GameStartTime { get; set; }

        TimeSpan FinalGameTime { get; set; }
        DateTime LastPausedTime { get; set; }
        TimeSpan GetElapsedTime();

        StandardSettings Settings { get; }
    }
    public interface IGamePresenter
    {
        void Present();
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