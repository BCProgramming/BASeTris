using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    public class Tetris2Theme : CustomPixelTheme<Tetris2Theme.BCT, Tetris2Theme.BlockTypes>
    {
        public override string Name => "Tetris 2";

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            throw new NotImplementedException();
        }

        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            throw new NotImplementedException();
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            throw new NotImplementedException();
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, BlockTypes BlockType, BCT PixelType)
        {
            throw new NotImplementedException();
        }

        public override BlockTypes[] PossibleBlockTypes()
        {
            throw new NotImplementedException();
        }

        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            throw new NotImplementedException();
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
            Shiny_block_Magenta_75,
            Shiny_Block_Orange_75,
            Shiny_Block_Yellow_100,
            Shiny_Block_Red_100,
            Shiny_Block_Blue_100,
            Shiny_Block_Green_100,
            Shiny_Block_Magenta_100,
            Shiny_Block_Orange_100
        }
        private static BCT[][] Normal_Block, Fixed_Block, Shiny_Block_25, Shiny_Block_50, Shiny_Block_75, Shiny_Block_100, Pop_Block;
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
        private static BCT[][] GetBCTBitmap(String ImageKey)
        {
            return GetBCTBitmap(ImageKey, ColorMapLookupFunc);
        }

        private static BCT ColorMapLookupFunc(SKColor Src)
        {
            if (bitmappixels_enhanced.ContainsKey(Src)) return bitmappixels_enhanced[Src]; else return BCT.Transparent;
        }

        private static bool ThemeDataPrepared = false;
        private static void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            //BCT[][] Normal_Block, Fixed_Block, Shiny_Block_25, Shiny_Block_50, Shiny_Block_75, Shiny_Block_100;

            Normal_Block = GetBCTBitmap("tetris2_normal_block");
            Fixed_Block = GetBCTBitmap("tetris2_fixed_block");
            Shiny_Block_25 = GetBCTBitmap("tetris2_shine_25");
            Shiny_Block_50 = GetBCTBitmap("tetris2_shine_50");
            Shiny_Block_75 = GetBCTBitmap("tetris2_shine_75");
            Shiny_Block_100 = GetBCTBitmap("tetris2_shine_100");
            Pop_Block = GetBCTBitmap("tetris_2_pop");
            

            BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Normal_Block_Yellow,Normal_Block},
            {BlockTypes.Normal_Block_Red,Normal_Block},
            {BlockTypes.Normal_Block_Blue,Normal_Block},
            {BlockTypes.Normal_Block_Green,Normal_Block},
            {BlockTypes.Normal_Block_Magenta,Normal_Block},
            {BlockTypes.Normal_Block_Orange,Normal_Block},
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
            {BlockTypes.Shiny_block_Magenta_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Orange_75,Shiny_Block_75},
            {BlockTypes.Shiny_Block_Yellow_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Red_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Blue_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Green_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Magenta_100,Shiny_Block_100},
            {BlockTypes.Shiny_Block_Orange_100,Shiny_Block_100}


        };


            ThemeDataPrepared = true;
        }
        //note: "Shiny" blocks should choose their image at time of selection based on the current time. They should also have the animated flag(s), so that they will
        //animate (like, say, Dr Mario Viruses, but to flash).

    }
}
