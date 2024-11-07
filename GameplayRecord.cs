using BASeCamp.Elementizer;
using BASeTris.GameStates;
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

        //in addition to game keys, should we also record every nomino that comes from the chooser? Should the choosers themselves be in some way responsible for that tracking?
        
        List<GameplayRecordElement> Elements = null;
        public List<Nomino> GeneratedMinos = null;
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
            Elements = new List<GameplayRecordElement>();
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
        public void SaveRecordedGame(IStateOwner pOwner,Type HandlerType)
        {
            //saves a new recorded game file.




        }
        //savetofile and readfromfile go beyond the constructor and GetXmlData information, and also include the list of all chooser generated minos.
        public void SaveToFile(IStateOwner pOwner,String sFilePath,List<Nomino> GeneratedChooserMinos)
        {
            var generatedNominoElement = StandardHelper.SaveList(GeneratedChooserMinos, "ChooserGenerated", null, true);
            var Mainrecord = GetXmlData("Record", null);
            Mainrecord.Add(generatedNominoElement);
            XDocument xdoc = new XDocument(Mainrecord);
            using (var gzout = new GZipStream(new FileStream(sFilePath, FileMode.Create), CompressionMode.Compress))
            {
                xdoc.Save(gzout);
            }
        }


        public static GameplayRecord ReadFromFile(IStateOwner pOwner, String sFilePath, out List<Nomino> GeneratedChooserMinos)
        {
            XDocument xdoc = null;

            using (var readstream = LoadDataStream(sFilePath))
            {
                xdoc = XDocument.Load(readstream);
            }

            //we can hydrate a new Gameplayrecord from the root node. Then we can check for ChooserGenerated element.
            GameplayRecord gpr = new GameplayRecord(xdoc.Root, null);

            var ChooserGeneratedElement = xdoc.Root.Element("ChooserGenerated");

            if (ChooserGeneratedElement != null)
            {
                GeneratedChooserMinos = StandardHelper.ReadList<Nomino>(ChooserGeneratedElement, null);
            }
            else
            {
                GeneratedChooserMinos = new List<Nomino>(); //empty list
            }

            return gpr;
        }
        private static StreamReader LoadDataStream(String sFileName)
        {
            String sCheckText = "<?xml";
            bool isPlain = true;
            //for non Gzip, first characters will be "<?xml"
            using (StreamReader sr = new StreamReader(new FileStream(sFileName, FileMode.Open)))
            {
                char[] Target = new char[sCheckText.Length];
                sr.ReadBlock(Target, 0, sCheckText.Length);
                String sCheck = new string(Target);
                isPlain = sCheck.StartsWith(sCheckText);
                
            }
            return isPlain ? new StreamReader(new FileStream(sFileName, FileMode.Open)) : new StreamReader(new GZipStream(new FileStream(sFileName, FileMode.Open), CompressionMode.Decompress));

        }
        //possibly questionable constructor, little but much in terms of work being done in the initializer thingie
        public GameplayRecord(String sFilePath):this(XDocument.Load(LoadDataStream(sFilePath)).Root,null)
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
