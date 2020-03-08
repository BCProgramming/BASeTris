using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BASeTris.Rendering
{
    //builds upon the BASeCamp Rendering interfaces.
    public interface IRenderingHandler:BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>
    {
        
    }
    public interface IStateRenderingHandler : IRenderingHandler
    {
        void RenderStats(IStateOwner pOwner, Object pRenderTarget, Object RenderSource, Object Element);
    }
    /// <summary>
    /// defines a Rendering Handler. 
    /// </summary>
    /// <typeparam name="TRenderTarget">Type of this rendering form. This is the main input parameter that describes the draw routine- for example, it might be a Drawing.Graphics or even a separate class type.</typeparam>
    /// <typeparam name="TRenderSource">Type of the class instance this Handler is designed to draw. (eg. A Block, Ball, Character, tile, etc.)</typeparam>
    /// <typeparam name="TDataElement">Element type that contains additional data.</typeparam>
    public interface IRenderingHandler<in TRenderTarget,in TRenderSource,in TDataElement> :BASeCamp.Rendering.Interfaces.IRenderingHandler<TRenderTarget,TRenderSource,TDataElement,IStateOwner>, IRenderingHandler where TRenderSource:class
    {
        //rendering handler has pretty much one method- to draw to the appropriate type.
        void Render(IStateOwner pOwner, TRenderTarget pRenderTarget, TRenderSource Source,TDataElement Element);
    }
    /// <summary>
    /// abstract Rendering interface.
    /// </summary>
    /// <typeparam name="TClassType">The class type of the draw target. (Graphics canvas for example)</typeparam>
    /// <typeparam name="TDrawType">Class type of the object being drawn. Must be a BASeTris GameState.</typeparam>
    /// <typeparam name="TDataType">Class type that holds additional data for the operation.</typeparam>
    public interface IStateRenderingHandler<in TRenderTarget,in TRenderSource,in TDataElement> : IRenderingHandler<TRenderTarget,TRenderSource,TDataElement>,IStateRenderingHandler  where TRenderSource:GameState
    {
        void RenderStats(IStateOwner pOwner, TRenderTarget pRenderTarget, TRenderSource Source, TDataElement Element);
    }
    /// <summary>
    /// no op class that is simply usable as the second parameter of IRenderingHandler to indicate there is no extra Element Data.
    /// </summary>
    public sealed class NoDataElement
    {

    }
}
