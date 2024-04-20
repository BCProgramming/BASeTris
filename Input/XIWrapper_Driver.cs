using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Input
{
    /// <summary>
    /// Wraps XInput.Wrapper into the GamepadDriver driver interface.
    /// </summary>
    internal class XIWrapper_Driver : GamepadDriverBase<XInput_Gamepad>

    {
        private XInput_Gamepad[] _Pads = null;
        public XIWrapper_Driver()
        {
            _Pads = (from xi in new[] { XInput.Wrapper.X.Gamepad_1, XInput.Wrapper.X.Gamepad_2, XInput.Wrapper.X.Gamepad_3, XInput.Wrapper.X.Gamepad_4 } select new XInput_Gamepad(xi)).ToArray();
        }
        public override XInput_Gamepad Getgamepad(int index)
        {
            return _Pads[index - 1];
        }
    }


    public class XInput_Gamepad : IGamepad
    {
        XInput.Wrapper.X.Gamepad _Pad = null;
        public XInput_Gamepad(XInput.Wrapper.X.Gamepad pPad)
        {
            _Pad = pPad;
        }
        public IGamepad.ControllerButtons Buttons => (IGamepad.ControllerButtons)_Pad.Buttons;

        public bool IsButtonDown(IGamepad.ControllerButtons buttonFlags)
        {
            return _Pad.IsButtonDown((XInput.Wrapper.X.Gamepad.GamepadButtons)buttonFlags);
        }

        

        public bool IsButtonUp(short prevButtons, IGamepad.ControllerButtons buttonFlags)
        {
            return _Pad.IsButtonUp(prevButtons, (XInput.Wrapper.X.Gamepad.GamepadButtons)buttonFlags);
        }
        public bool Update()
        {
            return _Pad.Update();
        }
    }
}
