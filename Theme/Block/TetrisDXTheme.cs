using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Tetrominoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme.Block
{

    //Tetris DX is a composite theme built out of a set of individual themes for each block type.
    //true purity would suggest that we need to have this arrangement for the Game Boy theme aswell.

    [HandlerTheme("Tetris DX (Mottled)", typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("The I-Piece style of Tetris DX- on all pieces")]
    public class TetrisDX_Mottled : ConnectedImageBlockTheme
    {
        public override string Name => "Mottled (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_mottle";
        }
    }


    public class TetrisDX_Hole : ConnectedImageBlockTheme
    {
        public override bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {
            return false;
        }
        public override string Name => "Hole (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_hole";
        }
    }

    public class TetrisDX_Block : ConnectedImageBlockTheme
    {
        public override bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {
            return false;
        }
        public override string Name => "Block (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_block";
        }
    }

    public class TetrisDX_Raised : ConnectedImageBlockTheme
    {
        public override bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {
            return false;
        }
        public override string Name => "Raised (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_raised";
        }
    }
    public class TetrisDX_Dot : ConnectedImageBlockTheme
    {
        public override bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {
            return false;
        }
        public override string Name => "Dot (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_dot";
        }
    }
    [HandlerTheme("Tetris DX Bigdot",HandlerThemeAttribute.HandlerThemeFlags.ThemeFlags_NonBrowsable ,typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    public class TetrisDX_BigDot : ConnectedImageBlockTheme
    {
        public override bool IsConnected(NominoBlock BlockA, NominoBlock BlockB)
        {
            return false;
        }
        public override string Name => "BigDot (Tetris DX)";
        protected override string GetImageKeyBase()
        {
            return "dx_bigdot";
        }
    }
    [HandlerTheme("Tetris DX",  typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Tetris DX on the Game Boy")]
    public class TetrisDXTheme : CompositeBlockTheme
    {

        public override string Name => "Tetrix DX (GB)";

        private TetrisDX_BigDot _BigDot = new TetrisDX_BigDot();
        private TetrisDX_Mottled _Mottled = new TetrisDX_Mottled();
        private TetrisDX_Dot _Dot = new TetrisDX_Dot();
        private TetrisDX_Raised _Raised = new TetrisDX_Raised();
        private TetrisDX_Block _Block = new TetrisDX_Block();
        private TetrisDX_Hole _Hole = new TetrisDX_Hole();

        private NominoTheme[] _AllThemes;
        public TetrisDXTheme()
        {
            _AllThemes = new NominoTheme[] {
                _BigDot,_Mottled,_Dot,_Raised,_Block,_Hole
                };
        }
        public override NominoTheme[] GetAllThemes()
        {
            return _AllThemes;
        }

        public override NominoTheme GetGroupTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return Group switch
            {
                Tetromino_I => _Mottled,
                Tetromino_O _ => _BigDot,
                Tetromino_J _ => _Hole,
                Tetromino_L _ => _Block,
                Tetromino_T _ => _Raised,
                Tetromino_Z => _Dot,
                Tetromino_S => _BigDot,
                _ => _Block
            };
        }
        private static Image LightImage = null;
        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(() =>
            {
                if (LightImage == null)
                {
                    LightImage = new Bitmap(250, 500);
                    using (Graphics drawdark = Graphics.FromImage(LightImage))
                    {
                        drawdark.Clear(Color.LightGreen);
                    }
                }
                return new PlayFieldBackgroundInfo(LightImage, Color.Yellow);
            });
            
        }
    }

    //the "old" tetris DX theme. It's actually identical to the above, but isn't composited.

    public class TetrisDXTheme_Depr : ConnectedImageBlockTheme
    {
        public override string Name => "Tetris DX";
        protected override string GetImageKeyBase()
        {
            return "dx";
        }
        protected override string GetImageKeyContext(NominoBlock nb)
        {
            return nb.Owner switch
            {
                Tetromino_I _ => "mottle",
                Tetromino_O _ => "bigdot",
                Tetromino_J _ => "dx_hole",
                Tetromino_L _ => "block",
                Tetromino_T _ => "raised",
                Tetromino_Z => "dot",
                Tetromino_S => "bigdot",
                _ => "block" //todo: select a type unique to the nomino, similar to other themes.
            };
        }
        protected override string[] GetAllImageKeyContexts()
        {
            return new[] { "mottle", "bigdot", "block", "dot", "hole", "raised" };
        }
        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            base.ApplyTheme(Group, GameHandler, Field, Reason);
        }
        protected override GenericCachedData.BlockTypeConstants GetGroupBlockType(IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason,Nomino Group) => Group switch
        {
            _ => GenericCachedData.BlockTypeConstants.Normal
        };
    }
}
