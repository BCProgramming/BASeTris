using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using BASeTris.AssetManager;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    //This one ought to be removed. It's sort of outdone by the bitmap based Connected Block theme.
    //[HandlerTheme("Outlined", typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("WIP.")]
    /// <summary>
    /// Theme which has tetrominoes be outlined rather than draw as distinct blocks.
    /// </summary>
    public class OutlinedTetrominoTheme : NominoTheme
    {
        public override string Name { get { return "Outlined"; } }
        [Flags]
        public enum BlockOutlines
        {
            Outline_Top = 1,
            Outline_Left = 2,
            Outline_Bottom=4,
            Outline_Right = 8,
            Square_Top_Left = 16,
            Square_Top_Right = 32,
            Square_Bottom_Right = 64,
            Square_Bottom_Left = 128
        }


        private Dictionary<String, Image> StoredImages = new Dictionary<String, Image>();
        Size StandardDrawSize = new Size(125,125);
        Color StandardDrawColor = Color.Red;
        Pen OutlinePen = new Pen(Color.Black, 25);
        float PenWidth = 25;
        private String getCacheKey(BlockOutlines outline, Color BaseColor)
        {
            return "(" + BaseColor.R + "," + BaseColor.G + "," + BaseColor.B + ")-" + ((int)outline).ToString();
        }
        private Image GetOutlinedImage(BlockOutlines outline,Color BaseColor)
        {
            string sKey = getCacheKey(outline, BaseColor);
            if(!StoredImages.ContainsKey(sKey))
            {
                Bitmap BuildImage = new Bitmap(StandardDrawSize.Width,StandardDrawSize.Height);
                using (Graphics g = Graphics.FromImage(BuildImage))
                {
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.Clear(BaseColor);
                }
                var ResultImage = DrawOutline(BuildImage, outline, OutlinePen);
                StoredImages.Add(sKey,ResultImage);
            }
            return StoredImages[sKey];
        }
        private Image DrawOutline(Image Target, BlockOutlines pOutlines, Pen pPen)
        {
            Bitmap BuildResult = new Bitmap(Target);
            using (Graphics g = Graphics.FromImage(BuildResult))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                if (pOutlines.HasFlag(BlockOutlines.Outline_Top))
                {
                    g.DrawLine(pPen, 0, 0, BuildResult.Width, 0);
                }

                if (pOutlines.HasFlag(BlockOutlines.Outline_Bottom))
                {
                    g.DrawLine(pPen, 0,BuildResult.Height, BuildResult.Width, BuildResult.Height);
                }
                if (pOutlines.HasFlag(BlockOutlines.Outline_Left))
                {
                    g.DrawLine(pPen, 0, 0, 0, BuildResult.Height);
                }
                if (pOutlines.HasFlag(BlockOutlines.Outline_Right))
                {
                    g.DrawLine(pPen, BuildResult.Width, 0, BuildResult.Width, BuildResult.Height);
                }
                //TODO: handle drawing dotted corners...

                if(pOutlines.HasFlag(BlockOutlines.Square_Top_Left))
                {
                    g.FillRectangle(pPen.Brush,new RectangleF(0,0,PenWidth,PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Top_Right))
                {
                    g.FillRectangle(pPen.Brush,new RectangleF(Target.Width-PenWidth,0,PenWidth,PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Bottom_Left))
                {
                    g.FillRectangle(pPen.Brush, new RectangleF(0, Target.Height-PenWidth, PenWidth, PenWidth));
                }
                else if(pOutlines.HasFlag(BlockOutlines.Square_Bottom_Right))
                {
                    g.FillRectangle(pPen.Brush, new RectangleF(Target.Width - PenWidth, Target.Height - PenWidth, PenWidth, PenWidth));
                }
            }
            return BuildResult;
        }

        public OutlinedTetrominoTheme()
        {

        }

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler Handler,TetrisField Field)
        {
            int LevelUse = TetrisGame.StatelessRandomizer.Next(10);

            ApplyImages(Group, new Image[] { GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Right | BlockOutlines.Outline_Left, GetLevelColor(LevelUse)) });

        }

        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler handler,TetrisField Field, ThemeApplicationReason Reason)
        {
            var LineCount = handler==null?0:(handler.Statistics is TetrisStatistics ts) ? ts.LineCount : 0;
            int CurrLevel = Field == null ? 0 : (int)(LineCount / 10);
            if (Group is Tetromino_I)
                ApplyTheme_I(Group, CurrLevel);
            else if (Group is Tetromino_J)
                ApplyTheme_J(Group, CurrLevel);
            else if (Group is Tetromino_L)
                ApplyTheme_L(Group, CurrLevel);
            else if (Group is Tetromino_O)
                ApplyTheme_O(Group, CurrLevel);
            else if (Group is Tetromino_S)
                ApplyTheme_S(Group, CurrLevel);
            else if (Group is Tetromino_Z)
                ApplyTheme_Z(Group, CurrLevel);
            else if (Group is Tetromino_T)
                ApplyTheme_T(Group, CurrLevel);
            else
                ApplyImages(Group,new Image[]{GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Right | BlockOutlines.Outline_Left,GetLevelColor(CurrLevel))});
                
        }
        private Color[] ColorArray = null;
        private Color GetLevelColor(int Level)
        {
            if (ColorArray == null)
            {
                var BaseColor = Color.Red;
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

        private void ApplyImages(Nomino Target,Image[] BlockImages)
        {
            var BlockData = Target.GetBlockData();
            
            for(int i=0;i<BlockData.Count;i++)
            {
                
                if (BlockData[i].Block is StandardColouredBlock)
                {
                    ((StandardColouredBlock)BlockData[i].Block).DisplayStyle = StandardColouredBlock.BlockStyle.Style_Custom;
                    ((StandardColouredBlock)BlockData[i].Block)._RotationImagesSK =  (from p in GetImageRotations(BlockImages[i]) select SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(p))).ToArray();
                }
            }
        }

        protected void ApplyTheme_I(Nomino Target,int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top|BlockOutlines.Outline_Right ,baseColor)
            });
            
        }
        protected void ApplyTheme_J(Nomino Target,int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Right | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom | BlockOutlines.Square_Top_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Bottom|BlockOutlines.Outline_Right ,baseColor)
            });
        }

        protected void ApplyTheme_L(Nomino Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top | BlockOutlines.Outline_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Top ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Right | BlockOutlines.Outline_Bottom |BlockOutlines.Square_Top_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left|BlockOutlines.Outline_Right ,baseColor)
            });
        }
        protected void ApplyTheme_O(Nomino Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Right,baseColor)
            });
        }

        protected void ApplyTheme_Z(Nomino Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Right|BlockOutlines.Square_Bottom_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }
        protected void ApplyTheme_S(Nomino Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right |BlockOutlines.Square_Top_Left,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }

        protected void ApplyTheme_T(Nomino Target, int CurrentLevel)
        {
            Color baseColor = GetLevelColor(CurrentLevel);
            ApplyImages(Target, new Image[]
            {
                GetOutlinedImage(BlockOutlines.Outline_Bottom|BlockOutlines.Square_Bottom_Right | BlockOutlines.Square_Bottom_Left ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Top | BlockOutlines.Outline_Left | BlockOutlines.Outline_Bottom ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Left | BlockOutlines.Outline_Top | BlockOutlines.Outline_Right ,baseColor),
                GetOutlinedImage(BlockOutlines.Outline_Bottom | BlockOutlines.Outline_Right | BlockOutlines.Outline_Top,baseColor)
            });
        }

        PlayFieldBackgroundInfo ColoredBG = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field,IBlockGameCustomizationHandler Handler)
        {
            if (ColoredBG == null) ColoredBG = GetColoredBackground(Color.AntiqueWhite, null);

            return ColoredBG;
        }
    }
}