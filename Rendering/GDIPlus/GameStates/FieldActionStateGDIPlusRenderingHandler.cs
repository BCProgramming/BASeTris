using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(FieldActionGameState), typeof(Graphics), typeof(BaseDrawParameters))]
    public class FieldActionStateGDIPlusRenderingHandler :StandardStateRenderingHandler<Graphics,FieldActionGameState,BaseDrawParameters>
    {
        SolidBrush FlashBrush = new SolidBrush(Color.FromArgb(128, Color.White));
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, BaseDrawParameters Element)
        {
            if (Source._BaseState != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._BaseState, Element);
                if(Source is FieldLineActionGameState linestate)
                {
                    if(linestate.FlashState)
                    {
                        pRenderTarget.FillRectangle(FlashBrush, Element.Bounds);
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, BaseDrawParameters Element)
        {  if(Source._BaseState!=null)
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._BaseState,Element);
        }
    }
}