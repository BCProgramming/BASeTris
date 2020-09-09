using System;
using System.Collections.Generic;
using System.Drawing;
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
        public override String Name { get { return "NES"; } }
        public override string GetNominoKey(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {

            //Dr Mario theme keys are based on the "Duomino" arrangement. We take the first type, and the second type and create a key for it.


            if (Group is Duomino.Duomino dm)
            {

                var OneBlock = dm.FirstBlock;
                var TwoBlock = dm.SecondBlock;
                if (OneBlock.Block is LineSeriesBlock sb1 && TwoBlock.Block is LineSeriesBlock sb2)
                {
                    return "1:" + sb1.CombiningIndex + ";2:" + sb2.CombiningIndex;
                }


            }

            return base.GetNominoKey(Group, GameHandler, Field);

            //return base.GetNominoKey(Group, GameHandler, Field);
        }
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
        private static BCT[][] GetBCTBitmap(String ImageKey)
        {
            SKImage sourceBitmap = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(ImageKey));
            return GetPixelsFromSKImage(sourceBitmap, ColorMapLookupFunc);

        }
        private static readonly Dictionary<SKColor, BCT> bitmappixels = new Dictionary<SKColor, BCT>()
            {
            {SKColors.Transparent,BCT.Transparent },
            {SKColors.Black,BCT.Black },
            {SKColors.Red,BCT.Primary },
            {SKColors.Yellow,BCT.Accent },
            {SKColors.Cyan,BCT.Accent2}
            };
        private static BCT ColorMapLookupFunc(SKColor Src)
        {
            if (bitmappixels.ContainsKey(Src)) return bitmappixels[Src]; else return BCT.Transparent;
        }
        private static bool ThemeDataPrepared = false;
        private static void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            Pill_Left = GetBCTBitmap("pill_left");
            Pill_Single = GetBCTBitmap("pill_single");
            Pill_Right = GetBCTBitmap("pill_right");
            Pill_Pop = GetBCTBitmap("pill_pop");
            Red_Virii_1 = GetBCTBitmap("red_virus_1");
            Red_Virii_2 = GetBCTBitmap("red_virus_2");
            Yellow_Virii_1 = GetBCTBitmap("yellow_virus_1");
            Yellow_Virii_2 = GetBCTBitmap("yellow_virus_2");
            Blue_Virii_1 = GetBCTBitmap("blue_virus_1");
            Blue_Virii_2 = GetBCTBitmap("blue_virus_2");
            Green_Virii_1 = GetBCTBitmap("green_virus_1");
            Green_Virii_2 = GetBCTBitmap("green_virus_2");
            BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
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
            {BlockTypes.Green_Virus_1,Green_Virii_1 },
            {BlockTypes.Green_Virus_2,Green_Virii_2 },
            {BlockTypes.Red_Pop,Pill_Pop},
            {BlockTypes.Blue_Pop,Pill_Pop},
            {BlockTypes.Yellow_Pop,Pill_Pop}


        };


            ThemeDataPrepared = true;
        }
        public DrMarioTheme()
        {
            PrepareThemeData();
        }
        static BCT[][] Pill_Left; /*= new BCT[][]
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
                    };*/
                                  //Bitmap for the left side of a pill.

        static BCT[][] Pill_Right; /*= new BCT[][]
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
                    };*/
        //bitmap for the right side of a pill.
       
        //note: pill rotations should be interesting, to get the "glint" to rotate correctly. though initially I think we can just have the glint rotate, too.
        static BCT[][] Pill_Single = null;
        //TODO: BCT Bitmaps for a Single pill piece, as well as the Red,Blue, and Yellow Virii.
        static BCT[][] Pill_Pop;
            //Frame 1 Yellow Virii
        static BCT[][] Yellow_Virii_1 = new BCT[][]
               {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Transparent, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Transparent , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black , BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Accent, BCT.Black , BCT.Accent, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent }
               };
        //Frame 2 Yellow Virii unchanged currently...
        static BCT[][] Yellow_Virii_2 = new BCT[][]
               {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Black , BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Accent2, BCT.Primary, BCT.Primary,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Accent2, BCT.Primary, BCT.Transparent, BCT.Transparent }
               };

        static BCT[][] Red_Virii_1 = new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent,BCT.Accent},
                new BCT[]{BCT.Transparent, BCT.Accent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Black, BCT.Accent2, BCT.Black , BCT.Accent2, BCT.Black , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Accent2}
                
              };
        static BCT[][] Red_Virii_2 = new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Accent, BCT.Transparent , BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Accent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Black, BCT.Accent2, BCT.Black , BCT.Accent2, BCT.Black , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Accent2},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent}

              };
        static BCT[][] Blue_Virii_1 = new BCT[][]
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
        static BCT[][] Blue_Virii_2 = new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Primary, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary,BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Transparent,BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Accent, BCT.Black, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Transparent}

              };
        static BCT[][] Green_Virii_1 = null;
        static BCT[][] Green_Virii_2 = null;
        static Dictionary<BlockTypes, BCT[][]> BitmapIndex = null;
        static SKPointI nine = new SKPointI(9, 9);
        static SKPointI eight = new SKPointI(8, 8);
        static SKPointI doublenine = new SKPointI(18, 18);
        public static Dictionary<BlockTypes, SKPointI> TypeSizes = new Dictionary<BlockTypes, SKPointI>()
        {

            {BlockTypes.Pill_Left_Yellow,doublenine},
            {BlockTypes.Pill_Right_Yellow,doublenine },
            {BlockTypes.Pill_Single_Yellow,doublenine},
            {BlockTypes.Pill_Left_Red,doublenine },
            {BlockTypes.Pill_Right_Red,doublenine},
            {BlockTypes.Pill_Single_Red,doublenine},
            {BlockTypes.Pill_Left_Blue,doublenine},
            {BlockTypes.Pill_Right_Blue,doublenine},
            {BlockTypes.Pill_Single_Blue,doublenine},
            {BlockTypes.Blue_Virus_1,nine },
            {BlockTypes.Blue_Virus_2,nine},
            {BlockTypes.Yellow_Virus_1,nine},
            {BlockTypes.Yellow_Virus_2,nine},
            {BlockTypes.Red_Virus_1,nine },
            {BlockTypes.Red_Virus_2,nine},
            {BlockTypes.Green_Virus_1,nine},
            {BlockTypes.Green_Virus_2,nine},
            {BlockTypes.Red_Pop,doublenine},
            {BlockTypes.Yellow_Pop,doublenine},
            {BlockTypes.Blue_Pop,doublenine}
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
        Dictionary<BCT, SKColor> GreenColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Green  },
            {BCT.Accent,SKColors.GreenYellow },
            {BCT.Accent2,SKColors.DarkMagenta }

        };
        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            return TypeSizes[BlockType];
        }
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PopTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Pop },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Pop},
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Pop }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> VirusTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Virus_1 }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> VirusTypes_2 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Virus_2 }
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
        
        protected override SKImage[] ApplyFunc_Custom(TetrisField field, Nomino Group, NominoBlock Target,BlockTypes chosentype)
        {
            var getElement = Group.FindEntry(Target);
            if(Target is LineSeriesBlock lsb)
            {
                if(lsb.Popping)
                {
                    if (Target is LineSeriesMasterBlock)
                    {
                        ;
                    }
                    BlockTypes useType  = BlockTypes.Yellow_Pop;
                    if (chosentype == BlockTypes.Blue_Virus_1 || chosentype == BlockTypes.Blue_Virus_2 || chosentype == BlockTypes.Blue_Pop)
                        useType = chosentype = BlockTypes.Blue_Pop;
                    else if (chosentype == BlockTypes.Yellow_Virus_1 || chosentype == BlockTypes.Yellow_Virus_2 || chosentype==BlockTypes.Yellow_Pop)
                        useType = chosentype = BlockTypes.Yellow_Pop;
                    else if (chosentype == BlockTypes.Red_Virus_1 || chosentype == BlockTypes.Red_Virus_2 || chosentype==BlockTypes.Red_Pop)
                        useType = chosentype = BlockTypes.Red_Pop;


                    
                    lsb.SpecialImageFunctionSK = null;
                    lsb._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetMappedImageSkia(field, Group, getElement, useType)) };
                    return lsb._RotationImagesSK;
                }
            }
            
            BlockTypes usetype1= BlockTypes.Yellow_Virus_1, usetype2 = BlockTypes.Yellow_Virus_2;
            if(chosentype == BlockTypes.Blue_Virus_1 || chosentype==BlockTypes.Blue_Virus_2)
            {
                usetype1 = BlockTypes.Blue_Virus_1;
                usetype2 = BlockTypes.Blue_Virus_2;
            }
            else if (chosentype == BlockTypes.Red_Virus_1 || chosentype == BlockTypes.Red_Virus_2)
            {
                usetype1 = BlockTypes.Red_Virus_1;
                usetype2 = BlockTypes.Red_Virus_2;
            }
            else if(chosentype==BlockTypes.Yellow_Virus_1 || chosentype == BlockTypes.Yellow_Virus_2)
            {
                usetype1 = BlockTypes.Yellow_Virus_1;
                usetype2 = BlockTypes.Yellow_Virus_2;
            }
            else if (chosentype == BlockTypes.Green_Virus_1 || chosentype == BlockTypes.Green_Virus_2)
            {
                usetype1 = BlockTypes.Green_Virus_1;
                usetype2 = BlockTypes.Green_Virus_2;
            }
            
            var firstImage = SKImage.FromBitmap(GetMappedImageSkia(field, Group,getElement, usetype1));
            var secondImage = SKImage.FromBitmap(GetMappedImageSkia(field, Group,getElement, usetype2));
            foreach(var iterate in Group)
            {
                if (iterate.Block is ImageBlock ib)
                {
                    ib.SpecialImageFunctionSK = (a) =>
                    {
                        int useindex = ((DateTime.Now.Millisecond / 250) + 2) % 2 == 0 ? 0 : 1;
                        return (a as ImageBlock)._RotationImagesSK[useindex];
                    };
                }
            }

            return new SKImage[] { firstImage, secondImage };
            
        }
        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb)
            {
                if(lsb.Popping)
                {
                    if(element.Block is LineSeriesMasterBlock lsm2)
                    {
                        ;
                    }
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                }
                if (element.Block is LineSeriesMasterBlock lsm)
                {
                    if (lsb.Popping)
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                    }
                    if (DateTime.Now.Millisecond <500)
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(VirusTypes_1[lsm.CombiningIndex],true);
                    else
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(VirusTypes_2[lsm.CombiningIndex], true);
                    }

                }

                else if (group == null || group.Count()==1)
                {
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillSingle[lsb.CombiningIndex]);
                    
                }
                else if (group.Count() == 2)
                {
                    int index = group.IndexOf(element);
                    if (index == 0) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillLeft[lsb.CombiningIndex]);
                    else if (index == 1) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillRight[lsb.CombiningIndex]);
                }
            }
            return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(BlockTypes.Yellow_Virus_1);
        }

        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            return BitmapIndex;
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, BlockTypes BlockType, BCT PixelType)
        {
            if (BlockType == BlockTypes.Yellow_Virus_1 || BlockType == BlockTypes.Yellow_Virus_2)
                return YellowColourSet[PixelType];
            else if (BlockType == BlockTypes.Red_Virus_1 || BlockType == BlockTypes.Red_Virus_2)
                return RedColourSet[PixelType];
            else if (BlockType == BlockTypes.Blue_Virus_1 || BlockType == BlockTypes.Blue_Virus_2)
                return BlueColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Yellow, BlockTypes.Pill_Right_Yellow, BlockTypes.Pill_Single_Yellow, BlockTypes.Yellow_Pop }.Contains(BlockType))
                return YellowColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Red, BlockTypes.Pill_Right_Red, BlockTypes.Pill_Single_Red, BlockTypes.Red_Pop }.Contains(BlockType))
                return RedColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Pill_Left_Blue, BlockTypes.Pill_Right_Blue, BlockTypes.Pill_Single_Blue, BlockTypes.Blue_Pop }.Contains(BlockType))
                return BlueColourSet[PixelType];
            else if (new BlockTypes[] { BlockTypes.Green_Virus_1, BlockTypes.Green_Virus_2 }.Contains(BlockType))
                return GreenColourSet[PixelType];
            return SKColors.Magenta;
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            if(testvalue.Block is LineSeriesMasterBlock)
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.CustomSelector;
            }
            else
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
            }
            
        }
        public override bool IsAnimated(NominoBlock block)
        {
            return (block is LineSeriesMasterBlock);
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
            BlockTypes.Blue_Virus_2,
            BlockTypes.Red_Pop,
            BlockTypes.Yellow_Pop,
            BlockTypes.Blue_Pop,
            BlockTypes.Green_Virus_1,
            BlockTypes.Green_Virus_2
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
            Blue_Virus_2,
            Green_Virus_1,
            Green_Virus_2,
            Red_Pop,
            Yellow_Pop,
            Blue_Pop
            
        }

    }
}
