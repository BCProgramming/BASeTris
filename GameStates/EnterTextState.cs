using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates
{
    public abstract class EnterTextState : GameState, IDirectKeyboardInputState
    {
        public String[] EntryPrompt = null;
        public enum EntryDrawStyle
        {
            EntryDrawStyle_Preblank,
            EntryDrawStyle_Centered
        }
        public EntryDrawStyle EntryStyle = EntryDrawStyle.EntryDrawStyle_Preblank;
        IStateOwner Owner = null;
        public StringBuilder NameEntered = new StringBuilder("__________");
        public readonly String AvailableChars = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public int CurrentPosition = 0; //position of character being "edited"
        

        public override DisplayMode SupportedDisplayMode
        {
            get { return DisplayMode.Full; }
        }

        public EnterTextState(IStateOwner pOwner, int EntryLength, String PossibleChars = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            Owner = pOwner;
            AvailableChars = PossibleChars;
            NameEntered = new StringBuilder(new String(Enumerable.Repeat('_', EntryLength).ToArray()));
            var sib = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();
           
            _BG = sib;
        }

        public abstract bool ValidateEntry(IStateOwner pOwner, String sCurrentEntry);

        public abstract void CommitEntry(IStateOwner pOwner, String sCurrentEntry);

      

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }

        public Font useFont = null;
        public Font EntryFont = null;
        String Char_Change_Up_Sound = "char_change";
        String Char_Change_Down_Sound = "char_change";
        String Char_Pos_Left = "switch_inactive";
        String Char_Pos_Right = "switch_active";

        protected void ChangeChar(int direction)
        {
            char changechar = NameEntered[CurrentPosition];
            int currentordinal = AvailableChars.IndexOf(changechar);
            if (currentordinal < 0)
                NameEntered[CurrentPosition] = '_';
            else
            {
                currentordinal += direction;
                if (currentordinal < 0) currentordinal = AvailableChars.Length + currentordinal;
                else if (currentordinal > AvailableChars.Length - 1) currentordinal = 0;
                NameEntered[CurrentPosition] = AvailableChars[currentordinal];
            }
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_Drop)
            {
                ChangeChar(1);
                TetrisGame.Soundman.PlaySound(Char_Change_Up_Sound, pOwner.Settings.EffectVolume);
                //change current character "upwards"
            }
            else if (g == GameKeys.GameKey_Down)
            {
                ChangeChar(-1);
                TetrisGame.Soundman.PlaySound(Char_Change_Up_Sound, pOwner.Settings.EffectVolume);
                //change current character "downwards"
            }
            else if (g == GameKeys.GameKey_Left)
            {
                int currpos = CurrentPosition;
                currpos--;
                if (currpos < 0) currpos = NameEntered.Length - 1;
                CurrentPosition = currpos;
                TetrisGame.Soundman.PlaySound(Char_Pos_Left, pOwner.Settings.EffectVolume);
                //change current position -1 (wrapping to the end if needed)
            }
            else if (g == GameKeys.GameKey_Right)
            {
                int currpos = CurrentPosition;
                currpos++;
                if (currpos > NameEntered.Length - 1) currpos = 0;
                CurrentPosition = currpos;
                TetrisGame.Soundman.PlaySound(Char_Pos_Right, pOwner.Settings.EffectVolume);
                //change current position 1 (wrapping tp the start if needed)
            }
            else if (g == GameKeys.GameKey_RotateCW)
            {
                String sEntry = NameEntered.ToString().Trim('_', ' ');
                if (ValidateEntry(pOwner, sEntry))
                {
                    CommitEntry(pOwner, sEntry);
                }
            }

            //throw new NotImplementedException();
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public void KeyPressed(IStateOwner pOwner, Keys pKey)
        {
            if (pKey == Keys.Enter) HandleGameKey(pOwner, GameKeys.GameKey_RotateCW);
            else if (pKey == Keys.Down) HandleGameKey(pOwner, GameKeys.GameKey_Down);
            else if (pKey == Keys.Up) HandleGameKey(pOwner, GameKeys.GameKey_Drop);
            else if (pKey == Keys.Left) HandleGameKey(pOwner, GameKeys.GameKey_Left);
            else if (pKey == Keys.Right) HandleGameKey(pOwner, GameKeys.GameKey_Right);
            else if (pKey == Keys.Back)
            {
                for (int i = CurrentPosition + 1; i < NameEntered.Length - 1; i++)
                {
                    NameEntered[i - 1] = NameEntered[i];
                }

                NameEntered[NameEntered.Length - 1] = '_';
                if (CurrentPosition > 0)
                    CurrentPosition--;
            }
            else if (pKey == Keys.Delete)
            {
                for (int i = CurrentPosition + 1; i < NameEntered.Length - 1; i++)
                {
                    NameEntered[i - 1] = NameEntered[i];
                }

                NameEntered[NameEntered.Length - 1] = '_';
            }

            else if (AvailableChars.Contains(Char.ToUpper((char) pKey)))
            {
                NameEntered[CurrentPosition] = (char) pKey;
                HandleGameKey(pOwner, GameKeys.GameKey_Right);
            }
        }
    }
}