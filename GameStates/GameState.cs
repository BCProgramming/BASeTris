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
using bcHighScores;
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
            GameKey_Hold
        }
        public abstract void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void GameProc(IStateOwner pOwner);
        public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void HandleGameKey(IStateOwner pOwner, GameKeys g);

        public abstract void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds);
    }




    }

