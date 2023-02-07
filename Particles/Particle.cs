using BASeTris.AssetManager;
using BASeTris.Rendering.Adapters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Particles
{
    public class BaseParticle
    {
        public Dictionary<String, Object> TagData = new Dictionary<string, object>();
        public BCPoint Position { get; set; }
        public BCPoint Velocity { get; set; }
        public BCPoint Decay { get; set; } = new BCPoint(0.95f, 0.95f);
        private BCColor _SingleColor;
        public BCColor Color { get { if (ColorCalculatorFunction == null) return _SingleColor; else  return ColorCalculatorFunction(this); } set { _SingleColor = value; } }

        public Func<BaseParticle, BCColor> ColorCalculatorFunction = null;


        

        public static Func<BaseParticle, BCColor> GetRainbowColorFunc(IStateOwner pOwner, int cycletime = 2000)
        {
            return new Func<BaseParticle, BCColor>((o) =>
            {
                int timebase = cycletime;
                int hue = (int)((float)(pOwner.GetElapsedTime().Ticks % timebase) / (float)timebase * 240);
                BCColor usecolor = new HSLColor(hue, 200d, 128d);

                return usecolor;
            });
        }
        

        private static uint GetTickCount()
        {
            return TetrisGame.GetTickCount();
        }

        public uint TTL { get; set; } = 300;
        public uint? Birth { get; set; }
        public uint Age { get { return Birth == null ? 0 : GetTickCount() - Birth.Value; } }
        public BaseParticle(BCPoint pPosition,BCPoint pVelocity,BCColor pColor)
        {
            Position = pPosition;
            Velocity = pVelocity;
            Color = pColor;
        }
        //returns true if we should die.
        public virtual bool GameProc(IStateOwner pOwner)
        {
            if (Birth == null) Birth = GetTickCount();
            Position += Velocity;
            Velocity *= Decay;
            return Age > TTL;
        }
    }
}
