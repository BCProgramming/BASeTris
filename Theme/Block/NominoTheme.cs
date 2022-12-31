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

namespace BASeTris
{
    //placed here for reference:
    //possible theme idea: a theme which itself is a preference-based setup.
    //Instead of defining themes of it's own, it can use existing Theme types and be configured to set certain tetrominos to specific themes.
    public abstract class NominoTheme
    {

     


        public enum ThemeApplicationReason
        {
            Normal,
            NewNomino,
            Theme_Changed,
            FieldSet
        }
        public virtual ThemeImageProvider ThemeProvider { get; set; }
        public abstract String Name { get; }
        public virtual bool IsAnimated(NominoBlock block)
        {
            return false;
        }
        public virtual String GetNominoKey(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return GetNominoTypeKey(Group.GetType(), GameHandler, Field);
        }
        public virtual String GetNominoTypeKey(Type src, IGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return src.FullName;
        }
        public abstract void ApplyTheme(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);
        public abstract void ApplyRandom(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field);
        public abstract PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IGameCustomizationHandler GameHandler);

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

    public class StandardTetrominoTheme : NominoTheme
    {
        StandardColouredBlock.BlockStyle _Style;

        public override String Name { get { return "Standard";} }
        public StandardColouredBlock.BlockStyle BlockStyle
        {
            get { return _Style; }
            set { _Style = value; }
        }

        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IGameCustomizationHandler GameHandler)
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

        public override void ApplyRandom(Nomino Group, IGameCustomizationHandler GameHandler,TetrisField Field)
        {
            int useLevel = TetrisGame.rgen.Next(50);
            ApplyColorSet(Group,useLevel);
        }

        public override void ApplyTheme(Nomino Group, IGameCustomizationHandler GameHandler,TetrisField Field, ThemeApplicationReason Reason)
        {
            var LineCount = (GameHandler.Statistics is TetrisStatistics ts) ? ts.LineCount : 0;
            int CurrLevel = Field == null ? 0 : (int) (LineCount / 10);
            ApplyColorSet(Group, CurrLevel);
        }

        private StandardColouredBlock.BlockStyle[] usabletypes = new StandardColouredBlock.BlockStyle[] {StandardColouredBlock.BlockStyle.Style_Chisel, StandardColouredBlock.BlockStyle.Style_CloudBevel, StandardColouredBlock.BlockStyle.Style_HardBevel, StandardColouredBlock.BlockStyle.Style_Shine,StandardColouredBlock.BlockStyle.Style_Mottled};

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