using BASeTris.GameStates.GameHandlers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme
{
    public class TetrominoImageManager
    {

        private IGameCustomizationHandler GameHandler = null;
        private  TetrisField PlayField = null;
        public Dictionary<String, List<SKBitmap>> NominoSKBitmaps = null;
        public Dictionary<String, List<Image>> NominoImages { set; get; } = null;

        public void Reset()
        {
            var copiedGDI = NominoImages;
            var CopiedSK = NominoSKBitmaps;
            NominoImages = null;
            NominoSKBitmaps = null;
            //dispose the old ones too.
            if(CopiedSK!=null)
            foreach (var iterate in CopiedSK)
            {
                foreach (var skb in iterate.Value)
                {
                    skb.Dispose();
                }
            }
            if (copiedGDI != null)
                foreach (var iterate in copiedGDI)
                {
                    foreach (var bmp in iterate.Value)
                    {
                        bmp.Dispose();
                    }
                }

        }

        public TetrominoImageManager(IGameCustomizationHandler pHandler, TetrisField pField)
        {
            GameHandler = pHandler;
            PlayField = pField;
        }
        public SKBitmap AddTetrominoBitmapSK(IStateOwner pOwner, Nomino Source)
        {
            String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SKSize useTetSize = new SKSize(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Normal);

            SKBitmap buildBitmap = TetrisGame.OutlineImageSK(Source.GetImageSK(useTetSize));
            if (!NominoSKBitmaps.ContainsKey(sAddKey))
                NominoSKBitmaps.Add(sAddKey, new List<SKBitmap>() { buildBitmap });

            if (!NominoImages.ContainsKey(sAddKey))
            {
                Image useimage = SkiaSharp.Views.Desktop.Extensions.ToBitmap(buildBitmap);
                NominoImages[sAddKey] = new List<Image>() { useimage };
            }

            return buildBitmap;
        }
        public SKBitmap GetTetrominoSKBitmap(IStateOwner pOwner, Nomino nom)
        {
            String GetKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if (!NominoSKBitmaps.ContainsKey(GetKey))
            {
                return AddTetrominoBitmapSK(pOwner, nom);
            }
            return GetTetrominoSKBitmap(GetKey);
        }
        public SKBitmap GetTetrominoSKBitmap(Type sType)
        {
            String GetKey = PlayField.Theme.GetNominoTypeKey(sType, GameHandler, PlayField);
            return GetTetrominoSKBitmap(GetKey);
        }
        public SKBitmap GetTetrominoSKBitmap(String Source)
        {
            if (NominoSKBitmaps == null) NominoSKBitmaps = new Dictionary<String, List<SKBitmap>>();
            if (!NominoSKBitmaps.ContainsKey(Source))
            {
                if (NominoImages != null && NominoImages.ContainsKey(Source))
                {
                    NominoSKBitmaps.Add(Source, new List<SKBitmap>());
                    foreach (var copyGDI in NominoImages[Source])
                    {
                        NominoSKBitmaps[Source].Add(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(copyGDI)));
                    }
                }
                else
                {
                    var GetImage = GetTetrominoImage(Source);
                    if(GetImage!=null) return GetTetrominoSKBitmap(Source);
                    
                }

            }

            return TetrisGame.Choose(NominoSKBitmaps[Source]);

        }
        public Image AddTetrominoImage(IStateOwner pOwner, Nomino Source)
        {
            String sAddKey = PlayField.Theme.GetNominoKey(Source, GameHandler, PlayField);
            float useSize = 18 * (float)pOwner.ScaleFactor;
            SizeF useTetSize = new SizeF(useSize, useSize);


            PlayField.Theme.ApplyTheme(Source, GameHandler, PlayField, NominoTheme.ThemeApplicationReason.Normal);

            Image buildBitmap = TetrisGame.OutLineImage(Source.GetImage(useTetSize));
            if (!NominoImages.ContainsKey(sAddKey))
                NominoImages.Add(sAddKey, new List<Image>() { buildBitmap });


            return buildBitmap;

        }
       
        public Image GetTetrominoImage(IStateOwner pOwner, Nomino nom)
        {

            String sKey = PlayField.Theme.GetNominoKey(nom, GameHandler, PlayField);
            if (!NominoImages.ContainsKey(sKey))
            {
                return AddTetrominoImage(pOwner, nom);
            }
            return GetTetrominoImage(sKey);
        }
        public Image GetTetrominoImage(Type pType)
        {
            String sKey = PlayField.Theme.GetNominoTypeKey(pType, GameHandler, PlayField);
            return GetTetrominoImage(sKey);
        }
        public Image GetTetrominoImage(String TetrominoType)
        {
            if (!HasTetrominoImages()) return null;
            return TetrisGame.Choose(NominoImages[TetrominoType]);
        }
        public bool HasTetrominoSKBitmaps() => NominoSKBitmaps != null;
        public void SetTetrominoSKBitmaps(Dictionary<String, List<SKBitmap>> bitmaps)
        {
            NominoSKBitmaps = bitmaps;
        }
        public bool HasTetrominoImages() => NominoImages != null;
        public SKBitmap[] GetTetrominoSKBitmaps() => TetrisGame.Coalesce(NominoSKBitmaps);
        public Image[] GetTetronimoImages() => TetrisGame.Coalesce(NominoImages);
        public void SetTetrominoImages(Dictionary<String, List<Image>> images)
        {
            NominoImages = images;
        }
        

    }
}
