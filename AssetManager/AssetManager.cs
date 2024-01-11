/*
 * BASeCamp BASeBlock
Copyright (c) 2011, Michael Burgwin
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    Neither the name of BASeCamp Corporation nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.BASeCamp;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Ionic.Zip;
using Ionic.Zlib;
using OpenTK.Input;
using SkiaSharp;
using XInput.Wrapper;
using static BASeTris.AssetManager.cNewSoundManager;

namespace BASeTris.AssetManager
{
    public class ControllerImageDataInformation
    {
        public String ControllerName { get; set; }
        public Dictionary<X.Gamepad.GamepadButtons, String> ButtonImageKeys = null;
        public String DiagramImageKey { get; private set; }
        public String SimpleDiagramImageKey { get; private set; }

        X.Gamepad.GamepadButtons[] XButtonOrder = new X.Gamepad.GamepadButtons[]

            {
                X.Gamepad.GamepadButtons.Y,
X.Gamepad.GamepadButtons.X,
X.Gamepad.GamepadButtons.A,
X.Gamepad.GamepadButtons.B,
X.Gamepad.GamepadButtons.Back,
X.Gamepad.GamepadButtons.Start,
X.Gamepad.GamepadButtons.RBumper,
X.Gamepad.GamepadButtons.LBumper,
X.Gamepad.GamepadButtons.Dpad_Up,
X.Gamepad.GamepadButtons.Dpad_Down,
X.Gamepad.GamepadButtons.Dpad_Right,
X.Gamepad.GamepadButtons.Dpad_Left,
X.Gamepad.GamepadButtons.LeftStick,
X.Gamepad.GamepadButtons.RightStick
            };
        public ControllerImageDataInformation(String pName, String DiagramKey, String SimpleDiagramKey, params String[] OrderedButtonKeys)
        {
            this.ControllerName = pName;

            this.DiagramImageKey = DiagramKey;
            this.SimpleDiagramImageKey = SimpleDiagramImageKey;

            ButtonImageKeys = new Dictionary<X.Gamepad.GamepadButtons, string>();
            for (int buttonindex = 0; buttonindex < OrderedButtonKeys.Length; buttonindex++)
            {
                ButtonImageKeys.Add(XButtonOrder[buttonindex], OrderedButtonKeys[buttonindex]);


            }

        }
        public ControllerImageDataInformation(String pName, String DiagramKey, String SimpleDiagramKey, Dictionary<X.Gamepad.GamepadButtons, String> ImageKeyLookup)
        {

            this.ControllerName = pName;
            this.ButtonImageKeys = ImageKeyLookup;
            this.DiagramImageKey = DiagramKey;
            this.SimpleDiagramImageKey = SimpleDiagramImageKey;

        }

    }
    public static class AssetHelper
    {
        static String[] Buttonset = new string[]{
            "XboxSeriesX_Y",
"XboxSeriesX_X",
"XboxSeriesX_A",
"XboxSeriesX_B",
"XboxSeriesX_Windows",
"XboxSeriesX_Menu",
"XBoxSeriesX_RB",
"XBoxSeriesX_LB",
"XBoxSeriesX_Dpad_Up",
"XBoxSeriesX_Dpad_Down",
"XBoxSeriesX_Dpad_Right",
"XBoxSeriesX_Dpad_Left",
"XBoxSeriesX_Left_Stick_Click",
"XBoxSeriesX_Right_Stick_Click"
        };

        public static ControllerImageDataInformation XBoxSeriesXImageKeyData = new ControllerImageDataInformation("XBox Series X", "XboxSeriesX_Diagram", "XboxSeriesX_Diagram_Simple",
           Buttonset);

        public static ControllerImageDataInformation XBoxOneImageKeydata = new ControllerImageDataInformation("XBox One", "XBoxOne_Diagram", "XBoxOne_Diagram_Simple", (from b in Buttonset select b.Replace("XBoxSeriesX", "XBoxOne")).ToArray());

        public static ControllerImageDataInformation XBox360ImageKeydata = new ControllerImageDataInformation("XBox 360", "xbox_360_diagram", null, (from b in Buttonset select b.Replace("XBoxSeriesX", "360")).ToArray());


        public static Dictionary<String, ControllerImageDataInformation> ControllerImageData = new Dictionary<string, ControllerImageDataInformation>()
        {
        {"XBox Series X",XBoxSeriesXImageKeyData },
            {"XBox One", XBoxOneImageKeydata },
            {"XBox 360",XBox360ImageKeydata }
        };
        private enum KeyboardKeyType
        {
            Normal,
            Wide,
            Tall,
            Super_Wide
        }
        private static OpenTK.Input.Key[] SuperWide = new OpenTK.Input.Key[] { };
        private static OpenTK.Input.Key[] Wide = new[] { OpenTK.Input.Key.ShiftLeft, OpenTK.Input.Key.ShiftRight, OpenTK.Input.Key.Enter };
        private static OpenTK.Input.Key[] Tall = new[] { OpenTK.Input.Key.KeypadPlus, OpenTK.Input.Key.KeypadEnter };
        private static KeyboardKeyType GetKeyboardKeyType(OpenTK.Input.Key k)
        {
            if (SuperWide.Contains(k)) return KeyboardKeyType.Super_Wide;
            else if (Wide.Contains(k)) return KeyboardKeyType.Wide;
            else if (Tall.Contains(k)) return KeyboardKeyType.Tall;
            return KeyboardKeyType.Normal;

        }
        public static String[] AllControllerTypes = new string[] { "XBox Series X", "XBox One", "XBox 360" };

        private static Dictionary<OpenTK.Input.Key, String> ImageKeyKeyPrefixes = new Dictionary<Key, string>()
        {
            { Key.AltLeft,"Alt_Key" },
            { Key.AltRight,"Alt_Key" },
            { Key.Down,"Arrow_Down" },
            { Key.Up,"Arrow_Up" },
            { Key.Left,"Arrow_Left" },
            { Key.Right,"Arrow_Right" },
            { Key.KeypadMultiply,"Asterisk" },
            {Key.BracketLeft,"Bracket_Left" },
            {Key.BracketRight,"Bracket_Right" },
            {Key.CapsLock,"Caps_Lock" },
            {Key.Escape,"Esc" }


        };


        private static Dictionary<OpenTK.Input.Key, String> KeyboardKeyDisplaytext = new Dictionary<Key, string>()
        {
            
            { Key.AltLeft,"Alt" },
            { Key.AltRight,"Alt" },
            { Key.Down,"↓" },
            { Key.Up,"↑" },
            { Key.Left,"←" },
            { Key.Right,"→" },
            { Key.KeypadMultiply,"*" },
            {Key.BracketLeft,"[" },
            {Key.BracketRight,"]" },
            {Key.CapsLock,"Caps Lock" },
            {Key.ShiftLeft,"Shift" },
            {Key.ShiftRight,"Shift" },
            {Key.ControlLeft,"Ctrl" },
            {Key.ControlRight,"Ctrl" },
            {Key.KeypadDivide,"/" },
            {Key.Grave,"`" },
            {Key.Escape,"Esc" },
            {Key.PageUp,"PgUp" },
            {Key.PageDown,"PgDn" }

        };

        private static SKBitmap DrawKeyboardKey(OpenTK.Input.Key k)
        {
            String sDrawText = null;
            if (KeyboardKeyDisplaytext.ContainsKey(k)) sDrawText = KeyboardKeyDisplaytext[k];
            else sDrawText = Enum.GetName(typeof(Key), k);
            var keytype = GetKeyboardKeyType(k);
            String sBlankKey = "Blank_Black_" + keytype.ToString();
            //retrieve the key blank SKBitmap.
            SKBitmap BlankKey = TetrisGame.Imageman.GetSKBitmap(sBlankKey);
            SKPaint DrawKeyPaint = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Arial"),TextSize=BlankKey.Height*.66f, Color = SKColors.White };
            SKBitmap GenerateKeyBitmap = new SKBitmap(BlankKey.Width, BlankKey.Height);
            using (SKCanvas skc = new SKCanvas(GenerateKeyBitmap))
            {
                
                
                //draw the blank
                skc.DrawBitmap(BlankKey, new SKPoint(0, 0));
                SKRect sbound = new SKRect();
                DrawKeyPaint.MeasureText(sDrawText, ref sbound);
                while (sbound.Width > BlankKey.Width*.5f || sbound.Height > BlankKey.Height*.5f)
                {
                    DrawKeyPaint.TextSize -= 2;
                    DrawKeyPaint.MeasureText(sDrawText, ref sbound);
                }

                skc.DrawText(sDrawText, BlankKey.Width / 2 - sbound.Width / 2, BlankKey.Height / 2 + sbound.Height / 2, DrawKeyPaint);

                //now draw the key.

            }
            return GenerateKeyBitmap;



        }
        public static SKBitmap GetSKBitmapForKeyboardKey(OpenTK.Input.Key k)
        {
            if (k == OpenTK.Input.Key.Escape)
            {
                ;
            }
            String sGenKey = ImageKeyForKeyboardKey(k);
            if (!TetrisGame.Imageman.HasSKBitmap(sGenKey))
            {
                //we need to generate a keyboard key.
                SKBitmap GeneratedKey = DrawKeyboardKey(k);
                TetrisGame.Imageman.AddImage(sGenKey, SkiaSharp.Views.Desktop.Extensions.ToBitmap(GeneratedKey));

            }

            return TetrisGame.Imageman.GetSKBitmap(sGenKey);


        }
        public static String ImageKeyForKeyboardKey(OpenTK.Input.Key k)
        {
            String sPrefix = null;
            if (ImageKeyKeyPrefixes.ContainsKey(k))
                sPrefix = ImageKeyKeyPrefixes[k];
            else
            {
                sPrefix = Enum.GetName(typeof(OpenTK.Input.Key), k);
            }

            String sFindKey = sPrefix + "_Key_Dark";

            return sFindKey;
        }

public static String ImageKeyForControllerButton(X.Gamepad.GamepadButtons button, ControllerImageDataInformation Source)
        {
            if (Source.ButtonImageKeys.ContainsKey(button))
            {
                return Source.ButtonImageKeys[button];
            }

            return null;
        }

        public static String ImageKeyForXBoxControllerButton(X.Gamepad.GamepadButtons button)
        {
            return ImageKeyForControllerButton(button, XBoxSeriesXImageKeyData);
        }
        public static IEnumerable<(String, StreamReader)> GetZipContents(String sZipPath, Predicate<String> FileNameFilterFunc)
        {

            ZipFile zf = ZipFile.Read(sZipPath);
            foreach (ZipEntry ze in zf.Entries)
            {
                if (ze.IsDirectory)
                {
                    //skip directories
                }
                else 
                {
                    if (FileNameFilterFunc(ze.FileName))
                    {
                        //we could use a using block here, but that would require that the stream be dealt with before the caller retrieves the next result of the enumeration, which could
                        //cause problems for things like parallelization. Instead, the caller will be responsible for closing any streams it gets back after it is done with them.
                        StreamReader sr = new StreamReader(ze.InputStream);
                        yield return (ze.FileName, sr);

                    }
                }

            }
        }


    }

    /// <summary>

    #region Sound Manager

    /// <summary>
    /// represents and manipulates a sound object that is in use/playing.
    /// </summary>

    
    public class SilentSoundManager : IAudioHandler
    {
        private IAudioHandler _Composite = null;
        public SilentSoundManager(IAudioHandler pComposite)
        {
            _Composite = pComposite;
        }
        public iSoundSourceObject GetPlayingMusic()
        {
            return null;
            //return _Composite.GetPlayingMusic();
        }

        public iActiveSoundObject GetPlayingMusic_Active()
        {
            return null;
            //return _Composite.GetPlayingMusic_Active();
        }

        public void PauseMusic()
        {
            _Composite.PauseMusic(); 
        }

        public void PauseMusic(bool pausestate)
        {
            _Composite.PauseMusic();
        }

        public iActiveSoundObject PlayMusic()
        {
            //we never play music
            return null;
        }

        public iActiveSoundObject PlayMusic(string key, float volume, bool loop)
        {
            return null;
        }

        public iActiveSoundObject PlayMusic(string[] key, MultiMusicPlayMode mplaymode)
        {
            return null;
        }

        public iActiveSoundObject PlayMusic(string[] key, MultiMusicPlayMode mplaymode, out iSoundSourceObject[] ssources)
        {
            ssources = null;
            return null;
        }

        public iActiveSoundObject PlaySound(string key, bool playlooped)
        {
            return null;
        }

        public iActiveSoundObject PlaySound(string key)
        {
            return null;
        }

        public iActiveSoundObject PlaySound(string key, float volume)
        {
            return null;
        }

        public iActiveSoundObject PlaySoundRnd(string key, float Volume)
        {
            return null;
        }
        public iActiveSoundObject PlaySound(String key, bool playlooped, float volume)
        {
            return null;
        }
        public iActiveSoundObject PlaySound(String key, AudioHandlerPlayDetails pDetails)
        {
            return null;
        }
        public iActiveSoundObject PlayMusic(String key, AudioHandlerPlayDetails pDetails)
        {
            return null;
        }
        public void StopMusic()
        {
            ;
        }
    }

    public class cNewSoundManager : IDisposable,IAudioHandler
    {
        // public event Action<iSoundSourceObject, String> SoundStopped;
        private iSoundEngineDriver mDriver;

        public iSoundEngineDriver Driver
        {
            get { return mDriver; }
        }

        private Dictionary<String, iSoundSourceObject> mSoundSources = new Dictionary<string, iSoundSourceObject>();
        private List<iActiveSoundObject> PlayingSounds = new List<iActiveSoundObject>();
        private iActiveSoundObject _mPlayingMusic;


        private static iManagerCallback _callback = new Nullcallback();

        public static iManagerCallback Callback
        {
            get { return _callback; }
            set { _callback = value; }
        }

        //private iActiveSoundObject mPlayingMusic { set { _mPlayingMusic = value; } get { return _mPlayingMusic; } }

        private iActiveSoundObject mPlayingMusic
        {
            set
            {
                if (mPlayingMusic != null)
                {
                    Debug.Print
                    ("mPlayingMusic: " + getKeyForSound(mPlayingMusic.Source) + "\n" +
                     "Stacktrace:" + new StackTrace().ToString());
                }

                _mPlayingMusic = value;
            }
            get { return _mPlayingMusic; }
        }

        protected String getKeyForSound(iSoundSourceObject sourceobject)
        {
            foreach (String key in mSoundSources.Keys)
            {
                if (mSoundSources[key] == sourceobject)
                {
                    return key;
                }
            }

            return "";
        }

        private iSoundSourceObject mPlayingMusicSource;
        public String scurrentPlayingMusic = "";

        private iManagerCallback mCallback = new Nullcallback();
        /* public void FireSoundStopped(iSoundSourceObject Soundobj,String SoundName)
         {
             var copied = SoundStopped;
             if (copied != null)
                 copied.Invoke(Soundobj, SoundName);

         }
         */

        public Dictionary<String, iSoundSourceObject> SoundSources
        {
            get { return mSoundSources; }
        }

        protected cNewSoundManager(iSoundEngineDriver sounddriver)
            : this(sounddriver, new Nullcallback())
        {
        }

        public void Dispose()
        {
            //mDriver.Dispose();
            //mDriver=null;
        }

        public iSoundSourceObject GetPlayingMusic()
        {
            return mPlayingMusicSource;
        }

        public void SetPlayingMusic(iActiveSoundObject revertmusic)
        {
            mPlayingMusicSource = revertmusic as iSoundSourceObject;
        }

        public iActiveSoundObject GetPlayingMusic_Active()
        {
            return mPlayingMusic;
        }

        protected cNewSoundManager(iSoundEngineDriver sounddriver, iManagerCallback mancallback)
        {
            mDriver = sounddriver;
            mCallback = mancallback;
            mCallback.ShowMessage("SoundManager: using sound driver for " + sounddriver.Name);
            mDriver.OnSoundStop += new OnSoundStopDelegate(mDriver_OnSoundStop);
            mDriver.OnSoundPlay += new OnSoundPlayDelegate(mDriver_OnSoundPlay);
        }
        //manages the queued sounds when a list of sounds is to be played.

        /// <summary>
        /// QueuedSoundManager: implements iActiveSoundObject and iSoundSourceObject, and wraps a queue of the driver-provided implementations
        /// of those types. Hooks into the driver classes soundstopped event, and plays the next in the queue.
        /// </summary>
        public class QueuedSoundManager : iActiveSoundObject, iSoundSourceObject
        {
            private Queue<iSoundSourceObject> mSoundQueue = new Queue<iSoundSourceObject>();
            private MultiMusicPlayMode mmpmode = MultiMusicPlayMode.MultiMusic_Random;
            private bool mPlayLooped = false;
            private iActiveSoundObject PlayingSound;
            private iSoundSourceObject PlayingSource;

            public Queue<iSoundSourceObject> SoundQueue
            {
                get { return mSoundQueue; }
                set { mSoundQueue = value; }
            }

            public MultiMusicPlayMode PlayMode
            {
                get { return mmpmode; }
                set { mmpmode = value; }
            }

            public bool PlayLooped
            {
                get { return mPlayLooped; }
                set { mPlayLooped = value; }
            }

            public float Progress
            {
                get { return PlayingSound.Progress; }
            }

            public float Level
            {
                get { return PlayingSound.Level; }
            }

            private float _useTempo = -1;
            private float _usePitch = int.MinValue;
            public iSoundSourceObject Source
            {
                get { return this; }
            }

            public float Tempo
            {
                set
                {
                    _useTempo = value;
                    if (PlayingSound != null)
                        PlayingSound.Tempo = _useTempo;
                }

                get
                {
                    if (PlayingSound != null && PlayingSound.Tempo != _useTempo)
                        _useTempo = PlayingSound.Tempo;

                    return _useTempo;
                }
            }
            public float Pitch
            {
                set
                {
                    _usePitch = value;
                    if (PlayingSound != null) 
                        PlayingSound.Pitch = _usePitch;
                }
                get
                {
                    if (PlayingSound != null && PlayingSound.Pitch != _usePitch)
                        _usePitch = PlayingSound.Pitch;

                    return _usePitch;
                }
            }

            private iSoundEngineDriver driverobj = null;


            public QueuedSoundManager(cNewSoundManager csound, IEnumerable<string> keys, iSoundEngineDriver sdriver, bool pplaylooped)
            {
                driverobj = sdriver;
                SoundQueue = new Queue<iSoundSourceObject>();
                PlayLooped = pplaylooped;
                foreach (String loadkey in keys)
                {
                    try
                    {
                        iSoundSourceObject sourceobj = csound.GetSoundRnd(loadkey);
                        if (sourceobj != null)
                        {
                            if (PlayingSource == null)
                            {
                                PlayingSource = sourceobj;
                            }
                            else
                            {
                                SoundQueue.Enqueue(sourceobj);
                                //ss.Add(sourceobj);
                            }
                        }
                    }
                    catch
                    {
                    }
                }


                sdriver.OnSoundStop += new OnSoundStopDelegate(sdriver_OnSoundStop);
            }

            public float getLength()
            {
                float returnvalue = 0;
                foreach (var loopitem in SoundQueue)
                {
                    returnvalue += loopitem.getLength();
                }

                return returnvalue;
            }

            public void Pause()
            {
                if (PlayingSound != null)
                {
                    //  if (PlayingSound.Paused) PlayingSound.UnPause(); else PlayingSound.Pause();
                    Paused = !Paused;
                }
            }

            public void Skip()
            {
                //skips to the next track.
                //if we stop the current sound, the stop event will fire and play the next one.
                //PlayingSound.Stop(); 
                PlayingSound.Stop();
                sdriver_OnSoundStop(PlayingSound);
            }

            public iActiveSoundObject Play(bool pPlayLooped)
            {
                //PlayLooped = pPlayLooped;
                return Play(pPlayLooped, 1.0f);
            }

            public void Stop()
            {
                //unhook first, otherwise the event fires and we play another sound right away (depending on the mode)
                if (driverobj != null)
                {
                    driverobj.OnSoundStop -= sdriver_OnSoundStop;
                    driverobj = null;
                }

                if (PlayingSound != null) PlayingSound.Stop();
                //PlayingSource=null;
                playedqueue = new Queue<iSoundSourceObject>();
            }

            Queue<iSoundSourceObject> playedqueue = new Queue<iSoundSourceObject>();

            void sdriver_OnSoundStop(iActiveSoundObject objstop)
            {
                String stoppedkey = TetrisGame.Soundman.getKeyForSound(objstop.Source);
                Debug.Print("QueuedSoundManager detected stop of " + stoppedkey);

                if (TetrisGame.Soundman.mPlayingMusicSource != this) return;
                //PlayingSound = PlayingSource.Play(false);
                if (objstop == PlayingSound)
                {
                    Debug.Print("Stop is QSM's currently playing sound.");
                    playedqueue.Enqueue(PlayingSource);
                    //if it is the sound we played

                    if (SoundQueue.Count == 0)
                    {
                        //if the sound queue is empty....
                        Debug.Print("QueuedSoundManager Sound queue is empty...");
                        //if we are set to loop...
                        if (PlayLooped)
                        {
                            //drain playedqueue back into the main queue.
                            while (playedqueue.Any())
                                SoundQueue.Enqueue(playedqueue.Dequeue());
                        }
                        else
                        {
                            //if we aren't set to loop, we're done.
                            driverobj.OnSoundStop -= sdriver_OnSoundStop;
                        }
                    }


                    //if the queue has elements...
                    if (SoundQueue.Any())
                    {
                        iSoundSourceObject playit = null;
                        //pop the top one off. Set PlayingSound.
                        if (mmpmode == MultiMusicPlayMode.MultiMusic_Order)
                        {
                            playit = SoundQueue.Dequeue();
                        }
                        else
                        {
                            var Listeq = SoundQueue.ToList();

                            //select a random element
                            int randomindex = TetrisGame.StatelessRandomizer.Next(0, Listeq.Count);
                            Debug.Print("Chosen item index:" + randomindex);
                            playit = Listeq[randomindex];
                            //remove the selected element...
                            Listeq.RemoveAt(randomindex);
                            //create a new queue from the list...
                            SoundQueue = new Queue<iSoundSourceObject>(Listeq);
                        }

                        String getname = TetrisGame.Soundman.getKeyForSound(playit);
                        Debug.Print("QueuedSoundManager about to play " + getname + ".");
                        PlayingSource = playit;
                        PlayingSound = PlayingSource.Play(false);
                        if (_useTempo > 0) PlayingSound.Tempo = _useTempo;
                        if(_usePitch != int.MinValue) PlayingSound.Pitch = _usePitch;
                    }
                }
            }

            void sdriver_OnSoundStopOld(iActiveSoundObject objstop)
            {
                //throw new NotImplementedException();
                if (objstop == PlayingSound)
                {
                    Debug.Print("queuedSoundManager detected OnSoundStop:" + DateTime.Now);

                    //are we out of sounds?

                    if (SoundQueue.Count == 0)
                    {
                        Debug.Print("QueuedSoundManager's SoundQueue is empty. Playedqueue has " + playedqueue.Count.ToString() + " Items");
                        //remove the hook...
                        if (mmpmode == MultiMusicPlayMode.MultiMusic_Random && PlayLooped)
                        {
                            //"drain" playedqueue back into the main queue.
                            while (playedqueue.Any())
                                SoundQueue.Enqueue(playedqueue.Dequeue());

                            PlayingSound = PlayingSource.Play(false);
                        }

                        driverobj.OnSoundStop -= sdriver_OnSoundStop;
                        driverobj = null;
                    }
                    else
                    {
                        objstop.Stop();
                        //sounds left; dequeue the next one...
                        if (mmpmode == MultiMusicPlayMode.MultiMusic_Order)
                        {
                            var deq = SoundQueue.Dequeue();

                            //play it...
                            PlayingSource = deq;
                            Play(false);
                            //and if we are supposed to loop, toss it to the end of the queue as well.
                            if (PlayLooped)
                                SoundQueue.Enqueue(deq);
                        }
                        else if (mmpmode == MultiMusicPlayMode.MultiMusic_Random)
                        {
                            //select a random element from the queue.
                            //play it, remove it from the queue.
                            //add it to the playedqueue.

                            //first create a list based on the queue so we have better granularity.
                            var Listeq = SoundQueue.ToList();

                            //select a random element
                            int randomindex = TetrisGame.StatelessRandomizer.Next(0, Listeq.Count);
                            Debug.Print("Chosen item index:" + randomindex);
                            var grabbeditem = Listeq[randomindex];
                            //remove the selected element...
                            Listeq.RemoveAt(randomindex);
                            //create a new queue from the list...
                            SoundQueue = new Queue<iSoundSourceObject>(Listeq);
                            playedqueue.Enqueue(grabbeditem);

                            PlayingSource = grabbeditem;
                            PlayingSound = grabbeditem.Play(false);
                            //play the grabbed item.
                            ///Play(false);
                        }
                    }
                }
            }


            #region iActiveSoundObject Members

            public bool Finished
            {
                get { return driverobj == null; }
            }

            public void UnPause()
            {
                Paused = false;
            }

            public bool Paused
            {
                get { return (PlayingSound != null && PlayingSound.Paused); }
                set
                {
                    Debug.Print("Setting PlayingSound Pause state to " + value);
                    if (PlayingSound != null) PlayingSound.Paused = value;
                }
            }

            public void setVolume(float volumeset)
            {
                PlayingSound.setVolume(volumeset);
            }

            #endregion

            #region iSoundSourceObject Members

            #endregion

            #region iActiveSoundObject Members

            bool iActiveSoundObject.Finished
            {
                get { return driverobj != null; }
            }

            void iActiveSoundObject.Stop()
            {
                this.Stop();
            }

            void iActiveSoundObject.Pause()
            {
                Pause();
            }

            void iActiveSoundObject.UnPause()
            {
                UnPause();
            }

            bool iActiveSoundObject.Paused
            {
                get { return PlayingSound.Paused; }
                set { PlayingSound.Paused = value; }
            }

            #endregion

            #region iSoundSourceObject Members

            public iActiveSoundObject Play(bool playlooped, float volume, float tempo = 1f, float pitch = 0f)
            {
                //PlayLooped = pPlayLooped;
                if (PlayingSound != null && PlayingSound.Paused)
                {
                    PlayingSound.UnPause();
                }
                else
                {
                    PlayingSound = PlayingSource.Play(false, volume,tempo,pitch); //the looping argument is ignored...
                    //if (_useTempo > 0) PlayingSound.Tempo = _useTempo;
                    //if (_usePitch != int.MinValue) PlayingSound.Pitch = _usePitch;
                }

                return this;
            }

            #endregion
        }


        //private Queue<iSoundSourceObject> QueuedMusic = new Queue<iSoundSourceObject>();


        //sounds can be queued, just like music :D
        //private List<Queue<iSoundSourceObject>> QueuedSounds = new List<Queue<iSoundSourceObject>>();

        void mDriver_OnSoundPlay(iActiveSoundObject objplay)
        {
            PlayingSounds.Add(objplay);
        }

        //When the sound driver indicates a sound stopped, we need to do some special processing for the case
        //where we were told to play a series of sounds.
        void mDriver_OnSoundStop(iActiveSoundObject objstop)
        {
            /*
            iSoundSourceObject sourceobj = objstop.Source;
            String usekey="";
            if (sourceobj != null)
            {
                foreach (var iteratesound in mSoundSources)
                {
                    if (iteratesound.Value == sourceobj)
                    {
                        usekey = iteratesound.Key;
                        break;
                    }


                }
            }
            if (!String.IsNullOrEmpty(usekey) && sourceobj != null)
            {

                FireSoundStopped(sourceobj, usekey);


            }
            */
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, String[] SoundFilePaths)
            : this(sounddriver, SoundFilePaths, new Nullcallback())
        {
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, String[] SoundFilePaths, iManagerCallback mancallback)
            : this(sounddriver, mancallback)
        {
            mCallback = mancallback;
            LoadSounds(SoundFilePaths);
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, DirectoryInfo[] DirsUse)
            : this(sounddriver, DirsUse, new Nullcallback())
        {
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, DirectoryInfo[] DirsUse, iManagerCallback mancallback)
            : this(sounddriver, mancallback)
        {
            mCallback = mancallback;
            LoadSounds(DirsUse);
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, String SoundFilePath) : this(sounddriver, SoundFilePath, new Nullcallback())
        {
        }

        public cNewSoundManager(iSoundEngineDriver sounddriver, String SoundFilePath, iManagerCallback mancallback)
            : this(sounddriver, mancallback)
        {
            LoadSounds(SoundFilePath);
        }

        public bool IsSupportedSound(String testfile)
        {
            //return return supportedtypes.Contains(Path.GetExtension(filename).ToUpper());
            return mDriver.GetSupportedExtensions().Contains(Path.GetExtension(testfile).ToUpper()) ||
                   Path.GetExtension(testfile).ToUpper().Equals(".DFS");
        }

        public void LoadSounds(IEnumerable<DirectoryInfo> loadfromdirs)
        {
            foreach (DirectoryInfo loopdir in loadfromdirs)
            {
                LoadSounds(loopdir);
            }
        }

        /// <summary>
        /// returns whether all the sound keys present exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool HasSound(IEnumerable<string> key)
        {
            foreach (String loopstring in key)
            {
                if (!HasSound(loopstring)) return false;
            }

            return true;
        }

        public bool HasSound(String key)
        {
            if (key.Contains("|"))
            {
                return HasSound(key.Split('|'));
            }
            else
            {
                return mSoundSources.Keys.Contains(key.ToUpper());
            }
        }

        /// <summary>
        /// retrieves a array
        /// </summary>
        /// <param name="basename">Base name of all keys to find. Found items will be all sounds whose key is the basename followed by an number</param>
        /// <returns>List of appropriate sound keys</returns>
        public String[] getMultiSounds(String basename)
        {
            SortedList<String, String> acquiredkeys = new SortedList<String, String>();
            foreach (String iteratek in mSoundSources.Keys)
            {
                if (iteratek.StartsWith(basename, StringComparison.OrdinalIgnoreCase))
                {
                    //make sure the remainder of the name is a number.
                    String remainder = iteratek.Substring(basename.Length);
                    int resultint = 0;
                    if (Int32.TryParse(remainder, out resultint))
                    {
                        acquiredkeys.Add(remainder, iteratek);
                    }
                }
            }

            return (from p in acquiredkeys select p.Value).ToArray();
        }

        public iSoundSourceObject GetSound(String key)
        {
            if (key.Contains("|"))
            {
                QueuedSoundManager qsm = new QueuedSoundManager(this, key.Split('|'), mDriver, false);

                return qsm;
            }

            if (mSoundSources.ContainsKey(key.ToUpper()))
                return mSoundSources[key.ToUpper()];
            else if (mSoundSources.ContainsKey(key.ToUpper() + "1"))
            {
                return mSoundSources[getRandomSound(key.ToUpper())];
            }
            else
            {
                throw new KeyNotFoundException(key);
            }

            //return GetSoundRnd(key);
        }

        public String getRandomSound(String keyprefix)
        {
            keyprefix = keyprefix.Trim().Replace('|', ':');

            //special code: allow in form of sound1:sound2:sound3 to choose one of those three randomly.
            if (keyprefix.Contains(":"))
            {
                String[] splitopt = keyprefix.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
                //choose a random index.
                int randomidex = TetrisGame.StatelessRandomizer.Next(splitopt.Length);
                keyprefix = splitopt[randomidex];
            }


            String[] gotkeys = (from w in mSoundSources.Keys
                where w.ToUpper().StartsWith(keyprefix.ToUpper())
                select w).ToArray();
            if (gotkeys.Length == 0) return "";
            int randomindex = TetrisGame.StatelessRandomizer.Next(0, gotkeys.Length);
            return gotkeys[randomindex];
        }

        public iSoundSourceObject GetSoundRnd(String key)
        {
            //gets a random sound; for example, if key="TALLYMUSIC", and we have the following:
            //"TALLYMUSIC"
            //"TALLYMUSIC1"
            //"TALLYMUSIC2"
            //etc, it will choose a random one of these.

            //so.... iterate through all of our keys, get those keys that start with the passed key, and choose a random one from that set.


            return mSoundSources[getRandomSound(key)];
        }

        /// <summary>
        /// copies the input stream to the output stream in chunks of chunksize.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="chunksize"></param>
        public static void CopyStream(Stream input, Stream output, int chunksize)
        {
            if (!input.CanRead) throw new ArgumentException("input");
            if (!output.CanWrite) throw new ArgumentException("output");

            byte[] buffer = new byte[chunksize];
            bool continuereading = true;

            while (continuereading)
            {
                //read in a chunk...
                int amountread = input.Read(buffer, 0, buffer.Length);
                //write out the amount written to the other stream.
                if (amountread > 0)
                    output.Write(buffer, 0, amountread);

                //only continue as long as the returned amount is equal to the chunk size.
                continuereading = amountread == buffer.Length;
            }
        }

        public static String GetTempPath()
        {
            String tpath = Path.GetTempPath();
            tpath = Path.Combine(tpath, "BASeTris");
            if (!Directory.Exists(tpath)) Directory.CreateDirectory(tpath);
            return tpath;
        }

        public static String GetTempFile(String useextension)
        {
            String tpath = GetTempPath();
            if (!useextension.StartsWith(".")) useextension = "." + useextension;
            //GetTempPath(1023,tpath);
            tpath = tpath.Replace('\0', ' ').Trim();
            String destfilename = Guid.NewGuid().ToString() + useextension;
            return Path.Combine(tpath, destfilename);
        }

        //"preprocesses" the sound file. Certain extensions are recognized as "speshul" and
        //we extract that file to a temporary location as the proper, recognizable type.
        private void ProcessSoundFile(ref String filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");
            //currently recognized extensions: dfs (deflated Sound)
            // which is merely a WAV file that was run through gzipper using  DeflateStream.

            if (Path.GetExtension(filename).Equals(".dfs", StringComparison.OrdinalIgnoreCase))
            {
                //get a temporary file.
                String tempfilename = GetTempFile(".dfs");
                //change the extension to .WAV


                //open the 'input' file...
                using (FileStream indata = new FileStream(filename, FileMode.Open))
                {
                    //use a DeflateStream...
                    using (Stream doreadstream = new DeflateStream(indata, CompressionMode.Decompress))
                    {
                        //open the output...
                        using (Stream outputdata = new FileStream(tempfilename, FileMode.Create))
                        {
                            CopyStream(doreadstream, outputdata, 8192);
                        }
                    }

                    //change the filename to point to the decompressed wav data.
                    filename = tempfilename;
                    TetrisGame.QueueDelete(tempfilename); //queue up the temporary file to be cleaned up.
                }
            }
        }


        public String AddSound(byte[] sounddata, String key, String type)
        {
            //get a temporary file.
            key = key.ToUpper();
            String outfile = GetTempFile(type);
            //open it for output...
            FileInfo finfo = new FileInfo(outfile);
            Stream writeit = finfo.OpenWrite();
            //write out the sound data...
            writeit.Write(sounddata, 0, sounddata.Length);
            //close the file.
            writeit.Close();
            //now we can load "properly" (normally)...
            String returnkey = AddSound(outfile, key);
            return returnkey;
        }

        public String AddSound(String filename)
        {
            // mCallback.ShowMessage("Loading Sound:" + filename);

            iSoundSourceObject ss = mDriver.LoadSound(filename);
            string usekey = Path.GetFileNameWithoutExtension(filename).ToUpper();
            return AddSound(filename, usekey);
        }

        /// <summary>
        /// Adds a given Sound file with the specified key
        /// </summary>
        /// <param name="filename">Filename of the file to load</param>
        /// <param name="useKey">Key to use in the hashTable</param>
        /// <returns>The key that was used</returns>
        public String AddSound(String filename, String usekey)
        {
            ProcessSoundFile(ref filename);
            usekey = usekey.ToUpper();
            mCallback.ShowMessage("Loading Sound:" + filename + " as " + usekey);
            iSoundSourceObject ss = mDriver.LoadSound(filename);
            //string usekey = Path.GetFileNameWithoutExtension(filename).ToUpper();

            if (mSoundSources.ContainsKey(usekey))
            {
                //   mSoundSources.Remove(usekey);
                mSoundSources[usekey] = ss;
            }
            else
            {
                mSoundSources.Add(usekey, ss);
            }

            return usekey;
        }

        public iActiveSoundObject PlaySoundRnd(String key, float Volume)
        {
            iSoundSourceObject grabbed = GetSoundRnd(key);
            return grabbed.Play(false, Volume);
        }

        public iActiveSoundObject PlaySound(String key, bool playlooped)
        {
            iSoundSourceObject grabbed = GetSound(key);
            return grabbed.Play(playlooped);
        }
        public iActiveSoundObject PlaySound(String key,bool playlooped,float volume)
        {
            
                iSoundSourceObject grabbed = GetSound(key);
                return grabbed.Play(playlooped,volume);
            
        }
        public iActiveSoundObject PlaySound(String key, AudioHandlerPlayDetails pDetails)
        {
            iSoundSourceObject grabbed = GetSound(key);
            return grabbed.Play(false, pDetails.Volume, pDetails.Tempo, pDetails.Pitch);


        }
        //TODO: add PlaySound() that supports array if String[] for key.
        //will use QueuedSounds list.
        public iActiveSoundObject PlaySound(String key)
        {
            if (key == null) return null;
            iSoundSourceObject grabbed = GetSound(key);
            return grabbed.Play(false);
        }

        public iActiveSoundObject PlaySound(String key, float volume)
        {
            iSoundSourceObject grabbed = GetSound(key);
            
            var result = grabbed.Play(false,volume);
            return result;
        }

        private MultiMusicPlayMode MultipleMusicPlayMode;

        public enum MultiMusicPlayMode
        {
            /// <summary>
            /// Play all Items in order.
            /// </summary>
            MultiMusic_Order,

            //Shuffle
            MultiMusic_Random,
        }

        /// <summary>
        /// new, dubious feature: play multiple musics, in a loop.
        /// </summary>
        /// <param name="key">array of Sound keys to play.</param>
        /// <param name="mplaymode">Play mode of the set. </param>
        /// <returns>first sound being played.</returns>
        public iActiveSoundObject PlayMusic(String[] key, MultiMusicPlayMode mplaymode)
        {
            iSoundSourceObject[] outret;
            return PlayMusic(key, mplaymode, out outret);
        }

        public iActiveSoundObject PlayMusic(String[] key, MultiMusicPlayMode mplaymode, out iSoundSourceObject[] ssources)
        {
            //stop any playing sounds.
            //QueuedMusic = new Queue<iSoundSourceObject>();
            ssources = new iSoundSourceObject[key.Length];
            for (int i = 0; i < ssources.Length; i++)
            {
                ssources[i] = GetSound(key[i]);
            }

            iActiveSoundObject retobj = PlayMusic(String.Join("|", key), 1.0f, true);
            if (retobj is QueuedSoundManager)
            {
                (retobj as QueuedSoundManager).PlayMode = mplaymode;
            }

            return retobj;

            /*
            var musicqueue = new QueuedSoundManager(this, key, mDriver, true);
            ssources = musicqueue.SoundQueue.ToArray();
            musicqueue.PlayMode = mplaymode;
            mPlayingMusic=musicqueue;
            musicqueue.Play(false, 1.0f);
            return musicqueue;
            */
        }


        public iActiveSoundObject PlayMusic(String key)
        {
            return PlayMusic(key, 1.0f, true);
        }

        public iActiveSoundObject PlayMusic(String key, bool loop)
        {
            return PlayMusic(key, 0, loop);
        }

        private class ActiveMusicData
        {
            public string Name;
            public iActiveSoundObject ActiveSound { get; set; }
            public iSoundSourceObject Source { get; set; }

            public ActiveMusicData(String pName, iActiveSoundObject pActiveSound, iSoundSourceObject pSource)
            {
                ActiveSound = pActiveSound;
                Source = pSource;
                Name = pName;
            }
        }

        /// <summary>
        /// holds data for "temporary" music.
        /// this is a replacement for the pushMusic and PopMusic() functions which,
        /// rather foreseeably, did not work as intended.
        /// </summary>
        private class TemporaryMusicData : ActiveMusicData, IComparable<TemporaryMusicData>
        {
            public TemporaryMusicData(String pName, iActiveSoundObject pActiveSound, iSoundSourceObject pSource) : base(pName, pActiveSound, pSource)
            {
            }

            public int Occurences; //reference count; we add one to this when a "temporary" music is played. and subtract one when it is "stopped".
            //we play the music with the highest "reference count"; items are removed when their "reference count" is zero.


            public int CompareTo(TemporaryMusicData other)
            {
                return Occurences.CompareTo(other.Occurences);
            }
        }

        private String OriginalSoundName; //allocated when TemporaryMusicData is empty when music is pushed.

        private iSoundSourceObject OriginalSoundObject = null;

        //private SortedList<TemporaryMusicData, TemporaryMusicData> TempMusicData = new SortedList<TemporaryMusicData, TemporaryMusicData>();
        private Dictionary<String, TemporaryMusicData> TempMusicData = new Dictionary<string, TemporaryMusicData>();


        /// <summary>
        /// stops music played with the PlayTemporaryMusic function.
        /// </summary>
        /// <param name="MusicName"></param>
        public iActiveSoundObject StopTemporaryMusic(String MusicName)
        {
            MusicName = MusicName.ToUpper();
            if (!TempMusicData.Any()) return mPlayingMusic; //no elements in Temporary Music Dictionary, so nothing to stop.


            TemporaryMusicData incrementData = getTemporaryMusicData(MusicName);
            incrementData.Occurences--; //add one to occurences.

            if (incrementData.Occurences == 0) TempMusicData.Remove(MusicName);
            return PlayMax(1.0f, true);
        }

        /// <summary>
        /// Plays "temporary" music; for example the music from a power up.
        /// </summary>
        /// <param name="MusicName"></param>
        public iActiveSoundObject PlayTemporaryMusic(String MusicName, float volume, bool loop)
        {
            MusicName = MusicName.ToUpper();
            //the idea is simple, we want to allow for the following:
            //Player starts game. Music playing is standard level music or something.
            //player get's a power up, which has it's own music. this powerup is timed.
            //player causes something else to have different music; for example, they could spawn a boss or something.
            //when the powerup runs out, the music will revert to the normal game music; and when the boss is killed, it will go back to the
            //music that was playing from the powerup.


            //This attempts to mitigate this behaviour.

            //step one: if the list is empty...
            if (!TempMusicData.Any())
            {
                //empty list. Initialize OriginalSoundName...
                OriginalSoundObject = mPlayingMusicSource;
                OriginalSoundName = scurrentPlayingMusic;
            }

            //step two: is there an element in the sorted list for MusicName?
            TemporaryMusicData incrementData = getTemporaryMusicData(MusicName);
            incrementData.Occurences++; //add one to occurences.


            return PlayMax(volume, loop);
        }

        private TemporaryMusicData getTemporaryMusicData(string MusicName)
        {
            MusicName = MusicName.ToUpper();
            TemporaryMusicData incrementData;
            if (TempMusicData.ContainsKey(MusicName))
            {
                incrementData = TempMusicData[MusicName];
            }
            else
            {
                incrementData = new TemporaryMusicData(MusicName, null, GrabSound(MusicName));
                TempMusicData.Add(MusicName, incrementData);
            }

            return incrementData;
        }

        private iActiveSoundObject PlayMax(float volume, bool loop)
        {
            //now, find the one with the maximum occurences.

            //since there are no entries on the list, we need to revert.
            if (!TempMusicData.Any())
            {
                mPlayingMusic.Stop(); //pause the original music. If we stop it, sometimes this can fuddle up the internal structures.
                //play default.
                scurrentPlayingMusic = OriginalSoundName;
                mPlayingMusic = OriginalSoundObject.Play(loop, volume);

                return mPlayingMusic;
            }

            int currentmax = int.MinValue;
            TemporaryMusicData tmduse = null;
            foreach (var iterate in TempMusicData)
            {
                if (iterate.Value.Occurences > currentmax)
                {
                    currentmax = iterate.Value.Occurences;
                    tmduse = iterate.Value;
                }
            }

            if (scurrentPlayingMusic != tmduse.Name)
            {
                if (scurrentPlayingMusic == OriginalSoundName)
                    mPlayingMusic.Pause(); //pause the "original" music.
                else
                    mPlayingMusic.Stop();

                scurrentPlayingMusic = tmduse.Name;
                mPlayingMusic = tmduse.Source.Play(loop, volume);
                return mPlayingMusic;
            }

            return mPlayingMusic;
        }


        /*
        private Stack<ActiveMusicData> PushedMusic = new Stack<ActiveMusicData>();
        
        /// <summary>
        /// pauses any currently playing music and plays the specified music. To revert to the previous music
        /// use "PopMusic"
        /// </summary>
        /// <param name="key">key of sound to play</param>
        /// <param name="volume">volume to play at</param>
        /// <returns>the active sound of the started music</returns>
        public iActiveSoundObject PushMusic(String key, float volume,bool loop)
        {
            if (mPlayingMusic != null)
            {
                //pause it...
                mPlayingMusic.Pause();
                
            }
            if (!loop)
            {
                   



            }

            PushedMusic.Push(new ActiveMusicData(key,mPlayingMusic, mPlayingMusicSource));

            //acquire the "new" sound...
            var newsnd = PlayMusic(key, volume, loop);
            


            return newsnd;


        }

        public iActiveSoundObject PopMusic()
        {

            return PopMusic("");


        }
        /// <summary>
        /// inverse of PushMusic; stops the currently playing music, pops a Active music object
        /// from PushedMusic, and unpauses it. (the assumption being it was paused immediately before being pushed).
        /// </summary>
        /// <returns></returns>
        public iActiveSoundObject PopMusic(String pName)
        {
            if (String.IsNullOrEmpty(pName))
            {
                if (PushedMusic.Count > 0)
                {
                    mPlayingMusic.Stop();
                    var popped = PushedMusic.Pop();
                    //play it...
                    mPlayingMusic = popped.ActiveSound;
                    mPlayingMusicSource = popped.Source;
                    mPlayingMusic.Paused = false; //resume playback
                    return mPlayingMusic;
                }
            }
            else
            {
                PopMusic("");
                List<ActiveMusicData> tempconv = new List<ActiveMusicData>(PushedMusic);
                tempconv.RemoveAll((w) => w.Name == pName);
                PushedMusic = new Stack<ActiveMusicData>(tempconv);
                
            }
            return null;
            



        }
         * */
        public void SetMusicVolume(float amount)
        {
            Debug.Print("setting mPlayingMusic volume to " + amount);
            if (mPlayingMusic != null)
                mPlayingMusic.setVolume(amount);
        }
        public iActiveSoundObject PlayMusic(String key, float volume, bool loop)
        {
            return PlayMusic(key, new AudioHandlerPlayDetails() { Volume = volume, Playlooped = loop });
        }
        public iActiveSoundObject PlayMusic(String key, AudioHandlerPlayDetails pDetails)
        {
            Debug.Print(new StackTrace().ToString());
            if (mPlayingMusicSource is QueuedSoundManager)
            {
                ((QueuedSoundManager) mPlayingMusicSource).Stop();
                Debug.Print("queued");
            }

            iSoundSourceObject getsource = null;
            if (key.Contains("|"))
            {
                getsource = new QueuedSoundManager(this, key.Split('|'), mDriver, pDetails.Playlooped);
            }
            else
            {
                getsource = GrabSound(key);
            }

            if (mPlayingMusic != null)
            {
                if (getsource == mPlayingMusicSource && mPlayingMusic.Paused)
                    mPlayingMusic.UnPause();
                else
                    mPlayingMusic.Stop();
            }

            //Change: March 31st 2012:
            //to support "temporary" music. When the main tune is changed, we need to clear all the temporary
            //music data.

            TempMusicData = new Dictionary<string, TemporaryMusicData>();


            iActiveSoundObject soundobj = getsource.Play(pDetails.Playlooped, pDetails.Volume,pDetails.Tempo,pDetails.Pitch);
            mPlayingMusicSource = getsource;
            mPlayingMusic = soundobj;
            scurrentPlayingMusic = key;
            return soundobj;
        }

        private iSoundSourceObject GrabSound(string key)
        {
            iSoundSourceObject getsource;
            if (File.Exists(key))
            {
                getsource = mDriver.LoadSound(key);
            }
            else
            {
                getsource = GetSoundRnd(key);
            }

            return getsource;
        }

        public void Stop()
        {
            foreach (var iterate in PlayingSounds)
            {
                iterate.Stop();
            }
        }

        public iActiveSoundObject PlayMusic()
        {
            if (mPlayingMusic != null)
                mPlayingMusic.UnPause();

            return mPlayingMusic;
        }

        public void PauseMusic()
        {
            if (mPlayingMusic != null)
            {
                if (mPlayingMusic.Paused)
                    mPlayingMusic.UnPause();
                else
                {
                    mPlayingMusic.Pause();
                }
            }
        }

        public void PauseMusic(bool pausestate)
        {
            if (mPlayingMusic is QueuedSoundManager)
            {
                Debug.Print("Queued...");
            }

            if (mPlayingMusic != null)
            {
                if (pausestate)
                {
                    mPlayingMusic.Pause();
                }
                else
                {
                    mPlayingMusic.UnPause();
                }
            }
        }


        public void StopMusic()
        {
            // QueuedMusic = new Queue<iSoundSourceObject>();
            if (mPlayingMusic != null)
                mPlayingMusic.Stop();

            mPlayingMusic = null;
            scurrentPlayingMusic = "";
        }

        public void LoadSounds(String[] loadfromfolders)
        {
            DirectoryInfo[] makeinfo = new DirectoryInfo[loadfromfolders.Length];
            for (int i = 0; i < makeinfo.Length; i++)
            {
                try
                {
                    makeinfo[i] = new DirectoryInfo(loadfromfolders[i]);
                }
                catch
                {
                    //ignore exceptions...
                }
            }

            LoadSounds(makeinfo);
        }

        public void LoadSounds(DirectoryInfo loadfolder)
        {
            mCallback.ShowMessage("Loading sounds from:" + loadfolder.FullName);
            if (!loadfolder.Exists) return;
            foreach (FileInfo loopfile in loadfolder.GetFiles())
            {
                if (IsSupportedSound(loopfile.FullName))
                {
                    mCallback.ShowMessage("Loading Sound:" + loopfile.Name);
                    String usefilename = loopfile.FullName;
                    //ProcessSoundFile(ref usefilename);
                    iSoundSourceObject ss = mDriver.LoadSound(usefilename);
                    //use loopfile.Fullname for the key.
                    String usekey = Path.GetFileNameWithoutExtension(loopfile.FullName).ToUpper();
                    if (!mSoundSources.ContainsKey(usekey))
                    {
                        mCallback.ShowMessage($"Loaded sound file {loopfile.FullName} as key {usekey}.");
                        mSoundSources.Add(usekey, ss);
                    }
                }
            }
        }

        public void LoadSounds(String loadfolder)
        {
            LoadSounds(new DirectoryInfo(loadfolder));
        }


        public void RemoveSound(string key)
        {
            mSoundSources.Remove(key);
        }
    }

    #endregion


    public class ScriptObject : ISerializable
    {
        private String _Language = "C#";
        private String _Code = "";

        public String Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        public String Code
        {
            get { return _Code; }
            set { _Code = value; }
        }

        public ScriptObject(SerializationInfo info, StreamingContext context)
        {
            _Language = info.GetString("Language");
            _Code = info.GetString("Code");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Language", _Language);
            info.AddValue("Code", _Code);
        }
    }


    //a class for serialization of images in conjunction with the ImageManager class...
    [Serializable]
    public class SerializableImage : ISerializable
    {
        public static readonly String SerialImageModeName = "ImageSerializationMode";
        public static readonly String SerialImageDataName = "ImageSerialData";

        private enum ImageSerializationMode
        {
            Serialize_Null, //image was null for some reason.
            Serialize_String, //only the name, to be used to access the proper image in the imagemanager, was stored.

            Serialize_ImageData
            //the actual Image object was stored (because no match was found in the imagemanager, presumably)
        }

        public Image ActualImage;


        public SerializableImage(Image pActualImage)
        {
            ActualImage = pActualImage;
        }
        
        public SerializableImage(SerializationInfo information, StreamingContext context)
        {
            ImageSerializationMode modetype = (ImageSerializationMode) information.GetInt32(SerialImageModeName);

            ActualImage = modetype switch
            {
                ImageSerializationMode.Serialize_Null => null,
                ImageSerializationMode.Serialize_String => TetrisGame.Imageman.getLoadedImage(information.GetString(SerialImageDataName)),
                ImageSerializationMode.Serialize_ImageData => (Image)information.GetValue(SerialImageDataName, typeof(Image)),
                _ =>throw new ArgumentOutOfRangeException(nameof(modetype), $"unexpected modetype {modetype}")
                
            };
            ///////
            switch (modetype)
            {
                case ImageSerializationMode.Serialize_Null:
                    ActualImage = null;
                    break;
                case ImageSerializationMode.Serialize_String:
                    ActualImage = TetrisGame.Imageman.getLoadedImage(information.GetString(SerialImageDataName));
                    break;
                case ImageSerializationMode.Serialize_ImageData:
                    ActualImage = (Image) information.GetValue(SerialImageDataName, typeof(Image));
                    break;
            }
            
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //see if this image is "speshul"
            if (ActualImage == null)
            {
                //null... so, write... well, not much, really.
                info.AddValue(SerialImageModeName, (Int32) ImageSerializationMode.Serialize_Null);
            }
            else
            {
                String foundkey = TetrisGame.Imageman.FindImage(ActualImage);
                if (foundkey != "")
                {
                    info.AddValue(SerialImageModeName, (int) ImageSerializationMode.Serialize_String);
                    info.AddValue(SerialImageDataName, foundkey);
                }
                else
                {
                    info.AddValue(SerialImageModeName, (int) ImageSerializationMode.Serialize_ImageData);
                    info.AddValue(SerialImageDataName, ActualImage);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// manages a set of images, automatically loads them from the directory specified.
    /// </summary>
    public class ImageManager
    {
        public delegate Stream NameToStreamFunc(String filename, FileMode OpenMode);

        private iManagerCallback mcallback = new Nullcallback();
        public NameToStreamFunc NameToStream = null;
        private Dictionary<String, Image> loadedimages = new Dictionary<String, Image>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<String, Icon> loadedicons = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<String, MemoryStream> loadedIconStreams = new Dictionary<string, MemoryStream>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<String, SKBitmap> SkiaImages { get; set; } = new Dictionary<String, SKBitmap>(StringComparer.OrdinalIgnoreCase);
        public bool HasSKBitmap(String man_key)
        {
            return SkiaImages != null && (SkiaImages.ContainsKey(man_key) || loadedimages.ContainsKey(man_key));


        }
        public SKBitmap GetSKBitmap(String man_key,float reductionfactor = 1)
        {
            String sFindKey = man_key + (reductionfactor==1?"":reductionfactor.ToString());

            if (!SkiaImages.ContainsKey(sFindKey))
            {
                Image findkey = this[man_key, reductionfactor];
                SkiaImages.Add(sFindKey, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(new Bitmap(findkey)));
            }
            return SkiaImages[sFindKey];
           

            //return SkiaImages[man_key];
                

            
        }
        
        public Image this[String man_key,float reductionFactor=1]
        {
            get
            {
                Image resultImage = null;
                man_key = man_key.ToUpper();
                //return loadedimages[man_key];
                if (loadedimages.ContainsKey(man_key))
                    resultImage = getLoadedImage(man_key);
                else if (loadedicons.ContainsKey(man_key))
                    resultImage = getLoadedIcon(man_key, new Size(16, 16)).ToBitmap();
                else
                    //log to imagelookup.log file.
                    mcallback.FlagError("Lookup of Image " + man_key + " Failed.", null);

                if(resultImage!=null && reductionFactor!=1)
                {
                    Size TargetSize = new Size((int)((float)resultImage.Width * reductionFactor), (int)((float)resultImage.Height * reductionFactor));
                    resultImage = ReduceImage(resultImage, TargetSize);
                }

                return resultImage;
            }
            set { loadedimages[man_key] = value; }
        }
        public static Image ReduceImage(Image Source, Size TargetSize)
        {
            Bitmap Result = new Bitmap(TargetSize.Width, TargetSize.Height);
            using (Graphics g = Graphics.FromImage(Result))
            {
                g.DrawImage(Source,new Rectangle(0,0,TargetSize.Width,TargetSize.Height));
            }
            return Result;

        }
        public static SKBitmap ReduceImageSK(SKBitmap Input, SKSizeI TargetSize)
        {
            SKImageInfo skinfo = new SKImageInfo(TargetSize.Width,TargetSize.Height);
            SKBitmap BuildImage = new SKBitmap(skinfo, SKBitmapAllocFlags.ZeroPixels);
            using (SKCanvas useG = new SKCanvas(BuildImage))
            {
                useG.DrawImage(SKImage.FromBitmap(Input), new SKRect(0, 0, TargetSize.Width, TargetSize.Height), new SKPaint() { });


            }
            return BuildImage;
        }
        //default "name to stream" implementation.
        public Stream DefaultNameToStream(String filename, FileMode OpenMode)
        {
            return new FileStream(filename, OpenMode);
        }

        public ImageManager(String imagepath)
            : this(imagepath, new Nullcallback())
        {
        }

        public ImageManager(String imagepath, iManagerCallback mancallback)
        {
            mcallback = mancallback;
            LoadImages(imagepath);
        }

        public ImageManager(String[] imagepaths)
            : this(imagepaths, new Nullcallback())
        {
        }

        public ImageManager(String[] imagepaths, iManagerCallback mancallback)
        {
            mcallback = mancallback;
            LoadImages(imagepaths);
        }

        /// <summary>
        /// given a two dimensional array of a given type and a mapping function,
        /// creates a bitmap of the pixel size of the matrix and maps the colour of each pixel
        /// according to the value returned by the predicate function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Values"></param>
        /// <param name="colormapping"></param>
        /// <returns></returns>
        public static Image CreateImageFromMatrix<T>(T[][] Values, Func<T, int, int, Color> colormapping)
        {
            //Create the bitmap.
            try
            {
                Bitmap buildbitmap = new Bitmap(Values.Length, Values[0].Length);
                for (int x = 0; x < Values.Length; x++)
                {
                    for (int y = 0; y < Values[x].Length; y++)
                    {
                        buildbitmap.SetPixel(y, x, colormapping(Values[x][y], x, y));
                    }
                }

                return buildbitmap;
            }
            catch (Exception exx)
            {
                Debug.Print(exx.ToString());
            }

            return null;
        }

        public String FindImage(Image findthis)
        {
            foreach (String loopkey in loadedimages.Keys)
            {
                if (loadedimages[loopkey] == findthis)
                    return loopkey;
            }

            return "";
        }

        public void LoadImages(String frompath)
        {
            LoadImages(frompath.Split(';'));
        }

        public String AddFromFile(String filename)
        {
            return AddFromFile(filename, null);
        }

        public String AddFromFile(String filename, String Usekey)
        {
            FileStream readimagestream = new FileStream(filename, FileMode.Open);
            Image readimage = Image.FromStream(readimagestream);
            //add to the imagelist.
            if (String.IsNullOrEmpty(Usekey)) Usekey = Path.GetFileNameWithoutExtension(filename);
            loadedimages.Add(Usekey, readimage);
            readimagestream.Close();
            return Usekey;
        }

        public static bool isFileSupported(String filename)
        {
            String[] goodext = new string[] {".BMP", ".PNG", ".JPG", ".GIF", ".GZI", ".ICO"};
            foreach (string currext in goodext)
            {
                if (Path.GetExtension(filename).ToUpper().Equals(currext))
                {
                    return true;
                }
            }

            return false;
        }


        public Image[] getImageFrames(String ImageName, out int FrameCount)
        {
            Image[] gotframes = getImageFrames(ImageName);
            FrameCount = gotframes.Length;

            return gotframes;
        }

        public String[] getImageFramesString(String BaseImageName)
        {
            List<String> returnnames = new List<string>();
            BaseImageName = BaseImageName.ToUpper();
            int foundframes = 1;

            if (HasImage(BaseImageName)) returnnames.Add(BaseImageName);
            List<String> usekeys = loadedimages.Keys.ToList();
            usekeys.Sort();
            foreach (String loopkey in usekeys)
            {
                String usekey = loopkey.ToUpper();
                string findthis = BaseImageName + foundframes.ToString();
                if (usekey.StartsWith(findthis))
                {
                    returnnames.Add(findthis);
                    foundframes++;
                }
            }

            return returnnames.ToArray();
        }

        public bool Exists(String key)
        {
            //change: need to support "commands" which are basically instructions before a colon.
            if (key.Contains(":"))
            {
                return Exists(key.Substring(key.IndexOf(":") + 1));
            }
            //return loadedimages.ContainsKey(key.SubString(key.LastIndexOf(":")))

            return loadedimages.ContainsKey(key);
        }

        public Image[] getImageFrames(String[] Framenames)
        {
            Image[] result = new Image[Framenames.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = getLoadedImage(Framenames[i]);
            }

            return result;
        }

        public Image[] getImageFrames(String ImageName)
        {
            //image "frames" are stored as "ImageNameX" where X is the frame number.
            //therefore, loop through all our keys, and find those that start with ImageName and are followed by a numeric value:
            LinkedList<Image> returnframes = new LinkedList<Image>();
            ImageName = ImageName.ToUpper();
            int foundframes = 1;
            List<String> usekeys = loadedimages.Keys.ToList();
            usekeys.Sort();
            foreach (String loopkey in usekeys)
            {
                String usekey = loopkey.ToUpper();
                //colon indicates a "special" command...
                //if (usekey.Contains(":")) usekey = usekey.Substring(usekey.IndexOf(":"));
                String findthis = ImageName + foundframes.ToString();
                if (usekey.StartsWith(findthis))
                {
                    // if(usekey.Substring(findthis.Length).Equals(foundframes.ToString()))
                    // {
                    returnframes.AddLast(loadedimages[findthis]);

                    foundframes++;
                    // }
                }
            }

            return returnframes.ToArray();
        }

        public Icon getLoadedIcon(String iconindex, Size desiredSize)
        {
            string useindex = iconindex.ToUpper();
            if (loadedicons.ContainsKey(iconindex))
            {
                Icon iconobj = loadedicons[iconindex];
                return iconobj;
            }

            return null;
        }

        public Image getImageRandom(String Prefix)
        {
            Image[] choosefrom = (from m in loadedimages
                where m.Key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
                select m.Value).ToArray();

            if (choosefrom.Length > 0)
                return TetrisGame.Choose(choosefrom);
            else
                return TetrisGame.Imageman.getLoadedImage(Prefix);
        }

        public Image getLoadedImage(String searchindex,float Reduction = 1)
        {
            String index = searchindex.ToUpper();
            if (loadedimages.ContainsKey(index)) return loadedimages[index.ToUpper()];

            if (index.Contains(":"))
            {
                //special "command code"
                //command code examples:

                //FLIPX:LEAF
                //COLOURIZE(128,255,55,8):MAINBG

                //etc...
                int colonpos = index.IndexOf(":");
                String commandcode = index.Substring(0, colonpos);
                String paramvalue = index.Substring(colonpos + 1);

                if (!loadedimages.ContainsKey(paramvalue)) throw new KeyNotFoundException("Key \"" + paramvalue + "\" not found.");
                //Image acquiredimage = loadedimages[paramvalue];
                Image acquiredimage = getLoadedImage(paramvalue);
                Image changedimage = (Image) new Bitmap(acquiredimage);

                //commandcode could have parameters- check for parens.
                String[] parameters = null;
                int paramcount = 0;
                if (commandcode.Contains("("))
                {
                    //parens found.
                    int startparen = commandcode.IndexOf("(");

                    int endparen = commandcode.IndexOf(")", startparen);
                    if (endparen == -1) endparen = commandcode.Length; //if not found- go with length (just take the rest of the string).


                    String argumentstring = commandcode.Substring(startparen + 1, endparen - startparen - 1);

                    parameters = argumentstring.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    //

                    paramcount = parameters.Length;

                    commandcode = commandcode.Substring(0, startparen);
                }


                switch (commandcode)
                {
                    case "FLIPX":

                        changedimage.RotateFlip(RotateFlipType.RotateNoneFlipX);


                        break;
                    case "FLIPY":
                        changedimage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;

                    case "FLIPXY":
                        changedimage.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                        break;
                    case "ROT90":
                        changedimage.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case "ROT180":
                        changedimage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case "ROT270":
                        changedimage.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case "FLIPXROT90":
                        changedimage.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case "FLIPXROT180":
                        changedimage.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case "FLIPXROT270":
                        changedimage.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case "COLORIZE":
                    case "COLOURIZE":
                        int reduse, greenuse, blueuse, alphause = 255;
                        if (paramcount >= 4)
                        {
                            alphause = Int32.Parse(parameters[3]);
                        }

                        reduse = Int32.Parse(parameters[0]);
                        greenuse = Int32.Parse(parameters[1]);
                        blueuse = Int32.Parse(parameters[2]);

                        ColorMatrix usematrix = ColorMatrices.GetColourizer(reduse, greenuse, blueuse, alphause);
                        ImageAttributes applyattributes = new ImageAttributes();
                        applyattributes.SetColorMatrix(usematrix);
                        changedimage = TetrisGame.ApplyImageAttributes(changedimage, applyattributes);

                        if(Reduction!=1)
                        {
                            Size targetsize = new Size((int)((float)changedimage.Width * Reduction), (int)((float)changedimage.Height * Reduction));
                            changedimage = ReduceImage(changedimage, targetsize);
                        }

                        break;
                }


                //add it using the passed key.
                loadedimages.Add(index, changedimage);
                return changedimage;
            }
            else
            {
                return loadedimages[index];
            }
        }

        //load images from a zip file.
        /*
        public int LoadImages(ZipFile fromZip)
        {
            int countaccum = 0;
            //loads ALL supported files from this zip...
            foreach (ZipEntry loopentry in fromZip.Entries)
            {
                if (isFileSupported(loopentry.FileName))
                {
                    //ok, assume we can Image.FromStream this entry.
                    Image loadedImage = Image.FromStream(loopentry.InputStream);
                    String basenameonly = Path.GetFileNameWithoutExtension(loopentry.FileName).ToUpper();
                    if (!loadedimages.ContainsKey(basenameonly))
                    {
                        loadedimages.Add(basenameonly, loadedImage);
                    }
                }
            }


            return 0;
        }
        */
        public void decodegzi(Stream readfrom, Stream WriteTo)
        {
            //read the stream...
            //StreamReader sreader = new StreamReader(readfrom);
            //gzi "format" is simply a gzipped stream...
            var gzipper = new GZipStream(readfrom, CompressionMode.Decompress);
            //read the entire thing into memory
            //MemoryStream readto = new MemoryStream();
            long fullength = gzipper.TotalOut;
            byte[] readit = new byte[fullength];
            readfrom.Read(readit, 0, (int) fullength);
            WriteTo.Write(readit, 0, readit.Length);
        }

        public void encodegzi(Stream readfrom, Stream writeto)
        {
            var gzipper = new GZipStream(readfrom, CompressionMode.Compress);
            long newlength = gzipper.TotalOut;
            byte[] readit = new byte[newlength];
            readfrom.Read(readit, 0, (int) newlength);
            writeto.Write(readit, 0, readit.Length);
        }

        /// <summary>
        /// Reads a Zip File Image pack. Each image file is added or replaced in our set of images, using the base filename as the key.
        /// </summary>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        public int ReadImagePack(String sFileName)
        {
            if (!File.Exists(sFileName)) throw new FileNotFoundException(sFileName);
            //open the ZipFile. we let Exceptions bubble up to the calling method.
            ZipFile zf = new ZipFile(sFileName);
            String[] validExtensions = new string[] {"*.png", "*.ico"};
            //go through each entry.
            int totalcount = 0;
            foreach (var entry in zf)
            {
                //get the extension...
                String Extensionget = entry.FileName.Substring(entry.FileName.LastIndexOf(".", System.StringComparison.OrdinalIgnoreCase));

                if (validExtensions.Contains(Extensionget))
                {
                    //grab a stream...
                    MemoryStream ms = new MemoryStream();
                    entry.Extract(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    int lastdirsep = entry.FileName.LastIndexOf(Path.DirectorySeparatorChar);
                    int lastdot = entry.FileName.LastIndexOf(".", StringComparison.Ordinal);
                    String gotbasename = entry.FileName.Substring(lastdirsep, lastdot - lastdirsep);
                    totalcount += AddImage(ms, gotbasename) ? 1 : 0;
                }
            }


            return totalcount;
        }
        public bool ImagePrepped = false;
        public int LoadImages(IEnumerable<string> paths)
        {
            int countaccum = 0;


            foreach (String currpath in (from p in paths where Directory.Exists(p) select p))
            {
                mcallback.ShowMessage("Loading compatible images in " + currpath);
                DirectoryInfo currentdir = null;
              
                    try
                    {
                        currentdir = new DirectoryInfo(currpath);
                    }
                    catch
                    {
                        Debug.Print("Error retrieving path:" + currpath);
                    }

                    //iterate through each file, loading the images that are valid.
                    foreach (FileInfo loopfile in currentdir.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (isFileSupported(loopfile.FullName))
                        {
                            String basenameonly = Path.GetFileNameWithoutExtension(loopfile.FullName).ToUpper();
                            if (Path.GetExtension(loopfile.FullName).Equals(".ico", StringComparison.OrdinalIgnoreCase))
                            {
                                MemoryStream streamread = new MemoryStream(File.ReadAllBytes(loopfile.FullName));
                                Icon geticon = new Icon(streamread, 16, 16);
                                loadedicons.Add(basenameonly, geticon);
                                countaccum++;
                            }
                            else
                            {
                                mcallback.ShowMessage("Loading image:" + loopfile.Name);

                                MemoryStream streamread = new MemoryStream(File.ReadAllBytes(loopfile.FullName));

                                if (AddImage(streamread, basenameonly))
                                    countaccum++;
                            }
                        }

                        else if(loopfile.Extension.Equals(".zip",StringComparison.OrdinalIgnoreCase) && (ZipFile.IsZipFile(loopfile.FullName)))
                        {
                            //if this is a zipfile, then we read the contents of the zipfile and get all the applicable entries that have image file extensions, and add them using AddImage...
                            ZipFile readfile = new ZipFile(loopfile.FullName);
                            foreach (ZipEntry loopentry in readfile.Entries)
                            {
                                if (isFileSupported(loopentry.FileName))
                                {
                                    byte[] readbuffer = new byte[loopentry.UncompressedSize];

                                    loopentry.OpenReader().Read(readbuffer, 0, (int)loopentry.UncompressedSize);
                                    //MemoryStream streamread = new MemoryStream(new StreamReader(loopentry.InputStream));
                                    MemoryStream streamread = new MemoryStream(readbuffer);
                                    String basenameonly = Path.GetFileNameWithoutExtension(loopentry.FileName).ToUpper();
                                    if (Path.GetExtension(loopentry.FileName).Equals(".gzi", StringComparison.OrdinalIgnoreCase))
                                    {
                                        MemoryStream resultstream = new MemoryStream();
                                        decodegzi(streamread, resultstream);
                                        streamread = resultstream;
                                    }

                                    if ((Path.GetExtension(loopentry.FileName).Equals(".ico", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        //icon file, so add to our icon list.
                                        Icon geticon = new Icon(streamread, 16, 16);
                                        loadedicons.Add(basenameonly, geticon);
                                        countaccum++;
                                    }
                                    else
                                    {
                                        if (AddImage(streamread, basenameonly))
                                            countaccum++;
                                    }
                                }
                            }
                    }
                }
            }
            ImagePrepped = true;
            return countaccum;
            
        }

        /// <summary>
        /// Adds an image to the list.
        /// </summary>
        /// <param name="addkey">key to add the image as</param>
        /// <param name="addimage">image to add</param>
        /// <remarks>Adds a given image to the list of images using the given key. If the key already exists, the existing image will be replaced with the new one.</remarks>
        public void AddImage(String addkey, Image addimage)
        {
            addkey = addkey.ToUpper();
            if (loadedimages.ContainsKey(addkey))
            {
                loadedimages.Remove(addkey);
            }

            loadedimages.Add(addkey, addimage);
        }

        public bool AddImage(String filename)
        {
            String keyuse = Path.GetFileName(filename);
            keyuse = keyuse.Substring(0, keyuse.IndexOf('.'));

            //read the image into a memory stream.

            using (FileStream imagestream = new FileStream(filename, FileMode.Open))
            {
                return AddImage(imagestream, keyuse);
            }
        }

        private bool AddImage(Stream streamread, string basenameonly)
        {
            long startpos = streamread.Position;
            Image loadedimage = Image.FromStream(streamread);
            streamread.Seek(startpos, SeekOrigin.Begin);
            SKBitmap readbitmap = SKBitmap.Decode(streamread);
            
            if (!loadedimages.ContainsKey(basenameonly))
            {
                loadedimages.Add(basenameonly, loadedimage);
            }
            else
            {
                Debug.Print("image already loaded with key " + basenameonly);
            }
            if(!SkiaImages.ContainsKey(basenameonly))
            {
                SkiaImages.Add(basenameonly, readbitmap);
            }
            return false;
        }

        public Dictionary<String, Image> GetImages()
        {
            return loadedimages;
        }

        public bool HasImage(String key)
        {
            return loadedimages.ContainsKey(key);
        }

        public String Remove(string key)
        {
            loadedimages.Remove(key);
            return key;
        }

        public string GetFilter()
        {
            return "All Supported Files(*.JPG,*.JPEG,*.PNG,*.BMP,*.GIF)|*.jpg;*.jpeg;*.bmp;*.gif;*.png|JPEG/JPG Image(*.jpg,*.jpeg)|*.jpg;*.jpeg|GIF Image(*.gif)|*.gif|PNG Image(*.PNG)|*.png|All Files(*.*)|*.*";
        }
    }


    public class ImageKeyEditor : UITypeEditor
    {
        private class ImageKeyList : ListBox
        {
            public ImageKeyList()
            {
                // Go over all properties, filtering out the ones we need (public/get/set/boolean).
                // None is a reserved type for a case where no property is selected.
                foreach (String loopkey in TetrisGame.Imageman.GetImages().Keys)
                {
                    this.Items.Add(loopkey);
                }

                // this.Items.Add("None");


                this.Sorted = true;
                // Not setting the border to none just doesn't look good.
                this.BorderStyle = BorderStyle.None;
            }
        }


        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            Debug.Print("In GetEditStyle for ObjectTypeEditor");

            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                // This service is in charge of popping our ListBox.
                IWindowsFormsEditorService service1 = ((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService)));

                if (service1 != null)
                {
                    // This is an internal Microsoft class representing the PropertyGrid entry for our component.

                    ImageKeyList list = new ImageKeyList();

                    // Drop the list control.
                    service1.DropDownControl(list);

                    if (list.SelectedIndices.Count == 1)
                    {
                        value = list.SelectedItem.ToString();
                    }

                    // Close the list control after selection.
                    service1.CloseDropDown();
                }
            }

            return value;
        }
    }
    public static class SKColorMatrices
    {
        public static SKColorFilter GetGrayScaleFilter()
        {
            return SKColorFilter.CreateColorMatrix(new float[]
            {
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0,     0,     0,     1, 0
            });
        }
        public static SKColorFilter GetFader(float Alpha)
        {
            return SKColorFilter.CreateColorMatrix(new float[]
            {
                1f, 0f, 0f, 0, 0,
                0f, 01f, 0f, 0, 0,
                0f, 0f, 1f, 0, 0,
                0,     0,     0,     Alpha, 0
            });
        }
        public static SKColorFilter GetColourizer(float red, float green, float blue, float alpha)
            {

                float[] buildmat = new float[]
                {
                    red, 0f, 0f, 0, 0,
                    0f, green, 0, 0, 0,
                    0f, 0f, blue, 0, 0,
                    0,     0,     0,     alpha, 0
                };
            return SKColorFilter.CreateColorMatrix(buildmat);
        }
        public static SKColorFilter GetBlackener()
        {
            float[] buildmat = new float[]
            {
                0, 0f, 0f, 0, 0,
                0f, 0, 0, 0, 0,
                0f, 0f, 0, 0, 0,
                0,     0,     0,     1, 0
            };
            return SKColorFilter.CreateColorMatrix(buildmat);
        }
    }

    public static class ColorMatrices
    {
        private static float[][] _colorMatrixElements =
        {
            new float[] {1, 0, 0, 0, 0}, // red scaling factor of 2
            new float[] {0, 1, 0, 0, 0}, // green scaling factor of 1
            new float[] {0, 0, 2, 0, 0}, // blue scaling factor of 1
            new float[] {0, 0, 0, 1, 0}, // alpha scaling factor of 1
            new float[] {-0.5f, -0.5f, .8f, 0, 1}
        }; // three translations of 0.2

        public static ColorMatrix GetFader(float alpha)
        {
            return new ColorMatrix
            (new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, alpha, 0},
                new float[] {0, 0, 0, 0, 1}
            });
        }

        public static ColorMatrix GetColourizer(Color fromcolor)
        {
            return GetColourizer(fromcolor.R, fromcolor.G, fromcolor.B, fromcolor.A);
        }

        public static ColorMatrix GetColourizer(float red, float green, float blue)
        {
            return GetColourizer(red, green, blue, 1);
        }

        public static void AddColourizer(ImageAttributes toia, Color usecolor)
        {
            ColorMatrix grayscaler = GrayScale();
            toia.SetColorMatrices(GetColourizer(usecolor), grayscaler);
        }


        public static ColorMatrix GrayScale()
        {
            return new ColorMatrix
            (
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            
        }

        public static float[][] GetIdentity()
        {
            return new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            };
        }

        public static ColorMatrix GetRedColourizer(float red, float green, float blue, float alpha)
        {
            float[][] matElement =
            {
                new float[] {red, 0, 0, 0, 0}, //red scaling factor
                new float[] {0, green, 0, 0, 0}, // green scaling factor of 1
                new float[] {0, 0, blue, 0, 0}, // blue scaling factor of 1
                new float[] {0, 0, 0, alpha, 0}, // alpha scaling factor of 1
                new float[] {0, 0, 0, 0, 1}
            }; // three translations of 0.2
            //change the appropriate elements to match....
            matElement[0][0] = red;
            matElement[1][1] = green;
            matElement[2][2] = blue;
            matElement[3][3] = alpha;
            matElement[4][4] = 1;
            return new ColorMatrix(matElement);
        }

        public static ColorMatrix GetColourizer(float red, float green, float blue, float alpha)
        {
            /*
            float[][] MatElement =  { 
   new float[] {red,  0,  0,  0, 0},        //red scaling factor
   new float[] {0,  green,  0,  0, 0},        // green scaling factor of 1
   new float[] {0,  0,  blue,  0, 0},        // blue scaling factor of 1
   new float[] {0,  0,  0,  alpha, 0},        // alpha scaling factor of 1
   new float[] {-0.5f, -0.5f, .8f, 0, alpha}};    // three translations of 0.2
             */
            float[][] matElement =
            {
                new float[] {red, 0, 0, 0, 0}, //red scaling factor
                new float[] {0, green, 0, 0, 0}, // green scaling factor of 1
                new float[] {0, 0, blue, 0, 0}, // blue scaling factor of 1
                new float[] {0, 0, 0, alpha, 0}, // alpha scaling factor of 1
                new float[] {0, 0, 0, 0, 1}
            }; // three translations of 0.2
            //change the appropriate elements to match....
            matElement[0][0] = red;
            matElement[1][1] = green;
            matElement[2][2] = blue;
            matElement[3][3] = alpha;
            matElement[4][4] = 1;
            return new ColorMatrix(matElement);
        }
    }

    #region HSLColor

    /// <summary>
    /// Class used to convert to and from Hue,Saturation, and Luminousity.
    /// </summary>
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double _hue = 1.0;
        private double _saturation = 1.0;
        private double _luminosity = 1.0;

        private const double Scale = 240.0;

        public double Hue
        {
            get { return _hue * Scale; }
            set { _hue = CheckRange(value / Scale); }
        }

        public double Saturation
        {
            get { return _saturation * Scale; }
            set { _saturation = CheckRange(value / Scale); }
        }

        public double Luminosity
        {
            get { return _luminosity * Scale; }
            set { _luminosity = CheckRange(value / Scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color) this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        #region Casts to/from System.Drawing.Color

        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor._luminosity != 0)
            {
                if (hslColor._saturation == 0)
                    r = g = b = hslColor._luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor._luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor._hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor._hue);
                    b = GetColorComponent(temp1, temp2, hslColor._hue - 1.0 / 3.0);
                }
            }

            return Color.FromArgb((int) (255 * r), (int) (255 * g), (int) (255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }

        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }

        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor._luminosity < 0.5) //<=??
                temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            else
                temp2 = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor._hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor._luminosity = color.GetBrightness();
            hslColor._saturation = color.GetSaturation();
            return hslColor;
        }

        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor) Color.FromArgb(red, green, blue);
            this._hue = hslColor._hue;
            this._saturation = hslColor._saturation;
            this._luminosity = hslColor._luminosity;
        }

        public HSLColor()
        {
        }

        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }

        public HSLColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }

        public HSLColor(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

        public static Color RotateHue(Color Source, int Amount)
        {
            Amount = Amount % 240;
            HSLColor hs = new HSLColor(Source);
            hs.Hue = (hs.Hue + Amount) % 240;
            return hs;
        }

        public static Color RandomHue(double useSat, double uselum)
        {
            return new HSLColor(new Random().NextDouble() * 240, useSat, uselum);
        }
    }

    #endregion


    /// <summary>
    /// Used to retrieve key state info
    /// </summary>
    public class KeyboardInfo
    {
        private KeyboardInfo()
        {
        }

        [DllImport("user32.dll")]
        public static extern Int16 GetAsyncKeyState(int vKey);

        public static bool IsPressed(Keys key)
        {
            return GetAsyncKeyState((int) key) < 0;
        }

        [DllImport("user32")]
        private static extern short GetKeyState(int vKey);

        public static KeyStateInfo GetKeyState(Keys key)
        {
            short keyState = GetKeyState((int) key);
            byte[] bits = BitConverter.GetBytes(keyState);
            bool toggled = bits[0] == 1, pressed = bits[1] == 1;
            return new KeyStateInfo(key, pressed, toggled);
        }
    }


    public struct KeyStateInfo
    {
        Keys _key;

        bool _isPressed,
            _isToggled;

        public KeyStateInfo(Keys key,
            bool ispressed,
            bool istoggled)
        {
            _key = key;
            _isPressed = ispressed;
            _isToggled = istoggled;
        }

        public static KeyStateInfo Default
        {
            get
            {
                return new KeyStateInfo
                (Keys.None,
                    false,
                    false);
            }
        }

        public Keys Key
        {
            get { return _key; }
        }

        public bool IsPressed
        {
            get { return _isPressed; }
        }

        public bool IsToggled
        {
            get { return _isToggled; }
        }
    }

    public static class StandardMatrices
    {
        public static ColorMatrix InvertImage()
        {
            ColorMatrix cmPicture = new System.Drawing.Imaging.ColorMatrix();

            // Change the elements

            cmPicture.Matrix00 = -1;

            cmPicture.Matrix11 = -1;

            cmPicture.Matrix22 = -1;
            return cmPicture;
        }
    }
}