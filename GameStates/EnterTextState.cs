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
        protected String[] EntryPrompt = null;
        public enum EntryDrawStyle
        {
            EntryDrawStyle_Preblank,
            EntryDrawStyle_Centered
        }
        private EntryDrawStyle EntryStyle = EntryDrawStyle.EntryDrawStyle_Preblank;
        IStateOwner Owner = null;
        StringBuilder NameEntered = new StringBuilder("__________");
        String AvailableChars = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        int CurrentPosition = 0; //position of character being "edited"
        public IBackgroundDraw _BG = null;

        public override DisplayMode SupportedDisplayMode
        {
            get { return DisplayMode.Full; }
        }

        public EnterTextState(IStateOwner pOwner, int EntryLength, String PossibleChars = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            Owner = pOwner;
            AvailableChars = PossibleChars;
            NameEntered = new StringBuilder(new String(Enumerable.Repeat('_', EntryLength).ToArray()));
            var sib = StandardImageBackgroundDraw.GetStandardBackgroundDrawer();
           
            _BG = sib;
        }

        public abstract bool ValidateEntry(IStateOwner pOwner, String sCurrentEntry);

        public abstract void CommitEntry(IStateOwner pOwner, String sCurrentEntry);

        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //never called...
            //Owner.DrawStats(pOwner,g,Bounds);
        }

        public override void GameProc(IStateOwner pOwner)
        {
            _BG.FrameProc();
            //throw new NotImplementedException();
        }

        Font useFont = null;
        protected Font EntryFont = null;
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (useFont == null) useFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);
            if (EntryFont == null) EntryFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);

            float Millipercent = (float) DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            int RotateAmount = (int) (Millipercent * 240);

            Color UseBackgroundColor = HSLColor.RotateHue(Color.DarkBlue, RotateAmount);
            Color UseHighLightingColor = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color useLightRain = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            //throw new NotImplementedException();
            _BG.DrawProc(g, Bounds);
            int StartYPosition = (int) (Bounds.Height * 0.15f);
            var MeasureBounds = g.MeasureString(EntryPrompt[0], useFont);
            for (int i = 0; i < EntryPrompt.Length; i++)
            {
                //draw this line centered at StartYPosition+Height*i...

                int useYPosition = (int) (StartYPosition + (MeasureBounds.Height + 5) * i);
                int useXPosition = (int) (Bounds.Width / 2 - MeasureBounds.Width / 2);
                g.DrawString(EntryPrompt[i], useFont, Brushes.Black, new PointF(useXPosition + 5, useYPosition + 5));
                g.DrawString(EntryPrompt[i], useFont, new SolidBrush(useLightRain), new PointF(useXPosition, useYPosition));
            }

            float nameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (EntryPrompt.Length + 1);


            var AllCharacterBounds = (from c in NameEntered.ToString().ToCharArray() select g.MeasureString(c.ToString(), useFont)).ToArray();
            float useCharWidth = g.MeasureString("_", EntryFont).Width;
            float TotalWidth;
            if (EntryStyle == EntryDrawStyle.EntryDrawStyle_Centered)
            {
                TotalWidth = (useCharWidth + 5) * NameEntered.ToString().Trim('_', ' ').Length;
                
            }
            TotalWidth = (useCharWidth + 5) * NameEntered.Length;
            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
            if (EntryStyle == EntryDrawStyle.EntryDrawStyle_Preblank)
            {
                for (int charpos = 0; charpos < NameEntered.Length; charpos++)
                {
                    char thischar = NameEntered[charpos];
                    float useX = NameEntryX + ((useCharWidth + 5) * (charpos));
                    Brush DisplayBrush = (CurrentPosition == charpos) ? new SolidBrush(UseHighLightingColor) : Brushes.NavajoWhite;
                    Brush ShadowBrush = (CurrentPosition == charpos) ? new SolidBrush(useLightRain) : Brushes.Black;
                    g.DrawString(thischar.ToString(), EntryFont, ShadowBrush, new PointF(useX + 2, nameEntryY + 2));
                    g.DrawString(thischar.ToString(), EntryFont, DisplayBrush, new PointF(useX, nameEntryY));
                }
            }
            else if (EntryStyle == EntryDrawStyle.EntryDrawStyle_Centered)
            {
                //"simpler"- we just draw the trimmed text.
                String TrimEntered = NameEntered.ToString().Trim(' ', '_');
                


            }
        }

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