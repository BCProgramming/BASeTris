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
        public static Color MixColor(Color ColorA, Color ColorB, float percentage)
        {
            float[] ColorAValues = new float[] { (float)ColorA.A, (float)ColorA.R, (float)ColorA.G, (float)ColorA.B };
            float[] ColorBValues = new float[] { (float)ColorB.A, (float)ColorB.R, (float)ColorB.G, (float)ColorB.B };
            float[] ColorCValues = new float[4];


            for (int i = 0; i <= 3; i++)
            {
                ColorCValues[i] = (ColorAValues[i] * percentage) + (ColorBValues[i] * (1 - percentage));
            }


            return Color.FromArgb((int)ColorCValues[0], (int)ColorCValues[1], (int)ColorCValues[2], (int)ColorCValues[3]);
        }
        public static Color MixColor(Color ColorA,Color ColorB)
        {
            return MixColor(ColorA, ColorB, 0.5f);
        }
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
        
        
        private Color _ForeColor;
        private Color _BackColor = Color.Transparent;
        public Color ForeColor { get{ return _ForeColor; } set{ _ForeColor = value;ForeBrush = new SolidBrush(value); } }

        public Color BackColor { get { return _BackColor; } set { _BackColor = value; BackBrush = new SolidBrush(value); } }


        public Brush ForeBrush { get; set; }

        public  Brush BackBrush { get; set; }

        public Brush ShadowBrush { get; set; }

        private Color _ShadowColor = Color.Gray;
        public Color ShadowColor { get { return _ShadowColor; } set { _ShadowColor = value;ShadowBrush = new SolidBrush(value); } }

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

     
       
        
        public static void DrawMenuText(IStateOwner pOwner, MenuTextDrawInfo DrawData, Graphics Target,Rectangle Bounds,StateMenuItemState DrawState)
        {
            //basically just draw the Text centered within the Bounds.
          
        }
        public override void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState)
        {


        }
    }
    public class MenuTextDrawInfo
    {
        public String Text;
        public HorizontalAlignment TextAlignment;
        public Font Font;
        public Brush ForegroundBrush;
        public Brush BackgroundBrush;
        public MenuTextDrawInfo()
        {

        }
        public MenuTextDrawInfo(String pText,HorizontalAlignment pAlignment,Font pFont,Brush ForeBrush,Brush BackBrush)
        {
            Text = pText;
            TextAlignment = pAlignment;
            Font = pFont;
            ForegroundBrush = ForeBrush;
            BackgroundBrush = BackBrush;
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
