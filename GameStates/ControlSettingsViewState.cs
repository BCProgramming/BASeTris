﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    //Gamestate for Viewing current control settings. Can be used to "advance" and choose to configure options for Keyboard or Mouse as well.
    public class ControlSettingsViewState : GameState, IDirectGamepadInputState, IDirectKeyboardInputState,ICompositeState<GameState>
    {
        public enum ControllerSettingType
        {
            Keyboard,
            Gamepad
        }
        public ControllerSettingType SettingsType { get; set; }
        public GameState RevertState { get; set; }
        public Settings.SettingsManager Settings {get;set;} = null;
        public ControlSettingsViewState(GameState pRevertState, Settings.SettingsManager pSettingsManager,ControllerSettingType pType)
        {
            Settings = pSettingsManager;
            RevertState = pRevertState;
            SettingsType = pType;
        }
        public void ButtonPressed(IStateOwner pOwner, int ButtonCode)
        {
           //throw new NotImplementedException();
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
        }

        public void KeyPressed(IStateOwner pOwner, int pKey)
        {
            //throw new NotImplementedException();
        }

        public GameState GetComposite()
        {
            return RevertState;
        }
    }
}