using BASeCamp.Rendering;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BASeTris.GameStates.Menu.MenuStateTextMenuItem;

namespace BASeTris.Rendering.Skia.MenuItems
{
    public interface ISizableMenuItemSkiaRenderingHandler
    {
        SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem item);
    }
#if false
    [RenderingHandler(typeof(MenuStateMenuItem), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]
    public class MenuStateMenuItemSkiaRenderer : IRenderingHandler<SKCanvas, MenuStateMenuItem, MenuStateMenuItemSkiaDrawData>
    {

        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {
            //
        }

        public void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateMenuItem)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateMenuItem Source)
        {
            if (Source is MenuStateSizedMenuItem sizer)
            {
                var RealRenderer = RenderingProvider.Static.GetHandler(Source.GetType(), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData));
                if (RealRenderer != null)
                {
                    if (RealRenderer is ISizableMenuItemSkiaRenderingHandler sizerender)
                    {
                        return sizerender.GetSize(pOwner, sizer);
                    }
                }
            }
            return SKPoint.Empty;

        }
    }


    [RenderingHandler(typeof(MenuStateTextMenuItem), typeof(SKCanvas), typeof(MenuStateMenuItemGDIPlusDrawData))]
    public class MenuStateTextMenuItemGDIRenderer : IRenderingHandler<SKCanvas, MenuStateTextMenuItem, MenuStateMenuItemSkiaDrawData>, ISizableMenuItemSkiaRenderingHandler
    {
        //protected Graphics Temp = Graphics.FromImage(new Bitmap(1, 1));
        /*static Dictionary<double, Dictionary<String, SKFontInfo>> FontSizeData = new Dictionary<double, Dictionary<String, SKFontInfo>>();
         
        public static SKFontInfo GetScaledFont(IStateOwner pOwner, String FontFace, float FontSize)
        {
            float findSize = (float)(FontSize * pOwner.ScaleFactor);

            return GetFont(FontFace, findSize);
        }
        public static SKFontInfo GetFont(String FontFace, float FontSize)
        {
            lock (FontSizeData)
            {
                if (!FontSizeData.ContainsKey(FontSize))
                {
                    var newdict = new Dictionary<String, SKFontInfo>(StringComparer.OrdinalIgnoreCase);
                    FontSizeData.Add(FontSize, newdict);
                }
                var SizeDict = FontSizeData[FontSize];
                if (!SizeDict.ContainsKey(FontFace))
                {
                    Font buildfont = new SKFontInfo(FontFace, FontSize);
                    SizeDict.Add(FontFace, buildfont);
                }
                return SizeDict[FontFace];
            }
        }*/
        
        public static SKFontInfo GetFont(float FontSize)
        {
            return new SKFontInfo(TetrisGame.RetroFontSK, FontSize);
        }
        public static SKFontInfo GetScaledFont(IStateOwner powner,float FontSize)
        {
            return GetFont((float)(FontSize * powner.ScaleFactor));
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateTextMenuItem)Source);
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateTextMenuItem Source)
        {
            //var testfont = GetScaledFont(pOwner, Source.FontFace, Source.FontSize);
            //var MeasureText = Temp.MeasureString(Source.Text, testfont);
            return SKPoint.Empty;
            //return MeasureText.ToSize();
        }

        private static float GetDrawX(SKRect pBounds, SKRect DrawSize, MenuHorizontalAlignment pAlign)
        {
            switch (pAlign)
            {
                case MenuHorizontalAlignment.Center:
                    return (pBounds.Left + pBounds.Width / 2) - DrawSize.Width / 2;
                case MenuHorizontalAlignment.Left:
                    return pBounds.Left;
                case MenuHorizontalAlignment.Right:
                    return pBounds.Right - DrawSize.Height;
            }
            return 0;
        }
        private static SKPoint GetDrawPosition(SKRect pBounds, SKRect DrawSize, MenuHorizontalAlignment pAlign)
        {
            float useX = GetDrawX(pBounds, DrawSize, pAlign);
            float useY = (pBounds.Top + pBounds.Height / 2 - (DrawSize.Width / 2));
            return new SKPoint(useX, useY);
        }
        private static SKPaint TransparentPaint = new SKPaint() { Color = SKColors.Transparent };
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateTextMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {
            var useFont = GetScaledFont(pOwner,Source.FontSize);
            var MeasureText = TetrisGame.MeasureSKText(useFont.TypeFace, useFont.FontSize, Source.Text);
            

            SKPoint DrawPosition = GetDrawPosition(Element.Bounds,  MeasureText, Source.TextAlignment);
            SKPaint BackPaint = null;
            

            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                BackPaint = new SKPaint() { Color = SKColors.DarkBlue };
            else
                BackPaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.BackColor) };
            pRenderTarget.DrawRect(Element.Bounds, BackPaint);


            SKPaint ForePaint = null;
            SKPaint ShadowPaint = null;
            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                ForePaint = new SKPaint() { Color = SKColors.Aqua };
            else
                ForePaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ForeColor)};

            ShadowPaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ShadowColor) };

            
           /* var useStyle = new DrawTextInformation()
            {
                Text = Source.Text,
                BackgroundBrush = Brushes.Transparent,
                DrawFont = useFont,
                ForegroundBrush = ForeBrush,
                ShadowBrush = ShadowBrush,
                Position = DrawPosition,
                ShadowOffset = new PointF(5f, 5f),
                Format = central
            };*/

            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
            {
                useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculator());
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
            Font useFont = MenuStateTextMenuItemGDIRenderer.GetScaledFont(pOwner, Source.FontFace, Source.FontSize);
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
#endif

}
