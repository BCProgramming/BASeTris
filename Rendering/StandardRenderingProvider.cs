using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Elementizer;
using BASeCamp.Rendering.Interfaces;

namespace BASeTris.Rendering
{
    public abstract class StandardRenderingHandler<TClassType,TDrawType,TDataType> : StandardRenderingHandler<TClassType,TDrawType,TDataType,IStateOwner> where TDrawType:class
    {

    }


    //derived implementation- RenderingHandler specifically for GameState classes.
    //Primarily, these add "RenderStats".
    /// <summary>
    /// abstract State Rendering base class.
    /// </summary>
    /// <typeparam name="TClassType">The class type of the draw target. (Graphics canvas for example)</typeparam>
    /// <typeparam name="TDrawType">Class type of the object being drawn. Must be a BASeTris GameState.</typeparam>
    /// <typeparam name="TDataType">Class type that holds additional data for the operation.</typeparam>
    public abstract class StandardStateRenderingHandler<TClassType,TDrawType,TDataType> : StandardRenderingHandler<TClassType, TDrawType, TDataType>, IStateRenderingHandler<TClassType, TDrawType, TDataType> where TDrawType: GameState
    {
        public abstract void RenderStats(IStateOwner pOwner, TClassType pRenderTarget, TDrawType Source, TDataType Element);
       
        public virtual void RenderStats(IStateOwner pOwner,object pRenderTarget,object Element,object ElementData)
        {
            this.RenderStats(pOwner, (TClassType)pRenderTarget, (TDrawType)Element, (TDataType)ElementData);
        }

        public void RenderGameObjects(IStateOwner pOwner, object pRenderTarget, object Element, object ElementData)
        {
            this.RenderGameObjects(pOwner, (TClassType)pRenderTarget, (TDrawType)Element, (TDataType)Element);
        }
        public void RenderGameObjects(IStateOwner pOwner, TClassType pRenderTarget, TDrawType Source, TDataType Element)
        {
            //base helper routine, renders the game objects. I mean, that's in the name, I suppose.

            foreach (var drawobject in Source.AllGameObjects())
            {
                //May need to generate a Data Element for the drawobject here. Might be able to share the same instance between calls depending on the contents... we need an actual GameObject implemented before this can be used.
                //Might be fine with the GameState Draw Parameters, though.
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, drawobject, Element);
            }


            
        }
    }

}
