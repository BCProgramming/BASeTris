using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace BaseTris
{
    class Win10MenuRenderer : ToolStripSystemRenderer
    {
        protected bool _Blur = true;
        protected Color AccentColor = Color.Orange;
        protected static Color DarkColor = Color.FromArgb(110, 50, 35, 10);
        protected static Brush DarkBrush = new SolidBrush(DarkColor);
        protected static Brush DarkBrushOpaque = new SolidBrush(Color.FromArgb(DarkColor.R, DarkColor.G, DarkColor.B));
        public Win10MenuRenderer(Color? pAccentColor = null, bool pBlur = true)
        {

            _Blur = pBlur;
            if (pAccentColor != null)
            {
                AccentColor = pAccentColor.Value;
            }
            else
            {
                AccentColor = GetWindowColorizationColor(false);
                DarkBrush = new SolidBrush(Color.FromArgb(AccentColor.A / 2, 35, 35, 35));
            }
        }

        private static Color GetWindowColorizationColor(bool opaque)
        {
            DWMCOLORIZATIONPARAMS parms = new DWMCOLORIZATIONPARAMS();
            DWMNativeMethods.DwmGetColorizationParameters(ref parms);

            //Color.FromArgb(parms.ColorizationColor);
            return Color.FromArgb(
                (byte)(opaque ? 255 : (parms.ColorizationColor >> 24) / 2),
                (byte)(parms.ColorizationColor >> 16),
                (byte)(parms.ColorizationColor >> 8),
                (byte)parms.ColorizationColor
            );
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (_Blur)
            {
                //if set to use dwm blur, clear to transparent, paint some "darkening", then color it with the accent color.
                e.Graphics.Clear(Color.Transparent);
                e.Graphics.FillRectangle(DarkBrush, e.AffectedBounds);
                using (Brush AccentBrush = new SolidBrush(AccentColor))
                {
                    e.Graphics.FillRectangle(AccentBrush, e.AffectedBounds);
                }
            }
            else
            {
                e.Graphics.FillRectangle(DarkBrushOpaque, e.AffectedBounds);
                //base.OnRenderToolStripBackground(e);
            }
        }

        protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
        {
            if (!_Blur) base.OnRenderToolStripContentPanelBackground(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (!_Blur) base.OnRenderToolStripBorder(e);
            //e.Graphics.DrawRectangle(new Pen(Color.White,2),e.ToolStrip.Bounds);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle useBounds = new Rectangle(0, 0, e.Item.Bounds.Width, e.Item.Bounds.Height);

            OnRenderMenuItemBackground(e);

            e.Graphics.DrawLine(new Pen(Color.White, 2), useBounds.Left + 25, useBounds.Top + useBounds.Height / 2, useBounds.Right - 25, useBounds.Top + useBounds.Height / 2);
        }
        private static Size? CachedLargestImageSize = null;
        private static Size? CachedLargestTextSize = null;
        protected void CalcBoundaries(ToolStripItem item, Graphics g, out Rectangle TextBounds, out Rectangle ImageBounds)
        {
            //First: The Image size we want to consider is going to be the largest image size of the siblings.
            if (CachedLargestImageSize == null)
            {
                foreach (var iterateitem in item.Owner.Items)
                {
                    if (iterateitem is ToolStripMenuItem)
                    {
                        ToolStripMenuItem castToolItem = iterateitem as ToolStripMenuItem;
                        if (castToolItem.Image != null)
                        {
                            if (CachedLargestImageSize == null)
                                CachedLargestImageSize = castToolItem.Image.Size;
                            else
                            {
                                if (CachedLargestImageSize.Value.Width < castToolItem.Image.Size.Width)
                                    CachedLargestImageSize = new Size(castToolItem.Image.Size.Width, CachedLargestImageSize.Value.Height);
                                if (CachedLargestImageSize.Value.Height < castToolItem.Image.Size.Height)
                                    CachedLargestImageSize = new Size(CachedLargestImageSize.Value.Width, castToolItem.Image.Size.Height);
                            }
                        }
                        else if (castToolItem.Text.Length > 0)
                        {
                            var MeasureSize = g.MeasureString(castToolItem.Text, castToolItem.Font);
                            if (CachedLargestTextSize == null)
                                CachedLargestTextSize = new Size((int)MeasureSize.Width, (int)MeasureSize.Height);
                            else
                            {
                                if (CachedLargestTextSize.Value.Width < MeasureSize.Width)
                                    CachedLargestTextSize = new Size((int)MeasureSize.Width, CachedLargestTextSize.Value.Height);
                                if (CachedLargestTextSize.Value.Height < MeasureSize.Height)
                                    CachedLargestTextSize = new Size(CachedLargestTextSize.Value.Width, (int)MeasureSize.Height);
                            }

                        }
                    }
                }
            }

            ImageBounds = new Rectangle(item.Bounds.Left, item.Bounds.Top, CachedLargestImageSize.Value.Width, CachedLargestImageSize.Value.Height);
            TextBounds = new Rectangle(item.Bounds.Left + ImageBounds.Width, item.Bounds.Top, item.Bounds.Right - ImageBounds.Right, item.Bounds.Height);
            //item.Owner.Items 
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle useBounds = new Rectangle(0, 0, e.Item.Bounds.Width, e.Item.Bounds.Height);
            Rectangle ImageBounds = Rectangle.Empty;
            //if the item has an image, we want it to the left. Technically, we should respect the Alignment, but this isn't a general purpose "Win10 Style" menu renderer. At least not yet.

            if (!_Blur)
            {
                e.Graphics.FillRectangle(DarkBrush, useBounds);
            }
            Brush useBrush = null;

            if (e.Item.Selected)
            {
                Color Light1 = Color.FromArgb(128, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B);
                Color Light2 = Color.FromArgb(150, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B);
                useBrush = new LinearGradientBrush(useBounds, Light1, Light2, LinearGradientMode.Vertical);
            }
            else if (!e.Item.Enabled)
            {
                e.Graphics.FillRectangle(DarkBrush, useBounds);
            }
            else if ((
                e.Item.BackColor.R != SystemColors.Menu.R ||
                e.Item.BackColor.G != SystemColors.Menu.G ||
                e.Item.BackColor.B != SystemColors.Menu.B)
                )
            {
                e.Graphics.SetClip(useBounds);
                e.Graphics.Clear(e.Item.BackColor);



                //e.Graphics.FillRectangle(new SolidBrush(e.Item.BackColor),useBounds);
            }
            if (useBrush != null) e.Graphics.FillRectangle(useBrush, useBounds);

            if (e.Item.Selected) useBrush.Dispose();
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;
            Color originalColor = e.TextColor;
            if (e.Item.ForeColor.R == SystemColors.MenuText.R &&
               e.Item.ForeColor.G == SystemColors.MenuText.G &&
               e.Item.ForeColor.B == SystemColors.MenuText.B)

                e.TextColor = e.Item.Selected ? e.TextColor : Color.LightGray;
            if (!e.Item.Enabled)
            {
                e.Item.Enabled = true;
                //if (_Blur) e.Item.ForeColor = Color.Transparent;
                e.Item.ForeColor = Color.White;
                //e.Item.ForeColor = Color.SlateBlue;
                base.OnRenderItemText(e);
                e.Item.Enabled = false;
            }
            else
            {
                base.OnRenderItemText(e);
            }
            e.TextColor = originalColor;
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle useBounds = new Rectangle(0, 0, e.Item.Bounds.Width, e.Item.Bounds.Height);
            if (e.Item is ToolStripMenuItem)
            {
                var tsItem = e.Item as ToolStripMenuItem;
                if (tsItem.Checked)
                {
                    //e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.AntiqueWhite)), new Rectangle(0, 0, useBounds.Height, useBounds.Height));

                    //e.Graphics.DrawString("b",new FontFamily("Marlett"),useBounds.Height,FontStyle.Bold),  GraphicsUnit.Pixel);
                    e.Graphics.DrawString("b", new Font(new FontFamily("Marlett"), useBounds.Height, FontStyle.Bold, GraphicsUnit.Pixel), new SolidBrush(Color.White), 0, 0);

                }
            }
        }

        public static class DWMNativeMethods
        {
            [Flags]
            public enum DWM_BB
            {
                Enable = 1,
                BlurRegion = 2,
                TransitionMaximized = 4
            }
            internal enum WindowCompositionAttribute
            {
                WCA_ACCENT_POLICY = 19
            }

            internal enum AccentState
            {
                ACCENT_DISABLED = 0,
                ACCENT_ENABLE_GRADIENT = 1,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_INVALID_STATE = 4
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct AccentPolicy
            {
                public AccentState AccentState;
                public int AccentFlags;
                public int GradientColor;
                public int AnimationId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DWM_BLURBEHIND
            {
                public DWM_BB dwFlags;
                public bool fEnable;
                public IntPtr hRgnBlur;
                public bool fTransitionOnMaximized;

                public DWM_BLURBEHIND(bool enabled)
                {
                    fEnable = enabled;
                    hRgnBlur = IntPtr.Zero;
                    fTransitionOnMaximized = false;
                    dwFlags = DWM_BB.Enable;
                }

                public Region Region
                {
                    get { return Region.FromHrgn(hRgnBlur); }
                }

                public bool TransitionOnMaximized
                {
                    get { return fTransitionOnMaximized; }
                    set
                    {
                        fTransitionOnMaximized = value;
                        dwFlags |= DWM_BB.TransitionMaximized;
                    }
                }

                public void SetRegion(Graphics graphics, Region region)
                {
                    hRgnBlur = region.GetHrgn(graphics);
                    dwFlags |= DWM_BB.BlurRegion;
                }
            }



            [DllImport("user32.dll")]
            private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
            [DllImport("user32.dll")]
            private static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);
            private const int LWA_ALPHA = 0x2;
            private const int LWA_COLORKEY = 0x1;
            private const int WS_EX_LAYERED = 0x80000;
            private const int GWL_EXSTYLE = -20;

            [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
            private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);


            [DllImport("dwmapi.dll", EntryPoint = "#127")]
            internal static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONPARAMS parms);

            public static void EnableBlur(IntPtr WindowHandle, bool pEnable = true)
            {
                var accent = new AccentPolicy();
                var accentStructSize = Marshal.SizeOf(accent);
                accent.AccentState = pEnable ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_DISABLED;

                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData();
                data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;
                SetWindowLong(WindowHandle, GWL_EXSTYLE, WS_EX_LAYERED);
                SetLayeredWindowAttributes(WindowHandle, Color.FromArgb(0, 0, 0, 0).ToArgb(), 255, LWA_ALPHA);
                SetWindowCompositionAttribute(WindowHandle, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WindowCompositionAttributeData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }
        }

        public struct DWMCOLORIZATIONPARAMS
        {
            public uint ColorizationColor,
                ColorizationAfterglow,
                ColorizationColorBalance,
                ColorizationAfterglowBalance,
                ColorizationBlurBalance,
                ColorizationGlassReflectionIntensity,
                ColorizationOpaqueBlend;
        }
    }
}