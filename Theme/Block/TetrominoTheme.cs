using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            Image Rotate90 = new Bitmap(Source);
            Image Rotate180 = new Bitmap(Source);
            Image Rotate270 = new Bitmap(Source);



            Rotate90.RotateFlip(RotateFlipType.Rotate90FlipNone);
            Rotate180.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Rotate270.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return new Image[] { Source, Rotate90, Rotate180, Rotate270 };
        }
    }

    public class NESTetrominoTheme : TetrominoTheme
    {
        public NESTetrominoTheme()
        {
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
                else
                    selected = Light;

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

    public class GameBoyTetrominoTheme : TetrominoTheme
    {
        static readonly Size ImageSize;
        static GameBoyTetrominoTheme() 
        {
            I_Right_Cap = TetrisGame.Imageman.getLoadedImage("mottle_right_cap", 0.25f);
            I_Left_Cap = TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap", 0.25f);
            I_Horizontal = TetrisGame.Imageman.getLoadedImage("mottle_horizontal", 0.25f);
            Solid_Square = TetrisGame.Imageman.getLoadedImage("standard_square", 0.25f);
            Dotted_Dark = Dotted_Light = Fat_Dotted_Light = Inset_Bevel = Solid_Square;
            ImageSize = I_Right_Cap.Size;



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