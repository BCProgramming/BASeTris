using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates.Menu
{
    //the 'standard' options menu.
    public class OptionsMenuState:MenuState
    {
        private GameState _OriginalState;
        
        public OptionsMenuState(IBackground background,IStateOwner pOwner,GameState OriginalState):base(background)
        {
            _OriginalState = OriginalState;
            PopulateOptions(pOwner);
        }

        private void PopulateOptions(IStateOwner pOwner)
        {
            int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
            Font standardFont = TetrisGame.GetRetroFont(DesiredFontPixelHeight, 1.0f);
            Font ItemFont = TetrisGame.GetRetroFont(DesiredFontPixelHeight * .75f, 1.0);
            MenuStateTextMenuItem ReturnItem = new MenuStateTextMenuItem() { Text = "Return" };
            StateHeader = "Options";
            HeaderTypeface = TetrisGame.GetRetroFont(14, 1.0f).FontFamily.Name;
            HeaderTypeSize = DesiredFontPixelHeight*.75f;

            
            MenuItemActivated += (obj, e) =>
            {
                if (e.MenuElement == ReturnItem)
                {
                    pOwner.CurrentState = _OriginalState;
                }
            };
            //add the sound options label.
            MenuStateLabelMenuItem SoundLabel = new MenuStateLabelMenuItem() { Text = "--Sound--" };
            
            MultiOptionManagerList<SoundOption> SoundOptions = new MultiOptionManagerList<SoundOption>(new SoundOption[]
            {
                new SoundOption("TDM_A_THEME","Classic"),
                new SoundOption("tetris_a_theme_techno","Korotechno"),
                new SoundOption("tetris_a_theme_techno_A","Korotechno - Alt"),
                new SoundOption("tetris_theme_A","NES A Theme"),
                new SoundOption("tetris_theme_B","NES B Theme"),
                new SoundOption("tetris_theme_C","NES C Theme"),
                new SoundOption("tetris_2","Tetris 2"),
                new SoundOption("smb3_tetris","Tetris DS-SMB3"),
                new SoundOption("tetrisds","Tetris DS"),
                new SoundOption("kirbysand","Kirby Sand"),
                new SoundOption("silius1","Silius"),
                new SoundOption("journey3","Journey 3"),
                new SoundOption("tetris_nes_theme","NES"),
                new SoundOption("tetris_gb_theme","GB"),
                new SoundOption("DrMarioFever","Fever"),
                new SoundOption("DrMarioFever_Rock","Fever Rock"),
                new SoundOption("DrMarioChill","Chill"),
                new SoundOption("DrMarioChill_Rock","Chill Rock"),
                new SoundOption("<RANDOM>","Random")

            }, 0);


            MenuStateMultiOption<SoundOption> MusicOptionItem = new MenuStateMultiOption<SoundOption>(SoundOptions);
            MusicOptionItem.Text = "Music";

            MusicOptionItem.OnChangeOption += MusicOptionItem_OnActivateOption;
            ReturnItem.FontFace = SoundLabel.FontFace = MusicOptionItem.FontFace = ItemFont.FontFamily.Name;
            ReturnItem.FontSize = SoundLabel.FontSize = MusicOptionItem.FontSize = ItemFont.Size;
            MenuElements.Add(ReturnItem);
            MenuElements.Add(SoundLabel);
            MenuElements.Add(MusicOptionItem);

        }

        private void MusicOptionItem_OnActivateOption(object sender, OptionActivated<SoundOption> e)
        {
            if (e.Option.SoundKey == "<RANDOM>")
            {
                TetrisGame.Soundman.PlayMusic(e.Owner.AudioThemeMan.BackgroundMusic.Key, true);
            }
            else
            {
                TetrisGame.Soundman.PlayMusic(e.Option.SoundKey, e.Owner.Settings.MusicVolume, true);
            }
            e.Owner.Settings.MusicOption = e.Option.SoundKey;
        }

        private class SoundOption
        {
            public String SoundKey;
            public String Text;
            public SoundOption(String pSoundKey,String pText)
            {
                SoundKey = pSoundKey;
                Text = pText;
            }
            public override String ToString()
            {
                return Text;
            }
        }
    }
}
