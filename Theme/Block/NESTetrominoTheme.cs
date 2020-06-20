using System.Collections.Generic;
using System.Drawing;
using BASeTris.Rendering.Adapters;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using SkiaSharp;

namespace BASeTris.Theme.Block
{

    public class NESTetrominoTheme : CustomPixelTheme<NESTetrominoTheme.BCT, NESTetrominoTheme.NESBlockTypes>
    {

        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
        {
            if (DarkImage == null)
            {
                DarkImage = new Bitmap(250, 500);
                using (Graphics drawdark = Graphics.FromImage(DarkImage))
                {
                    drawdark.Clear(Color.FromArgb(10, 10, 10));
                }
            }
            return new PlayFieldBackgroundInfo(DarkImage, Color.Transparent);
        }
        SKColor[][] LevelColorSets = new SKColor[][] { Level0Colors, Level1Colors, Level2Colors, Level3Colors, Level4Colors, Level5Colors, Level6Colors, Level7Colors, Level8Colors, Level9Colors };

        /* private void ApplyColorSet(Nomino bg, Color[] set)
         {
             foreach (var iterate in bg)
             {

                 Color[] Hollow = new Color[] { set[0], SKColors.White };
                 Color[] Dark = new Color[] { set[0], set[0] };
                 Color[] Light = new Color[] { set[1], set[1] };
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
                     var coloured = (StandardColouredBlock)iterate.Block;
                     coloured.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Pixeled;

                     coloured.BlockColor = selected[0];
                     coloured.InnerColor = selected[1];
                     if (coloured.InnerColor != coloured.BlockColor)
                     {
                         coloured.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Pixeled_Outline;
                     }
                 }
             }
         }*/

        public override SKPointI GetBlockSize(TetrisField field, NESBlockTypes BlockType)
        {
            return new SKPointI(9, 9);
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, NESBlockTypes BlockType, BCT PixelType)
        {
            int LevelNum = field.Level;
            int LevelIndex = MathHelper.mod(LevelNum, AllLevelColors.Length);
            switch (PixelType)
            {
                case BCT.Transparent:
                    return SKColors.Transparent;
                case BCT.Glint:
                    return SKColors.White;
                case BCT.Base_Dark:
                    return LevelColorSets[LevelIndex][0];
                case BCT.Base_Light:
                    return LevelColorSets[LevelIndex][1];
                default:
                    return LevelColorSets[LevelIndex][0];
            }



        }

        public override Dictionary<NESBlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            return new Dictionary<NESBlockTypes, BCT[][]>
            {
                { NESBlockTypes.Darker,DarkerBlock },
                {NESBlockTypes.Lighter,LighterBlock },
                { NESBlockTypes.Boxed,CenterWhiteBlock }
            };
        }

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            return CustomPixelTheme<BCT, NESBlockTypes>.BlockFlags.Static;
        }

        public override NESBlockTypes GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            var bg = group;
            if (bg is Tetromino_I || bg is Tetromino_T || bg is Tetromino_O)
            {
                return NESBlockTypes.Boxed;
            }
            else if (bg is Tetromino_J || bg is Tetromino_Z)
                return NESBlockTypes.Darker;
            else if (bg is Tetromino)
                return NESBlockTypes.Lighter;
            else
                return TetrisGame.Choose(new NESBlockTypes[] { NESBlockTypes.Boxed, NESBlockTypes.Darker, NESBlockTypes.Lighter });



        }

        public override NESBlockTypes[] PossibleBlockTypes()
        {
            return new NESBlockTypes[] { NESBlockTypes.Darker, NESBlockTypes.Lighter, NESBlockTypes.Boxed };
        }

        public enum BCT
        {
            Transparent,
            Glint,
            Base_Dark,
            Base_Light
        }
        public enum NESBlockTypes
        {
            Darker,
            Lighter,
            Boxed
        }

        public static BCT[][] DarkerBlock = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark}
        };

        public static BCT[][] LighterBlock = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Glint, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light}
        };

        public static BCT[][] CenterWhiteBlock = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark}
        };


        private static Dictionary<NESBlockTypes, BCT[][]> NESBlockMapLookup = new Dictionary<NESBlockTypes, BCT[][]>()
        {
            { NESBlockTypes.Darker,DarkerBlock},
            { NESBlockTypes.Lighter,LighterBlock},
            {NESBlockTypes.Boxed,CenterWhiteBlock }

        };
        private static SKImageInfo blockinfo = new SKImageInfo(8, 8, SKColorType.Rgb888x, SKAlphaType.Opaque);
        private static Dictionary<int, Dictionary<NESBlockTypes, SKBitmap>> SkiaImageCache = new Dictionary<int, Dictionary<NESBlockTypes, SKBitmap>>();
        private static Dictionary<int, Dictionary<NESBlockTypes, Image>> GDIPImageCache = new Dictionary<int, Dictionary<NESBlockTypes, Image>>();




        public static SKColor[][] AllLevelColors = new SKColor[][]
        {
                Level0Colors,
                Level1Colors,
                Level2Colors,
                Level3Colors,
                Level4Colors,
                Level5Colors,
                Level6Colors,
                Level7Colors,
                Level8Colors,
                Level9Colors
        };

        public static SKColor[] Level0Colors = new SKColor[] { SKColors.Blue, SKColors.DeepSkyBlue };
        public static SKColor[] Level1Colors = new SKColor[] { SKColors.Green, SKColors.GreenYellow };
        public static SKColor[] Level2Colors = new SKColor[] { SKColors.Purple, SKColors.Magenta };
        public static SKColor[] Level3Colors = new SKColor[] { SKColors.Blue, SKColors.GreenYellow };
        public static SKColor[] Level4Colors = new SKColor[] { SKColors.MediumVioletRed, SKColors.Aquamarine };
        public static SKColor[] Level5Colors = new SKColor[] { SKColors.Aquamarine, SKColors.DeepSkyBlue };
        public static SKColor[] Level6Colors = new SKColor[] { SKColors.Red, SKColors.SlateGray };
        public static SKColor[] Level7Colors = new SKColor[] { SKColors.Indigo, SKColors.Brown };
        public static SKColor[] Level8Colors = new SKColor[] { SKColors.DarkBlue, SKColors.Red };

        public static SKColor[] Level9Colors = new SKColor[] { SKColors.OrangeRed, SKColors.Orange };
        //Level 0 style:

        public static SKColor[] AllThemeColors = new SKColor[] { SKColors.Blue, SKColors.DeepSkyBlue, SKColors.Green, SKColors.GreenYellow, SKColors.Purple, SKColors.Magenta, SKColors.MediumVioletRed, SKColors.Aquamarine, SKColors.Red, SKColors.SlateGray, SKColors.Indigo, SKColors.DarkBlue, SKColors.Orange, SKColors.OrangeRed };
    }

}