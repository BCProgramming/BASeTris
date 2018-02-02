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
using bcHighScores;
using BaseTris.AssetManager;
using BASeTris.AssetManager;

namespace BASeTris
{
    public class TetrisGame : IStateOwner
    {
        public static cNewSoundManager Soundman;
        public static ImageManager Imageman;
        public static HighScoreManager ScoreMan;
        public static Random rgen = new Random();
        public static bool PortableMode = false;
        private GameState CurrentGameState = null;
        private IStateOwner GameOwner = null;
        private static string _datfolder = null;

        static PrivateFontCollection pfc = new PrivateFontCollection();
        public static FontFamily RetroFont;
        public static FontFamily GetMonospaceFont()
        {
            return RetroFont;
        }
        public static void InitState()
        {
            /*String ScoreFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BASeTris");
            if(!Directory.Exists(ScoreFolder))
            {
                Directory.CreateDirectory(ScoreFolder);
            }
            String ScoreFile = Path.Combine(ScoreFolder, "hi_score.dat");
            //ScoreMan = new HighScoreManager(ScoreFile);
            */

            String ApplicationFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String AssetsFolder = Path.Combine(ApplicationFolder, "Assets");

            Imageman = new ImageManager(TetrisGame.GetSearchFolders());

            String[] AudioAssets = (from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Audio")).ToArray();
            Soundman = new cNewSoundManager(new BASSDriver(), AudioAssets);

            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BASeTris.Pixel.ttf");

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
            RetroFont = pfc.Families[0];

        }
        public TetrisGame(IStateOwner pOwner)
        {
            if (Soundman == null) InitState();
            else
            {
                Soundman.StopMusic();
                Soundman.Stop();
            }
            GameOwner = pOwner;
            CurrentGameState = new StandardTetrisGameState();
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
        public GameState CurrentState { get { return CurrentGameState; } set { CurrentGameState = value; } }
        public void EnqueueAction(Action pAction)
        {
            GameOwner.EnqueueAction(pAction);
        }

        public Rectangle GameArea { get; }

        public void Feedback(float Strength,int Length)
        {
            GameOwner?.Feedback(Strength,Length);
        }

        public void AddGameObject(GameObject Source)
        {
            GameOwner.AddGameObject(Source);
        }

        public void AddParticle(Particle pParticle)
        {
            GameOwner.AddParticle(pParticle);
        }

        public void GameProc()
        {
            CurrentGameState.GameProc(GameOwner);
        }
        public void HandleGameKey(IStateOwner pOwner, GameState.GameKeys g)
        {
            CurrentGameState.HandleGameKey(pOwner, g);
        }
        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            CurrentGameState.DrawProc(this, g, Bounds);
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
                }
                while (sorttest.ContainsKey(rgg));
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
            return new String[] { GetLocalAssets(), AppDataFolder };
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
                if (hyphenfind > -1) usefoundlocation = hyphenfind; else if (slashfind > -1) usefoundlocation = slashfind;


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
                        DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            "BASeTris");
                    }

                 

                    return DataFolder;
                }
            }
        }


    }
}
