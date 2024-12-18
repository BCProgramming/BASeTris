﻿using BASeTris.Rendering.Adapters;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public enum StateMouseButtons
    {
        LButton,
        RButton,
        MButton,
        xButton1,
        xButton2

    }
    public interface IMouseInputState
    {



        void MouseDown(IStateOwner pOwner,StateMouseButtons ButtonDown, BCPoint Position);
        void MouseUp(IStateOwner pOwner,StateMouseButtons ButtonUp, BCPoint Position);
        void MouseMove(IStateOwner pOwner,BCPoint Position);

        public MouseStateAggregate MouseInputData { get; }
    }
    public class MouseInputStateHelper
    {
        public static StateMouseButtons TranslateButton(System.Windows.Forms.MouseButtons mb)
        {
            switch(mb)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    return StateMouseButtons.LButton;
                case System.Windows.Forms.MouseButtons.Middle:
                    return StateMouseButtons.MButton;
                case System.Windows.Forms.MouseButtons.Right:
                    return StateMouseButtons.RButton;
                case System.Windows.Forms.MouseButtons.XButton1:
                    return StateMouseButtons.xButton1;
                case System.Windows.Forms.MouseButtons.XButton2:
                    return StateMouseButtons.xButton2;
            }
            return StateMouseButtons.LButton;
                
        }
        public static StateMouseButtons TranslateButton(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button)
        {
            switch(button)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left:
                    return StateMouseButtons.LButton;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right:
                    return StateMouseButtons.RButton;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle:
                    return StateMouseButtons.MButton;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button4:
                    return StateMouseButtons.xButton1;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button5:
                    return StateMouseButtons.xButton2;
            }
            return StateMouseButtons.LButton;

        }
    }
}
