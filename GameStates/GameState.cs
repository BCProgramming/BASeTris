using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public abstract class GameState
    {
        public virtual bool GameProcSuspended { get; set; } = false;

        public enum DisplayMode
        {
            Partitioned,
            Full
        }
        public enum GameKeys
        {
            GameKey_Null,
            GameKey_RotateCW,
            GameKey_RotateCCW,
            GameKey_Drop,
            GameKey_Left,
            GameKey_Right,
            GameKey_Down,
            GameKey_Pause,
            GameKey_Hold,
            GameKey_Debug1,
            GameKey_Debug2,
            GameKey_Debug3,
            GameKey_Debug4,
            GameKey_Debug5,
            GameKey_Debug6
        }
        public virtual DisplayMode SupportedDisplayMode { get; }
        public abstract void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void GameProc(IStateOwner pOwner);
        public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void HandleGameKey(IStateOwner pOwner, GameKeys g);

        public abstract void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds);
    }




    }

