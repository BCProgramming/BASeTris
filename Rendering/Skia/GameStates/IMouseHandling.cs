using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    public static class MouseHandling
    {
        public enum MouseButtons
        {
            MouseButton_Null,
            MouseButton_Primary,
            MouseButton_Secondary,
            MouseButton_Middle,
            MouseButton_WheelUp,
            MouseButton_WheelDown
        }
        public enum MouseActions
        {
            Mouse_Move,
            Mouse_Down,
            Mouse_Up
        }
    }
    /// <summary>
    /// Interface to be implemented by Game States that support/accept Mouse Input.
    /// </summary>
    public interface IMouseSupportingState
    {
        void HandleMouseEvent(IStateOwner pOwner, MouseHandling.MouseActions MouseActionType, MouseHandling.MouseButtons pButtons);
    }

}
