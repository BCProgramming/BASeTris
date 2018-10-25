using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    //IRenderingProviderFactory interface takes a class type which is the core of the Graphics API in question.
    //If necessary that class type can of course be a custom class which includes multiple pieces of data.
    //It acts as a factory for creating instances capable of drawing specific drawable class types.
    public interface IRenderingProvider<T>
    {

        IRenderingHandler<T> GetHandler(Type ElementType);

    }
    public interface IRenderingHandler
    {
        void Render(Object RenderTarget, Object DrawElement);
    }
    public interface IRenderingHandler<RenderType>:IRenderingHandler
    {
        void Render(RenderType RenderTarget, Object DrawElement);
    }
    public interface IRenderingHandler<RenderType,TargetType>: IRenderingHandler<RenderType>
    {
        void Render(RenderType RenderTarget,TargetType DrawElement);
    }
}
