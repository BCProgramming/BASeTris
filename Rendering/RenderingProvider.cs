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
    public class RenderAbstractionException : Exception
    {
        public RenderAbstractionException(String pMessage):base(pMessage)
        {

        }
    }
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
                    {typeof(PauseGameState),new PauseGameStateRenderingHandler()},
                    {typeof(EnterTextState),new EnterTextStateRenderingHandler()},
                    {typeof(EnterCheatState),new EnterTextStateRenderingHandler() },
                    {typeof(GameOverGameState),new GameOverStateRenderingHandler() },
                    {typeof(FieldLineActionGameState),new  FieldActionStateRenderingHandler() },
                    {typeof(UnpauseDelayGameState),new UnpauseDelayStateRenderingHandler() },
                    {typeof(ShowHighScoresState),new ShowHighScoreStateRenderingHandler()},
                    {typeof(ViewScoreDetailsState),new ViewScoreDetailsStateHandler()}


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
            if(Handler==null)
            {
                throw new RenderAbstractionException("Type " + Element.GetType().Name + " Does not have a rendering provider for type " + Target.GetType().Name);
            }
            Handler.Render(pOwner, Target, Element, ElementData);
        }
        public void DrawStateStats(IStateOwner pOwner,Object Target, Object Element,Object ElementData)
        {
            var Handler = GetHandler(Target.GetType(), Element.GetType(), ElementData.GetType());
            if(Handler is IStateRenderingHandler)
            {
                (Handler as IStateRenderingHandler).RenderStats(pOwner,Target,Element,ElementData);
            }
        }
       public ExtendedData extendedInfo = new ExtendedData();
    }
    public class ExtendedData
    {
        //"per element data" is defined per element (the things being drawn, to be clear). Per state instance, per block instance, etc.
        // basically these can be used to store additional render handler specific fields/properties, for example it can be used to cache images or bitmaps or whatever from
        // one call to the next.

        private static System.Runtime.CompilerServices.ConditionalWeakTable<Object, Object> _extendedData = new System.Runtime.CompilerServices.ConditionalWeakTable<Object, Object>();
        /// <summary>
        /// Retrieves the instance information for the provided instance, or null of there is no current instance information.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public Object GetPerElementData(Object Item)
        {
            if (_extendedData.TryGetValue(Item, out Object result))
            {
                //we got a result- return it.
                return result;
            }
            else
            {
                return null;
            }
        }
        public Object GetPerElementData(Object Item, Func<Object, Object> NewFunc)
        {
            lock (_extendedData)
            {
                if (_extendedData.TryGetValue(Item, out Object result))
                {
                    //we got a result- return it.
                    return result;
                }
                else
                {
                    Object generatedinstance = NewFunc(Item);
                    _extendedData.Add(Item, generatedinstance);
                    return generatedinstance;
                }
            }
        }
        public void SetPerElementData(Object Item, Object Value)
        {
            _extendedData.Add(Item, Value);
        }
    }
}
