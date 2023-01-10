using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateMenuItemGDIPlusDrawData
    {
        public RectangleF Bounds = RectangleF.Empty;
        public MenuStateMenuItem.StateMenuItemState DrawState = MenuStateMenuItem.StateMenuItemState.State_Normal;
        public MenuStateMenuItemGDIPlusDrawData(RectangleF pBounds, MenuStateMenuItem.StateMenuItemState pDrawState)
        {
            Bounds = pBounds;
            DrawState = pDrawState;
        }
    }

    public class MenuStateMenuItemSkiaDrawData
    {
        public SkiaSharp.SKRect Bounds = SkiaSharp.SKRect.Empty;
        public MenuStateMenuItem.StateMenuItemState DrawState = MenuStateMenuItem.StateMenuItemState.State_Normal;
        public MenuStateMenuItemSkiaDrawData(SkiaSharp.SKRect pBounds, MenuStateMenuItem.StateMenuItemState pDrawState)
        {
            Bounds = pBounds;
            DrawState = pDrawState;
        }
    }

    //TODO: implement RenderingProvider framework for MenuStateMenuItem and subclasses, replacing the Draw() abstract routine.
    public abstract class MenuStateMenuItem
    {
        public String TipText { get; set; } = "";
        public Object Tag { get; set; }
        public enum StateMenuItemState
        {
            State_Normal,
            State_Selected,
            State_Unavailable
        }
        public enum MenuEventResultConstants
        {
            Unhandled,
            Handled
        }
        public abstract bool GetSelectable();
        public abstract void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState);
        /// <summary>
        /// Method called when this menu item is selected.
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnSelected() { return MenuEventResultConstants.Unhandled; }
        /// <summary>
        /// Method called when the menu item is deselected. (This is called After OnSelected)
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnDeselected() { return MenuEventResultConstants.Unhandled; }
        /// <summary>
        /// Called when the Menu Item is activated (with CW).
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnActivated() { return MenuEventResultConstants.Unhandled; }

        public virtual MenuEventResultConstants OnDeactivated() { return MenuEventResultConstants.Unhandled; }

        public virtual void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            //no default implementation.
        }

    }
    public static class HorizontalAlignmentExtensions
    {
        public static HorizontalAlignment GetGDIPlusAlignment(this MenuStateTextMenuItem.MenuHorizontalAlignment Source)
        {
            switch(Source)
            {
                case MenuStateTextMenuItem.MenuHorizontalAlignment.Left:
                    return HorizontalAlignment.Left;
                case MenuStateTextMenuItem.MenuHorizontalAlignment.Center:
                    return HorizontalAlignment.Center;
                case MenuStateTextMenuItem.MenuHorizontalAlignment.Right:
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }
    }
    public abstract class MenuStateSizedMenuItem : MenuStateMenuItem
    {
        
        
    }
    //Standard Item in a menu for a Menu State.
    public class MenuStateTextMenuItem: MenuStateSizedMenuItem
    {

        public enum MenuHorizontalAlignment
        {
            Left,
            Center,
            Right
        }
        public MenuHorizontalAlignment TextAlignment { get; set; } = MenuHorizontalAlignment.Center;
        public virtual String Text { get; set; }
        public String FontFace { get; set; }
        public float FontSize { get; set; }
        
        
        private BCColor _ForeColor;
        private BCColor _BackColor = Color.Transparent;
        public BCColor ForeColor { get{ return _ForeColor; } set{ _ForeColor = value; } }

        public BCColor BackColor { get { return _BackColor; } set { _BackColor = value;  } }



        private BCColor _ShadowColor = Color.Gray;
        public BCColor ShadowColor { get { return _ShadowColor; } set { _ShadowColor = value; } }

        public override bool GetSelectable()
        {
            return true;
        }

        public MenuStateTextMenuItem()
        {
            ForeColor = Color.Black;
            BackColor = Color.Transparent;
            ShadowColor = Color.Gray;

        }

     
       
        
        
        public override void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState)
        {


        }
    }
    

    public class MenuStateLabelMenuItem:MenuStateTextMenuItem
    {
        public override bool GetSelectable()
        {
            return false;
        }
    }
    //Additional Menu Options we'll want:
    //A "Toggle" State that shows several options and accepts Right and Left to switch between them.
    //Possibly just activating and then allowing different options to be chosen if there are too many to fit.

    //Some kind of "Text Entry" Menu item. We can cheat for a start and just use the TextInput State to get the appropriate text input needed.
    
}
