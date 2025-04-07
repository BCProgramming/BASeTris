using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace BASeTris.AssetManager
{
    public class BASSLoadedSound : ISoundSource
    {
        Func<int> NewStream = null;
        public bool SingleStream = false;
        List<BASSPlayingSound> PlayingSounds = new List<BASSPlayingSound>();
        public BASSLoadedSound(Func<int> pNewStream)
        {
            NewStream = pNewStream;
        }
        public IActiveSound Play(bool playlooped)
        {
            return Play(playlooped, 1.0f);
        }
        static bool _noFx = false;
        public IActiveSound Play(bool playlooped, float volume, float tempo = 1, float pitch = 0)
        {
            int ActiveStream = 0, maketempostream = 0;
            if (SingleStream && PlayingSounds.Any())
            {
                var sPlaying = PlayingSounds.First();
                ActiveStream = sPlaying._soundStream;
                maketempostream = sPlaying._tempoStream;
            }
            else
            {
                //remove all finished entries.
                var FinishedItems = PlayingSounds.Where((s) => s.Finished);
                foreach (var disposeit in FinishedItems)
                {
                    disposeit.Dispose();
                }

                PlayingSounds.RemoveAll((s) => s.Finished);

                ActiveStream = NewStream();
                if (ActiveStream == 0) return null;


                //create a tempostream if needed.

                if (!_noFx)
                {
                    try
                    {
                        maketempostream = ManagedBass.Fx.BassFx.TempoCreate(ActiveStream, ManagedBass.BassFlags.FxFreeSource);
                    }
                    catch (DllNotFoundException exx)
                    {
                        _noFx = true;
                        //grrr...
                        //oh well. Seems like BASS.NET ignores the call if it doesn't understand the tempo stuff.
                        maketempostream = 0;
                    }
                }
            }
            var playstream = () => maketempostream == 0 ? ActiveStream : maketempostream;

            ManagedBass.Bass.ChannelSetAttribute(playstream(),ManagedBass.ChannelAttribute.Volume , volume);
            



            if (playlooped) ManagedBass.Bass.ChannelFlags(playstream(), ManagedBass.BassFlags.Loop, ManagedBass.BassFlags.Loop);

            //ManagedBass.Bass.ChannelSetSync(ActiveStream, ManagedBass.SyncFlags.End |  ManagedBass.SyncFlags.Onetime, 0, soundstopproc, IntPtr.Zero);
            ManagedBass.Bass.ChannelSetAttribute(maketempostream, ManagedBass.ChannelAttribute.Tempo, tempo);
            ManagedBass.Bass.ChannelSetAttribute(maketempostream, ManagedBass.ChannelAttribute.Pitch, pitch);

            var playresult = ManagedBass.Bass.ChannelPlay(playstream(), true);
            
            var PlaySoundEntry = new BASSPlayingSound(this,ActiveStream,maketempostream);
            PlayingSounds.Add(PlaySoundEntry);
            
            return PlaySoundEntry;
        }
    }
    public class BASSPlayingSound : IActiveSound,IDisposable
    {

        internal readonly int _soundStream;
        internal readonly int _tempoStream;
        public BASSLoadedSound SoundSource { get; init; }
        public BASSPlayingSound(BASSLoadedSound pSoundSource,int hStream,int TempoStream)
        {
            _soundStream = hStream;
            _tempoStream = TempoStream;
            SoundSource = pSoundSource;
            //TODO: create stream handle here.
        }
        public int ActiveStream
        {
            get { return _tempoStream != 0 ? _tempoStream : _soundStream; }
        }
        private bool _disposed;
        ~BASSPlayingSound()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            ManagedBass.Bass.StreamFree(_soundStream);
            //if we have an unmanaged block, free it now.
            /*if (_UnmanagedBlock != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_UnmanagedBlock);
            }*/
        }



        public bool Finished
        {
            get { return ManagedBass.Bass.ChannelGetPosition(ActiveStream) >= ManagedBass.Bass.ChannelGetLength(ActiveStream); }
        }


        public bool Paused { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public float Progress
        {
            get
            {
                // return ManagedBass.Bass.StreamGetFilePosition(SoundStream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT);
                return ManagedBass.Bass.ChannelGetPosition(ActiveStream) / ((float) ManagedBass.Bass.ChannelGetLength(ActiveStream));
            }
        }

        public float Level
        {
            get
            {
                int levelresult = ManagedBass.Bass.ChannelGetLevel(ActiveStream);
                int left = LoWord(levelresult); // the left level
                int right = HighWord(levelresult); // the right level
                return 32767f / ((left + (float) right) / 2);
            }
        }

        public ISoundSource Source => SoundSource;


        public void Stop()
        {
            ManagedBass.Bass.ChannelStop(ActiveStream);
        }

        public void Pause()
        {
            ManagedBass.Bass.ChannelPause(ActiveStream);
        }

        public void UnPause()
        {
            ManagedBass.Bass.ChannelPlay(ActiveStream, false);
        }

        public float Tempo
        {
            get
            {
                float result = 0;
                ManagedBass.Bass.ChannelGetAttribute(_tempoStream,ManagedBass.ChannelAttribute.Tempo, out result);
                if (result == 0)
                    ManagedBass.Bass.ChannelGetAttribute(ActiveStream, ManagedBass.ChannelAttribute.MusicSpeed, out result);
                return result;
            }
            set
            {
                Debug.Print("Setting Tempo of Channel ID" + _soundStream + " (Tempo ID#" + _tempoStream + ")to " + value);
                ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Tempo, value);


                
                // ManagedBass.Bass.ChannelSetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_MUSIC_SPEED, CalculatedValue);
            }
        }
        public float Pitch
        {
            get
            {
                float result = 0;
                ManagedBass.Bass.ChannelGetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Pitch, out result);
                if (result == 0)
                    ManagedBass.Bass.ChannelGetAttribute(ActiveStream, ManagedBass.ChannelAttribute.Pitch, out result);
                return result;
            }
            set
            {
                Debug.Print("Setting Pitch of Channel ID " + _soundStream + " (Tempo ID#" + _tempoStream + ") to " + value);
                ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Pitch, value);
                
            }
        }
       

        public void SetVolume(float volumeset)
        {
            ManagedBass.Bass.ChannelSetAttribute(ActiveStream, ManagedBass.ChannelAttribute.MusicVolumeChannel, volumeset);
        }

        public int HighWord(int Source)
        {
            return Source >> 16;
        }
        public int LoWord(int Source)
        {
            return Source & 0xFFFF;
        }

    }


    //Older implementation, which had both the active and source implementation in the same class. Was separated later to allow multiple playing sounds of the same source.
    //This was pulled from BASeBlock at some point.

    public class BASSSound : ISoundSource, IActiveSound, IDisposable
    {
        //TODO: more SYNCPROC events...
        public delegate void BASS_SoundStoppedFunc(BASSSound sourcesound);

        //flag so that after we fail at TempoCreate we don't continue to fail over and over, instead
        //we skip creating the tempostream.
        static bool _noFx;

        private readonly int _soundStream;
        private readonly int _tempoStream;
        private readonly ManagedBass.SyncProcedure soundstopproc;
        private bool _disposed;
        private bool _paused = false;
        private IntPtr _UnmanagedBlock = IntPtr.Zero;

        public ISoundSource Source
        {
            get { return this; }
        }

        public float Progress
        {
            get
            {
                // return ManagedBass.Bass.StreamGetFilePosition(SoundStream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT);
                return ManagedBass.Bass.ChannelGetPosition(ActiveStream) / ((float) ManagedBass.Bass.ChannelGetLength(ActiveStream));
            }
        }
        /*
         //Get the higher order value.
        var high = number >> 16;
        Console.WriteLine($"High: {high:X}");

        //Get the lower order value.
        var low = number & 0xFFFF; //Or use 0x0000FFFF
        Console.WriteLine($"Low: {low:X}");

                     */
                     public int HighWord(int Source)
        {
            return Source >> 16;
        }
        public int LoWord(int Source)
        {
            return Source & 0xFFFF;
        }
        public float Level
        {
            get
            {
                int levelresult = ManagedBass.Bass.ChannelGetLevel(ActiveStream);
                int left = LoWord(levelresult); // the left level
                int right = HighWord(levelresult); // the right level
                return 32767f / ((left + (float) right) / 2);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            //if we have an unmanaged block, free it now.
            if (_UnmanagedBlock != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_UnmanagedBlock);
            }
        }

        public BASSSound(int streamnum, IntPtr freeblock) : this(streamnum)
        {
            _UnmanagedBlock = freeblock;
        }

        public BASSSound(int streamnum)
        {
            soundstopproc = BASSCallbackStop;
            _soundStream = streamnum;
            if (!_noFx)
            {
                try
                {
                    _tempoStream = ManagedBass.Fx.BassFx.TempoCreate(streamnum, ManagedBass.BassFlags.FxFreeSource);
                }
                catch (DllNotFoundException exx)
                {
                    _noFx = true;
                    //grrr...
                    //oh well. Seems like BASS.NET ignores the call if it doesn't understand the tempo stuff.
                    _tempoStream = 0;
                }
            }
        }

        private int ActiveStream
        {
            get { return _tempoStream != 0 ? _tempoStream : _soundStream; }
        }

        public event BASS_SoundStoppedFunc BASS_SoundStopped;

        private void InvokeSoundStopped()
        {
            var copy = BASS_SoundStopped;
            if (copy != null)
            {
                copy(this);
            }
        }

        public void Dispose(bool parameter)
        {
            Dispose();
        }

        ~BASSSound()
        {
        }


        #region iSoundSourceObject Members

       /* public float getLength()
        {
            var len = ManagedBass.Bass.ChannelGetLength(ActiveStream,ManagedBass.PositionFlags.Bytes);
            float result = (float) ManagedBass.Bass.ChannelBytes2Seconds(ActiveStream, len);
            return result;
            //QWORD len = BASS_ChannelGetLength(channel, BASS_POS_BYTE); // the length in bytes
            //double time = BASS_ChannelBytes2Seconds(channel, len); // the length in seconds
        }*/

        public IActiveSound Play(bool playlooped)
        {
            //ManagedBass.Bass.SetVolume(1.0f);

            return Play(playlooped, 1.0f);
        }

        private void BASSCallbackStop(int handle, int channel, int data, IntPtr uservalue)
        {
            InvokeSoundStopped();
        }

        public IActiveSound Play(bool playlooped, float volume,float tempo = 1f,float pitch = 0f)
        {
            ManagedBass.Bass.ChannelSetAttribute(ActiveStream,ManagedBass.ChannelAttribute.Volume , volume);
            
            if (playlooped) ManagedBass.Bass.ChannelFlags(ActiveStream, ManagedBass.BassFlags.Loop, ManagedBass.BassFlags.Loop);
            ManagedBass.Bass.ChannelSetSync(ActiveStream, ManagedBass.SyncFlags.End |  ManagedBass.SyncFlags.Onetime, 0, soundstopproc, IntPtr.Zero);
            ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Tempo, tempo);
            ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Pitch, pitch);

            ManagedBass.Bass.ChannelPlay(ActiveStream, true);
            
            return this;
        }

        #endregion

        #region iActiveSoundObject Members

        public bool Finished
        {
            get { return ManagedBass.Bass.ChannelGetPosition(ActiveStream) >= ManagedBass.Bass.ChannelGetLength(ActiveStream); }
        }

        public void Stop()
        {
            ManagedBass.Bass.ChannelStop(ActiveStream);
        }

        public void Pause()
        {
            ManagedBass.Bass.ChannelPause(ActiveStream);
        }

        public void UnPause()
        {
            ManagedBass.Bass.ChannelPlay(ActiveStream, false);
        }

        public float Tempo
        {
            get
            {
                float result = 0;
                ManagedBass.Bass.ChannelGetAttribute(_tempoStream,ManagedBass.ChannelAttribute.Tempo, out result);
                if (result == 0)
                    ManagedBass.Bass.ChannelGetAttribute(ActiveStream, ManagedBass.ChannelAttribute.MusicSpeed, out result);
                return result;
            }
            set
            {
                Debug.Print("Setting Tempo of Channel ID" + _soundStream + " (Tempo ID#" + _tempoStream + ")to " + value);
                ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Tempo, value);


                
                // ManagedBass.Bass.ChannelSetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_MUSIC_SPEED, CalculatedValue);
            }
        }
        public float Pitch
        {
            get
            {
                float result = 0;
                ManagedBass.Bass.ChannelGetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Pitch, out result);
                if (result == 0)
                    ManagedBass.Bass.ChannelGetAttribute(ActiveStream, ManagedBass.ChannelAttribute.Pitch, out result);
                return result;
            }
            set
            {
                Debug.Print("Setting Pitch of Channel ID " + _soundStream + " (Tempo ID#" + _tempoStream + ") to " + value);
                ManagedBass.Bass.ChannelSetAttribute(_tempoStream, ManagedBass.ChannelAttribute.Pitch, value);
                
            }
        }

        public bool Paused
        {
            get { return (ManagedBass.Bass.ChannelIsActive(ActiveStream) ==  ManagedBass.PlaybackState.Paused); }
            set
            {
                if (value)
                {
                    ManagedBass.Bass.ChannelPause(ActiveStream);
                }
                else
                {
                    ManagedBass.Bass.ChannelPlay(ActiveStream, false);
                }
            }
        }

        public void SetVolume(float volumeset)
        {
            ManagedBass.Bass.ChannelSetAttribute(ActiveStream, ManagedBass.ChannelAttribute.MusicVolumeChannel, volumeset);
        }

        #endregion
    }

    class BASSDriver : ISoundEngineDriver
    {
        public static string DrvName = "NBASS";

        
        private Dictionary<int, String> loadedbassplugs;

        public override string ToString()
        {
            return Name;
        }

        public String Name
        {
            get { return "NBASS"; }
        }

        public void Dispose()
        {
            
            ManagedBass.Bass.Free();
            
            //BassMix.FreeMe();
            foreach (var disposeplugin in loadedbassplugs)
            {
                ManagedBass.Bass.PluginFree(disposeplugin.Key);
            }
        }
        private Dictionary<int,String> BassLoadPluginsDir(String[] pPath)
        {
            Dictionary<int, String> result = new Dictionary<int, string>();
            foreach (var checkpath in pPath)
            {

                DirectoryInfo di = new DirectoryInfo(checkpath);
             
                foreach (var iterate in di.GetFiles("*.dll"))
                {
                    int PluginResult = ManagedBass.Bass.PluginLoad(iterate.FullName);
                    if (PluginResult != 0)
                    {
                        result.Add(PluginResult, iterate.FullName);
                    }
                }
            }
            return result;
        }
        public BASSDriver(String pluginFolder)
        {
            SupportedExtensions = ".MP3|.WAV|.OGG|.MID|.IT|.S3M|.XM";

            //C:\Users\BC_Programming\AppData\Roaming\BASeBlock\Lib\x86
            //or, the application data folder \Lib\x86...
          

            String[] x86DLL = TetrisGame.GetSearchFolders().Concat(from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Lib\\x86")).ToArray();
            String[] x64DLL = TetrisGame.GetSearchFolders().Concat(from s in TetrisGame.GetSearchFolders() select Path.Combine(s, "Lib\\x64")).ToArray();

            string pathuse = LoadProperDLL(x86DLL, x64DLL); //load the x64 or x86 version as needed
            
            
            //Interestingly, switching to ManagedBass seems to have <removed> the ability to load DLLs from a particular folder.
            //Though- turns out the program was set to always build x86 anyway.
            
            var result = ManagedBass.Bass.Init(-1, 44100,ManagedBass.DeviceInitFlags.Default, IntPtr.Zero);
            loadedbassplugs = BassLoadPluginsDir(new string[] { pathuse });
            //ManagedBass.Bass.SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 10 + info.minbuf + 1);
            // default buffer size = update period + 'minbuf' + 1ms extra margin

            //buflen = BASS_GetConfig(BASS_CONFIG_BUFFER);


            /*
              if (PluginFolder.Length > 0)
              {
                  Dictionary<int,String> loadedbassplugs = ManagedBass.Bass.PluginLoadDirectory(PluginFolder);
                  if (loadedbassplugs != null) //added Feb 16 2010
                  {
                      foreach (KeyValuePair<int, String> kvp in loadedbassplugs)
                      {
                          Debug.Print(kvp.Key.ToString() + ":" + kvp.Value);
                          String pluginfile = kvp.Value;
                          if (Path.GetFileNameWithoutExtension(pluginfile).Equals("bassflac", StringComparison.OrdinalIgnoreCase))
                          {
                              //add flac
                              SupportedExtensions += "|.FLAC";

                          }

                      }
                  }
              }*/
        }


        public BASSDriver()
            : this(TetrisGame.AppDataFolder)
        {
        }


        private void BasssoundStop(BASSSound soundstopped)
        {
            FireSoundStop(soundstopped);
        }

        /// <summary>
        /// </summary>
        /// <param name="x86Path"></param>
        /// <param name="x64Path"></param>
        /// <returns>the path that was chosen</returns>
        private string LoadProperDLL(String[] x86Path, String[] x64Path)
        {
            String dlltoload;
            //C:\Users\BC_Programming\AppData\Roaming\BASeBlock\Lib\x86
            if (Environment.Is64BitProcess)
                dlltoload = x64Path.FirstOrDefault(a => Directory.Exists(a));
            else
                dlltoload = x86Path.FirstOrDefault(a => Directory.Exists(a));

            Debug.Print("Loading bass.net from:" + dlltoload);
            
            
            //Bass.LoadMe(dlltoload);
            //BassMix.LoadMe(dlltoload);
            //Don't forget BassFX too!
            //BassFx.LoadMe(dlltoload);
            return dlltoload;
        }

        #region iSoundEngineDriver Members

        public event OnSoundStopDelegate OnSoundStop;

        public event OnSoundPlayDelegate OnSoundPlay;

        public void FireSoundPlay(IActiveSound soundPlayed)
        {
            OnSoundPlayDelegate temp = OnSoundPlay;
            if (temp != null)
                temp(soundPlayed);
        }

        public void FireSoundStop(IActiveSound soundStopped)
        {
            OnSoundStopDelegate temp = OnSoundStop;
            if (temp != null)
                temp(soundStopped);
        }

        public ISoundSource LoadSound(Stream fromstream)
        {
            //int stmake = ManagedBass.Bass.StreamCreateFile(
            return null;
        }
        private int GetStreamForData(byte[] data, string sName, String fileextension)
        {
            String extension = fileextension;
            if (extension.Equals(".xm", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Print("xm file...");
            }

            int stmake = 0;

            //we need to allocate some unmanaged memory.
            


            if (new[] {".xm", ".it"}.Contains(extension.ToLower()))
            {
                stmake = ManagedBass.Bass.MusicLoad(data, 0, 0);
            }
            else
            {
                stmake = ManagedBass.Bass.CreateStream(data, 0, 0,ManagedBass.BassFlags.Decode);
            }
            return stmake;
        }
        const bool UseDeluxeSound = true;
        public ISoundSource LoadSound(byte[] data, string sName, string fileextension)
        {
            if (!UseDeluxeSound)
            {
                var stmake = GetStreamForData(data, sName, fileextension);

                if (stmake != 0)
                {
                    //create the BASSSound object; we need to give it the unManagedPointer, so that it will be able to properly free that resource when it is destructed.
                    BASSSound returnsound = new BASSSound(stmake);
                    returnsound.BASS_SoundStopped += BasssoundStop;
                    return returnsound;
                }
            }
            else
            {

                BASSLoadedSound bls = new BASSLoadedSound(() => GetStreamForData(data, sName, fileextension));
                //todo: Sound stop event?
                return bls;


            }
            return null;
        }

        public int GetStreamForFile(String sFilename)
        {
            String extension = Path.GetExtension(sFilename);
            if (extension.Equals(".xm", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Print("xm file...");
            }

            int stmake = 0;

            if (new[] {".xm", ".it", ".mod"}.Contains(extension.ToLower()))
            {
                stmake = ManagedBass.Bass.MusicLoad(sFilename, 0, 0);
            }
            else
            {
                stmake = ManagedBass.Bass.CreateStream(sFilename, 0, 0, ManagedBass.BassFlags.Decode);
            }
            return stmake;
        }

        public ISoundSource LoadSound(string filename)
        {
            if (!UseDeluxeSound)
            {
                int stmake = GetStreamForFile(filename);

                if (stmake != 0)
                {
                    BASSSound returnsound = new BASSSound(stmake);
                    returnsound.BASS_SoundStopped += BasssoundStop;
                    return returnsound;
                }
            }
            else
            {
                BASSLoadedSound loadsound = new BASSLoadedSound(() => GetStreamForFile(filename));
                return loadsound;
            }
            return null;
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return SupportedExtensions.Split('|');
        }

        readonly String SupportedExtensions;

        #endregion
    }
}