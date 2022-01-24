using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using BASeTris;
using BASeTris.BackgroundDrawers;
using BASeTris.Rendering.Adapters;

namespace BASeTris.GameStates.Menu
{
    public class MenuState : GameState,IMouseInputState
    {
        //The "Menu" state presents a Menu. Well, I mean, obviously.

        //Draws a stack of "menu" Items
        //Above the stack is drawn some sort of "Header" Image. (or text).
        //One item can be selected at a time. Moving up and down moves the selection.
        //Pressing Enter will "Activate" the item. This will trigger it's action. For most items this will perform some action which will change to another state, for example.

        

        public event EventHandler<MenuStateMenuItemActivatedEventArgs> MenuItemActivated;
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemSelected;
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemDeselected;


        public String StateHeader { get; set; }

        public BCPoint LastMouseMovement { get; set; }

        public String HeaderTypeface { get; set; } = "Arial";
        public float HeaderTypeSize { get; set; } = 18;
        

        //Our menu elements.
        public List<MenuStateMenuItem> MenuElements = new List<MenuStateMenuItem>();
        public int MainXOffset;
        bool OffsetUsed = false;
        public int StartItemOffset = 0; //start drawing menu items at this index. This is
        //used to scroll the menu up and down.
        public int SelectedIndex = 0;

        

        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        

        public MenuState(IBackground pBG)
        {
            _BG = pBG;
        }
        public MenuState():this(null)
        {
            
        }
        
        int OffsetAnimationSpeed = 3;
        public override void GameProc(IStateOwner pOwner)
        {
            if(!OffsetUsed)
            {
                MainXOffset = -pOwner.GameArea.Width / 2;
                OffsetUsed = true;
            }
            if (MainXOffset < 0) MainXOffset += OffsetAnimationSpeed;
            OffsetAnimationSpeed += 3;
            //throw new NotImplementedException();
            
        }
        
       
        protected int GetPreviousIndex(int StartPos)
        {
            StartPos--;
            if (StartPos < 0)StartPos= MenuElements.Count - 1;

            return StartPos;
        }
        protected int GetNextIndex(int StartPos)
        {
            StartPos++;
            if (MenuElements.Count - 1 < StartPos)
            {
                StartPos = 0;
            }
            return StartPos;
        }
        public MenuStateMenuItem ActivatedItem = null;
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //handle up and down to change the currently selected menu item.
            //Other game keys we pass on to the currently selected item itself for additional handling.
            bool triggered = false;
            var OriginalIndex = SelectedIndex;
            if(g==GameKeys.GameKey_Down)
            {
                if (ActivatedItem != null)
                {
                    ActivatedItem.ProcessGameKey(pOwner, g);
                }
                else
                {
                    //move selected index upwards.

                    SelectedIndex = GetNextIndex(SelectedIndex);
                    while (!MenuElements[SelectedIndex].GetSelectable())
                        SelectedIndex = GetNextIndex(SelectedIndex);
                }
                triggered = true;
                //should also skip if disabled...
            }
            else if(g==GameKeys.GameKey_Drop)
            {
                if (ActivatedItem != null)
                {
                    ActivatedItem.ProcessGameKey(pOwner, g);
                }
                else
                { //move selected index downwards.
                    SelectedIndex = GetPreviousIndex(SelectedIndex);

                    while (!MenuElements[SelectedIndex].GetSelectable())
                        SelectedIndex = GetPreviousIndex(SelectedIndex);
                }
                triggered = true;
            }

            if(g==GameKeys.GameKey_RotateCW || g==GameKeys.GameKey_MenuActivate || g==GameKeys.GameKey_Pause)
            {
                if (ActivatedItem != null)
                {
                    TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemActivated.Key, pOwner.Settings.std.EffectVolume);
                    ActivatedItem.OnDeactivated();
                    ActivatedItem = null;
                }
                else
                {

                    //Activate the currently selected item.
                    var currentitem = MenuElements[SelectedIndex];
                    TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemActivated.Key, pOwner.Settings.std.EffectVolume);
                    ActivatedItem = currentitem;
                    MenuItemActivated?.Invoke(this, new MenuStateMenuItemActivatedEventArgs(currentitem));

                    currentitem.OnActivated();
                }
                triggered = true;
            }

            else if (OriginalIndex != SelectedIndex)
            {
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemSelected.Key, pOwner.Settings.std.EffectVolume);
                var previousitem = MenuElements[OriginalIndex];
                var currentitem = MenuElements[SelectedIndex];
                MenuItemDeselected?.Invoke(this, new MenuStateMenuItemSelectedEventArgs(previousitem));
                MenuItemSelected?.Invoke(this,new MenuStateMenuItemSelectedEventArgs(currentitem));
                previousitem.OnDeselected();
                currentitem.OnSelected();
            }

            if(!triggered)
            {
                var currentitem = MenuElements[SelectedIndex];
                currentitem.ProcessGameKey(pOwner,g);
                
            }
            //throw new NotImplementedException();
        }
        private void TriggerSelection()
        {
            
       
        }
        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public void MouseDown(StateMouseButtons ButtonDown, BCPoint Position)
        {
            //throw new NotImplementedException();
        }

        public void MouseUp(StateMouseButtons ButtonUp, BCPoint Position)
        {
            //throw new NotImplementedException();
        }

        public void MouseMove(BCPoint Position)
        {
            LastMouseMovement = Position;
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
