using System.Drawing;
using BASeCamp.Rendering;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(FieldActionGameState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class FieldActionStateGDIPlusRenderingHandler :StandardStateRenderingHandler<Graphics,FieldActionGameState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {
            if (Source._BaseState != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._BaseState, Element);
                Source.DrawForegroundEffect(pOwner,pRenderTarget,Element.Bounds);
            }
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {  if(Source._BaseState!=null)
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._BaseState,Element);
        }
    }
}