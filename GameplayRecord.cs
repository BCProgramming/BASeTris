using BASeCamp.Elementizer;
using BASeTris.Blocks;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //in addition to game keys, We need to have a record of all the Minos that were generated (in the case of game types that have choosers- eg Tetris Attack doesn't)

        //further, some game types need to add additional data- for example, Dr.Mario generates a new field for each level, so we'd need to have those sorts of actions generate a record and add it, and we also
        //will need to have this class delegate processing of actions to the game handler.
        
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

            var ElementNodes = SourceData.Elements("Element");

            foreach (var readElement in ElementNodes)
            {
                //does it have a type?
                String sType = readElement.GetAttributeString("Type", null);
                GameplayRecordElement AddElement = null;
                Type LoadType = typeof(GameplayRecordKeyPressElement);
                if (sType != null)
                {
                    LoadType = StandardHelper.ClassFinder(sType)??LoadType;
                        
                }
                ImageBlock? ib = null;
                var result = (ib?.IsAnimated ?? false);
                
                AddElement = (GameplayRecordElement)StandardHelper.ReadElement(LoadType, readElement, pContext);
                
                Elements.Add(AddElement);
            }


            /*foreach (var iterate in SourceData.Elements("Element"))
            {
                //String sGetType = iterate.GetAttributeString("Type", "");
                

                if (!String.IsNullOrWhiteSpace(sGetType))
                {
                    
                }

                GameplayRecordElement loadelement = new GameplayRecordKeyPressElement(iterate, pContext);
                Elements.Add(loadelement);
            }*/
            
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
                var gree = new GameplayRecordKeyPressElement(Elapsed, key);
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
            if (Elements.Count == 0) return; //if no elements (keypress, or otherwise) than no reason to save this replay.
            //saves a new recorded game file.
            //we want to save by type. We need to generate an appropriate path to save this to that includes it.
            String sAppFolder = TetrisGame.AppDataFolder;
            // add /Replay/Handler/<name>.btreplay and we'll generate a filename.
            String sTargetFile = Path.Combine(sAppFolder,"Replay",HandlerType.Name,DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffffff") + ".bcreplay");

            GameplayGameState usegameplaystate = null!;
            List<Nomino> GeneratedMinos = new List<Nomino>();
            if (pOwner.CurrentState is GameplayGameState) usegameplaystate = pOwner.CurrentState as GameplayGameState;
            if (pOwner.CurrentState is ICompositeState<GameplayGameState>) usegameplaystate = (pOwner.CurrentState as ICompositeState<GameplayGameState>)?.GetComposite()!;
            if (usegameplaystate != null)
            {
                var usechooser = usegameplaystate.GetChooser(pOwner);
                if (usechooser != null)
                {
                    GeneratedMinos = usechooser.AllGeneratedNominos;
                }
            }
            SaveToFile(pOwner, sTargetFile, GeneratedMinos);

        }
        /// <summary>
        /// read all the Replays for a given Handler type, returning an enumeration of the GameplayRecords that were deserialized.
        /// </summary>
        /// <param name="HandlerType"></param>
        /// <returns></returns>
        public static IEnumerable<GameplayRecord> GetHandlerReplays(Type HandlerType)
        {
            foreach (String sFile in GetHandlerReplayFiles(HandlerType))
            {
                        GameplayRecord loaded = null;
                        try
                        {
                            loaded = new GameplayRecord(sFile);
                        }
                        catch(Exception exr)
                        {
                        Trace.WriteLine($"Exception while loading a Gameplayrecord from file {sFile}\n{exr.ToString()}"); 
                            ; //swallow errors purely because we don't want to stop enumerating....
                        }
                        yield return loaded;
            }
        }

        public static IEnumerable<String> GetHandlerReplayFiles(Type HandlerType)
        {
            String sAppFolder = TetrisGame.AppDataFolder;
            // add /Replay/Handler/<name>.btreplay and we'll generate a filename.
            String sSourceDirectory =Path.Combine(sAppFolder,"Replay",HandlerType.Name);

            //get all .bcreplay files in the directory.
            if (Directory.Exists(sSourceDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(sSourceDirectory);
                foreach (var replayfile in di.EnumerateFiles("*.bcreplay"))
                {
                    yield return replayfile.FullName;
                }
            }
        }

        //savetofile and readfromfile go beyond the constructor and GetXmlData information, and also include the list of all chooser generated minos.
        public void SaveToFile(IStateOwner pOwner,String sFilePath,List<Nomino> GeneratedChooserMinos)
        {
            var generatedNominoElement = StandardHelper.SaveList(GeneratedChooserMinos, "ChooserGenerated", null, true);
            var Mainrecord = GetXmlData("Record", null);
            Mainrecord.Add(generatedNominoElement);
            XDocument xdoc = new XDocument(Mainrecord);
            String sFileDir = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sFileDir))
            {
                Directory.CreateDirectory(sFileDir);
            }

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
                foreach (var iterate in GeneratedChooserMinos)
                {
                    iterate.SetRotation(0);
                }
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

    
    public class GameplayRecordElement : IXmlPersistable
    {
        public TimeSpan Elapsed { get; set; }
        public GameplayRecordElement(TimeSpan pElapsed)
        {
            Elapsed = pElapsed;
        }
        public GameplayRecordElement(XElement SourceData, Object pContext)
        {
            Elapsed = TimeSpan.FromTicks(SourceData.GetAttributeLong("Elapsed",0));
        }

        public virtual XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            
            return new XElement(pNodeName);
            //throw new NotImplementedException();
        }
    }
    public class GameplayRecordKeyPressElement:GameplayRecordElement
    {
        
        public GameState.GameKeys GameKey { get; set; }
        public GameplayRecordKeyPressElement(TimeSpan pElapsed, GameState.GameKeys pKey):base(pElapsed)
        {
            
            GameKey = pKey;
        }
        public GameplayRecordKeyPressElement(XElement SourceData, Object pContext):base(SourceData,pContext)
        {
            
            GameKey = (GameState.GameKeys)SourceData.GetAttributeInt("Key",0);
        }
        public override XElement GetXmlData(String pNodeName, Object pContext)
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
