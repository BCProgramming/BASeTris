using System.Collections.Generic;
using System.Drawing;
using BASeTris.Rendering.Adapters;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using SkiaSharp;
using BASeTris.GameStates.GameHandlers;
using System.Linq;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("NES Style",typeof(StandardTetrisHandler),typeof(NTrisGameHandler))]
    public class NESTetrominoTheme : CustomPixelTheme<NESTetrominoTheme.BCT, NESTetrominoTheme.NESBlockTypes>
    {
        public override string Name { get { return "NES"; } }

        private StandardNESThemeImageProvider _ThemeProvider;
        public override ThemeImageProvider ThemeProvider { get => _ThemeProvider; set => _ThemeProvider = (StandardNESThemeImageProvider)value; }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IGameCustomizationHandler GameHandler)
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
        }
        SKColor[][] LevelColorSets = new SKColor[][] { Level0Colors, Level1Colors, Level2Colors, Level3Colors, Level4Colors, Level5Colors, Level6Colors, Level7Colors, Level8Colors, Level9Colors };

      

        public override SKPointI GetBlockSize(TetrisField field, NESBlockTypes BlockType)
        {
            return new SKPointI(DarkerBlock.Length, DarkerBlock[0].Length);
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            return CustomPixelTheme<BCT, NESBlockTypes>.BlockFlags.Static;
        }
        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block,NESBlockTypes BlockType, BCT PixelType)
        {
            int LevelNum = field.Level;
            int LevelIndex = MathHelper.mod(LevelNum, AllLevelColors.Length);
            var ChosenLevelSet = LevelColorSets[LevelIndex];

            int BlockCount = 0;
            if (!(Element is Tetromino))
            {
                //get the index.
                var cw = NNominoGenerator.GetNominoPoints(Element);
                //long GetSpecialIndex = NNominoGenerator.GetIndex(cw);
                string sHash = NNominoGenerator.StringRepresentation(cw);
                if (!LookupColorSet.ContainsKey((LevelIndex,sHash)))
                {
                    var chosenresult = CreateRandomColorSet();
                    LookupColorSet[(LevelIndex,sHash)] = chosenresult;
                    var cw2 = NNominoGenerator.RotateCW(cw);
                    var cw3 = NNominoGenerator.RotateCW(cw2);
                    var cw4 = NNominoGenerator.RotateCW(cw3);
                    LookupColorSet[(LevelIndex,NNominoGenerator.StringRepresentation(cw2))] = chosenresult;
                    LookupColorSet[(LevelIndex, NNominoGenerator.StringRepresentation(cw3))] = chosenresult;
                    LookupColorSet[(LevelIndex, NNominoGenerator.StringRepresentation(cw4))] = chosenresult;
                }
                ChosenLevelSet = LookupColorSet[(LevelIndex,sHash)];
            }
                
                //select a random type


            


            switch (PixelType)
            {
                case BCT.Transparent:
                    return SKColors.Transparent;
                case BCT.Glint:
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
        private Dictionary<(int,string), SKColor[]> LookupColorSet = new Dictionary<(int,string), SKColor[]>();
        public override BlockTypeReturnData  GetBlockType(Nomino group, NominoElement element, TetrisField field)
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
                    //get the index.
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

        public static SKColor[] Level0Colors = new SKColor[] { SKColors.Blue, SKColors.DeepSkyBlue,SKColors.Black };
        public static SKColor[] Level1Colors = new SKColor[] { SKColors.Green, SKColors.GreenYellow, SKColors.Black };
        public static SKColor[] Level2Colors = new SKColor[] { SKColors.Purple, SKColors.Magenta, SKColors.Black };
        public static SKColor[] Level3Colors = new SKColor[] { SKColors.Blue, SKColors.GreenYellow, SKColors.Black };
        public static SKColor[] Level4Colors = new SKColor[] { SKColors.MediumVioletRed, SKColors.Aquamarine, SKColors.Black };
        public static SKColor[] Level5Colors = new SKColor[] { SKColors.Aquamarine, SKColors.DeepSkyBlue, SKColors.Black };
        public static SKColor[] Level6Colors = new SKColor[] { SKColors.Red, SKColors.SlateGray, SKColors.Black };
        public static SKColor[] Level7Colors = new SKColor[] { SKColors.Indigo, SKColors.Brown, SKColors.Black };
        public static SKColor[] Level8Colors = new SKColor[] { SKColors.DarkBlue, SKColors.Red, SKColors.Black };

        public static SKColor[] Level9Colors = new SKColor[] { SKColors.OrangeRed, SKColors.Orange, SKColors.Black };
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
            if (TetrisGame.rgen.NextDouble() > 0.5d)
            {
                SelectedColourSet = SelectedColourSet.Concat(CreateHues(25)).ToArray();
            }


            return new SKColor[] { TetrisGame.Choose(AllThemeColors), TetrisGame.Choose(AllThemeColors), SKColors.Black };
        }
        
    }

}