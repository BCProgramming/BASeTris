using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("Simple Style",typeof(StandardTetrisHandler))]
    public class SimpleBlockTheme : CustomPixelTheme<SimpleBlockTheme.BBP,SimpleBlockTheme.BasicBlockTypes>
    {
        public override String Name { get { return "Simple"; }  }
        public enum BBP
        {
            Transparent,Glint,Center,Shade,DoubleShade
        }
        public enum BasicBlockTypes
        {
            Basic
        }


        private static BBP[][] BasicBlock = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent,BBP.Transparent,BBP.Transparent,BBP.Transparent,BBP.Transparent,BBP.Transparent,BBP.Transparent,BBP.Transparent},
            new []{BBP.Transparent, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Shade},
            new []{BBP.Transparent, BBP.Glint, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade}
};
        public SKColor[][] LevelColors = new SKColor[][]
            {
                new SKColor[]{SKColors.White,SKColors.SkyBlue,SKColors.Blue,SKColors.DarkBlue},
                new SKColor[]{SKColors.White,SKColors.Lime,SKColors.Green,SKColors.DarkGreen},
                new SKColor[]{SKColors.White,SKColors.Pink,SKColors.Purple,SKColors.DarkViolet}

            };
        public override SKPointI GetBlockSize(TetrisField field, BasicBlockTypes BlockType)
        {
            return new SKPointI(9, 9);
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement Block, BasicBlockTypes BlockType, BBP PixelType)
        {
            var Level = (field.Handler.Statistics is TetrisStatistics ts) ? ts.Level : 0;
            int LevelField = MathHelper.mod(Level, LevelColors.Length);
            
            switch(PixelType)
            {
                case BBP.Transparent:
                    return SKColors.Transparent;
                case BBP.Glint:
                case BBP.Center:
                case BBP.Shade:
                case BBP.DoubleShade:
                    return LevelColors[LevelField][((int)PixelType) - 1];
            }
            return SKColors.Black;
            
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            return CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Static;
        }
        public override Dictionary<BasicBlockTypes, BBP[][]> GetBlockTypeDictionary()
        {
            return new Dictionary<BasicBlockTypes, BBP[][]>()
            {
                {BasicBlockTypes.Basic,BasicBlock }
            };
        }

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            return CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Static;
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            return new BlockTypeReturnData(BasicBlockTypes.Basic);
        }

        public override BasicBlockTypes[] PossibleBlockTypes()
        {
            return new BasicBlockTypes[] { BasicBlockTypes.Basic };
        }
    }
}
