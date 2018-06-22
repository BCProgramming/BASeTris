using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XInput.Wrapper;

namespace BASeTris
{
    public class ControllerInputState 
    {

        public X.Gamepad Pad{ get; set; } = null;

        private HashSet<X.Gamepad.GamepadButtons> PressedButtons = new HashSet<X.Gamepad.GamepadButtons>();
        //checks button states and raises events if necessary.
        public ControllerInputState(X.Gamepad pPad)
        {
            Pad = pPad;
        }
        public void CheckState()
        {
            if(Pad.Update())
            {
                foreach(X.Gamepad.GamepadButtons iterate in Enum.GetValues(typeof(X.Gamepad.GamepadButtons)))
                {
                    bool wasPressed = PressedButtons.Contains(iterate);
                    bool Statepressed = GetButtonState(Pad, iterate);
                    if (!wasPressed && Statepressed)
                    {
                        PressedButtons.Add(iterate);
                        ButtonPressed?.Invoke(this,new ControllerButtonEventArgs(iterate));
                    }
                    else if(wasPressed && !Statepressed)
                    {
                        PressedButtons.Remove(iterate);
                        ButtonReleased?.Invoke(this,new ControllerButtonEventArgs(iterate));
                    }
                }



            }

            

        }
        private bool GetButtonState(X.Gamepad pad, X.Gamepad.GamepadButtons button)
        {
            return (((short)pad.Buttons & (short)button) == (short)button);
        }

        //tracks button state. Basically can be used to detect rising and falling edges of button presses.
        public EventHandler<ControllerButtonEventArgs> ButtonPressed;
        public EventHandler<ControllerButtonEventArgs> ButtonReleased;


        public class ControllerButtonEventArgs:EventArgs
        {
           public X.Gamepad.GamepadButtons button;
            public ControllerButtonEventArgs(X.Gamepad.GamepadButtons pButton)
            {
                button = pButton;
            }
        }
    }
}
