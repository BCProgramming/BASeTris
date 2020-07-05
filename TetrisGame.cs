using System;
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
using BASeTris.AssetManager;
using BASeTris.GameStates;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Replay;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Audio;
using SkiaSharp;
using OpenTK.Input;
using BASeTris.Rendering.Adapters;

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
        public static FontFamily LCDFont;
        public static SKTypeface RetroFontSK;
        public static SKTypeface LCDFontSK;
        private static Image _TiledCache = null;
        private DateTime _GameStartTime = DateTime.MinValue;
        private DateTime _LastPausedTime = DateTime.MinValue;

        public StandardSettings Settings
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
        public DateTime GameStartTime { get { return _GameStartTime; } set { _GameStartTime = value; } }
        public DateTime LastPausedTime
        {
            get { return _LastPausedTime; }
            set { _LastPausedTime = value; }
        }

        private TimeSpan _FinalGameTime = TimeSpan.MinValue;
        public TimeSpan FinalGameTime { get { return _FinalGameTime; } set { _FinalGameTime = value; } }

        public TimeSpan GetElapsedTime()
        {
            TimeSpan useCalc = (DateTime.Now - GameStartTime);

            if (FinalGameTime != TimeSpan.MinValue)
            {
                useCalc = FinalGameTime;
            }

            if (CurrentState is PauseGameState || CurrentState is UnpauseDelayGameState)
            {
                useCalc = LastPausedTime - GameStartTime;
            }

            return useCalc;
        }
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
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> Shufflethese)
        {
            if (rgen == null) rgen = new Random();
            var sl = new SortedList<float, T>();
            foreach (T iterate in Shufflethese)
            {
                sl.Add((float)rgen.NextDouble(), iterate);


            }
            Random rg = new Random();

            return sl.Select(iterator => iterator.Value);




        }
       
        public static void DrawTextSK(SKCanvas Target, String pText, SKPoint Position, SKTypeface typeface, SKColor Color, float DesiredSize, double ScaleFactor)
        {
            var rBytes = UTF8Encoding.Default.GetBytes(pText.ToCharArray());
            using (SKPaint skp = new SKPaint())
            {
                skp.Color = Color;
                /*skp.ColorFilter = SKColorFilter.CreateBlendMode(
                    SkiaSharp.Views.Desktop.Extensions.ToSKColor(
                    System.Drawing.Color.FromArgb(255, 255, 0, 0)),SKBlendMode.Screen);*/
                skp.TextSize = (float)(DesiredSize * ScaleFactor);
                skp.Typeface = typeface;
                skp.FilterQuality = SKFilterQuality.High;
                skp.SubpixelText = true;
                Target.DrawText(rBytes, Position.X, Position.Y, skp);
            }

            //SkiaCanvas.DrawText(UTF8Encoding.Default.GetBytes("testing".ToCharArray()), 67, 67, skp);



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
            DrawText(g, new DrawTextInformationGDI()
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
        public static void DrawTextSK(SKCanvas g, DrawTextInformationSkia DrawData)
        {

            var characterpositions = MeasureCharacterSizes(DrawData.ForegroundPaint, DrawData.Text);
            char[] drawcharacters = DrawData.Text.ToCharArray();
            foreach(int pass in new[] { 1,2})
            {
                for (int i = 0; i < drawcharacters.Length; i++)
                {
                    char drawcharacter = drawcharacters[i];
                    SKPoint DrawPosition = new SKPoint(characterpositions[i].Left + DrawData.Position.X, characterpositions[i].Top + DrawData.Position.Y);
                    DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, new SKPoint(characterpositions[i].Size.Width, characterpositions[i].Size.Height), i, drawcharacters.Length, pass);
                }
            }


        }
        public static void DrawText(Graphics g, DrawTextInformationGDI DrawData)
        {
            if (DrawData.Format == null)
            {
                DrawData.Format = new StringFormat();
            }

            //May 15th 2019- we now draw the string manually. None of this DrawString stuff.
            var characterpositions = MeasureCharacterSizes(g, DrawData.DrawFont, DrawData.Text);

            char[] drawcharacters = DrawData.Text.ToCharArray();
            g.PageUnit = GraphicsUnit.Pixel;
            for (int i = 0; i < drawcharacters.Length; i++)
            {
                //get the dimensions of this character

                char drawcharacter = drawcharacters[i];
                PointF DrawPosition = new PointF(characterpositions[i].Location.X + DrawData.Position.X, characterpositions[i].Location.Y + DrawData.Position.Y);
                DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, characterpositions[i].Size, i, drawcharacters.Length, 1);
            }

            //Draw the foreground
            for (int i = 0; i < drawcharacters.Length; i++)
            {
                //get the dimensions of this character
                char drawcharacter = drawcharacters[i];
                PointF DrawPosition = new PointF(characterpositions[i].Location.X + DrawData.Position.X, characterpositions[i].Location.Y + DrawData.Position.Y);
                DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, characterpositions[i].Size, i, drawcharacters.Length, 2);
            }
            //g.DrawString(DrawData.Text, DrawData.DrawFont, DrawData.ShadowBrush,DrawData.Position.X+DrawData.ShadowOffset.X, DrawData.Position.Y+DrawData.ShadowOffset.Y,DrawData.Format);
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

            RetroFont = GetResourceFont("BASeTris.Pixel.ttf");
            LCDFont = GetResourceFont("BASeTris.LCD.ttf");
            RetroFontSK = GetResourceFontSK("BASeTris.Pixel.ttf");
            LCDFontSK = GetResourceFontSK("BASeTris.LCD.ttf");
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
                BeforeGameStateChangeEventArgs eventargs = new BeforeGameStateChangeEventArgs(oldvalue, newvalue);
                BeforeGameStateChange?.Invoke(this, eventargs);
                if (eventargs.Cancel) return;

                CurrentGameState = value;
            }
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
            CurrentGameState.GameProc(GameOwner);
            if (CurrentGameState.BG != null)
            {
                CurrentGameState.BG.FrameProc(GameOwner);
            }
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
            {Key.F11,GameState.GameKeys.GameKey_Debug3}
        };


        public GameState.GameKeys? TranslateKey(Keys source)
        {
            if (KeyMapping.ContainsKey(source))
            {
                return KeyMapping[source];
            }

            return null;
        }
        public GameState.GameKeys? TranslateKey(Key source)
        {
            if(KeyMappingTK.ContainsKey(source))
            {
                return KeyMappingTK[source];
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
            RenderingProvider.Static.DrawElement(this, g, CurrentGameState, new BaseDrawParameters(Bounds));
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

        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            GameOwner?.SetDisplayMode(pMode);
        }
        private static String _AppDataFolder = null;
        public static String AppDataFolder
        {
            get
            {
                if (_AppDataFolder != null) return _AppDataFolder;
                String DataFolder = "";
                try
                {
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
                finally
                {
                    _AppDataFolder = DataFolder;
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
        public static Dictionary<Type, SKBitmap> GetTetrominoBitmapsSK(SKRect Bounds, TetrominoTheme UseTheme, TetrisField PlayField = null, float ScaleFactor = 1)
        {
            Dictionary<Type, SKBitmap> TetrominoImages = new Dictionary<Type, SKBitmap>();
            float useSize = 18 * ScaleFactor;
            SKSize useTetSize = new SKSize(useSize, useSize);
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
            SKBitmap Image_I = OutlineImageSK(TetI.GetImageSK(useTetSize));
            SKBitmap Image_J = OutlineImageSK(TetJ.GetImageSK(useTetSize));
            SKBitmap Image_L = OutlineImageSK(TetL.GetImageSK(useTetSize));
            SKBitmap Image_O = OutlineImageSK(TetO.GetImageSK(useTetSize));
            SKBitmap Image_S = OutlineImageSK(TetS.GetImageSK(useTetSize));
            SKBitmap Image_T = OutlineImageSK(TetT.GetImageSK(useTetSize));
            SKBitmap Image_Z = OutlineImageSK(TetZ.GetImageSK(useTetSize));


            TetrominoImages.Add(typeof(Tetromino_I), Image_I);
            TetrominoImages.Add(typeof(Tetromino_J), Image_J);
            TetrominoImages.Add(typeof(Tetromino_L), Image_L);
            TetrominoImages.Add(typeof(Tetromino_O), Image_O);
            TetrominoImages.Add(typeof(Tetromino_S), Image_S);
            TetrominoImages.Add(typeof(Tetromino_T), Image_T);
            TetrominoImages.Add(typeof(Tetromino_Z), Image_Z);
            return TetrominoImages;
        }
        public static Dictionary<Type, Image> GetTetrominoBitmaps(RectangleF Bounds, TetrominoTheme UseTheme, TetrisField PlayField = null, float ScaleFactor = 1)
        {
            Dictionary<Type, Image> TetrominoImages = new Dictionary<Type, Image>();
            float useSize = 18 * ScaleFactor;
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
        private static SKBitmap OutlineImageSK(SKBitmap Input)
        {
            int OutlineWidth = 3;
            SKImageInfo skinfo = new SKImageInfo(Input.Width + (OutlineWidth * 2), Input.Height + (OutlineWidth * 2));
            SKBitmap BuildImage = new SKBitmap(skinfo, SKBitmapAllocFlags.ZeroPixels);
            using (SKCanvas useG = new SKCanvas(BuildImage))
            {
                //may need to use a Color Matrix like the GDI+ version.

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
        // Measure the characters in a string with
        // no more than 32 characters.
        private static List<RectangleF> MeasureCharacterSizeInternal(
            Graphics gr, Font font, string text)
        {

            List<RectangleF> result = new List<RectangleF>();

            using (StringFormat string_format = new StringFormat())
            {
                RectangleF FullBound;

                string_format.Alignment = StringAlignment.Near;
                string_format.LineAlignment = StringAlignment.Near;
                string_format.Trimming = StringTrimming.None;
                string_format.FormatFlags =
                    StringFormatFlags.MeasureTrailingSpaces;
                var measurefull = gr.MeasureString(text, font, PointF.Empty, StringFormat.GenericDefault);
                FullBound = new RectangleF(0, 0, measurefull.Width, measurefull.Height);
                CharacterRange[] ranges = new CharacterRange[text.Length];
                for (int i = 0; i < text.Length; i++)
                {
                    ranges[i] = new CharacterRange(i, 1);
                }
                string_format.SetMeasurableCharacterRanges(ranges);

                // Find the character ranges.
                RectangleF rect = new RectangleF(0, 0, 10000, 100);
                Region[] regions =
                    gr.MeasureCharacterRanges(
                        text, font, FullBound,
                        string_format);

                // Convert the regions into rectangles.
                foreach (Region region in regions)
                    result.Add(region.GetBounds(gr));
            }

            return result;
        }
        private static List<SKRect> MeasureCharacterSizes(SKPaint skp,String text)
        {
            List<SKRect> results = new List<SKRect>();
            float xpos = 0;
            for(int index = 0;index < text.Length; index ++)
            {
                SKRect measuredchar = new SKRect();
                skp.MeasureText("█",ref measuredchar);
                measuredchar = new SKRect(xpos, measuredchar.Top, xpos + measuredchar.Width, measuredchar.Bottom);
                xpos += measuredchar.Width;
                results.Add(measuredchar);

            }
            return results;
        }
        // Measure the characters in the string.
        private static List<RectangleF> MeasureCharacterSizes(Graphics gr,
            Font font, string text)
        {
            List<RectangleF> results = new List<RectangleF>();

            // The X location for the next character.
            float x = 0;

            // Get the character sizes 31 characters at a time.
            for (int start = 0; start < text.Length; start += 32)
            {
                // Get the substring.
                int len = 32;
                if (start + len >= text.Length) len = text.Length - start;
                string substring = text.Substring(start, len);

                // Measure the characters.
                List<RectangleF> rects =
                    MeasureCharacterSizeInternal(gr, font, substring);

                // Remove lead-in for the first character.
                if (start == 0) x += rects[0].Left;

                // Save all but the last rectangle.
                for (int i = 0; i < rects.Count + 1 - 1; i++)
                {
                    RectangleF new_rect = new RectangleF(
                        x, rects[i].Top,
                        rects[i].Width, rects[i].Height);
                    results.Add(new_rect);

                    // Move to the next character's X position.
                    x += rects[i].Width;
                }
            }

            // Return the results.
            return results;
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