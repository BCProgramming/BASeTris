using BASeTris.Rendering.Adapters;
using OpenTK.Input;
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
        public int Index { get; set; }
        public SkiaSharp.SKRect Bounds = SkiaSharp.SKRect.Empty;
        public MenuStateMenuItem.StateMenuItemState DrawState = MenuStateMenuItem.StateMenuItemState.State_Normal;
        public MenuStateMenuItemSkiaDrawData(SkiaSharp.SKRect pBounds, MenuStateMenuItem.StateMenuItemState pDrawState,int pIndex)
        {
            Index = pIndex;
            Bounds = pBounds;
            DrawState = pDrawState;
        }
    }

    //TODO: implement RenderingProvider framework for MenuStateMenuItem and subclasses, replacing the Draw() abstract routine.
    public abstract class MenuStateMenuItem
    {
        public double TransitionPercentage { get; set; } = 1;
        public BCRect LastBounds { get; set; }
        public String TipText { get; set; } = "";
        public Object Tag { get; set; }
        public string Label { get; set; } = "";


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
        //public abstract void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState);
        /// <summary>
        /// Method called when this menu item is selected.
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnSelected(IStateOwner pOwner) { return MenuEventResultConstants.Unhandled; }
        /// <summary>
        /// Method called when the menu item is deselected. (This is called After OnSelected)
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnDeselected(IStateOwner pOwner) { return MenuEventResultConstants.Unhandled; }
        /// <summary>
        /// Called when the Menu Item is activated (with CW).
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnActivated(IStateOwner pOwner) { return MenuEventResultConstants.Unhandled; }

        public virtual MenuEventResultConstants OnDeactivated(IStateOwner pOwner) { return MenuEventResultConstants.Unhandled; }

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

    //like MenuStateTextMenuItem. 
    //1. Should be visually labelled
    //2. When activated, accepts typed text
    public class MenuStateTextInputMenuItem : MenuStateTextMenuItem, IMenuItemKeyboardInput
    {
        bool IsActivated = false;
        long ActivationTick = 0;
        public void KeyDown(IStateOwner pOwner, MenuState eStateOwner, int pKey)
        {
            if (!IsActivated) return;
            //throw new NotImplementedException();

        }

        public void KeyPressed(IStateOwner pOwner, MenuState StateOwner, int pKey)
        {
          
            if (!IsActivated) return;
            //throw new NotImplementedException();
            
            Text = Text+(char)pKey;
        }

        public void KeyUp(IStateOwner pOwner, MenuState StateOwner, int pKey)
        {
            var currenttick = TetrisGame.GetTickCount();

            if (!IsActivated || currenttick-ActivationTick < 500) return;
            
            if (pKey == (int)Keys.Enter || pKey == (int)OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter)
            {
                StateOwner.ActivatedItem = null;
                OnDeactivated(pOwner);
            }
            //throw new NotImplementedException();
        }
        String RememberedText = "";
        public override MenuEventResultConstants OnActivated(IStateOwner pOwner)
        {
            ActivationTick = TetrisGame.GetTickCount();
            IsActivated = true;
            RememberedText = Text;
            this.Text = "";
            return MenuEventResultConstants.Handled;
        }
        public override MenuEventResultConstants OnDeactivated(IStateOwner pOwner)
        {
            IsActivated = false;
            if (Text == "") Text = RememberedText;
            return MenuEventResultConstants.Handled;
        }


    }


    //Standard Item in a menu for a Menu State.
    public class MenuStateTextMenuItem: MenuStateSizedMenuItem
    {

        public enum AdditionalMenuFlags
        {
            MenuFlags_None = 0,
            MenuFlags_ShowSubmenuArrow = 1
        }
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

        public AdditionalMenuFlags MenuFlags { get; set; } = AdditionalMenuFlags.MenuFlags_None;
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
    public interface IMenuItemKeyboardInput
    {
        void KeyPressed(IStateOwner pOwner, MenuState StateOwner, int pKey);


        void KeyUp(IStateOwner pOwner, MenuState StateOwner, int pKey);
        void KeyDown(IStateOwner pOwner, MenuState eStateOwner, int pKey);
    }
}
