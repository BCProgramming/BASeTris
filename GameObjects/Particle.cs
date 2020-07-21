using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameObjects
{
    public class BaseParticle
    {
        public BCPoint Position { get; set; }
        public BCPoint Velocity { get; set; }
        public BCPoint Decay { get; set; } = new BCPoint(0.95f, 0.95f);
        public BCColor Color { get; set; }
        
        

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
    public class CharParticle : BaseParticle
    {
        public String Text = " ";
        public BCFont FontInfo = new BCFont("Pixel Emulator", 16, BCFont.BCFontStyle.Regular);
        public CharParticle(BCPoint pPosition,BCPoint pVelocity,BCColor pColor,String pText) :base(pPosition,pVelocity,pColor)
        {
            Text = pText;
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
