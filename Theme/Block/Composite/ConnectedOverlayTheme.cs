using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme.Block.Composite
{

    //simple theme, intended for composition purposes.

    public class ConnectedOverlayTheme : ConnectedImageBlockTheme
    {
        protected String _OverlayBase = null;
        protected String _UnderlayKey = null;
        public ConnectedOverlayTheme(String sOverlayBase,String sUnderlayKey)
        {
            _OverlayBase = sOverlayBase;
            _UnderlayKey = sUnderlayKey ;
            base.Underlay = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap(_UnderlayKey));
        }


        protected override string GetImageKeyBase()
        {
            return _OverlayBase;  //"chisel_overlay";
        }
    }

    [HandlerTheme("Chiseled Marble", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Chiseled Marble Theme")]
    

    public class ConnectedOverlayTileTheme : ConnectedOverlayTheme
    {
        public override string Name => "Chiseled Marble";
        public ConnectedOverlayTileTheme() : base("chisel_overlay", "tile_mottle")
        {
        }
    }

}
