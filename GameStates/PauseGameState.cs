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
        int NumFallingItems = 65;
        private List<PauseFallImage> FallImages = null;
        Random rgen = new Random();
        public PauseGameState(IStateOwner pOwner,GameState pPausedState)
        {
            PausedState = pPausedState;
            //initialize the given number of arbitrary tetronimo pause drawing images.
            if(PausedState is StandardTetrisGameState)
            {
                StandardTetrisGameState std = PausedState as StandardTetrisGameState;
                
                Image[] availableImages = std.GetTetronimoImages();
                var Areause = pOwner.GameArea;
                FallImages = new List<PauseFallImage>();
                for(int i=0;i<NumFallingItems;i++)
                {
                    PauseFallImage pfi = new PauseFallImage();
                    pfi.OurImage = TetrisGame.Choose(availableImages);
                    pfi.XSpeed = (float)(rgen.NextDouble() * 10)-5;
                    pfi.YSpeed = (float)(rgen.NextDouble() * 10)-5;
                    pfi.AngleSpeed = (float)(rgen.NextDouble() * 20)-10;
                    pfi.XPosition = (float)rgen.NextDouble() * (float)Areause.Width;
                    pfi.YPosition = (float)rgen.NextDouble() * (float)Areause.Height;
                    FallImages.Add(pfi);
                }

            }
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
            foreach(var iterate in FallImages)
            {
                iterate.Proc(pOwner.GameArea);
            }
            //no op!
        }

        Font usePauseFont = new Font(TetrisGame.RetroFont, 24);
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
         
            String sPauseText = "Pause";
            SizeF Measured = g.MeasureString(sPauseText, usePauseFont);
            g.FillRectangle(Brushes.Gray, Bounds);
            foreach (var iterate in FallImages)
            {
                iterate.Draw(g);
            }
            g.ResetTransform();
            PointF DrawPos = new PointF(Bounds.Width / 2 - Measured.Width / 2, Bounds.Height / 2 - Measured.Height / 2);
            g.DrawString(sPauseText, usePauseFont, Brushes.White, DrawPos);




        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_Pause)
            {
                var unpauser = new UnpauseDelayGameState(PausedState,()=>
                {
                    var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
                    playing?.UnPause();
                });
                TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Pause);



                pOwner.CurrentState = unpauser;

            }
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
                if (XPosition < GArea.Left-OurImage.Width) XPosition = GArea.Right+OurImage.Width;
                if (XPosition > GArea.Right + OurImage.Width) XPosition = GArea.Left - OurImage.Width;
                if (YPosition < GArea.Top - OurImage.Height) YPosition = GArea.Bottom + OurImage.Height;
                if (YPosition > GArea.Bottom + OurImage.Height) YPosition = GArea.Top - OurImage.Height;

            }
            public void Draw(Graphics g)
            {
                g.ResetTransform();
                g.TranslateTransform((XPosition + ((float)OurImage.Width / 2)), (YPosition + ((float)OurImage.Height / 2)));
                g.RotateTransform(Angle);
                g.TranslateTransform(- (XPosition+((float)OurImage.Width/2)),-(YPosition+((float)OurImage.Height/2)));
                
                g.DrawImage(OurImage, new Rectangle((int)XPosition, (int)YPosition, OurImage.Width, OurImage.Height), 0f, 0f, OurImage.Width, OurImage.Height, GraphicsUnit.Pixel);
            }
        }
    }
}
