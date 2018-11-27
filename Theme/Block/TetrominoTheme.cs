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
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public abstract class TetrominoTheme
    {
        public abstract void ApplyTheme(BlockGroup Group, TetrisField Field);
        public abstract PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field);
        
        protected Dictionary<String, Image> _Cache = new Dictionary<string, Image>();

        private Image AddCachedImage(String Key,Image pImage)
        {
            if (_Cache.ContainsKey(Key)) _Cache[Key] = pImage;
            else
            {
                _Cache.Add(Key,pImage);
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
        protected Image AddCachedImage(String pKey,Size pSize,Color pColor,Image pImage)
        {
            String pBuildKey = BuildCacheKey(pKey, pSize, pColor);
            return AddCachedImage(pBuildKey, pImage);
        }
        protected Image GetCachedImage(String pKey,Size pSize,Color pColor)
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
        static Color DefaultTint = Color.Transparent;
        protected PlayFieldBackgroundInfo GetColoredBackground(Color MainColor,Color? TintColor = null)
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
        public PlayFieldBackgroundInfo(Image pBackgroundImage,Color pTintColor)
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

    public class StandardTetrominoTheme : TetrominoTheme
    {
        StandardColouredBlock.BlockStyle _Style;

        public StandardColouredBlock.BlockStyle BlockStyle
        {
            get { return _Style; }
            set { _Style = value; }
        }

        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background",0.5f],Color.Transparent);
        }

        public StandardTetrominoTheme(StandardColouredBlock.BlockStyle pBlockStyle)
        {
            _Style = pBlockStyle;
        }

        public override void ApplyTheme(BlockGroup Group, TetrisField Field)
        {
            int CurrLevel = Field == null ? 0 : (int) (Field.LineCount / 10);
            ApplyColorSet(Group, CurrLevel);
        }

        private StandardColouredBlock.BlockStyle[] usabletypes = new StandardColouredBlock.BlockStyle[] {StandardColouredBlock.BlockStyle.Style_Chisel, StandardColouredBlock.BlockStyle.Style_CloudBevel, StandardColouredBlock.BlockStyle.Style_HardBevel, StandardColouredBlock.BlockStyle.Style_Shine};

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

        public static Color GetStandardColor(BlockGroup source, int Level)
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

        private void ApplyColorSet(BlockGroup bg, int Level)
        {
            if (bg == null) return;

            if (UseStyles == null) UseStyles = GetBlockStyleLookup(new Type[] {typeof(Tetromino_I), typeof(Tetromino_J), typeof(Tetromino_L), typeof(Tetromino_O), typeof(Tetromino_S), typeof(Tetromino_T), typeof(Tetromino_Z)});
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