﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.MenuItems
{
    [RenderingHandler(typeof(MenuStateMenuItem), typeof(Graphics), typeof(MenuStateMenuItemDrawData))]
    public class MenuStateMenuItemGDIRenderer : IRenderingHandler<Graphics,MenuStateMenuItem,MenuStateMenuItemDrawData>
    {
        public void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemDrawData Element)
        {

            //Cheating...
            Source.Draw(pOwner,pRenderTarget,Element.Bounds,Element.DrawState);
        }

        public void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner,(Graphics)pRenderTarget,(MenuStateMenuItem)RenderSource,(MenuStateMenuItemDrawData)Element);
        }
    }
}