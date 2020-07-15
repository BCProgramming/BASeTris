using BASeTris.Rendering.Adapters;
using BASeTris.Blocks;
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
    /// </summary>
    public abstract class MappedPixelBlockTheme:TetrominoTheme 
    {
        public abstract SKPointI GetBlockSize(TetrisField field, int BlockTypeIndex); //retrieve the bitmap size to be used. the returned array from GetBlockPixels should have the same dimensions.

        public abstract SKColor GetColor(TetrisField field,Nomino Element, int BlockTypeIndex, int X, int Y);
        public virtual SKColor[][] GetBlockPixels(TetrisField field, Nomino Element,int BlockTypeIndex)
        {

            SKColor[][] createresult;
            SKPointI blocksize = GetBlockSize(field, BlockTypeIndex);
            createresult = new SKColor[blocksize.Y][];
            for(int yval=0;yval<blocksize.Y;yval++)
            {
                createresult[yval] = new SKColor[blocksize.X];
                for(int xval=0;xval<blocksize.X;xval++)
                {
                    createresult[yval][xval] = GetColor(field,Element, BlockTypeIndex, xval, yval);
                }


            }
            return createresult;
        }

        public abstract int GetBlockType(Nomino group, NominoElement element, TetrisField field);

        public abstract int[] PossibleBlockTypes();
        //dictionary indexed by a level which indexes a dictionary that indexes image by block type.
        private Dictionary<int, Dictionary<int, Dictionary<Type,SKBitmap>>> CachedImageData = new Dictionary<int, Dictionary<int, Dictionary<Type,SKBitmap>>>();

        private Dictionary<int, Dictionary<int, Dictionary<Type,Image>>> CachedImageDataGDI = new Dictionary<int, Dictionary<int, Dictionary<Type,Image>>>();

        private System.Drawing.Image GetMappedImageGDI(TetrisField field, Nomino Element,int BlockTypeIndex)
        {
            var level = field.Level;
            if (!CachedImageDataGDI.ContainsKey(level))
            {

                CachedImageDataGDI.Add(level, new Dictionary<int, Dictionary<Type, Image>>());
            }
            if (!CachedImageDataGDI[level].ContainsKey(BlockTypeIndex))
            {
                CachedImageDataGDI[level].Add(BlockTypeIndex, new Dictionary<Type, Image>());
            }
            if(!CachedImageDataGDI[level][BlockTypeIndex].ContainsKey(Element.GetType()))
            {
                
                var SKresult = GetMappedImageSkia(field,Element,BlockTypeIndex);
                CachedImageDataGDI[level][BlockTypeIndex].Add(Element.GetType(), SkiaSharp.Views.Desktop.Extensions.ToBitmap(SKresult));

            }
            return CachedImageDataGDI[level][BlockTypeIndex][Element.GetType()];
        }
        private SKBitmap GetMappedImageSkia(TetrisField field,Nomino Element,int BlockTypeIndex)
        {
            var LevelIndex = field.Level;
            if (!CachedImageData.ContainsKey(LevelIndex))
            {
                CachedImageData.Add(LevelIndex, new Dictionary<int, Dictionary<Type, SKBitmap>>());
            }
            if (!CachedImageData[LevelIndex].ContainsKey(BlockTypeIndex))
            {
                CachedImageData[LevelIndex].Add(BlockTypeIndex, new Dictionary<Type, SKBitmap>());
            }
            if(!CachedImageData[LevelIndex][BlockTypeIndex].ContainsKey(Element.GetType()))
            { 

                SKBitmap buildbitmap = DrawMappedImageSkia(field, Element, BlockTypeIndex);
                CachedImageData[LevelIndex][BlockTypeIndex].Add(Element.GetType(), buildbitmap);

            }

            
            return CachedImageData[LevelIndex][BlockTypeIndex][Element.GetType()];


        }
        private static SKImageInfo blockinfo = new SKImageInfo(9, 9, SKColorType.Rgb888x, SKAlphaType.Opaque);
        private SKBitmap DrawMappedImageSkia(TetrisField field,Nomino Element,int BlockTypeIndex)
        {
            SKPoint blocksize = GetBlockSize(field, BlockTypeIndex);

            SKBitmap drawimage = new SKBitmap(blockinfo, SKBitmapAllocFlags.ZeroPixels);
            SKCanvas skc = new SKCanvas(drawimage);
            SKColor[][] blockpixels = GetBlockPixels(field,Element, BlockTypeIndex);
            for (int y = 0; y < blocksize.Y; y++)
            {
                for (int x = 0; x < blocksize.X; x++)
                {

                    BCColor PixelColor = blockpixels[y][x];
                    
                    skc.DrawPoint(new SKPoint(x, y), PixelColor);
                    //drawimage.SetPixel(x, y, ChosenColor);

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
                    sbc._RotationImages = new Image[] { GetMappedImageGDI(Field,Group,chosenType)};
                }
            }
            
        }
        public sealed override void ApplyRandom(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {
            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    var chosenType = TetrisGame.Choose(PossibleBlockTypes());
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    sbc.BlockColor = Color.Black;
                    sbc._RotationImages = new Image[] { GetMappedImageGDI(Field, Group,chosenType) };
                }
            }
        }
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
