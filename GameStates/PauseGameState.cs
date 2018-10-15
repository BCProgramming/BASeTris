using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.GameStates.Menu;

namespace BASeTris.GameStates
{
    public class PauseGameState : MenuState
    {
        private GameState PausedState = null;
        int NumFallingItems = 65;
        private List<PauseFallImage> FallImages = null;
        Random rgen = new Random();
        public override DisplayMode SupportedDisplayMode { get{ return DisplayMode.Partitioned; } }

        public PauseGameState(IStateOwner pOwner, GameState pPausedState)
        {
            PausedState = pPausedState;
            //initialize the given number of arbitrary tetronimo pause drawing images.
            if (PausedState is StandardTetrisGameState)
            {
                StandardTetrisGameState std = PausedState as StandardTetrisGameState;

                Image[] availableImages = std.GetTetronimoImages();
                var Areause = pOwner.GameArea;
                FallImages = new List<PauseFallImage>();
                for (int i = 0; i < NumFallingItems; i++)
                {
                    PauseFallImage pfi = new PauseFallImage();
                    pfi.OurImage = TetrisGame.Choose(availableImages);
                    pfi.XSpeed = (float) (rgen.NextDouble() * 10) - 5;
                    pfi.YSpeed = (float) (rgen.NextDouble() * 10) - 5;
                    pfi.AngleSpeed = (float) (rgen.NextDouble() * 20) - 10;
                    pfi.XPosition = (float) rgen.NextDouble() * (float) Areause.Width;
                    pfi.YPosition = (float) rgen.NextDouble() * (float) Areause.Height;
                    FallImages.Add(pfi);
                }
            }
            PopulatePauseMenu(pOwner);
        }
        private void PopulatePauseMenu(IStateOwner pOwner)
        {
            MenuStateTextMenuItem ResumeOption = new MenuStateTextMenuItem() { Text = "Resume" };
            MenuItemActivated += (o, e) =>
            {
                ResumeGame(pOwner);
            };
            ResumeOption.Font = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            MenuElements.Add(ResumeOption);
        }

        protected override float DrawHeader(Graphics Target, RectangleF Bounds)
        {
            //for the pause screen, we don't draw the header. We return half the size of the screen though.
            //return base.DrawHeader(Target, Bounds);
            return (float)Bounds.Height *0.6f;
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
            foreach (var iterate in FallImages)
            {
                iterate.Proc(pOwner.GameArea);
            }
            base.GameProc(pOwner);
            //no op!
        }


        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            Font usePauseFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }

            g.ResetTransform();
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            TetrisGame.DrawText(g,usePauseFont,sPauseText,Brushes.White,Brushes.Black,DrawPos.X,DrawPos.Y);

            base.DrawProc(pOwner,g,Bounds); //draw the menu itself.
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_Pause)
            {
                ResumeGame(pOwner);
            }
            else
            {
                base.HandleGameKey(pOwner,g);
            }
        }

        private void ResumeGame(IStateOwner pOwner)
        {
            var unpauser = new UnpauseDelayGameState
                (PausedState, () => { UnPause(); });

            var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
            playing?.UnPause();
            playing?.setVolume(0.5f);


            pOwner.CurrentState = unpauser;
        }

        private static void UnPause()
        {
            TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Pause);
            var playing2 = TetrisGame.Soundman.GetPlayingMusic_Active();
            playing2?.UnPause();
            playing2?.setVolume(1.0f);
        }

        private class PauseFallImage
        {
            public float Angle = 0;
            public float AngleSpeed = 3;
            public float XPosition;
            public float YPosition;
            public float XSpeed;
            public float YSpeed;
            public Image OurImage;

            public void Proc(Rectangle GArea)
            {
                XPosition += XSpeed;
                YPosition += YSpeed;
                Angle += AngleSpeed;
                if (XPosition < GArea.Left - OurImage.Width) XPosition = GArea.Right + OurImage.Width;
                if (XPosition > GArea.Right + OurImage.Width) XPosition = GArea.Left - OurImage.Width;
                if (YPosition < GArea.Top - OurImage.Height) YPosition = GArea.Bottom + OurImage.Height;
                if (YPosition > GArea.Bottom + OurImage.Height) YPosition = GArea.Top - OurImage.Height;
            }

            public void Draw(Graphics g)
            {
                g.ResetTransform();
                g.TranslateTransform((XPosition + ((float) OurImage.Width / 2)), (YPosition + ((float) OurImage.Height / 2)));
                g.RotateTransform(Angle);
                g.TranslateTransform(-(XPosition + ((float) OurImage.Width / 2)), -(YPosition + ((float) OurImage.Height / 2)));

                g.DrawImage(OurImage, new Rectangle((int) XPosition, (int) YPosition, OurImage.Width, OurImage.Height), 0f, 0f, OurImage.Width, OurImage.Height, GraphicsUnit.Pixel);
            }
        }
    }
}