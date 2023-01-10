using BASeTris.AssetManager;
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
    public class CharParticle : RotatableBaseParticle
    {
        public enum SpecialCharacterParticleFlags
        {
            Effect_Wave,
            Effect_Swirl,
            Effect_Jitter
        }
        public SpecialCharacterParticleFlags Flags { get; set; } = SpecialCharacterParticleFlags.Effect_Swirl;
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
