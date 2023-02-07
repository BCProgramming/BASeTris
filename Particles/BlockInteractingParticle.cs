using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Particles
{
    internal class BlockInteractingParticle : BaseParticle
    {
        public BlockInteractingParticle(BCPoint pPosition, BCPoint pVelocity, BCColor pColor) : base(pPosition, pVelocity, pColor)
        {
        }
    }
}
