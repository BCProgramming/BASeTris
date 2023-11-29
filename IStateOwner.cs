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

    public static class IGamePresenterExtensions
    {
        /// <summary>
        /// determines the divider between the desired framerate per second and the specified target framerate using the IGamePresenter's current frametime.
        /// for example, if the current frametime is 1/120 than it will return 0.5, as any "speed" values need to be divided by that to approximate the same movement over a specific absolute time period.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="DesiredFramerate"></param>
        /// <returns></returns>
        public static double GetSpeedDivider(this IGamePresenter src, double DesiredFramerate)
        {
            double movementscale = Math.Max(1, ((1d / 60d) / src.FrameTime));
            if (double.IsInfinity(movementscale)) movementscale = 1;
            return movementscale; 
        }
    }
}