using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameObjects
{
    public class Particle
    {
        public BCPoint Position { get; set; }
        public BCPoint Velocity { get; set; }
        public BCPoint Decay { get; set; } = new BCPoint(0.95f, 0.95f);
        public BCColor Color { get; set; }

        public TimeSpan TTL { get; set; } = new TimeSpan(0, 0, 0, 0, 300);
        public DateTime? Birth { get; set; }
        public TimeSpan Age { get { return Birth == null ? TimeSpan.Zero : DateTime.Now - Birth.Value; } }
        public Particle(BCPoint pPosition,BCPoint pVelocity,BCColor pColor)
        {
            Position = pPosition;
            Velocity = pVelocity;
            Color = pColor;
        }
        //returns true if we should die.
        public bool GameProc(IStateOwner pOwner)
        {
            if (Birth == null) Birth = DateTime.Now;
            Position += Velocity;
            Velocity *= Decay;
            return Age > TTL;
        }



    }
}
