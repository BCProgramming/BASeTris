using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates.Menu
{
    public class MenuState : GameState
    {
        //The "Menu" state presents a Menu. Well, I mean, obviously.

        //Draws a stack of "menu" Items
        //Above the stack is drawn some sort of "Header" Image. (or text).
        //One item can be selected at a time. Moving up and down moves the selection.
        //Pressing Enter will "Activate" the item. This will trigger it's action. For most items this will perform some action which will change to another state, for example.

        

        public event EventHandler<MenuStateMenuItemActivatedEventArgs> MenuItemActivated;
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemSelected;

        
        public String StateHeader { get; set; }

        public Font HeaderFont { get; set; } = new Font("Arial",28,FontStyle.Regular,GraphicsUnit.Pixel);
        //Our menu elements.
        public List<MenuStateMenuItem> MenuElements = new List<MenuStateMenuItem>();

        public int StartItemOffset = 0; //start drawing menu items at this index. This is
        //used to scroll the menu up and down.

        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        private IBackgroundDraw _BG = null;


        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //we are a full mode state, so this shouldn't be called.
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }
        private void DrawHeader(Graphics Target,RectangleF Bounds)
        {
            var HeaderSize = Target.MeasureString(StateHeader, HeaderFont);



        }
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            int CurrentIndex = StartItemOffset;

        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //handle up and down to change the currently selected menu item.
            //Other game keys we pass on to the currently selected item itself for additional handling.

            //throw new NotImplementedException();
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
    }
    public class MenuStateEventArgs : EventArgs
    {
        public MenuStateMenuItem MenuElement;
        public MenuStateEventArgs(MenuStateMenuItem _Element)
        {
            MenuElement = _Element;
        }
    }
    
    public class MenuStateMenuItemActivatedEventArgs : MenuStateEventArgs
    {
        public MenuStateMenuItemActivatedEventArgs(MenuStateMenuItem _Element):base(_Element)
        {

        }
    }
    public class MenuStateMenuItemSelectedEventArgs : MenuStateEventArgs
    {
        public MenuStateMenuItemSelectedEventArgs(MenuStateMenuItem _Element) : base(_Element)
        {

        }
    }
}
