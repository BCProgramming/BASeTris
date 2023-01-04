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


    [RenderingHandler(typeof(MenuStateTextMenuItem), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]
    public class MenuStateTextMenuItemSkiaRenderer : IRenderingHandler<SKCanvas, MenuStateTextMenuItem, MenuStateMenuItemSkiaDrawData>, ISizableMenuItemSkiaRenderingHandler
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
        public virtual SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateTextMenuItem)Source);
        }
        public virtual SKPoint GetSize(IStateOwner pOwner, MenuStateTextMenuItem Source)
        {
            //TODO- implement
            SKPaint MeasurePaint = new SKPaint();
            var RetroFont = TetrisGame.RetroFontSK;
            MeasurePaint.Typeface = RetroFont;
            MeasurePaint.TextSize = Source.FontSize;
            SKRect result = new SKRect();
            
            MeasurePaint.MeasureText(new String(Enumerable.Repeat('█',(Source.Text??"").Length).ToArray()), ref result);
            return new SKPoint(result.Width, result.Height);
            //var testfont = GetScaledFont(pOwner, Source.FontFace, Source.FontSize);
            //var MeasureText = Temp.MeasureString(Source.Text, testfont);
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
            float useY = (pBounds.Top + pBounds.Height / 2 - (DrawSize.Height / 2));
            return new SKPoint(useX, useY+DrawSize.Height);
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
            SKPaint ShadePaint = null;
            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                ForePaint = new SKPaint() { Color = SKColors.Aqua };
            else
                ForePaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ForeColor),TextAlign=SKTextAlign.Center};

            ShadePaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ShadowColor), TextAlign = SKTextAlign.Center };


            ForePaint.Typeface = useFont.TypeFace;
            ForePaint.TextSize = (int)(Source.FontSize * pOwner.ScaleFactor);
            ShadePaint.Typeface = useFont.TypeFace;
            ShadePaint.TextSize = (int)(Source.FontSize * pOwner.ScaleFactor); 

            var useStyle = new DrawTextInformationSkia()
            {
                Text = Source.Text,
                BackgroundPaint = new SKPaint() { Color = SKColors.Transparent },
                DrawFont = useFont,
                ForegroundPaint = ForePaint,
                ShadowPaint = ShadePaint,
                Position = new SKPoint(DrawPosition.X,DrawPosition.Y+MeasureText.Height/2),
                ShadowOffset = new SKPoint(5f, 5f),
            };

            if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
            {
                useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculatorSkia());
            }

            pRenderTarget.DrawTextSK(useStyle);


            //            TetrisGame.DrawText(Target, useFont, Text, ForeBrush, ShadowBrush, DrawPosition.X, DrawPosition.Y, 5f, 5f, central);

            //Cheating...
            // Source.Draw(pOwner,pRenderTarget,Element.Bounds,Element.DrawState);
        }

        public virtual void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateTextMenuItem)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
    }
    [RenderingHandler(typeof(MenuStateMultiOption), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]


    public class MenuStateMultiOptionItemSkiaRenderer : MenuStateTextMenuItemSkiaRenderer, IRenderingHandler<SKCanvas, MenuStateMultiOption, MenuStateMenuItemSkiaDrawData>, ISizableMenuItemSkiaRenderingHandler
    {

        public override SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateMultiOption)Source);
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateMultiOption Source)
        {
            return base.GetSize(pOwner, Source);
        }
        public override void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateMultiOption)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMultiOption Source, MenuStateMenuItemSkiaDrawData Element)
        {
            SKFontInfo useFont = MenuStateTextMenuItemSkiaRenderer.GetScaledFont(pOwner, Source.FontSize);
            var OptionManager = Source.OptionManagerBase;
            var Bounds = Element.Bounds;
            String sLeftCover = "< ";
            String sRightCover = ">";
            String PrevItem = null, NextItem = null;
            try
            {
                PrevItem = OptionManager.GetTextBase(OptionManager.PeekPreviousBase());
                NextItem = OptionManager.GetTextBase(OptionManager.PeekNextBase());
            }
            catch (IndexOutOfRangeException ire)
            {
                PrevItem = "";
                NextItem = "";
            }
            sLeftCover = PrevItem + sLeftCover;
            sRightCover = sRightCover + NextItem;
            
            SKPaint Foreground = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ForeColor) };
            SKPaint Background = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.BackColor) };
            SKPaint Shadow = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ShadowColor) };
            DrawTextInformationSkia dtis = new DrawTextInformationSkia() { ForegroundPaint = Foreground, BackgroundPaint = Background, ShadowPaint = Shadow };
            dtis.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, Source.FontSize);
            //TODO: need to get this implemented via a SKPaint, but the TextMenu item should probably have draw data in an "abstracted" form...
            SKRect MeasureLeft = new SKRect();
            SKRect MeasureRight = new SKRect();
            Foreground.MeasureText(sLeftCover,ref MeasureLeft);
            
            Foreground.MeasureText(sRightCover,ref MeasureRight);
            //var MeasureLeft = pRenderTarget.MeasureString(sLeftCover, useFont);
            //var MeasureRight = pRenderTarget.MeasureString(sRightCover, useFont);
            SKPoint LeftPos = new SKPoint(Bounds.Left - MeasureLeft.Width, Bounds.Top + (Bounds.Height / 2) - MeasureLeft.Height / 2);
            SKPoint RightPos = new SKPoint(Bounds.Right, Bounds.Top + (Bounds.Height / 2) - MeasureRight.Height / 2);

            
            if (Source.Activated)
            {
                dtis.Text = sLeftCover;
                pRenderTarget.DrawTextSK(dtis);
                dtis.Text = sRightCover;
                pRenderTarget.DrawTextSK(dtis);
            }
            base.Render(pOwner, pRenderTarget, Source, Element);
        }
    }


}
