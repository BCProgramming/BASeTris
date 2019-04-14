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
        
        public OptionsMenuState(IBackgroundDraw background,IStateOwner pOwner,GameState OriginalState):base(background)
        {
            _OriginalState = OriginalState;
            PopulateOptions(pOwner);
        }

        private void PopulateOptions(IStateOwner pOwner)
        {
            int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
            Font standardFont = new Font(TetrisGame.RetroFont, DesiredFontPixelHeight, FontStyle.Bold, GraphicsUnit.Pixel);
            Font ItemFont = new Font(TetrisGame.RetroFont, (int)((float)DesiredFontPixelHeight * (3f / 4f)), FontStyle.Bold, GraphicsUnit.Pixel);
            MenuStateTextMenuItem ReturnItem = new MenuStateTextMenuItem() { Text = "Return" };
            StateHeader = "Options";
            HeaderFont = standardFont;

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
                new SoundOption("tetris_2","Tetris 2")

            }, 0);


            MenuStateMultiOption<SoundOption> MusicOptionItem = new MenuStateMultiOption<SoundOption>(SoundOptions);


            MusicOptionItem.OnActivateOption += MusicOptionItem_OnActivateOption;
            ReturnItem.Font = SoundLabel.Font = MusicOptionItem.Font = ItemFont;
            MenuElements.Add(ReturnItem);
            MenuElements.Add(SoundLabel);
            MenuElements.Add(MusicOptionItem);

        }

        private void MusicOptionItem_OnActivateOption(object sender, OptionActivated<SoundOption> e)
        {
            
            TetrisGame.Soundman.PlayMusic(e.Option.SoundKey, e.Owner.Settings.MusicVolume, true);
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
