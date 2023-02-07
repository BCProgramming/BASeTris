using BASeTris.Rendering.Adapters;
using System;

namespace BASeTris.Particles
{
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
