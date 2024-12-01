using BASeTris.Rendering.Adapters;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;

namespace BASeTris
{
    [HandlerTheme("SNES Style", typeof(StandardTetrisHandler),typeof(NTrisGameHandler))]
    [ThemeDescription("From 'Tetris & Dr.Mario' on the SNES")]
    public class SNESTetrominoTheme : NominoTheme
    {
        //This theme is based on the colour and appearance of the Tetris game in SNES Tetris and Dr.Mario.
        //First we have some data structure classes which hold some of the colour information.
        public override String Name { get { return "SNES"; } }
        public class BlockColors
        {
            public BCColor LightColor;
            public BCColor GlintColor;
            public BCColor CenterColor;
            public BCColor ShadedColor;
            public BlockColors(BCColor pLightColor, BCColor pGlintColor, BCColor pCenterColor, BCColor pShadedColor)
            {
                LightColor = pLightColor;
                GlintColor = pGlintColor;
                CenterColor = pCenterColor;
                ShadedColor = pShadedColor;
            }
            public override String ToString()
            {
                return $"Light:{LightColor.ToString()}, Glint:{GlintColor.ToString()}, Center:{CenterColor.ToString()},Shaded:{ShadedColor.ToString()}";
            }
            public override int GetHashCode()
            {
                return LightColor.GetHashCode() ^ GlintColor.GetHashCode() ^ CenterColor.GetHashCode() ^ ShadedColor.GetHashCode();
            }

        }
        public class LevelColors
        {
            public BlockColors TColor { get; private set; }
            public BlockColors JColor { get; private set; }
            public BlockColors ZColor { get; private set; }
            public BlockColors OColor { get; private set; }
            public BlockColors SColor { get; private set; }
            public BlockColors LColor { get; private set; }
            public BlockColors IColor { get; private set; }
            public LevelColors(BlockColors pT, BlockColors pJ, BlockColors pZ, BlockColors pO, BlockColors pS, BlockColors pL, BlockColors pI)
            {
                TColor = pT;
                JColor = pJ;
                ZColor = pZ;
                OColor = pO;
                SColor = pS;
                LColor = pL;
                IColor = pI;
            }
            public BlockColors GetColor(char tettype)
            {
                switch(char.ToUpper(tettype))
                    {
                    case 'T':
                        return TColor;
                    case 'J':
                        return JColor;
                    case 'Z':
                        return ZColor;
                    case 'O':
                        return OColor;
                    case 'S':
                        return SColor;
                    case 'L':
                        return LColor;
                    case 'I':
                        return IColor;
                    default:
                        return IColor;
                }
            }
            public override string ToString()
            {
                return $"    T=[{TColor.ToString()}]\n    J=[{JColor.ToString()}]\n    Z=[{ZColor.ToString()}]\n    O=[{OColor.ToString()}]\n    S=[{SColor.ToString()}]\n    L=[{LColor.ToString()}]\n    I=[{IColor.ToString()}]";
            }
        }

        private static LevelColors[] ColourData = new LevelColors[]{new LevelColors(new BlockColors(new BCColor(255,255,0),new BCColor(214,214,214),new BCColor(247,189,0),new BCColor(198,140,0)),
new BlockColors(new BCColor(49,198,0),new BCColor(214,214,214),new BCColor(24,148,0),new BCColor(8,107,0)),
new BlockColors(new BCColor(49,198,0),new BCColor(214,214,214),new BCColor(24,148,0),new BCColor(8,107,0)),
new BlockColors(new BCColor(255,255,0),new BCColor(214,214,214),new BCColor(247,189,0),new BCColor(198,140,0)),
new BlockColors(new BCColor(33,173,255),new BCColor(214,214,214),new BCColor(0,115,255),new BCColor(0,66,255)),
new BlockColors(new BCColor(33,173,255),new BCColor(214,214,214),new BCColor(0,115,255),new BCColor(0,66,255)),
new BlockColors(new BCColor(255,99,24),new BCColor(214,214,214),new BCColor(198,24,0),new BCColor(123,24,0))),
new LevelColors(new BlockColors(new BCColor(74,173,255),new BCColor(214,214,214),new BCColor(49,115,255),new BCColor(57,66,222)),
new BlockColors(new BCColor(189,148,49),new BCColor(214,214,214),new BCColor(140,90,24),new BCColor(90,57,8)),
new BlockColors(new BCColor(189,148,49),new BCColor(214,214,214),new BCColor(140,90,24),new BCColor(90,57,8)),
new BlockColors(new BCColor(74,173,255),new BCColor(214,214,214),new BCColor(49,115,255),new BCColor(57,66,222)),
new BlockColors(new BCColor(16,239,148),new BCColor(214,214,214),new BCColor(16,181,107),new BCColor(16,123,49)),
new BlockColors(new BCColor(16,239,148),new BCColor(214,214,214),new BCColor(16,181,107),new BCColor(16,123,49)),
new BlockColors(new BCColor(255,115,66),new BCColor(214,214,214),new BCColor(198,66,16),new BCColor(115,8,24))),
new LevelColors(new BlockColors(new BCColor(173,132,214),new BCColor(214,214,214),new BCColor(123,74,189),new BCColor(82,24,165)),
new BlockColors(new BCColor(57,198,198),new BCColor(214,214,214),new BCColor(0,156,140),new BCColor(0,99,99)),
new BlockColors(new BCColor(57,198,198),new BCColor(214,214,214),new BCColor(0,156,140),new BCColor(0,99,99)),
new BlockColors(new BCColor(173,132,214),new BCColor(214,214,214),new BCColor(123,74,189),new BCColor(82,24,165)),
new BlockColors(new BCColor(173,239,82),new BCColor(214,214,214),new BCColor(115,198,0),new BCColor(66,148,0)),
new BlockColors(new BCColor(173,239,82),new BCColor(214,214,214),new BCColor(115,198,0),new BCColor(66,148,0)),
new BlockColors(new BCColor(255,140,140),new BCColor(214,214,214),new BCColor(231,66,57),new BCColor(148,24,8))),
new LevelColors(new BlockColors(new BCColor(189,132,214),new BCColor(214,214,214),new BCColor(140,82,198),new BCColor(107,49,165)),
new BlockColors(new BCColor(247,231,132),new BCColor(214,214,214),new BCColor(214,165,74),new BCColor(173,115,24)),
new BlockColors(new BCColor(247,231,132),new BCColor(214,214,214),new BCColor(214,165,74),new BCColor(173,115,24)),
new BlockColors(new BCColor(189,132,214),new BCColor(214,214,214),new BCColor(140,82,198),new BCColor(107,49,165)),
new BlockColors(new BCColor(173,231,57),new BCColor(214,214,214),new BCColor(132,173,0),new BCColor(66,132,0)),
new BlockColors(new BCColor(173,231,57),new BCColor(214,214,214),new BCColor(132,173,0),new BCColor(66,132,0)),
new BlockColors(new BCColor(198,132,148),new BCColor(214,214,214),new BCColor(198,41,82),new BCColor(123,8,24))),
new LevelColors(new BlockColors(new BCColor(255,239,140),new BCColor(214,214,214),new BCColor(255,198,66),new BCColor(214,132,41)),
new BlockColors(new BCColor(115,247,115),new BCColor(214,214,214),new BCColor(66,181,66),new BCColor(24,132,24)),
new BlockColors(new BCColor(115,247,115),new BCColor(214,214,214),new BCColor(66,181,66),new BCColor(24,132,24)),
new BlockColors(new BCColor(255,239,140),new BCColor(214,214,214),new BCColor(255,198,66),new BCColor(214,132,41)),
new BlockColors(new BCColor(123,222,255),new BCColor(214,214,214),new BCColor(90,148,255),new BCColor(74,107,181)),
new BlockColors(new BCColor(123,222,255),new BCColor(214,214,214),new BCColor(90,148,255),new BCColor(74,107,181)),
new BlockColors(new BCColor(255,132,132),new BCColor(214,214,214),new BCColor(214,66,66),new BCColor(140,41,41))),
new LevelColors(new BlockColors(new BCColor(239,181,198),new BCColor(214,214,214),new BCColor(231,123,140),new BCColor(206,66,107)),
new BlockColors(new BCColor(107,222,189),new BCColor(214,214,214),new BCColor(33,181,132),new BCColor(16,132,74)),
new BlockColors(new BCColor(107,222,189),new BCColor(214,214,214),new BCColor(33,181,132),new BCColor(16,132,74)),
new BlockColors(new BCColor(239,181,198),new BCColor(214,214,214),new BCColor(231,123,140),new BCColor(206,66,107)),
new BlockColors(new BCColor(214,247,74),new BCColor(214,214,214),new BCColor(132,222,24),new BCColor(90,148,16)),
new BlockColors(new BCColor(214,247,74),new BCColor(214,214,214),new BCColor(132,222,24),new BCColor(90,148,16)),
new BlockColors(new BCColor(214,148,123),new BCColor(214,214,214),new BCColor(214,66,57),new BCColor(148,33,24))),
new LevelColors(new BlockColors(new BCColor(222,247,148),new BCColor(214,214,214),new BCColor(123,231,0),new BCColor(74,156,57)),
new BlockColors(new BCColor(156,206,239),new BCColor(214,214,214),new BCColor(24,156,247),new BCColor(24,99,206)),
new BlockColors(new BCColor(156,206,239),new BCColor(214,214,214),new BCColor(24,156,247),new BCColor(24,99,206)),
new BlockColors(new BCColor(222,247,148),new BCColor(214,214,214),new BCColor(123,231,0),new BCColor(74,156,57)),
new BlockColors(new BCColor(214,165,239),new BCColor(214,214,214),new BCColor(156,107,206),new BCColor(115,82,165)),
new BlockColors(new BCColor(214,165,239),new BCColor(214,214,214),new BCColor(156,107,206),new BCColor(115,82,165)),
new BlockColors(new BCColor(255,189,123),new BCColor(214,214,214),new BCColor(255,115,107),new BCColor(206,49,90))),
new LevelColors(new BlockColors(new BCColor(198,132,239),new BCColor(214,214,214),new BCColor(173,66,214),new BCColor(140,16,206)),
new BlockColors(new BCColor(255,214,173),new BCColor(214,214,214),new BCColor(206,165,66),new BCColor(173,123,33)),
new BlockColors(new BCColor(255,214,173),new BCColor(214,214,214),new BCColor(206,165,66),new BCColor(173,123,33)),
new BlockColors(new BCColor(198,132,239),new BCColor(214,214,214),new BCColor(173,66,214),new BCColor(140,16,206)),
new BlockColors(new BCColor(33,173,255),new BCColor(214,214,214),new BCColor(0,115,255),new BCColor(0,66,255)),
new BlockColors(new BCColor(33,173,255),new BCColor(214,214,214),new BCColor(0,115,255),new BCColor(0,66,255)),
new BlockColors(new BCColor(255,156,115),new BCColor(214,214,214),new BCColor(198,66,16),new BCColor(132,24,41))),
new LevelColors(new BlockColors(new BCColor(247,247,107),new BCColor(214,214,214),new BCColor(189,173,24),new BCColor(148,123,16)),
new BlockColors(new BCColor(82,222,222),new BCColor(214,214,214),new BCColor(0,156,156),new BCColor(49,74,173)),
new BlockColors(new BCColor(82,222,222),new BCColor(214,214,214),new BCColor(0,156,156),new BCColor(49,74,173)),
new BlockColors(new BCColor(247,247,107),new BCColor(214,214,214),new BCColor(189,173,24),new BCColor(148,123,16)),
new BlockColors(new BCColor(165,247,165),new BCColor(214,214,214),new BCColor(41,198,0),new BCColor(33,140,0)),
new BlockColors(new BCColor(165,247,165),new BCColor(214,214,214),new BCColor(41,198,0),new BCColor(33,140,0)),
new BlockColors(new BCColor(231,165,99),new BCColor(214,214,214),new BCColor(222,82,41),new BCColor(140,41,0))),
new LevelColors(new BlockColors(new BCColor(222,239,115),new BCColor(214,214,214),new BCColor(140,198,0),new BCColor(99,148,0)),
new BlockColors(new BCColor(239,156,107),new BCColor(214,214,214),new BCColor(165,107,16),new BCColor(115,74,8)),
new BlockColors(new BCColor(239,156,107),new BCColor(214,214,214),new BCColor(165,107,16),new BCColor(115,74,8)),
new BlockColors(new BCColor(222,239,115),new BCColor(214,214,214),new BCColor(140,198,0),new BCColor(99,148,0)),
new BlockColors(new BCColor(231,181,214),new BCColor(214,214,214),new BCColor(189,107,189),new BCColor(123,74,156)),
new BlockColors(new BCColor(231,181,214),new BCColor(214,214,214),new BCColor(189,107,189),new BCColor(123,74,156)),
new BlockColors(new BCColor(255,214,115),new BCColor(214,214,214),new BCColor(255,107,57),new BCColor(198,49,24))),
new LevelColors(new BlockColors(new BCColor(255,255,140),new BCColor(214,214,214),new BCColor(247,214,0),new BCColor(198,165,0)),
new BlockColors(new BCColor(239,156,107),new BCColor(214,214,214),new BCColor(165,107,16),new BCColor(115,74,8)),
new BlockColors(new BCColor(239,156,107),new BCColor(214,214,214),new BCColor(165,107,16),new BCColor(115,74,8)),
new BlockColors(new BCColor(255,255,140),new BCColor(214,214,214),new BCColor(247,214,0),new BCColor(198,165,0)),
new BlockColors(new BCColor(198,239,132),new BCColor(214,214,214),new BCColor(132,214,0),new BCColor(66,156,0)),
new BlockColors(new BCColor(198,239,132),new BCColor(214,214,214),new BCColor(132,214,0),new BCColor(66,156,0)),
new BlockColors(new BCColor(255,206,82),new BCColor(214,214,214),new BCColor(255,107,8),new BCColor(173,16,8))),
new LevelColors(new BlockColors(new BCColor(222,239,239),new BCColor(255,255,255),new BCColor(115,231,231),new BCColor(33,198,198)),
new BlockColors(new BCColor(239,222,255),new BCColor(255,255,255),new BCColor(206,156,255),new BCColor(173,123,239)),
new BlockColors(new BCColor(239,222,255),new BCColor(255,255,255),new BCColor(206,156,255),new BCColor(173,123,239)),
new BlockColors(new BCColor(222,239,239),new BCColor(255,255,255),new BCColor(115,231,231),new BCColor(33,198,198)),
new BlockColors(new BCColor(222,247,165),new BCColor(255,255,255),new BCColor(148,247,0),new BCColor(107,206,0)),
new BlockColors(new BCColor(222,247,165),new BCColor(255,255,255),new BCColor(148,247,0),new BCColor(107,206,0)),
new BlockColors(new BCColor(247,222,206),new BCColor(255,255,255),new BCColor(247,173,107),new BCColor(222,123,66))),
new LevelColors(new BlockColors(new BCColor(239,222,255),new BCColor(255,255,255),new BCColor(206,156,255),new BCColor(173,123,239)),
new BlockColors(new BCColor(222,239,239),new BCColor(255,255,255),new BCColor(115,231,231),new BCColor(33,198,198)),
new BlockColors(new BCColor(222,239,239),new BCColor(255,255,255),new BCColor(115,231,231),new BCColor(33,198,198)),
new BlockColors(new BCColor(239,222,255),new BCColor(255,255,255),new BCColor(206,156,255),new BCColor(173,123,239)),
new BlockColors(new BCColor(255,239,222),new BCColor(255,255,255),new BCColor(255,255,66),new BCColor(239,189,66)),
new BlockColors(new BCColor(255,239,222),new BCColor(255,255,255),new BCColor(255,255,66),new BCColor(239,189,66)),
new BlockColors(new BCColor(255,222,255),new BCColor(255,255,255),new BCColor(255,156,231),new BCColor(255,115,173)))};



        private static Dictionary<BlockColors, SKBitmap> CachedUnsetImages = new Dictionary<BlockColors, SKBitmap>();
        private static Dictionary<BlockColors, SKBitmap> CachedSetImages = new Dictionary<BlockColors, SKBitmap>();

        private static Dictionary<BlockColors, Image> CachedUnsetImages_GDI = new Dictionary<BlockColors, Image>();
        private static Dictionary<BlockColors, Image> CachedSetImages_GDI = new Dictionary<BlockColors, Image>();


        public static Image GetSetImageGDI(BlockColors source)
        {
            if(!CachedSetImages_GDI.ContainsKey(source))
            {
                var SKresult = GetSetImage(source);
                CachedSetImages_GDI.Add(source, SkiaSharp.Views.Desktop.Extensions.ToBitmap(SKresult));
                
            }
            return CachedSetImages_GDI[source];
        }
        public static Image GetUnsetImageGDI(BlockColors source)
        {
            if (!CachedUnsetImages_GDI.ContainsKey(source))
            {
                var SKresult = GetUnsetImage(source);
                CachedUnsetImages_GDI.Add(source, SkiaSharp.Views.Desktop.Extensions.ToBitmap(SKresult));

            }
            return CachedUnsetImages_GDI[source];
        }
        public static SKBitmap GetSetImage(BlockColors source)
        {
            if(!CachedSetImages.ContainsKey(source))
            {
                var CreateImage = DrawSetImage(source);
                CachedSetImages.Add(source, CreateImage);
            }
            return CachedSetImages[source];
                
        }
        public static SKBitmap GetUnsetImage(BlockColors source)
        {
            if (!CachedUnsetImages.ContainsKey(source))
            {
                var CreateImage = DrawUnSetImage(source);
                CachedUnsetImages.Add(source, CreateImage);
            }
            return CachedUnsetImages[source];

        }
        public static SKBitmap GetUnsetImage(int LevelNumber, char TetrominoType)
        {
            BlockColors usebc = GetBlockColors(LevelNumber, TetrominoType);
            return GetUnsetImage(usebc);
        }

        public static SKBitmap GetSetImage(int LevelNumber,char TetrominoType)
        {
            BlockColors usebc = GetBlockColors(LevelNumber, TetrominoType);
            return GetSetImage(usebc);
        }
        public static Image GetUnsetImageGDI(int LevelNumber, char TetrominoType)
        {
            BlockColors usebc = GetBlockColors(LevelNumber, TetrominoType);
            return GetUnsetImageGDI(usebc);
        }
        public static Image GetSetImageGDI(int LevelNumber,char TetrominoType)
        {
            BlockColors usebc = GetBlockColors(LevelNumber, TetrominoType);
            return GetSetImageGDI(usebc);
        }

        static String TetOrder = "TJZOSLI";
        static char[] TetChars = TetOrder.ToCharArray();
       
        public static BlockColors GetBlockColors(int LevelNumber,char TetrominoType)
        {
            //first get the level data...
            var LevelColourData = ColourData[(LevelNumber>12?12:LevelNumber)];
            
            
            switch(TetrominoType)
            {
                case 'T':
                    return LevelColourData.TColor;
                case 'J':
                    return LevelColourData.JColor;
                case 'Z':
                    return LevelColourData.ZColor;
                case 'O':
                    return LevelColourData.OColor;
                case 'S':
                    return LevelColourData.SColor;
                case 'L':
                    return LevelColourData.LColor;
                case 'I':
                    return LevelColourData.IColor;
                default:
                    throw new ArgumentException("Tetromino " + TetrominoType + " not recognized.");
            }
        }
        
        private enum PT
        {
            Light,
            Glint,
            Center,
            Shaded
        }
        private static SKImageInfo blockinfo = new SKImageInfo(8, 8, SKColorType.Rgba8888,SKAlphaType.Opaque);
        readonly static PT[][] SetByNumbers = new PT[][]
            {
                new PT[]{PT.Center,PT.Center, PT.Center , PT.Center , PT.Center , PT.Center , PT.Center ,PT.Shaded},
                new PT[]{PT.Center,PT.Glint, PT.Light , PT.Light, PT.Light , PT.Light , PT.Light, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Shaded , PT.Shaded , PT.Shaded , PT.Center , PT.Light, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Shaded , PT.Shaded , PT.Shaded , PT.Center , PT.Light, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Shaded , PT.Shaded , PT.Shaded , PT.Center , PT.Light, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Center , PT.Center , PT.Center , PT.Center , PT.Light, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Light  , PT.Light  , PT.Light  , PT.Light  , PT.Light ,PT.Shaded},
                new PT[]{PT.Shaded,PT.Shaded, PT.Shaded , PT.Shaded , PT.Shaded , PT.Shaded, PT.Shaded ,PT.Shaded}

            };
        readonly static PT[][] UnSetByNumbers = new PT[][]
            {
                new PT[]{PT.Center,PT.Center, PT.Center , PT.Center , PT.Center , PT.Center , PT.Center ,PT.Shaded},
                new PT[]{PT.Center,PT.Glint, PT.Light , PT.Light, PT.Light , PT.Light , PT.Center, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Light , PT.Light , PT.Center , PT.Shaded , PT.Center, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Light , PT.Center , PT.Shaded , PT.Shaded , PT.Center, PT.Shaded},
                new PT[]{PT.Center,PT.Light, PT.Center , PT.Shaded , PT.Shaded , PT.Shaded , PT.Center, PT.Shaded},
                new PT[]{PT.Center,PT.Center, PT.Shaded , PT.Shaded , PT.Shaded , PT.Shaded , PT.Center, PT.Shaded},
                new PT[]{PT.Center,PT.Center, PT.Center, PT.Center, PT.Center, PT.Center, PT.Center ,PT.Shaded},
                new PT[]{PT.Shaded,PT.Shaded, PT.Shaded , PT.Shaded , PT.Shaded , PT.Shaded, PT.Shaded ,PT.Shaded}

            };
        private static BCColor SelectColor(PT Type,BlockColors basis)
        {
            switch(Type)
            {
                case PT.Light:
                    return basis.LightColor;
                case PT.Glint:
                    return basis.GlintColor;
                case PT.Center:
                    return basis.CenterColor;
                case PT.Shaded:
                    return basis.ShadedColor;
                default:
                    return basis.CenterColor;
            }
        }
        private static SKBitmap DrawSetImage(BlockColors basis)
        {
            return DrawArrayImage(basis, SetByNumbers);
        }
        private static SKBitmap DrawUnSetImage(BlockColors basis)
        {
            return DrawArrayImage(basis, UnSetByNumbers);
        }
        private static SKBitmap DrawArrayImage(BlockColors basis,PT[][] ArraySrc)
        {

            SKBitmap drawimage = new SKBitmap(blockinfo, SKBitmapAllocFlags.ZeroPixels);
            SKCanvas skc = new SKCanvas(drawimage);
            for(int y=0;y<8;y++)
            {
                for(int x=0;x<8;x++)
                {
                   
                    var PixelType = ArraySrc[y][x];
                    BCColor ChosenColor = SelectColor(PixelType,basis);
                    SKColor useChosen = ChosenColor;
                    skc.DrawPoint(new SKPoint(x, y), ChosenColor);
                    //drawimage.SetPixel(x, y, ChosenColor);

                }
            }
            skc.Flush();
            return drawimage;

        }
        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler,TetrisField Field)
        {
            //choose a random level...
            int Randomlevel = TetrisGame.StatelessRandomizer.Next(0, 13);

            //choose a random tetromino type.
            char randomTet = TetrisGame.Choose(TetOrder.ToCharArray());

            //get the image for it

            foreach(var iterate in Group)
            {
                if(iterate.Block is StandardColouredBlock)
                {
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetSetImage(ColourData[Randomlevel].GetColor(randomTet))) };
                }
            }

        }
        Dictionary<String, int> ChosenNominoLevelAssignments = new Dictionary<string, int>();
        Dictionary<String, char> ChosenNominoAssignments = new Dictionary<string, char>();
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            
            char DesiredNomino = 'I';
            Dictionary<Type, char> NominoLookup = new Dictionary<Type, char>()
            {
                {typeof(Tetromino_T),'T' },
                {typeof(Tetromino_S),'S' },
                {typeof(Tetromino_Z),'Z' },
                {typeof(Tetromino_L),'L' },
                {typeof(Tetromino_J),'J' },
                {typeof(Tetromino_O),'O' },
                {typeof(Tetromino_I),'I' },
                {typeof(Tetromino_Y),'Y' }
            };
            String sKey = null;
            
            if(NominoLookup.ContainsKey(Group.GetType()))
            {
               DesiredNomino = NominoLookup[Group.GetType()];
            }
            else
            {
                sKey = GetNominoKey(Group, GameHandler, Field);

                if (!ChosenNominoAssignments.ContainsKey(sKey))
                {
                    char chooseNomino = TetrisGame.Choose(TetOrder);
                    ChosenNominoAssignments.Add(sKey, chooseNomino);
                    String[] Otherkeys = NNominoGenerator.GetOtherRotationStrings(Group);
                    foreach (var assignkey in Otherkeys)
                        ChosenNominoAssignments[assignkey] = chooseNomino;
                }
                else
                {
                    ;
                }
                DesiredNomino = ChosenNominoAssignments[sKey];
                
            }
            var useLevel = Field.Level > 12 ? 12 : Field.Level;
            if (!(Group is Tetromino))
            {

                if (!ChosenNominoLevelAssignments.ContainsKey(sKey))
                {
                    var generated = TetrisGame.StatelessRandomizer.Next(13);
                    ChosenNominoLevelAssignments[sKey] = generated;
                    //get the three rotations and add the key for them too.
                    String[] Otherkeys = NNominoGenerator.GetOtherRotationStrings(Group);
                    foreach (var setkey in Otherkeys)
                        ChosenNominoLevelAssignments[setkey] = generated;

                }
                
                useLevel = ChosenNominoLevelAssignments[sKey];
                
            }
            foreach (var iterate in Group)
            {
                
                if (iterate.Block is StandardColouredBlock)
                {
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc.BlockColor = Color.Black;
                    
                    
                    if (Field.GetActiveBlockGroups().Contains(Group))
                        sbc._RotationImagesSK = new SKImage[] {  SKImage.FromBitmap(GetUnsetImage(ColourData[useLevel].GetColor(DesiredNomino)) )};
                    else
                    {
                        sbc._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetSetImage(ColourData[useLevel].GetColor(DesiredNomino)) ) };
                    }
                }
            }

        }
        public SNESTetrominoTheme()
        {
            
        }

        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(()=>new PlayFieldBackgroundInfo(TetrisGame.Imageman["background", 0.5f], Color.Transparent));
        }
    }
}
