using BASeCamp.Rendering;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{
    [RenderingHandler(typeof(ThemeSelectionMenuState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ThemeSelectionStateSkiaRenderingHandler : MenuStateSkiaRenderingHandler,IRenderingHandler<SKCanvas,ThemeSelectionMenuState,GameStateSkiaDrawParameters>
    {
        static Dictionary<MenuStateMenuItem, SKBitmap> UseCollageBitmaps = new Dictionary<MenuStateMenuItem, SKBitmap>();
        
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuState Source, GameStateSkiaDrawParameters Element)
        {
            base.Render(pOwner, pRenderTarget, Source, Element);
            if (Source is ThemeSelectionMenuState ms) this.Render(pOwner, pRenderTarget, ms, Element);
        }

        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ThemeSelectionMenuState Source, GameStateSkiaDrawParameters Element)
        {
            //this is called AFTER the default menu drawing.
            //really, we just want to show a little preview thingie.
            var currentlyselected = Source.MenuElements[Source.SelectedIndex];
            SKBitmap UseCollageBitmap = null;
            if (UseCollageBitmaps.ContainsKey(currentlyselected))
            {
                UseCollageBitmap = UseCollageBitmaps[currentlyselected];
            }
            else 
            {
                
                NominoTheme useNominoTheme = null;
                if (currentlyselected.Tag is MenuStateThemeSelection msts)
                {
                    useNominoTheme = msts.GenerateThemeFunc();
                }
                else
                {
                    useNominoTheme = Source.InitialTheme==null?null:(NominoTheme)Activator.CreateInstance(Source.InitialTheme, new Object[] { });
                }
                if (useNominoTheme != null)
                {
                    UseCollageBitmap = TetrominoCollageRenderer.GetBackgroundCollage(useNominoTheme, 32);
                    UseCollageBitmaps[currentlyselected] = UseCollageBitmap;
                }
            }
            if (UseCollageBitmap != null)
            {
                double useSizing = 250 * pOwner.ScaleFactor;
                SKPoint RenderPoint = new SKPoint((float)(Element.Bounds.Width / 2 - (useSizing/2)), (float)(Element.Bounds.Height - useSizing)-(float)(25*pOwner.ScaleFactor));
                pRenderTarget.DrawBitmap(UseCollageBitmap, new SKRect(RenderPoint.X, RenderPoint.Y, RenderPoint.X + (float)useSizing, RenderPoint.Y + (float)useSizing));
            }
            
        }
        ~ThemeSelectionStateSkiaRenderingHandler()
        {
            foreach (var disposeit in UseCollageBitmaps.Values)
            {
                disposeit.Dispose();
            }
        }
    }
}
