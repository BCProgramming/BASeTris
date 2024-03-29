using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.Choosers;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;
using BASeTris.Theme.Block;
using System.Reflection;
using System.Diagnostics;
using BASeTris.GameStates.Menu;

namespace BASeTris
{

    //TODO: both here, and elsewhere:
    //remove reliance of Type for unique indexing of Nominoes. Instead, we need to come up with some unique way to hash each unique tetromino.

    //placed here for reference:
    //possible theme idea: a theme which itself is a preference-based setup.
    //Instead of defining themes of it's own, it can use existing Theme types and be configured to set certain tetrominos to specific themes.
    //IConfigurableTheme would be a good name for the interface for that.
    //
    
    public abstract class NominoTheme
    {

        public enum ThemeApplicationReason
        {
            Normal,
            NewNomino,
            Theme_Changed,
            FieldSet
        }
        [Flags]
        public enum AdjacentBlockFlags
        {
            None = 0,
            Top = 1,
            Right = 2,
            Bottom = 4,
            Left = 8,
            TopRight = 16,
            BottomRight=32,
            BottomLeft=64,
            TopLeft=128
        }
        static Dictionary<(int, int), AdjacentBlockFlags> AdjacentLookup = new Dictionary<(int, int), AdjacentBlockFlags>()
            {
                {(-1,0),AdjacentBlockFlags.Left },
                {(1,0),AdjacentBlockFlags.Right },
                {(0,1),AdjacentBlockFlags.Bottom },
                {(0,-1),AdjacentBlockFlags.Top },


            };
        static Dictionary<(int, int), AdjacentBlockFlags> DiagonalLookup = new Dictionary<(int, int), AdjacentBlockFlags>()
            {
                {(1,-1),AdjacentBlockFlags.TopRight },
                {(1,1),AdjacentBlockFlags.BottomRight },
                {(-1,1),AdjacentBlockFlags.BottomLeft },
                {(-1,-1),AdjacentBlockFlags.TopLeft }
            };
        public static AdjacentBlockFlags GetAdjacentBlockFlags(Nomino group, NominoElement element,bool IncludeDiagonals = false)
        {
            AdjacentBlockFlags bbt = AdjacentBlockFlags.None;

            Dictionary<(int, int), AdjacentBlockFlags>[] DoCheck = IncludeDiagonals ? new[] { AdjacentLookup, DiagonalLookup } : new[] { AdjacentLookup };
            foreach (var dict in DoCheck)
            {
                foreach (var kvp in dict)
                {
                    if (group.Any((b) => (b.BaseX(), b.BaseY()) == (element.BaseX() + kvp.Key.Item1, element.BaseY() + kvp.Key.Item2))) bbt |= kvp.Value;
                }
            }
            return bbt;
        }
        private static Dictionary<AdjacentBlockFlags, AdjacentBlockFlags> RotateCWMapping = new Dictionary<AdjacentBlockFlags, AdjacentBlockFlags>()
        {
            {AdjacentBlockFlags.Top,AdjacentBlockFlags.Right },
            {AdjacentBlockFlags.Right,AdjacentBlockFlags.Bottom },
            {AdjacentBlockFlags.Bottom,AdjacentBlockFlags.Left },
            {AdjacentBlockFlags.Left,AdjacentBlockFlags.Top},
            {AdjacentBlockFlags.TopRight,AdjacentBlockFlags.BottomRight },
            {AdjacentBlockFlags.BottomRight,AdjacentBlockFlags.BottomLeft },
            {AdjacentBlockFlags.BottomLeft,AdjacentBlockFlags.TopRight }
        };
        public static AdjacentBlockFlags RotateAdjacentBlockFlags(AdjacentBlockFlags flags)
        {
            
            AdjacentBlockFlags result = AdjacentBlockFlags.None;

            foreach (var kvp in RotateCWMapping)
            {
                if (flags.HasFlag(kvp.Key)) result |= kvp.Value;
            }

            return result;

        }
        public static Func<NominoTheme>[] GetVisualizationThemes()
        {
            return new Func<NominoTheme>[] { () => new SNESTetris3Theme(), () => new GameBoyTetrominoTheme(), () => new SNESTetrominoTheme(), () => new NESTetrominoTheme(), () => new StandardTetrominoTheme(), () => new NESTetris2Theme(), () => new SNESTetris2Theme(), () => new GameBoyMottledTheme() };
        }
        public virtual ThemeImageProvider ThemeProvider { get; set; }
        public abstract String Name { get; }
        public virtual bool IsAnimated(NominoBlock block)
        {
            return false;
        }
        public virtual String GetNominoKey(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return GetNominoKey_Tetris(Group, GameHandler, Field);
        }
        [Obsolete("Nomino handling is too involved to index the images only by Type.")]
        public virtual String GetNominoTypeKey(Type src, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return src.FullName;
        }
        public String GetNominoKey_Tetris(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            if (Group is Tetromino)
            {
                return GetNominoTypeKey(Group.GetType(), GameHandler, Field);
            }
            else
            {
                String sStringRep = NNominoGenerator.StringRepresentation(NNominoGenerator.GetNominoPoints(Group));
                return sStringRep;
            }


        }
        
        public string GetNominoKey_LineSeries(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {

            //Dr Mario theme keys are based on the "Duomino" arrangement. We take the first type, and the second type and create a key for it.


            if (Group is Duomino.Duomino dm)
            {

                var OneBlock = dm.FirstBlock;
                var TwoBlock = dm.SecondBlock;
                if (OneBlock.Block is LineSeriesBlock sb1 && TwoBlock.Block is LineSeriesBlock sb2)
                {
                    return "1:" + sb1.CombiningIndex + ";2:" + sb2.CombiningIndex;
                }
            }
            else if (Group is Tetromino)
            {
                //means we are servicing a Tetris 2 Handler.
                //Since tetrominoes use 4 blocks, we've got our work cut out for us in terms of
                //creating the correct keys. means we have a lot of possibilities to cache!
                StringBuilder sbKey = new StringBuilder();
                int index = 0;
                foreach (var block in Group)
                {
                    if (block.Block is LineSeriesBlock b)
                    {
                        sbKey.Append($"{index++}:{b.CombiningIndex};");
                    }
                }
                return sbKey.ToString();
            }

            return GetNominoKey(Group, GameHandler, Field);

            //return base.GetNominoKey(Group, GameHandler, Field);
        }

        public abstract void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);
        public abstract void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field);
        public abstract PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler);

        protected Dictionary<String, Image> _Cache = new Dictionary<string, Image>();

        private Image AddCachedImage(String Key, Image pImage)
        {
            if (_Cache.ContainsKey(Key)) _Cache[Key] = pImage;
            else
            {
                _Cache.Add(Key, pImage);
                return pImage;
            }
            return null;
        }
        private Image GetCachedImage(String Key)
        {
            if (_Cache.ContainsKey(Key))
                return _Cache[Key];
            else return null;
        }
        protected Image AddCachedImage(String pKey, Size pSize, Color pColor, Image pImage)
        {
            String pBuildKey = BuildCacheKey(pKey, pSize, pColor);
            return AddCachedImage(pBuildKey, pImage);
        }
        protected Image GetCachedImage(String pKey, Size pSize, Color pColor)
        {
            String pBuildKey = BuildCacheKey(pKey, pSize, pColor);
            return GetCachedImage(pBuildKey);
        }
        private String BuildCacheKey(String pKey, Size pSize, Color pColor)
        {
            return pKey + "(" + pSize.Width + "," + pSize.Height + ")[" + pColor.R + "," + pColor.G + "," + pColor.B + "]";
        }
        public static NominoTheme GetNewThemeInstanceByName(String sThemeName,Type HandlerType=null)
        {
            //given a Theme name, finds the Theme with that name and returns a new instance of that NominoTheme.
            foreach (Type t in Program.DITypes[typeof(NominoTheme)].ManagedTypes)
            {
                ConstructorInfo ci = t.GetConstructor(new Type[] { });
                if (ci != null)
                {
                    HandlerThemeAttribute hta = t.GetCustomAttribute(typeof(HandlerThemeAttribute)) as HandlerThemeAttribute;
                    if (hta != null && HandlerType != null && !hta.HandlerType.Any((a) => a.IsAssignableFrom(HandlerType)))
                    {
                        Debug.Print("Skipping Theme " + t.GetType().Name + " as it does not specify as supporting Handler " + HandlerType.Name);
                        continue;
                    }
                    
                    else
                    {
                        NominoTheme buildResult = (NominoTheme)ci.Invoke(new object[] { });
                        if (String.IsNullOrEmpty(sThemeName) || String.Equals(buildResult.Name, sThemeName, StringComparison.OrdinalIgnoreCase))
                            return buildResult;
                    }
                    //MenuStateThemeSelection msst = new MenuStateThemeSelection(buildResult.Name, themeiter, () => buildResult);
                }


            }
            return null; //no match.
        }
        public static Image[] GetImageRotations(Image Source)
        {
            lock (Source)
            {
                Image Rotate90 = new Bitmap(Source);
                Image Rotate180 = new Bitmap(Source);
                Image Rotate270 = new Bitmap(Source);



                Rotate90.RotateFlip(RotateFlipType.Rotate90FlipNone);
                Rotate180.RotateFlip(RotateFlipType.Rotate180FlipNone);
                Rotate270.RotateFlip(RotateFlipType.Rotate270FlipNone);
                return new Image[] { Source, Rotate90, Rotate180, Rotate270 };
            }
        }


        public static SKBitmap RotateBitmap(SKBitmap bitmap, int degrees)
        {
            var rotated = new SKBitmap(bitmap.Width, bitmap.Height);

            var surface = new SKCanvas(rotated);
            surface.Clear(SKColors.Transparent);
            surface.Translate(rotated.Width / 2, rotated.Height / 2);
            surface.RotateDegrees(degrees);
            surface.Translate(-rotated.Width / 2, -rotated.Height / 2);
            surface.DrawBitmap(bitmap, 0, 0);

            return rotated;
        }

        public static SKImage[] GetImageRotations(SKBitmap Source)
        {
            lock (Source)
            {

                SKBitmap Rotate90 = RotateBitmap(Source, 90);
                SKBitmap Rotate180 = RotateBitmap(Source, 180);
                SKBitmap Rotate270 = RotateBitmap(Source, 270);


                return new SKImage[] { SKImage.FromBitmap(Source), SKImage.FromBitmap(Rotate90), SKImage.FromBitmap(Rotate180), SKImage.FromBitmap(Rotate270) };


            }
        }

        static Color DefaultTint = Color.Transparent;
        protected PlayFieldBackgroundInfo GetColoredBackground(Color MainColor, Color? TintColor = null)
        {
            Image ResultImage = new Bitmap(250, 500);
            using (Graphics drawdark = Graphics.FromImage(ResultImage))
            {
                drawdark.Clear(MainColor);
            }
            Color UseTint = TintColor == null ? Color.Transparent : TintColor.Value;
            return new PlayFieldBackgroundInfo(ResultImage, UseTint);
        }
    }
    public class PlayFieldBackgroundInfo
    {
        public Image BackgroundImage;
        public Color TintColor = Color.Transparent;
        public PlayFieldBackgroundInfo(Image pBackgroundImage, Color pTintColor)
        {
            BackgroundImage = pBackgroundImage;
            TintColor = pTintColor;
        }

    }


    public class TetrominoBlockTheme
    {
        public Color BlockColor;
        public Color BlockInnerColor;

        public TetrominoBlockTheme(Color BaseColor, Color InnerColor)
        {
            BlockColor = BaseColor;
            BlockInnerColor = InnerColor;
        }
    }
    [HandlerTheme("Standard", typeof(StandardTetrisHandler))]
    [ThemeDescription("A Good Default.")]
    public class StandardTetrominoTheme : NominoTheme
    {
        StandardColouredBlock.BlockStyle _Style;

        public override String Name { get { return "Standard";} }
        public StandardColouredBlock.BlockStyle BlockStyle
        {
            get { return _Style; }
            set { _Style = value; }
        }

        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background",0.5f],Color.Transparent);
        }
        public StandardTetrominoTheme():this(StandardColouredBlock.BlockStyle.Style_Shine)
        {

        }
        public StandardTetrominoTheme(StandardColouredBlock.BlockStyle pBlockStyle)
        {
            _Style = pBlockStyle;
        }

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler,TetrisField Field)
        {
            int useLevel = TetrisGame.StatelessRandomizer.Next(50);
            ApplyColorSet(Group,useLevel);
        }

        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler,TetrisField Field, ThemeApplicationReason Reason)
        {
            var LineCount = GameHandler==null?0:((GameHandler.Statistics is TetrisStatistics ts) ? ts.LineCount : 0);
            int CurrLevel = Field == null ? 0 : (int) (LineCount / 10);
            ApplyColorSet(Group, CurrLevel);
        }

        private StandardColouredBlock.BlockStyle[] usabletypes = new StandardColouredBlock.BlockStyle[] {StandardColouredBlock.BlockStyle.Style_Chisel, StandardColouredBlock.BlockStyle.Style_CloudBevel, StandardColouredBlock.BlockStyle.Style_HardBevel, StandardColouredBlock.BlockStyle.Style_Shine,StandardColouredBlock.BlockStyle.Style_Mottled,StandardColouredBlock.BlockStyle.Style_Grain};

        private Dictionary<Type, StandardColouredBlock.BlockStyle> GetBlockStyleLookup(Type[] Types)
        {
            Dictionary<Type, StandardColouredBlock.BlockStyle> Result = new Dictionary<Type, StandardColouredBlock.BlockStyle>();
            foreach (var iterate in Types)
            {
                StandardColouredBlock.BlockStyle selectstyle = TetrisGame.Choose(usabletypes);
                Result.Add(iterate, selectstyle);
            }

            return Result;
        }

        public static Color GetStandardColor(Nomino source, int Level)
        {
            Color[] Colors = new Color[] {Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Red, Color.Blue, Color.OrangeRed};
            int useIndex = -1;
            if (source is Tetromino_I)
            {
                useIndex = 0;
            }
            else if (source is Tetromino_O)
            {
                useIndex = 1;
            }
            else if (source is Tetromino_T)
            {
                useIndex = 2;
            }
            else if (source is Tetromino_S)
            {
                useIndex = 3;
            }
            else if (source is Tetromino_Z)
            {
                useIndex = 4;
            }
            else if (source is Tetromino_J)
                useIndex = 5;
            else if (source is Tetromino_L)
                useIndex = 6;

            if (useIndex == -1)
            {
                useIndex = rg.Next(Colors.Length);
                return Colors[useIndex];
            }

            return Colors[(useIndex + Level) % Colors.Length];
        }

        



        private static Random rg = new Random();
        private static Dictionary<Type, StandardColouredBlock.BlockStyle> UseStyles = null;

        private void ApplyColorSet(Nomino bg, int Level)
        {
            if (bg == null) return;

            if (UseStyles == null) UseStyles = GetBlockStyleLookup(new Type[] {typeof(Tetromino_I), typeof(Tetromino_J), typeof(Tetromino_L), typeof(Tetromino_O), typeof(Tetromino_S), typeof(Tetromino_T), typeof(Tetromino_Z), typeof(Tetromino_Y) });
            StandardColouredBlock.BlockStyle applystyle = _Style;
            if (UseStyles.ContainsKey(bg.GetType())) applystyle = UseStyles[bg.GetType()];

            foreach (var iterate in bg)
            {
                if (iterate.Block is StandardColouredBlock)
                {
                    StandardColouredBlock bl = iterate.Block as StandardColouredBlock;


                    bl.DisplayStyle = applystyle; //TetrisGame.Choose(new StandardColouredBlock.BlockStyle[] { StandardColouredBlock.BlockStyle.Style_HardBevel, StandardColouredBlock.BlockStyle.Style_CloudBevel, StandardColouredBlock.BlockStyle.Style_Shine });
                    Color useColor = GetStandardColor(bg, Level);
                    bl.BlockColor = bl.InnerColor = useColor;
                    /*QColorMatrix qc = new QColorMatrix();
                    qc.RotateHue(Level * 50);
                    
                    useColor = HSLColor.RotateHue(useColor, Level * 50);
                    bl.BlockColor = bl.InnerColor = useColor;
                   */
                }
            }
        }
    }

    //Cyan I
    //Yellow O
    //Purple T
    //Green S
    //Red Z
    //Blue J
    //Orange L
}