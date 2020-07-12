using BASeTris.Theme.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateMusicMenuItem: MenuStateMultiOption<MenuMusicItemSelection>
    {
        private IStateOwner _Owner;
        public MenuStateMusicMenuItem(IStateOwner pOwner) : base(null)
        {
            //task: get all Music tracks, create listing of MenuMusicItemSelection based on that. 
            //decide which one is currently playing and set that as the current option and index.
            /*
            _Owner = pOwner;
            base.OptionManager = new MultiOptionManagerList<MenuItemScaleItemSelection>(ScaleOptions, 1);
            var closest = (from so in ScaleOptions orderby Math.Abs(so.Scale - pOwner.ScaleFactor) ascending select so).First();
            this.Text = closest.Text;
            OptionManager.SetCurrentIndex(Array.IndexOf(ScaleOptions, closest));
            OnActivateOption += ScaleActivate;*/

            //get all Audio from the theme of the owner...
            AudioThemeSelection[] AllAudioKeys = pOwner.AudioThemeMan.CurrentTheme.BackgroundMusic.AudioKeys;



        }
    }
    public class MenuMusicItemSelection
    {
        private String[] Tracks = new string[] { };
        public String Track { get { return Tracks.FirstOrDefault(); } set { Tracks = new string[] { value }; } }
        public MenuMusicItemSelection(String[] pSelectedTracks)
        {
            Tracks = pSelectedTracks;
        }

    }
}
