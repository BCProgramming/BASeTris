using BASeCamp.Elementizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris
{
    public class GameplayRecord:IXmlPersistable
    {
        List<GameplayRecordElement> Elements = null;

        public GameplayRecord()
        {
            Elements = new List<GameplayRecordElement>();
        }
        public void AddKeyRecord(TimeSpan Elapsed, GameState.GameKeys key)
        {
            Elements.Add(new GameplayRecordElement(Elapsed, key));
        }
        public Queue<GameplayRecordElement> GetPlayQueue()
        {
            return GetPlayQueue(TimeSpan.Zero);
        }
        public Queue<GameplayRecordElement> GetPlayQueue(TimeSpan MinimumElapsed)
        {
            return new Queue<GameplayRecordElement>(from g in Elements where g.Elapsed > MinimumElapsed orderby g.Elapsed ascending select g);
        }

        public XElement GetXmlData(String pNodeName, Object pContext)
        {
            return new XElement(pNodeName, (from f in Elements orderby f.Elapsed ascending select f.GetXmlData("Element", null)));
        }

    }
    public class GameplayRecordElement:IXmlPersistable
    {
        public TimeSpan Elapsed { get; set; }
        public GameState.GameKeys GameKey { get; set; }
        public GameplayRecordElement(TimeSpan pElapsed, GameState.GameKeys pKey)
        {
            Elapsed = pElapsed;
            GameKey = pKey;
        }
        public GameplayRecordElement(XElement SourceData, Object pContext)
        {
            Elapsed = TimeSpan.FromTicks(SourceData.GetAttributeLong("Elapsed",0));
            GameKey = (GameState.GameKeys)SourceData.GetAttributeInt("Key",0);
        }
        public XElement GetXmlData(String pNodeName, Object pContext)
        {
            return new XElement(pNodeName, new XAttribute("Elapsed", Elapsed.Ticks), new XAttribute("Key", (int)GameKey));
        }
    }
}
