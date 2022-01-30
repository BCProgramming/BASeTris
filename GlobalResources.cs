
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    static class GlobalResources
    {
        public static SKColorType DefaultColorType = SKColorType.Rgba8888;
        public static GRGlInterface OpenGLInterface { get; set; }

        public static GRBackendRenderTarget CreateRenderTarget(int Width,int Height)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int framebuffer);
            GL.GetInteger(GetPName.StencilBits, out int stencil);
            GL.GetInteger(GetPName.Samples, out int samples);
            stencil = stencil == 0 ? 1 : stencil;
            int bufferWidth = 0;
            int bufferHeight = 0;
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
            GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);

            return new GRBackendRenderTarget(Width, Height, samples, Math.Min(1,stencil), new GRGlFramebufferInfo((uint)framebuffer, DefaultColorType.ToGlSizedFormat()));

        }
    }
}
