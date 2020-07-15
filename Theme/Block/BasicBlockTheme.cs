using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    [HandlerTheme(typeof(StandardTetrisHandler))]
    public class SimpleBlockTheme : CustomPixelTheme<SimpleBlockTheme.BBP,SimpleBlockTheme.BasicBlockTypes>
    {
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

        public override SKColor GetColor(TetrisField field, Nomino Element, BasicBlockTypes BlockType, BBP PixelType)
        {
            int LevelField = MathHelper.mod(field.Level, LevelColors.Length);
            
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
        protected override bool IsRotatable(NominoElement testvalue)
        {
            return false;
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

        public override BasicBlockTypes GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            return BasicBlockTypes.Basic;
        }

        public override BasicBlockTypes[] PossibleBlockTypes()
        {
            return new BasicBlockTypes[] { BasicBlockTypes.Basic };
        }
    }
}
