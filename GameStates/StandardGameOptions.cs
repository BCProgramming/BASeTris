using BASeCamp.Elementizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris.GameStates
{
    public class StandardGameOptions:IXmlPersistable
    {
        //options for the standard game.
        public bool AllowHold = true;
        public int NextQueueSize = 6;
        public bool MoveResetsSetTimer = true;
        public bool RotateResetsSetTimer = true;
        public bool DrawGhostDrop = true;

        public bool MusicRestartsOnTempoChange = false;
        public bool AllowWallKicks = true;
        private bool _MusicEnabled = true;


        public bool MusicEnabled
        {
            get { return _MusicEnabled; }
            set
            {
                _MusicEnabled = value;
                if (!_MusicEnabled) TetrisGame.Soundman.StopMusic();
            }
        }
        public StandardGameOptions()
        {

        }
        public StandardGameOptions(XElement src,Object Data)
        {

        }

        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            throw new NotImplementedException();
        }
    }
    public class OptionsManager
    {
        private Dictionary<String, StandardGameOptions> OptionData = new Dictionary<string, StandardGameOptions>();

        public static OptionsManager Static = null;

        public OptionsManager(String pXmlFileSource)
        {

        }





    }
}