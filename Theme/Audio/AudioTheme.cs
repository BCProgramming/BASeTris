using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme.Audio
{
    //class encapsulates the Key names for a given audio theme.
    //each method returns an array, with the array elements being each possible audio effect. 
    //
    public class AudioTheme
    {
        public AudioThemeElement BlockGroupMove { get; set; }
        public AudioThemeElement BlockGroupPlace { get; set; }
        public AudioThemeElement BlockGroupRotate { get; set; }
        public AudioThemeElement ClearLine { get; set; }
        public AudioThemeElement ClearTetris { get; set; }
        public AudioThemeElement BackgroundMusic { get; set; }
        public AudioThemeElement GameOverShade { get; set; }
        public AudioThemeElement Pause { get; set; }
        public AudioThemeElement GameOver { get; set; }

        public AudioThemeElement LevelUp { get; set; }

        public AudioThemeElement Hold { get; set; }

        public static AudioTheme GetDefault()
        {
            return new AudioTheme()
            {
                BackgroundMusic = new AudioThemeElement(new[] {"tetris_theme_A", "tetris_a_theme_techno"}, true),
                BlockGroupMove = new AudioThemeElement(new String[] {"block_move_2", "block_move"}, true),
                BlockGroupPlace = new AudioThemeElement(new String[] {"block_place_2", "block_place"}, false),
                BlockGroupRotate = new AudioThemeElement(new String[] {"block_rotate_2", "block_rotate"}, true),
                ClearLine = new AudioThemeElement(new String[] {"line_clear", "line_clear_2"}, false),
                ClearTetris = new AudioThemeElement(new String[] {"line_tetris", "line_tetris_2"}, false),
                GameOver = new AudioThemeElement("tetris_game_over"),
                GameOverShade = new AudioThemeElement("shade_move"),
                Pause = new AudioThemeElement("pause"),
                LevelUp = new AudioThemeElement("level_up"),
                Hold = new AudioThemeElement("drop")
            };
        }
    }

    public class AudioThemeElement
    {
        public String[] AudioKeys;
        public bool ChooseStatic; //if multiple keys are entered this means that one will be initially chosen and returned fro that point on until the Theme is reset.

        public AudioThemeElement(String[] pAudioKeys, bool pStatic)
        {
            AudioKeys = pAudioKeys;
            ChooseStatic = pStatic;
        }

        public AudioThemeElement(String pAudioKey) : this(pAudioKey, false)
        {
        }

        public AudioThemeElement(String pAudioKey, bool pStatic) : this(new string[] {pAudioKey}, pStatic)
        {
        }
    }
}