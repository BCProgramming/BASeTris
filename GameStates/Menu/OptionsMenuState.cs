using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;
using BASeTris.Settings;
using BASeTris.Theme.Audio;

namespace BASeTris.GameStates.Menu
{
    //the 'standard' options menu.
    public class OptionsMenuState:MenuState
    {
        private GameState _OriginalState;
        private StandardSettings _AlterSet;
        private String _SettingsTitle;
        public OptionsMenuState(IBackground background,IStateOwner pOwner,GameState OriginalState,String pSettingsTitle,StandardSettings AlterSettingsSet):base(background)
        {
            _OriginalState = OriginalState;
            _AlterSet = AlterSettingsSet;
            _SettingsTitle = pSettingsTitle;
            PopulateOptions(pOwner);
        }

        private void PopulateOptions(IStateOwner pOwner)
        {
            int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
            Font standardFont = TetrisGame.GetRetroFont(DesiredFontPixelHeight, 1.0f);
            Font ItemFont = TetrisGame.GetRetroFont(DesiredFontPixelHeight * .75f, 1.0);
            MenuStateTextMenuItem ReturnItem = new MenuStateTextMenuItem() { Text = "Return" };
            StateHeader = "Options (" + _SettingsTitle + ")";
            HeaderTypeface = TetrisGame.GetRetroFont(14, 1.0f).FontFamily.Name;
            HeaderTypeSize = DesiredFontPixelHeight*.75f;

            
            MenuItemActivated += (obj, e) =>
            {
                if (e.MenuElement == ReturnItem)
                {
                    _AlterSet.Save();
                    pOwner.CurrentState = _OriginalState;
                    
                }
            };
            //add the sound options label.
            MenuStateLabelMenuItem SoundLabel = new MenuStateLabelMenuItem() { Text = "--Sound--" };

            var useMusicOptions = new SoundOption[]
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

            };
           

            var ThemeArray = (from s in AudioTheme.AvailableSoundThemes select new SoundOption(s.Item1, s.Item2)).ToArray();
            int startIndex = 0;

            String CurrentTheme = _AlterSet.SoundScheme;
            for(int i=0;i<ThemeArray.Length;i++)
            {
                if (ThemeArray[i].Equals(CurrentTheme))
                {
                    startIndex = i;
                    break;
                }

            }


            int startMusicIndex = 0;
            String CurrentMusic = pOwner.Settings.std.MusicOption;

            for(int i=0;i<useMusicOptions.Length;i++)
            {
                if (useMusicOptions[i].SoundKey.Equals(CurrentMusic, StringComparison.OrdinalIgnoreCase))
                    startMusicIndex = i;
            }


            MultiOptionManagerList<SoundOption> SoundOptions = new MultiOptionManagerList<SoundOption>(useMusicOptions, startMusicIndex);
            MultiOptionManagerList<SoundOption> SoundThemeOptions = new MultiOptionManagerList<SoundOption>(
                ThemeArray,startIndex);
            MenuStateMultiOption<SoundOption> MusicOptionItem = new MenuStateMultiOption<SoundOption>(SoundOptions);
            MenuStateMultiOption<SoundOption> SoundThemeOptionItem = new MenuStateMultiOption<SoundOption>(SoundThemeOptions);
            MusicOptionItem.Text = "Music";
            SoundThemeOptionItem.Text = "Sound Theme";
            MusicOptionItem.OnChangeOption += MusicOptionItem_OnActivateOption;
            SoundThemeOptionItem.OnChangeOption += SoundThemeOptionItem_OnChangeOption;
            ReturnItem.FontFace = SoundLabel.FontFace = MusicOptionItem.FontFace = SoundThemeOptionItem.FontFace =   ItemFont.FontFamily.Name;
            ReturnItem.FontSize = SoundLabel.FontSize = MusicOptionItem.FontSize = SoundThemeOptionItem.FontSize = ItemFont.Size;
            MenuElements.Add(ReturnItem);
            MenuElements.Add(SoundLabel);
            MenuElements.Add(MusicOptionItem);

            MenuElements.Add(SoundThemeOptionItem);

        }

        private void SoundThemeOptionItem_OnChangeOption(object sender, OptionActivated<SoundOption> e)
        {
            e.Owner.Settings.std.SoundScheme = e.Option.SoundKey;
        }

        private void MusicOptionItem_OnActivateOption(object sender, OptionActivated<SoundOption> e)
        {
            if (e.Option.SoundKey == "<RANDOM>")
            {
                TetrisGame.Soundman.PlayMusic(e.Owner.AudioThemeMan.BackgroundMusic.Key, true);
            }
            else
            {
                TetrisGame.Soundman.PlayMusic(e.Option.SoundKey, e.Owner.Settings.std.MusicVolume, true);
            }
            e.Owner.Settings.std.MusicOption = e.Option.SoundKey;
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

    public class OptionsMenuSettingsSelectorState: MenuState
    {
        private GameState _OriginalState;
        
        public OptionsMenuSettingsSelectorState(IBackground background, IStateOwner pOwner, GameState OriginalState) : base(background)
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
            StateHeader = "Choose Handler";
            HeaderTypeface = TetrisGame.GetRetroFont(14, 1.0f).FontFamily.Name;
            HeaderTypeSize = DesiredFontPixelHeight * .75f;


            

            var useDictionary = pOwner.Settings.AllSettings;
            Dictionary<MenuStateTextMenuItem, KeyValuePair<String,StandardSettings>> setlookup = new Dictionary<MenuStateTextMenuItem, KeyValuePair<String,StandardSettings>>();
            foreach (var iterateset in useDictionary)
            {
                MenuStateTextMenuItem submenuitem = new MenuStateTextMenuItem() { Text = iterateset.Key };
                submenuitem.FontFace = ItemFont.FontFamily.Name;
                submenuitem.FontSize = ItemFont.Size;
                MenuElements.Add(submenuitem);
                setlookup.Add(submenuitem, iterateset);
            }
            
            MenuItemActivated += (obj, e) =>
            {
                if (e.MenuElement == ReturnItem)
                {
                    pOwner.Settings.Save();
                    pOwner.CurrentState = _OriginalState;
                }
                else
                {
                    MenuStateTextMenuItem selecteditem = e.MenuElement as MenuStateTextMenuItem;
                    var getkvp = setlookup[selecteditem];
                    OptionsMenuState oms = new OptionsMenuState(this._BG, pOwner, this, getkvp.Key, getkvp.Value);
                    pOwner.CurrentState = oms;
                    this.ActivatedItem = null;
                }
            };
            ReturnItem.FontFace = ItemFont.FontFamily.Name;
            ReturnItem.FontSize =  ItemFont.Size;
            MenuElements.Add(ReturnItem);


        }


        private class HandlerSettingsOption
        {
            private String _HandlerName;
            private String _DisplayText;
            private StandardSettings _Settings;
            public String HandlerName {  get { return _HandlerName; } set { _HandlerName = value; } }
            public String DisplayText {  get { return _DisplayText; } set { _DisplayText = value; } }
            public StandardSettings Settings {  get { return _Settings; } set { _Settings = value; } }
        }
      


    }
}
