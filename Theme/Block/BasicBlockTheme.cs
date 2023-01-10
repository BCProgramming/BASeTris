using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("Simple Style",typeof(StandardTetrisHandler))]
    public class SimpleBlockTheme : CustomPixelTheme<SimpleBlockTheme.BBP,SimpleBlockTheme.BasicBlockTypes>
    {
        public bool AdjacentConnection = false;
        public override String Name { get { return "Simple"; }  }
        public enum BBP
        {
            Transparent,Glint,Center,Shade,DoubleShade
        }
        [Flags]
        public enum BasicBlockTypes
        {
            Basic=0,
            BasicTop = 1,
            BasicLeft = 2,
            BasicBottom = 4,
            BasicRight = 8
        }

        //Block with no other blocks adjacent to it.
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
            new []{BBP.Transparent, BBP.Glint, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade} };





        



        public SKColor[][] LevelColors = new SKColor[][]
            {
                new SKColor[]{SKColors.White,SKColors.SkyBlue,SKColors.Blue,SKColors.DarkBlue},
                //new SKColor[]{SKColors.White,SKColors.Lime,SKColors.Green,SKColors.DarkGreen},
                //new SKColor[]{SKColors.White,SKColors.Pink,SKColors.Purple,SKColors.DarkViolet}

            };
        public SimpleBlockTheme()
        {
            LevelColors = LevelColors.Concat(from c in new SKColor[] { SKColors.Orange, SKColors.Purple, SKColors.Plum, SKColors.Gray, SKColors.DarkGray, SKColors.DarkCyan, SKColors.Goldenrod, SKColors.Brown } select GenerateColorSet(c)).ToArray();
        }
        private SKColor[] GenerateColorSet(SKColor Base)
        {
            return new SKColor[] { SKColors.White, Base, RenderHelpers.MixColor(Base, SKColors.DarkSlateGray, 40), RenderHelpers.MixColor(Base, SKColors.Black, 50) };
        }
       


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
            var buildDictionary =  new Dictionary<BasicBlockTypes, BBP[][]>()
            {
                {BasicBlockTypes.Basic,BasicBlock }
            };

            //get the other combinations too...
            var AllFlagOptions = EnumHelper.GetAllEnums<BasicBlockTypes>();
            foreach (var possibleoptions in AllFlagOptions)
            {
                BBP[][] BuildBitmap = TweakForSideFlags(possibleoptions);
                if(!buildDictionary.ContainsKey(possibleoptions))
                    buildDictionary.Add(possibleoptions, BuildBitmap);
            }

                



            return buildDictionary;
        }
        private BBP[][] TweakForSideFlags(BasicBlockTypes sideflags)
        {
            var Source = BasicBlock;
            if (sideflags == BasicBlockTypes.Basic) return Source;
            BBP[][] CreateArray = new BBP[Source.Length][];
            for (int i = 0; i < Source.Length; i++)
            {
                CreateArray[i] = new BBP[Source[i].Length];
                for (int p = 0; p < Source[i].Length; p++)
                {
                    CreateArray[i][p] = Source[i][p];
                }
            }


            //with the array copy, we now munge it up a bit based on the flags.

            if (sideflags.HasFlag(BasicBlockTypes.BasicTop))
            {
                //top flag, change top two rows, excepting far left and right, to center.
                for (int r = 0; r < 2; r++)
                {
                    for (int c = 1; c < CreateArray[r].Length-1; c++)
                    {
                        CreateArray[r][c] = BBP.Center;
                    }
                }
            }

            if (sideflags.HasFlag(BasicBlockTypes.BasicBottom))
            {
                //bottom flag, change bottom two rows, excepting far left and right, to center.
                for (int r = 0; r < 2; r++)
                {
                    for (int c = 1; c < CreateArray[CreateArray.Length-1-r].Length; c++)
                    {
                        CreateArray[CreateArray.Length-1-r][c] = BBP.Center;
                    }
                }
            }

            if (sideflags.HasFlag(BasicBlockTypes.BasicLeft))
            {
                //left flag, change first two columns to center.

                for (int c = 0; c < 2; c++)
                {
                    for (int r = 1; r < CreateArray.Length; r++)
                    {
                        CreateArray[r][c] = BBP.Center;
                    }
                
                }
            
            }
            if (sideflags.HasFlag(BasicBlockTypes.BasicRight))
            {
                //left flag, change first two columns to center.

                for (int c = 0; c < 2; c++)
                {
                    for (int r = 1; r < CreateArray.Length; r++)
                    {
                        CreateArray[r][CreateArray[r].Length-1-c] = BBP.Center;
                    }

                }

            }


            return CreateArray;

        }
        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            return AdjacentConnection? CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Rotatable:   CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Static;
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (AdjacentConnection)
            {
                BasicBlockTypes bbt = BasicBlockTypes.Basic;
                if (group.Any((b) => (b.X, b.Y) == (element.X - 1, element.Y))) bbt |= BasicBlockTypes.BasicLeft;
                if (group.Any((b) => (b.X, b.Y) == (element.X + 1, element.Y))) bbt |= BasicBlockTypes.BasicRight;
                if (group.Any((b) => (b.X, b.Y) == (element.X, element.Y+1))) bbt |= BasicBlockTypes.BasicBottom;
                if (group.Any((b) => (b.X, b.Y) == (element.X, element.Y - 1))) bbt |= BasicBlockTypes.BasicTop;
                return new BlockTypeReturnData(bbt);
            }
            else
            {
                return new BlockTypeReturnData(BasicBlockTypes.Basic);
            }
        }

        public override BasicBlockTypes[] PossibleBlockTypes()
        {
            if(AdjacentConnection)
                return EnumHelper.GetAllEnums<BasicBlockTypes>().ToArray();
            return new BasicBlockTypes[] { BasicBlockTypes.Basic };
        }
    }
}
