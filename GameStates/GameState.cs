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
using BASeTris.BackgroundDrawers;
using BASeTris.Choosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.Blocks;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public abstract class GameState
    {
        public virtual bool GameProcSuspended { get; set; } = false;
        /// <summary>
        /// Indicates whether this GameState allows active play. This is used to determine certain actions such as movement interpolation.
        /// </summary>
        public virtual bool GamePlayActive { get{ return false; } }
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
            GameKey_MenuActivate,
            GameKey_Debug1,
            GameKey_Debug2,
            GameKey_Debug3,
            GameKey_Debug4,
            GameKey_Debug5,
            GameKey_Debug6,
            GameKey_PopHold,
            GameKey_DesignerNextNomino,
            GameKey_DesignerPrevNomino,
            GameKey_DesignerChangeNomino,
            Gamekey_DesignerDeleteNomino
        }
        private static Dictionary<GameKeys, String> FriendlyNames = new Dictionary<GameKeys, string>()
        {
            { GameKeys.GameKey_Null,"NULL"},
{GameKeys.GameKey_RotateCW,"Rotate Clockwise"},
{GameKeys.GameKey_RotateCCW,"Rotate Counter-clockwise"},
{ GameKeys.GameKey_Drop,"Drop"},
{ GameKeys.GameKey_Left,"Move Left"},
{ GameKeys.GameKey_Right,"Move Right"},
{ GameKeys.GameKey_Down,"Move Down"},
{ GameKeys.GameKey_Pause,"Pause Game"},
{ GameKeys.GameKey_Hold,"Hold Piece"},
{ GameKeys.GameKey_MenuActivate,"Menu Activation(?)"},
{ GameKeys.GameKey_Debug1,"DEBUG1"},
{ GameKeys.GameKey_Debug2,"DEBUG1"},
{ GameKeys.GameKey_Debug3,"DEBUG1"},
{ GameKeys.GameKey_Debug4,"DEBUG1"},
{ GameKeys.GameKey_Debug5,"DEBUG1"},
{ GameKeys.GameKey_Debug6,"DEBUG1"},
{ GameKeys.GameKey_PopHold,"Pop Hold Stack"},
            { GameKeys.GameKey_DesignerChangeNomino,"Change Nomino (BG Design)"},
            { GameKeys.GameKey_DesignerNextNomino,"Next Nomino (BG Design)"},
            { GameKeys.GameKey_DesignerPrevNomino,"Previous Nomino (BG Design)"},
            { GameKeys.Gamekey_DesignerDeleteNomino,"Delete Nomino (BG Design)"}
        };
        public static String GetGameKeyFriendlyName(GameKeys src)
        {
            return FriendlyNames[src];
        }
        public virtual DisplayMode SupportedDisplayMode { get; }
        /*[Obsolete("Use Rendering Providers")]
        public abstract void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds);*/
        public abstract void GameProc(IStateOwner pOwner);
        
        //Obbsolete("Use Rendering Providers")]
        /*public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);*/
        public abstract void HandleGameKey(IStateOwner pOwner, GameKeys g);

        //[Obsolete("This method will be removed eventually... the rendering adapters are where this sort of behaviour should be.")]
        //public abstract void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds);

        protected IBackground _BG;
        public IBackground BG { get => _BG;
            set { _BG = value; } }
    }
}