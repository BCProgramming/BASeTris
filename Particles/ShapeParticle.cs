using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Particles
{
    public class ShapeParticle : BaseParticle
    {
        public enum ShapeTypes
        {
            Square,
            Ellipse
        }
        public float Size { get; set; } = 3;
        public ShapeTypes ShapeType { get; set; } = ShapeTypes.Square;
        public ShapeParticle(BCPoint pPosition, BCPoint pVelocity, BCColor pColor) : base(pPosition, pVelocity, pColor)
        {
        }
        public ShapeParticle(BCPoint pPosition, BCPoint pVelocity, BCColor[] pColors) : base(pPosition, pVelocity, pColors.First())
        {
            ColorCalculatorFunction = (e) => pColors[TetrisGame.StatelessRandomizer.Next(pColors.Length)];
        }
    }
}
