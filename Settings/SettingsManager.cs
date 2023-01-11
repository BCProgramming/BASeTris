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
using XInput.Wrapper;

namespace BASeTris.Settings
{
    public class SettingsManager : IXmlPersistable
    {
        String pLoadedFile = null;
        public Dictionary<String, StandardSettings> AllSettings = new Dictionary<string, StandardSettings>(StringComparer.OrdinalIgnoreCase);
        private IStateOwner _Owner = null;
        private Dictionary<String, GameState.GameKeys> DefaultButtonControls = new Dictionary<String, GameState.GameKeys>()
        {
            {"A", GameState.GameKeys.GameKey_RotateCW},
            {"X", GameState.GameKeys.GameKey_RotateCCW},
            {"RBumper", GameState.GameKeys.GameKey_Hold},
            {"Dpad_Left", GameState.GameKeys.GameKey_Left},
            {"Dpad_Right", GameState.GameKeys.GameKey_Right},
            {"Dpad_Down", GameState.GameKeys.GameKey_Down},
            {"Dpad_Up", GameState.GameKeys.GameKey_Drop},
            {"Start", GameState.GameKeys.GameKey_Pause}
        };
        Dictionary<String, GameState.GameKeys> DefaultKeyControls = new Dictionary<String, GameState.GameKeys>()
        {
            {"Left", GameState.GameKeys.GameKey_Left},
            {"Right", GameState.GameKeys.GameKey_Right},
            {"Down", GameState.GameKeys.GameKey_Down},
            {"Up", GameState.GameKeys.GameKey_Drop},
            {"X", GameState.GameKeys.GameKey_RotateCW},
            {"Z", GameState.GameKeys.GameKey_RotateCCW},
            {"Pause", GameState.GameKeys.GameKey_Pause},
            {"P", GameState.GameKeys.GameKey_Pause},
            {"Space", GameState.GameKeys.GameKey_Hold},
            {"Enter",GameState.GameKeys.GameKey_MenuActivate },
            {"F2", GameState.GameKeys.GameKey_Debug1},
            {"F7", GameState.GameKeys.GameKey_Debug2},
            {"F11",GameState.GameKeys.GameKey_Debug3},
            {"F12",GameState.GameKeys.GameKey_Debug4 },
            {"Number5",GameState.GameKeys.GameKey_Debug5 }
        };
        //since the enumerations that are used by the appropriate code shouldn't be used here, we have these delegate functions passed in as callbacks that convert a string into the int value for the enum in question.
        //eg. the XInput GamepadButtons or whatever. Then we can store that int indexed by the Gamepad keys, which is an enum we CAN access
        public delegate int GetKeyboardKeyFromName(String input);
        public delegate int GetGamepadKeyFromName(string input);
        public delegate Type GetKeyboardKeyEnumType();
        public delegate Type GetGamepadKeyEnumType();
        private GetKeyboardKeyFromName KeyboardKeyFunc = null;
        private GetGamepadKeyFromName GamepadKeyFunc = null;
        //private GetKeyboardKeyEnumType KeyEnumTypeFunc = null;
        //private GetGamepadKeyEnumType GamepadEnumType = null;
        private Type KeyEnumType = null;
        private Type GamepadEnumType = null;
        private Dictionary<int, GameState.GameKeys> KeyboardKeyCodeAssignments = new Dictionary<int, GameState.GameKeys>();
        private Dictionary<int, GameState.GameKeys> GamepadButtonCodeAssignments = new Dictionary<int, GameState.GameKeys>();

        private Dictionary<GameState.GameKeys, int> KeyCodeKeyboardCodeAssignments = new Dictionary<GameState.GameKeys, int>();
        private Dictionary<GameState.GameKeys, int> KeyCodeGamepadCodeAssignments = new Dictionary<GameState.GameKeys, int>();

        private void SetupDefaultControls()
        {
            //setup both dictionaries for the default configuration.
            foreach (var iterate in DefaultButtonControls)
            {
                SetGamepadButtonAssignment(GamepadKeyFunc(iterate.Key), iterate.Value);
            }

            foreach (var iterate in DefaultKeyControls)
            {
                SetKeyboardKeyAssignment(KeyboardKeyFunc(iterate.Key),iterate.Value);
            }

            /*
             *         
             * */



        }

        public bool HasAssignedKeyboardKey(GameState.GameKeys input)
        {
            return KeyCodeKeyboardCodeAssignments.ContainsKey(input);
        }
        public bool hasAssignedKeyboardKey(int input)
        {
            return KeyboardKeyCodeAssignments.ContainsKey(input);
        }

        public bool HasAssignedGamepadButton(GameState.GameKeys input)
        {
            return KeyCodeGamepadCodeAssignments.ContainsKey(input);
        }
        public bool HasAssignedGamepadButton(int input)
        {
            return GamepadButtonCodeAssignments.ContainsKey(input);
        }
        public void ClearKeyboardKeysForGameKey(GameState.GameKeys i)
        {
            KeyboardKeyCodeAssignments.Remove(KeyCodeKeyboardCodeAssignments[i]);
            KeyCodeKeyboardCodeAssignments.Remove(i);

        }
        public void ClearGamepadButtonsForGameKey(GameState.GameKeys i)
        {
            GamepadButtonCodeAssignments.Remove(KeyCodeKeyboardCodeAssignments[i]);
            KeyCodeKeyboardCodeAssignments.Remove(i);
        }
        public int? GetKeyBoardKeyFromGameKey(GameState.GameKeys i)
        {
            if(KeyCodeKeyboardCodeAssignments.ContainsKey(i))
                return KeyCodeKeyboardCodeAssignments[i];
            return null;
        }
        public int? GetGamePadButtonFromGameKey(GameState.GameKeys i)
        {
            if (KeyCodeGamepadCodeAssignments.ContainsKey(i))
                return KeyCodeGamepadCodeAssignments[i];
            return null;
        }
        public GameState.GameKeys? GetKeyboardKeyAssignment(int source)
        {
            if (KeyboardKeyCodeAssignments.ContainsKey(source)) return KeyboardKeyCodeAssignments[source];
            return null;
        }
        public GameState.GameKeys? GetGamepadButtonAssignment(int source)
        {
            if (GamepadButtonCodeAssignments.ContainsKey(source)) return GamepadButtonCodeAssignments[source];
            return null;
        }

        public void SetKeyboardKeyAssignment(int KeyCode, GameState.GameKeys? Value)
        {
            AssignMapping(KeyboardKeyCodeAssignments,KeyCodeKeyboardCodeAssignments, KeyCode, Value);
        }
        private void AssignMapping(Dictionary<int, GameState.GameKeys> KeyMap,Dictionary<GameState.GameKeys,int> MapKey, int Code, GameState.GameKeys? Value)
        {
            if (Value == null)
            {
                if (KeyMap.ContainsKey(Code))
                {
                    MapKey.Remove(KeyMap[Code]);
                    KeyMap.Remove(Code);
                }
            }
            else
            {
                KeyMap[Code] = Value.Value;
                MapKey[Value.Value] = Code;

            }
        }
        public void SetGamepadButtonAssignment(int ButtonCode, GameState.GameKeys? Value)
        {
                AssignMapping(GamepadButtonCodeAssignments, KeyCodeGamepadCodeAssignments, ButtonCode, Value);
        }
        private StackTrace MyCreator = null;
        public SettingsManager()
        {
            MyCreator = new StackTrace();
            //nothing...
        }
        public XElement SaveControlSettings()
        {
            XElement ControlRoot = new XElement("Controls");

            XElement KeyboardNode = new XElement("KeyboardKeys");
            XElement GamepadNode = new XElement("GamepadButtons");
            //first save KeyboardKeys
            foreach (var iterate in KeyboardKeyCodeAssignments)
            {
                KeyboardNode.Add(new XElement("Key", new XAttribute("GameKey", iterate.Value.ToString()), new XAttribute("Binding", Enum.Format(KeyEnumType,iterate.Key,"G"))));
                
            }
            foreach (var iterate in GamepadButtonCodeAssignments)
            {
                GamepadNode.Add(new XElement("Button", new XAttribute("GameKey", iterate.Value.ToString()), new XAttribute("Binding", Enum.Format(GamepadEnumType,iterate.Key,"G"))));
            }
            ControlRoot.Add(KeyboardNode, GamepadNode);
            return ControlRoot;

        }
        public void LoadControlSettings(XElement Source)
        {
            /*
             <Controls>
             <KeyboardKeys Rotate_CW="code" ....>
             <GamepadKeys Rotate_CW="code" ....>
             
             */
            XElement KeyboardNode = Source.Element("KeyboardKeys");
            XElement GamepadNode = Source.Element("GamepadButtons");


           

            //iterate through the attributes
            foreach (XElement KeyItem in KeyboardNode.Elements("Key"))
            {
                //name of attribute should be a GameKey.
                XAttribute GameKeyAttribute = KeyItem.Attribute("GameKey");
                XAttribute BindingAttribute = KeyItem.Attribute("Binding");
                GameState.GameKeys gk;
                if (Enum.TryParse<GameState.GameKeys>(GameKeyAttribute.Value, out gk))
                {

                    try
                    {
                        //Alright, Gamekey parsed. let's grab the attribute name, and try to parse that using the callback.
                        int GetKeyCode = KeyboardKeyFunc(BindingAttribute.Value);
                        //add it to the dictionary.
                        KeyboardKeyCodeAssignments.Add(GetKeyCode, gk);
                    }
                    catch (Exception ex1)
                    {
                    }
                }
            }

            foreach (XElement ButtonItem in GamepadNode.Elements("Button"))
            {
                //name of attribute should be a GameKey here
                GameState.GameKeys gk;
                XAttribute GameKeyAttribute = ButtonItem.Attribute("GameKey");
                XAttribute BindingAttribute = ButtonItem.Attribute("Binding");
                if (Enum.TryParse<GameState.GameKeys>(GameKeyAttribute.Value, out gk))
                {
                    try
                    {
                        int GetPadCode = GamepadKeyFunc(BindingAttribute.Value);

                        GamepadButtonCodeAssignments.Add(GetPadCode, gk);
                    }
                    catch (Exception ex2)
                    {
                        //no op.
                    }
                }
            }
            
        }
        public SettingsManager(String pSourceFile,IStateOwner pOwner,GetKeyboardKeyFromName KeyFunction,GetGamepadKeyFromName GamepadFunction,Type pKeyEnumType,Type pGamepadEnumType)
        {
            KeyboardKeyFunc = KeyFunction;
            GamepadKeyFunc = GamepadFunction;
            KeyEnumType = pKeyEnumType;
            GamepadEnumType = pGamepadEnumType;
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
                    else
                    {
                        SetupDefaultControls();
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
            XElement ControlNode = SaveControlSettings();
            MakeNode.Add(ControlNode);
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
