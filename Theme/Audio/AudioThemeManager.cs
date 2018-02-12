using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;

namespace BASeTris.Theme.Audio
{
    public class AudioThemeManager
    {
        private AudioTheme CurrentTheme;
        private Dictionary<AudioThemeElement, String> CachedStatics = new Dictionary<AudioThemeElement, string>();

        public void ResetTheme()
        {
            CachedStatics = new Dictionary<AudioThemeElement, string>();
        }
        public AudioThemeManager(AudioTheme UseTheme)
        {
            CurrentTheme = UseTheme;
        }
        
        private String GetThemeProperty(String pProp)
        {
            PropertyInfo grabprop = CurrentTheme.GetType().GetProperty(pProp,typeof(AudioThemeElement));
            Object result = grabprop.GetGetMethod().Invoke(CurrentTheme,new object[]{});
            AudioThemeElement CastResult = result as AudioThemeElement;
            if (CastResult.AudioKeys.Length == 1) return CastResult.AudioKeys[0];
            if (CastResult.ChooseStatic)
            {
                if (!CachedStatics.ContainsKey(CastResult))
                {
                    CachedStatics[CastResult] = TetrisGame.Choose(CastResult.AudioKeys);
                }
                return CachedStatics[CastResult];
            }
            else
            {
                return TetrisGame.Choose(CastResult.AudioKeys);
            }
            
        }

        public String BlockGroupMove => GetThemeProperty(nameof(this.BlockGroupMove));
        public String BlockGroupPlace => GetThemeProperty(nameof(this.BlockGroupPlace));

        public String BlockGroupRotate => GetThemeProperty(nameof(this.BlockGroupRotate));

        public String ClearLine => GetThemeProperty(nameof(this.ClearLine));
        public String ClearTetris => GetThemeProperty(nameof(this.ClearTetris));

        public String BackgroundMusic => GetThemeProperty(nameof(this.BackgroundMusic));
        public String GameOverShade => GetThemeProperty(nameof(this.GameOverShade));

        public String Pause => GetThemeProperty(nameof(this.Pause));

        public String GameOver => GetThemeProperty(nameof(this.GameOver));

        public String LevelUp => GetThemeProperty(nameof(this.LevelUp));

        public String Hold => GetThemeProperty(nameof(this.Hold));
        
        /*  public String[] BlockGroupMove;
        public String[] BlockGroupPlace;
        public String[] BlockGroupRotate;
        public String[] ClearLine;
        public String[] ClearTetris;
        public String[] BackgroundMusic;
        public String[] GameOverShade;
        public String[] Pause;
        public String[] GameOver;*/



    }
}
