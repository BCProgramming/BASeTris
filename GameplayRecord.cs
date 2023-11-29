using BASeCamp.Elementizer;
using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BASeTris.GameState;

namespace BASeTris
{
    /// <summary>
    /// Class representing a recorded game.
    /// </summary>
    public class GameplayRecord:IXmlPersistable
    {

        GameplayInitialStateData InitialData = null;
        //be sourced from it's own separate random instance.

        List<GameplayRecordElement> Elements = null;

        public Predicate<GameKeys> IsRecordableKey = IsRecordableKey_Default;
        private static GameKeys[] DefaultRejected = new[] { GameKeys.GameKey_Pause };
        private static bool IsRecordableKey_Default(GameKeys input)
        {
            return !DefaultRejected.Any((a) => a == input);
        }
        public GameplayRecord(XElement SourceData, Object pContext):this()
        {
            var xelem = SourceData.Element("InitialState");
            InitialData = new GameplayInitialStateData(xelem, pContext);
            foreach (var iterate in SourceData.Elements("Element"))
            {
                GameplayRecordElement loadelement = new GameplayRecordElement(iterate, pContext);
                Elements.Add(loadelement);
            }
            
        }
        private GameplayRecord()
        {
        }
        public GameplayRecord(int pSeed,GamePreparerOptions gpo):this(gpo)
        {
            InitialData = new GameplayInitialStateData() { InitialOptions = gpo };
         
            
        }
        public GameplayRecord(GamePreparerOptions gpo)
        {
            Elements = new List<GameplayRecordElement>();
        }
        public void AddKeyRecord(TimeSpan Elapsed, GameState.GameKeys key)
        {
            if(IsRecordableKey(key))
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
            return new XElement(pNodeName,InitialData.GetXmlData("InitialState",pContext), (from f in Elements orderby f.Elapsed ascending select f.GetXmlData("Element", null)));
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
    
    public class GameplayInitialStateData:IXmlPersistable
    {
        public GamePreparerOptions InitialOptions { get; set; } = null;
        public GameplayInitialStateData(XElement SourceData, Object pContext) 
        {
            String PreparerTypeName = SourceData.GetAttributeString("PreparerType", null);
            if (PreparerTypeName != null)
            {
                Type prepareType = Type.GetType(PreparerTypeName, false);
                if (prepareType != null)
                {
                    ConstructorInfo PersistableConstructor = prepareType.GetConstructor(new Type[] { typeof(XElement), typeof(Object) });
                    XElement PreparerOptions = SourceData.Element("PreparerOptions");
                    InitialOptions = (GamePreparerOptions)PersistableConstructor.Invoke(new object[] { PreparerOptions, pContext });

                }


            }


        }
        
        public GameplayInitialStateData()
        {
        }


        public virtual XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement xresult = new XElement(pNodeName);

            xresult.Add(new XAttribute("PreparerType", InitialOptions.GetType().AssemblyQualifiedName));
            XElement InitialOptionsElement = InitialOptions.GetXmlData("PreparerOptions", PersistenceData);
            xresult.Add(InitialOptionsElement);

            return xresult;
        }
    }
}
