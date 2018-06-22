using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public class UnpauseDelayGameState :GameState 
    {
        GameState _ReturnState = null;
        DateTime InitialUnpauseTime = DateTime.MinValue;
        TimeSpan PauseDelay = new TimeSpan(0,0,0,5);
        Action ReturnFunc = null;
        public UnpauseDelayGameState(GameState ReturnToState,Action pReturnFunc = null)
        {
            _ReturnState = ReturnToState;
            ReturnFunc = pReturnFunc;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _ReturnState.DrawStats(pOwner,g,Bounds);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
        }
        private int LastSecond;
        TimeSpan timeremaining = TimeSpan.Zero;
        public override void GameProc(IStateOwner pOwner)
        {
            //don't call GameProc of original State
            if (InitialUnpauseTime == DateTime.MinValue)
            {
                InitialUnpauseTime = DateTime.Now;
                LastSecond = PauseDelay.Seconds;
            }
            
          
            
            timeremaining = PauseDelay - (DateTime.Now - InitialUnpauseTime);
            if (timeremaining.Ticks < 0)
            {
                ReturnFunc?.Invoke();
                pOwner.CurrentState = _ReturnState;
                return;
            }
         
        }
        private double lastMillis = 1000;
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _ReturnState.DrawProc(pOwner,g,Bounds);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
            //draw a centered Countdown

            if (LastSecond != timeremaining.Seconds)
            {
                //emit a sound.
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.GameOverShade);
                LastSecond = timeremaining.Seconds;
                lastMillis = 1000;
            }
            double SecondsLeft = Math.Round(timeremaining.TotalSeconds, 1);
            String sSecondsLeft = timeremaining.ToString("%s");
            double Millis = (double)timeremaining.Milliseconds/1000d;  //millis in percent. We will use this to animate the unpause time left.
            Millis = Math.Min(Millis, lastMillis);
            float useSize = (float)(64f * (1 - (Millis)));
            var SecondsFont = TetrisGame.GetRetroFont(useSize, pOwner.ScaleFactor);
            var MeasureText = g.MeasureString(sSecondsLeft, SecondsFont);

            PointF DrawPosition = new PointF(Bounds.Width/2-MeasureText.Width/2,Bounds.Height/2 - MeasureText.Height/2);

            g.DrawString(sSecondsLeft, SecondsFont, Brushes.White,DrawPosition);
            lastMillis = Millis;

        }
        Brush fadeBrush = new SolidBrush(Color.FromArgb(200,Color.Black));
        private void DrawFadeOverlay(Graphics g,RectangleF Bounds)
        {
            g.FillRectangle(fadeBrush,Bounds);
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
    }
}
