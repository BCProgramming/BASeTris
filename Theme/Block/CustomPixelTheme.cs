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

namespace BASeTris.Theme.Block
{
    /// <summary>
    /// used for implementing a tetromino theme which works via "pixel mapping" And provides the image cache for each appropriate combination.
    /// T: Type for representing each Pixel
    /// K: Type for representing each Block.
    /// </summary>
    public abstract class CustomPixelTheme<PixelEnum, BlockEnum> : NominoTheme
    {
         public record class BlockTypeReturnData(BlockEnum BlockType,bool Animated = false);
        
        /*public class BlockTypeReturnData
        {
            public BlockEnum BlockType;
            public bool Animated;
            public BlockTypeReturnData(BlockEnum pEnum)
            {
                BlockType = pEnum;
            }
            public BlockTypeReturnData(BlockEnum pEnum,bool pAnimated)
            {
                BlockType = pEnum;
                Animated = pAnimated;
            }
        }*/
        [Flags]
        public enum AdjacentBlockFlags
        {
            None = 0,
            Left = 1,
            Top = 2,
            Right=4,
            Bottom=8,
            TopLeft = 16,
            TopRight = 32,
            BottomLeft = 64,
            BottomRight=128
        }
        public enum AdjacentBlockFlagResultTypes
        {
            Cardinal = 1,
            Diagonal = 2
        }
        public enum BlockFlags
        {
            Static,
            Rotatable,
            CustomSelector
        }
        public abstract SKPointI GetBlockSize(TetrisField field, BlockEnum BlockType);

        public abstract SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, BlockEnum BlockType, PixelEnum PixelType);

        public static PixelEnum[][] GetBCTBitmap(String ImageKey, Func<SKColor, PixelEnum> ColorMapFunc)
        {
            SKImage sourceBitmap = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(ImageKey));
            return GetPixelsFromSKImage(sourceBitmap, ColorMapFunc);
        }
        public static PixelEnum[][] GetPixelsFromSKImage(SKImage Source, Func<SKColor, PixelEnum> PixelMapRoutine)
        {
            PixelEnum[][] Result = new PixelEnum[Source.Height][];
            using (SKBitmap WorkMap = SKBitmap.FromImage(Source))
            {

                for (int y = 0; y < Source.Height; y++)
                {
                    Result[y] = new PixelEnum[Source.Width];
                    for (int x = 0; x < Source.Width; x++)
                    {
                        SKColor currPixel = WorkMap.GetPixel(x, y);
                        PixelEnum result = PixelMapRoutine(currPixel);
                        Result[y][x] = result;
                    }

                }

            }
            return Result;
        }


        protected PixelEnum[][] RotateMatrix(PixelEnum[][] Source)
        {
            var FirstRank = Source.Length;
            int SecondRank = -1;
            PixelEnum[][] Result = new PixelEnum[FirstRank][];
            for(int y=0;y<Source.Length;y++)
            {
                if(SecondRank == -1)
                {
                    SecondRank = Source[y].Length;

                }
                Result[y] = new PixelEnum[SecondRank];
                for(int x=0;x<SecondRank;x++)
                {
                    Result[y][x] = Source[x][y];
                }



            }

            return Result;

        }
        //routine which gives a dictionary that provides PixelEnum bitmap for (ideally each of) a set of BlockEnum types understood by the implementing class.
        private Dictionary<BlockEnum, PixelEnum[][]> _BlockTypeDictionary = null;
        private Dictionary<BlockEnum, PixelEnum[][]> BlockTypeDictionary
        {
            get
            {
                if (_BlockTypeDictionary == null)
                    _BlockTypeDictionary = GetBlockTypeDictionary();
                return _BlockTypeDictionary;
            }
        }
        protected AdjacentBlockFlags GetAdjacentFlags(TetrisField Field,int pColumn,int pRow,AdjacentBlockFlagResultTypes flags)
        {
            AdjacentBlockFlags flagresult = AdjacentBlockFlags.None;


            Dictionary<AdjacentBlockFlags, BCPointI> Cardinals = new Dictionary<AdjacentBlockFlags, BCPointI>()
            {
                { AdjacentBlockFlags.Left,(-1,0) },
                { AdjacentBlockFlags.Top,(0,-1) },
                { AdjacentBlockFlags.Right,(1,0) },
                { AdjacentBlockFlags.Bottom,(0,1) }

            };

            Dictionary<AdjacentBlockFlags, BCPointI> Diagonals = new Dictionary<AdjacentBlockFlags, BCPointI>()
            {
                { AdjacentBlockFlags.TopLeft,(-1,-1) },
                { AdjacentBlockFlags.TopRight,(1,-1) },
                { AdjacentBlockFlags.BottomRight,(1,1) },
                { AdjacentBlockFlags.BottomLeft,(-1,1) }

            };
            Dictionary<AdjacentBlockFlagResultTypes, Dictionary<AdjacentBlockFlags, BCPointI>> Vals = new Dictionary<AdjacentBlockFlagResultTypes, Dictionary<AdjacentBlockFlags, BCPointI>>()
            { {AdjacentBlockFlagResultTypes.Cardinal,Cardinals },
            {AdjacentBlockFlagResultTypes.Diagonal,Diagonals } };
            

            foreach(var useset in Vals)
            {
                if(flags.HasFlag(useset.Key))
                {
                    foreach(var iterateoffset in useset.Value)
                    {
                        var OffsetTuple = iterateoffset.Value;
                        BCPointI newposition = ((pColumn + OffsetTuple.X), pRow + OffsetTuple.Y);

                        if(newposition.X >=0 && newposition.X < Field.ColCount &&
                            newposition.Y >=0 && newposition.Y < Field.RowCount)
                        {
                            if(Field.Contents[newposition.Y][newposition.X]!=null)
                            {
                                flagresult &= iterateoffset.Key;
                            }
                        }
                    }
                }
            }


            return flagresult;

        }
        public abstract Dictionary<BlockEnum, PixelEnum[][]> GetBlockTypeDictionary();

        public SKColor[][] GetBlockPixels(TetrisField field, Nomino Element, NominoElement block, BlockEnum BlockTypeIndex)
        {
            SKColor[][] createresult;
            if (!BlockTypeDictionary.ContainsKey(BlockTypeIndex)) throw new InvalidOperationException("Invalid BlockTypeIndex: Not defined");
            var BlockPixelMatrix = BlockTypeDictionary[BlockTypeIndex];
            SKPointI blocksize = GetBlockSize(field, BlockTypeIndex);
            createresult = new SKColor[blocksize.Y][];
            for (int yval = 0; yval < blocksize.Y; yval++)
            {
                createresult[yval] = new SKColor[blocksize.X];
                for (int xval = 0; xval < blocksize.X; xval++)
                {
                    var ColorType = BlockPixelMatrix[yval][xval];
                    createresult[yval][xval] = GetColor(field, Element, block, BlockTypeIndex, ColorType);
                }
            }
            return createresult;
        }
        /// <summary>
        /// Retrieves the flags for a particular NominoElement. In particular, this determines whether the graphic will be automatically rotated or if the actual image should be retrieved with a custom selector routine.
        /// </summary>
        /// <param name="Group">Nomino that contains this Element</param>
        /// <param name="element">actual NominoElement for which the block flags are being retrieved</param>
        /// <param name="field">Field on which the Nomino and Element are present</param>
        /// <returns>BlockFlags for this nominoelement.</returns>
        public abstract BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field);
        public abstract BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field);

        public abstract BlockEnum[] PossibleBlockTypes();
        //dictionary indexed by a level which indexes a dictionary that indexes image by block type.
        private Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, SKBitmap>>> CachedImageData = new Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, SKBitmap>>>();

        private Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, Image>>> CachedImageDataGDI = new Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, Image>>>();


        protected SKBitmap GetMappedImageSkia(TetrisField field, Nomino Element, NominoElement Block,BlockEnum BlockTypeIndex)
        {
            var LevelIndex = (field.Handler.Statistics is TetrisStatistics ts) ? ts.Level : 0;
            if (!CachedImageData.ContainsKey(LevelIndex))
            {
                CachedImageData.Add(LevelIndex, new Dictionary<BlockEnum, Dictionary<Type, SKBitmap>>());
            }
            if (!CachedImageData[LevelIndex].ContainsKey(BlockTypeIndex))
            {
                CachedImageData[LevelIndex].Add(BlockTypeIndex, new Dictionary<Type, SKBitmap>());
            }
            if (!CachedImageData[LevelIndex][BlockTypeIndex].ContainsKey(Element.GetType()))
            {

                SKBitmap buildbitmap = DrawMappedImageSkia(field, Element,Block, BlockTypeIndex);
                CachedImageData[LevelIndex][BlockTypeIndex].Add(Element.GetType(), buildbitmap);

            }


            return CachedImageData[LevelIndex][BlockTypeIndex][Element.GetType()];


        }

        private SKBitmap DrawMappedImageSkia(TetrisField field, Nomino Element, NominoElement Block,BlockEnum BlockTypeIndex)
        {
            SKPointI blocksize = GetBlockSize(field, BlockTypeIndex);
            SKImageInfo drawinfo = new SKImageInfo(blocksize.X, blocksize.Y, SKColorType.Rgba8888, SKAlphaType.Premul);

            SKBitmap drawimage = new SKBitmap(drawinfo, SKBitmapAllocFlags.ZeroPixels);
            SKCanvas skc = new SKCanvas(drawimage);
            skc.Clear(SKColors.Transparent);
            SKColor[][] blockpixels = GetBlockPixels(field, Element, Block,BlockTypeIndex);
            for (int y = 0; y < blocksize.Y; y++)
            {
                for (int x = 0; x < blocksize.X; x++)
                {
                    BCColor PixelColor = blockpixels[y][x];
                    skc.DrawPoint(new SKPoint(x, y), PixelColor);
                }
            }
            skc.Flush();
            return drawimage;

        }

        public sealed override void ApplyTheme(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {

            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {

                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    var chosenType = GetBlockType(Group, iterate, Field);
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc.BlockColor = Color.Black;
                    var flagvalues = GetBlockFlags(iterate);
                    if (flagvalues==BlockFlags.Rotatable)
                    {
                        
                        sbc._RotationImagesSK = NominoTheme.GetImageRotations(GetMappedImageSkia(Field, Group, iterate,chosenType.BlockType));
                    }
                    else if (flagvalues == BlockFlags.Static)
                    {
                        sbc._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetMappedImageSkia(Field, Group, iterate,chosenType.BlockType)) };
                    }
                    else if(flagvalues == BlockFlags.CustomSelector)
                    {
                        sbc._RotationImagesSK = ApplyFunc_Custom(Field, Group,iterate.Block, chosenType.BlockType);
                    }
                }
            }

        }
        public sealed override void ApplyRandom(Nomino Group, IGameCustomizationHandler GameHandler,TetrisField Field)
        {
            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    var chosenType = TetrisGame.Choose(PossibleBlockTypes());
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc.BlockColor = Color.Black;
                    var Flags = GetBlockFlags(iterate);
                    if(Flags == BlockFlags.Rotatable)
                    {
                        sbc._RotationImagesSK = NominoTheme.GetImageRotations(GetMappedImageSkia(Field, Group,iterate, chosenType));
                    }
                    else if (Flags== BlockFlags.Static) {
                        sbc._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetMappedImageSkia(Field, Group, iterate,chosenType)) };
                    }
                    else if(Flags==BlockFlags.CustomSelector)
                    {
                        sbc._RotationImagesSK = ApplyFunc_Custom(Field, Group, iterate.Block,chosenType);
                    }
                    
                }
            }
        }
       
        protected virtual SKImage[] ApplyFunc_Custom(TetrisField field,Nomino Group,NominoBlock Target,BlockEnum chosentype)
        {
            return null;
        }
        protected abstract BlockFlags GetBlockFlags(NominoElement testvalue);
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



    }



}
