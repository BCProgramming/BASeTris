﻿using System.Collections.Generic;
using System.Drawing;
using BASeTris.Rendering.Adapters;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using SkiaSharp;
using BASeTris.GameStates.GameHandlers;
using System.Linq;
using BASeTris.GameStates.Menu;
using System;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("NES Style (Glitched)", typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("The glitched out colours from very high level play in the NES Game.")]
    public class NESGlitchedTetrominoTheme : NESTetrominoTheme
    {
        public override string Name => "NES (Glitched)";
        public NESGlitchedTetrominoTheme()
        {
            this.ColourSelectionType = NESTetrominoThemeColourSelectionTypeConstants.Glitched;
        }
    }
    [HandlerTheme("NES Style (Random)", typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("NES Style, but with randomly chosen colourways.")]
    public class NESRandomTetrominoTheme : NESTetrominoTheme
    {
        public override string Name => "NES (Random)";
        public NESRandomTetrominoTheme()
        {
            this.ColourSelectionType = NESTetrominoThemeColourSelectionTypeConstants.Random;
        }
    }


    [HandlerTheme("NES Style", typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("From the Visuals of the NES Release.")]
    public class NESTetrominoTheme : CustomPixelTheme<NESTetrominoTheme.BCT, NESTetrominoTheme.NESBlockTypes>
    {
        public override string Name { get { return "NES"; } }

        private StandardNESThemeImageProvider _ThemeProvider;
        public override ThemeImageProvider ThemeProvider { get => _ThemeProvider; set => _ThemeProvider = (StandardNESThemeImageProvider)value; }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(() =>
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
            });
        }
        SKColor[][] LevelColorSets = new SKColor[][] { Level0Colors, Level1Colors, Level2Colors, Level3Colors, Level4Colors, Level5Colors, Level6Colors, Level7Colors, Level8Colors, Level9Colors };

        private Dictionary<int, SKColor[]> StoredRandomColorSets = new Dictionary<int, SKColor[]>();

        public SKColor[] GetRandomLevelColorSet(int pLevel,IRandomizer rgen)
        {

            if (!StoredRandomColorSets.ContainsKey(pLevel))
            {
                StoredRandomColorSets.Add(pLevel, GetRandomColorSet(rgen));
            }
            return StoredRandomColorSets[pLevel];

        }
        public SKColor[] GetRandomColorSet(IRandomizer rgen)
        {
            return new SKColor[] { RandomColor(rgen), RandomColor(rgen), SKColors.Black, RandomColor(rgen) };
        }
        private SKColor RandomColor(IRandomizer rgen)
        {
            return new SKColor((byte)rgen.Next(255), (byte)rgen.Next(255), (byte)rgen.Next(255));
        }
        public override SKPointI GetBlockSize(TetrisField field, NESBlockTypes BlockType)
        {
            return new SKPointI(DarkerBlock.Length, DarkerBlock[0].Length);
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            return CustomPixelTheme<BCT, NESBlockTypes>.BlockFlags.Static;
        }

        public enum NESTetrominoThemeColourSelectionTypeConstants
        {
            Default,
            Glitched,
            Random
        }

        public NESTetrominoThemeColourSelectionTypeConstants ColourSelectionType = NESTetrominoThemeColourSelectionTypeConstants.Default;
        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, NESBlockTypes BlockType, BCT PixelType)
        {
            int LevelNum = field.Level % 256;
            int LevelIndex = MathHelper.mod(LevelNum, AllLevelColors.Length);
            
            var ChosenLevelSet = LevelColorSets[LevelIndex];

            if (ColourSelectionType == NESTetrominoThemeColourSelectionTypeConstants.Random)
            {
                ChosenLevelSet = GetRandomLevelColorSet(LevelNum, TetrisGame.StatelessRandomizer);
            }
            else if (LevelNum >= 138)
            {
                LevelIndex = LevelNum;
                ChosenLevelSet = GlitchColourSets[LevelNum - 138];
            }
            else if (ColourSelectionType == NESTetrominoThemeColourSelectionTypeConstants.Glitched)
            {
                ChosenLevelSet = GlitchColourSets[LevelNum];
            }
            
                int BlockCount = 0;
            if (!(Element is Tetromino) && Element.Count() > 1)
            {
                //get the index.
                var cw = NNominoGenerator.GetNominoPoints(Element);
                //long GetSpecialIndex = NNominoGenerator.GetIndex(cw);
                string sHash = NNominoGenerator.StringRepresentation(cw);
                if (!LookupColorSet.ContainsKey((LevelIndex, sHash)))
                {
                    var chosenresult = CreateRandomColorSet();
                    LookupColorSet[(LevelIndex, sHash)] = chosenresult;
                    var cw2 = NNominoGenerator.RotateCW(cw);
                    var cw3 = NNominoGenerator.RotateCW(cw2);
                    var cw4 = NNominoGenerator.RotateCW(cw3);
                    LookupColorSet[(LevelIndex, NNominoGenerator.StringRepresentation(cw2))] = chosenresult;
                    LookupColorSet[(LevelIndex, NNominoGenerator.StringRepresentation(cw3))] = chosenresult;
                    LookupColorSet[(LevelIndex, NNominoGenerator.StringRepresentation(cw4))] = chosenresult;
                }
                if(LevelNum<138 || Element.Count >4) ChosenLevelSet = LookupColorSet[(LevelIndex, sHash)];
            }

            //select a random type





            switch (PixelType)
            {
                case BCT.Transparent:
                    return SKColors.Transparent;
                case BCT.Glint:
                    if (ChosenLevelSet.Length >= 4)
                        return ChosenLevelSet[3];
                    else
                        return SKColors.White;
                case BCT.Base_Dark:
                    return ChosenLevelSet[0];
                case BCT.Base_Light:
                    return ChosenLevelSet[1];
                case BCT.Black:
                    return SKColors.Black;
                default:
                    return ChosenLevelSet[0];
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
        private Dictionary<string, NESBlockTypes> LookupBlockTypes = new Dictionary<string, NESBlockTypes>();
        private Dictionary<(int, string), SKColor[]> LookupColorSet = new Dictionary<(int, string), SKColor[]>();
        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            var bg = group;
            if (bg is Tetromino_I || bg is Tetromino_T || bg is Tetromino_O)
            {
                return new BlockTypeReturnData(NESBlockTypes.Boxed);
            }
            else if (bg is Tetromino_J || bg is Tetromino_Z)
                return new BlockTypeReturnData(NESBlockTypes.Darker);
            else if (bg is Tetromino)
                return new BlockTypeReturnData(NESBlockTypes.Lighter);
            else
            {
                int BlockCount = 0;
                if (!(bg is Tetromino))
                {


                    var result = NNominoGenerator.GetNominoData(LookupBlockTypes, bg, () => TetrisGame.Choose(new NESBlockTypes[] { NESBlockTypes.Boxed, NESBlockTypes.Darker, NESBlockTypes.Lighter }));
                    return new BlockTypeReturnData(result);
                    
                    /*//get the index.
                    var cw = NNominoGenerator.GetNominoPoints(bg);
                    //long GetSpecialIndex = NNominoGenerator.GetIndex(cw);
                    string sHash = NNominoGenerator.StringRepresentation(cw);
                    if (!LookupBlockTypes.ContainsKey(sHash))
                    {
                        var chosenresult = TetrisGame.Choose(new NESBlockTypes[] { NESBlockTypes.Boxed, NESBlockTypes.Darker, NESBlockTypes.Lighter });
                        LookupBlockTypes[sHash] = chosenresult;
                        var cw2 = NNominoGenerator.RotateCW(cw);
                        var cw3 = NNominoGenerator.RotateCW(cw2);
                        var cw4 = NNominoGenerator.RotateCW(cw3);
                        LookupBlockTypes[NNominoGenerator.StringRepresentation(cw2)] = chosenresult;
                        LookupBlockTypes[NNominoGenerator.StringRepresentation(cw3)] = chosenresult;
                        LookupBlockTypes[NNominoGenerator.StringRepresentation(cw4)] = chosenresult;
                    }
                    return new BlockTypeReturnData(LookupBlockTypes[sHash]);
                    //select a random type
                    */

                }
                else
                {
                    return new BlockTypeReturnData(TetrisGame.Choose(new NESBlockTypes[] { NESBlockTypes.Boxed, NESBlockTypes.Darker, NESBlockTypes.Lighter }));
                }
            }


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
            Base_Light,
            Black
        }
        public enum NESBlockTypes
        {
            Darker,
            Lighter,
            Boxed
        }

        public static BCT[][] DarkerBlock_Core = new BCT[][]
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
        public static BCT[][] DarkerBlock = DoubleAndOutline(DarkerBlock_Core);
        public static BCT[][] LighterBlock_Core = new BCT[][]
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
        public static BCT[][] LighterBlock = DoubleAndOutline(LighterBlock_Core);

        public static BCT[][] CenterWhiteBlock_Core = new BCT[][]
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
        public static BCT[][] CenterWhiteBlock = DoubleAndOutline(CenterWhiteBlock_Core);
        private static BCT[][] DoubleAndOutline(BCT[][] Source)
        {
            return Source;
            /*return (from u in Source select (from f in u select (f==BCT.Transparent?BCT.Black:f)).ToArray()).ToArray(); 
            //not used for now...
            int UseWidth = Source[0].Length*2+2;
            BCT[][] result = new BCT[Source.Length * 2 + 2][];
            //add the top outline.
            result[0] = Enumerable.Repeat<BCT>(BCT.Black, UseWidth).ToArray();
            result[result.Length - 1] = result[0];
            //iterate through each row in the source
            for(int row = 0;row<Source.Length;row++)
            {
                BCT[] buildrow = new BCT[Source.Length * 2 + 2];
                buildrow[0] = BCT.Black;
                buildrow[buildrow.Length - 1] = BCT.Black;
                int currentcopylocation = 0;
                for(int copyindex=0;copyindex<Source[row].Length;copyindex++)
                {
                    //copy this value twice.
                    buildrow[2 + currentcopylocation] = buildrow[1 + currentcopylocation] = Source[row][copyindex];
                    currentcopylocation += 2;
                }
                result[row * 2+1] = result[row * 2 + 2] = buildrow;

            }
            return result;
            */
        }

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

        public static SKColor[] Level0Colors = new SKColor[] { SKColors.Blue, SKColors.DeepSkyBlue, SKColors.Black,SKColors.White };
        public static SKColor[] Level1Colors = new SKColor[] { SKColors.Green, SKColors.GreenYellow, SKColors.Black,SKColors.White };
        public static SKColor[] Level2Colors = new SKColor[] { SKColors.Purple, SKColors.Magenta, SKColors.Black,SKColors.White };
        public static SKColor[] Level3Colors = new SKColor[] { SKColors.Blue, SKColors.GreenYellow, SKColors.Black,SKColors.White };
        public static SKColor[] Level4Colors = new SKColor[] { SKColors.MediumVioletRed, SKColors.Aquamarine, SKColors.Black,SKColors.White };
        public static SKColor[] Level5Colors = new SKColor[] { SKColors.Aquamarine, SKColors.DeepSkyBlue, SKColors.Black,SKColors.White };
        public static SKColor[] Level6Colors = new SKColor[] { SKColors.Red, SKColors.SlateGray, SKColors.Black,SKColors.White };
        public static SKColor[] Level7Colors = new SKColor[] { SKColors.Indigo, SKColors.Brown, SKColors.Black,SKColors.White };
        public static SKColor[] Level8Colors = new SKColor[] { SKColors.DarkBlue, SKColors.Red, SKColors.Black,SKColors.White };

        public static SKColor[] Level9Colors = new SKColor[] { SKColors.OrangeRed, SKColors.Orange, SKColors.Black };

        //Glitch colours start at "level 138" in the NES Game.
        SKColor[][] GlitchColourSets = new SKColor[][]{
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFFFCFCFC),new SKColor(0xFFD800CC),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFFFCFCFC),new SKColor(0xFFD800CC),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFF007800),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFFF83800),new SKColor(0xFFFCFCFC),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF878F8),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFFFCE0A8),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF58F898),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFF85898)},
    new SKColor[]{new SKColor(0xFF004058),new SKColor(0xFFF87858),new SKColor(0xFF000000),new SKColor(0xFFA80020)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFAC7C00)},
    new SKColor[]{new SKColor(0xFFF0D0B0),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFFFCE0A8)},
    new SKColor[]{new SKColor(0xFF008888),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF0000FC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF503000)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFFF87858),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFFF83800)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFF004058),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFF58F898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFBCBCBC),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFF6844FC),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFF7C7C7C),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF006800),new SKColor(0xFF006800),new SKColor(0xFF000000),new SKColor(0xFF006800)},
    new SKColor[]{new SKColor(0xFFF8D8F8),new SKColor(0xFF58D854),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFF8F8F8),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFFE45C10),new SKColor(0xFF000000),new SKColor(0xFFF85898)},
    new SKColor[]{new SKColor(0xFF881400),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF00A800)},
    new SKColor[]{new SKColor(0xFF503000),new SKColor(0xFFFCFCFC),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF008888),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFF0000BC),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFE45C10)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFF58F898),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFF881400),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFF881400),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFB8F818),new SKColor(0xFFF8D878),new SKColor(0xFF000000),new SKColor(0xFFA81000)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF58D854),new SKColor(0xFF000000),new SKColor(0xFF004058)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF7C7C7C),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFF0000BC),new SKColor(0xFF0000FC),new SKColor(0xFF000000),new SKColor(0xFF0000FC)},
    new SKColor[]{new SKColor(0xFF940084),new SKColor(0xFF940084),new SKColor(0xFF000000),new SKColor(0xFF4428BC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFFA80020)},
    new SKColor[]{new SKColor(0xFF0058F8),new SKColor(0xFF3CBCFC),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFF00A800),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFD800CC),new SKColor(0xFFF878F8),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFF0058F8),new SKColor(0xFF58D854),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFE40058),new SKColor(0xFF58F898),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFF58F898),new SKColor(0xFF6888FC),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFF83800),new SKColor(0xFF7C7C7C),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFF6844FC),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFF0058F8),new SKColor(0xFFF83800),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFF83800),new SKColor(0xFFFCA044),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFFFCFCFC),new SKColor(0xFFD800CC),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFFFCFCFC),new SKColor(0xFFD800CC),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFF007800),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFFF83800),new SKColor(0xFFFCFCFC),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF878F8),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFFFCE0A8),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF58F898),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFF85898)},
    new SKColor[]{new SKColor(0xFF004058),new SKColor(0xFFF87858),new SKColor(0xFF000000),new SKColor(0xFFA80020)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFFAC7C00)},
    new SKColor[]{new SKColor(0xFFF0D0B0),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFFFCE0A8)},
    new SKColor[]{new SKColor(0xFF008888),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFFCFCFC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFFF878F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF0000FC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF503000)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFFF87858),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFFF83800)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFFF85898),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFF004058),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFF58F898),new SKColor(0xFF000000),new SKColor(0xFFF8F8F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF000000)},
    new SKColor[]{new SKColor(0xFFBCBCBC),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFF007800)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFF6844FC),new SKColor(0xFF000000),new SKColor(0xFFF87858)},
    new SKColor[]{new SKColor(0xFF000000),new SKColor(0xFF7C7C7C),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF006800),new SKColor(0xFF006800),new SKColor(0xFF000000),new SKColor(0xFF006800)},
    new SKColor[]{new SKColor(0xFFF8D8F8),new SKColor(0xFF58D854),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFF8F8F8),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFF87858),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFFE45C10),new SKColor(0xFF000000),new SKColor(0xFFF85898)},
    new SKColor[]{new SKColor(0xFF881400),new SKColor(0xFFB8F818),new SKColor(0xFF000000),new SKColor(0xFF00A800)},
    new SKColor[]{new SKColor(0xFF503000),new SKColor(0xFFFCFCFC),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF008888),new SKColor(0xFFBCBCBC),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFF0000BC),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFFE45C10)},
    new SKColor[]{new SKColor(0xFFF85898),new SKColor(0xFF58F898),new SKColor(0xFF000000),new SKColor(0xFF881400)},
    new SKColor[]{new SKColor(0xFFAC7C00),new SKColor(0xFF881400),new SKColor(0xFF000000),new SKColor(0xFFB8F818)},
    new SKColor[]{new SKColor(0xFF881400),new SKColor(0xFF007800),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFFB8F818),new SKColor(0xFFF8D878),new SKColor(0xFF000000),new SKColor(0xFFA81000)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF58D854),new SKColor(0xFF000000),new SKColor(0xFF004058)},
    new SKColor[]{new SKColor(0xFF00B800),new SKColor(0xFF000000),new SKColor(0xFF000000),new SKColor(0xFFF8D8F8)},
    new SKColor[]{new SKColor(0xFF7C7C7C),new SKColor(0xFFF8F8F8),new SKColor(0xFF000000),new SKColor(0xFF00B800)},
    new SKColor[]{new SKColor(0xFF0000FC),new SKColor(0xFF7C7C7C),new SKColor(0xFF000000),new SKColor(0xFF7C7C7C)},
    new SKColor[]{new SKColor(0xFF0000BC),new SKColor(0xFF0000FC),new SKColor(0xFF000000),new SKColor(0xFF0000FC)},
    new SKColor[]{new SKColor(0xFF940084),new SKColor(0xFF940084),new SKColor(0xFF000000),new SKColor(0xFF4428BC)},
    new SKColor[]{new SKColor(0xFFA80020),new SKColor(0xFFA80020),new SKColor(0xFF000000),new SKColor(0xFFA80020)}
};



        //level 138 starts glitch colors. because of how many there are I have separated them into individual arrays for each component, so it's easier for me to translate 


        //Level 0 style:

        public static SKColor[] AllThemeColors = new SKColor[] { SKColors.Blue, SKColors.DeepSkyBlue, SKColors.Green, SKColors.GreenYellow, SKColors.Purple, SKColors.Magenta, SKColors.MediumVioletRed, SKColors.Aquamarine, SKColors.Red, SKColors.SlateGray, SKColors.Indigo, SKColors.DarkBlue, SKColors.Orange, SKColors.OrangeRed };

        public static SKColor[] CreateHues(int Count)
        {
            double offset = 1d / (double)Count;

            return (from c in Enumerable.Range(0, Count) select SKColor.FromHsl((float)(offset * c), .8f, 0.5f)).ToArray();


        }
        public static SKColor[] CreateRandomColorSet()
        {

            var SelectedColourSet = AllThemeColors;
            if (TetrisGame.StatelessRandomizer.NextDouble() > 0.5d)
            {
                SelectedColourSet = SelectedColourSet.Concat(CreateHues(25)).ToArray();
            }


            return new SKColor[] { TetrisGame.Choose(AllThemeColors), TetrisGame.Choose(AllThemeColors), SKColors.Black };
        }
        
    }

}