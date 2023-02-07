using BASeTris.Rendering.Adapters;
using System;

namespace BASeTris.Particles
{
    public class BitmapParticle : RotatableBaseParticle
    {
        public int Width { get; set; } = 3;
        public int Height { get; set; } = 3;
        public String Text { get; set; } = " ";
        public BCImage Image { get; set; } = null;

        public BCRect SourceClip = BCRect.Empty;
        
        public BitmapParticle(BCPoint pPosition, BCPoint pVelocity, BCColor pColor, BCImage img) : base(pPosition, pVelocity, pColor)
        {
            Image = img;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override bool GameProc(IStateOwner pOwner)
        {
            return base.GameProc(pOwner);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
