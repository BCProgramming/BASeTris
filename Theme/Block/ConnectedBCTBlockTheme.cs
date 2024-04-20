using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;



namespace BASeTris.Theme.Block
{
    public class ConnectedNESBlockTheme : ConnectedBCTBlockTheme<NESTetrominoTheme.NESBlockTypes, NESTetrominoTheme.BCT>
    {
        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            
            throw new NotImplementedException();
        }

        public override NESTetrominoTheme.BCT FromDefaultImagePixel(SKColor PixelColor)
        {
            throw new NotImplementedException();
        }

        public override NESTetrominoTheme.BCT[][] GetBlock(GenericCachedData<NESTetrominoTheme.NESBlockTypes, NESTetrominoTheme.BCT[][]>.BlockTypeConstants btc, NESTetrominoTheme.NESBlockTypes chosen)
        {
            throw new NotImplementedException();
        }

        public override string GetBlockAdditionalKey(NESTetrominoTheme.NESBlockTypes BlockType, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            throw new NotImplementedException();
        }

        public override (GenericCachedData<NESTetrominoTheme.NESBlockTypes, NESTetrominoTheme.BCT[][]>.BlockTypeConstants, NESTetrominoTheme.NESBlockTypes) GetBlockData(Nomino Group, NominoBlock block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            throw new NotImplementedException();
        }

        public override string GetBlockEnumSuffix(NESTetrominoTheme.NESBlockTypes BlockType)
        {
            throw new NotImplementedException();
        }

        public override NESTetrominoTheme.NESBlockTypes GetBlockType(Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            throw new NotImplementedException();
        }

        public override SKImage GetImage(NESTetrominoTheme.BCT[][] src)
        {
            throw new NotImplementedException();
        }

        public override SKColor GetPixelColor(NESTetrominoTheme.NESBlockTypes BlockType, NESTetrominoTheme.BCT PixelType, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            throw new NotImplementedException();
        }

        public override NESTetrominoTheme.NESBlockTypes RandomChoice()
        {
            throw new NotImplementedException();
        }

        protected override string GetImageKeyBase()
        {
            throw new NotImplementedException();
        }
    }
    public abstract class ConnectedBCTBlockTheme<BlockEnum, PixelEnum> :  ConnectedImageLineSeriesBlockThemeBase<BlockEnum, PixelEnum[][]> where BlockEnum:struct where PixelEnum:struct
    {

        //indexes the cardinal block data by the BlockEnum
        CardinalConnectionDictionary<BlockEnum, PixelEnum[][]> ConnectedPixelSets = new CardinalConnectionDictionary<BlockEnum, PixelEnum[][]>();
        
        Dictionary<String, SKImage> CardinalImageCache = new Dictionary<String, SKImage>();

        /// <summary>
        /// given the pixel in the "default" image asset, return the corresponding Pixel enumeration.
        /// </summary>
        /// <param name="PixelColor"></param>
        /// <returns></returns>
        public abstract PixelEnum FromDefaultImagePixel(SKColor PixelColor);
        

        //ConnectedBCTBlockTheme is the base class for themes that want to use the customPixelTheme featureset but for connected images.
        /// <summary>
        /// retrieves the Color of a particular Pixel type in a given BlockType, given the information about the appropriate field and Nomino information.
        /// </summary>
        /// <param name="BlockType"></param>
        /// <param name="PixelType"></param>
        /// <param name="Group"></param>
        /// <param name="Block"></param>
        /// <param name="GameHandler"></param>
        /// <param name="Field"></param>
        /// <param name="Reason"></param>
        /// <returns></returns>
        /// 
        public abstract SKColor GetPixelColor(BlockEnum BlockType,PixelEnum PixelType, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);

        //return any additional key to be stapled on when indexing images. For example, NES Theme also indexes by the current level number.
        public abstract String GetBlockAdditionalKey(BlockEnum BlockType, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);

        public abstract BlockEnum GetBlockType(Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason);
        /// <summary>
        /// retrieve the filename suffix for this blocktype. This is used when deciding what filenames to load and translate into Pixel arrays.
        /// </summary>
        /// <param name="BlockType"></param>
        /// <returns></returns>
        public abstract string GetBlockEnumSuffix(BlockEnum BlockType);

        

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
                
                //Dictionary<SKColor, SKImage> Sourcedict = null;
                LineSeriesBlock.CombiningTypes? chosenType = null;





                BlockEnum blocktype = GetBlockType(Group, iterate.Block, GameHandler, Field, Reason);

                string useKey = BuildSpecialKey(blocktype, Group, iterate.Block, GameHandler, Field, Reason);
                //---
                if (iterate.Block is ImageBlock ibb)
                {
                    if (iterate.Block is StandardColouredBlock scb)
                    {
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }

                    if (UseConnectedImages)
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

                                if (IsConnected(GroupElements[Checkconnected.Item1].Block, iterate.Block))
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




                        var useRotationData = from c in useConnectionStyles select ConnectedPixelSets[blocktype, c];
                        var currdata = ConnectedPixelSets[blocktype,cs];

                        var useRotationImages = from r in useConnectionStyles select GetImageFromPixelData(blocktype, ConnectedPixelSets[blocktype, r], r, Group, iterate.Block, GameHandler, Field, Reason);

                        ibb._RotationImagesSK = useRotationImages.ToArray();
                        //ibb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(useImage));
                    }
                    else
                    {
                        //ibb._RotationImagesSK = new SKImage[] { ImageCache.GetBlock(blocktype, useKey) };
                    }

                }
            }
        }
        
        private SKImage GetImageFromPixelData(BlockEnum BlockType, PixelEnum[][] pixeldata, CardinalConnectionSet.ConnectedStyles Connection, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason reason)
        {

            String sUseKey = BuildCacheKey(BlockType, Connection, Group, Block, GameHandler, Field, reason);
            if (!CardinalImageCache.ContainsKey(sUseKey))
            {


                int RowCount = pixeldata.Length;
                int ColCount = pixeldata[0].Length;
                SKImageInfo drawinfo = new SKImageInfo(ColCount, RowCount, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (SKBitmap DrawBitmap = new SKBitmap(drawinfo))
                {

                    for (int r = 0; r < RowCount; r++)
                    {
                        for (int c = 0; c < ColCount; c++)
                        {
                            PixelEnum pixel = pixeldata[r][c];
                            SKColor getColor = GetPixelColor(BlockType, pixel, Group, Block, GameHandler, Field, reason);
                            DrawBitmap.SetPixel(c, r, getColor);

                        }
                    }
                    CardinalImageCache.Add(sUseKey, SKImage.FromBitmap(DrawBitmap));
                }
            }
            return CardinalImageCache[sUseKey];
        }
        private String BuildCacheKey(BlockEnum BlockType, CardinalConnectionSet.ConnectedStyles Connections,  Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {

            String sSpecialKey = GetBlockAdditionalKey(BlockType, Group, Block, GameHandler, Field, Reason);
            String sBuildKey = String.Join("_", from j in new String[] { BlockType.ToString(),CardinalConnectionSet.GetSuffix(Connections),   sSpecialKey } where !String.IsNullOrEmpty(j) select j);
            return sBuildKey;


        }

        private String BuildSpecialKey(BlockEnum BlockType, Nomino Group, NominoBlock Block, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {

            String sSpecialKey = GetBlockAdditionalKey(BlockType, Group, Block, GameHandler, Field, Reason);
            String sBuildKey = String.Join("_",from j in new String[] { BlockType.ToString(), sSpecialKey } where !String.IsNullOrEmpty(j) select j );
            return sBuildKey;


        }

        protected override void PrepareThemeData()
        {
            var AllArrangements = EnumHelper.GetAllEnums<CardinalConnectionSet.ConnectedStyles>();
            //task: we want to populate ConnectedPixelSets.
            foreach (var blocktype in Enum.GetValues(typeof(BlockEnum)))
            {
                //processing for each blocktype.
                //1 get the suffix we want for this blocktype.
                String sBlockTypeSuffix = GetBlockEnumSuffix((BlockEnum)blocktype);
                String sGetBaseKey = GetImageKeyBase();

                foreach (var arrangement in AllArrangements)
                {

                    //filename to seek is sGetBaseKey_sBlockTypeSuffix_Cardinal
                    String sCardinalStr = CardinalConnectionSet.GetSuffix(arrangement);


                    String sImageKey = String.Join("_", from j in new[] { sBlockTypeSuffix.ToString(), sGetBaseKey, sCardinalStr } where !String.IsNullOrEmpty(j) select j);
                    var getresult = LoadPixelDataArrayFromDefaultAsset(sImageKey);
                    ConnectedPixelSets[(BlockEnum)blocktype, arrangement] = getresult;
                }
            }
        }
        
        protected PixelEnum[][] LoadPixelDataArrayFromDefaultAsset(String sImageKey)
        {
            try
            {
                if (TetrisGame.Imageman.HasSKBitmap(sImageKey))
                {
                    var getimage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(sImageKey));
                    return LoadPixelDataArrayFromDefaultAsset(getimage);
                }
            }
            finally
            {
            }
            return null;

        }
        protected PixelEnum[][] LoadPixelDataArrayFromDefaultAsset(SKImage ImageSource)
        {
            SKPixmap map = ImageSource.PeekPixels();

            PixelEnum[][] MapResult = new PixelEnum[map.Height][];
            for (int row = 0; row < map.Height; row++)
            {
                MapResult[row] = new PixelEnum[map.Width];
                for (int col = 0; col < map.Width; col++)
                {
                    SKColor thispixel = map.GetPixelColor(col, row);
                    MapResult[row][col] = FromDefaultImagePixel(thispixel);
                }
            }
            return MapResult;

        }
        

    }


}
