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
        public override string Name => "Tetris 3 SNES";
        protected override string GetImageKeyBase()
        {
            return "Tetris_3";
        }
        Bitmap DarkImage;
        
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
        public override string Name => "Tetris 2 SNES";
        protected override string GetImageKeyBase()
        {
            return "tetris_2";
        }
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            base.ApplyTheme(Group, GameHandler, Field, Reason);
        }
        protected override GenericCachedData.BlockTypeConstants GetGroupBlockType(Nomino Group) =>Group switch 
            {
                Tetromino_O => GenericCachedData.BlockTypeConstants.Fixed,
                Tetromino_T => GenericCachedData.BlockTypeConstants.Fixed,
                _ =>GenericCachedData.BlockTypeConstants.Normal
            };
        
    }



}
