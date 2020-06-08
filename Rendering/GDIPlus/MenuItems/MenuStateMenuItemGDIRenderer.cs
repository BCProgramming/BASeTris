using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.Rendering;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.MenuItems
{

    public interface ISizableMenuItemGDIPlusRenderingHandler
    {
        SizeF GetSize(IStateOwner pOwner, MenuStateSizedMenuItem item);
    }

    [RenderingHandler(typeof(MenuStateMenuItem), typeof(Graphics), typeof(MenuStateMenuItemGDIPlusDrawData))]
    public class MenuStateMenuItemGDIRenderer : IRenderingHandler<Graphics,MenuStateMenuItem,MenuStateMenuItemGDIPlusDrawData>
    {

        public void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemGDIPlusDrawData Element)
        {
//
        }

        public void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner,(Graphics)pRenderTarget,(MenuStateMenuItem)RenderSource,(MenuStateMenuItemGDIPlusDrawData)Element);
        }
        public SizeF GetSize(IStateOwner pOwner, MenuStateMenuItem Source)
        {
            if (Source is MenuStateSizedMenuItem sizer)
            {
                var RealRenderer = RenderingProvider.Static.GetHandler(Source.GetType(), typeof(Graphics), typeof(MenuStateMenuItemGDIPlusDrawData));
                if (RealRenderer != null)
                {
                    if (RealRenderer is ISizableMenuItemGDIPlusRenderingHandler sizerender)
                    {
                        return sizerender.GetSize(pOwner, sizer);
                    }
                }
            }
            return SizeF.Empty;

        }
    }


    [RenderingHandler(typeof(MenuStateTextMenuItem), typeof(Graphics), typeof(MenuStateMenuItemGDIPlusDrawData))]
    public class MenuStateTextMenuItemGDIRenderer : IRenderingHandler<Graphics,MenuStateTextMenuItem,MenuStateMenuItemGDIPlusDrawData>, ISizableMenuItemGDIPlusRenderingHandler
    {
        protected Graphics Temp = Graphics.FromImage(new Bitmap(1, 1));
        static Dictionary<double, Dictionary<String,Font>> FontSizeData = new Dictionary<double, Dictionary<String,Font>>();
        public static Font GetScaledFont(IStateOwner pOwner, String FontFace,float FontSize)
        {
            float findSize = (float)(FontSize * pOwner.ScaleFactor);

            return GetFont(FontFace, findSize);
        }
        public static Font GetFont(String FontFace, float FontSize)
        {
            lock(FontSizeData)
            {
                if(!FontSizeData.ContainsKey(FontSize))
                {
                    var newdict = new Dictionary<String, Font>(StringComparer.OrdinalIgnoreCase);
                    FontSizeData.Add(FontSize, newdict);
                }
                var SizeDict = FontSizeData[FontSize];
                if(!SizeDict.ContainsKey(FontFace))
                {
                    Font buildfont = new Font(FontFace, FontSize, FontStyle.Regular);
                    SizeDict.Add(FontFace, buildfont);
                }
                return SizeDict[FontFace];
            }
        }
        public SizeF GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateTextMenuItem)Source);
        }
        public SizeF GetSize(IStateOwner pOwner,MenuStateTextMenuItem Source)
        {
            var testfont = GetScaledFont(pOwner,Source.FontFace,Source.FontSize);
            var MeasureText = Temp.MeasureString(Source.Text, testfont);
            return MeasureText.ToSize();
        }
        private static float GetDrawX(RectangleF pBounds, SizeF DrawSize, HorizontalAlignment pAlign)
        {
            switch (pAlign)
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
        private static PointF GetDrawPosition(RectangleF pBounds, SizeF DrawSize, HorizontalAlignment pAlign)
        {
            float useX = GetDrawX(pBounds, DrawSize, pAlign);
            float useY = (pBounds.Top + pBounds.Height / 2 - (DrawSize.Height / 2));
            return new PointF(useX, useY);
        }
        public void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuStateTextMenuItem Source, MenuStateMenuItemGDIPlusDrawData Element)
        {
            var useFont = GetScaledFont(pOwner, Source.FontFace,Source.FontSize);
            var MeasureText = pRenderTarget.MeasureString(Source.Text, useFont);

            PointF DrawPosition = GetDrawPosition(Element.Bounds, MeasureText, Source.TextAlignment.GetGDIPlusAlignment());
            Brush BackBrush = Source.BackBrush;
            
            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                BackBrush = Brushes.DarkBlue;

            pRenderTarget.FillRectangle(BackBrush, Element.Bounds);

            StringFormat central = new StringFormat();
            central.Alignment = StringAlignment.Near;
            central.LineAlignment = StringAlignment.Near;
            Brush ForeBrush = Source.ForeBrush;
            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                ForeBrush = Brushes.Aqua;
            Brush ShadowBrush = Source.ShadowBrush;
            var useStyle = new DrawTextInformationGDI()
            {
                Text = Source.Text,
                BackgroundBrush = Brushes.Transparent,
                DrawFont = useFont,
                ForegroundBrush = ForeBrush,
                ShadowBrush = ShadowBrush,
                Position = DrawPosition,
                ShadowOffset = new PointF(5f, 5f),
                Format = central
            };
            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
            {
                useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculatorGDI());
            }
            TetrisGame.DrawText(pRenderTarget, useStyle);


            //            TetrisGame.DrawText(Target, useFont, Text, ForeBrush, ShadowBrush, DrawPosition.X, DrawPosition.Y, 5f, 5f, central);

            //Cheating...
            // Source.Draw(pOwner,pRenderTarget,Element.Bounds,Element.DrawState);
        }

        public virtual void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (Graphics)pRenderTarget, (MenuStateTextMenuItem)RenderSource, (MenuStateMenuItemGDIPlusDrawData)Element);
        }
    }
    [RenderingHandler(typeof(MenuStateMultiOption), typeof(Graphics), typeof(MenuStateMenuItemGDIPlusDrawData))]
    
    
    public class MenuStateMultiOptionItemGDIRenderer : MenuStateTextMenuItemGDIRenderer, IRenderingHandler<Graphics, MenuStateMultiOption, MenuStateMenuItemGDIPlusDrawData>, ISizableMenuItemGDIPlusRenderingHandler
    {

        public SizeF GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateMultiOption)Source);
        }
        public SizeF GetSize(IStateOwner pOwner, MenuStateMultiOption Source)
        {
            return base.GetSize(pOwner, Source);
        }
        public override void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (Graphics)pRenderTarget, (MenuStateMultiOption)RenderSource, (MenuStateMenuItemGDIPlusDrawData)Element);
        }
        public void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuStateMultiOption Source, MenuStateMenuItemGDIPlusDrawData Element)
        {
            Font useFont = MenuStateTextMenuItemGDIRenderer.GetScaledFont(pOwner,Source.FontFace,Source.FontSize);
            var OptionManager = Source.OptionManagerBase;
            var Bounds = Element.Bounds;
            String sLeftCover = "< ";
            String sRightCover = ">";

            var PrevItem = OptionManager.GetTextBase(OptionManager.PeekPreviousBase());
            var NextItem = OptionManager.GetTextBase(OptionManager.PeekNextBase());
            sLeftCover = PrevItem + sLeftCover;
            sRightCover = sRightCover + NextItem;
            var MeasureLeft = pRenderTarget.MeasureString(sLeftCover, useFont);
            var MeasureRight = pRenderTarget.MeasureString(sRightCover, useFont);

            PointF LeftPos = new PointF(Bounds.Left - MeasureLeft.Width, Bounds.Top + (Bounds.Height / 2) - MeasureLeft.Height / 2);
            PointF RightPos = new PointF(Bounds.Right, Bounds.Top + (Bounds.Height / 2) - MeasureRight.Height / 2);

            if (Source.Activated)
            {
                TetrisGame.DrawText(pRenderTarget, useFont, sLeftCover, Source.ForeBrush, Source.ShadowBrush, LeftPos.X, LeftPos.Y);
                TetrisGame.DrawText(pRenderTarget, useFont, sRightCover, Source.ForeBrush, Source.ShadowBrush, RightPos.X, RightPos.Y);
            }
            base.Render(pOwner, pRenderTarget, Source, Element);
        }
    }
    //public class MenuStateTextMenuItemGDIRenderer : IRenderingHandler<Graphics,MenuStateTextMenuItem,MenuStateMenuItemDrawData>, ISizableMenuItemGDIPlusRenderingHandler
}
