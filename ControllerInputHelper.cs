using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XInput.Wrapper;

namespace BASeTris
{

    //TODO: somehow, we got ourselves coupled to XInput. Ideally this class would not be directly coupled to XInput, instead it would perhaps be an abstract class
    //which gets derived from for the specific implementations.
    //The GamePresenter class also has some controller mapping stuff, we'd want that moved to those specific implementations, and mapping would be per-implementation and the controller inputs would only give back the GameKeys.
    //That is a major overhaul of the game controller featureset; so I'll defer that to at least after the XMLTheme stuff is implemented.
    public class ControllerInputState
    {
        //tracks button state. Basically can be used to detect rising and falling edges of button presses.
        public EventHandler<ControllerButtonEventArgs> ButtonPressed;
        public EventHandler<ControllerButtonEventArgs> ButtonReleased;
        public X.Gamepad Pad { get; set; } = null;

        private HashSet<X.Gamepad.GamepadButtons> PressedButtons = new HashSet<X.Gamepad.GamepadButtons>();

        //checks button states and raises events if necessary.
        public ControllerInputState(X.Gamepad pPad)
        {
            Pad = pPad;
        }
        /// <summary>
        /// returns the set of currently pressed buttons as a HashSet.
        /// </summary>
        /// <returns></returns>
        public ISet<X.Gamepad.GamepadButtons> GetPressedButtons()
        {
            return new HashSet<X.Gamepad.GamepadButtons>(PressedButtons);
        }
        public void CheckState()
        {
             if (Pad.Update())
            {
                foreach (X.Gamepad.GamepadButtons iterate in Enum.GetValues(typeof(X.Gamepad.GamepadButtons)))
                {
                    bool wasPressed = PressedButtons.Contains(iterate);
                    bool Statepressed = GetButtonState(Pad, iterate);
                    if (!wasPressed && Statepressed)
                    {
                        PressedButtons.Add(iterate);
                        ButtonPressed?.Invoke(this, new ControllerButtonEventArgs(iterate));
                    }
                    else if (wasPressed && !Statepressed)
                    {
                        PressedButtons.Remove(iterate);
                        ButtonReleased?.Invoke(this, new ControllerButtonEventArgs(iterate));
                    }
                }
            }
        }
        private bool GetButtonState(X.Gamepad pad, X.Gamepad.GamepadButtons button)
        {
            return (((short) pad.Buttons & (short) button) == (short) button);
        }
       public class ControllerButtonEventArgs : EventArgs
        {
            public X.Gamepad.GamepadButtons button;

            public ControllerButtonEventArgs(X.Gamepad.GamepadButtons pButton)
            {
                button = pButton;
            }
        }
    }
}