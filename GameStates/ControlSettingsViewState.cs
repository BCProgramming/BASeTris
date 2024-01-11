using BASeTris.AssetManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public int ControllerDisplayIndex = 0;
        public ControllerSettingType SettingsType { get; set; }
        public GameState RevertState { get; set; }
        public Settings.SettingsManager Settings {get;set;} = null;
        public ControlSettingsViewState(GameState pRevertState, Settings.SettingsManager pSettingsManager,ControllerSettingType pType)
        {
            Settings = pSettingsManager;
            RevertState = pRevertState;
            SettingsType = pType;
        }
        public bool AllowDirectGamepadInput()
        {
            return false;
        }
        public bool AllowDirectKeyboardInput()
        {
            return false;
        }
        public void ButtonPressed(IStateOwner pOwner, int ButtonCode)
        {
            pOwner.EnqueueAction(() =>
            {
                pOwner.CurrentState = RevertState;
            });
            //throw new NotImplementedException();
        }

      

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
            if (g == GameKeys.GameKey_Right)
            {
                ControllerDisplayIndex = (ControllerDisplayIndex + 1) % AssetHelper.AllControllerTypes.Length;
            }
            else
            {
                pOwner.EnqueueAction(() =>
                {
                    pOwner.CurrentState = RevertState;
                });
            }
        }
        public void KeyDown(IStateOwner pOwner, int pKey)
        {
        }
        public void KeyPressed(IStateOwner pOwner, int pKey)
        {
            //throw new NotImplementedException();
        }
        public void KeyUp(IStateOwner pOwner, int pKey)
        {
            if (pKey == (int)Keys.Escape)
            {
                pOwner.EnqueueAction(() =>
                {
                    pOwner.CurrentState = RevertState;
                });
            }
        }
        public override DisplayMode SupportedDisplayMode => DisplayMode.Full;
        public GameState GetComposite()
        {
            return RevertState;
        }
    }
}
