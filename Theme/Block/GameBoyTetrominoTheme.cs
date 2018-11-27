using System;
using System.Collections.Generic;
using System.Drawing;
using BASeTris.AssetManager;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public class GameBoyTetrominoTheme : TetrominoTheme
    {
        static readonly Size ImageSize;
        static GameBoyTetrominoTheme() 
        {
            I_Right_Cap = TetrisGame.Imageman.getLoadedImage("mottle_right_cap", 0.25f);
            I_Left_Cap = TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap", 0.25f);
            I_Horizontal = TetrisGame.Imageman.getLoadedImage("mottle_horizontal", 0.25f);
            Solid_Square = TetrisGame.Imageman.getLoadedImage("standard_square", 0.25f);
            Dotted_Dark = TetrisGame.Imageman.getLoadedImage("dark_dotted", 0.25f);
            Dotted_Light = TetrisGame.Imageman.getLoadedImage("light_dotted", 0.25f);
            Fat_Dotted_Light = TetrisGame.Imageman.getLoadedImage("lighter_big_dotted", 0.25f);
            Inset_Bevel = TetrisGame.Imageman.getLoadedImage("solid_beveled", 0.25f);
            ImageSize = I_Right_Cap.Size;



        }
        private Bitmap LightImage = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field)
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
        }

        protected Image GetRightCap(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Right_Cap, "Right_Cap", LevelColor);
        }
        protected Image GetLeftCap(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Left_Cap, "Left_Cap", LevelColor);
        }
        protected Image GetHorizontal(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(I_Horizontal, "Horizontal", LevelColor);
        }
        protected Image GetSolidSquare(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Solid_Square, "Solid_Square", LevelColor);
        }
        protected Image GetDottedDark(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Dark, "Dotted_Dark", LevelColor);
        }
        protected Image GetDottedLight(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Dotted_Light, "Dotted_Light", LevelColor);
        }
        protected Image GetInsetBevel(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Inset_Bevel, "Inset_Bevel", LevelColor);
        }
        protected Image GetFatDotted(int Level)
        {
            Color LevelColor = GetLevelColor(Level);
            return GetCached(Fat_Dotted_Light, "Fat_Dotted", LevelColor);
        }
        protected Image GetCached(Image Original,String pKey,Color pColor)
        {
            var found = GetCachedImage(pKey, ImageSize, pColor);
            if(found==null)
            {
                Image Recolored = StandardColouredBlock.RecolorImage(Original, pColor);
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

        public override void ApplyTheme(BlockGroup Group, TetrisField Field)
        {
            int CurrLevel = Field == null ? 0 : (int)(Field.LineCount / 10);

            if(Group is Tetromino_L)
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
            else
            {
                foreach (var blockcheck in Group)
                {
                    if (blockcheck.Block is StandardColouredBlock)
                    {
                        StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                        scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                        TetrisGame.Choose(new Image[] { GetSolidSquare(CurrLevel), GetDottedLight(CurrLevel), GetDottedDark(CurrLevel), GetFatDotted(CurrLevel), GetInsetBevel(CurrLevel) });
                        scb._RotationImages = new Image[] { GetInsetBevel(CurrLevel) };
                        //scb.BaseImageKey = Solid_Square; 
                    }
                }
            }
        }

        public void Apply_L(Tetromino_L Group, TetrisField Field,int CurrLevel)
        {
            //L block is a solid darker colour.
            
            foreach(var blockcheck in Group)
            {
                if(blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetSolidSquare(CurrLevel) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_J(Tetromino_J Group, TetrisField Field,int CurrLevel)
        {
            //darker outline with a middle white square.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedLight(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }

        }
        static float ReductionFactor = 0.5f;
        public void Apply_I(Tetromino_I Group, TetrisField Field,int CurrLevel)
        {
            //mottled. need to set rotation images as well.

            //we have four indices:
            //index one is left side
            //index two is left middle
            //index three is right middle
            //index four is right side.
            var BlockData = Group.GetBlockData();
            var LeftSide = BlockData[0];
            var LeftMiddle = BlockData[1];
            var RightMiddle = BlockData[2];
            var RightSide = BlockData[3];
                    
            if(LeftSide.Block is StandardColouredBlock)
            {
                var scb = (LeftSide.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetLeftCap(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("FLIPX:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT90:mottle_right_cap",ReductionFactor),
                //        TetrisGame.Imageman.getLoadedImage("FLIPXROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("FLIPXROT270:mottle_right_cap",ReductionFactor) };
            }
            Image i;
            if(LeftMiddle.Block is StandardColouredBlock)
            {
                var scb = (LeftMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetHorizontal(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }

            if (RightMiddle.Block is StandardColouredBlock)
            {
                var scb = (RightMiddle.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetHorizontal(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_horizontal",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_horizontal",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_horizontal",ReductionFactor) };
            }
            if (RightSide.Block is StandardColouredBlock)
            {
                var scb = (RightSide.Block as StandardColouredBlock);
                scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                scb._RotationImages = TetrominoTheme.GetImageRotations(GetRightCap(CurrLevel));
                //scb._RotationImages = new Image[] {TetrisGame.Imageman.getLoadedImage("mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT90:mottle_right_cap",ReductionFactor),
                //    TetrisGame.Imageman.getLoadedImage("ROT180:mottle_right_cap",ReductionFactor), TetrisGame.Imageman.getLoadedImage("ROT270:mottle_right_cap",ReductionFactor) };
            }

            
            
        }
        public void Apply_Z(Tetromino_Z Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, darker colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedDark(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_O(Tetromino_O Group, TetrisField Field,int CurrLevel)
        {
            //white, inset block
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] {GetFatDotted(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_S(Tetromino_S Group, TetrisField Field,int CurrLevel)
        {
            //dotted center, light colour.
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetDottedLight(CurrLevel) };
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }
        public void Apply_T(Tetromino_T Group, TetrisField Field,int CurrLevel)
        {
            //inset bevel
            foreach (var blockcheck in Group)
            {
                if (blockcheck.Block is StandardColouredBlock)
                {
                    StandardColouredBlock scb = blockcheck.Block as StandardColouredBlock;
                    scb.DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    scb._RotationImages = new Image[] { GetInsetBevel(CurrLevel)};
                    //scb.BaseImageKey = Solid_Square; 
                }
            }
        }

    }
}