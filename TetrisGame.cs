using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BaseTris.AssetManager;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.GameStates;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Audio;

namespace BASeTris
{
    public class TetrisGame : IStateOwner
    {
        public enum KeyInputSource
        {
            Input_Keyboard,
            Input_HID
        }

        public static cNewSoundManager Soundman;

        public static ImageManager Imageman;

        //public static HighScoreManager ScoreMan;
        public static XMLScoreManager<TetrisHighScoreData> ScoreMan;
        public static AudioThemeManager AudioThemeMan;
        public static Random rgen = new Random();
        public static bool PortableMode = false;
        private GameState CurrentGameState = null;
        private IStateOwner GameOwner = null;
        private static string _datfolder = null;
        public static bool DJMode { get; set; } = true;
        static PrivateFontCollection pfc = new PrivateFontCollection();
        public static FontFamily RetroFont;
        private static Image _TiledCache = null;

        public static Image StandardTiledTetrisBackground
        {
            get
            {
                if (_TiledCache == null)
                {
                    Image reduceit = Imageman["block_arrangement"];
                    //reduce total size to 20%.
                    Bitmap ReduceSize = new Bitmap((int) (reduceit.Width * .1), (int) (reduceit.Height * .1));
                    using (Graphics greduce = Graphics.FromImage(ReduceSize))
                    {
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

        public static Font GetRetroFont(float desiredSize, double ScaleFactor, FontStyle desiredStyle = FontStyle.Regular, GraphicsUnit GUnit = GraphicsUnit.Point)
        {
            return new Font(RetroFont, (float) (desiredSize * ScaleFactor), desiredStyle, GUnit);
        }

        public static FontFamily GetMonospaceFont()
        {
            return RetroFont;
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

            AudioThemeMan = new AudioThemeManager(AudioTheme.GetDefault());

            String ScoreFile = Path.Combine(ScoreFolder, "hi_score.xml");
            ScoreMan = XMLScoreManager<TetrisHighScoreData>.FromFile(ScoreFile);
            if (ScoreMan == null) ScoreMan = new XMLScoreManager<TetrisHighScoreData>(ScoreFile);
            String ApplicationFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String AssetsFolder = Path.Combine(ApplicationFolder, "Assets");

            Imageman = new ImageManager(TetrisGame.GetSearchFolders());

            String[] AudioAssets = (from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Audio")).ToArray();
            Soundman = new cNewSoundManager(new BASSDriver(), AudioAssets);

            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BASeTris.Pixel.ttf");

            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int) fontStream.Length);
            fontStream.Close();
            unsafe
            {
                fixed (byte* pFontData = fontdata)
                {
                    pfc.AddMemoryFont((System.IntPtr) pFontData, fontdata.Length);
                }
            }

            RetroFont = pfc.Families[0];
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
            IComparable cvalue = (IComparable) Value;
            IComparable cmin = (IComparable) min;
            IComparable cmax = (IComparable) max;

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
            set { CurrentGameState = value; }
        }

        public void EnqueueAction(Action pAction)
        {
            GameOwner.EnqueueAction(pAction);
        }

        public Rectangle GameArea { get; }

        public void Feedback(float Strength, int Length)
        {
            GameOwner?.Feedback(Strength, Length);
        }

        public void AddGameObject(GameObject Source)
        {
            GameOwner.AddGameObject(Source);
        }

        public void AddParticle(Particle pParticle)
        {
            GameOwner.AddParticle(pParticle);
        }

        public double ScaleFactor
        {
            get { return this.GameOwner.ScaleFactor; }
        }

        public void GameProc()
        {
            CurrentGameState.GameProc(GameOwner);
        }

        Dictionary<Keys, GameState.GameKeys> KeyMapping = new Dictionary<Keys, GameState.GameKeys>()
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
            {Keys.F2, GameState.GameKeys.GameKey_Debug1},
            {Keys.F7, GameState.GameKeys.GameKey_Debug2}
        };


        public GameState.GameKeys? TranslateKey(Keys source)
        {
            if (KeyMapping.ContainsKey(source))
            {
                return KeyMapping[source];
            }

            return null;
        }

        public void HandleGameKey(IStateOwner pOwner, GameState.GameKeys g, KeyInputSource pSource)
        {
            if (pSource == KeyInputSource.Input_Keyboard && CurrentGameState is IDirectKeyboardInputState) return; //do nothing if it supports that interface.

            CurrentGameState.HandleGameKey(pOwner, g);
        }

        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            CurrentGameState.DrawProc(this, g, Bounds);
            CurrentGameState.DrawForegroundEffect(this, g, Bounds);
        }


        public static T Choose<T>(IEnumerable<T> ChooseArray)
        {
            if (rgen == null) rgen = new Random();
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
            return new String[] {GetLocalAssets(), AppDataFolder};
        }

        private static String GetLocalAssets()
        {
            String exepath;
            if (Debugger.IsAttached)
            {
                exepath = Path.Combine(new FileInfo(Application.ExecutablePath).DirectoryName, "..", "..");
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

        public static String AppDataFolder
        {
            get
            {
                String DataFolder = "";

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

                    DataFolder = joined.Substring(firstchar, endchar - firstchar);
                    return DataFolder;
                }
                else
                {
                    if (!PortableMode)
                    {
                        DataFolder = Path.Combine
                        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "BASeTris");
                    }


                    return DataFolder;
                }
            }
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

        public static Dictionary<Type, Image> GetTetrominoBitmaps(RectangleF Bounds, TetrominoTheme UseTheme, TetrisField PlayField = null)
        {
            Dictionary<Type, Image> TetrominoImages = new Dictionary<Type, Image>();
            float useSize = 18 * ((float) Bounds.Height / 644f);
            SizeF useTetSize = new SizeF(useSize, useSize);
            Tetromino_I TetI = new Tetromino_I();
            Tetromino_J TetJ = new Tetromino_J();
            Tetromino_L TetL = new Tetromino_L();
            Tetromino_O TetO = new Tetromino_O();
            Tetromino_S TetS = new Tetromino_S();
            Tetromino_T TetT = new Tetromino_T();
            Tetromino_Z TetZ = new Tetromino_Z();


            UseTheme.ApplyTheme(TetI, PlayField);
            UseTheme.ApplyTheme(TetJ, PlayField);
            UseTheme.ApplyTheme(TetL, PlayField);
            UseTheme.ApplyTheme(TetO, PlayField);
            UseTheme.ApplyTheme(TetS, PlayField);
            UseTheme.ApplyTheme(TetT, PlayField);
            UseTheme.ApplyTheme(TetZ, PlayField);
            Image Image_I = OutLineImage(TetI.GetImage(useTetSize));
            Image Image_J = OutLineImage(TetJ.GetImage(useTetSize));
            Image Image_L = OutLineImage(TetL.GetImage(useTetSize));
            Image Image_O = OutLineImage(TetO.GetImage(useTetSize));
            Image Image_S = OutLineImage(TetS.GetImage(useTetSize));
            Image Image_T = OutLineImage(TetT.GetImage(useTetSize));
            Image Image_Z = OutLineImage(TetZ.GetImage(useTetSize));


            TetrominoImages.Add(typeof(Tetromino_I), Image_I);
            TetrominoImages.Add(typeof(Tetromino_J), Image_J);
            TetrominoImages.Add(typeof(Tetromino_L), Image_L);
            TetrominoImages.Add(typeof(Tetromino_O), Image_O);
            TetrominoImages.Add(typeof(Tetromino_S), Image_S);
            TetrominoImages.Add(typeof(Tetromino_T), Image_T);
            TetrominoImages.Add(typeof(Tetromino_Z), Image_Z);
            return TetrominoImages;
        }

        private static Image OutLineImage(Image Input)
        {
            Bitmap BuildImage = new Bitmap(Input.Width + 6, Input.Height + 6);
            using (Graphics useG = Graphics.FromImage(BuildImage))
            {
                var shadowtet = GetShadowAttributes(0f);
                int offset = 2;
                foreach (Point shadowblob in new Point[] {new Point(offset, offset), new Point(-offset, offset), new Point(offset, -offset), new Point(-offset, -offset)})
                {
                    useG.DrawImage(Input, new Rectangle(3 + shadowblob.X, 3 + shadowblob.Y, Input.Width, Input.Height), 0, 0, Input.Width, Input.Height, GraphicsUnit.Pixel, shadowtet);
                }

                useG.DrawImage(Input, new Point(3, 3));
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