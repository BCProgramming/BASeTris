using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BASeTris
{
    public class DASRepeatHandler
    {
        Dictionary<GameState.GameKeys, KeyRepeatInformation> KeyData = new Dictionary<GameState.GameKeys, KeyRepeatInformation>();
        Action<GameState.GameKeys> _FireKey;
        List<GameState.GameKeys> RepeatKeys = new List<GameState.GameKeys>() {GameState.GameKeys.GameKey_Down, GameState.GameKeys.GameKey_Left, GameState.GameKeys.GameKey_Right};

        Thread DASRepeatThread = null;

        private void DASThread()
        {
            //we exit out of here if no keys are pressed.
            while (KeyData.Any((f) => f.Value.IsPressed()))
            {
                lock (KeyData)
                {
                    DateTime CurrTime = DateTime.Now;
                    foreach (var iterate in KeyData)
                    {
                        if (iterate.Value.IsPressed())
                        {
                            Debug.Print("Key " + iterate.Value + " Is Pressed.");
                            //if the currenttime minus the interval is larger than the delay time...
                            if ((CurrTime - iterate.Value.LastRepeatTime).Ticks > iterate.Value.RepeatTicks)
                            {
                                //fire a repeat.
                                _FireKey?.Invoke(iterate.Key);
                                iterate.Value.LastRepeatTime = DateTime.Now;
                            }
                            else if ((CurrTime - iterate.Value.LastKeyDown).Ticks > iterate.Value.InitialRepeatDelayTicks) //otherwise, if the current time minus the last pressed is larger than the repeat time...
                            {
                                _FireKey?.Invoke(iterate.Key);
                                iterate.Value.LastRepeatTime = DateTime.Now;
                            }
                        }
                    }
                }

                Thread.Sleep(50);
            }

            DASRepeatThread = null;
        }

        public DASRepeatHandler(Action<GameState.GameKeys> FireKey)
        {
            _FireKey = FireKey;
            foreach (var iterate in Enum.GetValues(typeof(GameState.GameKeys)))
            {
                KeyData.Add((GameState.GameKeys) iterate, new KeyRepeatInformation((GameState.GameKeys) iterate));
            }
        }

        public void GameKeyDown(GameState.GameKeys Key)
        {
            Debug.Print("keyDown in DAS Handler:" + Key.ToString() + new StackTrace().ToString());
            if (!RepeatKeys.Contains(Key)) return;
            KeyData[Key].LastKeyDown = DateTime.Now;
            if (DASRepeatThread == null)
            {
                DASRepeatThread = new Thread(DASThread);
                DASRepeatThread.Start();
            }
        }

        public void GameKeyUp(GameState.GameKeys Key)
        {
            Debug.Print("keyUp in DAS Handler:" + Key.ToString() + new StackTrace().ToString());
            if (!RepeatKeys.Contains(Key)) return;
            KeyData[Key].LastKeyUp = DateTime.Now;
            KeyData[Key].LastRepeatTime = DateTime.MaxValue;
        }

        public class KeyRepeatInformation
        {
            public GameState.GameKeys Key;
            public DateTime LastKeyDown = DateTime.MinValue;
            public DateTime LastKeyUp = DateTime.MinValue;
            public DateTime LastRepeatTime = DateTime.MaxValue;
            public long InitialRepeatDelayTicks = TimeSpan.TicksPerSecond / 3;
            public long RepeatTicks = TimeSpan.TicksPerSecond / 10;
            public bool IsPressed()
            {
                return LastKeyDown > LastKeyUp;
            }

            public KeyRepeatInformation(GameState.GameKeys pkey)
            {
                Key = pkey;
            }
        }
    }
}