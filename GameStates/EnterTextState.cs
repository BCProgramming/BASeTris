using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using OpenTK.Input;

namespace BASeTris.GameStates
{
    public abstract class EnterTextState : GameState, IDirectKeyboardInputState
    {
        public DateTime InitialStateTime;
        public TimeSpan EntryAllowanceDelay = new TimeSpan(0, 0, 1);
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
        public bool AllowDirectKeyboardInput()
        {
            return true;
        }
        public EnterTextState(IStateOwner pOwner, int EntryLength, String PossibleChars = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            EntryLength = Math.Min(48, EntryLength);
            Owner = pOwner;
            AvailableChars = PossibleChars;
            NameEntered = new StringBuilder(new String(Enumerable.Repeat('_', EntryLength).ToArray()));
            if (pOwner is BASeTris bt)
            {
                _BG = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();
            }
            else if (pOwner is BASeTrisTK)
            {
                _BG = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
            }
            //var sib = StandardImageBackgroundGDI.GetStandardBackgroundDrawer();

            //_BG = sib;
        }

        public abstract bool ValidateEntry(IStateOwner pOwner, String sCurrentEntry);

        public abstract void CommitEntry(IStateOwner pOwner, String sCurrentEntry);

      
        protected bool AllowTextEntry()
        {
            return (DateTime.Now - InitialStateTime) > EntryAllowanceDelay;
        }
        public override void GameProc(IStateOwner pOwner)
        {
            if(InitialStateTime==default(DateTime))
                InitialStateTime = DateTime.Now;
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
            return;
            if (!AllowTextEntry()) return;
            if (g == GameKeys.GameKey_Drop)
            {
                ChangeChar(1);
                TetrisGame.Soundman.PlaySound(Char_Change_Up_Sound, pOwner.Settings.std.EffectVolume);
                //change current character "upwards"
            }
            else if (g == GameKeys.GameKey_Down)
            {
                ChangeChar(-1);
                TetrisGame.Soundman.PlaySound(Char_Change_Up_Sound, pOwner.Settings.std.EffectVolume);
                //change current character "downwards"
            }
            else if (g == GameKeys.GameKey_Left)
            {
                MovePos(pOwner, -1);
                
                
                //change current position -1 (wrapping to the end if needed)

            }
            else if (g == GameKeys.GameKey_Right)
            {
                MovePos(pOwner, 1);
                //change current position 1 (wrapping tp the start if needed)
            }
            else if (g == GameKeys.GameKey_RotateCW)
            {
                CommitScore(pOwner);
            }

            //throw new NotImplementedException();
        }
        private void MovePos(IStateOwner pOwner,int MoveAmount)
        {
            int currpos = CurrentPosition;
            currpos+=MoveAmount;
            if (currpos > NameEntered.Length - 1) currpos = 0;
            CurrentPosition = currpos;
            TetrisGame.Soundman.PlaySound(MoveAmount<0?Char_Pos_Left:Char_Pos_Right, pOwner.Settings.std.EffectVolume);

        }
        private void CommitScore(IStateOwner pOwner)
        {
            String sEntry = NameEntered.ToString().Trim('_', ' ');
            if (ValidateEntry(pOwner, sEntry))
            {
                CommitEntry(pOwner, sEntry);
            }
        }
        public void KeyDown(IStateOwner pOwner, int pKey)
        {
        }
        public void KeyUp(IStateOwner pOwner, int pKey)
        {

            var k= (OpenTK.Input.Key)pKey;
            if (pKey == (int)OpenTK.Input.Key.Enter)
            {
                CommitScore(pOwner);
            }
            else if (k == Key.BackSpace)
            {
                for (int i = CurrentPosition + 1; i < NameEntered.Length - 1; i++)
                {
                    NameEntered[i - 1] = NameEntered[i];
                }

                NameEntered[NameEntered.Length - 1] = '_';
                if (CurrentPosition > 0)
                    CurrentPosition--;
            }
            else if (k== OpenTK.Input.Key.Delete)
            {
                for (int i = CurrentPosition + 1; i < NameEntered.Length - 1; i++)
                {
                    NameEntered[i - 1] = NameEntered[i];
                }

                NameEntered[NameEntered.Length - 1] = '_';
            }

        }
        public void KeyPressed(IStateOwner pOwner, int pKey)
        {
            
            var k = (OpenTK.Input.Key)pKey;
            if (!AllowTextEntry()) return;

            //if (k == Key.Enter) CommitScore(pOwner);
            if (k == Key.Down) HandleGameKey(pOwner, GameKeys.GameKey_Down);
            else if (k == Key.Up) HandleGameKey(pOwner, GameKeys.GameKey_Drop);
            else if (k == Key.Left) MovePos(pOwner, -1);
            else if (k == Key.Right) MovePos(pOwner, 1);
           

            else if (AvailableChars.Contains(Char.ToUpper((char)pKey)))
            {
                NameEntered[CurrentPosition] = (char)pKey;
                MovePos(pOwner, 1);
                //HandleGameKey(pOwner, GameKeys.GameKey_Right);
            }
        }
    }
}