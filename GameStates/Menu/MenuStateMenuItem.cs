using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateMenuItemDrawData
    {
        public RectangleF Bounds = RectangleF.Empty;
        public MenuStateMenuItem.StateMenuItemState DrawState = MenuStateMenuItem.StateMenuItemState.State_Normal;
        public MenuStateMenuItemDrawData(RectangleF pBounds, MenuStateMenuItem.StateMenuItemState pDrawState)
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
    public abstract class MenuStateSizedMenuItem : MenuStateMenuItem
    {
        public abstract SizeF GetSize(IStateOwner pOwner);
        
    }
    //Standard Item in a menu for a Menu State.
    public class MenuStateTextMenuItem: MenuStateSizedMenuItem
    {
        
        Graphics Temp = Graphics.FromImage(new Bitmap(1, 1));
        public HorizontalAlignment TextAlignment { get; set; } = HorizontalAlignment.Center;
        public virtual String Text { get; set; }
        private Font _Font;
        
        public Font Font { get{ return _Font; } set{ _Font = value; lock(FontSizeData){FontSizeData = new Dictionary<double, Font>();} } }
        private Color _ForeColor;
        private Color _BackColor = Color.Transparent;
        public Color ForeColor { get{ return _ForeColor; } set{ _ForeColor = value;ForeBrush = new SolidBrush(value); } }

        public Color BackColor { get { return _BackColor; } set { _BackColor = value; BackBrush = new SolidBrush(value); } }


        protected Brush ForeBrush { get; set; }

        protected  Brush BackBrush { get; set; }

        protected Brush ShadowBrush { get; set; }

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

        public override SizeF GetSize(IStateOwner pOwner)
        {
            var testfont = GetScaledFont(pOwner);
            var MeasureText = Temp.MeasureString(Text, testfont);
            return MeasureText.ToSize();
        }
        private static float GetDrawX(RectangleF pBounds,SizeF DrawSize,HorizontalAlignment pAlign)
        {
            switch(pAlign)
            {
                case HorizontalAlignment.Center:
                    return (pBounds.Left + pBounds.Width / 2) - DrawSize.Width / 2;
                case HorizontalAlignment.Left:
                    return pBounds.Left;
                case HorizontalAlignment.Right:
                    return pBounds.Right - DrawSize.Width;
            }
            return 0;
        }
        private static PointF GetDrawPosition(RectangleF pBounds,SizeF DrawSize,HorizontalAlignment pAlign)
        {
            float useX = GetDrawX(pBounds, DrawSize, pAlign);
            float useY = (pBounds.Top + pBounds.Height / 2 - (DrawSize.Height / 2));
            return new PointF(useX,useY);
        }
        Dictionary<double, Font> FontSizeData = new Dictionary<double, Font>();
        public Font GetScaledFont(IStateOwner pOwner)
        {
            lock (FontSizeData)
            {
                if (!FontSizeData.ContainsKey(pOwner.ScaleFactor))
                {
                    Font buildfont = new Font(this.Font.FontFamily, (float)(this.Font.Size * pOwner.ScaleFactor), this.Font.Style);
                    FontSizeData.Add(pOwner.ScaleFactor, buildfont);
                }
                return FontSizeData[pOwner.ScaleFactor];
            }
        }
        public static void DrawMenuText(IStateOwner pOwner, MenuTextDrawInfo DrawData, Graphics Target,Rectangle Bounds,StateMenuItemState DrawState)
        {
            //basically just draw the Text centered within the Bounds.
          
        }
        public override void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState)
        {
            var useFont = GetScaledFont(pOwner);
            var MeasureText = Target.MeasureString(Text, useFont);

            PointF DrawPosition = GetDrawPosition(Bounds, MeasureText, TextAlignment);
            Brush BackBrush = this.BackBrush;
            if (DrawState == StateMenuItemState.State_Selected)
                BackBrush = Brushes.DarkBlue;

            Target.FillRectangle(BackBrush, Bounds);

            StringFormat central = new StringFormat();
            central.Alignment = StringAlignment.Near;
            central.LineAlignment = StringAlignment.Near;
            Brush ForeBrush = this.ForeBrush;
            if (DrawState == StateMenuItemState.State_Selected)
                ForeBrush = Brushes.Aqua;
            Brush ShadowBrush = this.ShadowBrush;
            var useStyle = new DrawTextInformation()
            {
                Text = Text,
                BackgroundBrush = Brushes.Transparent,
                DrawFont = useFont,
                ForegroundBrush = ForeBrush,
                ShadowBrush = ShadowBrush,
                Position = DrawPosition,
                ShadowOffset = new PointF(5f, 5f),
                Format = central
            };
            if(DrawState==StateMenuItemState.State_Selected)
            {
                useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculator());
            }
            TetrisGame.DrawText(Target, useStyle);

//            TetrisGame.DrawText(Target, useFont, Text, ForeBrush, ShadowBrush, DrawPosition.X, DrawPosition.Y, 5f, 5f, central);

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
