using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.Skia;
using BASeTris.TetrisBlocks;

namespace BASeTris.Rendering
{
    public class RenderingProvider : IRenderingProvider
    {
        public static RenderingProvider Static = new RenderingProvider();
        private Dictionary<Type, Dictionary<Type, IRenderingHandler>> handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler>>();
        bool InitProviderDictionary = false;
        public IRenderingHandler GetHandler(Type ClassType, Type DrawType, Type DrawDataType)
        {
            if (!InitProviderDictionary)
            {
                InitProviderDictionary = true;
                handlerLookup = new Dictionary<Type, Dictionary<Type, IRenderingHandler>>();


                handlerLookup.Add(typeof(Graphics), new Dictionary<Type, IRenderingHandler>()
                { { typeof(TetrisBlock),new TetrisBlockGDIRenderingHandler()},
                    { typeof(ImageBlock),new TetrisImageBlockGDIRenderingHandler()},
                    { typeof(StandardColouredBlock),new TetrisStandardColouredBlockGDIRenderingHandler() },
                    {typeof(StandardTetrisGameState),new StandardTetrisGameStateRenderingHandler()},
                    {typeof(MenuState),new MenuStateRenderingHandler()},
                    {typeof(PauseGameState),new PauseGameStateRenderingHandler()}
                });
                handlerLookup.Add(typeof(SkiaSharp.SKCanvas), new Dictionary<Type, IRenderingHandler>()
                { { typeof(TetrisBlock),new TetrisBlockSkiaRenderingHandler()},
                    { typeof(ImageBlock),new TetrisImageBlockSkiaRenderingHandler()},
                    { typeof(StandardColouredBlock),new TetrisStandardColouredBlockSkiaRenderingHandler() }
                });


            }
            if (handlerLookup.ContainsKey(ClassType))
            {
                if (handlerLookup[ClassType].ContainsKey(DrawType))
                {
                    return handlerLookup[ClassType][DrawType];
                }
            }
            return null;
        }
        public void DrawElement(IStateOwner pOwner, Object Target, Object Element, Object ElementData)
        {
            var Handler = GetHandler(Target.GetType(), Element.GetType(), ElementData.GetType());
            Handler.Render(pOwner, Target, Element, ElementData);
        }
    }
}
