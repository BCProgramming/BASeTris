using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{


    public class PauseGameState : GameState
    {
        private GameState PausedState = null;
        public PauseGameState(GameState pPausedState)
        {
            PausedState = pPausedState;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            PausedState.DrawStats(pOwner, g, Bounds);
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            //no op!
        }

        Font usePauseFont = new Font(TetrisGame.RetroFont, 24);
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            g.DrawString(sPauseText, usePauseFont, Brushes.White, DrawPos);




        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_Pause)
            {
                pOwner.CurrentState = PausedState;

                var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                playing.UnPause();
                TetrisGame.Soundman.PlaySound("pause");

            }
        }
    }
}
