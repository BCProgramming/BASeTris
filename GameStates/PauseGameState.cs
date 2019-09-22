using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using SkiaSharp;

namespace BASeTris.GameStates
{
    public class PauseGameState : MenuState
    {
        public GameState PausedState = null;
        int NumFallingItems = 65;
        internal List<PauseFallImageGDIPlus> FallImages = null;
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
                FallImages = new List<PauseFallImageGDIPlus>();
                for (int i = 0; i < NumFallingItems; i++)
                {
                    PauseFallImageGDIPlus pfi = new PauseFallImageGDIPlus();
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
                if(e.MenuElement==ResumeOption)
                    ResumeGame(pOwner);
            };
            ResumeOption.Font = TetrisGame.GetRetroFont(14, 1.0f);

            var scaleitem = new MenuStateScaleMenuItem(pOwner);
            scaleitem.Font = ResumeOption.Font;

            var ThemeItem = new MenuStateDisplayThemeMenuItem(pOwner);
            ThemeItem.Font = ResumeOption.Font;
            

            var ExitItem = new ConfirmedTextMenuItem() {Text="Quit"};
            ExitItem.Font = scaleitem.Font;
            ExitItem.OnOptionConfirmed += (a, b) =>
            {
                Application.Exit();
            };

            MenuElements.Add(ResumeOption);
            MenuElements.Add(scaleitem);
            MenuElements.Add(ThemeItem);
            MenuElements.Add(ExitItem);
        }

        public override float DrawHeader(IStateOwner pOwner,Graphics Target, RectangleF Bounds)
        {
            //for the pause screen, we don't draw the header. We return half the size of the screen though.
            //return base.DrawHeader(Target, Bounds);
            return (float)Bounds.Height *0.6f;
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


     
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
           
                base.HandleGameKey(pOwner,g);
        }

        private void ResumeGame(IStateOwner pOwner)
        {
            var unpauser = new UnpauseDelayGameState
                (PausedState, () => { UnPause(pOwner); });

            var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
            playing?.UnPause();
            playing?.setVolume(0.5f);


            pOwner.CurrentState = unpauser;
        }

        private static void UnPause(IStateOwner pOwner)
        {
            TetrisGame.Soundman.PlaySound(TetrisGame.AudioThemeMan.Pause, pOwner.Settings.EffectVolume);
            var playing2 = TetrisGame.Soundman.GetPlayingMusic_Active();
            playing2?.UnPause();
            playing2?.setVolume(1.0f);
        }

        public abstract class PauseFallImageBase
        {
            public float Angle = 0;
            public float AngleSpeed = 3;
            public float XPosition;
            public float YPosition;
            public float XSpeed;
            public float YSpeed;
            public object BaseImage;
            public abstract void Proc(Object bound);
            public abstract void Draw(Object g);
        }
        public abstract class PauseFallImageBase<BoundType,CanvasType,ImageType> : PauseFallImageBase
        {
            
            public abstract void Proc(BoundType GArea);
            public abstract void Draw(CanvasType g);
            public override void Proc(Object bound)
            {
                this.Proc((BoundType)bound);
            }
            public override void Draw(Object g)
            {
                this.Proc((CanvasType)g);
            }
            public ImageType OurImage {  get { return (ImageType)BaseImage; } set { BaseImage = value; } }
        }

        public class PauseFallImageGDIPlus : PauseFallImageBase<Rectangle,Graphics,Image>
        {


            public override void Proc(Rectangle GArea)
            {
                XPosition += XSpeed;
                YPosition += YSpeed;
                Angle += AngleSpeed;
                if (XPosition < GArea.Left - OurImage.Width) XPosition = GArea.Right + OurImage.Width;
                if (XPosition > GArea.Right + OurImage.Width) XPosition = GArea.Left - OurImage.Width;
                if (YPosition < GArea.Top - OurImage.Height) YPosition = GArea.Bottom + OurImage.Height;
                if (YPosition > GArea.Bottom + OurImage.Height) YPosition = GArea.Top - OurImage.Height;
            }

            public override void Draw(Graphics g)
            {
                g.ResetTransform();
                g.TranslateTransform((XPosition + ((float) OurImage.Width / 2)), (YPosition + ((float) OurImage.Height / 2)));
                g.RotateTransform(Angle);
                g.TranslateTransform(-(XPosition + ((float) OurImage.Width / 2)), -(YPosition + ((float) OurImage.Height / 2)));

                g.DrawImage(OurImage, new Rectangle((int) XPosition, (int) YPosition, OurImage.Width, OurImage.Height), 0f, 0f, OurImage.Width, OurImage.Height, GraphicsUnit.Pixel);
            }
        }

       /* public class PauseFaillImageSkiaSharp : PauseFallImageBase<SKRect,Graphics,Image>
        {

        }*/
    }
}