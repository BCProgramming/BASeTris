using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris.GameStates
{
    /// <summary>
    /// This interface should be supported by any GameState that wants to intercept direct keyboard input.
    /// If an interface supports this, HandleGameKey() will only be called for Gamepad inputs.
    /// </summary>
    public interface IDirectKeyboardInputState
    {
        bool AllowDirectKeyboardInput();
        void KeyPressed(IStateOwner pOwner, int pKey);
        void KeyUp(IStateOwner pOwner, int pKey);
    }

    public interface IDirectGamepadInputState
    {
        bool AllowDirectGamepadInput();
        void ButtonPressed(IStateOwner pOwner, int ButtonCode);
    }
}