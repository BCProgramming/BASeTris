using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;

namespace BASeTris.Rendering.GDIPlus
{
    /// <summary>
    /// Base DrawElement type
    /// </summary>
    public class GameStateDrawParameters
    {
        public RectangleF Bounds;
        public GameStateDrawParameters(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    
    public class StandardTetrisGameStateRenderingHandler : StandardStateRenderingHandler<Graphics, StandardTetrisGameState, GameStateDrawParameters>
    {

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            //render the stats image.
            //throw new NotImplementedException();
        }

        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, StandardTetrisGameState Source, GameStateDrawParameters Element)
        {
            Source._DrawHelper.DrawProc(Source, pOwner, pRenderTarget, Element.Bounds);
            //throw new NotImplementedException();
        }
    }
    public class PauseGameStateRenderingHandler: StandardStateRenderingHandler<Graphics, PauseGameState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            var FallImages = Source.FallImages;
            //Render the paused state.
            Font usePauseFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }

            g.ResetTransform();
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            TetrisGame.DrawText(g, usePauseFont, sPauseText, Brushes.White, Brushes.Black, DrawPos.X, DrawPos.Y);

            var basecall = RenderingProvider.Static.GetHandler(typeof(Graphics), typeof(MenuState), typeof(GameStateDrawParameters));
            basecall?.Render(pOwner,pRenderTarget,Source,Element); //draw the menu itself.

        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, PauseGameState Source, GameStateDrawParameters Element)
        {
            //delegate...

            
        }

       
    }
    public class MenuStateRenderingHandler :StandardStateRenderingHandler<Graphics,MenuState, GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            Graphics g = pRenderTarget;
            var Bounds = Element.Bounds;
            if (Source._BG != null) Source._BG.DrawProc(g, Bounds);
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
                drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Height + 5;
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, MenuState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
