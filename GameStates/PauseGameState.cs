using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using OpenTK;
using SkiaSharp;

namespace BASeTris.GameStates
{
    public class PauseGameState : MenuState,ICompositeState<StandardTetrisGameState>
    {
        public bool DrawDataInitialized = false;
        public StandardTetrisGameState PausedState = null;
        public const int NumFallingItems = 65;
        public List<PauseFallImageBase> FallImages = null;
        public override DisplayMode SupportedDisplayMode { get{ return DisplayMode.Partitioned; } }
        public StandardTetrisGameState GetComposite()
        {
            return PausedState;
        }
        public PauseGameState(IStateOwner pOwner, StandardTetrisGameState pPausedState)
        {
            StateHeader = "PAUSED";
            PausedState = pPausedState;
            //initialize the given number of arbitrary tetronimo pause drawing images.
          
            PopulatePauseMenu(pOwner);
        }
        private void PopulatePauseMenu(IStateOwner pOwner)
        {
            MenuStateTextMenuItem ResumeOption = new MenuStateTextMenuItem() { Text = "Resume" };
            var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores" };
            MenuItemActivated += (o, e) =>
            {
                if(e.MenuElement==ResumeOption)
                    ResumeGame(pOwner);
                else if(e.MenuElement==HighScoresItem)
                {
                    ShowHighScoresState scorestate = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], this, null);
                    pOwner.CurrentState = scorestate;
                    ActivatedItem = null; //we need to reset this or the item will remain active.
                }
                
            };
            var FontSrc = TetrisGame.GetRetroFont(14, 1.0f);
            ResumeOption.FontFace = FontSrc.FontFamily.Name;
            ResumeOption.FontSize = FontSrc.Size;
            //ResumeOption.Font = TetrisGame.GetRetroFont(14, 1.0f);

            var scaleitem = new MenuStateScaleMenuItem(pOwner);
            scaleitem.FontFace = FontSrc.FontFamily.Name;
            scaleitem.FontSize = FontSrc.Size;
            

            var ThemeItem = new MenuStateDisplayThemeMenuItem(pOwner);
            ThemeItem.FontFace = FontSrc.FontFamily.Name;
            ThemeItem.FontSize = FontSrc.Size;


            HighScoresItem.FontFace = FontSrc.FontFamily.Name;
            HighScoresItem.FontSize = FontSrc.Size;


            var ExitItem = new ConfirmedTextMenuItem() {Text="Quit"};
            ExitItem.FontFace = FontSrc.FontFamily.Name;
            ExitItem.FontSize = FontSrc.Size;
            ExitItem.OnOptionConfirmed += (a, b) =>
            {
                if(pOwner is Form f)
                {
                    f.Close();
                }
                else if(pOwner is GameWindow gw)
                {
                    gw.Exit();
                }
            };

            MenuElements.Add(ResumeOption);
            MenuElements.Add(scaleitem);
            MenuElements.Add(ThemeItem);
            MenuElements.Add(HighScoresItem);
            MenuElements.Add(ExitItem);
        }

       

        

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }

        public override void GameProc(IStateOwner pOwner)
        {
            if (!DrawDataInitialized) return;
            foreach (var iterate in FallImages)
            {
                if(Program.RunMode==Program.StartMode.Mode_WinForms)
                    iterate.Proc(pOwner.GameArea);
                else if(Program.RunMode == Program.StartMode.Mode_OpenTK)
                {
                    var ga = pOwner.GameArea;
                    iterate.Proc(new SKRect(ga.Left, ga.Top, ga.Left + ga.Width, ga.Top + ga.Height));
                }
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
                this.Draw((CanvasType)g);
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

        public class PauseFallImageSkiaSharp : PauseFallImageBase<SKRect,SKCanvas,SKBitmap>
        {
            public override void Proc(SKRect GArea)
            {
                XPosition += XSpeed;
                YPosition += YSpeed;
                Angle += AngleSpeed;
                if (XPosition < GArea.Left - OurImage.Width) XPosition = GArea.Right + OurImage.Width;
                if (XPosition > GArea.Right + OurImage.Width) XPosition = GArea.Left - OurImage.Width;
                if (YPosition < GArea.Top - OurImage.Height) YPosition = GArea.Bottom + OurImage.Height;
                if (YPosition > GArea.Bottom + OurImage.Height) YPosition = GArea.Top - OurImage.Height;

            }
            public override void Draw(SKCanvas g)
            {
                g.ResetMatrix();
                g.Translate((XPosition + ((float)OurImage.Width / 2)), (YPosition + ((float)OurImage.Height / 2)));
                g.RotateDegrees(Angle);
                g.Translate(-(XPosition + ((float)OurImage.Width / 2)), -(YPosition + ((float)OurImage.Height / 2)));
                g.DrawBitmap(OurImage, new SKRect(XPosition, YPosition, XPosition + OurImage.Width, YPosition + OurImage.Height));
            }
        }

       /* public class PauseFaillImageSkiaSharp : PauseFallImageBase<SKRect,Graphics,Image>
        {

        }*/
    }
}