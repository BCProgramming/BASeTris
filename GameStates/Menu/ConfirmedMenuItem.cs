using BASeTris.Rendering.Skia.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class ConfirmedTextMenuItem: MenuStateTextMenuItem
    {
        //A "confirmed" Option. Overrides activation and shows a confirmation before it actually allows the confirmation to fall through and therefore raise the activation event
        //for this menu item.

        bool IsActivated = false;
        bool Confirmation = false;
        public event EventHandler<MenuStateMenuItemActivatedEventArgs> OnOptionConfirmed;
        protected String GetConfirmString()
        {
            if (Confirmation) return "<Yes>/ No ";
            else return " Yes /<No>";
        }
       
        public override void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            if(IsActivated)
            {
                if (pKey == GameState.GameKeys.GameKey_Left)
                    Confirmation = true;
                else if (pKey == GameState.GameKeys.GameKey_Right)
                    Confirmation = false;
            }
        }

        public override MenuEventResultConstants OnActivated(IStateOwner pOwner)
        {
            IsActivated = true;
            return MenuEventResultConstants.Handled;
        }

        public override string Text
        {
            get{ if (IsActivated) return GetConfirmString(); else return base.Text; }

            set { base.Text = value; }
        }

        public override MenuEventResultConstants OnDeactivated(IStateOwner pOwner)
        {
            
            
            IsActivated = false;
            if(Confirmation)
            {
                OnOptionConfirmed?.Invoke(this,new MenuStateMenuItemActivatedEventArgs(this,pOwner));
                Confirmation = false;
            }
            return MenuEventResultConstants.Handled;
        }
        
        
    }
}
