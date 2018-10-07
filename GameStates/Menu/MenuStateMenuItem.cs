using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
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
        public abstract void Draw(Graphics Target, Rectangle Bounds, StateMenuItemState DrawState);
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
        /// Called when the Menu Item is activated (with enter).
        /// </summary>
        /// <returns></returns>
        public virtual MenuEventResultConstants OnActivated() { return MenuEventResultConstants.Unhandled; }
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

    //Standard Item in a menu for a Menu State.
    public class MenuStateTextMenuItem:MenuStateMenuItem
    {
        
        public String Text { get; set; }

        public Font Font { get; set; }

        public Color ForeColor { get; set; }

        public Color BackColor { get; set; } = Color.Transparent;

        public override void Draw(Graphics Target, Rectangle Bounds, StateMenuItemState DrawState)
        {
            //basically just draw the Text centered within the Bounds.
            var MeasureText = Target.MeasureString(Text, Font);

            PointF DrawPosition = new PointF(((Bounds.Left + Bounds.Width / 2) - MeasureText.Width / 2),( Bounds.Top + Bounds.Height / 2 - (MeasureText.Height / 2)));

            using (Brush BackBrush = new SolidBrush(BackColor))
            {
                Target.FillRectangle(BackBrush,Bounds);
                using (Brush ForeBrush = new SolidBrush(ForeColor))
                {
                    Target.DrawString(Text,Font,ForeBrush,DrawPosition);
                }
            }
        }
    }

    //Additional Menu Options we'll want:
    //A "Toggle" State that shows several options and accepts Right and Left to switch between them.
    //Possibly just activating and then allowing different options to be chosen if there are too many to fit.

    //Some kind of "Text Entry" Menu item. We can cheat for a start and just use the TextInput State to get the appropriate text input needed.
    
}
