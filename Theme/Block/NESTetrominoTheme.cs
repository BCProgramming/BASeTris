using System.Drawing;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
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
}