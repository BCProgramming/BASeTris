using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.Elementizer;

namespace BASeTris
{
    public class StandardSettings : IXmlPersistable 
    {

        //Audio volume settings.
        public float MusicVolume = 0.7f;
        public float EffectVolume = 1.0f;
        
        public float DisplayScaleFactor = 1.6f;
        //TODO (?) Support additional Audio Drivers other than BASSDriver. Probably not worthwhile to be fair.

        public long DASStartDelay = 450; //key repeat delay for DAS.
        public long DASRate = 100;
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            
            return new XElement(pNodeName,
                new XAttribute("Music",MusicVolume),
                new XAttribute("Effects",EffectVolume),
                new XAttribute("DisplayFactor",DisplayScaleFactor));
        }
        public StandardSettings(String pFileSource)
        {
            XDocument loadFile = XDocument.Load(pFileSource);
            InitFromNode(loadFile.Root);
        }
        public StandardSettings(XElement Source)
        {
            InitFromNode(Source);
        }
        private void InitFromNode(XElement Node)
        {
            MusicVolume = Node.GetAttributeFloat("Music", 0.7f);
            EffectVolume = Node.GetAttributeFloat("Effects", 1.0f);
            DisplayScaleFactor = Node.GetAttributeFloat("DisplayScaleFactor", 1.6f);
        }
    }
}
