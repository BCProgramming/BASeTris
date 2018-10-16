using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
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
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemDeselected;


        public String StateHeader { get; set; }

        public Font HeaderFont { get; set; } = new Font("Arial",28,FontStyle.Regular,GraphicsUnit.Pixel);
        //Our menu elements.
        public List<MenuStateMenuItem> MenuElements = new List<MenuStateMenuItem>();

        public int StartItemOffset = 0; //start drawing menu items at this index. This is
        //used to scroll the menu up and down.
        public int SelectedIndex = 0;
        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        private IBackgroundDraw _BG = null;

        public MenuState(IBackgroundDraw pBG)
        {
            _BG = pBG;
        }
        public MenuState():this(null)
        {

        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //we are a full mode state, so this shouldn't be called.
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
            _BG?.FrameProc();
        }
        Dictionary<double, Font> FontSizeData = new Dictionary<double, Font>();
        private Font GetScaledHeaderFont(IStateOwner pOwner)
        {
            lock (FontSizeData)
            {
                if (!FontSizeData.ContainsKey(pOwner.ScaleFactor))
                {
                    Font buildfont = new Font(this.HeaderFont.FontFamily, (float)(this.HeaderFont.Size * pOwner.ScaleFactor), this.HeaderFont.Style);
                    FontSizeData.Add(pOwner.ScaleFactor, buildfont);
                }
                return FontSizeData[pOwner.ScaleFactor];
            }
        }
        protected virtual float DrawHeader(IStateOwner pOwner,Graphics Target,RectangleF Bounds)
        {

            Font useHeaderFont = GetScaledHeaderFont(pOwner);
            var HeaderSize = Target.MeasureString(StateHeader, useHeaderFont);
            float UseX = Bounds.Width / 2 - HeaderSize.Width / 2;
            float UseY = HeaderSize.Height / 3;

            TetrisGame.DrawText(Target,useHeaderFont,StateHeader,Brushes.Black,Brushes.White,UseX,UseY);

            return UseY + HeaderSize.Height;
        }
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            if(_BG!=null) _BG.DrawProc(g,Bounds);
            int CurrentIndex = StartItemOffset;
            float CurrentY = DrawHeader(pOwner,g, Bounds);
            float MaxHeight = 0, MaxWidth = 0;
            //we want to find the widest item.
            foreach(var searchitem in MenuElements)
            {
                if(searchitem is MenuStateSizedMenuItem mss)
                {
                    var grabsize = mss.GetSize(pOwner);
                    if (grabsize.Height > MaxHeight) MaxHeight = grabsize.Height;
                    if (grabsize.Width > MaxWidth) MaxWidth = grabsize.Width;
                }
            }
            //we draw each item at the maximum size.
            SizeF ItemSize = new SizeF(MaxWidth, MaxHeight);
            CurrentY += (float)(pOwner.ScaleFactor * 5);
            for(int menuitemindex = 0;menuitemindex < MenuElements.Count;menuitemindex++)
            {
                var drawitem = MenuElements[menuitemindex];
                Rectangle TargetBounds = new Rectangle((int)(Bounds.Width / 2 - ItemSize.Width / 2), (int)CurrentY, (int)(ItemSize.Width), (int)(ItemSize.Height));
                MenuStateMenuItem.StateMenuItemState useState = menuitemindex == SelectedIndex ? MenuStateMenuItem.StateMenuItemState.State_Selected : MenuStateMenuItem.StateMenuItemState.State_Normal;
                    drawitem.Draw(pOwner,g, TargetBounds, useState);
                CurrentY += ItemSize.Height + 5;
            }
         

        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //handle up and down to change the currently selected menu item.
            //Other game keys we pass on to the currently selected item itself for additional handling.
            bool triggered = false;
            var OriginalIndex = SelectedIndex;
            if(g==GameKeys.GameKey_Down)
            {
                //move selected index upwards.

                SelectedIndex++;
                if(MenuElements.Count-1 < SelectedIndex)
                {
                    SelectedIndex = 0;
                }
                triggered = true;
                //should also skip if disabled...
            }
            else if(g==GameKeys.GameKey_Drop)
            {
                //move selected index downwards.
                SelectedIndex--;
                if (SelectedIndex < 0) SelectedIndex = MenuElements.Count - 1;
                triggered = true;
            }

            if(g==GameKeys.GameKey_RotateCW || g==GameKeys.GameKey_MenuActivate || g==GameKeys.GameKey_Pause)
            {
                //Activate the currently selected item.
                var currentitem = MenuElements[SelectedIndex];
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.MenuItemActivated);
                MenuItemActivated?.Invoke(this,new MenuStateMenuItemActivatedEventArgs(currentitem));
                currentitem.OnActivated();
                triggered = true;
            }

            else if (OriginalIndex != SelectedIndex)
            {
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.MenuItemSelected);
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
