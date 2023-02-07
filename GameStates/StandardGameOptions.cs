using BASeCamp.Elementizer;
using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris.GameStates
{
    //game options that apply to all handlers.
    public abstract class GameOptions:IXmlPersistable
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
        public GameOptions()
        {

        }
        public GameOptions(XElement src,Object Data)
        {
            
        }

        public virtual XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            return new XElement(pNodeName,
                new XAttribute("AllowHold", AllowHold ? "True" : "False"),
                new XAttribute("NextQueueSize", NextQueueSize),
                new XAttribute("MoveResetsSetTimer", MoveResetsSetTimer ? "True" : "False"),
                new XAttribute("RotateResetsSetTimer", RotateResetsSetTimer ? "True" : "False"),
                new XAttribute("DrawGhostDrop", DrawGhostDrop ? "True" : "False"),
                new XAttribute("MusicRestartsOnTempoChange", MusicRestartsOnTempoChange ? "True" : "False"),
                new XAttribute("AllowWallKicks", AllowWallKicks ? "True" : "False"),
                new XAttribute("MusicEnabled", _MusicEnabled ? "True" : "False"));


        }
        
    }
    //options for each specific type.
    public class TetrisGameOptions : GameOptions
    {
        public override XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            return base.GetXmlData(pNodeName, PersistenceData);
        }
    }
    public class ColumnsGameOptions : GameOptions
    {
        public override XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            return base.GetXmlData(pNodeName, PersistenceData);
        }
    }
    public class DrMarioGameOptions : GameOptions
    {
        public override XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            return base.GetXmlData(pNodeName, PersistenceData);
        }
    }
    public class TetrisAttackGameOptions : GameOptions
    {
    }
    public class Tetris2GameOptions :GameOptions
    {
        public override XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            return base.GetXmlData(pNodeName, PersistenceData);
        }
    }


    public class OptionsManager
    {
        private Dictionary<String, GameOptions> OptionData = new Dictionary<string, GameOptions>();

        public static OptionsManager Static = null;

        public OptionsManager(String pXmlFileSource)
        {

        }





    }
}