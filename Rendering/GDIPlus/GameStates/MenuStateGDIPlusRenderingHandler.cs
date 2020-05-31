using System.Collections.Generic;
using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(MenuState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class MenuStateGDIPlusRenderingHandler :StandardStateRenderingHandler<Graphics,MenuState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,Source.BG,new GDIBackgroundDrawData(Bounds));
            }
            int CurrentIndex = Source.StartItemOffset;
            float CurrentY = DrawHeader(pOwner,Source, g, Bounds);
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
                RenderingProvider.Static.DrawElement(pOwner,g,drawitem,new MenuStateMenuItemDrawData(TargetBounds,useState) );
                //drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Height + 5;
            }
        }
        protected Dictionary<double, Font> FontSizeData = new Dictionary<double, Font>();
        protected Font GetScaledHeaderFont(IStateOwner pOwner,MenuState Source)
        {
            lock (FontSizeData)
            {
                if (!FontSizeData.ContainsKey(pOwner.ScaleFactor))
                {
                    Font buildfont = new Font(Source.HeaderFont.FontFamily, (float)(Source.HeaderFont.Size * pOwner.ScaleFactor), Source.HeaderFont.Style);
                    FontSizeData.Add(pOwner.ScaleFactor, buildfont);
                }
                return FontSizeData[pOwner.ScaleFactor];
            }
        }
        public virtual float DrawHeader(IStateOwner pOwner, MenuState Source,Graphics Target, RectangleF Bounds)
        {

            Font useHeaderFont = GetScaledHeaderFont(pOwner,Source);
            var HeaderSize = Target.MeasureString(Source.StateHeader, useHeaderFont);
            float UseX = (Bounds.Width / 2) - (HeaderSize.Width / 2) + Source.MainXOffset;
            float UseY = HeaderSize.Height / 3;

            TetrisGame.DrawText(Target, useHeaderFont, Source.StateHeader, Brushes.Black, Brushes.White, UseX, UseY);

            return UseY + HeaderSize.Height;
        }
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}