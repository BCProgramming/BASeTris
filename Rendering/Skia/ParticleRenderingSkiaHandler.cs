using BASeCamp.Rendering;
using BASeTris.GameObjects;
using BASeTris.GameStates;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{
    //[RenderingHandler(typeof(TetrisBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    //public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics, TetrisBlock, TetrisBlockDrawParameters>
    [RenderingHandler(typeof(List<Particle>), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ParticleRenderingSkiaHandler : StandardRenderingHandler<SKCanvas , List<Particle>, GameStateSkiaDrawParameters>
    {
        public void UpdateParticles(IStateOwner pOwner,List<Particle> Source)
        {
            List<Particle> RemoveParticles = new List<Particle>();
            lock (Source)
            {
                foreach (var iterate in Source)
                {
                    if (iterate.GameProc(pOwner))
                    {
                        RemoveParticles.Add(iterate);
                    }
                }
                foreach (var itrem in RemoveParticles)
                {
                    Source.Remove(itrem);
                }

            }
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, List<Particle> Source, GameStateSkiaDrawParameters Element)
        {
            if (Source.Count > 0)
            {
                Debug.Print("Rendering Particle Set of " + Source.Count.ToString());
                Debug.Print("Trace:" + new StackTrace().ToString());
            }
            foreach (var iterate in Source)
            {
                BCPoint usePosition = new BCPoint(iterate.Position.X, iterate.Position.Y);
                //have to try to get the standardRenderingProvider. If we can then we will use the coordinates as if they are a block position- otherwise, we use the coordinates directly.
                StandardTetrisGameState foundstandard = null;
                if(pOwner.CurrentState is StandardTetrisGameState standard)
                {
                    foundstandard = standard;
                }
                else if(pOwner.CurrentState is ICompositeState<StandardTetrisGameState> composite)
                {
                    foundstandard = composite.GetComposite();
                }
                
                if(foundstandard!=null)
                {
                    usePosition.X = foundstandard.PlayField.GetBlockWidth(Element.Bounds) * iterate.Position.X;
                    usePosition.Y = foundstandard.PlayField.GetBlockHeight(Element.Bounds) * iterate.Position.Y;
                }

                byte useAlpha = 255;

                var PercentAlpha = 1-((float)iterate.Age.Ticks / (float)iterate.TTL.Ticks);
                //clamp
                PercentAlpha = PercentAlpha > 1 ?1:PercentAlpha<0?0:PercentAlpha;
                useAlpha = (byte)(PercentAlpha * 255);

                using (SKPaint skp = new SKPaint() { Color = new SKColor(iterate.Color.R,iterate.Color.G,iterate.Color.B,useAlpha) })
                {
                    pRenderTarget.DrawRect(new SKRect(usePosition.X, usePosition.Y, usePosition.X + 2, usePosition.Y + 2), skp);
                }
               

            }
            if(Source.Count > 0)
            {
                ;
            }
            
                if(pOwner.CurrentState.GameProcSuspended)
                {
                    ;
                }
            
            UpdateParticles(pOwner,Source);
            
        }
    }
}
