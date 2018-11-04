using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
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
}
