﻿
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
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

            /*
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget,Source.BG, new SkiaBackgroundDrawData(Element.Bounds));
                //RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new GDIBackgroundDrawData(Bounds));
            }
            int CurrentIndex = Source.StartItemOffset;
            float CurrentY = Source.DrawHeader(pOwner, g, Bounds);
            float MaxHeight = 0, MaxWidth = 0;
            //we want to find the widest item.
            foreach (var searchitem in Source.MenuElements)
            {
                if (searchitem is MenuStateSizedMenuItem mss)
                {
                    var grabsize = mss.GetSize(pOwner);
                    if (grabsize.Height > MaxHeight) MaxHeight = grabsize.Height;
                    if (grabsize.Width > MaxWidth) MaxWidth = grabsize.Width;
                }
            }
            //we draw each item at the maximum size.
            SizeF ItemSize = new SizeF(MaxWidth, MaxHeight);
            CurrentY += (float)(pOwner.ScaleFactor * 5);
            for (int menuitemindex = 0; menuitemindex < Source.MenuElements.Count; menuitemindex++)
            {
                var drawitem = Source.MenuElements[menuitemindex];
                Rectangle TargetBounds = new Rectangle((int)(Bounds.Width / 2 - ItemSize.Width / 2) + Source.MainXOffset, (int)CurrentY, (int)(ItemSize.Width), (int)(ItemSize.Height));
                MenuStateMenuItem.StateMenuItemState useState = menuitemindex == Source.SelectedIndex ? MenuStateMenuItem.StateMenuItemState.State_Selected : MenuStateMenuItem.StateMenuItemState.State_Normal;
                RenderingProvider.Static.DrawElement(pOwner, g, drawitem, new MenuStateMenuItemDrawData(TargetBounds, useState));
                //drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Height + 5;
            }
            */
        }


        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            throw new NotImplementedException();
        }
    }
}