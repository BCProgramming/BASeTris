using BASeTris.Rendering.Adapters;
using SkiaSharp;
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
        private BCColor _SingleColor;
        public BCColor Color { get { if (ColorCalculatorFunction != null) return ColorCalculatorFunction(this); else return _SingleColor; } set { _SingleColor = value; } }

        public Func<BaseParticle, BCColor> ColorCalculatorFunction = null;

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
    public class BitmapParticle : BaseParticle
    {
        public String Text = " ";
        public SKImage _Image = null;
        public BitmapParticle(BCPoint pPosition, BCPoint pVelocity, BCColor pColor, SKImage img) : base(pPosition, pVelocity, pColor)
        {
            _Image = img;
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
    public class CharParticle : BaseParticle
    {
        public String Text = " ";
        public BCFont FontInfo = new BCFont("Pixel Emulator", 32, BCFont.BCFontStyle.Regular);
        System.Drawing.Font useFont = TetrisGame.GetRetroFont(1, 1, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        public CharParticle(BCPoint pPosition,BCPoint pVelocity,BCColor pColor,String pText) :base(pPosition,pVelocity,pColor)
        {
            Text = pText;
            
            FontInfo = new BCFont(useFont.FontFamily.Name, 1, BCFont.BCFontStyle.Regular);
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
