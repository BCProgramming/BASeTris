using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    public interface IRenderingHandler
    {
        void Render(IStateOwner pOwner, Object pRenderTarget, Object RenderSource,Object Element);
    }
    /// <summary>
    /// defines a Rendering Handler. 
    /// </summary>
    /// <typeparam name="TRenderTarget">Type of this rendering form. This is the main input parameter that describes the draw routine- for example, it might be a Drawing.Graphics or even a separate class type.</typeparam>
    /// <typeparam name="TRenderSource">Type of the class instance this Handler is designed to draw. (eg. A Block, Ball, Character, tile, etc.)</typeparam>
    /// <typeparam name="TDataElement">Element type that contains additional data.</typeparam>
    public interface IRenderingHandler<in TRenderTarget,in TRenderSource,in TDataElement> : IRenderingHandler where TRenderSource:class
    {
        //rendering handler has pretty much one method- to draw to the appropriate type.
        void Render(IStateOwner pOwner, TRenderTarget pRenderTarget, TRenderSource Source,TDataElement Element);
    }
    /// <summary>
    /// no op class that is simply usable as the second parameter of IRenderingHandler to indicate there is no extra Element Data.
    /// </summary>
    public sealed class NoDataElement
    {

    }
}
