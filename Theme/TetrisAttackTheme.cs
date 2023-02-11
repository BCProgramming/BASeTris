using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme
{
    [HandlerTheme("Tetris Attack", typeof(TetrisAttackHandler))]
    [ThemeDescription("Tetris Attack/Panel De Pon style blocks")]
    public class TetrisAttackTheme : NominoTheme
    {
        public enum TetrisAttackBlockTypes
        {
            Star,
            Circle,
            Diamond,
            Heart,
            Club,
            Triangle,
            Exclamation
        }


        private Image Star_Image, Circle_Image, Diamond_Image, Heart_Image, Club_Image, Triangle_Image, Exclamation_Image;
        private Image BlockSelect_Image;
        private void InitializeThemeData()
        {
            Star_Image = TetrisGame.Imageman["block_star",0.25f];
            Circle_Image = TetrisGame.Imageman["block_circle",0.25f];
            Diamond_Image = TetrisGame.Imageman["block_diamond", 0.25f];
            Heart_Image = TetrisGame.Imageman["block_heart", 0.25f];
            Club_Image = TetrisGame.Imageman["block_club", 0.25f];
            Triangle_Image = TetrisGame.Imageman["block_triangle", 0.25f];
            Exclamation_Image = TetrisGame.Imageman["block_exclamation", 0.25f];
            BlockSelect_Image = TetrisGame.Imageman["block_select"];
            BaseRedImages = new Dictionary<TetrisAttackBlockTypes, Image>()
            {
                {TetrisAttackBlockTypes.Star,Star_Image },
                {TetrisAttackBlockTypes.Circle,Circle_Image },
                {TetrisAttackBlockTypes.Diamond,Diamond_Image },
                {TetrisAttackBlockTypes.Heart,Heart_Image },
                {TetrisAttackBlockTypes.Club,Club_Image },
                {TetrisAttackBlockTypes.Triangle,Triangle_Image },
                {TetrisAttackBlockTypes.Exclamation,Exclamation_Image },

            };

        }
        public Color GetStandardColor(TetrisAttackBlockTypes blocktype) => blocktype switch
        {
            TetrisAttackBlockTypes.Star => Color.Yellow,
            TetrisAttackBlockTypes.Circle => Color.Green,
            TetrisAttackBlockTypes.Diamond => Color.Purple,
            TetrisAttackBlockTypes.Heart => Color.Red,
            TetrisAttackBlockTypes.Club => Color.Blue,
            TetrisAttackBlockTypes.Triangle => Color.SkyBlue,
            TetrisAttackBlockTypes.Exclamation => Color.LightSlateGray,
            _ => Color.LightSeaGreen
        };
        public TetrisAttackTheme()
        {
            InitializeThemeData();
        }
        private Dictionary<TetrisAttackBlockTypes, Image> BaseRedImages = null;
        private Dictionary<TetrisAttackBlockTypes, Dictionary<BCColor, Image>> BlockImageLookup = new Dictionary<TetrisAttackBlockTypes, Dictionary<BCColor, Image>>();
        public override string Name => "Tetris Attack";


        private Image GetBlockImage(TetrisAttackBlockTypes blocktype, BCColor color)
        {
            if (!BlockImageLookup.ContainsKey(blocktype))
                BlockImageLookup.Add(blocktype, new Dictionary<BCColor, Image>());


            if (!BlockImageLookup[blocktype].ContainsKey(color))
            {

                var basecolorimage = BaseRedImages[blocktype];
                //recolor it.
                Image Recolored = GDIPlusHelpers.RecolorImage(basecolorimage, color);
                BlockImageLookup[blocktype].Add(color, Recolored);

            }
            return BlockImageLookup[blocktype][color];
        }

        public override void ApplyRandom(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {
            //Choose a random Type, then use the standard colour for it, then apply it.
            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {
                    var choosetype = TetrisGame.Choose<TetrisAttackBlockTypes>((IEnumerable<TetrisAttackBlockTypes>)Enum.GetValues(typeof(TetrisAttackBlockTypes)));
                    var useColor = GetStandardColor(choosetype);
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    Bitmap useBitmap = new Bitmap(GetBlockImage(choosetype, useColor));
                    sbc._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(useBitmap) };
                }
            }
        }

        public override void ApplyTheme(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            if (Reason == ThemeApplicationReason.NewNomino)
            {
                foreach (var iterate in Group)
                {
                    if (iterate.Block is LineSeriesBlock lsb)
                    {
                        lsb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        Bitmap useBitmap = new Bitmap(BlockSelect_Image);
                        lsb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(useBitmap) };


                    }


                }

            }
            else
            {
                foreach (var iterate in Group)
                {
                    if (iterate.Block is LineSeriesBlock lsb)
                    {
                        TetrisAttackBlockTypes chosenType = (TetrisAttackBlockTypes)lsb.CombiningIndex;
                        var useColor = GetStandardColor(chosenType);
                        lsb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        Bitmap useBitmap = new Bitmap(GetBlockImage(chosenType, useColor));
                        lsb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(useBitmap) };


                    }


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
