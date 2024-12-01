using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BASeTris.AssetManager;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;
using BASeTris.GameStates.Menu;
using BASeTris.Theme.Block;

namespace BASeTris
{




  


    [HandlerTheme("Game Boy I Style", typeof(StandardTetrisHandler))]
    [ThemeDescription("The Game Boy I piece theme on all.")]
    public class GameBoyMottledTheme : GameBoyTetrominoTheme
    {
        public GameBoyMottledTheme()
        {
            ForceType = GameBoyBlockSelections.Mottled;
        }
    }


    [HandlerTheme("Game Boy Style",typeof(StandardTetrisHandler))]
    [ThemeDescription("The style from the Game Boy Game. Inexplicably upscaled.")]
    public class GameBoyTetrominoTheme : NominoTheme
    {
        public GameBoyBlockSelections? ForceType = null;
        public enum GameBoyBlockSelections
        {
            Standard,
            Dark_Dotted,
            Light_Dotted,
            Lighter_Big_Dotted,
            Solid_Beveled,
            Mottled
        }
        
        public override String Name { get { return "Game Boy"; } }
        static Size ImageSize;
        static GameBoyTetrominoTheme() 
        {
          



        }
        private Image GetMottleImage(Nomino n, NominoElement element,int level)
        {
            var BlockFlags = NominoTheme.GetAdjacentBlockFlags(n, element, true);
            return GetMottleImage(BlockFlags,level);
        }
        int mottle_generated = 0;
        private Image GetMottleImage(AdjacentBlockFlags abf,int pLevel)
        {
            
            if (!AdjacentMottles.ContainsKey(abf))
            {
                List<Image> Overlays = new List<Image>();
                Bitmap BuildResult = new Bitmap(I_Fill);

                Dictionary<AdjacentBlockFlags, Image> OverlayDict = new Dictionary<AdjacentBlockFlags, Image>()
                {
                    { AdjacentBlockFlags.Left , Line_Left },
                    { AdjacentBlockFlags.Top , Line_Top },
                    { AdjacentBlockFlags.Right , Line_Right },
                    { AdjacentBlockFlags.Bottom , Line_Bottom },
                    { AdjacentBlockFlags.TopLeft , Corner_Top_Left},
                    { AdjacentBlockFlags.TopRight , Corner_Top_Right},
                    { AdjacentBlockFlags.BottomRight , Corner_Bottom_Right},
                    { AdjacentBlockFlags.BottomLeft , Corner_Bottom_Left},
                };
                foreach (var checkadjacents in OverlayDict)
                {
                    if (!abf.HasFlag(checkadjacents.Key))
                    {
                        Overlays.Add(checkadjacents.Value);
                    }
                }
                using (Graphics g = Graphics.FromImage(BuildResult))
                {
                    foreach (var dooverlay in Overlays)
                    {
                        g.DrawImage(dooverlay,new Rectangle(0,0,BuildResult.Width,BuildResult.Height));
                    }
                }
                AdjacentMottles[abf] = BuildResult;
                mottle_generated++;
                

            }
            return GetCached(new Bitmap(AdjacentMottles[abf]),"Mottle_" + (int)abf,GetLevelColor(pLevel));


        }
        private Dictionary<AdjacentBlockFlags, Bitmap> AdjacentMottles = new Dictionary<AdjacentBlockFlags, Bitmap>();

        bool Prepared = false;
        private void PrepareTheme()
        {
            if (Prepared) return;
            Line_Top = TetrisGame.Imageman.getLoadedImage("line_top", 0.25f);
            Line_Left = new Bitmap(Line_Top);
            Line_Bottom = new Bitmap(Line_Top);
            Line_Right = new Bitmap(Line_Top);
            Line_Right.RotateFlip(RotateFlipType.Rotate90FlipNone);
            Line_Left.RotateFlip(RotateFlipType.Rotate270FlipNone);
            Line_Bottom.RotateFlip(RotateFlipType.Rotate180FlipNone);
            

            Corner_Top_Left = TetrisGame.Imageman.getLoadedImage("mottle_corner", 0.25f);
            Corner_Top_Right = new Bitmap(Corner_Top_Left);
            Corner_Bottom_Right = new Bitmap(Corner_Top_Left);
            Corner_Bottom_Left = new Bitmap(Corner_Top_Left);
            Corner_Top_Right.RotateFlip(RotateFlipType.Rotate90FlipNone);
            Corner_Bottom_Right.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Corner_Bottom_Left.RotateFlip(RotateFlipType.Rotate270FlipNone);


            I_Fill = TetrisGame.Imageman.getLoadedImage("mottle_fill", 0.25f);
            I_Right_Cap = TetrisGame.Imageman.getLoadedImage("mottle_right_cap", 0.25f);
            I_Left_Cap = TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap", 0.25f);
            I_Horizontal = TetrisGame.Imageman.getLoadedImage("mottle_horizontal", 0.25f);
            Solid_Square = TetrisGame.Imageman.getLoadedImage("standard_square", 0.25f);
            Dotted_Dark = TetrisGame.Imageman.getLoadedImage("dark_dotted", 0.25f);
            Dotted_Light = TetrisGame.Imageman.getLoadedImage("light_dotted", 0.25f);
            Fat_Dotted_Light = TetrisGame.Imageman.getLoadedImage("lighter_big_dotted", 0.25f);
            Inset_Bevel = TetrisGame.Imageman.getLoadedImage("solid_beveled", 0.25f);
            ImageSize = I_Right_Cap.Size;
            Prepared = true;
        }
        private Bitmap LightImage = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(() =>
            {

                if (LightImage == null)
                {
                    LightImage = new Bitmap(250, 500);
                    using (Graphics drawdark = Graphics.FromImage(LightImage))
                    {
                        drawdark.Clear(Color.PeachPuff);
                    }
                }
                return new PlayFieldBackgroundInfo(LightImage, Color.Transparent);
            });

        }

        protected Image GetRightCap(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Right_Cap, "Right_Cap", LevelColor);
        }
        protected Image GetLeftCap(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Left_Cap, "Left_Cap", LevelColor);
        }
        protected Image GetHorizontal(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Horizontal, "Horizontal", LevelColor);
        }
        
        protected Image GetSolidSquare(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Solid_Square, "Solid_Square", LevelColor);
        }
        protected Image GetDottedDark(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Dark, "Dotted_Dark", LevelColor);
        }
        protected Image GetDottedLight(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Light, "Dotted_Light", LevelColor);
        }
        protected Image GetInsetBevel(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Inset_Bevel, "Inset_Bevel", LevelColor);
        }
        protected Image GetFatDotted(int Level)
        {
            PrepareTheme();
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Fat_Dotted_Light, "Fat_Dotted", LevelColor);
        }
        protected Image GetCached(Image Original,String pKey,Color pColor)
        {
            PrepareTheme();
            var found = GetCachedImage(pKey, ImageSize, pColor);
            if(found==null)
            {
                Image Recolored = GDIPlusHelpers.RecolorImage(Original, pColor);
                Recolored = GDIPlusHelpers.ResizeImage(Recolored, new Size(ImageSize.Width / 4, ImageSize.Height / 4));
                return AddCachedImage(pKey, ImageSize, pColor, Recolored);
            }
            else
            {
                return found;
            }
        }
        public GameBoyTetrominoTheme()
        {
            
        }
        private Color[] ColorArray = null;
        private Color GetLevelColor(int Level)
        {
            if (ColorArray == null)
            {
                var BaseColor = Color.OliveDrab;
                int ColorCount = 10;
                double Partitions = 240d / (double)ColorCount;
                List<Color> BuildColors = new List<Color>();
                for (int i = 0; i < ColorCount - 1; i++)
                {
                    BuildColors.Add(HSLColor.RotateHue(BaseColor, (int)(i * Partitions)));
                }
                ColorArray = BuildColors.ToArray();
            }
            return ColorArray[Level % (ColorArray.Length - 1)];


        }
        private static Image Solid_Square;
        private static Image Dotted_Light;
        private static Image Dotted_Dark;
        private static Image Fat_Dotted_Light;
        private static Image Inset_Bevel;
        private static Image I_Right_Cap;
        private static Image I_Horizontal;
        private static Image I_Left_Cap;
        private static Image I_Fill;
        private static Image Line_Top;
        private static Image Line_Right;
        private static Image Line_Bottom;
        private static Image Line_Left;
        private static Image Corner_Top_Left;
        private static Image Corner_Top_Right;
        private static Image Corner_Bottom_Left;
        private static Image Corner_Bottom_Right;

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler,TetrisField Field)
        {
            PrepareTheme();
            int RandomLevel = TetrisGame.StatelessRandomizer.Next(25);
            Action<Nomino,TetrisField,int> SelectL = Apply_L;
            Action<Nomino, TetrisField, int>[] Types = new Action<Nomino, TetrisField, int>[]
            {
                Apply_L,Apply_J,Apply_I,Apply_O,Apply_S,Apply_Z,Apply_T
            };
            var selected = TetrisGame.Choose(Types);
            selected(Group, Field, RandomLevel);
        }
        private Dictionary<String, GameBoyBlockSelections> ChosenBlockSelections = new Dictionary<string, GameBoyBlockSelections>();
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            PrepareTheme();
            var LineCount = GameHandler==null?0:(GameHandler.Statistics is TetrisStatistics ts) ? ts.LineCount : 0;
            int CurrLevel = Field == null ? 0 : (int)(LineCount / 10);
            if (ForceType!=null && ForceType.Value == GameBoyBlockSelections.Mottled)
            {
                ApplyStyle(Group, CurrLevel,GameBoyBlockSelections.Mottled);
            }
            else if(Group is Tetromino_L)
            {
                Apply_L(Group as Tetromino_L,Field,CurrLevel);
            }
            else if(Group is Tetromino_J)
            {
                Apply_J(Group as Tetromino_J, Field,CurrLevel);
            }
            else if(Group is Tetromino_I)
            {
                Apply_I(Group as Tetromino_I, Field,CurrLevel);
            }
            else if(Group is Tetromino_O)
            {
                Apply_O(Group as Tetromino_O, Field,CurrLevel);
            }
            else if(Group is Tetromino_S)
            {
                Apply_S(Group as Tetromino_S, Field,CurrLevel);
            }
            else if(Group is Tetromino_Z)
            {
                Apply_Z(Group as Tetromino_Z, Field,CurrLevel);
            }
            else if(Group is Tetromino_T)
            {
                Apply_T(Group as Tetromino_T, Field,CurrLevel);
            }
            else if(!(Group is Tetromino))
            {
                ApplyStyle(Group, CurrLevel);
            }
            else
            {
                foreach (var blockcheck in Group)
                {
                    if (blockcheck.Block is StandardColouredBlock)
                    {
                        StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        TetrisGame.Choose(new Image[] { GetSolidSquare(CurrLevel), GetDottedLight(CurrLevel), GetDottedDark(CurrLevel), GetFatDotted(CurrLevel), GetInsetBevel(CurrLevel) });
                        scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetInsetBevel(CurrLevel))) };
                        //scb.BaseImageKey = Solid_Square; 
                    }
                }
            }
        }

        private void ApplyStyle(Nomino Group, int CurrLevel, GameBoyBlockSelections? Style = null)
        {
            var nPoints = NNominoGenerator.GetNominoPoints(Group);
            GameBoyBlockSelections s;
            String sStrRep = NNominoGenerator.StringRepresentation(nPoints);
            if (!ChosenBlockSelections.ContainsKey(sStrRep))
            {
                s = Style != null?Style.Value: TetrisGame.Choose<GameBoyBlockSelections>((IEnumerable<GameBoyBlockSelections>)Enum.GetValues(typeof(GameBoyBlockSelections)));
                var nPoints2 = NNominoGenerator.RotateCW(nPoints);
                var nPoints3 = NNominoGenerator.RotateCW(nPoints2);
                var nPoints4 = NNominoGenerator.RotateCW(nPoints3);
                String strRep2 = NNominoGenerator.StringRepresentation(nPoints2);
                String strRep3 = NNominoGenerator.StringRepresentation(nPoints3);
                String strRep4 = NNominoGenerator.StringRepresentation(nPoints4);
                ChosenBlockSelections[sStrRep] = s;
                ChosenBlockSelections[strRep2] = s;
                ChosenBlockSelections[strRep3] = s;
                ChosenBlockSelections[strRep4] = s;

            }

            s = ChosenBlockSelections[sStrRep];
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock scb)
                {
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    Image useImage = null;
                    switch (s)
                    {
                        case GameBoyBlockSelections.Standard:
                            useImage = GetSolidSquare(CurrLevel);
                            break;
                        case GameBoyBlockSelections.Dark_Dotted:
                            useImage = GetDottedDark(CurrLevel);
                            break;
                        case GameBoyBlockSelections.Light_Dotted:
                            useImage = GetDottedLight(CurrLevel);
                            break;
                        case GameBoyBlockSelections.Lighter_Big_Dotted:
                            useImage = GetFatDotted(CurrLevel);
                            break;
                        case GameBoyBlockSelections.Solid_Beveled:
                            useImage = GetInsetBevel(CurrLevel);
                            break;
                    }
                    if (s == GameBoyBlockSelections.Mottled)
                    {
                        Image useMottleImage = GetMottleImage(Group, blockcheck, CurrLevel);
                        Image[] Rotations = NominoTheme.GetImageRotations(useMottleImage);
                        scb._RotationImagesSK = GetImageRotations(SKBitmap.FromImage(SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(useMottleImage))));
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    }
                    else
                    {
                        scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(useImage)) };
                    }

                }
            }
        }

        public void Apply_L(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //L block is a solid darker colour.
            
            foreach(var blockcheck in Group)
            {
                if(blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetSolidSquare(CurrLevel))) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_J(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //darker outline with a middle white square.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetDottedLight(CurrLevel)))};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }

        }
        static float ReductionFactor = 0.5f;
        public void Apply_I(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //mottled. need to set rotation images as well.

            //we have four indices:
            //index one is left side
            //index two is left middle
            //index three is right middle
            //index four is right side.
            var BlockData = Group.GetBlockData();

            if (BlockData.Count < 4)
            {
                BlockData = new List<NominoElement>(BlockData);
                while (BlockData.Count < 4)
                {
                    BlockData.Add(null);
                }
                BlockData = RandomHelpers.Static.Shuffle(BlockData,TetrisGame.StatelessRandomizer).ToList();
            }
            var LeftSide = BlockData[0];
            var LeftMiddle = BlockData[1];
            var RightMiddle = BlockData[2];
            var RightSide = BlockData[3];
                    
            if(LeftSide!=null && LeftSide.Block is StandardColouredBlock)
            {
                var scb = (LeftSide.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImagesSK = NominoTheme.GetImageRotations(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(GetLeftCap(CurrLevel))));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT90:mottle_right_cap",ReductionFactor),
                //        TetrisGame.Imageman.getLoadedImage("FLIPXROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT270:mottle_right_cap",ReductionFactor) };
            }
            if(LeftMiddle !=null && LeftMiddle.Block is StandardColouredBlock)
            {
                var scb = (LeftMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImagesSK = NominoTheme.GetImageRotations(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(GetHorizontal(CurrLevel))));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }

            if (RightMiddle!=null && RightMiddle.Block is StandardColouredBlock)
            {
                var scb = (RightMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImagesSK = NominoTheme.GetImageRotations(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(GetHorizontal(CurrLevel))));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }
            if (RightSide!=null && RightSide.Block is StandardColouredBlock)
            {
                var scb = (RightSide.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImagesSK = NominoTheme.GetImageRotations(SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(GetRightCap(CurrLevel))));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_right_cap",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_right_cap",ReductionFactor) };
            }

            
            
        }
        public void Apply_Z(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, darker colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] {  SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetDottedDark(CurrLevel)))};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_O(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //white, inset block
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] {SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(  GetFatDotted(CurrLevel)))};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_S(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, light colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetDottedLight(CurrLevel))) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_T(Nomino Group, TetrisField Field,int CurrLevel)
        {
            //inset bevel
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImagesSK = new SKImage[] { SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GetInsetBevel(CurrLevel))) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }

    }
}