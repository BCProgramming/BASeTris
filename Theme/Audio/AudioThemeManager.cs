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
        public AudioTheme CurrentTheme { get; set; }
        private Dictionary<AudioThemeElement, AudioThemeSelection> CachedStatics = new Dictionary<AudioThemeElement, AudioThemeSelection>();

        public void ResetTheme()
        {
            CachedStatics = new Dictionary<AudioThemeElement, AudioThemeSelection>();
        }

        public AudioThemeManager(AudioTheme UseTheme)
        {
            CurrentTheme = UseTheme;
        }

        private AudioThemeSelection GetThemeProperty(String pProp)
        {
            PropertyInfo grabprop = CurrentTheme.GetType().GetProperty(pProp, typeof(AudioThemeElement));
            Object result = grabprop.GetGetMethod().Invoke(CurrentTheme, new object[] { });
            AudioThemeElement CastResult = result as AudioThemeElement;
            if (CastResult.AudioKeys.Length == 1) return CastResult.AudioKeys[0];
            if (CastResult.ChooseFlag==AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static)
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

        public AudioThemeSelection BlockGroupMove => GetThemeProperty(nameof(this.BlockGroupMove));
        public AudioThemeSelection BlockGroupPlace => GetThemeProperty(nameof(this.BlockGroupPlace));

        public AudioThemeSelection BlockGroupRotate => GetThemeProperty(nameof(this.BlockGroupRotate));

        public AudioThemeSelection BlockStopped => GetThemeProperty(nameof(this.BlockStopped));

        public AudioThemeSelection MenuItemSelected => GetThemeProperty(nameof(this.MenuItemSelected));

        public AudioThemeSelection MenuItemActivated =>GetThemeProperty(nameof(this.MenuItemActivated));
        public AudioThemeSelection ClearLine => GetThemeProperty(nameof(this.ClearLine));
        public AudioThemeSelection ClearTetris => GetThemeProperty(nameof(this.ClearTetris));

        public AudioThemeSelection BackgroundMusic => GetThemeProperty(nameof(this.BackgroundMusic));
        public AudioThemeSelection GameOverShade => GetThemeProperty(nameof(this.GameOverShade));

        public AudioThemeSelection Pause => GetThemeProperty(nameof(this.Pause));

        public AudioThemeSelection GameOver => GetThemeProperty(nameof(this.GameOver));

        public AudioThemeSelection LevelUp => GetThemeProperty(nameof(this.LevelUp));

        public AudioThemeSelection Hold => GetThemeProperty(nameof(this.Hold));

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