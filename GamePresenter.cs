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
using BASeTris.Tetrominoes;
using BASeTris.Theme.Block;
using OpenTK.Input;
using XInput.Wrapper;

namespace BASeTris
{
    /// <summary>
    /// Game Presenter implements the base logic that is above TetrisGame itself, but "below" the ultimate "UI" layer involved in handling keys or rendering.
    /// The overall idea is that whatever layer is being used will pass key events to this class, which will handle the Game Logic involved in those keys, and
    /// then the drawing can be dealt with appropriately based on the UI layer, using the Rendering Provider/Handling design for whatever target "canvas" type is being used.
    /// </summary>
    public class GamePresenter
    {
        Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys> ControllerKeyLookup = new Dictionary<X.Gamepad.GamepadButtons, GameState.GameKeys>()
        {
            {X.Gamepad.GamepadButtons.A, GameState.GameKeys.GameKey_RotateCW},
            {X.Gamepad.GamepadButtons.X, GameState.GameKeys.GameKey_RotateCCW},
            {X.Gamepad.GamepadButtons.RBumper, GameState.GameKeys.GameKey_Hold},
            {X.Gamepad.GamepadButtons.Dpad_Left, GameState.GameKeys.GameKey_Left},
            {X.Gamepad.GamepadButtons.Dpad_Right, GameState.GameKeys.GameKey_Right},
            {X.Gamepad.GamepadButtons.Dpad_Down, GameState.GameKeys.GameKey_Down},
            {X.Gamepad.GamepadButtons.Dpad_Up, GameState.GameKeys.GameKey_Drop},
            {X.Gamepad.GamepadButtons.Start, GameState.GameKeys.GameKey_Pause}
        };
        public bool GameThreadPaused = false;
        public ControllerInputState CIS { get; set; } = null;
        private IStateOwner _Owner = null;
        private IGamePresenter _Presenter = null;
        private StandardSettings _GameSettings = null;
        
        public bool IgnoreController = false;
        HashSet<GameState.GameKeys> ActiveKeys = new HashSet<GameState.GameKeys>();
        private TetrisGame _Game;
        public StandardSettings GameSettings {  get { return _GameSettings; } set { _GameSettings = value; } }
        
        public TetrisGame Game {  get { return _Game; } set { _Game = value; } }
        public DASRepeatHandler RepeatHandler { get; set; } = null;
        public Thread GameThread = null;
        public Thread InputThread = null;
        public TetrisAI ai { get; set; }

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
        public void StartGame(GameHandlingConstants option=GameHandlingConstants.Handle_GameThread)
        {
            String sDataFolder = TetrisGame.AppDataFolder;
            String sSettingsFile = Path.Combine(sDataFolder, "Settings.xml");
            GameSettings = new StandardSettings(sSettingsFile);


            var standardstate = new StandardTetrisGameState(Tetromino.BagTetrominoChooser(), new GarbageFieldInitializer(new Random(), new NESTetrominoTheme(), 1));
            Game = new TetrisGame(_Owner, standardstate);
            
            //standardstate.Chooser = new MeanChooser(standardstate,Tetromino.StandardTetrominoFunctions);


            TetrisGame.AudioThemeMan.ResetTheme();
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
            if (ProcThreadActions.TryDequeue(out Action pResult))
            {
                pResult();
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

                Thread.Sleep(5);
            }
        }
        private void CheckInputs()
        {
            if (X.Gamepad_1.Update())
            {
                HandleKey(GameState.GameKeys.GameKey_RotateCW, X.Gamepad_1.A_down, X.Gamepad_1.A_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_RotateCCW, X.Gamepad_1.X_down, X.Gamepad_1.X_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Left, X.Gamepad_1.Dpad_Left_down, X.Gamepad_1.Dpad_Left_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Right, X.Gamepad_1.Dpad_Right_down, X.Gamepad_1.Dpad_Right_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Down, X.Gamepad_1.Dpad_Down_down, X.Gamepad_1.Dpad_Down_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Drop, X.Gamepad_1.Dpad_Up_down, X.Gamepad_1.Dpad_Up_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Pause, X.Gamepad_1.Start_down, X.Gamepad_1.Start_up, TetrisGame.KeyInputSource.Input_HID);
                HandleKey(GameState.GameKeys.GameKey_Hold, X.Gamepad_1.RBumper_down, X.Gamepad_1.RBumper_up, TetrisGame.KeyInputSource.Input_HID);
            }
        }
        private void GamepadInputThread()
        {
            while (true)
            {
                Thread.Sleep(3);
                CIS.CheckState();
            }
        }
        public GameState.GameKeys? TranslateKey(Key source)
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
            if (ControllerKeyLookup.ContainsKey(Button))
                return ControllerKeyLookup[Button];
            return null;
        }
        public GamePresenter(IStateOwner pOwner):this(pOwner,pOwner as IGamePresenter)
        {

        }
        public GamePresenter(IStateOwner pOwner, IGamePresenter pPresenter)
        {
            _Owner = pOwner;
            _Presenter = pPresenter;
            RepeatHandler = new DASRepeatHandler((k) =>
            {
                if (Game != null) Game.HandleGameKey(pOwner, k, TetrisGame.KeyInputSource.Input_HID);
            });
            CIS = new ControllerInputState(X.Gamepad_1);
            CIS.ButtonPressed += CIS_ButtonPressed;
            CIS.ButtonReleased += CIS_ButtonReleased;
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
            IgnoreController = false;
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
}
