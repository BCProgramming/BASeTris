using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BASeTris.Theme.Block
{
    [HandlerTheme("Tetris 2 SNES", typeof(Tetris2Handler), typeof(DrMarioHandler))]
    [ThemeDescription("Tetris 2 Theme from the SNES")]
    public class SNESTetris2Theme : NominoTheme
    {
        public enum BlockTypes
        {
            Normal,
            Fixed,
            Pop,
            Shiny
        }
        
 

        static CardinalImageSet SNES_Red_Normal = null;
        static SKImage SNES_Red_Fixed = null;
        static SKImage SNES_Red_Pop = null;
        static CardinalImageSet SNES_Yellow_Normal = null;
        static SKImage SNES_Yellow_Fixed = null;
        static SKImage SNES_Yellow_Pop = null;
        static CardinalImageSet SNES_Blue_Normal = null;
        static SKImage SNES_Blue_Fixed = null;
        static SKImage SNES_Blue_Pop = null;
        static CardinalImageSet SNES_Green_Normal = null;
        static SKImage SNES_Green_Fixed = null;
        static SKImage SNES_Green_Pop = null;
        static CardinalImageSet SNES_Magenta_Normal = null;
        static SKImage SNES_Magenta_Fixed = null;
        static SKImage SNES_Magenta_Pop = null;
        static CardinalImageSet SNES_Orange_Normal = null;
        static SKImage SNES_Orange_Fixed = null;
        static SKImage SNES_Orange_Pop = null;
        static Dictionary<LineSeriesBlock.CombiningTypes, CardinalImageSet> NormalConnectedBlocks = null;
        static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> NormalBlocks = null;
        static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> FixedBlocks = null;
        static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> ShinyBlocks = null;
        static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> PopBlocks = null;
        static bool ThemeDataPrepared = false;

        public override string Name => "Tetris 2 SNES";

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            PrepareThemeData();
            var Sourcedict = TetrisGame.Choose(new[] { NormalBlocks, FixedBlocks });
            foreach (var iterate in Group)
            {
                var chosenType = TetrisGame.Choose<LineSeriesBlock.CombiningTypes>((LineSeriesBlock.CombiningTypes[])Enum.GetValues(typeof(LineSeriesBlock.CombiningTypes)));

                if (iterate.Block is ImageBlock ibb)
                {
                    if (ibb is StandardColouredBlock sbc)
                    {
                        sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }
                    
                    ibb._RotationImagesSK = new SKImage[] { Sourcedict[chosenType] };
                }
            }
        }
        //before I go and forget, I'll tag in a comment here that this seems to have glitches related to the new resurrection code, that creates new nominoes, as we are seeing old behaviour where the connections don't quite reflect what the nomino in question should look like.
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            PrepareThemeData();
            if (Reason == ThemeApplicationReason.FieldSet)
            {
                
                ;
            }
            Dictionary<Point, NominoElement> GroupElements = (from g in Group select g).ToDictionary((ne) => new Point(ne.BaseX(), ne.BaseY()));
            foreach (var iterate in Group)
            {
                Dictionary<LineSeriesBlock.CombiningTypes, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes chosenType;
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    chosenType = lsb.CombiningIndex;
                    if (lsb.Popping)
                    {
                        Sourcedict = PopBlocks;
                    }
                    else
                    {
                        Sourcedict = NormalBlocks;

                        if (lsb is LineSeriesPrimaryBlock lsbp)
                        {

                            if (lsbp is LineSeriesPrimaryShinyBlock)
                            {
                                Sourcedict = ShinyBlocks;
                            }
                            else
                            {
                                Sourcedict = FixedBlocks;
                            }
                        }
                    }
                }
                else {
                    chosenType = TetrisGame.Choose<LineSeriesBlock.CombiningTypes>((LineSeriesBlock.CombiningTypes[])Enum.GetValues(typeof(LineSeriesBlock.CombiningTypes)));
                }

                if (iterate.Block is ImageBlock ibb)
                {
                    if (iterate.Block is StandardColouredBlock scb)
                    {
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }

                    if (Sourcedict == NormalBlocks && UseConnectedImages)
                    {

                        //determine the flags by checking the Nomino.
                        CardinalConnectionSet.ConnectedStyles cs = CardinalConnectionSet.ConnectedStyles.None;
                        //check above
                        Point North = new Point(iterate.BaseX(), iterate.BaseY()-1);
                        Point South = new Point(iterate.BaseX(), iterate.BaseY() + 1);
                        Point West = new Point(iterate.BaseX() - 1, iterate.BaseY());
                        Point East = new Point(iterate.BaseX() + 1, iterate.BaseY());

                        Point[] DirectionPoints = new Point[] { North, South, West, East };
                        List<(Point, CardinalConnectionSet.ConnectedStyles)> Setuplist = new List<(Point, CardinalConnectionSet.ConnectedStyles)>()
                        {
                            (DirectionPoints[0],CardinalConnectionSet.ConnectedStyles.North),
                            (DirectionPoints[2],CardinalConnectionSet.ConnectedStyles.West),
                            (DirectionPoints[1],CardinalConnectionSet.ConnectedStyles.South),
                            (DirectionPoints[3],CardinalConnectionSet.ConnectedStyles.East)

                        };
                        foreach (var Checkconnected in Setuplist)
                        {
                            if (GroupElements.ContainsKey(Checkconnected.Item1))
                            {
                                if (!VisuallyConnectOnlySameCombiningType ||( GroupElements[Checkconnected.Item1].Block is LineSeriesBlock lsbg && iterate.Block is LineSeriesBlock lsbb))
                                {
                                    cs |= Checkconnected.Item2;
                                }
                            }
                        }
                        CardinalConnectionSet.ConnectedStyles[] useConnectionStyles = CardinalConnectionSet.GetRotations(cs).Prepend(cs).ToArray();

                        var useRotationImages = from c in useConnectionStyles select NormalConnectedBlocks[chosenType][c];
                        var useImage = NormalConnectedBlocks[chosenType][cs];
                        ibb._RotationImagesSK = useRotationImages.ToArray();//GetImageRotations(SKBitmap.FromImage(useImage));
                        //ibb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(useImage));


                    }
                    else
                    {
                        ibb._RotationImagesSK = new SKImage[] { Sourcedict[chosenType] };
                    }
                    
                }


            }
        }
        private bool VisuallyConnectOnlySameCombiningType = false;
        private bool UseConnectedImages = true;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_3", 0.5f], Color.Transparent);
        }

        private void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            ThemeDataPrepared = true;
            //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
            SNES_Red_Normal = new CardinalImageSet();
            SNES_Red_Normal[CardinalConnectionSet.ConnectedStyles.None] = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));

            var AllArrangements = EnumHelper.GetAllEnums<CardinalConnectionSet.ConnectedStyles>();
            String NormalBlockPrefix = "tetris_2_normal_block_connected";
            foreach (var checkflags in AllArrangements)
            {
                String useSuffix = CardinalConnectionSet.GetSuffix(checkflags);
                if (useSuffix != null)
                {
                    String sFindImage = NormalBlockPrefix + (useSuffix.Length>0?"_":" ") + useSuffix;
                    try
                    {
                        if (TetrisGame.Imageman.HasSKBitmap(sFindImage))
                            {
                            var getimage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(sFindImage));
                            SNES_Red_Normal[checkflags] = getimage;
                        }
                    }
                    finally
                    {
                    }


                }
            }


            //SNES_Red_Normal = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));
            SNES_Red_Fixed = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_fixed_snes"));
            SNES_Red_Pop = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_pop_snes"));
            SNES_Yellow_Normal =  new CardinalImageSet(SNES_Red_Normal,SKColors.Yellow);
            SNES_Yellow_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Yellow);
            SNES_Yellow_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Yellow);
            SNES_Blue_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Blue);
            SNES_Blue_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Blue);
            SNES_Blue_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Blue);
            SNES_Green_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Green) ;
            SNES_Green_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Green);
            SNES_Green_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Green);
            SNES_Magenta_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Magenta);
            SNES_Magenta_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Magenta);
            SNES_Magenta_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Magenta);
            SNES_Orange_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Orange);
            SNES_Orange_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Orange);
            SNES_Orange_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Orange);

            NormalConnectedBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, CardinalImageSet>()
            {
                {LineSeriesBlock.CombiningTypes.Red,SNES_Red_Normal },
                {LineSeriesBlock.CombiningTypes.Yellow,SNES_Yellow_Normal },
                {LineSeriesBlock.CombiningTypes.Blue,SNES_Blue_Normal},
                {LineSeriesBlock.CombiningTypes.Green,SNES_Green_Normal },
                {LineSeriesBlock.CombiningTypes.Magenta,SNES_Magenta_Normal },
                {LineSeriesBlock.CombiningTypes.Orange,SNES_Orange_Normal },
            };
            NormalBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, SKImage>()
            {
                {LineSeriesBlock.CombiningTypes.Red,SNES_Red_Normal[0] },
                {LineSeriesBlock.CombiningTypes.Yellow,SNES_Yellow_Normal[0] },
                {LineSeriesBlock.CombiningTypes.Blue,SNES_Blue_Normal[0] },
                {LineSeriesBlock.CombiningTypes.Green,SNES_Green_Normal[0] },
                {LineSeriesBlock.CombiningTypes.Magenta,SNES_Magenta_Normal[0] },
                {LineSeriesBlock.CombiningTypes.Orange,SNES_Orange_Normal[0] },
            };
            FixedBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, SKImage>()
            {
                {LineSeriesBlock.CombiningTypes.Red,SNES_Red_Fixed },
                {LineSeriesBlock.CombiningTypes.Yellow,SNES_Yellow_Fixed },
                {LineSeriesBlock.CombiningTypes.Blue,SNES_Blue_Fixed },
                {LineSeriesBlock.CombiningTypes.Green,SNES_Green_Fixed },
                {LineSeriesBlock.CombiningTypes.Magenta,SNES_Magenta_Fixed },
                {LineSeriesBlock.CombiningTypes.Orange,SNES_Orange_Fixed },
            };

            PopBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, SKImage>()
            {
                {LineSeriesBlock.CombiningTypes.Red,SNES_Red_Pop },
                {LineSeriesBlock.CombiningTypes.Yellow,SNES_Yellow_Pop },
                {LineSeriesBlock.CombiningTypes.Blue,SNES_Blue_Pop },
                {LineSeriesBlock.CombiningTypes.Green,SNES_Green_Pop },
                {LineSeriesBlock.CombiningTypes.Magenta,SNES_Magenta_Pop },
                {LineSeriesBlock.CombiningTypes.Orange,SNES_Orange_Pop },
            };


            ShinyBlocks = FixedBlocks;

        }

    }
}
