
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.Skia.MenuItems;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    
    [RenderingHandler(typeof(MenuState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class MenuStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, MenuState, GameStateSkiaDrawParameters>
    {
        
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Bounds));
            }
            int CurrentIndex = Source.StartItemOffset;
            float CurrentY = DrawHeader(pOwner, Source, g, Bounds);
            float MaxHeight = 0, MaxWidth = 0;

            //we want to find the widest item.
            foreach (var searchitem in Source.MenuElements)
            {

                if (searchitem is MenuStateSizedMenuItem mss)
                {
                    var sizehandler = RenderingProvider.Static.GetHandler(typeof(SKCanvas), searchitem.GetType(), typeof(MenuStateMenuItemSkiaDrawData));
                    if (sizehandler is ISizableMenuItemSkiaRenderingHandler isizer)

                    {
                        var grabsize = isizer.GetSize(pOwner, mss);
                        grabsize = new SKPoint((float)(grabsize.X * pOwner.ScaleFactor*2), (float)(grabsize.Y * pOwner.ScaleFactor*2));
                        if (grabsize.Y > MaxHeight) MaxHeight = grabsize.Y;
                        if (grabsize.X > MaxWidth) MaxWidth = grabsize.X;

                    }

                }
            }
            //we draw each item at the maximum size.
            SKPoint ItemSize = new SKPoint(MaxWidth, MaxHeight);
            CurrentY += (float)(pOwner.ScaleFactor * 5);
            for (int menuitemindex = 0; menuitemindex < Source.MenuElements.Count; menuitemindex++)
            {
                var drawitem = Source.MenuElements[menuitemindex];
                var XPos = (int)(Bounds.Width / 2 - ItemSize.X / 2) + Source.MainXOffset;
                SKRect TargetBounds = new SKRect(XPos, (int)CurrentY, XPos + (int)(ItemSize.X), CurrentY + (int)(ItemSize.Y));
                MenuStateMenuItem.StateMenuItemState useState = menuitemindex == Source.SelectedIndex ? MenuStateMenuItem.StateMenuItemState.State_Selected : MenuStateMenuItem.StateMenuItemState.State_Normal;
                RenderingProvider.Static.DrawElement(pOwner, g, drawitem, new MenuStateMenuItemSkiaDrawData(TargetBounds, useState));
                //drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Y + 5;
            }
            if (CursorBitmap == null)
            {
                CursorBitmap = SkiaSharp.Views.Desktop.Extensions.ToSKImage(new System.Drawing.Bitmap(TetrisGame.Imageman["cursor"]));
                using (SKCanvas canvo = new SKCanvas(new SKBitmap(CursorBitmap.Width, CursorBitmap.Height)))
                {
                    canvo.Clear(SKColors.Transparent);
                    canvo.DrawImage(CursorBitmap, new SKPoint(0, 0));
                }
            }


            g.DrawImage(CursorBitmap, Source.LastMouseMovement, null);

        }
        SKImage CursorBitmap = null;
        protected SKFontInfo GetScaledHeaderFont(IStateOwner pOwner, MenuState Source)
        {
            return MenuStateTextMenuItemSkiaRenderer.GetScaledFont(pOwner, Source.HeaderTypeSize);
            
        }
        static SKPaint Painter = null;
        static SKPaint BackPainter = null;
        public virtual float DrawHeader(IStateOwner pOwner, MenuState Source, SKCanvas Target, SKRect Bounds)
        {
            if (String.IsNullOrEmpty(Source.StateHeader)) return 0;
            SKFontInfo useHeaderFont = GetScaledHeaderFont(pOwner, Source);
            Painter = new SKPaint() { Color = SKColors.Black };
            BackPainter = new SKPaint() { Color = SKColors.White };
            BackPainter.Typeface = Painter.Typeface = useHeaderFont.TypeFace;
            BackPainter.TextSize = Painter.TextSize = (float)(useHeaderFont.FontSize * pOwner.ScaleFactor);


            SKRect HeaderSize = new SKRect();
            Painter.MeasureText(Source.StateHeader??"", ref HeaderSize);
            
            float UseX = (Bounds.Width / 2) - (HeaderSize.Width / 2) + Source.MainXOffset;
            float UseY = HeaderSize.Height + HeaderSize.Height / 3;
            //paint foreground.
            Target.DrawText(Source.StateHeader ?? "", new SKPoint(UseX + 5, UseY + 5), BackPainter);
            Target.DrawText(Source.StateHeader ?? "", new SKPoint(UseX, UseY), Painter);


            //TetrisGame.DrawTextSK(Target, Source.StateHeader,new SKPoint(UseX, UseY), useHeaderFont, SKColors.Black, SKColors.White);

            return UseY + HeaderSize.Height;
        }
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
}
}
