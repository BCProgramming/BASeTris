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
        public SettingsManager()
        {
            //nothing...
        }
        public SettingsManager(String pSourceFile,IStateOwner pOwner)
        {
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
