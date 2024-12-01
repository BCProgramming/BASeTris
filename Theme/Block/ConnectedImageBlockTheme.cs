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
using BASeTris.AssetManager;
using BASeCamp.Logging;



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
        protected virtual GenericCachedData<SKColor, SKImage>.BlockTypeConstants GetGroupBlockType(Nomino Group)
        {
            return GenericCachedData<SKColor, SKImage>.BlockTypeConstants.Normal;
        }
        protected virtual SKColor GetGroupBlockColor(Nomino Group)
        {
            //I,O,T,S,Z,J,L
            //{Color.Cyan, Color.Yellow, Color.Purple, Color.Green, Color.Red, Color.Blue, Color.OrangeRed};
            //chosen color based on the type.
            SKColor useColor = SKColors.Red;
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
            return useColor;
        }
        protected virtual (GenericCachedData<SKColor, SKImage>.BlockTypeConstants,SKColor) GetGroupBlockData(Nomino Group)
        {

            return (GetGroupBlockType(Group), GetGroupBlockColor(Group));

        }
        Dictionary<String, SKColor> ChosenNominoColours = new Dictionary<string, SKColor>();
        public override (GenericCachedData<SKColor, SKImage>.BlockTypeConstants, SKColor) GetBlockData(Nomino Group, NominoBlock block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {

            GenericCachedData<SKColor, SKImage>.BlockTypeConstants blocktype = GenericCachedData<SKColor, SKImage>.BlockTypeConstants.Normal;
            SKColor useColor = SKColors.Red;
            Dictionary<Point, NominoElement> GroupElements = (from g in Group select g).ToDictionary((ne) => new Point(ne.BaseX(), ne.BaseY()));
            var iterate = block;
            {
                LineSeriesBlock.CombiningTypes? chosenType = null;
                if (iterate is LineSeriesBlock lsb)
                {
                    if (Group.Count > 3)
                    {
                        ;
                    }

                    chosenType = lsb.CombiningIndex;
                    useColor = LineSeriesBlock.GetCombiningTypeColor(chosenType.Value);
                    
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
                    return (blocktype, useColor);
                }
                else
                {

                    (blocktype,useColor) = GetGroupBlockData(Group);
                   

                    //chosenType = TetrisGame.Choose<LineSeriesBlock.CombiningTypes>((LineSeriesBlock.CombiningTypes[])Enum.GetValues(typeof(LineSeriesBlock.CombiningTypes)));
                }
                
            }
            return (blocktype, useColor);
        }
        //TODO: need to fix up the themes for various colours here. Fioxed blocks also stopped working.
        protected override void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            ThemeDataPrepared = true;
            //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
            Red.Normal = new CardinalImageSet();
            Red.Normal[CardinalConnectionSet.ConnectedStyles.None] = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_normal")?? TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase()));
            //"Field" reflects blocks that are part of the field, rather than active block groups.
            Red.Field = new CardinalImageSet();
            var fieldbitmap = TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_field");
            Red.Field[CardinalConnectionSet.ConnectedStyles.None] = fieldbitmap != null ? SKImage.FromBitmap(fieldbitmap) : Red.Normal[CardinalConnectionSet.ConnectedStyles.None];
            if (this is TetrisDXTheme_Depr)
            {
                ;
            }
            var FixedBitmap= TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_fixed");
            Red.Fixed = new CardinalImageSet();
            Red.Fixed[CardinalConnectionSet.ConnectedStyles.None] = FixedBitmap != null ? SKImage.FromBitmap(FixedBitmap) : Red.Normal[CardinalConnectionSet.ConnectedStyles.None];


            var PopBitmap = TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_pop");

            Red.Pop = new CardinalImageSet();

            Red.Pop[CardinalConnectionSet.ConnectedStyles.None] = PopBitmap != null ? SKImage.FromBitmap(PopBitmap) : Red.Normal[CardinalConnectionSet.ConnectedStyles.None];

            var allContexts = GetAllImageKeyContexts();
            foreach (var additionalentry in allContexts)
            {
                String sConnectKey = GetImageKeyBase() + "_" + additionalentry + "_block_connected";
                SKBitmap getbitmap = null;
                try
                {
                    getbitmap = TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_" + additionalentry)??fieldbitmap;
                }
                catch (Exception exr)
                {
                    DebugLogger.Log.WriteLine(exr.ToString());
                    getbitmap = fieldbitmap; //default to thge field bitmap...
                }
                Red[additionalentry] = new CardinalImageSet();
                Red[additionalentry][CardinalConnectionSet.ConnectedStyles.None] = getbitmap==null? Red.Normal[CardinalConnectionSet.ConnectedStyles.None]:SKImage.FromBitmap(getbitmap);
                
            }



            var AllArrangements = EnumHelper.GetAllEnums<CardinalConnectionSet.ConnectedStyles>();
            String[] NormalBlockPrefix = new String[] { GetImageKeyBase() + "_normal_block_connected", GetImageKeyBase() + "_block_connected" };
            String FieldBlockPrefix = GetImageKeyBase() + "_field_block_connected";
            String FixedBlockPrefix = GetImageKeyBase() + "_fixed_block_connected";
            String PopBlockPrefix = GetImageKeyBase() + "_pop_block_connected";
            var AdditionalEntries = (from s in GetAllImageKeyContexts() select s + "_block_connected").ToList();
            foreach (var checkflags in AllArrangements)
            {
                String useSuffix = CardinalConnectionSet.GetSuffix(checkflags);
                if (useSuffix != null)
                {
                    ProcessArrangement(NormalBlockPrefix, Red.Normal_, null, checkflags, useSuffix);
                    ProcessArrangement(FieldBlockPrefix, Red.Field_, Red.Normal_, checkflags, useSuffix);
                    ProcessArrangement(FixedBlockPrefix, Red.Fixed_,null, checkflags, useSuffix);
                    ProcessArrangement(PopBlockPrefix, Red.Pop_, null, checkflags, useSuffix);
                    //process the arrangements for the other entries.
                    foreach (var additionalentry in allContexts)
                    {
                        String sConnectKey = GetImageKeyBase() + "_" + additionalentry + "_block_connected";
                        ProcessArrangement(sConnectKey, (CardinalImageSet)Red[additionalentry], null, checkflags, useSuffix);
                    }


                }
            }
            ImageCache.NormalConnectedBlocks_Color.Add(SKColors.Red, Red.Normal);
            ImageCache.FixedConnectedBlocks_Color.Add(SKColors.Red, Red.Fixed);
            //SNES_Red_Normal = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("tetris_2_normal_snes"));
            
            //Red.Fixed = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_fixed"));
            //Red.Pop = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(GetImageKeyBase() + "_pop"));




            Yellow.Normal = new CardinalImageSet(Red.Normal_, SKColors.Yellow);
            Yellow.Field = new CardinalImageSet(Red.Field_, SKColors.Yellow);
            Yellow.Fixed = new CardinalImageSet(Red.Fixed_, SKColors.Yellow);
            Yellow.Pop = new CardinalImageSet(Red.Pop_, SKColors.Yellow); 
            Blue.Normal = new CardinalImageSet(Red.Normal_, SKColors.Cyan);
            Blue.Field = new CardinalImageSet(Red.Field_, SKColors.Cyan);
            Blue.Fixed = new CardinalImageSet(Red.Fixed_, SKColors.Cyan);
            Blue.Pop = new CardinalImageSet(Red.Pop_, SKColors.Cyan);
            Green.Normal = new CardinalImageSet(Red.Normal_, SKColors.Green);
            Green.Field = new CardinalImageSet(Red.Field_, SKColors.Green);
            Green.Fixed = new CardinalImageSet(Red.Fixed_, SKColors.Green);
            Green.Pop = new CardinalImageSet(Red.Pop_, SKColors.Green); 
            Magenta.Normal = new CardinalImageSet(Red.Normal_, SKColors.Magenta);
            Magenta.Field = new CardinalImageSet(Red.Field_, SKColors.Magenta);
            Magenta.Fixed = new CardinalImageSet(Red.Fixed_, SKColors.Magenta);
            Magenta.Pop = new CardinalImageSet(Red.Pop_, SKColors.Magenta);
            Orange.Normal = new CardinalImageSet(Red.Normal_, SKColors.Orange);
            Orange.Field = new CardinalImageSet(Red.Field_, SKColors.Orange);
            Orange.Fixed = new CardinalImageSet(Red.Fixed_, SKColors.Orange);
            Orange.Pop = new CardinalImageSet(Red.Pop_, SKColors.Orange);


            //need to do the same for all additional contexts

            foreach (var additionalentry in allContexts)
            {
                Yellow[additionalentry] = new CardinalImageSet(Red.RetrieveSet(additionalentry), SKColors.Yellow);
                Blue[additionalentry] = new CardinalImageSet(Red.RetrieveSet(additionalentry), SKColors.Cyan);
                Green[additionalentry] = new CardinalImageSet(Red.RetrieveSet(additionalentry), SKColors.Green);
                Magenta[additionalentry] = new CardinalImageSet(Red.RetrieveSet(additionalentry), SKColors.Magenta);
                Orange[additionalentry] = new CardinalImageSet(Red.RetrieveSet(additionalentry), SKColors.Orange);

                //prep image cache. This corresponds to the later code blocks doing something similar for the standard/old types (normal, Pop, fixed, etc)
                ImageCache[additionalentry] = new Dictionary<SKColor, CardinalConnectionSet<SKImage, SKColor>>()
                {
                    {SKColors.Red,Red[additionalentry] },
                    {SKColors.Yellow,Yellow[additionalentry] },
                    {SKColors.Blue,Blue[additionalentry] },
                    {SKColors.Green,Green[additionalentry] },
                    {SKColors.Magenta,Magenta[additionalentry] },
                    {SKColors.Orange,Orange[additionalentry] },
                };

            }

                
            


                ImageCache.NormalConnectedBlocks_Color = new Dictionary<SKColor, CardinalConnectionSet<SKImage, SKColor>>()
            {
                {SKColors.Red,Red.Normal },
                {SKColors.Yellow,Yellow.Normal },
                {SKColors.Blue,Blue.Normal},
                {SKColors.Green,Green.Normal },
                {SKColors.Magenta,Magenta.Normal },
                {SKColors.Orange,Orange.Normal },
            };
            ImageCache.FixedConnectedBlocks_Color = new Dictionary<SKColor, CardinalConnectionSet<SKImage, SKColor>>()
            {
                {SKColors.Red,Red.Fixed },
                {SKColors.Yellow,Yellow.Fixed },
                {SKColors.Blue,Blue.Fixed },
                {SKColors.Green,Green.Fixed },
                {SKColors.Magenta,Magenta.Fixed },
                {SKColors.Orange,Orange.Fixed },
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
                {SKColors.Red,Red.Fixed[0] },
                {SKColors.Yellow,Yellow.Fixed[0] },
                {SKColors.Blue,Blue.Fixed[0] },
                {SKColors.Green,Green.Fixed[0] },
                {SKColors.Magenta,Magenta.Fixed[0] },
                {SKColors.Orange,Orange.Fixed[0] },
            };

            ImageCache.PopBlocks_Color = new Dictionary<SKColor, SKImage>()
            {
                {SKColors.Red,Red.Pop[0] },
                {SKColors.Yellow,Yellow.Pop[0] },
                {SKColors.Blue,Blue.Pop[0] },
                {SKColors.Green,Green.Pop[0] },
                {SKColors.Magenta,Magenta.Pop[0] },
                {SKColors.Orange,Orange.Pop[0] },
            };


            ImageCache.ShinyBlocks_Color = ImageCache.FixedBlocks_Color;

        }
        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            //TODO: Random application should also consider custom context types, not just the blocktypeconstants.
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

        private void ProcessArrangement(string[] BlockPrefix, CardinalImageSet Target, CardinalImageSet DefaultSet, CardinalConnectionSet.ConnectedStyles checkflags, string useSuffix, SKImage[] Compositors = null)
        {
            if (BlockPrefix == null) throw new ArgumentNullException("BlockPrefix");
            if (BlockPrefix.Length == 0) throw new ArgumentException("BlockPrefix is empty array.");

            String sFindNormalImage = BlockPrefix.First() + (useSuffix.Length > 0 ? "_" : "") + useSuffix;

            try
            {
                if (TetrisGame.Imageman.HasSKBitmap(sFindNormalImage))
                {
                    var getimage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(sFindNormalImage));
                    //todo: implement compositors. If provided, we want to composite a randomly selected element from that array with the Bitmap we retrieved.
                    Target[checkflags] = getimage;
                }
                else
                {
                    if (BlockPrefix.Length == 1)//last prefix to process
                    {


                        if (DefaultSet != null) Target[checkflags] = DefaultSet[checkflags];

                        Target[checkflags] = Target[CardinalConnectionSet.ConnectedStyles.None];
                    }
                    else
                    {
                        ProcessArrangement(BlockPrefix.Skip(1).ToArray(), Target, DefaultSet, checkflags, useSuffix, Compositors);
                    }
                    

                }
            }
            finally
            {
            }
        }

        private void ProcessArrangement(string BlockPrefix, CardinalImageSet Target, CardinalImageSet DefaultSet,  CardinalConnectionSet.ConnectedStyles checkflags, string useSuffix,SKImage[] Compositors = null)
        {
            ProcessArrangement(new String[] { BlockPrefix }, Target, DefaultSet, checkflags, useSuffix, Compositors);
            return;
        }


    }

    public class CardinalImageSetBlockData : CardinalBlockData<SKColor,SKImage>
    {

        public CardinalImageSetBlockData()
        {
        }

        

        public CardinalImageSetBlockData(CardinalImageSet pNormal, CardinalImageSet pField, CardinalImageSet pFixed, CardinalImageSet pPop)
        {
            
            Normal = pNormal;
            Field = pField;
            Fixed = pFixed;
            Pop = pPop;
        }
        public new CardinalConnectionSet<SKImage, SKColor> this[String pIndex]
        {
            get
            {
                return ConnectionSetData?[pIndex];
            }
            set
            {
                ConnectionSetData[pIndex] = value;
            }
        }

        public CardinalImageSet RetrieveSet(String pKey)
        {
            return ConnectionSetData?[pKey] as CardinalImageSet;
        }
        public void AssignSet(String pKey, CardinalImageSet value)
        {
            ConnectionSetData[pKey] = value;
        }
        //these are "helpers" more than anything, making it easier to get/set the most common sets from the dictionary via a property.
        public CardinalImageSet Normal_ { get { return (CardinalImageSet)this["Normal"]; } set { this["Normal"] = value; } }
        public override CardinalConnectionSet<SKImage, SKColor> Normal { get => Normal_; set => Normal_ = (CardinalImageSet)value; }

        public CardinalImageSet Field_ { get { return (CardinalImageSet)this["Field"]; } set { this["Field"] = value; } }
        //public CardinalImageSet Field_ { get; set; } = null;
        public override CardinalConnectionSet<SKImage, SKColor> Field { get => Field_; set =>Field_= (CardinalImageSet)value; }

        public CardinalImageSet Fixed_ { get { return (CardinalImageSet)this["Fixed"]; } set { this["Fixed"] = value; } } 
        public override CardinalConnectionSet<SKImage, SKColor> Fixed { get => Fixed_; set => Fixed_= (CardinalImageSet)value; }
        //public SKImage Fixed_ { get; set; } = null;
        //public override SKImage Fixed { get => Fixed_; set => Fixed_= value; }
        public CardinalImageSet Pop_ { get { return (CardinalImageSet)this["Pop"]; } set { this["Pop"] = value; } } 
        public override CardinalConnectionSet<SKImage,SKColor> Pop { get => Pop_; set => Pop_ = (CardinalImageSet)value; }
    }
    public class CardinalBlockData<Key, CacheType>
    {
        
        protected Dictionary<String, CardinalConnectionSet<CacheType, Key>> ConnectionSetData = new Dictionary<string, CardinalConnectionSet<CacheType, Key>>();

        public virtual CardinalConnectionSet<CacheType, Key> Normal { get { return this["Normal"]; } set { this["Normal"] = value; } }
        public virtual CardinalConnectionSet<CacheType, Key> Field { get { return this["Field"]; } set { this["Field"] = value; } }
        public virtual CardinalConnectionSet<CacheType, Key> Fixed { get { return this["Fixed"]; } set { this["Fixed"] = value; } }
        public virtual CardinalConnectionSet<CacheType, Key> Pop { get { return this["Pop"]; } set { this["Pop"] = value; } }

        public virtual CardinalConnectionSet<CacheType, Key> this[String pIndex]
        {
            get
            {
                return ConnectionSetData?[pIndex];
            }
            set
            {
                ConnectionSetData[pIndex] = value;
            }
        }


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
            if (Reason == ThemeApplicationReason.NewNomino)
            {

                ;
            }
            
            //---
            Dictionary<Point, NominoElement> GroupElements = (from g in Group select g).ToDictionary((ne) => new Point(ne.BaseX(), ne.BaseY()));
            foreach (var iterate in Group)
            {
                //CachedImageData<Key>.BlockTypeConstants blocktype = CachedImageData<Key>.BlockTypeConstants.Normal;
                //Dictionary<SKColor, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes? chosenType = null;


                GenericCachedData.BlockTypeConstants btc;

                
                Key useKey;
                (btc, useKey) = GetBlockData(Group, iterate.Block, GameHandler, Field, Reason);

                //---
                if (iterate.Block is ImageBlock ibb)
                {
                    if (iterate.Block is StandardColouredBlock scb)
                    {
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }


                    
                    
                    if (btc == GenericCachedData.BlockTypeConstants.Normal || btc == GenericCachedData.BlockTypeConstants.Fixed && UseConnectedImages)
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
                        var sGetContext = GetImageKeyContext(iterate.Block);

                         
                        var useRotationImages = (from c in useConnectionStyles select  ImageCache.GetConnectedBlocksByType(useKey, btc,sGetContext)[c]).ToArray();
                        

                        //var useImage = ImageCache.GetNormalConnectedBlocks(useKey)[cs];
                        ibb._RotationImagesSK = useRotationImages.ToArray();//GetImageRotations(SKBitmap.FromImage(useImage));
                        //ibb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(useImage));


                    }
                    else 
                    {
                        ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock((GenericCachedData<Key, SKImage>.BlockTypeConstants)btc, useKey) };
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

            return base.HandleBGCache(() => new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_4", 0.5f], Color.Transparent));
        }
        protected abstract String GetImageKeyBase();

        protected virtual String GetImageKeyContext(NominoBlock nb)
        {
            return String.Empty;
        }
        protected virtual String[] GetAllImageKeyContexts()
        {
            return new String[] { };
        }
        protected abstract void PrepareThemeData();
        
    }


}
