using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia;
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
        static SKImage SNES_Red_Normal = null;
        static SKImage SNES_Red_Fixed = null;
        static SKImage SNES_Red_Pop = null;
        static SKImage SNES_Yellow_Normal = null;
        static SKImage SNES_Yellow_Fixed = null;
        static SKImage SNES_Yellow_Pop = null;
        static SKImage SNES_Blue_Normal = null;
        static SKImage SNES_Blue_Fixed = null;
        static SKImage SNES_Blue_Pop = null;
        static SKImage SNES_Green_Normal = null;
        static SKImage SNES_Green_Fixed = null;
        static SKImage SNES_Green_Pop = null;
        static SKImage SNES_Magenta_Normal = null;
        static SKImage SNES_Magenta_Fixed = null;
        static SKImage SNES_Magenta_Pop = null;
        static SKImage SNES_Orange_Normal = null;
        static SKImage SNES_Orange_Fixed = null;
        static SKImage SNES_Orange_Pop = null;

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

        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            PrepareThemeData();
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
                    ibb._RotationImagesSK = new SKImage[] { Sourcedict[chosenType] };
                    
                }


            }
        }
        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_3", 0.5f], Color.Transparent);
        }

        private void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            ThemeDataPrepared = true;
            SNES_Red_Normal = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));
            SNES_Red_Fixed = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_fixed_snes"));
            SNES_Red_Pop = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_pop_snes"));
            SNES_Yellow_Normal = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Normal, SKColors.Yellow);
            SNES_Yellow_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Yellow);
            SNES_Yellow_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Yellow);
            SNES_Blue_Normal = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Normal, SKColors.Blue);
            SNES_Blue_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Blue);
            SNES_Blue_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Blue);
            SNES_Green_Normal = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Normal, SKColors.Green);
            SNES_Green_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Green);
            SNES_Green_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Green);
            SNES_Magenta_Normal =TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Normal, SKColors.Magenta);
            SNES_Magenta_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Magenta);
            SNES_Magenta_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Magenta);
            SNES_Orange_Normal = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Normal, SKColors.Orange);
            SNES_Orange_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Orange);
            SNES_Orange_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Orange);
            NormalBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, SKImage>()
            {
                {LineSeriesBlock.CombiningTypes.Red,SNES_Red_Normal },
                {LineSeriesBlock.CombiningTypes.Yellow,SNES_Yellow_Normal },
                {LineSeriesBlock.CombiningTypes.Blue,SNES_Blue_Normal },
                {LineSeriesBlock.CombiningTypes.Green,SNES_Green_Normal },
                {LineSeriesBlock.CombiningTypes.Magenta,SNES_Magenta_Normal },
                {LineSeriesBlock.CombiningTypes.Orange,SNES_Orange_Normal },
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
