using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
using static SkiaSharp.SKPath;
using OpenTK.Input;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.IO;
using System.CodeDom;
using OpenTK.Graphics.ES11;



namespace BASeTris.Theme.Block
{


    public abstract class ConnectedImageBlockTheme : ConnectedImageLineSeriesBlockThemeBase<SKColor, SKImage>
    {
        protected CardinalImageSetBlockData Red = new CardinalImageSetBlockData();
        protected CardinalImageSetBlockData Yellow = new CardinalImageSetBlockData();
        protected CardinalImageSetBlockData Blue = new CardinalImageSetBlockData();
        protected CardinalImageSetBlockData Green = new CardinalImageSetBlockData();
        protected CardinalImageSetBlockData Magenta = new CardinalImageSetBlockData();
        protected CardinalImageSetBlockData Orange = new CardinalImageSetBlockData();
        public override SKColor RandomChoice()
        {
            return new SKColor((byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255));
        }
        public override SKImage GetBlock(GenericCachedData<SKColor, SKImage>.BlockTypeConstants btc, SKColor chosen)
        {
            return ImageCache.GetBlock(btc, chosen);
        }
        public override SKImage GetImage(SKImage src)
        {
            return src;
        }
        public ConnectedImageBlockTheme()
        {
            base.ImageCache = new CachedImageDataByColor();
        }
        Dictionary<String, SKColor> ChosenNominoColours = new Dictionary<string, SKColor>();
        public override (GenericCachedData<SKColor, SKImage>.BlockTypeConstants, SKColor) GetBlockData(Nomino Group, NominoBlock block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            GenericCachedData<SKColor, SKImage>.BlockTypeConstants blocktype = GenericCachedData<SKColor, SKImage>.BlockTypeConstants.Normal;
            SKColor useColor = SKColors.Red;
            Dictionary<Point, NominoElement> GroupElements = (from g in Group select g).ToDictionary((ne) => new Point(ne.BaseX(), ne.BaseY()));
            foreach (var iterate in Group)
            {
                //Dictionary<SKColor, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes? chosenType = null;
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    chosenType = lsb.CombiningIndex;

                    if (lsb.Popping)
                    {
                        blocktype = CachedImageDataByColor.BlockTypeConstants.Pop;
                        //Sourcedict = ImageCache.PopBlocks_Color;
                    }
                    else
                    {
                        blocktype = CachedImageDataByColor.BlockTypeConstants.Normal;

                        if (lsb is LineSeriesPrimaryBlock lsbp)
                        {

                            if (lsbp is LineSeriesPrimaryShinyBlock)
                            {
                                blocktype = CachedImageDataByColor.BlockTypeConstants.Shiny;

                            }
                            else
                            {
                                blocktype = CachedImageDataByColor.BlockTypeConstants.Fixed;
                            }
                        }
                    }
                }
                else
                {
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
                    else
                    {
                        useColor = NNominoGenerator.GetNominoData<SKColor>(ChosenNominoColours, Group, () => RandomColor());
                    }
                    //else useColor = SKColors.Gray;



                    //chosenType = TetrisGame.Choose<LineSeriesBlock.CombiningTypes>((LineSeriesBlock.CombiningTypes[])Enum.GetValues(typeof(LineSeriesBlock.CombiningTypes)));
                }
                if (chosenType != null) useColor = LineSeriesBlock.GetCombiningTypeColor(chosenType.Value);
            }
            return (blocktype, useColor);
        }

        protected override void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            ThemeDataPrepared = true;
            //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
            Red.Normal = new CardinalImageSet();
            Red.Normal[CardinalConnectionSet.ConnectedStyles.None] = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_normal"));
            //"Field" reflects blocks that are part of the field, rather than active block groups.
            Red.Field = new CardinalImageSet();
            var fieldbitmap = TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_field");
            Red.Field[CardinalConnectionSet.ConnectedStyles.None] = fieldbitmap != null ? SKImage.FromBitmap(fieldbitmap) : Red.Normal[CardinalConnectionSet.ConnectedStyles.None];


            var AllArrangements = EnumHelper.GetAllEnums<CardinalConnectionSet.ConnectedStyles>();
            String NormalBlockPrefix = GetImageKeyBase() + "_normal_block_connected";
            String FieldBlockPrefix = GetImageKeyBase() + "_field_block_connected";
            foreach (var checkflags in AllArrangements)
            {
                String useSuffix = CardinalConnectionSet.GetSuffix(checkflags);
                if (useSuffix != null)
                {
                    ProcessArrangement(NormalBlockPrefix, Red.Normal_, null, checkflags, useSuffix);
                    ProcessArrangement(FieldBlockPrefix, Red.Field_, Red.Normal_, checkflags, useSuffix);

                }
            }
            ImageCache.NormalConnectedBlocks_Color.Add(SKColors.Red, Red.Normal);

            //SNES_Red_Normal = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));
            Red.Fixed = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_fixed"));
            Red.Pop = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_pop"));




            Yellow.Normal = new CardinalImageSet(Red.Normal_, SKColors.Yellow);
            Yellow.Field = new CardinalImageSet(Red.Field_, SKColors.Yellow);
            Yellow.Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Fixed, SKColors.Yellow);
            Yellow.Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Pop, SKColors.Yellow);
            Blue.Normal = new CardinalImageSet(Red.Normal_, SKColors.Cyan);
            Blue.Field = new CardinalImageSet(Red.Field_, SKColors.Cyan);
            Blue.Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Fixed, SKColors.Cyan);
            Blue.Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Pop, SKColors.Cyan);
            Green.Normal = new CardinalImageSet(Red.Normal_, SKColors.Green);
            Green.Field = new CardinalImageSet(Red.Field_, SKColors.Green);
            Green.Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Fixed, SKColors.Green);
            Green.Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Pop, SKColors.Green);
            Magenta.Normal = new CardinalImageSet(Red.Normal_, SKColors.Magenta);
            Magenta.Field = new CardinalImageSet(Red.Field_, SKColors.Magenta);
            Magenta.Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Fixed, SKColors.Magenta);
            Magenta.Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Pop, SKColors.Magenta);
            Orange.Normal = new CardinalImageSet(Red.Normal_, SKColors.Orange);
            Orange.Field = new CardinalImageSet(Red.Field_, SKColors.Orange);
            Orange.Fixed = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Fixed, SKColors.Orange);
            Orange.Pop = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(Red.Pop, SKColors.Orange);

            ImageCache.NormalConnectedBlocks_Color = new Dictionary<SKColor, CardinalConnectionSet<SKImage, SKColor>>()
            {
                {SKColors.Red,Red.Normal },
                {SKColors.Yellow,Yellow.Normal },
                {SKColors.Blue,Blue.Normal},
                {SKColors.Green,Green.Normal },
                {SKColors.Magenta,Magenta.Normal },
                {SKColors.Orange,Orange.Normal },
            };
            ImageCache.NormalBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,Red.Normal[0] },
                {SKColors.Yellow,Yellow.Normal[0] },
                {SKColors.Blue,Blue.Normal[0] },
                {SKColors.Green,Green.Normal[0] },
                {SKColors.Magenta,Magenta.Normal[0] },
                {SKColors.Orange,Orange.Normal[0] },
            };
            ImageCache.FixedBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,Red.Fixed },
                {SKColors.Yellow,Yellow.Fixed },
                {SKColors.Blue,Blue.Fixed },
                {SKColors.Green,Green.Fixed },
                {SKColors.Magenta,Magenta.Fixed },
                {SKColors.Orange,Orange.Fixed },
            };

            ImageCache.PopBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,Red.Pop },
                {SKColors.Yellow,Yellow.Pop },
                {SKColors.Blue,Blue.Pop },
                {SKColors.Green,Green.Pop },
                {SKColors.Magenta,Magenta.Pop },
                {SKColors.Orange,Orange.Pop },
            };


            ImageCache.ShinyBlocks_Color = ImageCache.FixedBlocks_Color;

        }
        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {

            PrepareThemeData();
            var drawtype = RandomHelpers.Static.Select(new[] { CachedImageDataByColor.BlockTypeConstants.Fixed, CachedImageDataByColor.BlockTypeConstants.Normal, CachedImageDataByColor.BlockTypeConstants.Pop }, new float[] { 20, 60, 5 });

            foreach (var iterate in Group)
            {
                var chosenColor = RandomChoice();     //  new SKColor((byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255));


                if (iterate.Block is ImageBlock ibb)
                {
                    if (ibb is StandardColouredBlock sbc)
                    {
                        sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }

                    ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock(drawtype, chosenColor) };
                }
            }
        }
        private void ProcessArrangement(string BlockPrefix, CardinalImageSet Target, CardinalImageSet DefaultSet, CardinalConnectionSet.ConnectedStyles checkflags, string useSuffix)
        {
            String sFindNormalImage = BlockPrefix + (useSuffix.Length > 0 ? "_" : "") + useSuffix;

            try
            {
                if (TetrisGame.Imageman.HasSKBitmap(sFindNormalImage))
                {
                    var getimage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(sFindNormalImage));
                    Target[checkflags] = getimage;
                }
                else
                {
                    if (DefaultSet != null) Target[checkflags] = DefaultSet[checkflags];
                    Target[checkflags] = Target[CardinalConnectionSet.ConnectedStyles.None];
                }
            }
            finally
            {
            }
        }


    }

    public class CardinalImageSetBlockData : CardinalBlockData<SKColor,SKImage>
    {

        public CardinalImageSetBlockData()
        {
        }
        public CardinalImageSetBlockData(CardinalImageSet pNormal, CardinalImageSet pField, SKImage pFixed, SKImage pPop)
        {
            
            Normal = pNormal;
            Field = pField;
            Fixed = pFixed;
            Pop = pPop;
        }
        public CardinalImageSet Normal_ { get; set; } = null;
        public override CardinalConnectionSet<SKImage, SKColor> Normal { get => Normal_; set => Normal_ = (CardinalImageSet)value; }
        public CardinalImageSet Field_ { get; set; } = null;
        public override CardinalConnectionSet<SKImage, SKColor> Field { get => Field_; set =>Field_= (CardinalImageSet)value; }
        public SKImage Fixed_ { get; set; } = null;
        public override SKImage Fixed { get => Fixed_; set => Fixed_= value; }
        public SKImage Pop_ { get; set; } = null;
        public override SKImage Pop { get => Pop_; set => Pop_ = value; }
    }
    public class CardinalBlockData<Key, CacheType>
    {

        public virtual CardinalConnectionSet<CacheType, Key> Normal { get; set; } = null;
        public virtual CardinalConnectionSet<CacheType, Key> Field { get; set; } = null;
        public virtual CacheType Fixed { get; set; }
        public virtual CacheType Pop { get; set; }


    }


    //todo here: rework this class - SKColor is the key, SKImage is the DataType. This rework is to allow the class to be used in some way with the color replacement custom pixel themes, which rely on BCT types as a key and provide BCT[][] arrays as a result.
    public abstract class ConnectedImageLineSeriesBlockThemeBase<Key, DataType> : NominoTheme
    {
        



        protected CachedImageData<Key> ImageCache = null;
        //protected CachedImageDataByColor ImageCache = new CachedImageDataByColor();
        //static Dictionary<LineSeriesBlock.CombiningTypes, CardinalImageSet> NormalConnectedBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> NormalBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> FixedBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> ShinyBlocks = null;
        //static Dictionary<LineSeriesBlock.CombiningTypes, SKImage> PopBlocks = null;
        protected bool ThemeDataPrepared = false;

        public override string Name => "ConnectedBlockTheme";

        public abstract Key RandomChoice();
        public abstract DataType GetBlock(GenericCachedData<Key, DataType>.BlockTypeConstants btc, Key chosen);

        public abstract (GenericCachedData<Key, DataType>.BlockTypeConstants, Key) GetBlockData(Nomino Group, NominoBlock block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);
        public abstract SKImage GetImage(DataType src);
        //public abstract void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field);
        

        Dictionary<String, Key> ChosenNominoColours = new Dictionary<string, Key>();



        public virtual bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {

            return (!VisuallyConnectOnlySameCombiningType || (BlockA is LineSeriesBlock lsbg && BlockB is LineSeriesBlock lsbb));

        }
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            PrepareThemeData();
            if (Reason == ThemeApplicationReason.FieldSet)
            {

                ;
            }
            //---
            Dictionary<Point, NominoElement> GroupElements = (from g in Group select g).ToDictionary((ne) => new Point(ne.BaseX(), ne.BaseY()));
            foreach (var iterate in Group)
            {
                CachedImageData<Key>.BlockTypeConstants blocktype = CachedImageData<Key>.BlockTypeConstants.Normal;
                //Dictionary<SKColor, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes? chosenType = null;


                GenericCachedData<Key, DataType>.BlockTypeConstants btc;

                
                Key useKey;
                (btc, useKey) = GetBlockData(Group, iterate.Block, GameHandler, Field, Reason);

                //---
                if (iterate.Block is ImageBlock ibb)
                {
                    if (iterate.Block is StandardColouredBlock scb)
                    {
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }

                    if (blocktype == CachedImageData<Key>.BlockTypeConstants.Normal && UseConnectedImages)
                    {

                        //determine the flags by checking the Nomino.
                        CardinalConnectionSet.ConnectedStyles cs = CardinalConnectionSet.ConnectedStyles.None;
                        //check above
                        Point North = new Point(iterate.BaseX(), iterate.BaseY() - 1);
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

                                if(IsConnected(GroupElements[Checkconnected.Item1].Block, iterate.Block))


                                
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

                        var useRotationImages = from c in useConnectionStyles select   ImageCache.GetConnectedBlocks(useKey)[c];
                        var useImage = ImageCache.GetConnectedBlocks(useKey)[cs];
                        ibb._RotationImagesSK = useRotationImages.ToArray();//GetImageRotations(SKBitmap.FromImage(useImage));
                        //ibb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(useImage));


                    }
                    else
                    {
                        ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock(blocktype, useKey) };
                    }

                }


            }
        }
        protected SKColor RandomColor()
        {
            return new SKColor((byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256));
        }
        protected bool VisuallyConnectOnlySameCombiningType = false;
        protected bool UseConnectedImages = true;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_4", 0.5f], Color.Transparent);
        }
        protected abstract String GetImageKeyBase();

        protected abstract void PrepareThemeData();
        
    }


}
