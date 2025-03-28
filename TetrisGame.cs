﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BASeTris.AssetManager;
using BASeCamp.BASeScores;
using BASeTris.GameStates;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Replay;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Audio;
using SkiaSharp;
using OpenTK.Input;
using BASeTris.Rendering.Adapters;
using BASeTris.GameStates.GameHandlers;
using System.Runtime.InteropServices;
using BASeTris.Settings;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BASeTris.AI;
using TKKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using BASeTris.Choosers;

namespace BASeTris
{
    public class TetrisGame : IStateOwner
    {

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();
        public enum KeyInputSource
        {
            Input_Keyboard,
            Input_HID
        }
        public Stopwatch GameTime { get; set; } = new Stopwatch();
        public static cNewSoundManager Soundman;

        public static ImageManager Imageman;

        //public static HighScoreManager ScoreMan;
        public static XMLScoreManager<TetrisHighScoreData> ScoreMan;

        //public static AudioThemeManager AudioThemeMan;
        //the  "stateless" randomizer below is intended for stuff that "doesn't matter" for deterministic stuff like replays. It shouldn't be used by, for example, choosers, but it should be used by stuff like drawing randomization, or choices made for drawing and themes and stuff.
        public static IRandomizer StatelessRandomizer = RandomHelpers.Construct();
        public static bool PortableMode = false;
        private GameState CurrentGameState = null;
        private IStateOwner GameOwner = null;
        private static string _datfolder = null;
        public static bool DJMode { get; set; } = true;
        static PrivateFontCollection pfc = new PrivateFontCollection();
        public static FontFamily[] PixelFonts;
        //public static FontFamily GBFont;
        public static FontFamily RetroFont;
        public static FontFamily LCDFont;
        public static SKTypeface[] PixelFontSKs;
        //public static SKTypeface GBFontSK;
        public static SKTypeface RetroFontSK;
        public static SKTypeface CreditFontSK;
        public static SKTypeface LCDFontSK;
        public static SKTypeface ArialFontSK;
        private static Image _TiledCache = null;
        public GameplayRecord GameRecorder { get; set; } = null;
        public AudioThemeManager AudioThemeMan { get { return GameOwner.AudioThemeMan; } set
            {
                //???
                GameOwner.AudioThemeMan = value;
            }
        }
        //private DateTime _GameStartTime = DateTime.MinValue;
        //private DateTime _LastPausedTime = DateTime.MinValue;
        public event EventHandler<GameClosingEventArgs> GameClosing
        {
            add
            {
                GameOwner.GameClosing += value;
            }
            remove
            {
                GameOwner.GameClosing -= value;
            }
        }

        public SettingsManager Settings
        {
            get
            {
                return GameOwner.Settings;
            }
        }
        public BCRect LastDrawBounds
        {
            get { return GameOwner.LastDrawBounds; }
        }
        //public DateTime GameStartTime { get { return _GameStartTime; } set { _GameStartTime = value; } }
        //public DateTime LastPausedTime
        // {
        //     get { return _LastPausedTime; }
        //     set { _LastPausedTime = value; }
        // }

        private TimeSpan _FinalGameTime = TimeSpan.MinValue;
        public TimeSpan FinalGameTime { get { return _FinalGameTime; } set { _FinalGameTime = value; } }

        public TimeSpan GetElapsedTime()
        {
            return GameTime.Elapsed + GameTimeOffset;

        }
        public TimeSpan GameTimeOffset { get; set; } = TimeSpan.Zero; //for 'loaded' games, we want to start the time at the saved time, so we will set this to add that timespan to GetElapsedTime when called.
        public event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange;
        public static Image StandardTiledTetrisBackground
        {
            get
            {
                if (_TiledCache == null)
                {
                    Image reduceit = Imageman["block_arrangement"];
                    //reduce total size to 20%.
                    Bitmap ReduceSize = new Bitmap((int)(reduceit.Width * .1), (int)(reduceit.Height * .1));
                    using (Graphics greduce = Graphics.FromImage(ReduceSize))
                    {
                        greduce.InterpolationMode = InterpolationMode.NearestNeighbor;
                        greduce.CompositingQuality = CompositingQuality.HighSpeed;
                        greduce.SmoothingMode = SmoothingMode.HighSpeed;
                        greduce.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                        greduce.DrawImage(reduceit, new Rectangle(0, 0, ReduceSize.Width, ReduceSize.Height));
                    }

                    _TiledCache = ReduceSize;
                }

                return _TiledCache;
            }
        }

        public static float GetDesiredEmSize(float emSize, Graphics g)
        {
            float realSize = (g.DpiY / 72) * emSize;
            return realSize;
        }
        public static T Successor<T>(T[] Elements, T Element)
        {
            int foundindex = Array.IndexOf(Elements, Element);
            if (foundindex == -1) throw new ArgumentException("Provided Array does not contain element.");
            return Elements[(foundindex + 1) % Elements.Length];
        }
        public static T Predecessor<T>(T[] Elements, T Element)
        {
            int foundindex = Array.IndexOf(Elements, Element);
            if (foundindex == -1) throw new ArgumentException("Provided Array does not contain element.");
            if (foundindex == 0) return Elements[Elements.Length];
            return Elements[foundindex - 1];
        }

        public static Font GetRetroFont(float desiredSize, double ScaleFactor, FontStyle desiredStyle = FontStyle.Regular, GraphicsUnit GUnit = GraphicsUnit.Point)
        {
            return new Font(RetroFont, (float)(desiredSize * ScaleFactor), desiredStyle, GUnit);
        }

        public static FontFamily GetMonospaceFont()
        {
            return RetroFont;
        }
        public static Color GetRainbowColor(Color Source, double Multiplier)
        {
            long usetick = DateTime.Now.Ticks;
            double usepercent = 240 / ((double)(usetick % 240) * Multiplier);
            return GetRainbowColor(Source, (float)usepercent);
        }
        public static Color GetRainbowColor(Color Source, float PercentOffset)
        {
            int useRotation = (int)(PercentOffset * 240) % 240;
            return HSLColor.RotateHue(Source, useRotation);

        }
        public static void DrawText(Graphics g, Font UseFont, String sText, Brush ForegroundBrush, Brush ShadowBrush, float XPosition, float YPosition, float ShadowXOffset = 5, float ShadowYOffset = 5, StringFormat sf = null)
        {
            g.DrawText(new DrawTextInformationGDI()
            {
                Text = sText,
                BackgroundBrush = Brushes.Transparent,
                DrawFont = UseFont,
                ForegroundBrush = ForegroundBrush,
                ShadowBrush = ShadowBrush,
                Position = new PointF(XPosition, YPosition),
                ShadowOffset = new PointF(ShadowXOffset, ShadowYOffset),
                Format = sf
            });
        }


        public static void InitState()
        {
            String ScoreFolder = Path.Combine
            (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BASeTris");
            if (!Directory.Exists(ScoreFolder))
            {
                Directory.CreateDirectory(ScoreFolder);
            }



            String ScoreFile = Path.Combine(ScoreFolder, "hi_score.xml");
            ScoreMan = XMLScoreManager<TetrisHighScoreData>.FromFile(ScoreFile);
            if (ScoreMan == null) ScoreMan = new XMLScoreManager<TetrisHighScoreData>(ScoreFile);
            String ApplicationFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String AssetsFolder = Path.Combine(ApplicationFolder, "Assets");

            Imageman = new ImageManager(TetrisGame.GetSearchFolders());

            var basesearchfolders = TetrisGame.GetSearchFolders();

            String[] AudioAssets = (from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Audio")).Concat
                (from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Assets")).Concat
                (from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Assets", "Audio")).Where((y) => Directory.Exists(y)).ToArray();
            var Driver = new BASSDriver();
            Soundman = new cNewSoundManager(Driver, AudioAssets);
            

            String[] sPixelFontNames = new[] { "BASeTris.Pixel.ttf", "BASeTris.EGB.ttf", "BASeTris.pp.ttf" };

            //RetroFont = GetResourceFont("BASeTris.Pixel.ttf");
            LCDFont = GetResourceFont("BASeTris.LCD.ttf");
            PixelFonts = sPixelFontNames.Select(GetResourceFont).ToArray();  
            //GBFont = GetResourceFont("BASeTris.EGB.ttf");

            RetroFont = PixelFonts[0]; 



            PixelFontSKs = sPixelFontNames.Select(GetResourceFontSK).ToArray(); 
            //GBFontSK = GetResourceFontSK("BASeTris.EGB.ttf");

            RetroFontSK = PixelFontSKs[0];
            LCDFontSK = GetResourceFontSK("BASeTris.LCD.ttf");
            CreditFontSK = GetResourceFontSK("BASeTris.Enterprise.ttf");
            ArialFontSK = SKTypeface.FromFamilyName("Arial");



            //RetroFont = pfc.Families[0];
        }
       
        private static SKPaint MeasureText = new SKPaint() { };
        public static SKRect MeasureSKText(SKTypeface pTypeFace, float pFontSize, String pText)
        {
            MeasureText.Typeface = pTypeFace;
            MeasureText.TextSize = pFontSize;
            SKRect Bounds = SKRect.Empty;
            MeasureText.MeasureText(pText, ref Bounds);
            return Bounds;
        }

        static Dictionary<String, SKTypeface> LoadedSKFonts = new Dictionary<String, SKTypeface>();

        private static SKTypeface GetResourceFontSK(String ResourceName)
        {
            if (LoadedSKFonts.ContainsKey(ResourceName))
            {
                return LoadedSKFonts[ResourceName];
            }
            else
            {
                SKTypeface result;
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(ResourceName);
                if (stream == null)
                    return null;

                result = SKTypeface.FromStream(stream);
                if (result != null)
                    LoadedSKFonts.Add(ResourceName, result);

                return result;



            }
        }


        static Dictionary<String, FontFamily> LoadedFonts = new Dictionary<String, FontFamily>();
        private static FontFamily GetResourceFont(String ResourceName)
        {

            if (LoadedFonts.ContainsKey(ResourceName))
            {
                return LoadedFonts[ResourceName];
            }
            else
            {
                Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);

                byte[] fontdata = new byte[fontStream.Length];
                fontStream.Read(fontdata, 0, (int)fontStream.Length);
                fontStream.Close();
                unsafe
                {
                    fixed (byte* pFontData = fontdata)
                    {
                        pfc.AddMemoryFont((System.IntPtr)pFontData, fontdata.Length);
                    }
                }

                LoadedFonts.Add(ResourceName, pfc.Families[0]);
                return pfc.Families[0];
            }
        }
        public TetrisGame(IStateOwner pOwner, GameState InitialGameState)
        {
            if (Soundman == null) InitState();
            else
            {
                Soundman.StopMusic();
                Soundman.Stop();
            }

            GameOwner = pOwner;
            CurrentGameState = InitialGameState;
        }

        public static T ClampValue<T>(T Value, T min, T max) where T : IComparable
        {
            //cast to IComparable
            IComparable cvalue = (IComparable)Value;
            IComparable cmin = (IComparable)min;
            IComparable cmax = (IComparable)max;

            //return (T)(cvalue.CompareTo(cmin)< 0 ?cmin:cvalue.CompareTo(cmax)>0?max:Value);
            if (cvalue.CompareTo(cmin) < 0)
            {
                return min;
            }
            else if (cvalue.CompareTo(cmax) > 0)
            {
                return max;
            }

            return Value;
        }

        public GameState CurrentState
        {
            get { return CurrentGameState; }
            set
            {
                var newvalue = value;
                var oldvalue = CurrentGameState;
                if (newvalue == oldvalue) return; //it's already the same so no need for any change here.
                BeforeGameStateChangeEventArgs eventargs = new BeforeGameStateChangeEventArgs(GameOwner,oldvalue, newvalue);
                BeforeGameStateChange?.Invoke(this, eventargs);
                if (eventargs.Cancel) return;

                CurrentGameState = value;
            }
        }

        public void EnqueueAction(Func<bool> pAction)
        {
            GameOwner.EnqueueAction(pAction);
        }

        public Rectangle GameArea { get; }

        public void Feedback(float Strength, int Length)
        {
            GameOwner?.Feedback(Strength, Length);
        }





        public double ScaleFactor
        {
            get { return this.GameOwner.ScaleFactor; }
        }
        public void SetScale(double pScale)
        {
            this.GameOwner.SetScale(pScale);
        }
        public void GameProc()
        {
            
            if (GameRecorder==null) //if we don't have a game recorder, see if the current state is a preparable game. if it is, than initialize with that prep data.
            {
                if (GameOwner.GetHandler() is IPreparableGame ipg)
                {
                    GameRecorder = new GameplayRecord(ipg.PrepInstance);
                }
            }
            CurrentGameState.GameProc(GameOwner);
            if (CurrentGameState.BG != null)
            {
                CurrentGameState.BG.FrameProc(GameOwner);
            }
            
        }

        /*Dictionary<Keys, GameState.GameKeys> KeyMapping = new Dictionary<Keys, GameState.GameKeys>()
        {
            {Keys.Left, GameState.GameKeys.GameKey_Left},
            {Keys.Right, GameState.GameKeys.GameKey_Right},
            {Keys.Down, GameState.GameKeys.GameKey_Down},
            {Keys.Up, GameState.GameKeys.GameKey_Drop},
            {Keys.X, GameState.GameKeys.GameKey_RotateCW},
            {Keys.Z, GameState.GameKeys.GameKey_RotateCCW},
            {Keys.Pause, GameState.GameKeys.GameKey_Pause},
            {Keys.P, GameState.GameKeys.GameKey_Pause},
            {Keys.Space, GameState.GameKeys.GameKey_Hold},
            {Keys.Return,GameState.GameKeys.GameKey_MenuActivate },
            {Keys.F2, GameState.GameKeys.GameKey_Debug1},
            {Keys.F7, GameState.GameKeys.GameKey_Debug2},
            {Keys.F11,GameState.GameKeys.GameKey_Debug3}
        };

        Dictionary<Key, GameState.GameKeys> KeyMappingTK = new Dictionary<Key, GameState.GameKeys>()
        {
            {Key.Left, GameState.GameKeys.GameKey_Left},
            {Key.Right, GameState.GameKeys.GameKey_Right},
            {Key.Down, GameState.GameKeys.GameKey_Down},
            {Key.Up, GameState.GameKeys.GameKey_Drop},
            {Key.X, GameState.GameKeys.GameKey_RotateCW},
            {Key.Z, GameState.GameKeys.GameKey_RotateCCW},
            {Key.Pause, GameState.GameKeys.GameKey_Pause},
            {Key.P, GameState.GameKeys.GameKey_Pause},
            {Key.Space, GameState.GameKeys.GameKey_Hold},
            {Key.Enter,GameState.GameKeys.GameKey_MenuActivate },
            {Key.F2, GameState.GameKeys.GameKey_Debug1},
            {Key.F7, GameState.GameKeys.GameKey_Debug2},
            {Key.F11,GameState.GameKeys.GameKey_Debug3},
            {Key.F12,GameState.GameKeys.GameKey_Debug4 },
            {Key.Number5,GameState.GameKeys.GameKey_Debug5 }
        };
        */

        public GameState.GameKeys? TranslateKey(Keys source)
        {
            if (Settings.hasAssignedKeyboardKey((int)source))
                return Settings.GetKeyboardKeyAssignment((int)source);
            return null;
        }
        public GameState.GameKeys? TranslateKey(TKKey source)
        {

            if (Settings.hasAssignedKeyboardKey((int)source))
                return Settings.GetKeyboardKeyAssignment((int)source);
            return null;
        }
        //GameplayRecord GameKeyRecorder = new GameplayRecord();
        public void HandleGameKey(IStateOwner pOwner, GameState.GameKeys g, KeyInputSource pSource)
        {
            if (pSource == KeyInputSource.Input_Keyboard && CurrentGameState is IDirectKeyboardInputState && (CurrentGameState as IDirectKeyboardInputState).AllowDirectKeyboardInput()) return; //do nothing if it supports that interface.
            if (pSource == KeyInputSource.Input_HID && CurrentGameState is IDirectGamepadInputState && (CurrentGameState as IDirectGamepadInputState).AllowDirectGamepadInput()) return;
            if (GameRecorder != null && CurrentState is GameplayGameState)
            {
                if (pOwner is IGamePresenter igp)
                {
                    var gp = igp.GetPresenter();
                    if (gp != null && gp.ai is ReplayInputInjector)
                        return;
                }
                GameRecorder.AddKeyRecord(GetElapsedTime(), g);
            }
            CurrentGameState.HandleGameKey(pOwner, g);
        }

        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            RenderingProvider.Static.DrawElement(this, g, CurrentGameState, new BaseDrawParameters(Bounds));
            //CurrentGameState.DrawForegroundEffect(this, g, Bounds);
        }

        
        public static T Choose<T>(IEnumerable<T> ChooseArray,IRandomizer rgen = null)
        {
            if (rgen == null) rgen = TetrisGame.StatelessRandomizer;
            SortedList<double, T> sorttest = new SortedList<double, T>();
            foreach (T loopvalue in ChooseArray)
            {
                double rgg;
                do
                {
                    rgg = rgen.NextDouble();
                } while (sorttest.ContainsKey(rgg));

                sorttest.Add(rgg, loopvalue);
            }

            //return the first item.
            return sorttest.First().Value;
        }

        public static Image ApplyImageAttributes(Image applyto, ImageAttributes applyattribs)
        {
            Image newimage = new Bitmap(applyto.Width, applyto.Height);
            using (Graphics usegraph = Graphics.FromImage(newimage))
            {
                usegraph.DrawImage(applyto, new Rectangle(0, 0, applyto.Width, applyto.Height), 0, 0, applyto.Width, applyto.Height, GraphicsUnit.Pixel, applyattribs);
            }


            return newimage;
        }

        private static List<DeletionHelper> QueuedDeletions = new List<DeletionHelper>();

        public static void QueueDelete(String foldername)
        {
            if (!QueuedDeletions.Exists((q) => (q.DeleteThis == foldername)))
                QueuedDeletions.Add(new DeletionHelper(foldername));
        }

        public static String[] GetSearchFolders()
        {
            return new String[] { GetLocalAssets(), AppDataFolder }.Concat(CommandLineDataFolder??new string[] { }).ToArray();
        }

        private static String GetLocalAssets()
        {
            String exepath = "";
            if (Debugger.IsAttached)
            {
                int numUppies = 1;
                while (numUppies < 10)
                {
                    String testDir = Path.Combine(new FileInfo(Application.ExecutablePath).DirectoryName, String.Join("", Enumerable.Repeat("..\\", numUppies)),"Assets");
                    if (Directory.Exists(testDir))
                    {
                        exepath = testDir;
                        return exepath;
                    }
                    numUppies++;
                }



                //exepath = Path.Combine(new FileInfo(Application.ExecutablePath).DirectoryName, "..", "..");
            }
            else
            {
                exepath = (new FileInfo(Application.ExecutablePath).DirectoryName);
            }
            //get the executable path.

            //append APPDATA to that exe path.
            return Path.Combine(exepath, "Assets");
        }

        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            GameOwner?.SetDisplayMode(pMode);
        }
        private static String[] _CommandLineDataFolder;
        public static string[] CommandLineDataFolder { 
            
            
            get {
                if (_CommandLineDataFolder != null)
                {
                    return _CommandLineDataFolder;
                }
                //check for commandline...
                var allargs = System.Environment.GetCommandLineArgs().ToList();
                int usefoundlocation = -1;
                String joined = String.Join(" ", System.Environment.GetCommandLineArgs());

                int hyphenfind = joined.IndexOf("-datafolder:", StringComparison.OrdinalIgnoreCase);
                int slashfind = joined.IndexOf("/datafolder:", StringComparison.OrdinalIgnoreCase);
                if (hyphenfind > -1) usefoundlocation = hyphenfind;
                else if (slashfind > -1) usefoundlocation = slashfind;


                if (usefoundlocation > -1)
                {
                    int firstchar = usefoundlocation + 12;
                    int endchar = -1;
                    //if the first character is a quote, find the next quote; increment firstchar to avoid capturing that quote later, as well.

                    if (joined[firstchar] == '\'')
                    {
                        endchar = joined.IndexOf('\'', firstchar + 1);
                        firstchar++;
                    }
                    else
                    {
                        endchar = joined.IndexOf(' ', firstchar + 1);
                        if (endchar == -1) endchar = joined.Length;
                    }

                    _CommandLineDataFolder = joined.Substring(firstchar, endchar - firstchar).Split(';');
                    return _CommandLineDataFolder;
                }
                else
                {
                    return null;
                }

            } }


        private static String _AppDataFolder = null;
        public static String AppDataFolder
        {
            get
            {
                if (_AppDataFolder != null) return _AppDataFolder;
                String DataFolder = "";
                try
                {
                    if (!PortableMode)
                    {
                        String RealAppDataFolder = Path.Combine
                        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "BASeTris") ;
                        return DataFolder = RealAppDataFolder;

                    }
                    else {
                        //if Portable Mode, use the location of the executable.
                        String sExecutableLocation = Assembly.GetExecutingAssembly().Location;
                        return DataFolder = Path.Combine(Path.GetDirectoryName(sExecutableLocation), "Assets");
                    }
                }
                finally
                {
                    _AppDataFolder = DataFolder;
                }
            }
        }
        public static String GetReplayDataPath(Type HandlerType)
        {
            String sUsePath = Path.Combine(TetrisGame.AppDataFolder, "Replay", HandlerType.Name);
            return Path.Combine(sUsePath, DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss.btreplay"));
        }
        public static String GetSuspendedGamePath(Type HandlerType)
        {
            String SuspendedGameFilename = Path.Combine(TetrisGame.AppDataFolder, "Suspend", HandlerType.Name + ".suspend");
            return SuspendedGameFilename;
        }
        public static void EnsurePath(String sPath)
        {
            if (Path.Exists(sPath) || File.Exists(sPath)) return;
            String sDirectoryName = Path.GetDirectoryName(sPath);
            Directory.CreateDirectory(sDirectoryName);
        }
        public static String FancyNumber(int number)
        {
            String sNumber = number.ToString().Trim();
            String sEnder = "";
            if (sNumber.EndsWith("1")) sEnder = "st";
            else if (sNumber.EndsWith("2")) sEnder = "nd";
            else if (sNumber.EndsWith("3")) sEnder = "rd";
            else
                sEnder = "th";
            return sNumber + sEnder;
        }
        public static Dictionary<String, List<SKBitmap>> GetTetrominoBitmapsSK(SKRect Bounds, NominoTheme UseTheme, IBlockGameCustomizationHandler handler,TetrisField PlayField = null, float ScaleFactor = 1)
        {
            Dictionary<String, List<SKBitmap>> TetrominoImages = new Dictionary<String, List<SKBitmap>>();
            float useSize = 18 * ScaleFactor;
            SKSize useTetSize = new SKSize(useSize, useSize);
            Nomino[] AllNominos = handler.GetNominos();
            SKBitmap[] bitmaps = new SKBitmap[AllNominos.Length];
            foreach(var iterate in AllNominos)
            {
                UseTheme.ApplyTheme(iterate, handler, PlayField, NominoTheme.ThemeApplicationReason.Normal);
            }

            for(int i=0;i<AllNominos.Length;i++)
            {
                bitmaps[i] = TetrisGame.OutlineImageSK(AllNominos[i].GetImageSK(useTetSize));
                String GetNominoKey = UseTheme.GetNominoKey(AllNominos[i], handler, PlayField);
                if (!TetrominoImages.ContainsKey(GetNominoKey)) TetrominoImages.Add(GetNominoKey, new List<SKBitmap>());
                TetrominoImages[GetNominoKey].Add(bitmaps[i]);
            }
            
            return TetrominoImages;
        }
        public static T[] Coalesce<K,T>(Dictionary<K,List<T>> Input)
        {
            List<T> Result = new List<T>();
            foreach(var iterate in Input)
            {
                Result.AddRange(iterate.Value);
            }

            return Result.ToArray();

        }
        public static Dictionary<String, List<Image>> GetTetrominoBitmaps(RectangleF Bounds, NominoTheme UseTheme,IBlockGameCustomizationHandler Handler, TetrisField PlayField = null, float ScaleFactor = 1)
        {
            Dictionary<String, List<Image>> TetrominoImages = new Dictionary<String, List<Image>>();
            float useSize = 18 * ScaleFactor;
            SizeF useTetSize = new SizeF(useSize, useSize);
            Nomino[] AllNominos = Handler.GetNominos();
            Image[] bitmaps = new Image[AllNominos.Length];
            
            foreach(var nom in AllNominos)
            {
                UseTheme.ApplyTheme(nom, Handler, PlayField, NominoTheme.ThemeApplicationReason.Normal);
            }

            for(int i=0;i<AllNominos.Length;i++)
            {
                bitmaps[i] = OutLineImage(AllNominos[i].GetImage(useTetSize));
                String NominoKey = UseTheme.GetNominoKey(AllNominos[i], Handler, PlayField);
                if (!TetrominoImages.ContainsKey(NominoKey))
                    TetrominoImages.Add(NominoKey, new List<Image>());
                TetrominoImages[NominoKey].Add(bitmaps[i]);
            }

            return TetrominoImages;
        }

        public static Image OutLineImage(Image Input)
        {
            Bitmap BuildImage = new Bitmap(Input.Width + 6, Input.Height + 6);
            using (Graphics useG = Graphics.FromImage(BuildImage))
            {
                useG.CompositingQuality = CompositingQuality.HighQuality;
                useG.InterpolationMode = InterpolationMode.Bicubic;
                useG.SmoothingMode = SmoothingMode.AntiAlias;
                var shadowtet = GetShadowAttributes(0f);
                int offset = 2;
                foreach (Point shadowblob in new Point[] { new Point(offset, offset), new Point(-offset, offset), new Point(offset, -offset), new Point(-offset, -offset) })
                {
                    useG.DrawImage(Input, new Rectangle(3 + shadowblob.X, 3 + shadowblob.Y, Input.Width, Input.Height), 0, 0, Input.Width, Input.Height, GraphicsUnit.Pixel, shadowtet);
                }

                useG.DrawImage(Input, new Point(3, 3));
            }

            return BuildImage;
        }
        static Dictionary<Color, Brush> ColorBrushes = new Dictionary<Color, Brush>();
        public static Brush GetColorBrush(Color src)
        {
            if(!ColorBrushes.ContainsKey(src))
            {
                ColorBrushes.Add(src, new SolidBrush(src));
            }
            return ColorBrushes[src];
        }


        //SkiaSharp implementation of OutlineImage.
        public static SKBitmap OutlineImageSK(SKBitmap Input,int OutlineWidth=3)
        {
            SKImageInfo skinfo = new SKImageInfo(Input.Width + (OutlineWidth * 2), Input.Height + (OutlineWidth * 2));
            SKBitmap BuildImage = new SKBitmap(skinfo, SKBitmapAllocFlags.ZeroPixels);
            using (SKCanvas useG = new SKCanvas(BuildImage))
            {
                //may need to use a Color Matrix like the GDI+ version.
                useG.Clear(SKColors.Transparent);
                SKPaint Blacken = new SKPaint();
                Blacken.ColorFilter = SKColorMatrices.GetBlackener();
                int offset = OutlineWidth / 2;
                foreach (SKPoint shadowblob in new SKPoint[] { new SKPoint(0, 0), new SKPoint(0, offset * 2), new SKPoint(offset * 2, 0), new SKPoint(offset * 2, offset * 2) })
                {
                    SKRect TargetPos = new SKRect(shadowblob.X, shadowblob.Y, Input.Width, Input.Height);

                    useG.DrawBitmap(Input, new SKRect(TargetPos.Left, TargetPos.Top, TargetPos.Left + Input.Width, TargetPos.Top + Input.Height), Blacken);

                }
                useG.DrawBitmap(Input, new SKPoint(offset, offset));
            }
            return BuildImage;
        }



        public static ImageAttributes GetShadowAttributes(float ShadowBrightness = 0.1f)
        {
            float brt = ShadowBrightness;
            ImageAttributes resultAttr = new ImageAttributes();
            System.Drawing.Imaging.ColorMatrix cm = new ColorMatrix
            (new float[][]
            {
                new float[] {brt, 0f, 0f, 0f, 0f},
                new float[] {0f, brt, 0f, 0f, 0f},
                new float[] {0f, 0f, brt, 0f, 0f},
                new float[] {0f, 0f, 0f, 1f, 0f},
                new float[] {0f, 0f, 0f, 0f, 1f}
            });
            resultAttr.SetColorMatrix(cm);
            return resultAttr;
        }
    }
    
    

}