using BASeCamp.Rendering;
using BASeTris.GameStates.GameObjects;
using BASeTris.Particles;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{

    [RenderingHandler(typeof(List<GameObject>), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class GameObjectRenderingSkiaHandler : StandardRenderingHandler<SKCanvas, List<GameObject>, GameStateSkiaDrawParameters>
    {
        static Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>> GameObjectRenderProviders = new Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>>();
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, List<GameObject> Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }

        private BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner> GetProvider(Type forType)
        {
            if (!GameObjectRenderProviders.ContainsKey(forType))
            {
                var getrenderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), forType, typeof(GameStateSkiaDrawParameters));
                if (getrenderer != null)
                    GameObjectRenderProviders.Add(forType, getrenderer);

            }
            return GameObjectRenderProviders[forType];
        }
    }


}
