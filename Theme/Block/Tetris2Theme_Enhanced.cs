using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    //this needs the Tetris2 Theme to be completed, we need the basic set of block types to be implemented as well.
    
    [HandlerTheme("Tetris 2 NES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler))]
    [ThemeDescription("Tetris 2 from the NES")]
    public class Tetris2Theme_Standard : Tetris2Theme_Enhanced

    {
        public override String Name => "Tetris 2 (8-bit)";
        public Tetris2Theme_Standard()
        {
            base.UseEnhancedImages = false;
        }
    }

    [HandlerTheme("Tetris 2 Redux", typeof(Tetris2Handler), typeof(DrMarioHandler),typeof(StandardTetrisHandler))]
    [ThemeDescription("A redesigned variant of Tetris 2 Blocks")]
    public class Tetris2Theme_Enhanced : CustomPixelTheme<Tetris2Theme_Enhanced.BCT, Tetris2Theme_Enhanced.BlockTypes>
    {



        public override string Name => "Tetris 2";



        static SKColor BlueColor = new SKColor(60, 188, 252);

        public static Dictionary<BCT, SKColor> CreateColorSet(SKColor SourceColor)
        {
            return new Dictionary<BCT, SKColor>()
            {
                { BCT.Transparent,SKColors.Transparent },
                {BCT.Black,SKColors.Black },
                {BCT.Primary,SourceColor },
                {BCT.Accent,RenderHelpers.InvertColor(SourceColor) },
                {BCT.Accent2,RenderHelpers.RotateHue(SourceColor,0.3f) },
                {BCT.Enhanced_1,RenderHelpers.MatchHue(SourceColor,new SKColor(205,205,0)) },
            {BCT.Enhanced_2,RenderHelpers.MatchHue(SourceColor,new SKColor(157,157,0)) },
            {BCT.Enhanced_3,RenderHelpers.MatchHue(SourceColor,new SKColor(116,116,0)) },
            {BCT.Enhanced_4,RenderHelpers.MatchHue(SourceColor,new SKColor(249,249,3)) },
            {BCT.Enhanced_5,RenderHelpers.MatchHue(SourceColor,new SKColor(67,67,1)) },
            {BCT.Enhanced_6,RenderHelpers.MatchHue(SourceColor,new SKColor(255,255,18)) },
            {BCT.Enhanced_7,RenderHelpers.MatchHue(SourceColor,new SKColor(255,255,45)) },
            {BCT.Enhanced_8,RenderHelpers.MatchHue(SourceColor,new SKColor(255,255,75)) },
            {BCT.Enhanced_9,RenderHelpers.MatchHue(SourceColor,new SKColor(255,255,103)) },
            {BCT.Enhanced_10,RenderHelpers.MatchHue(SourceColor,new SKColor(88,88,88)) },
            {BCT.Enhanced_11,RenderHelpers.MatchHue(SourceColor,new SKColor(1,1,1)) }
            };
        }

        public static Dictionary<BCT, SKColor> YellowColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,BlueColor  },
            {BCT.Accent2,SKColors.Red }

        };

        public static Dictionary<BCT, SKColor> EnhancedYellowColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,BlueColor  },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(205,205,0) },
            {BCT.Enhanced_2,new SKColor(157,157,0) },
            {BCT.Enhanced_3,new SKColor(116,116,0) },
            {BCT.Enhanced_4,new SKColor(249,249,3) },
            {BCT.Enhanced_5,new SKColor(67,67,1) },
            {BCT.Enhanced_6,new SKColor(255,255,18) },
            {BCT.Enhanced_7,new SKColor(255,255,45) },
            {BCT.Enhanced_8,new SKColor(255,255,75) },
            {BCT.Enhanced_9,new SKColor(255,255,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };


        public static Dictionary<BCT, SKColor> RedColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Red },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,BlueColor  }

        };
      
        public static Dictionary<BCT, SKColor> EnhancedRedColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Red  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(205,0,0) },
            {BCT.Enhanced_2,new SKColor(157,0,0) },
            {BCT.Enhanced_3,new SKColor(116,0,0) },
            {BCT.Enhanced_4,new SKColor(249,3,3) },
            {BCT.Enhanced_5,new SKColor(67,1,1) },
            {BCT.Enhanced_6,new SKColor(255,18,18) },
            {BCT.Enhanced_7,new SKColor(255,45,45) },
            {BCT.Enhanced_8,new SKColor(255,75,75) },
            {BCT.Enhanced_9,new SKColor(255,103,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        public static Dictionary<BCT, SKColor> BlueColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,BlueColor  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red }

        };
        public static Dictionary<BCT, SKColor> EnhancedBlueColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,BlueColor  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(0,204,205) },
            {BCT.Enhanced_2,new SKColor(0,156,157) },
            {BCT.Enhanced_3,new SKColor(0,116,116) },
            {BCT.Enhanced_4,new SKColor(3,249,249) },
            {BCT.Enhanced_5,new SKColor(1,67,67) },
            {BCT.Enhanced_6,new SKColor(18,255,255) },
            {BCT.Enhanced_7,new SKColor(45,255,255) },
            {BCT.Enhanced_8,new SKColor(75,255,255) },
            {BCT.Enhanced_9,new SKColor(103,255,255) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };
        //weird idea, Green, Orange, and Magenta colours.
        public static Dictionary<BCT, SKColor> GreenColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Green  },
            {BCT.Accent,SKColors.GreenYellow },
            {BCT.Accent2,SKColors.DarkMagenta }

        };

        public static Dictionary<BCT, SKColor> EnhancedGreenColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Green  },
            {BCT.Accent,SKColors.GreenYellow },
            {BCT.Accent2,SKColors.DarkMagenta },
            {BCT.Enhanced_1,new SKColor(0,205,0) },
            {BCT.Enhanced_2,new SKColor(0,205,0) },
            {BCT.Enhanced_3,new SKColor(0,116,0) },
            {BCT.Enhanced_4,new SKColor(3,249,3) },
            {BCT.Enhanced_5,new SKColor(1,67,1) },
            {BCT.Enhanced_6,new SKColor(18,255,18) },
            {BCT.Enhanced_7,new SKColor(45,255,45) },
            {BCT.Enhanced_8,new SKColor(75,255,75) },
            {BCT.Enhanced_9,new SKColor(103,255,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        public static Dictionary<BCT, SKColor> OrangeColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Orange  },
            {BCT.Accent,SKColors.PaleGoldenrod },
            {BCT.Accent2,SKColors.LightBlue }

        };

        public static Dictionary<BCT, SKColor> EnhancedOrangeColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Orange  },
            {BCT.Accent,SKColors.PaleGoldenrod },
            {BCT.Accent2,SKColors.LightBlue },
            {BCT.Enhanced_1,new SKColor(205,103,0) },
            {BCT.Enhanced_2,new SKColor(157,75,0) },
            {BCT.Enhanced_3,new SKColor(116,55,0) },
            {BCT.Enhanced_4,new SKColor(249,128,3) },
            {BCT.Enhanced_5,new SKColor(67,30,1) },
            {BCT.Enhanced_6,new SKColor(255,128,18) },
            {BCT.Enhanced_7,new SKColor(255,128,45) },
            {BCT.Enhanced_8,new SKColor(255,128,75) },
            {BCT.Enhanced_9,new SKColor(255,50,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        static Dictionary<BCT, SKColor> MagentaColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Magenta  },
            {BCT.Accent,SKColors.Pink },
            {BCT.Accent2,SKColors.Orchid },


        };
        private static SKColor Gray(SKColor Source)
        {
            var gray = (byte)(.299 * (double)Source.Red + 0.587 * (double)Source.Green + 0.114 * (double)Source.Blue);
            return new SKColor(gray, gray, gray);
        }
        public static Dictionary<BCT, SKColor> EnhancedMagentaColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.DarkGray  },
            {BCT.Accent,SKColors.Gray },
            {BCT.Accent2,SKColors.LightGray },
            {BCT.Enhanced_1,new SKColor(205,0,205) },
            {BCT.Enhanced_2,new SKColor(157,0,157) },
            {BCT.Enhanced_3,new SKColor(116,0,116) },
            {BCT.Enhanced_4,new SKColor(249,3,249) },
            {BCT.Enhanced_5,new SKColor(67,1,67) },
            {BCT.Enhanced_6,new SKColor(255,18,255) },
            {BCT.Enhanced_7,new SKColor(255,45,255) },
            {BCT.Enhanced_8,new SKColor(255,75,255) },
            {BCT.Enhanced_9,new SKColor(255,103,255) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) }

        };


        public static Dictionary<BCT, SKColor> GrayColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Magenta  },
            {BCT.Accent,SKColors.Pink },
            {BCT.Accent2,SKColors.Orchid },


        };

        public static Dictionary<BCT, SKColor> EnhancedGrayColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Magenta  },
            {BCT.Accent,SKColors.Pink },
            {BCT.Accent2,SKColors.Orchid },
            {BCT.Enhanced_1,Gray(new SKColor(205,0,205)) },
            {BCT.Enhanced_2,Gray(new SKColor(157,0,157)) },
            {BCT.Enhanced_3,Gray(new SKColor(116,0,116)) },
            {BCT.Enhanced_4,Gray(new SKColor(249,3,249)) },
            {BCT.Enhanced_5,Gray(new SKColor(67,1,67)) },
            {BCT.Enhanced_6,Gray(new SKColor(255,18,255)) },
            {BCT.Enhanced_7,Gray(new SKColor(255,45,255)) },
            {BCT.Enhanced_8,Gray(new SKColor(255,75,255)) },
            {BCT.Enhanced_9,Gray(new SKColor(255,103,255)) },
            {BCT.Enhanced_10,Gray(new SKColor(88,88,88)) },
            {BCT.Enhanced_11,Gray(new SKColor(1,1,1)) }

        };



        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PopTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pop_Block_Red },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pop_Block_Yellow},
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pop_Block_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pop_Block_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pop_Block_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pop_Block_Green }

        };

        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> NormalTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Normal_Block_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Normal_Block_Yellow},
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Normal_Block_Blue},
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Normal_Block_Orange},
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Normal_Block_Magenta},
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Normal_Block_Green},
        };

        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Fixed_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Fixed_Block_Red  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Fixed_Block_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Fixed_Block_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Fixed_Block_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Fixed_Block_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Fixed_Block_Green }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Shiny_0 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Fixed_Block_Red  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Fixed_Block_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Fixed_Block_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Fixed_Block_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Fixed_Block_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Fixed_Block_Green }
        };

        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Shiny_25 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Shiny_Block_Red_25  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Shiny_Block_Yellow_25 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Shiny_Block_Blue_25 },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Shiny_Block_Orange_25 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Shiny_Block_Magenta_25 },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Shiny_Block_Green_25 }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Shiny_50 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Shiny_Block_Red_50  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Shiny_Block_Yellow_50 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Shiny_Block_Blue_50 },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Shiny_Block_Orange_50 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Shiny_Block_Magenta_50 },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Shiny_Block_Green_50 }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Shiny_75 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Shiny_Block_Red_75  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Shiny_Block_Yellow_75 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Shiny_Block_Blue_75 },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Shiny_Block_Orange_75 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Shiny_Block_Magenta_75},
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Shiny_Block_Green_75 }
        };

        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> Shiny_100 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Shiny_Block_Red_100  },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Shiny_Block_Yellow_100 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Shiny_Block_Blue_100 },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Shiny_Block_Orange_100 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Shiny_Block_Magenta_100},
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Shiny_Block_Green_100 }
        };


        public static BlockTypes[] RedNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Red };
        public static BlockTypes[] YellowNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Yellow };
        public static BlockTypes[] BlueNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Blue };
        public static BlockTypes[] OrangeNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Orange };
        public static BlockTypes[] MagentaNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Magenta };
        public static BlockTypes[] GreenNormalTypes = new BlockTypes[] { BlockTypes.Normal_Block_Green };

        public static BlockTypes[] RedFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Red };
        public static BlockTypes[] YellowFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Yellow };
        public static BlockTypes[] BlueFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Blue };
        public static BlockTypes[] OrangeFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Orange };
        public static BlockTypes[] MagentaFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Magenta };
        public static BlockTypes[] GreenFixedTypes = new BlockTypes[] { BlockTypes.Fixed_Block_Green };


        public static BlockTypes[] RedPopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Red };
        public static BlockTypes[] YellowPopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Yellow };
        public static BlockTypes[] BluePopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Blue };
        public static BlockTypes[] OrangePopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Orange };
        public static BlockTypes[] MagentaPopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Magenta };
        public static BlockTypes[] GreenPopTypes = new BlockTypes[] { BlockTypes.Pop_Block_Green};

        public static BlockTypes[] RedShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Red_25, BlockTypes.Shiny_Block_Red_50, BlockTypes.Shiny_Block_Red_75, BlockTypes.Shiny_Block_Red_100 };
        public static BlockTypes[] YellowShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Yellow_25, BlockTypes.Shiny_Block_Yellow_50, BlockTypes.Shiny_Block_Yellow_75, BlockTypes.Shiny_Block_Yellow_100 };
        public static BlockTypes[] BlueShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Blue_25, BlockTypes.Shiny_Block_Blue_50, BlockTypes.Shiny_Block_Blue_75, BlockTypes.Shiny_Block_Blue_100 };
        public static BlockTypes[] OrangeShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Orange_25, BlockTypes.Shiny_Block_Orange_50, BlockTypes.Shiny_Block_Orange_75, BlockTypes.Shiny_Block_Orange_100 };
        public static BlockTypes[] MagentaShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Magenta_25, BlockTypes.Shiny_Block_Magenta_50, BlockTypes.Shiny_Block_Magenta_75, BlockTypes.Shiny_Block_Magenta_100 };
        public static BlockTypes[] GreenShinyTypes = new BlockTypes[] { BlockTypes.Shiny_Block_Green_25, BlockTypes.Shiny_Block_Green_50, BlockTypes.Shiny_Block_Green_75, BlockTypes.Shiny_Block_Green_100 };


        public static bool IsInSet(BlockTypes test,params BlockTypes[][] typescheck)
        {
            return typescheck.Any((a) => a.Contains(test));
            
        }
        public static bool IsRedColor(BlockTypes test)
        {
            return IsInSet(test, RedNormalTypes, RedFixedTypes, RedShinyTypes,RedPopTypes);
        }
        public static bool IsYellowColor(BlockTypes test)
        {
            return IsInSet(test, YellowNormalTypes, YellowFixedTypes, YellowShinyTypes,YellowPopTypes);
        }
        public static bool IsBlueColor(BlockTypes test)
        {
            return IsInSet(test, BlueNormalTypes, BlueFixedTypes, BlueShinyTypes,BluePopTypes);
        }
        public static bool IsOrangeColor(BlockTypes test)
        {
            return IsInSet(test, OrangeNormalTypes, OrangeFixedTypes, OrangeShinyTypes,GreenPopTypes);
        }
        public static bool IsMagentaColor(BlockTypes test)
        {
            return IsInSet(test, MagentaNormalTypes, MagentaFixedTypes, MagentaShinyTypes,MagentaPopTypes);
        }
        public static bool IsGreenColor(BlockTypes test)
        {
            return IsInSet(test, GreenNormalTypes, GreenFixedTypes, GreenShinyTypes,GreenPopTypes);
        }
        private static BCT[][] GetBCTBitmap(String ImageKey)
        {
            return GetBCTBitmap(ImageKey, ColorMapLookupFunc);
        }

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Static;
        }

        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            if (UseEnhancedImages)
                return new SKPointI(32, 32);
            else return new SKPointI(9, 9);
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb)
            {
                if (lsb.Popping)
                {
                    if (element.Block is LineSeriesPrimaryBlock lsm2)
                    {
                        ;
                    }
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                }
                if (element.Block is LineSeriesPrimaryBlock lsm)
                {
                    if (lsb.Popping)
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                    }
                    const int MaxInterval = 20000;
                    var modded = DateTime.Now.Ticks % MaxInterval;
                    if (modded < MaxInterval * (1 / 5))
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(Shiny_0[lsb.CombiningIndex], true);
                    }
                    else if (modded < MaxInterval * (2 / 5))
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(Shiny_25[lsm.CombiningIndex], true);
                    }
                    else if (modded < MaxInterval * (3 / 5))
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(Shiny_50[lsm.CombiningIndex], true);
                    }
                    else if (modded < MaxInterval * (4 / 5))
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(Shiny_75[lsm.CombiningIndex], true);
                    }
                    else
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(Shiny_100[lsm.CombiningIndex], true);
                    }

                }
                return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(NormalTypes_1[lsb.CombiningIndex]);
                /* else if (group == null || group.Count() == 1)
                 {
                     return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillSingle[lsb.CombiningIndex]);

                 }
                 else if (group.Count() == 2)
                 {
                     int index = group.IndexOf(element);

                     var PillSource = new[] { new[] { PillLeft, PillTop, PillRight, PillBottom }, new[] { PillRight, PillBottom, PillLeft, PillTop } };
                     var rotmod = 0; //NominoElement.sMod(group.GetBlockData()[0].RotationModulo,4); //ideally we'd use this instead of 0 in the index below, but we need to refactor t his theme I think...

                     return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillSource[index][rotmod][lsb.CombiningIndex]);
                     //possibly, should rotmod always be zero here?

                     //if (index == 0) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillLeft[lsb.CombiningIndex]);
                     //else if (index == 1) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillRight[lsb.CombiningIndex]);
                 }*/
            }
            else
            {
                if (group is Tetrominoes.Tetromino_T || group is Tetrominoes.Tetromino_O || group is Tetrominoes.Tetromino_T)
                {
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(BlockTypes.Fixed_Block_Gray);
                }
                
            }
            return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(BlockTypes.Normal_Block_Gray);

        }
        Dictionary<String, Dictionary<BCT, SKColor>> ExtraNominoColourLookup = new Dictionary<string, Dictionary<BCT, SKColor>>();
        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            if (UseEnhancedImages)
                return BitmapIndexEnhanced;
            else return BitmapIndex;
        }

        public override string GetNominoKey(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {
            if (Group is Tetrominoes.Tetromino)
            {
                return base.GetNominoTypeKey(Group.GetType(), GameHandler, Field);

            }



            var gotpoints = NNominoGenerator.GetNominoPoints(Group);
            String sStringRep = NNominoGenerator.StringRepresentation(gotpoints);
            return sStringRep;
        }
        protected bool UseEnhancedImages = true;
        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, BlockTypes BlockType, BCT PixelType)
        {
            if (IsYellowColor(BlockType))
                return UseEnhancedImages ? EnhancedYellowColourSet[PixelType] : YellowColourSet[PixelType];
            else if (IsRedColor(BlockType))
                return UseEnhancedImages ? EnhancedRedColourSet[PixelType] : RedColourSet[PixelType];
            else if (IsBlueColor(BlockType))
                return UseEnhancedImages ? EnhancedBlueColourSet[PixelType] : BlueColourSet[PixelType];
            else if (IsOrangeColor(BlockType))
                return UseEnhancedImages ? EnhancedOrangeColourSet[PixelType] : OrangeColourSet[PixelType];
            else if (IsGreenColor(BlockType))
                return UseEnhancedImages ? EnhancedGreenColourSet[PixelType] : GreenColourSet[PixelType];
            else if (IsMagentaColor(BlockType))
                return UseEnhancedImages ? EnhancedMagentaColourSet[PixelType] : MagentaColourSet[PixelType];
            else
            {
                //if none of the other tests pass, then we might be working with some normal tetris style thing.
                SKColor SelectedColor = SKColors.Gray;
                //cyan I, yellow O, purple T, green S, blue J, red Z and orange L
                if (Element is Tetrominoes.Tetromino_Z) SelectedColor = SKColors.Red;
                else if (Element is Tetrominoes.Tetromino_L) SelectedColor = SKColors.Orange;
                else if (Element is Tetrominoes.Tetromino_J) SelectedColor = SKColors.Blue;
                else if (Element is Tetrominoes.Tetromino_S) SelectedColor = SKColors.Green;
                else if (Element is Tetrominoes.Tetromino_T) SelectedColor = SKColors.Purple;
                else if (Element is Tetrominoes.Tetromino_O) SelectedColor = SKColors.Yellow;
                else if (Element is Tetrominoes.Tetromino_I) SelectedColor = SKColors.Cyan;
                else SelectedColor = RenderHelpers.RandomColor();

                var points = NNominoGenerator.GetNominoPoints(Element);
                String strval = NNominoGenerator.StringRepresentation(points);

                if (!ExtraNominoColourLookup.ContainsKey(strval))
                {
                    String[] othersets = NNominoGenerator.GetOtherRotationStrings(points);
                        
                    var CreatedSet = CreateColorSet(SelectedColor);
                    foreach (String addkey in new String[] { strval }.Concat(othersets))
                    {
                        ExtraNominoColourLookup[strval] = CreatedSet;
                    }
                }

                return ExtraNominoColourLookup[strval][PixelType];


                
                

                return UseEnhancedImages ? EnhancedGrayColourSet[PixelType] : GrayColourSet[PixelType];
            }
            return SKColors.Magenta;
        }

        public override BlockTypes[] PossibleBlockTypes()
        {
            return (BlockTypes[])Enum.GetValues(typeof(BlockTypes));
        }

        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            if (testvalue.Block is LineSeriesPrimaryShinyBlock)
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.CustomSelector;
            }
            else
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Static;
                //return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
            }
        }
        
            public override bool IsAnimated(NominoBlock block)
        {
            return (block is LineSeriesPrimaryShinyBlock);
        
        }

        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background", 0.5f], Color.Transparent);
        }
        public enum BCT
        {
            Primary,
            Accent,
            Accent2,
            Transparent,
            Black,
            Enhanced_1,
            Enhanced_2,
            Enhanced_3,
            Enhanced_4,
            Enhanced_5,
            Enhanced_6,
            Enhanced_7,
            Enhanced_8,
            Enhanced_9,
            Enhanced_10,
            Enhanced_11
        }
        //if we really wanted to be fancy we'd have blocks for each connection type and stuff.
        //but, we really just want a pretty basic "Tetris 2" look like the NES Game to start with. We can
        //create additional themes later... and I guess a better way of choosing themes...
        public enum BlockTypes
        {
            Normal_Block_Yellow,
            Normal_Block_Red,
            Normal_Block_Blue,
            Normal_Block_Green,
            Normal_Block_Magenta,
            Normal_Block_Orange,
            Normal_Block_Gray,
            Pop_Block_Yellow,
            Pop_Block_Red,
            Pop_Block_Blue,
            Pop_Block_Green,
            Pop_Block_Magenta,
            Pop_Block_Orange,
            Fixed_Block_Yellow,
            Fixed_Block_Red,
            Fixed_Block_Blue,
            Fixed_Block_Green,
            Fixed_Block_Magenta,
            Fixed_Block_Orange,
            Fixed_Block_Gray,
            Shiny_Block_Yellow_25,
            Shiny_Block_Red_25,
            Shiny_Block_Blue_25,
            Shiny_Block_Green_25,
            Shiny_Block_Magenta_25,
            Shiny_Block_Orange_25,
            Shiny_Block_Yellow_50,
            Shiny_Block_Red_50,
            Shiny_Block_Blue_50,
            Shiny_Block_Green_50,
            Shiny_Block_Magenta_50,
            Shiny_Block_Orange_50,
            Shiny_Block_Yellow_75,
            Shiny_Block_Red_75,
            Shiny_Block_Blue_75,
            Shiny_Block_Green_75,
            Shiny_Block_Magenta_75,
            Shiny_Block_Orange_75,
            Shiny_Block_Yellow_100,
            Shiny_Block_Red_100,
            Shiny_Block_Blue_100,
            Shiny_Block_Green_100,
            Shiny_Block_Magenta_100,
            Shiny_Block_Orange_100
        }
        public static BCT[][] Normal_Block = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black}
        };
        public static BCT[][] Fixed_Block = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Accent2, BCT.Accent2, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary, BCT.Black, BCT.Black, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
            new []{BCT.Transparent, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black}
        };

        public static BCT[][] Pop_Block = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent},
            new []{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent}
        };


        

        public static BCT[][] Shiny_Block_25 = Fixed_Block;

        public static BCT[][] Shiny_Block_50 = Fixed_Block;

        public static BCT[][] Shiny_Block_75 = Fixed_Block;

        public static BCT[][] Shiny_Block_100 = Fixed_Block;

        private static BCT[][] Normal_Block_Enhanced, Fixed_Block_Enhanced, Shiny_Block_25_Enhanced, Shiny_Block_50_Enhanced, Shiny_Block_75_Enhanced, Shiny_Block_100_Enhanced, Pop_Block_Enhanced;
        private static readonly Dictionary<SKColor, BCT> bitmappixels = new Dictionary<SKColor, BCT>()
            {
            {SKColors.Transparent,BCT.Transparent },
            {SKColors.Black,BCT.Black },
            {SKColors.Red,BCT.Primary },
            {SKColors.Yellow,BCT.Accent },
            {SKColors.Cyan,BCT.Accent2}
            };

        private static readonly Dictionary<SKColor, BCT> bitmappixels_enhanced = new Dictionary<SKColor, BCT>()
        {
            {SKColors.Transparent,BCT.Transparent },
            {SKColors.Black,BCT.Black },
            {SKColors.Red,BCT.Primary },
            {SKColors.Yellow,BCT.Accent },
            {SKColors.Cyan,BCT.Accent2},
            {new SKColor(205,0,0),BCT.Enhanced_1 },
            {new SKColor(157,0,0), BCT.Enhanced_2 },
            {new SKColor(116,0,0),BCT.Enhanced_3 },
            {new SKColor(249,3,3),BCT.Enhanced_4 },
            {new SKColor(67,1,1),BCT.Enhanced_5 },
            {new SKColor(255,18,18),BCT.Enhanced_6 },
            {new SKColor(255,45,45),BCT.Enhanced_7 },
            {new SKColor(255,75,75),BCT.Enhanced_8},
            {new SKColor(255,103,103) ,BCT.Enhanced_9},
            {new SKColor(88,88,88) ,BCT.Enhanced_10},
            {new SKColor(1,1,1) ,BCT.Enhanced_11},
        };
        private static Dictionary<BlockTypes, BCT[][]> BitmapIndex;
        private static Dictionary<BlockTypes, BCT[][]> BitmapIndexEnhanced;
       

        private static BCT ColorMapLookupFunc(SKColor Src)
        {
            if (bitmappixels_enhanced.ContainsKey(Src)) return bitmappixels_enhanced[Src]; else return BCT.Transparent;
        }
        public Tetris2Theme_Enhanced()
        {
            PrepareThemeData();
        }
        private static bool ThemeDataPrepared = false;
        private static void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            //BCT[][] Normal_Block, Fixed_Block, Shiny_Block_25, Shiny_Block_50, Shiny_Block_75, Shiny_Block_100;

            Normal_Block_Enhanced = GetBCTBitmap("tetris_2_normal_block");
            Fixed_Block_Enhanced = GetBCTBitmap("tetris_2_fixed_block");
            Shiny_Block_25_Enhanced = GetBCTBitmap("tetris_2_shine_25");
            Shiny_Block_50_Enhanced = GetBCTBitmap("tetris_2_shine_50");
            Shiny_Block_75_Enhanced = GetBCTBitmap("tetris_2_shine_75");
            Shiny_Block_100_Enhanced = GetBCTBitmap("tetris_2_shine_100");
            Pop_Block_Enhanced = GetBCTBitmap("tetris_2_pop");



            BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Normal_Block_Yellow,Normal_Block},
            {BlockTypes.Normal_Block_Red,Normal_Block},
            {BlockTypes.Normal_Block_Blue,Normal_Block},
            {BlockTypes.Normal_Block_Green,Normal_Block},
            {BlockTypes.Normal_Block_Magenta,Normal_Block},
            {BlockTypes.Normal_Block_Orange,Normal_Block},
            {BlockTypes.Normal_Block_Gray,Normal_Block},
            {BlockTypes.Pop_Block_Yellow,Pop_Block},
            {BlockTypes.Pop_Block_Red,Pop_Block},
            {BlockTypes.Pop_Block_Blue,Pop_Block},
            {BlockTypes.Pop_Block_Green,Pop_Block},
            {BlockTypes.Pop_Block_Magenta,Pop_Block},
            {BlockTypes.Pop_Block_Orange,Pop_Block},
            {BlockTypes.Fixed_Block_Yellow,Fixed_Block},
            {BlockTypes.Fixed_Block_Red,Fixed_Block},
            {BlockTypes.Fixed_Block_Blue,Fixed_Block},
            {BlockTypes.Fixed_Block_Green,Fixed_Block},
            {BlockTypes.Fixed_Block_Magenta,Fixed_Block},
            {BlockTypes.Fixed_Block_Orange,Fixed_Block},
            {BlockTypes.Fixed_Block_Gray,Fixed_Block},
            {BlockTypes.Shiny_Block_Yellow_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Red_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Blue_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Green_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Magenta_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Orange_25,Shiny_Block_25},
            {BlockTypes.Shiny_Block_Yellow_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Red_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Blue_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Green_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Magenta_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Orange_50,Shiny_Block_50},
            {BlockTypes.Shiny_Block_Yellow_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Red_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Blue_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Green_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Magenta_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Orange_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Yellow_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Red_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Blue_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Green_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Magenta_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Orange_100,Shiny_Block_100}


        };


            BitmapIndexEnhanced = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Normal_Block_Yellow,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Red,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Blue,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Green,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Magenta,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Orange,Normal_Block_Enhanced},
            {BlockTypes.Normal_Block_Gray,Normal_Block_Enhanced},
            {BlockTypes.Pop_Block_Yellow,Pop_Block_Enhanced},
            {BlockTypes.Pop_Block_Red,Pop_Block_Enhanced},
            {BlockTypes.Pop_Block_Blue,Pop_Block_Enhanced},
            {BlockTypes.Pop_Block_Green,Pop_Block_Enhanced},
            {BlockTypes.Pop_Block_Magenta,Pop_Block_Enhanced},
            {BlockTypes.Pop_Block_Orange,Pop_Block_Enhanced},
            {BlockTypes.Fixed_Block_Yellow,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Red,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Blue,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Green,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Magenta,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Orange,Fixed_Block_Enhanced},
            {BlockTypes.Fixed_Block_Gray,Fixed_Block_Enhanced},
            {BlockTypes.Shiny_Block_Yellow_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Red_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Blue_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Green_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Magenta_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Orange_25,Shiny_Block_25_Enhanced},
            {BlockTypes.Shiny_Block_Yellow_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Red_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Blue_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Green_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Magenta_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Orange_50,Shiny_Block_50_Enhanced},
            {BlockTypes.Shiny_Block_Yellow_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Red_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Blue_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Green_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Magenta_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Orange_75,Shiny_Block_75_Enhanced},
            {BlockTypes.Shiny_Block_Yellow_100,Shiny_Block_100_Enhanced},
            {BlockTypes.Shiny_Block_Red_100,Shiny_Block_100_Enhanced},
            {BlockTypes.Shiny_Block_Blue_100,Shiny_Block_100_Enhanced},
            {BlockTypes.Shiny_Block_Green_100,Shiny_Block_100_Enhanced},
            {BlockTypes.Shiny_Block_Magenta_100,Shiny_Block_100_Enhanced},
            {BlockTypes.Shiny_Block_Orange_100,Shiny_Block_100_Enhanced}


        };


            ThemeDataPrepared = true;
        }
        //note: "Shiny" blocks should choose their image at time of selection based on the current time. They should also have the animated flag(s), so that they will
        //animate (like, say, Dr Mario Viruses, but to flash).

    }
}
