using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using static XInput.Wrapper.X.Gamepad;

namespace BASeTris.Input
{
    //Interface for Gamepad "Drivers"
    //WIP
    public interface IGamepadDriver 
    {
        //interface for a gamepad "driver" which would be the overarching handler, drilling down to specific Gamepad devices from here.
        
        IGamepad Getgamepad(int index);

        

    }
    public interface IGamepadDriver<IGAMEPAD> where IGAMEPAD : IGamepad
    {
        IGAMEPAD Getgamepad(int index);
    }

   
    public abstract class GamepadDriverBase<IGAMEPAD> : IGamepadDriver<IGAMEPAD>,IGamepadDriver where IGAMEPAD : IGamepad
    {

        public abstract IGAMEPAD Getgamepad(int index);
        IGamepad IGamepadDriver.Getgamepad(int index)
        {
            return this.Getgamepad(index);
        }
        IGAMEPAD IGamepadDriver<IGAMEPAD>.Getgamepad(int index)
        {
            return this.Getgamepad(index);
        }
    }

    public interface IGamepad
    {
        [Flags]
        public enum ControllerButtons : int
        {
            Dpad_Up = 0x0001,
            Dpad_Down = 0x0002,
            Dpad_Left = 0x0004,
            Dpad_Right = 0x0008,
            Start = 0x0010,
            Back = 0x0020,
            LeftStick = 0x0040,
            RightStick = 0x0080,
            LBumper = 0x0100,
            RBumper = 0x0200,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000,
        };
        public ControllerButtons Buttons { get; }

        public bool IsButtonDown(ControllerButtons buttonFlags);

        public bool IsButtonUp(short prevButtons, ControllerButtons buttonFlags);


        public bool Update();

    }

}
