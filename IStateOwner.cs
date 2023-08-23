using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Adapters;
using BASeTris.Settings;
using BASeTris.Theme.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        AudioThemeManager AudioThemeMan { get; set; }
        double ScaleFactor { get; }
        void SetScale(double pScale);
        event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange;
        //DateTime GameStartTime { get; set; }
        Stopwatch GameTime { get; set; }
        TimeSpan FinalGameTime { get; }
        //DateTime LastPausedTime { get; set; }
        TimeSpan GetElapsedTime();

        BCRect LastDrawBounds { get; }
        SettingsManager Settings { get; }
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
        Type GetCanvasType();
        public double FrameTime { get; }
    }
    
    public class BeforeGameStateChangeEventArgs : CancelEventArgs
    {
        public IStateOwner Owner;
        public GameState PreviousState;
        public GameState NewState;
        public BeforeGameStateChangeEventArgs(IStateOwner pOwner,GameState pPrevious, GameState pNew)
        {
            Owner = pOwner;
            PreviousState = pPrevious;
            NewState = pNew;
            Cancel = false;
        }
    }
}