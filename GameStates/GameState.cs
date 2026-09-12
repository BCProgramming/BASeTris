using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates.GameObjects;
using BASeTris.Rendering.Adapters;
using BASeCamp.StateLib;
using BASeTris.Tetrominoes;
using System;
using System.Collections.Concurrent;
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


namespace BASeTris
{
    //derives from more generic GameState. intent is for the "generic" one to be part of some sort of BCGameLib, then it could be shared between projects.
    public abstract class GameState:BaseGameState<IStateOwner,GameState.GameKeys,GameObject>
    {
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
        private static Dictionary<GameKeys, String> GameKeyAbbreviations = new Dictionary<GameKeys, string>()
        {
            {GameKeys.GameKey_RotateCW,"CW_" },
            {GameKeys.GameKey_RotateCCW,"CCW" },
            {GameKeys.GameKey_Down,"DWN" },
            {GameKeys.GameKey_Hold,"HLD" },
            {GameKeys.GameKey_Drop,"DRP" },
            {GameKeys.GameKey_Left,"LFT" },
            {GameKeys.GameKey_Right,"RGT" },
        };
        public static String GetGameKeyAbbreviation(GameKeys g)
        {
            if (GameKeyAbbreviations.ContainsKey(g)) return GameKeyAbbreviations[g];
            return "???";
        }
        public virtual DisplayMode SupportedDisplayMode { get; }
        protected override Dictionary<GameState.GameKeys, string> GetFriendlyKeyNames()
        {
            return new Dictionary<GameKeys, string>()
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
        }
    }



  
  
}