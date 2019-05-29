using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    //Concrete base class IRenderingHandler.
    /// <summary>
    /// abstract Rendering base class.
    /// </summary>
    /// <typeparam name="TClassType">The class type of the draw target. (Graphics canvas for example)</typeparam>
    /// <typeparam name="TDrawType">Class type of the object being drawn.</typeparam>
    /// <typeparam name="TDataType">Class type that holds additional data for the operation.</typeparam>
    public abstract class StandardRenderingHandler<TClassType, TDrawType, TDataType> : IRenderingHandler<TClassType, TDrawType, TDataType> where TDrawType : class
    {
        public abstract void Render(IStateOwner pOwner, TClassType pRenderTarget, TDrawType Source, TDataType Element);


        public void Render(IStateOwner pOwner, object pRenderTarget, object Element, object ElementData)
        {
            this.Render(pOwner, (TClassType)pRenderTarget, (TDrawType)Element,(TDataType)ElementData);
        }
        //StandardRenderingHandler keeps a Weak
        public static System.Runtime.CompilerServices.ConditionalWeakTable<TDrawType, Object> extendedData = new System.Runtime.CompilerServices.ConditionalWeakTable<TDrawType, Object>();
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
    }

}
