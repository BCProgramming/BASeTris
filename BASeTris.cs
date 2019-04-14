using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseTris;
using BASeCamp.BASeScores;
using BASeTris.AI;
using BASeTris.AssetManager;
using BASeTris.Choosers;
using BASeTris.Choosers.AIChoosers;
using BASeTris.FieldInitializers;
using BASeTris.GameStates;
using BASeTris.TetrisBlocks;
using BASeTris.Tetrominoes;
using XInput.Wrapper;

namespace BASeTris
{
    public partial class BASeTris : Form, IStateOwner
    {
        private StandardSettings GameSettings = null;
        private List<GameObject> GameObjects = new List<GameObject>();
        private TetrisGame _Game;
        //delegate the BeforeGameStateChange event...
        public event EventHandler<BeforeGameStateChangeEventArgs> BeforeGameStateChange
        {
            add => _Game.BeforeGameStateChange += value;
            remove => _Game.BeforeGameStateChange -= value;
        }
        public StandardSettings Settings 
        {
            get
            {
                return GameSettings;
            }
        }
        public DateTime GameStartTime
        {
            get => _Game.GameStartTime;
            set => _Game.GameStartTime = value;
        }

        public TimeSpan FinalGameTime
        {
            get => _Game.FinalGameTime;
            set => _Game.FinalGameTime = value;
        }

        public DateTime LastPausedTime
        {
            get => _Game.LastPausedTime;
            set => _Game.LastPausedTime = value;
        }

        public TimeSpan GetElapsedTime()
        {
            return _Game.GetElapsedTime();
        }


        ControllerInputState CIS = null;
        HashSet<Keys> PressedKeys = new HashSet<Keys>();

        public BASeTris()
        {
            InitializeComponent();
        }

        public void AddGameObject(GameObject go)
        {
            GameObjects.Add(go);
        }

      

        public double ScaleFactor
        {
            get { return current_factor; }
        }

        public Rectangle GameArea
        {
            get { return picTetrisField.ClientRectangle; }
        }

        public static readonly double DefaultWidth = 643d;
        public static readonly double DefaultHeight = 734d;

        public void SetDisplayMode(GameState.DisplayMode pMode)
        {
            if (picFullSize.Visible == (pMode == GameState.DisplayMode.Full)) return; //if full size visibility matches the passed state being full, we are already in that state.
            picFullSize.Visible = pMode == GameState.DisplayMode.Full;
            picTetrisField.Visible = pMode == GameState.DisplayMode.Partitioned;
            picStatistics.Visible = pMode == GameState.DisplayMode.Partitioned;
        }

        private double current_factor = 1;

        public void SetScale(double factor)
        {
            current_factor = factor;
            
            Invoke((MethodInvoker)(()=>
            {
                mnuScale_Tiny.Checked = mnuScale_Small.Checked = mnuScale_Large.Checked = mnuScale_Biggliest.Checked = false;
                Size = new Size((int) (DefaultWidth * factor), (int) (DefaultHeight * factor));
            }));
        }

        public void Feedback(float Strength, int Length)
        {
            if (IgnoreController) return;
            X.Gamepad_1.FFB_Vibrate(Strength, Strength, Length);
        }

        DASRepeatHandler repeatHandler = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            repeatHandler = new DASRepeatHandler
            ((k) =>
            {
                if (_Game != null) _Game.HandleGameKey(this, k, TetrisGame.KeyInputSource.Input_HID);
            });
            //XMLHighScores<NoSpecialInfo> TestScores = new XMLHighScores<NoSpecialInfo>(35000,(r)=>new NoSpecialInfo());
            //int Position1 = TestScores.IsEligible(12000);
            //int Position2 = TestScores.IsEligible(3000);
            CIS = new ControllerInputState(X.Gamepad_1);
            CIS.ButtonPressed += CIS_ButtonPressed;
            CIS.ButtonReleased += CIS_ButtonReleased;
            menuStrip1.Font = new Font(menuStrip1.Font.FontFamily, 14, FontStyle.Regular);
            Win10MenuRenderer buildrender = new Win10MenuRenderer(null, true);

            menuStrip1.Renderer = buildrender;
            menuStrip1.BackColor = SystemColors.Control;
            TetrisGame.InitState();
        }

        private void CIS_ButtonPressed(Object sender, ControllerInputState.ControllerButtonEventArgs args)
        {
            Debug.Print("Button pressed:" + args.button);
            var translated = TranslateKey(args.button);
            if (translated != null)
            {
                _Game.HandleGameKey(this, translated.Value, TetrisGame.KeyInputSource.Input_HID);
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

        bool XPolling = false;
        BlockGroup testBG = null;

        private void StartGame()
        {
            String sDataFolder = TetrisGame.AppDataFolder;
            String sSettingsFile = Path.Combine(sDataFolder, "Settings.xml");
            GameSettings = new StandardSettings(sSettingsFile);
            var standardstate = new StandardTetrisGameState(Tetromino.BagTetrominoChooser(), new GarbageFieldInitializer(new Random(), new NESTetrominoTheme(), 1));
            _Game = new TetrisGame(this, standardstate);
            //standardstate.Chooser = new MeanChooser(standardstate,Tetromino.StandardTetrominoFunctions);


            TetrisGame.AudioThemeMan.ResetTheme();
            if (GameThread != null) GameThread.Abort();
            GameThread = new Thread(GameProc);
            GameThread.Start();
            if (InputThread != null) InputThread.Abort();
            InputThread = new Thread(GamepadInputThread);
            InputThread.Start();
        }

        private TetrisAI ai;
        private Thread GameThread = null;
        private Thread InputThread = null;
        private ConcurrentQueue<Action> ProcThreadActions = new ConcurrentQueue<Action>();
        HashSet<GameState.GameKeys> ActiveKeys = new HashSet<GameState.GameKeys>();
        private bool IgnoreController = true;

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


        //GameKeyUp and GameKeyDown are used for Key repeat.
        //We implement key repeat manually. Typematic rate doesn't work for that anyway.

        private void GameKeyUp(GameState.GameKeys key)
        {
            repeatHandler.GameKeyUp(key);
        }

        private void GameKeyDown(GameState.GameKeys key)
        {
            repeatHandler.GameKeyDown(key);
        }

        private void HandleKey(GameState.GameKeys key, bool DownState, bool UpState, TetrisGame.KeyInputSource pSource)
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
            _Game.HandleGameKey(this, key, pSource);
        }

        private void GamepadInputThread()
        {
            while (true)
            {
                Thread.Sleep(3);
                CIS.CheckState();
                //CheckInputs();
            }
        }

        private GameState LastFrameState = null;

        private void GameProc()
        {
            while (true)
            {
                if (!IsHandleCreated || IsDisposed)
                {
                    if (!this.Focused)
                    {
                        Thread.Sleep(250);
                        continue;
                    }
                }

                if (ProcThreadActions.TryDequeue(out Action pResult))
                {
                    pResult();
                }

                if (LastFrameState != _Game.CurrentState)
                {
                    Invoke((MethodInvoker) (() => { SetDisplayMode(_Game.CurrentState.SupportedDisplayMode); }));
                }

                if (_Game.CurrentState != null && !_Game.CurrentState.GameProcSuspended)
                {
                    _Game.GameProc();
                }

                Invoke
                ((MethodInvoker) (() =>
                {
                    if (_Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
                    {
                        picTetrisField.Invalidate();
                        picTetrisField.Refresh();
                        picStatistics.Invalidate();
                        picStatistics.Refresh();
                    }
                    else if (_Game.CurrentState.SupportedDisplayMode == GameState.DisplayMode.Full)
                    {
                        picFullSize.Invalidate();
                        picFullSize.Refresh();
                    }
                }));

                Thread.Sleep(5);
            }
        }

        static Random rgen = new Random();

        private void picFullSize_Paint(object sender, PaintEventArgs e)
        {
            if (_Game == null) return;
            if (CurrentState.SupportedDisplayMode == GameState.DisplayMode.Full)
            {
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;

                _Game.DrawProc(e.Graphics, new RectangleF(picFullSize.ClientRectangle.Left, picFullSize.ClientRectangle.Top, picFullSize.ClientRectangle.Width, picFullSize.ClientRectangle.Height));
            }
        }

        private void picTetrisField_Paint(object sender, PaintEventArgs e)
        {
            if (_Game == null) return;
            if (picTetrisField.Visible == false) return;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            _Game.DrawProc(e.Graphics, new RectangleF(picTetrisField.ClientRectangle.Left, picTetrisField.ClientRectangle.Top, picTetrisField.ClientRectangle.Width, picTetrisField.ClientRectangle.Height));
        }

        private void picTetrisField_Resize(object sender, EventArgs e)
        {
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }

        private void picTetrisField_Click(object sender, EventArgs e)
        {
            //testBG.Rotate(false);
            picTetrisField.Invalidate();
            picTetrisField.Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            IgnoreController = true;
            if (e.KeyCode == Keys.G)
            {
                if (_Game.CurrentState is StandardTetrisGameState)
                {
                    StandardTetrisGameState gs = _Game.CurrentState as StandardTetrisGameState;
                    TetrisBlock[][] inserts = new TetrisBlock[4][];
                    for (int i = 0; i < inserts.Length; i++)
                    {
                        inserts[i] = new TetrisBlock[gs.PlayField.ColCount];
                        for (int c = 1; c < inserts[i].Length; c++)
                        {
                            inserts[i][c] = new StandardColouredBlock() {BlockColor = Color.Red, DisplayStyle = StandardColouredBlock.BlockStyle.Style_CloudBevel};
                        }
                    }

                    InsertBlockRowsActionGameState irs = new InsertBlockRowsActionGameState(gs, 0, inserts, Enumerable.Empty<Action>());
                    CurrentState = irs;
                }
            }
            else if (e.KeyCode == Keys.C)
            {
                if (e.Shift && e.Control)
                {
                    EnterCheatState cheatstate = new EnterCheatState(CurrentState, _Game, 16);
                    CurrentState = cheatstate;
                }
            }

            Debug.Print("Button pressed:" + e.KeyCode);
            var translated = TranslateKey(e.KeyCode);
            if (translated != null)
            {
                _Game.HandleGameKey(this, translated.Value, TetrisGame.KeyInputSource.Input_HID);
                GameKeyDown(translated.Value);
            }
        }

        private GameState.GameKeys? TranslateKey(Keys source)
        {
            if (_Game != null)
            {
                return _Game.TranslateKey(source);
            }

            return null;
        }

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

        private GameState.GameKeys? TranslateKey(X.Gamepad.GamepadButtons Button)
        {
            if (ControllerKeyLookup.ContainsKey(Button))
                return ControllerKeyLookup[Button];
            return null;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GameThread != null)
                GameThread.Abort();
            if (InputThread != null)
            {
                InputThread.Abort();
            }

            if (X.IsAvailable)
            {
                X.StopPolling();
            }
            
            if (ai != null) ai.AbortAI();
            TetrisGame.Soundman.StopMusic();
            Application.Exit();
        }

        public GameState CurrentState
        {
            get { return _Game?.CurrentState; }
            set { _Game.CurrentState = value; }
        }

        public void EnqueueAction(Action pAction)
        {
            ProcThreadActions.Enqueue(pAction);
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void picStatistics_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentState != null)
            {
                if (picStatistics.Visible == false) return;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                CurrentState.DrawStats(this, e.Graphics, picStatistics.ClientRectangle);
            }
        }

        private void BASeTris_KeyUp(object sender, KeyEventArgs e)
        {
            Debug.Print("Button released:" + e.KeyCode);
            var translated = TranslateKey(e.KeyCode);
            if (translated != null)
            {
                GameKeyUp(translated.Value);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(0.5f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetScale(1);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetScale(1.3f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SetScale(1.6f);
            ((ToolStripMenuItem) sender).Checked = true;
        }

        private void BASeTris_ResizeEnd(object sender, EventArgs e)
        {
        }

        private void BASeTris_SizeChanged(object sender, EventArgs e)
        {
            picTetrisField.Width = (int) (picTetrisField.Height * (332f / 641f));
            int statright = picStatistics.Right;
            picStatistics.Left = picTetrisField.Right + 6;
            picStatistics.Width = statright - picStatistics.Left;
        }

        private void aIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aIToolStripMenuItem.Checked = !aIToolStripMenuItem.Checked;
            if (ai == null)
            {
                ai = new TetrisAI(this);
            }
            else
            {
                ai.AbortAI();
                ai = null;
            }
        }

        private void BASeTris_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_Game != null && _Game.CurrentState is IDirectKeyboardInputState)
            {
                var Casted = (IDirectKeyboardInputState) _Game.CurrentState;
                Casted.KeyPressed(this, (Keys) e.KeyChar);
            }
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void picTetrisField_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Right)
            {
                ContextMenuStrip cms = new ContextMenuStrip();
                for(int i=0;i<10;i++)
                {
                    ToolStripMenuItem tsm = new ToolStripMenuItem("Item " + i);
                    cms.Items.Add(tsm);
                }
                cms.Show();
            }
        }
    }
}