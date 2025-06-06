﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.GameHandlers;
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
                   
                        pOwner.CurrentState = MenuState.CreateOutroState(pOwner, _OriginalState);
                   
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
                new SoundOption("yogurtyard","Yogurt"),
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

            MenuStateSliderOption MusicVolumeItem = new MenuStateSliderOption(0, 2, _AlterSet.MusicVolume) {Label = "Music Volume", ChangeSize=0.05,LargeDetentCount = 10,SmallDetent=0.1,TipText = "Change Music Volume" } ;
            MenuStateSliderOption SoundVolumeItem = new MenuStateSliderOption(0, 2, _AlterSet.EffectVolume) { Label="Sound Volume", ChangeSize = 0.05, LargeDetentCount = 10, SmallDetent = 0.1,TipText = "Change Sound Volume" };
            MusicVolumeItem.ValueChanged += (senderM,eM)=>
            {
                _AlterSet.MusicVolume = (float)eM.Value;
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key  , _AlterSet.MusicVolume);


            };
            SoundVolumeItem.ValueChanged += (senderS, eS) =>
            {
                _AlterSet.EffectVolume = (float)eS.Value;
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key, _AlterSet.EffectVolume);
            };
            SoundVolumeItem.ValueChanged += SoundVolumeItem_ValueChanged;
            TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemSelected.Key, pOwner.Settings.std.EffectVolume);
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
            MenuElements.Add(MusicVolumeItem);
            MenuElements.Add(SoundVolumeItem);
            
        }

        private void SoundVolumeItem_ValueChanged(object sender, MenuStateSliderOption.SliderValueChangeEventArgs e)
        {
            
           

        }

        private void MusicVolumeItem_ValueChanged(object sender, MenuStateSliderOption.SliderValueChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SoundThemeOptionItem_OnChangeOption(object sender, OptionActivated<SoundOption> e)
        {
            _AlterSet.SoundScheme = e.Option.SoundKey;
        }

        private void MusicOptionItem_OnActivateOption(object sender, OptionActivated<SoundOption> e)
        {
            if (e.Option.SoundKey == "<RANDOM>")
            {
                TetrisGame.Soundman.PlayMusic(e.Owner.AudioThemeMan.BackgroundMusic.Key, true);
            }
            else
            {
                TetrisGame.Soundman.PlayMusic(e.Option.SoundKey, _AlterSet.MusicVolume, true);
            }
            _AlterSet.MusicOption = e.Option.SoundKey;
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

    public class OptionsMenuSettingsSelectorState: PagedMenuOptionsState
    {
        private GameState _OriginalState;
        private String _Category;
        
        public OptionsMenuSettingsSelectorState(IBackground background, IStateOwner pOwner, GameState OriginalState,String pCategory=null) : base(pOwner,background,OriginalState,null,"Options")
        {
            _OriginalState = OriginalState;
            _Category = pCategory;
            var ChosenOptions = PopulateOptions(pOwner);

            //if (ChosenOptions.Count > 9)
            //{
                PartitionMenuItems(pOwner, ChosenOptions);
            //}
            //else
            //{
            //    MenuElements = ChosenOptions;
            //}
        }
        protected Dictionary<MenuStateTextMenuItem, KeyValuePair<String, StandardSettings>> setlookup;
        private List<MenuStateMenuItem> PopulateOptions(IStateOwner pOwner)
        {
            
            var ReturnItems = new List<MenuStateMenuItem>();
            int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
            Font standardFont = TetrisGame.GetRetroFont(12, pOwner.ScaleFactor);
            Font ItemFont = TetrisGame.GetRetroFont(12, pOwner.ScaleFactor);
            MenuStateTextMenuItem ReturnItem = new MenuStateTextMenuItem() { Text = "Return" };
            StateHeader = "Option Set";

            if (_Category != null) StateHeader = "Options:" + _Category;
            HeaderTypeface = TetrisGame.GetRetroFont(12, 1.0f).FontFamily.Name;
            HeaderTypeSize = DesiredFontPixelHeight * .75f;
            //get the handlers....
            var allHandlers = Program.DITypes[typeof(IBlockGameCustomizationHandler)];
            Dictionary<String, List<IBlockGameCustomizationHandler>> AllHandlerCategories = new Dictionary<string, List<IBlockGameCustomizationHandler>>();
            Dictionary<MenuStateTextMenuItem, String> CategoryItems = new Dictionary<MenuStateTextMenuItem, string>();
            foreach (var iterate in allHandlers.GetManagedTypes())
            {
                var HandlerAttrib = iterate.GetCustomAttributes(typeof(HandlerMenuCategoryAttribute),true).FirstOrDefault() as HandlerMenuCategoryAttribute;
                String getCategory = HandlerAttrib == null ? "" : HandlerAttrib.Category;
                if (!AllHandlerCategories.ContainsKey(getCategory))
                    AllHandlerCategories.Add(getCategory, new List<IBlockGameCustomizationHandler>());

                ConstructorInfo ci = iterate.GetConstructor(new Type[] { });
                if (ci != null)
                {
                    IBlockGameCustomizationHandler handler = (IBlockGameCustomizationHandler)ci.Invoke(new object[] { });
                    AllHandlerCategories[getCategory].Add(handler);
                    //MenuStateTextMenuItem builditem = new MenuStateTextMenuItem() { Text = handler.Name, TipText="Change Settings for " + handler.Name };
                    //HandlerLookup.Add(builditem, handler);
                    //AllItems.Add(builditem);
                }


                
            }

            

            var useDictionary = pOwner.Settings.AllSettings;
            setlookup = new Dictionary<MenuStateTextMenuItem, KeyValuePair<String,StandardSettings>>();
            
            foreach (var iterateset in useDictionary)
            {
                //find the category of this entry.
                String getCategory = AllHandlerCategories.Keys.FirstOrDefault((k) => AllHandlerCategories[k].Any((h) => h.Name == iterateset.Key));
                getCategory = getCategory ?? "";
                if (getCategory == (_Category??""))
                {
                    MenuStateTextMenuItem submenuitem = new MenuStateTextMenuItem() { Text = iterateset.Key };
                    submenuitem.FontFace = ItemFont.FontFamily.Name;
                    submenuitem.FontSize = ItemFont.Size;
                    ReturnItems.Add(submenuitem);
                    setlookup.Add(submenuitem, iterateset);
                }
            }

            if (String.IsNullOrEmpty(_Category))
            {
                //base category shows other categories.

                foreach (var iterate in AllHandlerCategories)
                {
                    if (!String.IsNullOrEmpty(iterate.Key))
                    {
                        MenuStateTextMenuItem submenuitem = new MenuStateTextMenuItem() { Text = iterate.Key,TipText="View Handler Category" };
                        submenuitem.FontFace = ItemFont.FontFamily.Name;
                        submenuitem.FontSize = ItemFont.Size;
                        ReturnItems.Add(submenuitem);
                        CategoryItems.Add(submenuitem, iterate.Key);
                    }
                }



            }


            MenuItemActivated += (obj, e) =>
            {
                if (e.MenuElement == ReturnItem)
                {
                    pOwner.Settings.Save();
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, _OriginalState);
                    //pOwner.CurrentState = _OriginalState;
                }
                
                else
                {
                    
                    MenuStateTextMenuItem selecteditem = e.MenuElement as MenuStateTextMenuItem;
                    if (CategoryItems.ContainsKey(selecteditem))
                    {
                        OptionsMenuSettingsSelectorState oms = new OptionsMenuSettingsSelectorState(this._BG, pOwner, this, selecteditem.Text);
                        pOwner.CurrentState = oms;
                        this.ActivatedItem = null;

                    }
                    else
                    {
                        if (setlookup.ContainsKey(selecteditem))
                        {
                            var getkvp = setlookup[selecteditem];
                            OptionsMenuState oms = new OptionsMenuState(this._BG, pOwner, this, getkvp.Key, getkvp.Value);
                            pOwner.CurrentState = oms;
                            this.ActivatedItem = null;
                        }
                    }
                }
            };
            ReturnItem.FontFace = ItemFont.FontFamily.Name;
            ReturnItem.FontSize =  ItemFont.Size;
            ReturnItems.Add(ReturnItem);

            return ReturnItems;


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
