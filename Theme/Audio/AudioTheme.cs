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
        public AudioThemeElement BlockFalling { get; set; }
        public AudioThemeElement ScoreChime { get; set; }
        public AudioThemeElement BlockPop { get; set; }
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
                BackgroundMusic = new AudioThemeElement(new[]
                    { ATS("tetris_theme_A","Theme A" ), ATS("tetris_a_theme_techno"),ATS("tetris_theme_B"),ATS("tetris_theme_C"),
                    ATS("TetrisDS"),ATS("smb3_tetris"),ATS("kirbysand"),ATS("silius1"),ATS("journey3"),ATS("tetris_gb_theme"),ATS("tetris_nes_theme"),ATS("drMarioChill"),ATS("DrMarioFever") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                BlockGroupMove = new AudioThemeElement(new[] { ATS("block_move_2"), ATS("block_move"), ATS("block_move_3") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockGroupPlace = new AudioThemeElement(new[] { ATS("block_place_3"), ATS("block_place_2"), ATS("block_place") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockGroupRotate = new AudioThemeElement(new[] { ATS("block_rotate_3"), ATS("block_rotate_2"), ATS("block_rotate"), ATS("block_rotate_4") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockStopped = new AudioThemeElement(new[] { ATS("block_stop") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                ClearLine = new AudioThemeElement(new[] { ATS("line_clear_3"), ATS("line_clear"), ATS("line_clear_2") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                ClearTetris = new AudioThemeElement(new[] { ATS("line_tetris"), ATS("line_tetris_2") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                GameOver = new AudioThemeElement("tetris_game_over", "Game Over"),
                GameOverShade = new AudioThemeElement("shade_move", "Game Over Blinds"),
                Pause = new AudioThemeElement("pause", "Pause"),
                LevelUp = new AudioThemeElement("level_up", "Advance Level"),
                Hold = new AudioThemeElement("drop", "Block Dropped"),
                MenuItemSelected = new AudioThemeElement(new[] { ATS("block_rotate"), ATS("block_rotate_2"), ATS("block_rotate_3") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                MenuItemActivated = new AudioThemeElement(new[] { ATS("block_place"), ATS("block_place_2"), ATS("block_place_3") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                BlockFalling = new AudioThemeElement(new[] { ATS("block_falling") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Static),
                ScoreChime = new AudioThemeElement(new[] { ATS("score_chime") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized),
                BlockPop = new AudioThemeElement(new[] { ATS("block_pop") }, AudioThemeElement.AudioThemeElementChooseFlags.Flag_Randomized)
            };
        }
        private static AudioThemeSelection ATS(String pKey,String pFriendly)
        {
            return new AudioThemeSelection(pKey, pFriendly);
        }
        private static AudioThemeSelection ATS(String pKey)
        {
            return ATS(pKey, pKey);
        }
    }

    public class AudioThemeElement
    {
        public enum AudioThemeElementChooseFlags
        {
            Flag_Randomized,
            Flag_Static
        }
        public AudioThemeSelection[] AudioKeys;
        
        public AudioThemeElementChooseFlags ChooseFlag; //if multiple keys are entered this means that one will be initially chosen and returned fro that point on until the Theme is reset.

        public AudioThemeElement(AudioThemeSelection[] pAudioKeys, AudioThemeElementChooseFlags pFlags)
        {
            AudioKeys = pAudioKeys;
            ChooseFlag = pFlags;
        }

        public AudioThemeElement(String pAudioKey,String pFriendlyName) : this(pAudioKey,pFriendlyName, AudioThemeElementChooseFlags.Flag_Randomized)
        {
        }

        public AudioThemeElement(String pAudioKey,String pFriendlyName, AudioThemeElementChooseFlags pFlags) : this(new [] {new AudioThemeSelection(pAudioKey,pFriendlyName)}, pFlags)
        {
        }
    }
    public class AudioThemeSelection
    {
        public String Key { get; }
        public String FriendlyName { get; }
        public AudioThemeSelection(String pKey,String pFriendlyName)
        {
            Key = pKey;
            FriendlyName = pFriendlyName;
        }
    }
}