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
       


        private Image Star_Image, Circle_Image, Diamond_Image, Heart_Image, Club_Image, Triangle_Image, Smiley_Image, Spade_Image,Speaker_Image,Exclamation_Image;
        private Image BlockSelect_Image;
        private void InitializeThemeData()
        {
            Star_Image = TetrisGame.Imageman["block_star",0.25f];
            Circle_Image = TetrisGame.Imageman["block_circle",0.25f];
            Diamond_Image = TetrisGame.Imageman["block_diamond", 0.25f];
            Heart_Image = TetrisGame.Imageman["block_heart", 0.25f];
            Club_Image = TetrisGame.Imageman["block_club", 0.25f];
            Triangle_Image = TetrisGame.Imageman["block_triangle", 0.25f];
            Smiley_Image = TetrisGame.Imageman["block_triangle", 0.25f];
            Speaker_Image = TetrisGame.Imageman["block_speaker", 0.25f];
            Spade_Image = TetrisGame.Imageman["block_spade", 0.25f];
            Exclamation_Image = TetrisGame.Imageman["block_exclamation", 0.25f];
            BlockSelect_Image = TetrisGame.Imageman["block_select"];
            BaseRedImages = new Dictionary<TetrisAttackHandler.TetrisAttackBlockTypes, Image>()
            {
                {TetrisAttackHandler.TetrisAttackBlockTypes.Star,Star_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Circle,Circle_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Diamond,Diamond_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Heart,Heart_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Club,Club_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Triangle,Triangle_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Smiley,Smiley_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Spade,Spade_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Speaker,Speaker_Image },
                {TetrisAttackHandler.TetrisAttackBlockTypes.Exclamation,Exclamation_Image },

            };

        }
        public Color GetStandardColor(TetrisAttackHandler.TetrisAttackBlockTypes blocktype) => blocktype switch
        {
            TetrisAttackHandler.TetrisAttackBlockTypes.Star => Color.Yellow,
            TetrisAttackHandler.TetrisAttackBlockTypes.Circle => Color.Green,
            TetrisAttackHandler.TetrisAttackBlockTypes.Diamond => Color.Purple,
            TetrisAttackHandler.TetrisAttackBlockTypes.Heart => Color.Red,
            TetrisAttackHandler.TetrisAttackBlockTypes.Club => Color.Blue,
            TetrisAttackHandler.TetrisAttackBlockTypes.Triangle => Color.SkyBlue,
            TetrisAttackHandler.TetrisAttackBlockTypes.Spade => Color.DarkGray,
            TetrisAttackHandler.TetrisAttackBlockTypes.Smiley => Color.OliveDrab,
            TetrisAttackHandler.TetrisAttackBlockTypes.Speaker => Color.Silver,
            TetrisAttackHandler.TetrisAttackBlockTypes.Exclamation => Color.LightSlateGray,
            _ => Color.LightSeaGreen
        };
        public TetrisAttackTheme()
        {
            InitializeThemeData();
        }
        private Dictionary<TetrisAttackHandler.TetrisAttackBlockTypes, Image> BaseRedImages = null;
        private Dictionary<TetrisAttackHandler.TetrisAttackBlockTypes, Dictionary<BCColor, Image>> BlockImageLookup = new Dictionary<TetrisAttackHandler.TetrisAttackBlockTypes, Dictionary<BCColor, Image>>();
        public override string Name => "Tetris Attack";


        private Image GetBlockImage(TetrisAttackHandler.TetrisAttackBlockTypes blocktype, BCColor color)
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

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            //Choose a random Type, then use the standard colour for it, then apply it.
            foreach (var iterate in Group)
            {
                if (iterate.Block is StandardColouredBlock)
                {
                    var choosetype = TetrisGame.Choose<TetrisAttackHandler.TetrisAttackBlockTypes>((IEnumerable<TetrisAttackHandler.TetrisAttackBlockTypes>)Enum.GetValues(typeof(TetrisAttackHandler.TetrisAttackBlockTypes)));
                    var useColor = GetStandardColor(choosetype);
                    StandardColouredBlock sbc = iterate.Block as StandardColouredBlock;
                    sbc.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    Bitmap useBitmap = new Bitmap(GetBlockImage(choosetype, useColor));
                    sbc._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(useBitmap) };
                }
            }
        }

        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
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
                        TetrisAttackHandler.TetrisAttackBlockTypes chosenType = (TetrisAttackHandler.TetrisAttackBlockTypes)lsb.CombiningIndex;
                        var useColor = GetStandardColor(chosenType);
                        lsb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        Bitmap useBitmap = new Bitmap(GetBlockImage(chosenType, useColor));
                        lsb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(useBitmap) };


                    }


                }
            }
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
}
