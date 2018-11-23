using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.AssetManager;
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
    public class NESTetrominoTheme : TetrominoTheme
    {
        public NESTetrominoTheme()
        {
        }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
        {
            if(DarkImage==null)
            {
                DarkImage = new Bitmap(250, 500);
                using (Graphics drawdark = Graphics.FromImage(DarkImage))
                {
                    drawdark.Clear(Color.FromArgb(10,10,10));
                }
            }
            return new PlayFieldBackgroundInfo(DarkImage,Color.Transparent);
        }

        public override void ApplyTheme(BlockGroup Group, TetrisField Field)
        {
            Color[] useColorSet;
            Color[][] ChooseColorSets = new Color[][] {Level0Colors, Level1Colors, Level2Colors, Level3Colors, Level4Colors, Level5Colors, Level6Colors, Level7Colors, Level8Colors, Level9Colors};
            long CurrLevel = (Field == null) ? 0 : (Field.LineCount / 10);
            int ColorSet = (int) (CurrLevel) % ChooseColorSets.Length;
            useColorSet = ChooseColorSets[ColorSet];
            ApplyColorSet(Group, useColorSet);
        }

        private void ApplyColorSet(BlockGroup bg, Color[] set)
        {
            foreach (var iterate in bg)
            {
                
                Color[] Hollow = new Color[] {set[0], Color.MintCream};
                Color[] Dark = new Color[] {set[0], set[0]};
                Color[] Light = new Color[] {set[1], set[1]};
                Color[] selected;
                if (bg is Tetromino_I || bg is Tetromino_T || bg is Tetromino_O)
                    selected = Hollow;
                else if (bg is Tetromino_J || bg is Tetromino_Z)
                    selected = Dark;
                else if (bg is Tetromino)
                    selected = Light;
                else
                    selected = TetrisGame.Choose(new Color[][] { Hollow, Dark, Light });
                if (iterate.Block is StandardColouredBlock)
                {
                    var coloured = (StandardColouredBlock) iterate.Block;
                    coloured.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Gummy;
                    coloured.BlockColor = selected[0];
                    coloured.InnerColor = selected[1];
                }
            }
        }
      
        public static Color[] Level0Colors = new Color[] {Color.Blue, Color.DeepSkyBlue};
        public static Color[] Level1Colors = new Color[] {Color.Green, Color.GreenYellow};
        public static Color[] Level2Colors = new Color[] {Color.Purple, Color.Magenta};
        public static Color[] Level3Colors = new Color[] {Color.Blue, Color.GreenYellow};
        public static Color[] Level4Colors = new Color[] {Color.MediumVioletRed, Color.Aquamarine};
        public static Color[] Level5Colors = new Color[] {Color.Aquamarine, Color.DeepSkyBlue};
        public static Color[] Level6Colors = new Color[] {Color.Red, Color.SlateGray};
        public static Color[] Level7Colors = new Color[] {Color.Indigo, Color.Brown};
        public static Color[] Level8Colors = new Color[] {Color.DarkBlue, Color.Red};

        public static Color[] Level9Colors = new Color[] {Color.OrangeRed, Color.Orange};
        //Level 0 style:

        public static Color[] AllThemeColors = new Color[] {Color.Blue, Color.DeepSkyBlue, Color.Green, Color.GreenYellow, Color.Purple, Color.Magenta, Color.MediumVioletRed, Color.Aquamarine, Color.Red, Color.SlateGray, Color.Indigo, Color.DarkBlue, Color.Orange, Color.OrangeRed};
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

    /// <summary>
    /// Theme which has tetrominoes be outlined rather than draw as distinct blocks.
    /// </summary>
    public class OutlinedTetrominoTheme : TetrominoTheme
    {
        [Flags]
        public enum BlockOutlines
        {
            Outline_Top = 1,
            Outline_Left = 2,
            Outline_Bottom=4,
            Outline_Right = 8,
            Square_Top_Left = 16,
            Square_Top_Right = 32,
            Square_Bottom_Right = 64,
            Square_Bottom_Left = 128
        }


        private Dictionary<String, Image> StoredImages = new Dictionary<String, Image>();
        Size StandardDrawSize = new Size(125,125);
        Color StandardDrawColor = Color.Red;
        Pen OutlinePen = new Pen(Color.Black, 10);
        float PenWidth = 10;
        private String getCacheKey(BlockOutlines outline, Color BaseColor)
        {
            return "(" + BaseColor.R + "," + BaseColor.G + "," + BaseColor.B + ")-" + ((int)outline).ToString();
        }
        private Image GetOutlinedImage(BlockOutlines outline,Color BaseColor)
        {
            string sKey = getCacheKey(outline, BaseColor);
            if(!StoredImages.ContainsKey(sKey))
            {
                Bitmap BuildImage = new Bitmap(StandardDrawSize.Width,StandardDrawSize.Height);
                using (Graphics g = Graphics.FromImage(BuildImage))
                {
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.Clear(BaseColor);
                }
                var ResultImage = DrawOutline(BuildImage, outline, OutlinePen);
                StoredImages.Add(sKey,ResultImage);
            }
            return StoredImages[sKey];
        }
        private Image DrawOutline(Image Target, BlockOutlines pOutlines, Pen pPen)
        {
            Bitmap BuildResult = new Bitmap(Target);
            using (Graphics g = Graphics.FromImage(BuildResult))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                if (pOutlines.HasFlag(BlockOutlines.Outline_Top))
                {
                    g.DrawLine(pPen, 0, 0, BuildResult.Width, 0);
                }

                if (pOutlines.HasFlag(BlockOutlines.Outline_Bottom))
                {
                    g.DrawLine(pPen, 0,BuildResult.Height, BuildResult.Width, BuildResult.Height);
                }
                if (pOutlines.HasFlag(BlockOutlines.Outline_Left))
                {
                    g.DrawLine(pPen, 0, 0, 0, BuildResult.Height);
                }
                if (pOutlines.HasFlag(BlockOutlines.Outline_Right))
                {
                    g.DrawLine(pPen, BuildResult.Width, 0, BuildResult.Width, BuildResult.Height);
                }
                //TODO: handle drawing dotted corners...

                if(pOutlines.HasFlag(BlockOutlines.Square_Top_Left))
                {
                    g.FillRectangle(pPen.Brush,new RectangleF(0,0,PenWidth,PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Top_Right))
                {
                    g.FillRectangle(pPen.Brush,new RectangleF(Target.Width-PenWidth,0,PenWidth,PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Bottom_Left))
                {
                    g.FillRectangle(pPen.Brush, new RectangleF(0, Target.Height-PenWidth, PenWidth, PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Bottom_Right))
                {
                    g.FillRectangle(pPen.Brush, new RectangleF(Target.Width - PenWidth, Target.Height - PenWidth, PenWidth, PenWidth));
                }
            }
            return BuildResult;
        }

        public OutlinedTetrominoTheme()
        {

        }
        public override void ApplyTheme(BlockGroup Group, TetrisField Field)
        {
            int CurrLevel = Field == null ? 0 : (int)(Field.LineCount / 10);
            if (Group is Tetromino_I)
                ApplyTheme_I(Group, CurrLevel);
            else if (Group is Tetromino_J)
                ApplyTheme_J(Group, CurrLevel);
            else if (Group is Tetromino_L)
                ApplyTheme_L(Group, CurrLevel);
            else if (Group is Tetromino_O)
                ApplyTheme_O(Group, CurrLevel);
            else if (Group is Tetromino_S)
                ApplyTheme_S(Group, CurrLevel);
            else if (Group is Tetromino_Z)
                ApplyTheme_Z(Group, CurrLevel);
            else if (Group is Tetromino_T)
                ApplyTheme_T(Group, CurrLevel);
            else
                ApplyImages(Group,new Image[]{GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Right | BlockOutlines.Outline_Left,GetLevelColor(CurrLevel))});
                
        }
        private Color[] ColorArray = null;
        private Color GetLevelColor(int Level)
        {
            if (ColorArray == null)
            {
                var BaseColor = Color.Red;
                int ColorCount = 10;
                double Partitions = 240d / (double)ColorCount;
                List<Color> BuildColors = new List<Color>();
                for (int i = 0; i < ColorCount - 1; i++)
                {
                    BuildColors.Add(HSLColor.RotateHue(BaseColor, (int)(i * Partitions)));
                }
                ColorArray = BuildColors.ToArray();
            }
            return ColorArray[Level % (ColorArray.Length - 1)];


        }

        private void ApplyImages(BlockGroup Target,Image[] BlockImages)
        {
            var BlockData = Target.GetBlockData();
            
            for(int i=0;i<BlockData.Count;i++)
            {
                
                if (BlockData[i].Block is StandardColouredBlock)
                {
                    ((StandardColouredBlock)BlockData[i].Block).DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    ((StandardColouredBlock)BlockData[i].Block)._RotationImages = GetImageRotations(BlockImages[i]);
                }
            }
        }

        protected void ApplyTheme_I(BlockGroup Target,int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top|BlockOutlines.Outline_Right ,baseColor)
            });
            
        }
        protected void ApplyTheme_J(BlockGroup Target,int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Right | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom | BlockOutlines.Square_Top_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Bottom|BlockOutlines.Outline_Right ,baseColor)
            });
        }

        protected void ApplyTheme_L(BlockGroup Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Right | BlockOutlines.Outline_Bottom |BlockOutlines.Square_Top_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left|BlockOutlines.Outline_Right ,baseColor)
            });
        }
        protected void ApplyTheme_O(BlockGroup Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Right,baseColor)
            });
        }

        protected void ApplyTheme_Z(BlockGroup Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Right|BlockOutlines.Square_Bottom_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }
        protected void ApplyTheme_S(BlockGroup Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right |BlockOutlines.Square_Top_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }

        protected void ApplyTheme_T(BlockGroup Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom|BlockOutlines.Square_Bottom_Right | BlockOutlines.Square_Bottom_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Top | BlockOutlines.Outline_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }

        PlayFieldBackgroundInfo ColoredBG = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
        {
            if (ColoredBG == null) ColoredBG = GetColoredBackground(Color.AntiqueWhite, null);

            return ColoredBG;
        }
    }


    public class GameBoyTetrominoTheme : TetrominoTheme
    {
        static readonly Size ImageSize;
        static GameBoyTetrominoTheme() 
        {
            I_Right_Cap = TetrisGame.Imageman.getLoadedImage("mottle_right_cap", 0.25f);
            I_Left_Cap = TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap", 0.25f);
            I_Horizontal = TetrisGame.Imageman.getLoadedImage("mottle_horizontal", 0.25f);
            Solid_Square = TetrisGame.Imageman.getLoadedImage("standard_square", 0.25f);
            Dotted_Dark = TetrisGame.Imageman.getLoadedImage("dark_dotted", 0.25f);
            Dotted_Light = TetrisGame.Imageman.getLoadedImage("light_dotted", 0.25f);
            Fat_Dotted_Light = TetrisGame.Imageman.getLoadedImage("lighter_big_dotted", 0.25f);
            Inset_Bevel = TetrisGame.Imageman.getLoadedImage("solid_beveled", 0.25f);
            ImageSize = I_Right_Cap.Size;



        }
        private Bitmap LightImage = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
        {
            if (LightImage == null)
            {
                LightImage = new Bitmap(250, 500);
                using (Graphics drawdark = Graphics.FromImage(LightImage))
                {
                    drawdark.Clear(Color.PeachPuff);
                }
            }
            return new PlayFieldBackgroundInfo(LightImage, Color.Transparent);
        }

        protected Image GetRightCap(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Right_Cap, "Right_Cap", LevelColor);
        }
        protected Image GetLeftCap(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Left_Cap, "Left_Cap", LevelColor);
        }
        protected Image GetHorizontal(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Horizontal, "Horizontal", LevelColor);
        }
        protected Image GetSolidSquare(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Solid_Square, "Solid_Square", LevelColor);
        }
        protected Image GetDottedDark(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Dark, "Dotted_Dark", LevelColor);
        }
        protected Image GetDottedLight(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Light, "Dotted_Light", LevelColor);
        }
        protected Image GetInsetBevel(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Inset_Bevel, "Inset_Bevel", LevelColor);
        }
        protected Image GetFatDotted(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Fat_Dotted_Light, "Fat_Dotted", LevelColor);
        }
        protected Image GetCached(Image Original,String pKey,Color pColor)
        {
            var found = GetCachedImage(pKey, ImageSize, pColor);
            if(found==null)
            {
                Image Recolored = StandardColouredBlock.RecolorImage(Original, pColor);
                return AddCachedImage(pKey, ImageSize, pColor, Recolored);
            }
            else
            {
                return found;
            }
        }
        public GameBoyTetrominoTheme()
        {
            
        }
        private Color[] ColorArray = null;
        private Color GetLevelColor(int Level)
        {
            if (ColorArray == null)
            {
                var BaseColor = Color.OliveDrab;
                int ColorCount = 10;
                double Partitions = 240d / (double)ColorCount;
                List<Color> BuildColors = new List<Color>();
                for (int i = 0; i < ColorCount - 1; i++)
                {
                    BuildColors.Add(HSLColor.RotateHue(BaseColor, (int)(i * Partitions)));
                }
                ColorArray = BuildColors.ToArray();
            }
            return ColorArray[Level % (ColorArray.Length - 1)];


        }
        private static Image Solid_Square;
        private static Image Dotted_Light;
        private static Image Dotted_Dark;
        private static Image Fat_Dotted_Light;
        private static Image Inset_Bevel;
        private static Image I_Right_Cap;
        private static Image I_Horizontal;
        private static Image I_Left_Cap;

        public override void ApplyTheme(BlockGroup Group, TetrisField Field)
        {
            int CurrLevel = Field == null ? 0 : (int)(Field.LineCount / 10);

            if(Group is Tetromino_L)
            {
                Apply_L(Group as Tetromino_L,Field,CurrLevel);
            }
            else if(Group is Tetromino_J)
            {
                Apply_J(Group as Tetromino_J, Field,CurrLevel);
            }
            else if(Group is Tetromino_I)
            {
                Apply_I(Group as Tetromino_I, Field,CurrLevel);
            }
            else if(Group is Tetromino_O)
            {
                Apply_O(Group as Tetromino_O, Field,CurrLevel);
            }
            else if(Group is Tetromino_S)
            {
                Apply_S(Group as Tetromino_S, Field,CurrLevel);
            }
            else if(Group is Tetromino_Z)
            {
                Apply_Z(Group as Tetromino_Z, Field,CurrLevel);
            }
            else if(Group is Tetromino_T)
            {
                Apply_T(Group as Tetromino_T, Field,CurrLevel);
            }
            else
            {
                foreach (var blockcheck in Group)
                {
                    if (blockcheck.Block is StandardColouredBlock)
                    {
                        StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        TetrisGame.Choose(new Image[] { GetSolidSquare(CurrLevel), GetDottedLight(CurrLevel), GetDottedDark(CurrLevel), GetFatDotted(CurrLevel), GetInsetBevel(CurrLevel) });
                        scb._RotationImages = new Image[] { GetInsetBevel(CurrLevel) };
                        //scb.BaseImageKey = Solid_Square; 
                    }
                }
            }
        }

        public void Apply_L(Tetromino_L Group, TetrisField Field,int CurrLevel)
        {
            //L block is a solid darker colour.
            
            foreach(var blockcheck in Group)
            {
                if(blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetSolidSquare(CurrLevel) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_J(Tetromino_J Group, TetrisField Field,int CurrLevel)
        {
            //darker outline with a middle white square.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedLight(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }

        }
        static float ReductionFactor = 0.5f;
        public void Apply_I(Tetromino_I Group, TetrisField Field,int CurrLevel)
        {
            //mottled. need to set rotation images as well.

            //we have four indices:
            //index one is left side
            //index two is left middle
            //index three is right middle
            //index four is right side.
            var BlockData = Group.GetBlockData();
            var LeftSide = BlockData[0];
            var LeftMiddle = BlockData[1];
            var RightMiddle = BlockData[2];
            var RightSide = BlockData[3];
                    
            if(LeftSide.Block is StandardColouredBlock)
            {
            var scb = (LeftSide.Block as StandardColouredBlock);
            scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetLeftCap(CurrLevel));
            //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT90:mottle_right_cap",ReductionFactor),
            //        TetrisGame.Imageman.getLoadedImage("FLIPXROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT270:mottle_right_cap",ReductionFactor) };
            }
            Image i;
            if(LeftMiddle.Block is StandardColouredBlock)
            {
                var scb = (LeftMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetHorizontal(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }

            if (RightMiddle.Block is StandardColouredBlock)
            {
                var scb = (RightMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetHorizontal(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }
            if (RightSide.Block is StandardColouredBlock)
            {
                var scb = (RightSide.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetRightCap(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_right_cap",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_right_cap",ReductionFactor) };
            }

            
            
        }
        public void Apply_Z(Tetromino_Z Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, darker colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedDark(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_O(Tetromino_O Group, TetrisField Field,int CurrLevel)
        {
            //white, inset block
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] {GetFatDotted(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_S(Tetromino_S Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, light colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedLight(CurrLevel) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_T(Tetromino_T Group, TetrisField Field,int CurrLevel)
        {
            //inset bevel
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetInsetBevel(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }

    }
}