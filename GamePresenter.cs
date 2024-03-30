using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeTris.AI;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Settings;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Audio;
using BASeTris.Theme.Block;
using OpenTK.Input;
using XInput.Wrapper;
using TKKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace BASeTris
{
    
    /// <summary>
    /// Game Presenter implements the base logic that is above TetrisGame itself, but "below" the ultimate "UI" layer involved in handling keys or rendering.
    /// The overall idea is that whatever layer is being used will pass key events to this class, which will handle the Game Logic involved in those keys, and
    /// then the drawing can be dealt with appropriately based on the UI layer, using the Rendering Provider/Handling design for whatever target "canvas" type is being used.
    /// </summary>
    public class GamePresenter
    {
        /*Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys> ControllerKeyLookup = new Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys>()
        {
            {X.Gamepad.GamepadButtons.A, GameState.GameKeys.GameKey_RotateCW},
            {X.Gamepad.GamepadButtons.X, GameState.GameKeys.GameKey_RotateCCW},
            {X.Gamepad.GamepadButtons.RBumper, GameState.GameKeys.GameKey_Hold},
            {X.Gamepad.GamepadButtons.Dpad_Left, GameState.GameKeys.GameKey_Left},
            {X.Gamepad.GamepadButtons.Dpad_Right, GameState.GameKeys.GameKey_Right},
            {X.Gamepad.GamepadButtons.Dpad_Down, GameState.GameKeys.GameKey_Down},
            {X.Gamepad.GamepadButtons.Dpad_Up, GameState.GameKeys.GameKey_Drop},
            {X.Gamepad.GamepadButtons.Start, GameState.GameKeys.GameKey_Pause}
        };*/
        public bool GameThreadPaused = false;
        public bool UserInputDisabled = false;
        public ControllerInputState CIS { get; set; } = null;
        private IStateOwner _Owner = null;
        private IGamePresenter _Presenter = null;
        private SettingsManager _GameSettings = null;
        public AudioThemeManager AudioThemeMan { get; set; } = null;
        public bool IgnoreController = false;
        HashSet<GameState.GameKeys> ActiveKeys = new HashSet<GameState.GameKeys>();
        private TetrisGame _Game;
        public SettingsManager GameSettings {  get { return _GameSettings; } set { _GameSettings = value; } }
        
        public TetrisGame Game {  get { return _Game; } set { _Game = value; } }
        public DASRepeatHandler RepeatHandler { get; set; } = null;
        public Thread GameThread = null;
        public Thread InputThread = null;
        public BaseAI ai { get; set; }

        private Dictionary<String, RenderTag> RenderTags = new Dictionary<string, RenderTag>();
        

        public void AddRenderTag(String Key,RenderTag tag)
        {
            lock (RenderTags)
            {
                if(!RenderTags.ContainsKey(Key))
                RenderTags.Add(Key,tag);
            }
        }
        public void ProcessRenderTags(Func<RenderTag,bool> ProcessTagFunc)
        {
            if (!RenderTags.Any()) return;
            HashSet<String> RemoveItems = new HashSet<String>();
            lock (RenderTags)
            {
                foreach (var iterate in RenderTags)
                {
                    if (ProcessTagFunc(iterate.Value))
                    RemoveItems.Add(iterate.Key);
                }
                foreach (var removekey in RemoveItems)
                {
                    RenderTags.Remove(removekey);
                }
            }
        }

        public ConcurrentQueue<Action> ProcThreadActions { get; set; } = new ConcurrentQueue<Action>();
        public void EnqueueAction(Action pAction)
        {
            ProcThreadActions.Enqueue(pAction);
        }
        public void Feedback(float Strength, int Length)
        {
            if (IgnoreController) return;
            X.Gamepad_1.FFB_Vibrate(Strength, Strength, Length);
        }
        public enum GameHandlingConstants
        {
            Handle_GameThread,
            Handle_Manual
        }
        bool GameLoopsRunning = false;
        public bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }
        public int GetKeyboardKeyFromName(String input)
        {

            TKKey result;
            if (Enum.TryParse<TKKey>(input, out result))
                return (int)result;

            return 0;
        }
        public int GetGamepadButtonFromName(String input)
        {
            XInput.Wrapper.X.Gamepad.GamepadButtons  result;
            if (Enum.TryParse<XInput.Wrapper.X.Gamepad.GamepadButtons>(input, out result))
                return (int)result;

            return 0;
        }
        
        public Func<GameState> ReplayStateCreator(GameReplayOptions gpo)
        {
            //task: create an action that sets up this GamePresenter and returns a new GameplayGame that is prepared and ready to start a replay with the correct handler.


            return () =>
            {
                GameSettings = gpo?.Settings ?? GameSettings;
                IBlockGameCustomizationHandler GameHandler = null;

                Type desiredHandlerType = gpo.GameplayRecord.InitialData.InitialOptions.HandlerType;
                //construct the Game Handler type.
                GameHandler = (IBlockGameCustomizationHandler)Activator.CreateInstance(desiredHandlerType);
                //set the initial options to the ones we saved.
                GameHandler.PrepInstance = gpo.GameplayRecord.InitialData.InitialOptions;

                this.UserInputDisabled = true; //disable user-capable inputs, only take from the injector.
                this.ai = new ReplayInputInjector(_Owner,gpo.GameplayRecord.GetPlayQueue());

                return new GameplayGameState(_Owner, GameHandler, null, TetrisGame.Soundman, null);


            };

            
        }
        public void StartGame(GameHandlingConstants option=GameHandlingConstants.Handle_GameThread)
        {
            String[] sDataFolders = TetrisGame.GetSearchFolders();
            String sSettingsFile = Path.Combine(sDataFolders.First((d)=>Directory.Exists(d) && IsDirectoryWritable(d)), "Settings.xml");
            //Note: GameStartOptions could optionally include this data...
            GameSettings = new SettingsManager(sSettingsFile,_Owner,GetKeyboardKeyFromName,GetGamepadButtonFromName,typeof(TKKey),typeof(XInput.Wrapper.X.Gamepad.GamepadButtons));
            AudioThemeMan = new AudioThemeManager(AudioTheme.GetDefault(GameSettings.std.SoundScheme));
            AudioThemeMan.ResetTheme();
            if (GameLoopsRunning) return;

            /*if (gso.GameplayRecord != null)
            {
                //if we are supplied a record, than this "game" is supposed to be a replay.
                //gso.GameplayRecord should have everything we need to set stuff up.
            }*/
            
            
            GameLoopsRunning = true;
            
            
            
            
            
            //_Owner.GameStartTime = DateTime.MinValue;
            Game = new TetrisGame(_Owner, null);
            var standardstate = new GameplayGameState(_Owner, new StandardTetrisHandler(), null, TetrisGame.Soundman, null);
            Game.CurrentState = standardstate;
            //standardstate.Chooser = new MeanChooser(standardstate,Tetromino.StandardTetrominoFunctions);



            if (GameThread != null)
            {
                GameThread.Abort();
                GameThread = null;
            }
            if (option == GameHandlingConstants.Handle_GameThread)
            {
                GameThread = new Thread(GameProc);
                GameThread.Start();
            }
            if (InputThread != null) InputThread.Abort();
            InputThread = new Thread(GamepadInputThread);
            InputThread.Start();
        }
        public void RunNextThreadAction()
        {
            while (!ProcThreadActions.IsEmpty)
            {
                if (ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }
            }
        
        }
        private GameState LastFrameState = null;
        private void GameProc()
        {
            while (true)
            {
                if (GameThreadPaused)
                {
                    Thread.Sleep(250);
                }


                RunNextThreadAction();

                if (LastFrameState != Game.CurrentState)
                {
                    _Owner.SetDisplayMode(Game.CurrentState.SupportedDisplayMode);
                }

                if (Game.CurrentState != null && !Game.CurrentState.GameProcSuspended)
                {
                    Game.GameProc();
                }

                _Presenter.Present();

                //Thread.Sleep(1);
            }
        }
        private bool CheckButtonState(XInput.Wrapper.X.Gamepad.GamepadButtons button, bool flUpState)
        {
            switch (button)
            {
                case X.Gamepad.GamepadButtons.Dpad_Up:
                    return flUpState ? X.Gamepad_1.Dpad_Up_up : X.Gamepad_1.Dpad_Up_down;
                case X.Gamepad.GamepadButtons.Dpad_Down:
                    return flUpState ? X.Gamepad_1.Dpad_Down_up : X.Gamepad_1.Dpad_Down_down;
                case X.Gamepad.GamepadButtons.Dpad_Left:
                    return flUpState ? X.Gamepad_1.Dpad_Left_up : X.Gamepad_1.Dpad_Left_down;
                case X.Gamepad.GamepadButtons.Dpad_Right:
                    return flUpState ? X.Gamepad_1.Dpad_Right_up : X.Gamepad_1.Dpad_Right_down;
                case X.Gamepad.GamepadButtons.Start:
                    return flUpState ? X.Gamepad_1.Start_up : X.Gamepad_1.Start_down;
                case X.Gamepad.GamepadButtons.Back:
                    return flUpState ? X.Gamepad_1.Back_up : X.Gamepad_1.Back_down;
                    
                case X.Gamepad.GamepadButtons.LeftStick:
                    return flUpState ? X.Gamepad_1.LStick_down : X.Gamepad_1.LStick_up;
                case X.Gamepad.GamepadButtons.RightStick:
                    return flUpState ? X.Gamepad_1.RStick_down : X.Gamepad_1.RStick_up;
                    
                case X.Gamepad.GamepadButtons.LBumper:
                    return flUpState ? X.Gamepad_1.LBumper_up : X.Gamepad_1.LBumper_up;
                    
                case X.Gamepad.GamepadButtons.RBumper:
                    return flUpState ? X.Gamepad_1.RBumper_up : X.Gamepad_1.RBumper_up;
                case X.Gamepad.GamepadButtons.A:
                    return flUpState ? X.Gamepad_1.A_up : X.Gamepad_1.A_up;
                case X.Gamepad.GamepadButtons.B:
                    return flUpState ? X.Gamepad_1.B_up : X.Gamepad_1.B_up;
                case X.Gamepad.GamepadButtons.X:
                    return flUpState ? X.Gamepad_1.X_up : X.Gamepad_1.X_up;
                case X.Gamepad.GamepadButtons.Y:
                    return flUpState ? X.Gamepad_1.Y_up : X.Gamepad_1.Y_up;
            }

            return false;

        }
        private Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys> DefaultButtonMap = new Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys>()
        {
            {X.Gamepad.GamepadButtons.A,GameState.GameKeys.GameKey_RotateCW },
            {X.Gamepad.GamepadButtons.X,GameState.GameKeys.GameKey_RotateCCW },
            {X.Gamepad.GamepadButtons.Dpad_Left,GameState.GameKeys.GameKey_Left },
            {X.Gamepad.GamepadButtons.Dpad_Right,GameState.GameKeys.GameKey_Right },
            {X.Gamepad.GamepadButtons.Dpad_Down,GameState.GameKeys.GameKey_Down },
            {X.Gamepad.GamepadButtons.Dpad_Up,GameState.GameKeys.GameKey_Drop },
            {X.Gamepad.GamepadButtons.Start,GameState.GameKeys.GameKey_Pause },
            {X.Gamepad.GamepadButtons.RBumper,GameState.GameKeys.GameKey_Hold }


        };

        private X.Gamepad.GamepadButtons? LookupKeyMap(GameState.GameKeys gkey)
        {

            foreach (var iterate in DefaultButtonMap)
            {
                if (iterate.Value == gkey) return iterate.Key;
            }
            return null;

        }
        
        private void GamepadInputThread()
        {
            Thread.CurrentThread.IsBackground = true;
            try
            {
                while (true)
                {
                    Thread.Sleep(2);
                    CIS.CheckState();
                }
            }
            catch(ThreadAbortException)
            {
                return;
            }
        }
        
        public GameState.GameKeys? TranslateKey(TKKey source)
        {
            if(Game!=null)
            {
                return Game.TranslateKey(source);
            }
            return null;
        }
        public GameState.GameKeys? TranslateKey(Keys source)
        {
            if (Game != null)
            {
                return Game.TranslateKey(source);
            }
            
            return null;
        }
        public GameState.GameKeys? TranslateKey(X.Gamepad.GamepadButtons Button)
        {
            if (_GameSettings.HasAssignedGamepadButton((int)Button))
            {
                return _GameSettings.GetGamepadButtonAssignment((int)Button);
            }
           /* if (ControllerKeyLookup.ContainsKey(Button))
                return ControllerKeyLookup[Button];*/
            return null;
        }
        public GamePresenter(IStateOwner pOwner):this(pOwner,pOwner as IGamePresenter)
        {

        }
        public GamePresenter(IStateOwner pOwner, IGamePresenter pPresenter,bool NoController = false)
        {
            _Owner = pOwner;
            _Presenter = pPresenter;
            RepeatHandler = new DASRepeatHandler((k) =>
            {
                if (Game != null) Game.HandleGameKey(pOwner, k, TetrisGame.KeyInputSource.Input_HID);
            });
            CIS = new ControllerInputState(X.Gamepad_1);
            if (!NoController)
            {
                CIS.ButtonPressed += CIS_ButtonPressed;
                CIS.ButtonReleased += CIS_ButtonReleased;
            }
        }
        public void HandleKey(GameState.GameKeys key, bool DownState, bool UpState, TetrisGame.KeyInputSource pSource)
        {
            if (ActiveKeys.Contains(key) && !DownState)
            {
                ActiveKeys.Remove(key);
                GameKeyUp(key);
                return;
            }

            if (ActiveKeys.Contains(key)) return;
            if (!DownState) return;
            ActiveKeys.Add(key);
            GameKeyDown(key);
            if (pSource == TetrisGame.KeyInputSource.Input_HID)
            {
                if(UserInputDisabled) return;
                IgnoreController = false;
            }
            Game.HandleGameKey(_Owner, key, pSource);
        }
        private void CIS_ButtonPressed(Object sender, ControllerInputState.ControllerButtonEventArgs args)
        {
            Debug.Print("Button pressed:" + args.button);
            var translated = TranslateKey(args.button);
            if (translated != null)
            {
                Game.HandleGameKey(_Owner, translated.Value, TetrisGame.KeyInputSource.Input_HID);
                GameKeyDown(translated.Value);
            }
        }

        private void CIS_ButtonReleased(Object sender, ControllerInputState.ControllerButtonEventArgs args)
        {
            Debug.Print("Button released:" + args.button);
            var translated = TranslateKey(args.button);
            if (translated != null)
            {
                GameKeyUp(translated.Value);
            }
        }
        //GameKeyUp and GameKeyDown are used for Key repeat.
        //We implement key repeat manually. Typematic rate doesn't work for that anyway.

        public void GameKeyUp(GameState.GameKeys key)
        {
            RepeatHandler.GameKeyUp(key);
        }

        public void GameKeyDown(GameState.GameKeys key)
        {
            RepeatHandler.GameKeyDown(key);
        }

    }
    public class GameReplayOptions
    {
        public SettingsManager Settings { get; set; } = null;
        public GameplayRecord GameplayRecord { get; set; } = null;

    }
    //Presenter has a list of RenderTags. after drawing everything, each rendertag is also called with the RenderObject as the source element. It does this until the DoRender delegate returns false. At that point the RenderTag is removed from the list.
    public class RenderTag
    {
        public Object RenderObject;
        public Func<bool> DoRender;
    }
}
