using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BASeTris.Theme.Block
{

    public class CachedImageData
    {
        public enum BlockTypeConstants
        {
            Normal,
            Fixed,
            Shiny,
            Pop
        }

        public Dictionary<SKColor, CardinalImageSet> NormalConnectedBlocks_Color = new Dictionary<SKColor, CardinalImageSet>();
        public Dictionary<SKColor, SKImage> NormalBlocks_Color = new Dictionary<SKColor, SKImage>();
        public Dictionary<SKColor, SKImage> FixedBlocks_Color = new Dictionary<SKColor, SKImage>();
        public Dictionary<SKColor, SKImage> ShinyBlocks_Color = new Dictionary<SKColor, SKImage>();
        public Dictionary<SKColor, SKImage> PopBlocks_Color = new Dictionary<SKColor, SKImage>();


        public CardinalImageSet GetConnectedBlocks(SKColor src)
        {
            if (!NormalConnectedBlocks_Color.ContainsKey(src))
            {
                //red must be added first!
                var redSet = NormalConnectedBlocks_Color[SKColors.Red];
                CardinalImageSet newSet = new CardinalImageSet(redSet, src);
                NormalConnectedBlocks_Color[src] = newSet;
            }
            return NormalConnectedBlocks_Color[src];
        }
        public SKImage GetNormalBlock(SKColor src)
        {
            return GetBlockFromDictionary(NormalBlocks_Color, src);
        }
        public SKImage GetFixedBlock(SKColor src)
        {
            return GetBlockFromDictionary(FixedBlocks_Color, src);
        }
        public SKImage GetShinyBlock(SKColor src)
        {
            return GetBlockFromDictionary(ShinyBlocks_Color, src);
        }
        public SKImage GetPopBlock(SKColor src)
        {
            return GetBlockFromDictionary(PopBlocks_Color, src);
        }
        private SKImage GetBlockFromDictionary(Dictionary<SKColor,SKImage> Input, SKColor src)
        {
            if (!Input.ContainsKey(src))
            {
                var redImage = Input[SKColors.Red];
                var recolored = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(redImage, src);
                Input.Add(src, recolored);
            }
            return Input[src];
        }
        public SKImage GetBlock(BlockTypeConstants btc, SKColor color)
        {
            return GetBlockFromDictionary(GetDictionaryForType(btc), color);
        }
        public Dictionary<SKColor, SKImage> GetDictionaryForType(BlockTypeConstants bt)
        {
            return bt switch
            {
                BlockTypeConstants.Normal => NormalBlocks_Color,
                BlockTypeConstants.Pop => PopBlocks_Color,
                BlockTypeConstants.Fixed => FixedBlocks_Color,
                BlockTypeConstants.Shiny => ShinyBlocks_Color,
                _ => null
            };
        }
        //public Dictionary<LineSeriesBlock.CombiningTypes, CardinalImageSet> NormalConnectedBlocks_Combining = null;
        //public Dictionary<LineSeriesBlock.CombiningTypes, SKImage> NormalBlocks_Combine = null;
        //public Dictionary<LineSeriesBlock.CombiningTypes, SKImage> FixedBlocks_Combine = null;
        //public Dictionary<LineSeriesBlock.CombiningTypes, SKImage> ShinyBlocks_Combine = null;
        //public Dictionary<LineSeriesBlock.CombiningTypes, SKImage> PopBlocks_Combine = null;
    }
    [HandlerTheme("Tetris 3 SNES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler))]
    [ThemeDescription("Tetris 3 Theme from the SNES")]

    public class SNESTetris3Theme : ConnectedImageLineSeriesBlockTheme
    {
        public override string Name => "Tetris 3 SNES";
        protected override string GetImageKeyBase()
        {
            return "Tetris_3";
        }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
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
    }



    [HandlerTheme("Tetris 2 NES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler))]
    [ThemeDescription("Tetris 2 Theme from the NES")]

    public class NESTetris2Theme : ConnectedImageLineSeriesBlockTheme
    {
        public override string Name => "Tetris 2 NES";
        protected override string GetImageKeyBase()
        {
            return "tetris_2_NES";
        }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
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
    }


    [HandlerTheme("Tetris 2 SNES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris 2 Theme from the SNES")]

    public class SNESTetris2Theme : ConnectedImageLineSeriesBlockTheme
    {
        public override string Name => "Tetris 2 SNES";
        protected override string GetImageKeyBase()
        {
            return "tetris_2";
        }
    }
    public abstract class ConnectedImageLineSeriesBlockTheme: NominoTheme
    {
        CardinalImageSet SNES_Red_Normal = null;
        SKImage SNES_Red_Fixed = null;
        SKImage SNES_Red_Pop = null;
        CardinalImageSet SNES_Yellow_Normal = null;
        SKImage SNES_Yellow_Fixed = null;
        SKImage SNES_Yellow_Pop = null;
        CardinalImageSet SNES_Blue_Normal = null;
        SKImage SNES_Blue_Fixed = null;
        SKImage SNES_Blue_Pop = null;
        CardinalImageSet SNES_Green_Normal = null;
        SKImage SNES_Green_Fixed = null;
        SKImage SNES_Green_Pop = null;
        CardinalImageSet SNES_Magenta_Normal = null;
        SKImage SNES_Magenta_Fixed = null;
        SKImage SNES_Magenta_Pop = null;
        CardinalImageSet SNES_Orange_Normal = null;
        SKImage SNES_Orange_Fixed = null;
        SKImage SNES_Orange_Pop = null;
        CachedImageData ImageCache = new CachedImageData();
        //static Dictionary<LineSeriesBlock.CombiningTypes, CardinalImageSet> NormalConnectedBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> NormalBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> FixedBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> ShinyBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> PopBlocks = null;
        bool ThemeDataPrepared = false;

        public override string Name => "ConnectedBlockTheme";

        
        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            PrepareThemeData();
            var drawtype = RandomHelpers.Static.Select(new[] { CachedImageData.BlockTypeConstants.Fixed, CachedImageData.BlockTypeConstants.Normal, CachedImageData.BlockTypeConstants.Pop }, new float[] { 20, 60, 5 });
            
            foreach (var iterate in Group)
            {
                var chosenColor = new SKColor((byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255));
               
                
                if (iterate.Block is ImageBlock ibb)
                {
                    if (ibb is StandardColouredBlock sbc)
                    {
                        sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }
                    
                    ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock(drawtype,chosenColor) };
                }
            }
        }

        Dictionary<String, SKColor> ChosenNominoColours = new Dictionary<string, SKColor>();
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
                CachedImageData.BlockTypeConstants blocktype = CachedImageData.BlockTypeConstants.Normal;
                //Dictionary<SKColor, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes? chosenType = null;
                SKColor useColor = SKColors.Red;
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    chosenType = lsb.CombiningIndex;
                    
                    if (lsb.Popping)
                    {
                        blocktype = CachedImageData.BlockTypeConstants.Pop;
                        //Sourcedict = ImageCache.PopBlocks_Color;
                    }
                    else
                    {
                        blocktype = CachedImageData.BlockTypeConstants.Normal;

                        if (lsb is LineSeriesPrimaryBlock lsbp)
                        {

                            if (lsbp is LineSeriesPrimaryShinyBlock)
                            {
                                blocktype = CachedImageData.BlockTypeConstants.Shiny;
                                
                            }
                            else
                            {
                                blocktype = CachedImageData.BlockTypeConstants.Fixed;
                            }
                        }
                    }
                }
                else {
                    //I,O,T,S,Z,J,L
                    //{Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Red, Color.Blue, Color.OrangeRed};
                    //chosen color based on the type.
                    if (Group is Tetromino_I) useColor = SKColors.Cyan;
                    else if (Group is Tetromino_O) useColor = SKColors.Yellow;
                    else if (Group is Tetromino_T) useColor = SKColors.Purple;
                    else if (Group is Tetromino_S) useColor = SKColors.Green;
                    else if (Group is Tetromino_Z) useColor = SKColors.Red;
                    else if (Group is Tetromino_J) useColor = SKColors.Navy;
                    else if (Group is Tetromino_L) useColor = SKColors.OrangeRed;
                    else {
                        useColor = NNominoGenerator.GetNominoData<SKColor>(ChosenNominoColours, Group, () => RandomColor());
                    }
                    //else useColor = SKColors.Gray;



                    //chosenType = TetrisGame.Choose<LineSeriesBlock.CombiningTypes>((LineSeriesBlock.CombiningTypes[])Enum.GetValues(typeof(LineSeriesBlock.CombiningTypes)));
                }
                if(chosenType!=null) useColor = LineSeriesBlock.GetCombiningTypeColor(chosenType.Value);
                if (iterate.Block is ImageBlock ibb)
                {
                    if (iterate.Block is StandardColouredBlock scb)
                    {
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }

                    if (blocktype == CachedImageData.BlockTypeConstants.Normal  && UseConnectedImages)
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
                                if (!VisuallyConnectOnlySameCombiningType || (GroupElements[Checkconnected.Item1].Block is LineSeriesBlock lsbg && iterate.Block is LineSeriesBlock lsbb))
                                {
                                    cs |= Checkconnected.Item2;
                                }
                                else
                                {
                                    cs |= Checkconnected.Item2;
                                }
                            }
                        }
                        CardinalConnectionSet.ConnectedStyles[] useConnectionStyles = CardinalConnectionSet.GetRotations(cs).Prepend(cs).ToArray();

                        var useRotationImages = from c in useConnectionStyles select ImageCache.GetConnectedBlocks(useColor)[c];
                        var useImage = ImageCache.GetConnectedBlocks(useColor)[cs];
                        ibb._RotationImagesSK = useRotationImages.ToArray();//GetImageRotations(SKBitmap.FromImage(useImage));
                        //ibb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(useImage));


                    }
                    else
                    {
                        ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock(blocktype,useColor) };
                    }
                    
                }


            }
        }
        private SKColor RandomColor()
        {
            return new SKColor((byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256));
        }
        private bool VisuallyConnectOnlySameCombiningType = false;
        private bool UseConnectedImages = true;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_4", 0.5f], Color.Transparent);
        }
        protected abstract String GetImageKeyBase();

        private void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            ThemeDataPrepared = true;
            //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
            SNES_Red_Normal = new CardinalImageSet();
            SNES_Red_Normal[CardinalConnectionSet.ConnectedStyles.None] = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase()+ "_normal"));

            
            var AllArrangements = EnumHelper.GetAllEnums<CardinalConnectionSet.ConnectedStyles>();
            String NormalBlockPrefix =  GetImageKeyBase() + "_normal_block_connected";
            foreach (var checkflags in AllArrangements)
            {
                String useSuffix = CardinalConnectionSet.GetSuffix(checkflags);
                if (useSuffix != null)
                {
                    String sFindImage = NormalBlockPrefix + (useSuffix.Length>0?"_":"") + useSuffix;
                    try
                    {
                        if (TetrisGame.Imageman.HasSKBitmap(sFindImage))
                        {
                            var getimage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(sFindImage));
                            SNES_Red_Normal[checkflags] = getimage;
                        }
                        else
                        {
                            SNES_Red_Normal[checkflags] = SNES_Red_Normal[CardinalConnectionSet.ConnectedStyles.None];
                        }
                    }
                    finally
                    {
                    }


                }
            }
            ImageCache.NormalConnectedBlocks_Color.Add(SKColors.Red,SNES_Red_Normal);

            //SNES_Red_Normal = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));
            SNES_Red_Fixed = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_fixed"));
            SNES_Red_Pop = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap( GetImageKeyBase() + "_pop"));

            


            SNES_Yellow_Normal =  new CardinalImageSet(SNES_Red_Normal,SKColors.Yellow);
            SNES_Yellow_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Yellow);
            SNES_Yellow_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Yellow);
            SNES_Blue_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Cyan);
            SNES_Blue_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Cyan);
            SNES_Blue_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Cyan);
            SNES_Green_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Green) ;
            SNES_Green_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Green);
            SNES_Green_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Green);
            SNES_Magenta_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Magenta);
            SNES_Magenta_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Magenta);
            SNES_Magenta_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Magenta);
            SNES_Orange_Normal = new CardinalImageSet(SNES_Red_Normal, SKColors.Orange);
            SNES_Orange_Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Fixed, SKColors.Orange);
            SNES_Orange_Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(SNES_Red_Pop, SKColors.Orange);

            ImageCache.NormalConnectedBlocks_Color = new Dictionary<SKColor, CardinalImageSet>()
            {
                {SKColors.Red,SNES_Red_Normal },
                {SKColors.Yellow,SNES_Yellow_Normal },
                {SKColors.Blue,SNES_Blue_Normal},
                {SKColors.Green,SNES_Green_Normal },
                {SKColors.Magenta,SNES_Magenta_Normal },
                {SKColors.Orange,SNES_Orange_Normal },
            };
            ImageCache.NormalBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,SNES_Red_Normal[0] },
                {SKColors.Yellow,SNES_Yellow_Normal[0] },
                {SKColors.Blue,SNES_Blue_Normal[0] },
                {SKColors.Green,SNES_Green_Normal[0] },
                {SKColors.Magenta,SNES_Magenta_Normal[0] },
                {SKColors.Orange,SNES_Orange_Normal[0] },
            };
            ImageCache.FixedBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,SNES_Red_Fixed },
                {SKColors.Yellow,SNES_Yellow_Fixed },
                {SKColors.Blue,SNES_Blue_Fixed },
                {SKColors.Green,SNES_Green_Fixed },
                {SKColors.Magenta,SNES_Magenta_Fixed },
                {SKColors.Orange,SNES_Orange_Fixed },
            };

            ImageCache.PopBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,SNES_Red_Pop },
                {SKColors.Yellow,SNES_Yellow_Pop },
                {SKColors.Blue,SNES_Blue_Pop },
                {SKColors.Green,SNES_Green_Pop },
                {SKColors.Magenta,SNES_Magenta_Pop },
                {SKColors.Orange,SNES_Orange_Pop },
            };


            ImageCache.ShinyBlocks_Color = ImageCache.FixedBlocks_Color;

        }

    }
}
