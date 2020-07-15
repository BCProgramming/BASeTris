using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    [HandlerTheme(typeof(DrMarioHandler))]
    //DrMarioTheme will need to specify the DrMario customization Handler as it's valid Theme once ready.
    //this one doesn't care about the game level- it has two block types- the pills, and the virii.
    //of those we've got 3 colors. We could add more, I suppose, but Dr. Mario has three so let's keep things a bit simpler.
    public class DrMarioTheme : CustomPixelTheme<DrMarioTheme.BCT, DrMarioTheme.BlockTypes>
    {
        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb && lsb is LineSeriesMasterBlock)
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Static;
            }
            else
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
            }
        }
        readonly static BCT[][] Pill_Left = new BCT[][]
                    {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black,BCT.Primary, BCT.Primary , BCT.Accent , BCT.Accent, BCT.Accent, BCT.Accent, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Accent , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black}
                    };
        //Bitmap for the left side of a pill.
      
        readonly static BCT[][] Pill_Right = new BCT[][]
                    {
                new BCT[]{BCT.Black,BCT.Black, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent},
                new BCT[]{BCT.Black,BCT.Accent, BCT.Accent , BCT.Accent , BCT.Accent, BCT.Accent, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent},
                new BCT[]{BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Transparent,BCT.Transparent}
                    };
        //bitmap for the right side of a pill.
       
        //note: pill rotations should be interesting, to get the "glint" to rotate correctly. though initially I think we can just have the glint rotate, too.
        readonly static BCT[][] Pill_Single = Pill_Left;
        //TODO: BCT Bitmaps for a Single pill piece, as well as the Red,Blue, and Yellow Virii.

            //Frame 1 Yellow Virii
        readonly static BCT[][] Yellow_Virii_1 = new BCT[][]
               {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Transparent, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Transparent , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black , BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Accent, BCT.Black , BCT.Accent, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent }
               };
        //Frame 2 Yellow Virii unchanged currently...
        readonly static BCT[][] Yellow_Virii_2 = Yellow_Virii_1;

        readonly static BCT[][] Red_Virii_1 = new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent,BCT.Accent},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Black, BCT.Accent2, BCT.Black , BCT.Accent2, BCT.Black , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Accent2}
                
              };
        readonly static BCT[][] Red_Virii_2 = Red_Virii_1;
        readonly static BCT[][] Blue_Virii_1 = new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Primary, BCT.Black, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary,BCT.Transparent}

              };
        readonly static BCT[][] Blue_Virii_2 = Blue_Virii_1;


        readonly static Dictionary<BlockTypes, BCT[][]> BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Pill_Left_Yellow,Pill_Left },
            {BlockTypes.Pill_Right_Yellow,Pill_Right },
            {BlockTypes.Pill_Single_Yellow,Pill_Single },
            {BlockTypes.Pill_Left_Red,Pill_Left },
            {BlockTypes.Pill_Right_Red,Pill_Right },
            {BlockTypes.Pill_Single_Red,Pill_Single },
            {BlockTypes.Pill_Left_Blue,Pill_Left },
            {BlockTypes.Pill_Right_Blue,Pill_Right },
            {BlockTypes.Pill_Single_Blue,Pill_Single },
            {BlockTypes.Blue_Virus_1,Blue_Virii_1 },
            {BlockTypes.Blue_Virus_2,Blue_Virii_2 },
            {BlockTypes.Yellow_Virus_1,Yellow_Virii_1 },
            {BlockTypes.Yellow_Virus_2,Yellow_Virii_2 },
            {BlockTypes.Red_Virus_1,Red_Virii_1 },
            {BlockTypes.Red_Virus_2,Red_Virii_2 },



        };
        static SKPointI nine = new SKPointI(9, 9);
        static SKPointI eight = new SKPointI(8, 8);
        public static Dictionary<BlockTypes, SKPointI> TypeSizes = new Dictionary<BlockTypes, SKPointI>()
        {

            {BlockTypes.Pill_Left_Yellow,nine},
            {BlockTypes.Pill_Right_Yellow,nine },
            {BlockTypes.Pill_Single_Yellow,eight},
            {BlockTypes.Pill_Left_Red,nine },
            {BlockTypes.Pill_Right_Red,nine},
            {BlockTypes.Pill_Single_Red,eight},
            {BlockTypes.Pill_Left_Blue,nine},
            {BlockTypes.Pill_Right_Blue,nine},
            {BlockTypes.Pill_Single_Blue,eight},
            {BlockTypes.Blue_Virus_1,eight },
            {BlockTypes.Blue_Virus_2,eight},
            {BlockTypes.Yellow_Virus_1,eight},
            {BlockTypes.Yellow_Virus_2,eight},
            {BlockTypes.Red_Virus_1,eight },
            {BlockTypes.Red_Virus_2,eight},
        };

        static SKColor BlueColor = new SKColor(60, 188, 252);
        //Virii are animated, usually. We can deal with that later by implementing additional block types, with the chosen block type depending on the current timer to return one or another animation bitmap frame.
        //but, we'll deal with that as it comes up.

        Dictionary<BCT, SKColor> YellowColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,BlueColor  },
            {BCT.Accent2,SKColors.Red }

        };
        Dictionary<BCT, SKColor> RedColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Red },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,BlueColor  }

        };
        Dictionary<BCT, SKColor> BlueColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,BlueColor  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red }

        };

        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            return TypeSizes[BlockType];
        }
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> VirusTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Virus_1 }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillLeft = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Left_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Left_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Left_Blue }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillRight = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Right_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Right_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Right_Blue }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillSingle = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Single_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Single_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Single_Blue }
        };
        public override BlockTypes GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb)
            {
                if (element.Block is LineSeriesMasterBlock lsm)
                {

                    return VirusTypes_1[lsm.CombiningIndex];

                }

                else if (group == null)
                {
                    return PillSingle[lsb.CombiningIndex];
                    
                }
                else if (group.Count() == 2)
                {
                    int index = group.IndexOf(element);
                    if (index == 0) return PillLeft[lsb.CombiningIndex];
                    else if (index == 1) return PillRight[lsb.CombiningIndex];
                }
            }
                return BlockTypes.Yellow_Virus_1;
        }

        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            return BitmapIndex;
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, BlockTypes BlockType, BCT PixelType)
        {
            if (BlockType == BlockTypes.Yellow_Virus_1 || BlockType == BlockTypes.Yellow_Virus_2)
                return YellowColourSet[PixelType];
            else if (BlockType == BlockTypes.Red_Virus_1 || BlockType == BlockTypes.Red_Virus_2)
                return RedColourSet[PixelType];
            else if (BlockType == BlockTypes.Blue_Virus_1 || BlockType == BlockTypes.Blue_Virus_2)
                return BlueColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Yellow, BlockTypes.Pill_Right_Yellow, BlockTypes.Pill_Single_Yellow }.Contains(BlockType))
                return YellowColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Red, BlockTypes.Pill_Right_Red, BlockTypes.Pill_Single_Red }.Contains(BlockType))
                return RedColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Blue, BlockTypes.Pill_Right_Blue, BlockTypes.Pill_Single_Blue }.Contains(BlockType))
                return BlueColourSet[PixelType];
            return SKColors.Magenta;
        }
        protected override bool IsRotatable(NominoElement testvalue)
        {
            //note:Virii shouldn't rotate...
            return true;
        }
        public override BlockTypes[] PossibleBlockTypes()
        {
            return new BlockTypes[]
            {
                            BlockTypes.Pill_Left_Yellow,
            BlockTypes.Pill_Right_Yellow,
            BlockTypes.Pill_Single_Yellow,
            BlockTypes.Pill_Left_Red,
            BlockTypes.Pill_Right_Red,
            BlockTypes.Pill_Single_Red,
            BlockTypes.Pill_Left_Blue,
            BlockTypes.Pill_Right_Blue,
            BlockTypes.Pill_Single_Blue,
            BlockTypes.Red_Virus_1,
            BlockTypes.Red_Virus_2,
            BlockTypes.Yellow_Virus_1,
            BlockTypes.Yellow_Virus_2,
            BlockTypes.Blue_Virus_1,
            BlockTypes.Blue_Virus_2
            };
        }

        public enum BCT
        {
            Primary,
            Accent,
            Accent2,
            Transparent,
            Black
        }
        public enum BlockTypes
        {
            Pill_Left_Yellow,
            Pill_Right_Yellow,
            Pill_Single_Yellow,
            Pill_Left_Red,
            Pill_Right_Red,
            Pill_Single_Red,
            Pill_Left_Blue,
            Pill_Right_Blue,
            Pill_Single_Blue,
            Red_Virus_1,
            Red_Virus_2,
            Yellow_Virus_1,
            Yellow_Virus_2,
            Blue_Virus_1,
            Blue_Virus_2
            
        }

    }
}
