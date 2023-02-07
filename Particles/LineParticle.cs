using BASeTris.Rendering.Adapters;

namespace BASeTris.Particles
{
    public class LineParticle : BaseParticle
    {
        public BCPoint EndPoint { get; set; }
        public LineParticle(BCPoint StartPoint,BCPoint EndPoint,BCPoint pVelocity,BCColor pColor):base(StartPoint,pVelocity,pColor)
        {

        }
        public override bool GameProc(IStateOwner pOwner)
        {
            EndPoint += Velocity;
            return base.GameProc(pOwner);
        }

    }
}
