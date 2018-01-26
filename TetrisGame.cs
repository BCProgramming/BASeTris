using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BaseTris.AssetManager;
using BASeTris.AssetManager;

namespace BASeTris
{
    public class TetrisGame : IStateOwner
    {
        public static cNewSoundManager Soundman;
        public static ImageManager Imageman;
        public static Random rgen = new Random();
        public static bool PortableMode = false;
        private GameState CurrentGameState = null;
        private IStateOwner GameOwner = null;
        private static string _datfolder = null;
       
     
        public static void InitState()
        {
            String ApplicationFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String AssetsFolder = Path.Combine(ApplicationFolder, "Assets");
            List<String> AssetsFolders = new List<string>() { Path.Combine(AppDataFolder, "Audio") };
            if(Directory.Exists(AssetsFolder)) AssetsFolders.Add(AssetsFolder);
                Soundman = new cNewSoundManager(new BASSDriver(), AssetsFolders.ToArray());
        }
        public TetrisGame(IStateOwner pOwner)
        {
            if(Soundman==null) InitState();
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
        public GameState CurrentState { get{ return CurrentGameState; } set{ CurrentGameState = value; } }
        public void EnqueueAction(Action pAction)
        {
            GameOwner.EnqueueAction(pAction);
        }
        public void GameProc()
        {
            CurrentGameState.GameProc(GameOwner);
        }
        public void HandleGameKey(IStateOwner pOwner, GameState.GameKeys g)
        {
            CurrentGameState.HandleGameKey(pOwner,g);
        }
        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            CurrentGameState.DrawProc(this,g,Bounds);
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
            Graphics usegraph = Graphics.FromImage(newimage);
            usegraph.DrawImage(applyto, new Rectangle(0, 0, applyto.Width, applyto.Height), 0, 0, applyto.Width, applyto.Height, GraphicsUnit.Pixel, applyattribs);



            return newimage;
        }

        private static List<DeletionHelper> QueuedDeletions = new List<DeletionHelper>();
        public static void QueueDelete(String foldername)
        {
            if (!QueuedDeletions.Exists((q) => (q.DeleteThis == foldername)))
                QueuedDeletions.Add(new DeletionHelper(foldername));


        }

        public static String AppDataFolder
        {
            get
            {
                if (_datfolder != null) return _datfolder;

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

                    _datfolder = joined.Substring(firstchar, endchar - firstchar);
                    return _datfolder;

                }
                else
                {
                    if (!PortableMode)
                    {
                        _datfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            "BASeTris");
                    }
                    else
                    {
                        //get the executable path.
                        String exepath = (new FileInfo(Application.ExecutablePath).DirectoryName);
                        //append APPDATA to that exe path.
                        _datfolder = Path.Combine(exepath, "APPDATA");

                    }
                    return _datfolder;
                }
            }
        }


    }
}
