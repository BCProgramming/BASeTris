using BASeTris.AssetManager;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BASeTris.Theme.Block
{



    [HandlerTheme("Tetris 3 SNES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris 3 Theme from the SNES")]

    public class SNESTetris3Theme : ConnectedImageBlockTheme
    {
        private static SKColor Outline = new SKColor(241, 187, 187); //color of the typically bright outline.
        private static SKColor Shadow = new SKColor(47, 10, 10);
        private static SKColor Shadow_2 = new SKColor(116, 24, 24);
        private static SKColor Shadow_3 = new SKColor(163, 34, 34);
        private static SKColor Fill = new SKColor(213, 49, 49);
        private static SKColor[] StandardColors = new SKColor[] { Outline, Shadow, Shadow_3, Shadow_3, Fill };



        private static Dictionary<SKColor, SKColor>[] ColorShifts = null; /* new Dictionary<SKColor, SKColor>[]{
            GetColorShiftDictionary(StandardColors,0),
            GetColorShiftDictionary(StandardColors,1),
            GetColorShiftDictionary(StandardColors,2),
            GetColorShiftDictionary(StandardColors,3),
            GetColorShiftDictionary(StandardColors,4)
        };*/




                private static BlockColorInformation ShiftColorReplacement= new BlockColorInformation(new Dictionary<SKColor, SKColor>()
                    {{Outline,Shadow },
                    {Shadow,Fill },
                    {Shadow_2,Shadow_3 },
                    {Shadow_3,Shadow_2 },
                    {Fill,Shadow }
                });

      
        public override string Name => "Tetris 3 SNES";
        protected override string GetImageKeyBase()
        {
            return "Tetris_3";
        }
        Bitmap DarkImage;

        protected override SKColor[] GetStandardColors()
        {
            return StandardColors;
        }

        protected override BlockColorInformation GetGroupBlockColor(IBlockGameCustomizationHandler GameHandler,TetrisField Field,ThemeApplicationReason Reason,  Nomino Group)
        {


            // return base.GetGroupBlockColor(GameHandler, Field, Reason, Group);

            //base is standard Mino colors for tetris.
            if (ColorShifts == null) ColorShifts = GetAllColorShiftDictionaries(StandardColors);

           
                var getresult = base.GetGroupBlockColor(GameHandler,Field,Reason,Group);
            var usereplacement = ColorShifts[Field.Level % ColorShifts.Length];
                return new BlockColorInformation(getresult.SolidColor.Value) { ColorMapping = usereplacement};

            if (false || Group is Tetromino_T)
            {
                var result = base.GetGroupBlockColor(GameHandler,Field,Reason,Group);
                ShiftColorReplacement.SolidColor = result.SolidColor;
                return new BlockColorInformation(result.SolidColor.Value) { ColorMapping = ShiftColorReplacement.ColorMapping };

            }
            else
            {
                return base.GetGroupBlockColor(GameHandler,Field,Reason,Group);
            }
           
        }
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(()=>new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_5", 0.5f], Color.Transparent));
        }
    }
    [HandlerTheme("Tengen Tetris", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris Theme from Tengen Tetris on the NES")]
    public class NESTengenTetrisTheme : ConnectedImageBlockTheme
    {
        public override string Name => "NES Tengen Tetris";
        protected override string GetImageKeyBase()
        {
            return "Tengen_nes";
        }
        Bitmap DarkImage;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(()=> new PlayFieldBackgroundInfo(TetrisGame.Imageman["background_5", 0.5f], Color.Transparent));
            
        }
    }


    [HandlerTheme("Tetris 2 NES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris 2 Theme from the NES")]

    public class NESTetris2Theme : ConnectedImageBlockTheme
    {
        public override string Name => "Tetris 2 NES";
        protected override string GetImageKeyBase()
        {
            return "tetris_2_NES";
        }

        Bitmap DarkImage;
        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {

            return HandleBGCache(() =>
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
            });
        

        }
    }


    [HandlerTheme("Tetris 2 SNES", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris 2 Theme from the SNES")]

    public class SNESTetris2Theme : ConnectedImageBlockTheme
    {

        /*
         Darkest 78,13,13
        106,17,17
        170,27,27
        223,48,48
        227,77,77
        223,112,112
        237,141,141
        241,162,162
        246,198,198

         
         */
        static SKColor[] BaseColors = new SKColor[]{
            new SKColor(78,13,13),
            new SKColor(107,17,17),
            new SKColor(223,48,48),
            new SKColor(227,77,77),
            new SKColor(223,112,112),
            new SKColor(237,141,141),
            new SKColor(241,162,162),
            new SKColor(246,198,198)
        };

        public override string Name => "Tetris 2 SNES";
        protected override string GetImageKeyBase()
        {
            return "tetris_2";
        }
        private static Dictionary<SKColor, SKColor> FlipWave = new Dictionary<SKColor, SKColor>()
            {
            { BaseColors[6],BaseColors[0] },
            { BaseColors[5],BaseColors[0]},
            { BaseColors[4],BaseColors[0]},
            { BaseColors[3],BaseColors[1]},
            { BaseColors[2],BaseColors[2]},
            { BaseColors[1],BaseColors[3]}
            };


        private static Dictionary<SKColor, SKColor> RetroWave = new Dictionary<SKColor, SKColor>()
            {
            { BaseColors[6],BaseColors[0] },
            { BaseColors[5],BaseColors[0]},
            { BaseColors[4],BaseColors[0]},
            { BaseColors[3],BaseColors[0]},
            { BaseColors[2],BaseColors[0]},
            { BaseColors[1],BaseColors[0]},
            { BaseColors[0],BaseColors[7]}
            };


        private static Dictionary<SKColor, SKColor>[] ColorShifts = null;

        Type[] MinoTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_T), typeof(Tetromino_S), typeof(Tetromino_Z), typeof(Tetromino_J), typeof(Tetromino_L) };

        public SKColor[][] ColorSets = new SKColor[][]{
            new SKColor[]{SKColors.Cyan,SKColors.Yellow,SKColors.Purple,SKColors.Green,SKColors.Red,SKColors.Navy,SKColors.OrangeRed },
           /*new SKColor[]{SKColors.LightBlue,SKColors.DarkBlue,SKColors.Navy,SKColors.Cyan,SKColors.LightCyan,SKColors.DarkSlateBlue,SKColors.SkyBlue },
            new SKColor[]{SKColors.LightGreen,SKColors.DarkGreen,SKColors.DarkOliveGreen,SKColors.LimeGreen,SKColors.DarkSeaGreen ,SKColors.GreenYellow,SKColors.LawnGreen },
            new SKColor[]{SKColors.Magenta,SKColors.Pink,SKColors.LightPink,SKColors.Coral,SKColors.Lavender ,SKColors.MediumPurple,SKColors.Violet },
            new SKColor[]{SKColors.Blue,SKColors.LightGreen,SKColors.DarkBlue,SKColors.DarkGreen,SKColors.LightSkyBlue,SKColors.Lime,SKColors.MediumBlue}*/
        };



        /*
                     if (Group is Tetromino_I) useColor = SKColors.Cyan;
            else if (Group is Tetromino_O) useColor = SKColors.Yellow;
            else if (Group is Tetromino_T) useColor = SKColors.Purple;
            else if (Group is Tetromino_S) useColor = SKColors.Green;
            else if (Group is Tetromino_Z) useColor = SKColors.Red;
            else if (Group is Tetromino_J) useColor = SKColors.Navy;
            else if (Group is Tetromino_L) useColor = SKColors.OrangeRed;
         
         */


        protected override BlockColorInformation GetGroupBlockColor(IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason, Nomino Group)
        {

            int FoundIndex = -1;
            for (int i = 0; i < MinoTypes.Length; i++)
            {
                if (Group.GetType().IsAssignableFrom(MinoTypes[i]))
                {
                    FoundIndex = i;
                    break;
                }
            }
            if (FoundIndex > -1)
            {
                return new BlockColorInformation(ColorSets[Field.Level % ColorSets.Length][FoundIndex]);
            }


            //if (ColorShifts == null) ColorShifts = GetAllColorShiftDictionaries(BaseColors);

           /*
                var getresult = base.GetGroupBlockColor(GameHandler,Field,Reason,Group);
            var usereplacement = ColorShifts[Field.Level % ColorShifts.Length];
                return new BlockColorInformation(getresult.SolidColor.Value) { ColorMapping = usereplacement};
           */

            return base.GetGroupBlockColor(GameHandler, Field, Reason, Group);
        }
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            base.ApplyTheme(Group, GameHandler, Field, Reason);
        }
        protected override GenericCachedData.BlockTypeConstants GetGroupBlockType(IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason,Nomino Group) =>Group switch 
            {
                Tetromino_O => GenericCachedData.BlockTypeConstants.Normal,
                Tetromino_T => GenericCachedData.BlockTypeConstants.Normal,
                _ =>GenericCachedData.BlockTypeConstants.Normal
            };
        
    }



}
