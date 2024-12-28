using System;
using System.Collections.Generic;
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

    [HandlerTheme("Connected Style", typeof(StandardTetrisHandler))]
    [ThemeDescription("Simple connected style.")]
    public class SimpleConnectedTheme : SimpleBlockTheme
    {
        public override string Name => "Connected";
        public SimpleConnectedTheme()
        {
            AdjacentConnection = true;
        }
    }

    [HandlerTheme("Simple Style", typeof(StandardTetrisHandler))]
    [ThemeDescription("Basic Blocks. BPS NES Tetris")]
    public class SimpleBlockTheme : CustomPixelTheme<SimpleBlockTheme.BBP, SimpleBlockTheme.BasicBlockTypes>
    {
        public bool AdjacentConnection = false;
        public override String Name { get { return "Simple"; } }
        public enum BBP
        {
            Transparent, Glint, Center, Shade, DoubleShade
        }
        [Flags]
        public enum BasicBlockTypes
        {
            Basic = 0,
            BasicTop = 1,
            BasicRight = 2,
            BasicBottom = 4,
            BasicLeft = 8,
            BasicTopRight = 16,
            BasicBottomRight = 32,
            BasicBottomLeft = 64,
            BasicTopLeft = 128
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


        private static BBP[][] BlankBlock = new BBP[][]
{
            new []{BBP.Center, BBP.Center,BBP.Center,BBP.Center,BBP.Center,BBP.Center,BBP.Center,BBP.Center,BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center},
            new []{BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center, BBP.Center } };


        private static BBP[][] LeftShaded = new BBP[][]
{
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent } };

        private static BBP[][] TopShaded = new BBP[][]
{
            new []{BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint, BBP.Glint},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent } };

        private static BBP[][] BottomShaded = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade, BBP.Shade} };

        private static BBP[][] RightShaded = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade } };


        private static BBP[][] TopRightShaded = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent } };

        private static BBP[][] TopLeftShaded = new BBP[][]
{
            new []{BBP.Glint, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent } };
        private static BBP[][] BottomLeftShaded = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Shade, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent } };
        private static BBP[][] BottomRightShaded = new BBP[][]
{
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent},
            new []{BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Transparent, BBP.Shade } };
        public static BasicBlockTypes RotateBlockTypeCW(BasicBlockTypes pInput)
        {

            BasicBlockTypes result = BasicBlockTypes.Basic;
            if (pInput.HasFlag(BasicBlockTypes.BasicTop)) result |= BasicBlockTypes.BasicRight;
            if (pInput.HasFlag(BasicBlockTypes.BasicRight)) result |= BasicBlockTypes.BasicBottom;
            if (pInput.HasFlag(BasicBlockTypes.BasicBottom)) result |= BasicBlockTypes.BasicLeft;
            if (pInput.HasFlag(BasicBlockTypes.BasicLeft)) result |= BasicBlockTypes.BasicTop;

            return result;

        }


        public SKColor[][] LevelColors = new SKColor[][]
            {
                new SKColor[]{SKColors.White,SKColors.SkyBlue,SKColors.Blue,SKColors.DarkBlue},
                //new SKColor[]{SKColors.White,SKColors.Lime,SKColors.Green,SKColors.DarkGreen},
                //new SKColor[]{SKColors.White,SKColors.Pink,SKColors.Purple,SKColors.DarkViolet}

            };

        public SKColor[] GrayedColor = new SKColor[]{SKColors.White,new SKColor(117,117,117),SKColors.Black,SKColors.Black
            };


        public SKColor[] GreenColors = new SKColor[] { SKColors.Lime, SKColors.Green, SKColors.DarkGreen, SKColors.Black };
        public SKColor[] RedColors = new SKColor[] { SKColors.Pink, SKColors.Red, SKColors.DarkRed, SKColors.Black };

        public bool ForceGray { get; set; } = false;


        public bool Christmas { get; set; } = false;

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

                    if (Christmas)
                    {
                        return (Element.GetHashCode() % 2 == 1 ? RedColors : GreenColors)[((int)PixelType) - 1];
                    }

                    if (ForceGray) return GrayedColor[((int)PixelType) - 1];
                    return LevelColors[LevelField][((int)PixelType) - 1];
            }
            return SKColors.Black;
            
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            return AdjacentConnection ? CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.CustomSelector : CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Static;
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
            Source = BlankBlock;
            BBP[][] CreateArray = new BBP[Source.Length][];
            for (int i = 0; i < Source.Length; i++)
            {
                CreateArray[i] = new BBP[Source[i].Length];
                for (int p = 0; p < Source[i].Length; p++)
                {
                    CreateArray[i][p] = Source[i][p];
                }
            }
            Dictionary<BasicBlockTypes, BBP[][]> MaskLookup = new Dictionary<BasicBlockTypes, BBP[][]>()
            { {BasicBlockTypes.BasicBottom,BottomShaded },
                 {BasicBlockTypes.BasicLeft,LeftShaded },
                 {BasicBlockTypes.BasicRight,RightShaded },
                 {BasicBlockTypes.BasicTop,TopShaded }
            };

            foreach (var iterate in MaskLookup.Keys)
            {

                if (!sideflags.HasFlag(iterate))
                {
                    var useMask = MaskLookup[iterate];
                    for (int row = 0; row < useMask.Length; row++)
                    {
                        var thisRow = useMask[row];
                        for (int col = 0; col < thisRow.Length; col++)
                        {
                            if (thisRow[col] != BBP.Transparent)
                            {
                                CreateArray[row][col] = thisRow[col];
                            }
                        }
                    }
                }

            }

            return CreateArray;
            //with the array copy, we now munge it up a bit based on the flags.
            /*
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
                    for (int c = 2; c < CreateArray[CreateArray.Length-1-r].Length-1; c++)
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
                    for (int r = 2; r < CreateArray.Length-1; r++)
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
                    for (int r = 2; r < CreateArray.Length-1; r++)
                    {
                        CreateArray[r][CreateArray[r].Length-1-c] = BBP.Center;
                        
                    }

                }
                CreateArray[CreateArray.Length - 1][CreateArray[0].Length - 1] = BBP.Shade;

            }
            */

            return CreateArray;

        }
        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            return AdjacentConnection? CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.CustomSelector:   CustomPixelTheme<BBP, BasicBlockTypes>.BlockFlags.Static;
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (AdjacentConnection)
            {
                BasicBlockTypes bbt = BasicBlockTypes.Basic;
                if (group.Any((b) => (b.BaseX(), b.BaseY()) == (element.BaseX() - 1, element.BaseY()))) bbt |= BasicBlockTypes.BasicLeft;
                if (group.Any((b) => (b.BaseX(), b.BaseY()) == (element.BaseX() + 1, element.BaseY()))) bbt |= BasicBlockTypes.BasicRight;
                if (group.Any((b) => (b.BaseX(), b.BaseY()) == (element.BaseX(), element.BaseY() + 1))) bbt |= BasicBlockTypes.BasicBottom;
                if (group.Any((b) => (b.BaseX(), b.BaseY()) == (element.BaseX(), element.BaseY() - 1))) bbt |= BasicBlockTypes.BasicTop;
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
        protected override SKImage[] ApplyFunc_Custom(TetrisField field, Nomino Group, NominoElement element, NominoBlock Target, BasicBlockTypes chosentype)
        {
            if (AdjacentConnection && Target is StandardColouredBlock scb)
            {
                //this will be our "correct" 
                
                var getBlockTypeResult =  GetBlockType(Group, element, field);
                chosentype = getBlockTypeResult.BlockType;
                //get the three added rotations for this Nomino.
                var Rotated1 = RotateBlockTypeCW(chosentype);
                var Rotated2 = RotateBlockTypeCW(Rotated1);
                var Rotated3 = RotateBlockTypeCW(Rotated2);
                
                var FirstImage = GetMappedImageSkia(field, Group, element, chosentype);
                var SecondImage = GetMappedImageSkia(field, Group, element, Rotated1);
                var ThirdImage = GetMappedImageSkia(field, Group, element, Rotated2);
                var FourthImage = GetMappedImageSkia(field, Group, element, Rotated3);
                return new SKImage[] { SKImage.FromBitmap(FirstImage), SKImage.FromBitmap(SecondImage), SKImage.FromBitmap(ThirdImage), SKImage.FromBitmap(FourthImage) };
                
            }
            return null;
        }
    }
}
