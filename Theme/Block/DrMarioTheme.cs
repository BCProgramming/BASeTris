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
        //Bitmap for the left side of a pill.
        readonly static BCT[][] Pill_Left = new BCT[][]
                    {
                new BCT[]{BCT.Transparent,BCT.Black, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black},
                new BCT[]{BCT.Black, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black,BCT.Primary, BCT.Primary , BCT.Accent , BCT.Accent , BCT.Accent , BCT.Accent, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Accent , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary},
                new BCT[]{BCT.Black, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary }
                    };
        //bitmap for the right side of a pill.
        readonly static BCT[][] Pill_Right = new BCT[][]
                    {
                new BCT[]{BCT.Transparent,BCT.Black, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black},
                new BCT[]{BCT.Black,BCT.Accent, BCT.Accent, BCT.Accent , BCT.Accent , BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary },
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black }
                    };
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

        readonly static BCT[][] Red_Virii_1 = Yellow_Virii_1;
        readonly static BCT[][] Red_Virii_2 = Yellow_Virii_2;
        readonly static BCT[][] Blue_Virii_1 = Yellow_Virii_1;
        readonly static BCT[][] Blue_Virii_2 = Yellow_Virii_2;


        readonly static Dictionary<BlockTypes, BCT[][]> BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Pill_Left,Pill_Left },
            {BlockTypes.Pill_Right,Pill_Right },
            {BlockTypes.Pill_Single,Pill_Single },
            {BlockTypes.Blue_Virus_1,Blue_Virii_1 },
            {BlockTypes.Blue_Virus_2,Blue_Virii_2 },
            {BlockTypes.Yellow_Virus_1,Yellow_Virii_1 },
            {BlockTypes.Yellow_Virus_2,Yellow_Virii_2 },
            {BlockTypes.Red_Virus_1,Red_Virii_1 },
            {BlockTypes.Red_Virus_2,Red_Virii_2 },



        };

        //Virii are animated, usually. We can deal with that later by implementing additional block types, with the chosen block type depending on the current timer to return one or another animation bitmap frame.
        //but, we'll deal with that as it comes up.

        Dictionary<BCT, SKColor> YellowColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,SKColors.LightBlue },
            {BCT.Accent2,SKColors.Red }

        };


        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            return new SKPointI(8, 8);
        }

        public override BlockTypes GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if(group==null)
            {
                return BlockTypes.Pill_Single;
            }
            if(group.Count() == 2)
            {
                int index = group.IndexOf(element);
                if (index == 0) return BlockTypes.Pill_Left;
                else if (index == 1) return BlockTypes.Pill_Right;
            }

            return BlockTypes.Yellow_Virus_1;
        }

        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            return BitmapIndex;
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, BlockTypes BlockType, BCT PixelType)
        {
            return YellowColourSet[PixelType];
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
                            BlockTypes.Pill_Left,
            BlockTypes.Pill_Right,
            BlockTypes.Pill_Single,
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
            Pill_Left,
            Pill_Right,
            Pill_Single,
            Red_Virus_1,
            Red_Virus_2,
            Yellow_Virus_1,
            Yellow_Virus_2,
            Blue_Virus_1,
            Blue_Virus_2
            
        }

    }
}
