using BASeTris.Rendering.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme.Block
{
    public class BCTCollect<BCTEnum, PixelEnum> : BCTCollectBase
    {
        public override Type BCTType => typeof(BCTEnum);
        public override Type PixelType => typeof(PixelEnum);
    }
    public abstract class BCTCollectBase
    {
        public abstract Type BCTType { get; }
        public abstract Type PixelType { get; }
    }
    //class definition which is (supposed) to be part of allowing connectedblockthemes that use the BCT Custom Pixel definitions (and pixel mapping).
    public abstract class CachedImageDataByBCT<BCTType, PixelType> : CachedImageData<BCTCollect<BCTType, PixelType>>
    {
        protected CachedImageDataByBCT(BCTCollect<BCTType, PixelType> pDefaultKey, Func<BCTCollect<BCTType, PixelType>, SKImage, SKImage> pProcessFunc) : base(pDefaultKey, pProcessFunc)
        {
        }
    }



    //SKImage Data cache, keyed by SKColor.
    public class CachedImageDataByColor : CachedImageData<SKColor>
    {
        public CachedImageDataByColor() : base(SKColors.Red, (c, i) => TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(i, c))
        {
        }
        public override SKImage ApplyToDefault(SKColor src, SKImage StandardImage)
        {
            //applies the given color to the default.
            var recolored = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(StandardImage, src);
            return recolored;
        }
    }
    public class CachedImageDataByBlockInformation : CachedImageData<BlockColorInformation>
    {
        
        public CachedImageDataByBlockInformation() : base(SKColors.Red, (c, i) => TetrisStandardColouredBlockSkiaRenderingHandler.ProcessBlockImageInfo(i, c))
        {
        }
        public override SKImage ApplyToDefault(BlockColorInformation src, SKImage StandardImage)
        {
            //TODO: need to also support the color remapping dictionary, not just the colorization!
            //applies the given color to the default.
            if (src.ColorMapping != null)
            {
                var remapped = TetrisStandardColouredBlockSkiaRenderingHandler.RemapImageColors(StandardImage, src.ColorMapping);
                return remapped;
            }

            var recolored = TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(StandardImage, src);
            return recolored;
        }
    }

    public abstract class CachedImageData<Key> : GenericCachedData<Key, SKImage>
    {
        protected CachedImageData(Key pDefaultKey, Func<Key, SKImage, SKImage> pProcessFunc) : base(pDefaultKey, pProcessFunc)
        {
        }
        public override SKImage GetBlockFromDictionary(Dictionary<Key, SKImage> Input, Key src)
        {
            if (!Input.ContainsKey(src))
            {
                var defData = Input[DefaultKey];
                var applied = ApplyToDefault(src, defData);
                Input.Add(src, applied);
            }
            return Input[src];
        }
        public abstract SKImage ApplyToDefault(Key src, SKImage StandardImage);
    }
    public abstract class GenericCachedData
    {
        public enum BlockTypeConstants
        {
            Normal,
            Fixed,
            Shiny,
            Pop
        }
    }
    public abstract class GenericCachedData<Key, DataTag>:GenericCachedData
    {


        private Dictionary<String, Dictionary<Key, CardinalConnectionSet<DataTag, Key>>> ConnectedBlocks_Color = new Dictionary<string, Dictionary<Key, CardinalConnectionSet<DataTag, Key>>>();


        public Dictionary<Key, CardinalConnectionSet<DataTag, Key>> this[String context]
        {
            get
            {
                return GetDictionaryFromContextString(context);
            }
            set
            {
                ConnectedBlocks_Color[context] = value;
            }
        }
        public Dictionary<Key, CardinalConnectionSet<DataTag, Key>> GetDictionaryFromContextString(String pKey)
        {
            if (!ConnectedBlocks_Color.ContainsKey(pKey))
                ConnectedBlocks_Color.Add(pKey, new Dictionary<Key, CardinalConnectionSet<DataTag, Key>>());

            return ConnectedBlocks_Color[pKey];


        }

        public Dictionary<Key, CardinalConnectionSet<DataTag, Key>> NormalConnectedBlocks_Color
        {
            get
            {
                return GetDictionaryFromContextString("Normal");
                /*if(!ConnectedBlocks_Color.ContainsKey("Normal"))
                    ConnectedBlocks_Color.Add("Normal",new Dictionary<Key, CardinalConnectionSet<DataTag, Key>>());

                return ConnectedBlocks_Color["Normal"];*/
            }
            set
            {
                ConnectedBlocks_Color["Normal"] = value;
            }
        }
        public Dictionary<Key, CardinalConnectionSet<DataTag, Key>> FixedConnectedBlocks_Color
        {
            get
            {
                return GetDictionaryFromContextString("Fixed");
                /*if (!ConnectedBlocks_Color.ContainsKey("Fixed"))
                    ConnectedBlocks_Color.Add("Fixed", new Dictionary<Key, CardinalConnectionSet<DataTag, Key>>());

                return ConnectedBlocks_Color["Fixed"];*/
            }
            set
            {
                ConnectedBlocks_Color["Fixed"] = value;
            }
        }
        public Dictionary<Key, DataTag> NormalBlocks_Color = new Dictionary<Key, DataTag>();
        public Dictionary<Key, DataTag> FixedBlocks_Color = new Dictionary<Key, DataTag>();
        public Dictionary<Key, DataTag> ShinyBlocks_Color = new Dictionary<Key, DataTag>();
        public Dictionary<Key, DataTag> PopBlocks_Color = new Dictionary<Key, DataTag>();

        private Func<Key, DataTag, DataTag> ProcessFunc = null;
        protected Key DefaultKey;

        //(c, i) => TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(i, c)
        protected GenericCachedData(Key pDefaultKey, Func<Key, DataTag, DataTag> pProcessFunc)
        {
            DefaultKey = pDefaultKey;
            ProcessFunc = pProcessFunc;
        }
        public CardinalConnectionSet<DataTag, Key> GetFixedConnectedBlocks(Key src)
        {
            return GetConnectedBlocks(FixedConnectedBlocks_Color, src);
        }
        public CardinalConnectionSet<DataTag, Key> GetNormalConnectedBlocks(Key src)
        {
            return GetConnectedBlocks(NormalConnectedBlocks_Color, src);
            /*
            if (!NormalConnectedBlocks_Color.ContainsKey(src))
            {
                //red must be added first!
                var redSet = NormalConnectedBlocks_Color[DefaultKey];
                CardinalConnectionSet<DataTag, Key> newSet = new CardinalConnectionSet<DataTag, Key>(redSet, src, ProcessFunc);
                NormalConnectedBlocks_Color[src] = newSet;
            }
            return NormalConnectedBlocks_Color[src];*/
        }
        private CardinalConnectionSet<DataTag, Key> GetConnectedBlocks(Dictionary<Key, CardinalConnectionSet<DataTag, Key>> sourcedict, Key src)
        {
            if (!sourcedict.ContainsKey(src))
            {
                //red must be added first!
                var redSet = sourcedict[DefaultKey];
                CardinalConnectionSet<DataTag, Key> newSet = new CardinalConnectionSet<DataTag, Key>(redSet, src, ProcessFunc);
                sourcedict[src] = newSet;
            }
            return sourcedict[src];
        }
        public CardinalConnectionSet<DataTag, Key> GetConnectedBlocksByType(Key src, BlockTypeConstants btc,String sContext = null)
        {

            if (!String.IsNullOrEmpty(sContext))
            {
                var useDictionary = GetConnectedBlocks(GetDictionaryFromContextString(sContext),src);
                //var useDictionary = ConnectedBlocks_Color[sContext][src];
                return useDictionary;
            }
            else
            {
                return btc switch
                {
                    BlockTypeConstants.Fixed => GetFixedConnectedBlocks(src),
                    _ => GetNormalConnectedBlocks(src)
                };
            }
        }
        public DataTag GetNormalBlock(Key src)
        {
            return GetBlockFromDictionary(NormalBlocks_Color, src);
        }
        public DataTag GetFixedBlock(Key src)
        {
            return GetBlockFromDictionary(FixedBlocks_Color, src);
        }
        public DataTag GetShinyBlock(Key src)
        {
            return GetBlockFromDictionary(ShinyBlocks_Color, src);
        }
        public DataTag GetPopBlock(Key src)
        {
            return GetBlockFromDictionary(PopBlocks_Color, src);
        }
        public abstract DataTag GetBlockFromDictionary(Dictionary<Key, DataTag> Input, Key src);

        public DataTag GetBlock(String pContext, Key color, CardinalConnectionSet.ConnectedStyles pConnectionStyle)
        {
            //GetBlock
            return GetDictionaryFromContextString(pContext)[color][pConnectionStyle];
        }
        public DataTag GetBlock(String pContext, Key color)
        {
            return GetBlock(pContext, color, CardinalConnectionSet.ConnectedStyles.None);
        }
        public DataTag GetBlock(BlockTypeConstants btc, Key color)
        {
            return GetBlockFromDictionary(GetDictionaryForType(btc), color);
        }
        public Dictionary<Key, DataTag> GetDictionaryForType(BlockTypeConstants bt)
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
}
