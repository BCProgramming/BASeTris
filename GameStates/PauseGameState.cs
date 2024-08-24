using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using BASeCamp.Logging;
using BASeTris.AI;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.Choosers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Rendering.Adapters;
using BASeTris.Settings;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Audio;
using OpenTK;
using OpenTK.Windowing.Desktop;
using SkiaSharp;

namespace BASeTris.GameStates
{
    public class PauseGameState : MenuState,ICompositeState<GameplayGameState>,IStateOwner
    {


        #region IStateOwner implementations. 

        //Remember these are for the "Fake" Game Owner so the inner Standard State thinks it is playing a game but we are just letting it run and painting it's playfield!
        public GameState CurrentState
        {
            get => PauseGamePlayerState; set
            {
                PauseGamePlayerState = value;
            }
        }
        public event EventHandler<GameClosingEventArgs> GameClosing;
        
        //GameArea is the full pause screen area, so it's the same as our owner.
        public Rectangle GameArea => PauseOwner.GameArea;

        public double ScaleFactor => PauseOwner.ScaleFactor;

        public Stopwatch GameTime
        {
            get => PauseOwner.GameTime;
            set { }

        }


        //these hopefully aren't needed. We do not want to assign, that is for sure.
        
        public TimeSpan FinalGameTime { get => PauseOwner.FinalGameTime; set  {; } }
        public AudioThemeManager AudioThemeMan { get { return PauseOwner.AudioThemeMan; } set { PauseOwner.AudioThemeMan = value; } }
        public BCRect LastDrawBounds => PauseOwner.LastDrawBounds;

        public SettingsManager Settings => PauseOwner.Settings;

        //we implement IStateOwner as a delegate, so we can provide ourselves to the PausePlayerState as an owner and it won't actually interfere with the main state.
        public bool DrawDataInitialized = false;
        public GameplayGameState PausedState = null;
        public const int NumFallingItems = 65;
        public List<PauseFallImageBase> FallImages = null;
        public override DisplayMode SupportedDisplayMode { get{ return DisplayMode.Partitioned; } }
        
        public GameState PauseGamePlayerState { get; set; }
        //public GamePresenter PauseGamePresenter { get; set; }
      
        public StandardNominoAI PausePlayerAI = null;

        public event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange;

        public GameplayGameState GetComposite()
        {
            return PausedState;
        }
        private IStateOwner PauseOwner = null;
        public PauseGameState(IStateOwner pOwner, GameplayGameState pPausedState)
        {
            StateHeader = "PAUSED";
            PausedState = pPausedState;
            PauseOwner = pOwner;
            //initialize the given number of arbitrary tetronimo pause drawing images.
          
            PopulatePauseMenu(pOwner);
            pOwner.GameClosing += POwner_GameClosing;
            
            PauseGamePlayerState = new GameplayGameState(pOwner, pPausedState.GameHandler.NewInstance(), null,new SilentSoundManager(TetrisGame.Soundman),null);
            (PauseGamePlayerState as GameplayGameState).Flags = GameplayGameState.GameplayStateFlags.Paused;
            //PauseGamePresenter = new GamePresenter(this);
            PausePlayerAI = new StandardNominoAI(this);
            PausePlayerAI.ScoringRules.StupidFactor = 1f;
            //PausePlayerAI.ScoringRules.Moronic = true;
            
            //PauseGamePresenter.ai = PausePlayerAI;
            //PauseGamePresenter.IgnoreController = true;

        }

        private void POwner_GameClosing(object sender, GameClosingEventArgs e)
        {
            if(PausePlayerAI!=null) PausePlayerAI.AbortAI();
        }

        private void PopulatePauseMenu(IStateOwner pOwner)
        {
            var FontSrc = TetrisGame.GetRetroFont(14, 1.0f);
            MenuStateTextMenuItem ResumeOption = new MenuStateTextMenuItem() { Text = "Resume",TipText = "Back to the game" };
            var HighScoresItem = new MenuStateHighScoreItem(pOwner, this, FontSrc) { Text = "High Scores", TipText = "View high scores" };
            var ControlsOption = new MenuStateTextMenuItem() { Text = "Controls", TipText = "Display control settings." };
            //var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores",TipText="View high scores" };
          
            
            ResumeOption.FontFace = FontSrc.FontFamily.Name;
            ResumeOption.FontSize = FontSrc.Size;
            //ResumeOption.Font = TetrisGame.GetRetroFont(14, 1.0f);

            var scaleitem = new MenuStateScaleMenuItem(pOwner) { TipText = "Change Scaling" };
            scaleitem.FontFace = FontSrc.FontFamily.Name;
            scaleitem.FontSize = FontSrc.Size;

            var ThemeItem = new MenuStateTextMenuItem() { Text = PausedState.PlayField.Theme.Name };
            
            //var ThemeItem = new MenuStateDisplayThemeMenuItem(pOwner, PausedState.GameHandler.GetType()) { TipText = "Change Display Theme" };
            ThemeItem.FontFace = FontSrc.FontFamily.Name;
            ThemeItem.FontSize = FontSrc.Size;


            HighScoresItem.FontFace = FontSrc.FontFamily.Name;
            HighScoresItem.FontSize = FontSrc.Size;

            ControlsOption.FontFace = FontSrc.FontFamily.Name;
            ControlsOption.FontSize = FontSrc.Size;

            this.MenuItemActivated += (o, a) =>
            {
                if (a.MenuElement == ResumeOption)
                    ResumeGame(pOwner);
                else if (a.MenuElement == ControlsOption)
                {

                    var ControlsState = new ControlSettingsViewState(pOwner.CurrentState, pOwner.Settings, ControlSettingsViewState.ControllerSettingType.Gamepad);
                    pOwner.CurrentState = ControlsState;
                    ActivatedItem = null;

                }
                else if (a.MenuElement == ThemeItem)
                {

                    a.CancelActivation = true;
                    NominoTheme FoundCurrent = null;
                    if (a.Owner.CurrentState is GameplayGameState gpgs)
                        FoundCurrent = gpgs.PlayField.Theme;
                    else if (a.Owner.CurrentState is ICompositeState<GameplayGameState> ics)
                    {
                        FoundCurrent = ics.GetComposite().PlayField.Theme;
                    }
                    ThemeSelectionMenuState ms = new ThemeSelectionMenuState(a.Owner, StandardImageBackgroundSkia.GetMenuBackgroundDrawer(), a.Owner.CurrentState, a.Owner.GetHandler().GetType(), FoundCurrent==null?null:FoundCurrent.GetType(), (nt) =>
                    {
                        ChangeThemeOption = nt;
                        a.Owner.BeforeGameStateChange -= Owner_BeforeGameStateChange;
                        a.Owner.BeforeGameStateChange += Owner_BeforeGameStateChange;
                        a.Owner.CurrentState = this;
                        this.ActivatedItem = null;
                        if (a.MenuElement is MenuStateTextMenuItem mstmi)
                        {
                            mstmi.Text = ChangeThemeOption.Name;
                        }
                    });
                    
                    a.Owner.CurrentState = ms;
                    this.ActivatedItem = null;
                    

                }
            };


            var ExitItem = new ConfirmedTextMenuItem() {Text="Quit"};
            ExitItem.FontFace = FontSrc.FontFamily.Name;
            ExitItem.FontSize = FontSrc.Size;
            
            ExitItem.OnOptionConfirmed += (a, b) =>
            {
                //testing: save a suspension of this game.
                if (PausedState is GameplayGameState ggs)
                {
                    if (false && pOwner.GetElapsedTime().TotalMinutes > 5)
                    {
                        DebugLogger.Log.WriteLine("Game is being quit, after more than a minute. Saving game state.");
                        XElement result = ggs.SaveState(pOwner,"Field");
                        //save suspended game.
                        String SuspendedGameFilename = TetrisGame.GetSuspendedGamePath(pOwner.GetHandler().GetType());
                        TetrisGame.EnsurePath(SuspendedGameFilename);
                        result.Save(SuspendedGameFilename);
                    }
                    else 
                    {
                        DebugLogger.Log.WriteLine("Game is being quit, but was played for less than a minute. Current Game will not be suspended.");
                    }

                }
                
                if (pOwner is Form f)
                {
                    pOwner.CurrentState = new GenericMenuState(StandardImageBackgroundGDI.GetStandardBackgroundDrawer(),pOwner , new TitleMenuPopulator()) { StateHeader = "BASeTris" };
                }
                else if(pOwner is GameWindow gw)
                {
                    pOwner.CurrentState = new GenericMenuState(StandardImageBackgroundSkia.GetMenuBackgroundDrawer(), pOwner, new TitleMenuPopulator()) { StateHeader = "BASeTris" };
                }
                PausePlayerAI.AbortAI();
            };

            MenuElements.Add(ResumeOption);
            MenuElements.Add(scaleitem);
            MenuElements.Add(ThemeItem);
            MenuElements.Add(ControlsOption);
            MenuElements.Add(HighScoresItem);
            MenuElements.Add(ExitItem);
        }
        NominoTheme ChangeThemeOption = null;
        private void Owner_BeforeGameStateChange(object sender, BeforeGameStateChangeEventArgs e)
        {
            
            {
                if (e.NewState is GameplayGameState newstate)
                {
                    //if it's a standard state, we set the Theme of the TetrisField, and un-assign this event.
                    var generated = ChangeThemeOption;
                    e.Owner.Settings.GetSettings(e.Owner.GetHandler().Name).Theme = generated.Name;
                    e.Owner.Settings.Save();
                    newstate.PlayField.Theme = generated;
                    newstate.DoRefreshBackground = true;

                    e.Owner.BeforeGameStateChange -= Owner_BeforeGameStateChange;
                }
            }
        }
        

        public override void GameProc(IStateOwner pOwner)
        {
            if(pOwner.GameTime.IsRunning) pOwner.GameTime.Stop();
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
            lock (QueuedOwnerActions)
            {
                while (QueuedOwnerActions.Any())
                {
                    QueuedOwnerActions.Dequeue()();
                }
            }
            PauseGamePlayerState.GameProc(this);
            //addendum: when blocks are higher than say 16, let's remove the 4 bottom lines or something.
            if(PauseGamePlayerState is GameplayGameState stgs)
            {
                if (stgs.PlayField.Contents[6].Any((w) => w != null))
                {
                    //remove the bottom 4 rows.
                    for (int r1 = stgs.PlayField.Contents.Length - 4; r1 < stgs.PlayField.Contents.Length; r1++)
                    {
                        for (int g = r1; g > 0; g--)
                        {
                            //Debug.Print("Moving row " + (g - 1).ToString() + " to row " + g);

                            for (int i = 0; i < stgs.PlayField.ColCount; i++)
                            {
                                stgs.PlayField.Contents[g][i] = stgs.PlayField.Contents[g - 1][i];
                            }
                        }
                    }
                }
            }
            base.GameProc(pOwner);
            //no op!
        }


     
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {

            base.HandleGameKey(pOwner, g);
            //PauseGamePlayerState.HandleGameKey(this, g);
        }

        private void ResumeGame(IStateOwner pOwner)
        {
            var unpauser = new UnpauseDelayGameState
                (PausedState, () => { UnPause(pOwner); });


            var playing = TetrisGame.Soundman.GetPlayingMusic_Active();
            playing?.UnPause();
            playing?.setVolume(0.5f);
            PausePlayerAI.AbortAI();


            pOwner.CurrentState = unpauser;
        }

        private static void UnPause(IStateOwner pOwner)
        {
            TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.Pause.Key, pOwner.Settings.std.EffectVolume);
            var playing2 = TetrisGame.Soundman.GetPlayingMusic_Active();
            pOwner.GameTime.Start();
            playing2?.UnPause();
            playing2?.setVolume(1.0f);
        }
        Queue<Func<bool>> QueuedOwnerActions = new Queue<Func<bool>>();
        public void SetDisplayMode(DisplayMode pMode)
        {
            //throw new NotImplementedException();
            
        }

        public void EnqueueAction(Func<bool> pAction)
        {
            lock (QueuedOwnerActions)
            {
                QueuedOwnerActions.Enqueue(pAction);
            }
        }

        public void Feedback(float Strength, int Length)
        {
            //throw new NotImplementedException();
        }

        public void SetScale(double pScale)
        {
            //throw new NotImplementedException();
        }

        public TimeSpan GetElapsedTime()
        {
            return TimeSpan.Zero;
        }
        #endregion
        public abstract class PauseFallImageBase
        {
            public DateTime LastRotation = DateTime.MinValue;
            public readonly TimeSpan MinimumRotationTime = new TimeSpan(0, 0, 3);
            public readonly TimeSpan MaximumRotationTime = new TimeSpan(0, 0, 20);
            public DateTime? NextSpinTime = null;
            public float Angle = 0;
            public float AngleSpeed = 3;
            public float XPosition;
            public float YPosition;
            public float XSpeed;
            public float YSpeed;
            public object BaseImage;
            public int Rotation = 0;
            
            public virtual void Proc(Object bound)
            {
                //if no spin time is set, set a new one. We nullify the spin time when we spin, so this will reset.
                if(NextSpinTime == null)
                {
                    var maxTime = MaximumRotationTime.Ticks;
                    var minTime = MinimumRotationTime.Ticks;
                    var Range = maxTime - minTime;
                    //if the next spin time is not set, then set it to the current time plus a random rotation time.
                    var randomticks = (long)((TetrisGame.StatelessRandomizer.NextDouble() * (double)Range) + minTime);
                    NextSpinTime = DateTime.Now + TimeSpan.FromTicks(randomticks);
                }
                else if(DateTime.Now > NextSpinTime)
                {
                    NextSpinTime = null;//nullify it
                    //increment the rotation and set the last RotationTime
                    Rotation = MathHelper.mod(Rotation + (TetrisGame.StatelessRandomizer.NextDouble() >0.5?1:-1), 4);
                    LastRotation = DateTime.Now;
                }
            }
            public abstract void Draw(Object g);
        }
        public abstract class PauseFallImageBase<BoundType,CanvasType,ImageType> : PauseFallImageBase
        {
            
            protected abstract void Proc(BoundType GArea);
            public abstract void Draw(CanvasType g);
            public override void Proc(Object bound)
            {
                base.Proc(bound);
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


            protected override void Proc(Rectangle GArea)
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
            protected override void Proc(SKRect GArea)
            {
                XPosition += XSpeed;
                YPosition += YSpeed;
                Angle += AngleSpeed;
                if (XPosition < GArea.Left - OurImage.Width) XPosition = GArea.Right + OurImage.Width;
                if (XPosition > GArea.Right + OurImage.Width) XPosition = GArea.Left - OurImage.Width;
                if (YPosition < GArea.Top - OurImage.Height) YPosition = GArea.Bottom + OurImage.Height;
                if (YPosition > GArea.Bottom + OurImage.Height) YPosition = GArea.Top - OurImage.Height;

            }
            const double SpinTimeSeconds = 0.250;
            public override void Draw(SKCanvas g)
            {
                
                double Angleuse = Angle;
                double PercentRotationComplete = Math.Min(SpinTimeSeconds, (DateTime.Now - LastRotation).TotalSeconds) / SpinTimeSeconds;
                double useRotation = (Rotation * 90d) - ((1-PercentRotationComplete) * 90);
                g.ResetMatrix();
                g.Translate((XPosition + ((float)OurImage.Width / 2)), (YPosition + ((float)OurImage.Height / 2)));
                g.RotateDegrees((float)useRotation);
                g.Translate(-(XPosition + ((float)OurImage.Width / 2)), -(YPosition + ((float)OurImage.Height / 2)));
                g.DrawBitmap(OurImage, new SKRect(XPosition, YPosition, XPosition + OurImage.Width, YPosition + OurImage.Height));
            }
        }

       /* public class PauseFaillImageSkiaSharp : PauseFallImageBase<SKRect,Graphics,Image>
        {

        }*/
    }
}