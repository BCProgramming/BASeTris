﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.Elementizer;

namespace BASeTris.Settings
{
    public class StandardSettings : IXmlPersistable
    {

        //Audio volume settings.
        public float MusicVolume = 1.0f;
        public float EffectVolume = 1.0f;
        private WeakReference<SettingsManager> _ManagerOwner { get; set; } = new WeakReference<SettingsManager>(null);
        
        public void SetOwner(SettingsManager pOwner)
        {
            _ManagerOwner.SetTarget(pOwner);
        }
        public float DisplayScaleFactor = 1.6f;
        //TODO (?) Support additional Audio Drivers other than BASSDriver. Probably not worthwhile to be fair.

        public long DASStartDelay = 450; //key repeat delay for DAS.
        public long DASRate = 100;
        public long LockTime = 666;
        public bool SmoothFall = false;
        public bool SmoothRotate = true;
        private String sLoadedSource = null;
        public String Chooser = "Default";
        public String MusicOption { get; set; } = "<RANDOM>";
        public string Theme { get; set; } = "";
        public String SoundScheme { get; set; } = "Default";
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            
            return new XElement
            (pNodeName,
                new XAttribute("Music", MusicVolume),
                new XAttribute("Effects", EffectVolume),
                new XAttribute("DisplayFactor", DisplayScaleFactor),
                new XAttribute("DASStartDelay", DASStartDelay),
                new XAttribute("DASRate", DASRate),
                new XAttribute("LockTime", LockTime),
                new XAttribute("SmoothFall", SmoothFall),
                new XAttribute("SmoothRotate", SmoothRotate),
                new XAttribute("MusicOption",MusicOption),
                new XAttribute("SoundScheme",SoundScheme),
                new XAttribute("Chooser",Chooser),
                new XAttribute("Theme",Theme));

        }
        public StandardSettings()
        {

        }
        ~StandardSettings()
        {
            Save();
        }
        public StandardSettings(String pFileSource)
        {
            sLoadedSource = pFileSource;
            if (File.Exists(pFileSource))
            {
                XDocument loadFile = XDocument.Load(pFileSource);
                InitFromNode(loadFile.Root);
            }
        }

        public StandardSettings(XElement Source)
        {
            
            InitFromNode(Source);
        }
        public void Save()
        {
            if (_ManagerOwner.TryGetTarget(out SettingsManager mgr))
            {
                mgr.Save();
            }
            else
                Save(sLoadedSource);
        }
        public void Save(String pTargetFile)
        {
            if(pTargetFile!=null)
            {
                XDocument newDoc = new XDocument();
                XElement ThisNode = this.GetXmlData("Settings", null);
                newDoc.Add(ThisNode);
                newDoc.Save(pTargetFile);
            }

        }
        private void InitFromNode(XElement Node)
        {
            
            MusicVolume = Node.GetAttributeFloat("Music", 0.7f);
            EffectVolume = Node.GetAttributeFloat("Effects", 1.0f);
            DisplayScaleFactor = Node.GetAttributeFloat("DisplayScaleFactor", 1.6f);
            DASStartDelay = Node.GetAttributeInt("DASStartDelay", 450);
            DASRate = Node.GetAttributeInt("DASRate", 100);
            SmoothFall = Node.GetAttributeBool("SmoothFall", false);
            SmoothRotate = Node.GetAttributeBool("SmoothRotate", true);
            MusicOption = Node.GetAttributeString("MusicOption", "<RANDOM>");
            SoundScheme = Node.GetAttributeString("SoundScheme", "Default");
            Chooser = Node.GetAttributeString("Chooser", "Default");
            Theme = Node.GetAttributeString("Theme", "");
        }
    }
}