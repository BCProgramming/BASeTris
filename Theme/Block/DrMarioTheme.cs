using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Tetrominoes;
using SkiaSharp;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("Dr.Mario NES Style",typeof(DrMarioHandler))]
    public class DrMarioThemeEnhanced : DrMarioTheme
    {
        public override String Name { get { return "SNES Style"; } }
        public DrMarioThemeEnhanced() : base(InitializationFlags.Flags_EnhancedPillGraphics)
        {

        }

    }




    [HandlerTheme("Dr. Mario Style",typeof(DrMarioHandler))]
    //CascadingBlockTheme will need to specify the DrMario customization Handler as it's valid Theme once ready.
    //this one doesn't care about the game level- it has two block types- the pills, and the virii.
    //of those we've got 3 colors. We could add more, I suppose, but Dr. Mario has three so let's keep things a bit simpler.
    //ideally, we'd have some more generic way of defining different colours, rather than having to define all the different block types for each colour
    public class DrMarioTheme : CustomPixelTheme<DrMarioTheme.BCT, DrMarioTheme.BlockTypes>
    {
        [Flags]
        public enum InitializationFlags
        {
            Flags_None,
            Flags_EnhancedPillGraphics = 1
        }
        public bool UseEnhancedImages = true;
        public static bool AllowAdvancedRotations = true;
        public override String Name { get { return "NES Style"; } }
        public override string GetNominoKey(Nomino Group, IGameCustomizationHandler GameHandler, TetrisField Field)
        {

            //Dr Mario theme keys are based on the "Duomino" arrangement. We take the first type, and the second type and create a key for it.
            return base.GetNominoKey_LineSeries(Group, GameHandler, Field);

            //return base.GetNominoKey(Group, GameHandler, Field);
        }
        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb && lsb is LineSeriesPrimaryBlock)
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Static;
            }
            else
            {
                return AllowAdvancedRotations ? CustomPixelTheme<BCT, BlockTypes>.BlockFlags.CustomSelector : CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
            }
        }
        private static BCT[][] GetBCTBitmap(String ImageKey)
        {
            return GetBCTBitmap(ImageKey, ColorMapLookupFunc);
        }
        
        private static BCT ColorMapLookupFunc(SKColor Src)
        {
            if (bitmappixels_enhanced.ContainsKey(Src)) return bitmappixels_enhanced[Src]; else return BCT.Transparent;
        }
        private static readonly Dictionary<SKColor, BCT> bitmappixels = new Dictionary<SKColor, BCT>()
            {
            {SKColors.Transparent,BCT.Transparent },
            {SKColors.Black,BCT.Black },
            {SKColors.Red,BCT.Primary },
            {SKColors.Yellow,BCT.Accent },
            {SKColors.Cyan,BCT.Accent2}
            };

        private static readonly Dictionary<SKColor, BCT> bitmappixels_enhanced = new Dictionary<SKColor, BCT>()
        {
            {SKColors.Transparent,BCT.Transparent },
            {SKColors.Black,BCT.Black },
            {SKColors.Red,BCT.Primary },
            {SKColors.Yellow,BCT.Accent },
            {SKColors.Cyan,BCT.Accent2},
            {new SKColor(205,0,0),BCT.Enhanced_1 },
            {new SKColor(157,0,0), BCT.Enhanced_2 },
            {new SKColor(116,0,0),BCT.Enhanced_3 },
            {new SKColor(249,3,3),BCT.Enhanced_4 },
            {new SKColor(67,1,1),BCT.Enhanced_5 },
            {new SKColor(255,18,18),BCT.Enhanced_6 },
            {new SKColor(255,45,45),BCT.Enhanced_7 },
            {new SKColor(255,75,75),BCT.Enhanced_8},
            {new SKColor(255,103,103) ,BCT.Enhanced_9},
            {new SKColor(88,88,88) ,BCT.Enhanced_10},
            {new SKColor(1,1,1) ,BCT.Enhanced_11},
        };

       


        private static bool ThemeDataPrepared = false;
        private static void PrepareThemeData()
        {
            if (ThemeDataPrepared) return;
            Pill_Left = GetBCTBitmap("pill_left_enhanced");
            Pill_Single = GetBCTBitmap("pill_single_enhanced");
            Pill_Right = GetBCTBitmap("pill_right_enhanced");
            Pill_Top = GetBCTBitmap("pill_top_enhanced");
            Pill_Bottom = GetBCTBitmap("pill_bottom_enhanced");
            Pill_Pop = GetBCTBitmap("pill_pop");
            Red_Virii_1 = GetBCTBitmap("red_virus_1");
            Red_Virii_2 = GetBCTBitmap("red_virus_2");
            Yellow_Virii_1 = GetBCTBitmap("yellow_virus_1");
            Yellow_Virii_2 = GetBCTBitmap("yellow_virus_2");
            Blue_Virii_1 = GetBCTBitmap("blue_virus_1");
            Blue_Virii_2 = GetBCTBitmap("blue_virus_2");
            Green_Virii_1 = GetBCTBitmap("green_virus_1");
            Green_Virii_2 = GetBCTBitmap("green_virus_2");
            Orange_Virii_1 = GetBCTBitmap("orange_virus_1");
            Orange_Virii_2 = GetBCTBitmap("orange_virus_2");
            Magenta_Virii_1 = GetBCTBitmap("magenta_virus_1");
            Magenta_Virii_2 = GetBCTBitmap("magenta_virus_2");


            BitmapIndex = new Dictionary<BlockTypes, BCT[][]>()
        {
            {BlockTypes.Pill_Left_Yellow,Pill_Left },
            {BlockTypes.Pill_Top_Yellow,Pill_Top },
            {BlockTypes.Pill_Right_Yellow,Pill_Right },
            {BlockTypes.Pill_Bottom_Yellow,Pill_Bottom },
            {BlockTypes.Pill_Single_Yellow,Pill_Single },
            {BlockTypes.Pill_Left_Red,Pill_Left },
            {BlockTypes.Pill_Top_Red,Pill_Top },
            {BlockTypes.Pill_Right_Red,Pill_Right },
            {BlockTypes.Pill_Bottom_Red,Pill_Bottom },
            {BlockTypes.Pill_Single_Red,Pill_Single },
            {BlockTypes.Pill_Left_Blue,Pill_Left },
            {BlockTypes.Pill_Top_Blue,Pill_Top },
            {BlockTypes.Pill_Right_Blue,Pill_Right },
            {BlockTypes.Pill_Bottom_Blue,Pill_Bottom },
            {BlockTypes.Pill_Single_Blue,Pill_Single },

            {BlockTypes.Pill_Left_Orange,Pill_Left },
            {BlockTypes.Pill_Top_Orange,Pill_Top },
            {BlockTypes.Pill_Right_Orange,Pill_Right },
            {BlockTypes.Pill_Bottom_Orange,Pill_Bottom },
            {BlockTypes.Pill_Single_Orange,Pill_Single },

            {BlockTypes.Pill_Left_Magenta,Pill_Left },
            {BlockTypes.Pill_Top_Magenta,Pill_Top },
            {BlockTypes.Pill_Right_Magenta,Pill_Right },
            {BlockTypes.Pill_Bottom_Magenta,Pill_Bottom },
            {BlockTypes.Pill_Single_Magenta,Pill_Single },

            {BlockTypes.Pill_Left_Green,Pill_Left },
            {BlockTypes.Pill_Top_Green,Pill_Top },
            {BlockTypes.Pill_Right_Green,Pill_Right },
            {BlockTypes.Pill_Bottom_Green,Pill_Bottom },
            {BlockTypes.Pill_Single_Green,Pill_Single },

            {BlockTypes.Blue_Virus_1,Blue_Virii_1 },
            {BlockTypes.Blue_Virus_2,Blue_Virii_2 },
            {BlockTypes.Yellow_Virus_1,Yellow_Virii_1 },
            {BlockTypes.Yellow_Virus_2,Yellow_Virii_2 },
            {BlockTypes.Red_Virus_1,Red_Virii_1 },
            {BlockTypes.Red_Virus_2,Red_Virii_2 },
            {BlockTypes.Green_Virus_1,Green_Virii_1 },
            {BlockTypes.Green_Virus_2,Green_Virii_2 },
            {BlockTypes.Orange_Virus_1,Orange_Virii_1 },
            {BlockTypes.Orange_Virus_2,Orange_Virii_2 },
            {BlockTypes.Magenta_Virus_1,Magenta_Virii_1 },
            {BlockTypes.Magenta_Virus_2,Magenta_Virii_2 },
            {BlockTypes.Red_Pop,Pill_Pop},
            {BlockTypes.Blue_Pop,Pill_Pop},
            {BlockTypes.Yellow_Pop,Pill_Pop},
            {BlockTypes.Magenta_Pop,Pill_Pop},
            {BlockTypes.Orange_Pop,Pill_Pop},
            {BlockTypes.Green_Pop,Pill_Pop},


        };


            ThemeDataPrepared = true;
        }
        
        public DrMarioTheme(InitializationFlags flags)
        {
            if (flags.HasFlag(InitializationFlags.Flags_EnhancedPillGraphics)) UseEnhancedImages = true;
            PrepareThemeData();
        }
        public DrMarioTheme():this(InitializationFlags.Flags_None)
        {
        }

        static BCT[][] Tetris2NormalBlock;
        static BCT[][] Tetris2FixedBlock;
        static BCT[][] Tetris2ShinyBlock;

        static BCT[][] Pill_Left; /*= new BCT[][]
                    {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black,BCT.Primary, BCT.Primary , BCT.Accent , BCT.Accent, BCT.Accent, BCT.Accent, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Accent , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Black}
                    };*/
                                  //Bitmap for the left side of a pill.

        static BCT[][] Pill_Right; /*= new BCT[][]
                    {
                new BCT[]{BCT.Black,BCT.Black, BCT.Black , BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent},
                new BCT[]{BCT.Black,BCT.Accent, BCT.Accent , BCT.Accent , BCT.Accent, BCT.Accent, BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary , BCT.Primary, BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary ,BCT.Primary,BCT.Black},
                new BCT[]{BCT.Black, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent},
                new BCT[]{BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Transparent,BCT.Transparent}
                    };*/
        static BCT[][] Pill_Top;

        static BCT[][] Pill_Bottom;
        //bitmap for the right side of a pill.

        //note: pill rotations should be interesting, to get the "glint" to rotate correctly. though initially I think we can just have the glint rotate, too.
        static BCT[][] Pill_Single = null;
        //TODO: BCT Bitmaps for a Single pill piece, as well as the Red,Blue, and Yellow Virii.
        static BCT[][] Pill_Pop;
        //Frame 1 Yellow Virii
        static BCT[][] Yellow_Virii_1 = null; /* new BCT[][]
               {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Transparent, BCT.Primary, BCT.Primary , BCT.Primary , BCT.Transparent , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black , BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Accent, BCT.Black , BCT.Accent, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Black, BCT.Black,BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent }
               };*/
        //Frame 2 Yellow Virii unchanged currently...
        static BCT[][] Yellow_Virii_2 = null; /* new BCT[][]
               {
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Accent, BCT.Black , BCT.Accent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Accent2, BCT.Primary, BCT.Primary,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Accent2, BCT.Primary, BCT.Transparent, BCT.Transparent }
               };*/

        static BCT[][] Red_Virii_1 = null; /* new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent,BCT.Accent},
                new BCT[]{BCT.Transparent, BCT.Accent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Black, BCT.Accent2, BCT.Black , BCT.Accent2, BCT.Black , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Accent2}

              };*/
        static BCT[][] Red_Virii_2 = null; /*new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Accent, BCT.Transparent , BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Accent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Black, BCT.Accent2, BCT.Black , BCT.Accent2, BCT.Black , BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Black, BCT.Primary, BCT.Accent2},
                new BCT[]{BCT.Transparent, BCT.Accent2, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Black,BCT.Transparent}

              };*/
        static BCT[][] Blue_Virii_1 = null;/* new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Transparent, BCT.Transparent , BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent,BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Primary,BCT.Primary, BCT.Black, BCT.Primary, BCT.Black, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary,BCT.Transparent}

              };*/
        static BCT[][] Blue_Virii_2 = null; /*new BCT[][]
              {
                  new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Transparent },
                new BCT[]{BCT.Transparent,BCT.Primary, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary,BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent,BCT.Transparent,BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Transparent, BCT.Transparent, BCT.Transparent},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Accent, BCT.Black, BCT.Accent, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Primary, BCT.Black, BCT.Black, BCT.Accent, BCT.Black, BCT.Black, BCT.Primary},
                new BCT[]{BCT.Transparent, BCT.Transparent, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary, BCT.Primary,BCT.Transparent}

              };*/
        static BCT[][] Orange_Virii_1 = null;
        static BCT[][] Orange_Virii_2 = null;
        static BCT[][] Magenta_Virii_1 = null;
        static BCT[][] Magenta_Virii_2 = null;
        static BCT[][] Green_Virii_1 = null;
        static BCT[][] Green_Virii_2 = null;
        
        static Dictionary<BlockTypes, BCT[][]> BitmapIndex = null;
        static SKPointI nine = new SKPointI(9, 9);
        static SKPointI eight = new SKPointI(8, 8);
        static SKPointI doublenine = new SKPointI(18, 18);
        public static Dictionary<BlockTypes, SKPointI> TypeSizes = new Dictionary<BlockTypes, SKPointI>()
        {

            {BlockTypes.Pill_Left_Yellow,doublenine},
            {BlockTypes.Pill_Top_Yellow,doublenine},
            {BlockTypes.Pill_Right_Yellow,doublenine },
            {BlockTypes.Pill_Bottom_Yellow,doublenine },
            {BlockTypes.Pill_Single_Yellow,doublenine},
            {BlockTypes.Pill_Left_Red,doublenine },
            {BlockTypes.Pill_Top_Red,doublenine },
            {BlockTypes.Pill_Right_Red,doublenine},
            {BlockTypes.Pill_Bottom_Red,doublenine},
            {BlockTypes.Pill_Single_Red,doublenine},
            {BlockTypes.Pill_Left_Blue,doublenine},
            {BlockTypes.Pill_Top_Blue,doublenine},
            {BlockTypes.Pill_Right_Blue,doublenine},
            {BlockTypes.Pill_Bottom_Blue,doublenine},
            {BlockTypes.Pill_Single_Blue,doublenine},

            {BlockTypes.Pill_Left_Orange,doublenine},
            {BlockTypes.Pill_Top_Orange,doublenine},
            {BlockTypes.Pill_Right_Orange,doublenine},
            {BlockTypes.Pill_Bottom_Orange,doublenine},
            {BlockTypes.Pill_Single_Orange,doublenine},

            {BlockTypes.Pill_Left_Magenta,doublenine},
            {BlockTypes.Pill_Top_Magenta,doublenine},
            {BlockTypes.Pill_Right_Magenta,doublenine},
            {BlockTypes.Pill_Bottom_Magenta,doublenine},
            {BlockTypes.Pill_Single_Magenta,doublenine},

            {BlockTypes.Pill_Left_Green,doublenine},
            {BlockTypes.Pill_Top_Green,doublenine},
            {BlockTypes.Pill_Right_Green,doublenine},
            {BlockTypes.Pill_Bottom_Green,doublenine},
            {BlockTypes.Pill_Single_Green,doublenine},

            {BlockTypes.Blue_Virus_1,nine },
            {BlockTypes.Blue_Virus_2,nine},
            {BlockTypes.Yellow_Virus_1,nine},
            {BlockTypes.Yellow_Virus_2,nine},
            {BlockTypes.Red_Virus_1,nine },
            {BlockTypes.Red_Virus_2,nine},
            {BlockTypes.Orange_Virus_1,nine},
            {BlockTypes.Orange_Virus_2,nine},
            {BlockTypes.Magenta_Virus_1,nine},
            {BlockTypes.Magenta_Virus_2,nine},
            {BlockTypes.Green_Virus_1,nine},
            {BlockTypes.Green_Virus_2,nine},
            {BlockTypes.Red_Pop,doublenine},
            {BlockTypes.Yellow_Pop,doublenine},
            {BlockTypes.Blue_Pop,doublenine},
            {BlockTypes.Orange_Pop,doublenine},
            {BlockTypes.Magenta_Pop,doublenine},
            {BlockTypes.Green_Pop,doublenine}
        };

        static SKColor BlueColor = new SKColor(60, 188, 252);
        //Virii are animated, usually. We can deal with that later by implementing additional block types, with the chosen block type depending on the current timer to return one or another animation bitmap frame.
        //but, we'll deal with that as it comes up.

        public static Dictionary<BCT, SKColor> YellowColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,BlueColor  },
            {BCT.Accent2,SKColors.Red }

        };

        public static Dictionary<BCT, SKColor> EnhancedYellowColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Yellow },
            {BCT.Accent,BlueColor  },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(205,205,0) },
            {BCT.Enhanced_2,new SKColor(157,157,0) },
            {BCT.Enhanced_3,new SKColor(116,116,0) },
            {BCT.Enhanced_4,new SKColor(249,249,3) },
            {BCT.Enhanced_5,new SKColor(67,67,1) },
            {BCT.Enhanced_6,new SKColor(255,255,18) },
            {BCT.Enhanced_7,new SKColor(255,255,45) },
            {BCT.Enhanced_8,new SKColor(255,255,75) },
            {BCT.Enhanced_9,new SKColor(255,255,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };


        public static Dictionary<BCT, SKColor> RedColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Red },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,BlueColor  }

        };
        /*205,0,0
        157,0,0
        116,0,0
        249,3,3
        67,1,1
        255,18,18
        255,45,45
        255,75,75
        255,103,103
        88,88,88
        1,1,1*/
        public static Dictionary<BCT,SKColor> EnhancedRedColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Red  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(205,0,0) },
            {BCT.Enhanced_2,new SKColor(157,0,0) },
            {BCT.Enhanced_3,new SKColor(116,0,0) },
            {BCT.Enhanced_4,new SKColor(249,3,3) },
            {BCT.Enhanced_5,new SKColor(67,1,1) },
            {BCT.Enhanced_6,new SKColor(255,18,18) },
            {BCT.Enhanced_7,new SKColor(255,45,45) },
            {BCT.Enhanced_8,new SKColor(255,75,75) },
            {BCT.Enhanced_9,new SKColor(255,103,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        public static Dictionary<BCT, SKColor> BlueColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,BlueColor  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red }

        };
        public static Dictionary<BCT, SKColor> EnhancedBlueColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,BlueColor  },
            {BCT.Accent,SKColors.Yellow },
            {BCT.Accent2,SKColors.Red },
            {BCT.Enhanced_1,new SKColor(0,204,205) },
            {BCT.Enhanced_2,new SKColor(0,156,157) },
            {BCT.Enhanced_3,new SKColor(0,116,116) },
            {BCT.Enhanced_4,new SKColor(3,249,249) },
            {BCT.Enhanced_5,new SKColor(1,67,67) },
            {BCT.Enhanced_6,new SKColor(18,255,255) },
            {BCT.Enhanced_7,new SKColor(45,255,255) },
            {BCT.Enhanced_8,new SKColor(75,255,255) },
            {BCT.Enhanced_9,new SKColor(103,255,255) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };
        //weird idea, Green, Orange, and Magenta colours.
        public static Dictionary<BCT, SKColor> GreenColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Green  },
            {BCT.Accent,SKColors.GreenYellow },
            {BCT.Accent2,SKColors.DarkMagenta }

        };

        public static Dictionary<BCT, SKColor> EnhancedGreenColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Green  },
            {BCT.Accent,SKColors.GreenYellow },
            {BCT.Accent2,SKColors.DarkMagenta },
            {BCT.Enhanced_1,new SKColor(0,205,0) },
            {BCT.Enhanced_2,new SKColor(0,205,0) },
            {BCT.Enhanced_3,new SKColor(0,116,0) },
            {BCT.Enhanced_4,new SKColor(3,249,3) },
            {BCT.Enhanced_5,new SKColor(1,67,1) },
            {BCT.Enhanced_6,new SKColor(18,255,18) },
            {BCT.Enhanced_7,new SKColor(45,255,45) },
            {BCT.Enhanced_8,new SKColor(75,255,75) },
            {BCT.Enhanced_9,new SKColor(103,255,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        public static Dictionary<BCT, SKColor> OrangeColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Orange  },
            {BCT.Accent,SKColors.PaleGoldenrod },
            {BCT.Accent2,SKColors.LightBlue }

        };

        public static Dictionary<BCT, SKColor> EnhancedOrangeColourSet = new Dictionary<BCT, SKColor>()
            {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Orange  },
            {BCT.Accent,SKColors.PaleGoldenrod },
            {BCT.Accent2,SKColors.LightBlue },
            {BCT.Enhanced_1,new SKColor(205,103,0) },
            {BCT.Enhanced_2,new SKColor(157,75,0) },
            {BCT.Enhanced_3,new SKColor(116,55,0) },
            {BCT.Enhanced_4,new SKColor(249,128,3) },
            {BCT.Enhanced_5,new SKColor(67,30,1) },
            {BCT.Enhanced_6,new SKColor(255,128,18) },
            {BCT.Enhanced_7,new SKColor(255,128,45) },
            {BCT.Enhanced_8,new SKColor(255,128,75) },
            {BCT.Enhanced_9,new SKColor(255,50,103) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) },

            };

        static Dictionary<BCT, SKColor> MagentaColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Magenta  },
            {BCT.Accent,SKColors.Pink },
            {BCT.Accent2,SKColors.Orchid },
           

        };

        public static Dictionary<BCT, SKColor> EnhancedMagentaColourSet = new Dictionary<BCT, SKColor>()
        {
            {BCT.Transparent,SKColors.Transparent },
            {BCT.Black,SKColors.Black },
            {BCT.Primary,SKColors.Magenta  },
            {BCT.Accent,SKColors.Pink },
            {BCT.Accent2,SKColors.Orchid },
            {BCT.Enhanced_1,new SKColor(205,0,205) },
            {BCT.Enhanced_2,new SKColor(157,0,157) },
            {BCT.Enhanced_3,new SKColor(116,0,116) },
            {BCT.Enhanced_4,new SKColor(249,3,249) },
            {BCT.Enhanced_5,new SKColor(67,1,67) },
            {BCT.Enhanced_6,new SKColor(255,18,255) },
            {BCT.Enhanced_7,new SKColor(255,45,255) },
            {BCT.Enhanced_8,new SKColor(255,75,255) },
            {BCT.Enhanced_9,new SKColor(255,103,255) },
            {BCT.Enhanced_10,new SKColor(88,88,88) },
            {BCT.Enhanced_11,new SKColor(1,1,1) }

        };

        public override SKPointI GetBlockSize(TetrisField field, BlockTypes BlockType)
        {
            return TypeSizes[BlockType];
        }
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PopTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Pop },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Pop},
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Pop },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Orange_Pop },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Magenta_Pop },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Green_Pop }
            
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> VirusTypes_1 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Orange_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Magenta_Virus_1 },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Green_Virus_1 }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> VirusTypes_2 = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Red_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Yellow_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Blue_Virus_2 },
            { LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Orange_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Magenta_Virus_2 },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Green_Virus_2 },
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillLeft = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Left_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Left_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Left_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pill_Left_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pill_Left_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pill_Left_Green }
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillTop = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Top_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Top_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Top_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pill_Top_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pill_Top_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pill_Top_Green }
        };

        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillRight = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Right_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Right_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Right_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pill_Right_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pill_Right_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pill_Right_Green }

        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillBottom = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Bottom_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Bottom_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Bottom_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pill_Bottom_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pill_Bottom_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pill_Bottom_Green },
        };
        Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes> PillSingle = new Dictionary<LineSeriesBlock.CombiningTypes, BlockTypes>()
        {
            {LineSeriesBlock.CombiningTypes.Red,BlockTypes.Pill_Single_Red},
            {LineSeriesBlock.CombiningTypes.Yellow,BlockTypes.Pill_Single_Yellow },
            {LineSeriesBlock.CombiningTypes.Blue,BlockTypes.Pill_Single_Blue },
            {LineSeriesBlock.CombiningTypes.Orange,BlockTypes.Pill_Single_Orange },
            {LineSeriesBlock.CombiningTypes.Magenta,BlockTypes.Pill_Single_Magenta },
            {LineSeriesBlock.CombiningTypes.Green,BlockTypes.Pill_Single_Green },
        };
        private BlockTypes[] YellowRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Yellow, BlockTypes.Pill_Top_Yellow, BlockTypes.Pill_Right_Yellow, BlockTypes.Pill_Bottom_Yellow };
        private BlockTypes[] RedRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Red, BlockTypes.Pill_Top_Red, BlockTypes.Pill_Right_Red, BlockTypes.Pill_Bottom_Red };
        private BlockTypes[] BlueRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Blue, BlockTypes.Pill_Top_Blue, BlockTypes.Pill_Right_Blue, BlockTypes.Pill_Bottom_Blue };
        private BlockTypes[] GreenRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Green, BlockTypes.Pill_Top_Green, BlockTypes.Pill_Right_Green, BlockTypes.Pill_Bottom_Green };
        private BlockTypes[] OrangeRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Orange, BlockTypes.Pill_Top_Orange, BlockTypes.Pill_Right_Orange, BlockTypes.Pill_Bottom_Orange };
        private BlockTypes[] MagentaRotationTypes = new BlockTypes[] { BlockTypes.Pill_Left_Magenta, BlockTypes.Pill_Top_Magenta, BlockTypes.Pill_Right_Magenta, BlockTypes.Pill_Bottom_Magenta };

        protected IEnumerable<T> GetArrayRun<T>(T[] src,int StartIndex,int Count)
        {
            int CurrIndex = StartIndex;
            for(int i = 0;i<Count;i++)
            {
                yield return src[(StartIndex + i) % src.Length];
            }
            
        }
        protected BlockTypes[] GetRotationBlockTypes(BlockTypes Original)
        {

            switch (Original)
            {
                case BlockTypes.Pill_Left_Yellow:
                    return GetArrayRun(YellowRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Yellow:
                    return GetArrayRun(YellowRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Yellow:
                    return GetArrayRun(YellowRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Yellow:
                    return GetArrayRun(YellowRotationTypes, 3, 4).ToArray();
                case BlockTypes.Pill_Left_Red:
                    return GetArrayRun(RedRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Red:
                    return GetArrayRun(RedRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Red:
                    return GetArrayRun(RedRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Red:
                    return GetArrayRun(RedRotationTypes, 3, 4).ToArray();
                case BlockTypes.Pill_Left_Blue:
                    return GetArrayRun(BlueRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Blue:
                    return GetArrayRun(BlueRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Blue:
                    return GetArrayRun(BlueRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Blue:
                    return GetArrayRun(BlueRotationTypes, 3, 4).ToArray();
                case BlockTypes.Pill_Left_Green:
                    return GetArrayRun(GreenRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Green:
                    return GetArrayRun(GreenRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Green:
                    return GetArrayRun(GreenRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Green:
                    return GetArrayRun(GreenRotationTypes, 3, 4).ToArray();
                case BlockTypes.Pill_Left_Magenta:
                    return GetArrayRun(MagentaRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Magenta:
                    return GetArrayRun(MagentaRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Magenta:
                    return GetArrayRun(MagentaRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Magenta:
                    return GetArrayRun(MagentaRotationTypes, 3, 4).ToArray();
                case BlockTypes.Pill_Left_Orange:
                    return GetArrayRun(OrangeRotationTypes, 0, 4).ToArray();
                case BlockTypes.Pill_Top_Orange:
                    return GetArrayRun(OrangeRotationTypes, 1, 4).ToArray();
                case BlockTypes.Pill_Right_Orange:
                    return GetArrayRun(OrangeRotationTypes, 2, 4).ToArray();
                case BlockTypes.Pill_Bottom_Orange:
                    return GetArrayRun(OrangeRotationTypes, 3, 4).ToArray();

                default:
                    return new BlockTypes[] { Original };
            }



        }
        protected override SKImage[] ApplyFunc_Custom(TetrisField field, Nomino Group, NominoBlock Target,BlockTypes chosentype)
        {
            //I think this messes up when applying to the field because it changes the blocktype being used based on the rotation, which gives a different sequence of blocks.



            var getElement = Group.FindEntry(Target);
            if(Target is LineSeriesBlock lsb)
            {
                if(lsb.Popping)
                {
                    BlockTypes[] useType  = new[] { BlockTypes.Yellow_Pop };
                    if (chosentype == BlockTypes.Blue_Virus_1 || chosentype == BlockTypes.Blue_Virus_2 || chosentype == BlockTypes.Blue_Pop)
                        useType = new[] { chosentype = BlockTypes.Blue_Pop };
                    else if (chosentype == BlockTypes.Yellow_Virus_1 || chosentype == BlockTypes.Yellow_Virus_2 || chosentype == BlockTypes.Yellow_Pop)
                        useType = new[] { chosentype = BlockTypes.Yellow_Pop };
                    else if (chosentype == BlockTypes.Red_Virus_1 || chosentype == BlockTypes.Red_Virus_2 || chosentype == BlockTypes.Red_Pop)
                        useType = new[] { chosentype = BlockTypes.Red_Pop };
                    else if (new[] { BlockTypes.Green_Virus_1,BlockTypes.Green_Virus_2,BlockTypes.Green_Pop}.Contains(chosentype))
                    {
                        useType = new[] { chosentype = BlockTypes.Green_Pop };
                    }
                    else if (new[] { BlockTypes.Magenta_Virus_1, BlockTypes.Magenta_Virus_2, BlockTypes.Magenta_Pop }.Contains(chosentype))
                    {
                        useType = new[] { chosentype = BlockTypes.Magenta_Pop };
                    }
                    else if (new[] { BlockTypes.Orange_Virus_1, BlockTypes.Orange_Virus_2, BlockTypes.Orange_Pop }.Contains(chosentype))
                    {
                        useType = new[] { chosentype = BlockTypes.Orange_Pop };
                    }
                    else
                    {
                        
                    }
                    
                    
                    lsb.SpecialImageFunctionSK = null;
                    //TODO: the rotationimages should have the proper rotations. we only really use left and right, so if it's a left or right piece we want to create the appropriate rotation images as needed, using the other
                    //directions in that same colour.

                    lsb._RotationImagesSK = (from s in useType select SKImage.FromBitmap(GetMappedImageSkia(field, Group, getElement, s))).ToArray();

                    //lsb._RotationImagesSK = new SKImage[] { SKImage.FromBitmap(GetMappedImageSkia(field, Group, getElement, useType)) };
                    return lsb._RotationImagesSK;
                }
                else if(!(lsb is LineSeriesPrimaryBlock))
                {
                    BlockTypes[] useType = GetRotationBlockTypes(chosentype);
                    lsb._RotationImagesSK = (from s in useType select SKImage.FromBitmap(GetMappedImageSkia(field, Group, getElement, s))).ToArray();
                    return lsb._RotationImagesSK;
                }
            }
            
            BlockTypes usetype1= BlockTypes.Yellow_Virus_1, usetype2 = BlockTypes.Yellow_Virus_2;
            if(chosentype == BlockTypes.Blue_Virus_1 || chosentype==BlockTypes.Blue_Virus_2)
            {
                usetype1 = BlockTypes.Blue_Virus_1;
                usetype2 = BlockTypes.Blue_Virus_2;
            }
            else if (chosentype == BlockTypes.Red_Virus_1 || chosentype == BlockTypes.Red_Virus_2)
            {
                usetype1 = BlockTypes.Red_Virus_1;
                usetype2 = BlockTypes.Red_Virus_2;
            }
            else if(chosentype==BlockTypes.Yellow_Virus_1 || chosentype == BlockTypes.Yellow_Virus_2)
            {
                usetype1 = BlockTypes.Yellow_Virus_1;
                usetype2 = BlockTypes.Yellow_Virus_2;
            }
            else if (chosentype == BlockTypes.Green_Virus_1 || chosentype == BlockTypes.Green_Virus_2)
            {
                usetype1 = BlockTypes.Green_Virus_1;
                usetype2 = BlockTypes.Green_Virus_2;
            }
            else if(chosentype==BlockTypes.Orange_Virus_1 || chosentype == BlockTypes.Orange_Virus_2)
            {
                usetype1 = BlockTypes.Orange_Virus_1;
                usetype2 = BlockTypes.Orange_Virus_2;
            }
            else if (chosentype == BlockTypes.Magenta_Virus_1 || chosentype == BlockTypes.Magenta_Virus_2)
            {
                usetype1 = BlockTypes.Magenta_Virus_1;
                usetype2 = BlockTypes.Magenta_Virus_2;
            }
            else
            {
                usetype1 = usetype2 = chosentype;
            }
            if(usetype1==BlockTypes.Yellow_Virus_1 && !(chosentype==BlockTypes.Yellow_Virus_1 || chosentype==BlockTypes.Yellow_Virus_2))
            {
                ;
            }
            var firstImage = SKImage.FromBitmap(GetMappedImageSkia(field, Group,getElement, usetype1));
            var secondImage = SKImage.FromBitmap(GetMappedImageSkia(field, Group,getElement, usetype2));
            foreach(var iterate in Group)
            {
                if (iterate.Block is ImageBlock ib)
                {
                    ib.SpecialImageFunctionSK = (a) =>
                    {
                        int useindex = ((DateTime.Now.Millisecond / 250) + 2) % 2 == 0 ? 0 : 1;
                        return (a as ImageBlock)._RotationImagesSK[useindex];
                    };
                }
            }

            return new SKImage[] { firstImage, secondImage };
            
        }
        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            if (element.Block is LineSeriesBlock lsb)
            {
                if(lsb.Popping)
                {
                    if(element.Block is LineSeriesPrimaryBlock lsm2)
                    {
                        ;
                    }
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                }
                if (element.Block is LineSeriesPrimaryBlock lsm)
                {
                    if (lsb.Popping)
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PopTypes_1[lsb.CombiningIndex]);
                    }
                    if (DateTime.Now.Millisecond <500)
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(VirusTypes_1[lsm.CombiningIndex],true);
                    else
                    {
                        return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(VirusTypes_2[lsm.CombiningIndex], true);
                    }

                }

                else if (group == null || group.Count()==1)
                {
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillSingle[lsb.CombiningIndex]);
                    
                }
                else if (group.Count() == 2)
                {
                    int index = group.IndexOf(element);

                    var PillSource = new[] { new[] { PillLeft, PillTop, PillRight, PillBottom }, new[] { PillRight, PillBottom, PillLeft, PillTop } };
                    var rotmod = 0; //NominoElement.sMod(group.GetBlockData()[0].RotationModulo,4); //ideally we'd use this instead of 0 in the index below, but we need to refactor t his theme I think...
                    
                    return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillSource[index][rotmod][lsb.CombiningIndex]);
                    //possibly, should rotmod always be zero here?

                    //if (index == 0) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillLeft[lsb.CombiningIndex]);
                    //else if (index == 1) return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(PillRight[lsb.CombiningIndex]);
                }
            }
            return new CustomPixelTheme<BCT, BlockTypes>.BlockTypeReturnData(BlockTypes.Yellow_Virus_1);
        }

        public override Dictionary<BlockTypes, BCT[][]> GetBlockTypeDictionary()
        {
            return BitmapIndex;
        }
        private static BlockTypes[] YellowVirusTypes = new BlockTypes[] { BlockTypes.Yellow_Virus_1, BlockTypes.Yellow_Virus_2 };
        private static BlockTypes[] RedVirusTypes = new BlockTypes[] { BlockTypes.Red_Virus_1, BlockTypes.Red_Virus_2 };
        private static BlockTypes[] BlueVirusTypes = new BlockTypes[] { BlockTypes.Blue_Virus_1, BlockTypes.Blue_Virus_2 };
        private static BlockTypes[] MagentaVirusTypes = new BlockTypes[] { BlockTypes.Magenta_Virus_1, BlockTypes.Magenta_Virus_2 };
        private static BlockTypes[] OrangeVirusTypes = new BlockTypes[] { BlockTypes.Orange_Virus_1, BlockTypes.Orange_Virus_2 };
        private static BlockTypes[] GreenVirusTypes = new BlockTypes[] { BlockTypes.Green_Virus_1, BlockTypes.Green_Virus_2 };
        private static BlockTypes[] YellowPillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Yellow, BlockTypes.Pill_Right_Yellow, BlockTypes.Pill_Single_Yellow, BlockTypes.Yellow_Pop, BlockTypes.Pill_Top_Yellow, BlockTypes.Pill_Bottom_Yellow };
        private static BlockTypes[] RedPillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Red, BlockTypes.Pill_Right_Red, BlockTypes.Pill_Single_Red, BlockTypes.Red_Pop, BlockTypes.Pill_Top_Red, BlockTypes.Pill_Bottom_Red };
        private static BlockTypes[] BluePillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Blue, BlockTypes.Pill_Right_Blue, BlockTypes.Pill_Single_Blue, BlockTypes.Blue_Pop, BlockTypes.Pill_Top_Blue, BlockTypes.Pill_Bottom_Blue };
        private static BlockTypes[] MagentaPillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Magenta, BlockTypes.Pill_Right_Magenta, BlockTypes.Pill_Single_Magenta, BlockTypes.Magenta_Pop, BlockTypes.Pill_Top_Magenta, BlockTypes.Pill_Bottom_Magenta };
        private static BlockTypes[] OrangePillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Orange, BlockTypes.Pill_Right_Orange, BlockTypes.Pill_Single_Orange, BlockTypes.Orange_Pop, BlockTypes.Pill_Top_Orange, BlockTypes.Pill_Bottom_Orange };
        private static BlockTypes[] GreenPillTypes = new BlockTypes[] { BlockTypes.Pill_Left_Green, BlockTypes.Pill_Right_Green, BlockTypes.Pill_Single_Green, BlockTypes.Green_Pop, BlockTypes.Pill_Top_Green, BlockTypes.Pill_Bottom_Green };

        private static bool IsInTypes(BlockTypes value,BlockTypes[] Virii,BlockTypes[] Pills)
        {
            return Virii.Contains(value) || Pills.Contains(value);
        }
        public static bool IsYellowType(BlockTypes value)
        {
            return IsInTypes(value, YellowVirusTypes, YellowPillTypes);
        }
        public static bool IsRedType(BlockTypes value)
        {
            return IsInTypes(value, RedVirusTypes, RedPillTypes);
        }
        public static bool IsBlueType(BlockTypes value)
        {
            return IsInTypes(value, BlueVirusTypes, BluePillTypes);
        }
        public static bool IsOrangeType(BlockTypes value)
        {
            return IsInTypes(value, OrangeVirusTypes, OrangePillTypes);
        }
        public static bool IsMagentaType(BlockTypes value)
        {
            return IsInTypes(value, MagentaVirusTypes, MagentaPillTypes);
        }
        public static bool IsGreenType(BlockTypes value)
        {
            return IsInTypes(value, GreenVirusTypes, GreenPillTypes);
        }
        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, BlockTypes BlockType, BCT PixelType)
        {
            if (new BlockTypes[] { BlockTypes.Pill_Single_Red, BlockTypes.Pill_Single_Yellow, BlockTypes.Pill_Single_Blue }.Contains(BlockType))
                {
                ;
            }
            
            if(BlockType==BlockTypes.Magenta_Virus_1 || BlockType==BlockTypes.Magenta_Virus_2)
            {
                ;
            }

            if (IsYellowType(BlockType))
                return UseEnhancedImages ? EnhancedYellowColourSet[PixelType] : YellowColourSet[PixelType];
            else if (IsRedType(BlockType))
                return UseEnhancedImages ? EnhancedRedColourSet[PixelType] : RedColourSet[PixelType];
            else if (IsBlueType(BlockType))
                return UseEnhancedImages?EnhancedBlueColourSet[PixelType]:BlueColourSet[PixelType];
            else if (IsOrangeType(BlockType))
                return UseEnhancedImages ? EnhancedOrangeColourSet[PixelType] : OrangeColourSet[PixelType];
            else if(IsGreenType(BlockType))
                return UseEnhancedImages ? EnhancedGreenColourSet[PixelType] : GreenColourSet[PixelType];
            else if (IsMagentaType(BlockType))
                return UseEnhancedImages ? EnhancedMagentaColourSet[PixelType] : MagentaColourSet[PixelType];
            return SKColors.Magenta;
        }
        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            if(testvalue.Block is LineSeriesPrimaryBlock)
            {
                return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.CustomSelector;
            }
            else
            {
                return AllowAdvancedRotations ? CustomPixelTheme<BCT, BlockTypes>.BlockFlags.CustomSelector : CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
                //return CustomPixelTheme<BCT, BlockTypes>.BlockFlags.Rotatable;
            }
            
        }
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IGameCustomizationHandler GameHandler)
        {
            return new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_3", 0.5f], Color.Transparent);
        }
        public override bool IsAnimated(NominoBlock block)
        {
            return (block is LineSeriesPrimaryBlock);
        }
        public override BlockTypes[] PossibleBlockTypes()
        {
            return (BlockTypes[])Enum.GetValues(typeof(BlockTypes));
            /*return new BlockTypes[]
            {
            BlockTypes.Pill_Left_Yellow,
            BlockTypes.Pill_Right_Yellow,
            BlockTypes.Pill_Single_Yellow,
            BlockTypes.Pill_Left_Red,
            BlockTypes.Pill_Right_Red,
            BlockTypes.Pill_Single_Red,
            BlockTypes.Pill_Left_Blue,
            BlockTypes.Pill_Right_Blue,
            BlockTypes.Pill_Single_Blue,
            BlockTypes.Red_Virus_1,
            BlockTypes.Red_Virus_2,
            BlockTypes.Yellow_Virus_1,
            BlockTypes.Yellow_Virus_2,
            BlockTypes.Blue_Virus_1,
            BlockTypes.Blue_Virus_2,
            BlockTypes.Red_Pop,
            BlockTypes.Yellow_Pop,
            BlockTypes.Blue_Pop,
            BlockTypes.Green_Virus_1,
            BlockTypes.Green_Virus_2
            };*/
        }
        //The "A" BCT Types are used for the hi-color "sprites".
        public enum BCT
        {
            Primary,
            Accent,
            Accent2,
            Transparent,
            Black,
            Enhanced_1,
            Enhanced_2,
            Enhanced_3,
            Enhanced_4,
            Enhanced_5,
            Enhanced_6,
            Enhanced_7,
            Enhanced_8,
            Enhanced_9,
            Enhanced_10,
            Enhanced_11
        }

        //enhanced red colors:
        /*
        205,0,0
        157,0,0
        116,0,0
        249,3,3
        67,1,1
        255,18,18
        255,45,45
        255,75,75
        255,103,103
        88,88,88
        1,1,1
             
             */


        public enum BlockTypes
        {
            Pill_Left_Yellow,
            Pill_Top_Yellow,
            Pill_Right_Yellow,
            Pill_Bottom_Yellow,
            Pill_Single_Yellow,
            Pill_Left_Red,
            Pill_Top_Red,
            Pill_Right_Red,
            Pill_Bottom_Red,
            Pill_Single_Red,
            Pill_Left_Blue,
            Pill_Top_Blue,
            Pill_Right_Blue,
            Pill_Bottom_Blue,
            Pill_Single_Blue,
            Pill_Left_Green,
            Pill_Top_Green,
            Pill_Right_Green,
            Pill_Bottom_Green,
            Pill_Single_Green,
            Pill_Left_Magenta,
            Pill_Top_Magenta,
            Pill_Right_Magenta,
            Pill_Bottom_Magenta,
            Pill_Single_Magenta,
            Pill_Left_Orange,
            Pill_Top_Orange,
            Pill_Right_Orange,
            Pill_Bottom_Orange,
            Pill_Single_Orange,
            Red_Virus_1,
            Red_Virus_2,
            Yellow_Virus_1,
            Yellow_Virus_2,
            Blue_Virus_1,
            Blue_Virus_2,
            Green_Virus_1,
            Green_Virus_2,
            Magenta_Virus_1,
            Magenta_Virus_2,
            Orange_Virus_1,
            Orange_Virus_2,
            Red_Pop,
            Yellow_Pop,
            Blue_Pop,
            Magenta_Pop,
            Orange_Pop,
            Green_Pop
            
        }

    }
}
