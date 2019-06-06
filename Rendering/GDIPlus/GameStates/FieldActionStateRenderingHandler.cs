using System.Drawing;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    public class FieldActionStateRenderingHandler :StandardStateRenderingHandler<Graphics,FieldActionGameState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {
            if (Source._BaseState != null)
                RenderingProvider.Static.DrawElement(pOwner,pRenderTarget,Source._BaseState,Element);
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, FieldActionGameState Source, GameStateDrawParameters Element)
        {  if(Source._BaseState!=null)
            RenderingProvider.Static.DrawStateStats(pOwner,pRenderTarget,Source._BaseState,Element);
        }
    }
}