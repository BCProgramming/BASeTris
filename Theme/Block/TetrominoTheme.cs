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

        private Color GetStandardColor(BlockGroup source, int Level)
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