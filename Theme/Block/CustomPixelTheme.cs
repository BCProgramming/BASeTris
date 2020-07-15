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
    public abstract class CustomPixelTheme<PixelEnum, BlockEnum> : TetrominoTheme
    {
        public enum BlockFlags
        {
            Static,
            Rotatable
        }
        public abstract SKPointI GetBlockSize(TetrisField field, BlockEnum BlockType);

        public abstract SKColor GetColor(TetrisField field, Nomino Element, BlockEnum BlockType, PixelEnum PixelType);

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
        public abstract Dictionary<BlockEnum, PixelEnum[][]> GetBlockTypeDictionary();

        public SKColor[][] GetBlockPixels(TetrisField field, Nomino Element, BlockEnum BlockTypeIndex)
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
                    createresult[yval][xval] = GetColor(field, Element, BlockTypeIndex, ColorType);
                }
            }
            return createresult;
        }

        public abstract BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field);
        public abstract BlockEnum GetBlockType(Nomino group, NominoElement element, TetrisField field);

        public abstract BlockEnum[] PossibleBlockTypes();
        //dictionary indexed by a level which indexes a dictionary that indexes image by block type.
        private Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, SKBitmap>>> CachedImageData = new Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, SKBitmap>>>();

        private Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, Image>>> CachedImageDataGDI = new Dictionary<int, Dictionary<BlockEnum, Dictionary<Type, Image>>>();

        private System.Drawing.Image GetMappedImageGDI(TetrisField field, Nomino Element, BlockEnum BlockTypeIndex)
        {
            var level = (field.Handler.Statistics is TetrisStatistics ts) ? ts.Level : 0;
            if (!CachedImageDataGDI.ContainsKey(level))
            {

                CachedImageDataGDI.Add(level, new Dictionary<BlockEnum, Dictionary<Type, Image>>());
            }
            if (!CachedImageDataGDI[level].ContainsKey(BlockTypeIndex))
            {
                CachedImageDataGDI[level].Add(BlockTypeIndex, new Dictionary<Type, Image>());
            }
            if (!CachedImageDataGDI[level][BlockTypeIndex].ContainsKey(Element.GetType()))
            {

                var SKresult = GetMappedImageSkia(field, Element, BlockTypeIndex);
                CachedImageDataGDI[level][BlockTypeIndex].Add(Element.GetType(), SkiaSharp.Views.Desktop.Extensions.ToBitmap(SKresult));

            }
            return CachedImageDataGDI[level][BlockTypeIndex][Element.GetType()];
        }
        private SKBitmap GetMappedImageSkia(TetrisField field, Nomino Element, BlockEnum BlockTypeIndex)
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

                SKBitmap buildbitmap = DrawMappedImageSkia(field, Element, BlockTypeIndex);
                CachedImageData[LevelIndex][BlockTypeIndex].Add(Element.GetType(), buildbitmap);

            }


            return CachedImageData[LevelIndex][BlockTypeIndex][Element.GetType()];


        }

        private SKBitmap DrawMappedImageSkia(TetrisField field, Nomino Element, BlockEnum BlockTypeIndex)
        {
            SKPointI blocksize = GetBlockSize(field, BlockTypeIndex);
            SKImageInfo drawinfo = new SKImageInfo(blocksize.X, blocksize.Y, SKColorType.Rgb888x, SKAlphaType.Opaque);

            SKBitmap drawimage = new SKBitmap(drawinfo, SKBitmapAllocFlags.ZeroPixels);
            SKCanvas skc = new SKCanvas(drawimage);
            SKColor[][] blockpixels = GetBlockPixels(field, Element, BlockTypeIndex);
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

        public sealed override void ApplyTheme(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {

            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {

                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    var chosenType = GetBlockType(Group, iterate, Field);
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc.BlockColor = Color.Black;
                    if (IsRotatable(iterate))
                    {
                        
                        sbc._RotationImages = TetrominoTheme.GetImageRotations(GetMappedImageGDI(Field, Group, chosenType));
                    }
                    else
                    {
                        sbc._RotationImages = new Image[] { GetMappedImageGDI(Field, Group, chosenType) };
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
                    if(IsRotatable(iterate))
                    {
                        sbc._RotationImages = TetrominoTheme.GetImageRotations(GetMappedImageGDI(Field, Group, chosenType));
                    }
                    else {
                        sbc._RotationImages = new Image[] { GetMappedImageGDI(Field, Group, chosenType) };
                    }
                    
                }
            }
        }
        protected abstract bool IsRotatable(NominoElement testvalue);
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
