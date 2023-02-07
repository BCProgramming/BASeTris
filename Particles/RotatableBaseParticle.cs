using BASeTris.Rendering.Adapters;

namespace BASeTris.Particles
{
    public abstract class RotatableBaseParticle :BaseParticle
    {
        protected RotatableBaseParticle(BCPoint pPosition,BCPoint pVelocity,BCColor pColor):base(pPosition,pVelocity,pColor)
        {
        }

        public override bool GameProc(IStateOwner pOwner)
        {
            Angle += AngleDelta;
            AngleDelta *= AngleDecay;
            return base.GameProc(pOwner);
        }
        public double Angle { get; set; } = 0;
        public double AngleDelta { get; set; } = 0;

        public double AngleDecay { get; set; } = 0.95d;
    }
}
