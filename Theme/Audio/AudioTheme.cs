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
        public AudioThemeElement BlockStopped { get; set; }
        public AudioThemeElement ClearLine { get; set; }
        public AudioThemeElement ClearTetris { get; set; }
        public AudioThemeElement BackgroundMusic { get; set; }
        public AudioThemeElement GameOverShade { get; set; }
        public AudioThemeElement Pause { get; set; }
        public AudioThemeElement GameOver { get; set; }

        public AudioThemeElement LevelUp { get; set; }

        public AudioThemeElement Hold { get; set; }

        public AudioThemeElement MenuItemSelected { get; set; }
        public AudioThemeElement MenuItemActivated { get; set; }
        public static AudioTheme GetDefault()
        {
            return new AudioTheme()
            {
                BackgroundMusic = new AudioThemeElement(new[] { "tetris_theme_A", "tetris_a_theme_techno","tetris_theme_B","tetris_theme_C" }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                BlockGroupMove = new AudioThemeElement(new String[] { "block_move_2", "block_move","block_move_3" }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockGroupPlace = new AudioThemeElement(new String[] { "block_place_3", "block_place_2", "block_place" }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockGroupRotate = new AudioThemeElement(new String[] { "block_rotate_3", "block_rotate_2", "block_rotate","block_rotate_4" }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockStopped = new AudioThemeElement(new String[] { "block_stop" }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                ClearLine = new AudioThemeElement(new String[] {"line_clear_3","line_clear", "line_clear_2","line_clear_4"}, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                ClearTetris = new AudioThemeElement(new String[] {"line_tetris", "line_tetris_2","line_tetris_3"}, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                GameOver = new AudioThemeElement("tetris_game_over"),
                GameOverShade = new AudioThemeElement("shade_move"),
                Pause = new AudioThemeElement("pause"),
                LevelUp = new AudioThemeElement("level_up"),
                Hold = new AudioThemeElement("drop"),
                MenuItemSelected = new AudioThemeElement(new String[]{"block_rotate","block_rotate_2","block_rotate_3"}, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                MenuItemActivated = new AudioThemeElement(new String[] { "block_place","block_place_2","block_place_3"}, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static)


            };
        }
    }

    public class AudioThemeElement
    {
        public enum AudioThemeElementChooseFlags
        {
            Flag_Randomized,
            Flag_Static
        }
        public String[] AudioKeys;
        public AudioThemeElementChooseFlags ChooseFlag; //if multiple keys are entered this means that one will be initially chosen and returned fro that point on until the Theme is reset.

        public AudioThemeElement(String[] pAudioKeys, AudioThemeElementChooseFlags pFlags)
        {
            AudioKeys = pAudioKeys;
            ChooseFlag = pFlags;
        }

        public AudioThemeElement(String pAudioKey) : this(pAudioKey, AudioThemeElementChooseFlags.Flag_Randomized)
        {
        }

        public AudioThemeElement(String pAudioKey, AudioThemeElementChooseFlags pFlags) : this(new string[] {pAudioKey}, pFlags)
        {
        }
    }
}