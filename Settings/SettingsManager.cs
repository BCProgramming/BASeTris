using BASeCamp.Elementizer;
using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace BASeTris.Settings
{
    public class SettingsManager : IXmlPersistable
    {
        String pLoadedFile = null;
        public Dictionary<String, StandardSettings> AllSettings = new Dictionary<string, StandardSettings>(StringComparer.OrdinalIgnoreCase);
        private IStateOwner _Owner = null;

        //since the enumerations that are used by the appropriate code shouldn't be used here, we have these delegate functions passed in as callbacks that convert a string into the int value for the enum in question.
        //eg. the XInput GamepadButtons or whatever. Then we can store that int indexed by the Gamepad keys, which is an enum we CAN access
        public delegate int GetKeyboardKeyFromName(String input);
        public delegate int GetGamepadKeyFromName(string input);
        private GetKeyboardKeyFromName KeyboardKeyFunc = null;
        private GetGamepadKeyFromName GamepadKeyFunc = null;
        private Dictionary<GameState.GameKeys, int> KeyboardKeyCodeAssignments = new Dictionary<GameState.GameKeys, int>();
        private Dictionary<GameState.GameKeys, int> GamepadButtonCodeAssignments = new Dictionary<GameState.GameKeys, int>();
        GameState.GameKeys k;
        public SettingsManager()
        {
            //nothing...
        }
        public void LoadControlSettings(XElement Source)
        {
            /*
             <Controls>
             <KeyboardKeys Rotate_CW="code" ....>
             <GamepadKeys Rotate_CW="code" ....>
             
             */
            XElement KeyboardNode = Source.Element("KeyboardKeys");
            XElement GamepadNode = Source.Element("GamepadKeys");

            //iterate through the attributes
            foreach (XAttribute KeyAttrib in KeyboardNode.Attributes())
            {
                //name of attribute should be a GameKey.
                GameState.GameKeys gk;
                if (Enum.TryParse<GameState.GameKeys>(KeyAttrib.Name.LocalName, out gk))
                {
                    try
                    {
                        //Alright, Gamekey parsed. let's grab the attribute name, and try to parse that using the callback.
                        int GetKeyCode = KeyboardKeyFunc(KeyAttrib.Value);
                        //add it to the dictionary.
                        KeyboardKeyCodeAssignments.Add(gk, GetKeyCode);
                    }
                    catch (Exception ex1)
                    {
                    }
                }
            }

            foreach (XAttribute PadAttrib in GamepadNode.Attributes())
            {
                //name of attribute should be a GameKey here
                GameState.GameKeys gk;
                if (Enum.TryParse<GameState.GameKeys>(PadAttrib.Name.LocalName, out gk))
                {
                    try
                    {
                        int GetPadCode = GamepadKeyFunc(PadAttrib.Value);

                        GamepadButtonCodeAssignments.Add(gk, GetPadCode);
                    }
                    catch (Exception ex2)
                    {
                        //no op.
                    }
                }
            }
        }
        public SettingsManager(String pSourceFile,IStateOwner pOwner,GetKeyboardKeyFromName KeyFunction,GetGamepadKeyFromName GamepadFunction)
        {
            KeyboardKeyFunc = KeyFunction;
            GamepadKeyFunc = GamepadFunction;
            _Owner = pOwner;
            pLoadedFile = pSourceFile;
            if(File.Exists(pSourceFile))
            {
                XDocument xdoc = XDocument.Load(pSourceFile);
                var RootNode = xdoc.Root;

                if (RootNode.Name == "Settings")
                {
                    //old settings, add those as the default.
                    StandardSettings DefaultSettings = new StandardSettings(RootNode);
                    DefaultSettings.SetOwner(this);
                    AllSettings.Add("Default", DefaultSettings);
                }
                else if(RootNode.Name=="SettingsGroups")
                {
                    //load "core" settings. Controls, for example.
                    var ControlsNode = RootNode.Element("Controls");
                    if (ControlsNode != null)
                    {
                        LoadControlSettings(ControlsNode);
                    }

                    foreach(var groupnode in RootNode.Elements("SettingsGroup"))
                    {
                        String sGroupName = groupnode.Attribute("Name").Value;
                        XElement DataNode = groupnode.Element("Data");
                        StandardSettings SettingsData = new StandardSettings(DataNode);
                        SettingsData.SetOwner(this);
                        AllSettings.Add(sGroupName, SettingsData);
                    }
                    
                }
            }
        }
        /// <summary>
        /// Returns the current applicable settings. This is based on the handler of the current owner state.
        /// </summary>
        public StandardSettings std
        {
            get {

                //try to get the current handler, so we can get the settings for it.
                var currenthandler = _Owner.GetHandler();
                if(currenthandler==null)
                {
                    Debug.Print("Unable to determine current handler. returning default settings.");
                    return GetSettings("Default");
                }  
                else
                {
                    Debug.Print("Found Handler:" + currenthandler.Name);
                    return GetSettings(currenthandler.Name);
                }
                
                 }
        }
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement MakeNode = new XElement(pNodeName);
            foreach(var iterate in AllSettings)
            {
                XElement GroupNode = new XElement("SettingsGroup", new XAttribute("Name", iterate.Key));
                XElement SavedNode = iterate.Value.GetXmlData("Data", null);
                GroupNode.Add(SavedNode);
                MakeNode.Add(GroupNode);
            }
            return MakeNode;
        }
        public void Save(String pSaveFile=null)
        {
            pSaveFile = (pSaveFile ?? pLoadedFile);
            if(pSaveFile!=null)
            {
                XElement SavedNode = GetXmlData("SettingsGroups", null);
                XDocument xdoc = new XDocument(SavedNode);
                xdoc.Save(pSaveFile);
            }
        }
        ~SettingsManager()
        {
            Save(pLoadedFile);
        }
        public StandardSettings GetSettings(String pHandler="Default")
        {
            if (!AllSettings.ContainsKey(pHandler))
            {
                var buildsettings = new StandardSettings();
                buildsettings.SetOwner(this);
                AllSettings.Add(pHandler, buildsettings);
            }

            return AllSettings[pHandler];
        }

       
    }
}
