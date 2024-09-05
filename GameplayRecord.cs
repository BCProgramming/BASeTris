using BASeCamp.Elementizer;
using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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

        public GameplayInitialStateData InitialData = null;
        //be sourced from it's own separate random instance.

        
        List<GameplayRecordElement> Elements = null;
        public int EntryCount { get { return Elements.Count; } }
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
        //for testing purposes. Just random inputs with random delay.
        public static GameplayRecord GetDrunkRecording(TimeSpan TotalLength)
        {
            GameplayRecord DrunkResult = new GameplayRecord();
            TimeSpan CurrentTime = TimeSpan.Zero;
            while (CurrentTime < TotalLength)
            {
                
                
                var Delta = new TimeSpan(TetrisGame.StatelessRandomizer.Next(5000000,10000000));
                var ChooseKey = TetrisGame.Choose(new GameKeys[] { GameKeys.GameKey_Left, GameKeys.GameKey_Right, GameKeys.GameKey_RotateCCW, GameKeys.GameKey_RotateCW,GameKeys.GameKey_Drop }, TetrisGame.StatelessRandomizer);
                //GameplayRecordElement gre = new GameplayRecordElement(CurrentTime += Delta, ChooseKey);

                DrunkResult.AddKeyRecord(CurrentTime+=Delta, ChooseKey);

            }
            DrunkResult.InitialData = new GameplayInitialStateData() { InitialOptions = new StandardTetrisPreparer(null)};
            DrunkResult.InitialData.InitialOptions.HandlerType = typeof(StandardTetrisHandler);
            return DrunkResult;


        }
        private GameplayRecord()
        {
        }
        
        public GameplayRecord(GamePreparerOptions gpo)
        {
            Elements = new List<GameplayRecordElement>();
            InitialData = new GameplayInitialStateData() { InitialOptions = gpo };
        }
        public GameplayRecordElement AddKeyRecord(TimeSpan Elapsed, GameState.GameKeys key)
        {
            if (IsRecordableKey(key))
            {
                var gree = new GameplayRecordElement(Elapsed, key);
                if (Elements == null) Elements = new List<GameplayRecordElement>();
                Elements.Add(gree);
                return gree;
            }
            return null;
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
        public void SaveRecordedGame(Type HandlerType)
        {




        }
        public void SaveToFile(String sFilePath)
        {
            XDocument xdoc = new XDocument(GetXmlData("Record", null));
            using (var gzout = new GZipStream(new FileStream(sFilePath, FileMode.Create), CompressionMode.Compress))
            {
                xdoc.Save(gzout);
            }
        }
        //possibly questionable constructor, little but much in terms of work being done in the initializer thingie
        public GameplayRecord(String sFilePath):this(XDocument.Load(new GZipStream(new FileStream(sFilePath,FileMode.Open),CompressionMode.Decompress)).Root,null)
        {

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
