using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.BASeCamp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BASeCamp.Rendering.Interfaces;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.GDIPlus.Backgrounds;
using BASeTris.Rendering.MenuItems;
using BASeTris.Rendering.Skia;
using BASeTris.Rendering.Skia.Backgrounds;
using BASeTris.Rendering.Skia.GameStates;
using BASeTris.Blocks;

namespace BASeTris.Rendering
{
   public class RenderingProvider : BASeCamp.Rendering.RenderingProvider<IStateOwner>
    {
        public static RenderingProvider Static = new RenderingProvider();
        public void DrawStateStats(IStateOwner pOwner, Object Target, Object Element, Object ElementData)
        {
            var Handler = GetHandler(Target.GetType(), Element.GetType(), ElementData.GetType());
            if (Handler is IStateRenderingHandler)
            {
                (Handler as IStateRenderingHandler).RenderStats(pOwner, Target, Element, ElementData);
            }
        }
    }

}
