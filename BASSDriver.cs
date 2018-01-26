using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;

namespace BASeTris.AssetManager
{
    public class BASSSound : iSoundSourceObject, iActiveSoundObject, IDisposable
    {

        private int ActiveStream
        {
            get
            {
                return _tempoStream != 0 ? _tempoStream : _soundStream;

            }



        }

        private readonly int _soundStream = 0;
        private readonly int _tempoStream = 0;
        private bool _paused = false;
        private readonly SYNCPROC soundstopproc;

        public iSoundSourceObject Source
        {
            get { return this; }
        }
        public float Progress
        {
            get
            {

                // return Bass.BASS_StreamGetFilePosition(SoundStream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT);
                return ((float)Bass.BASS_ChannelGetPosition(ActiveStream)) / ((float)Bass.BASS_ChannelGetLength(ActiveStream));
            }
        }
        private IntPtr _UnmanagedBlock = IntPtr.Zero;
        //TODO: more SYNCPROC events...
        public delegate void BASS_SoundStoppedFunc(BASSSound sourcesound);

        public event BASS_SoundStoppedFunc BASS_SoundStopped;

        private void InvokeSoundStopped()
        {
            var copy = BASS_SoundStopped;
            if (copy != null)
            {
                copy(this);


            }



        }
        public BASSSound(int streamnum, IntPtr freeblock) : this(streamnum)
        {
            _UnmanagedBlock = freeblock;


        }
        //flag so that after we fail at TempoCreate we don't continue to fail over and over, instead
        //we skip creating the tempostream.
        static bool _noFx = false;
        public BASSSound(int streamnum)
        {
            soundstopproc = BASSCallbackStop;
            _soundStream = streamnum;
            if (!_noFx)
            {
                try
                {
                    _tempoStream = Un4seen.Bass.AddOn.Fx.BassFx.BASS_FX_TempoCreate(streamnum, BASSFlag.BASS_FX_FREESOURCE);

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
        private bool _disposed = false;
        public void Dispose(bool parameter)
        {
            Dispose();
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
        ~BASSSound()
        {


        }


        #region iSoundSourceObject Members
        public float getLength()
        {

            var len = Bass.BASS_ChannelGetLength(ActiveStream, BASSMode.BASS_POS_BYTES);
            float result = (float)Bass.BASS_ChannelBytes2Seconds(ActiveStream, len);
            return result;
            //QWORD len = BASS_ChannelGetLength(channel, BASS_POS_BYTE); // the length in bytes
            //double time = BASS_ChannelBytes2Seconds(channel, len); // the length in seconds
        }

        public iActiveSoundObject Play(bool playlooped)
        {
            //Bass.BASS_SetVolume(1.0f);

            return Play(playlooped, 1.0f);
        }
        private void BASSCallbackStop(int handle, int channel, int data, IntPtr uservalue)
        {
            InvokeSoundStopped();


        }

        public iActiveSoundObject Play(bool playlooped, float volume)
        {
            Bass.BASS_ChannelSetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_VOL, volume);

            if (playlooped) Bass.BASS_ChannelFlags(ActiveStream, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
            Bass.BASS_ChannelSetSync(ActiveStream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_ONETIME, 0, soundstopproc, IntPtr.Zero);
            Bass.BASS_ChannelPlay(ActiveStream, true);
            
            return this;
        }

        
        #endregion

        #region iActiveSoundObject Members

        public bool Finished
        {
            get { return Bass.BASS_ChannelGetPosition(ActiveStream) >= Bass.BASS_ChannelGetLength(ActiveStream); }
        }

        public void Stop()
        {
            Bass.BASS_ChannelStop(ActiveStream);
        }

        public void Pause()
        {
            Bass.BASS_ChannelPause(ActiveStream);
        }

        public void UnPause()
        {
            Bass.BASS_ChannelPlay(ActiveStream, false);
        }
        public float Tempo
        {

            get
            {
              
                float result = 0;
                Bass.BASS_ChannelGetAttribute(_tempoStream, BASSAttribute.BASS_ATTRIB_TEMPO, ref result);
                if (result == 0)
                    Bass.BASS_ChannelGetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_MUSIC_SPEED, ref result);
                return result;

            }
            set
            {
                Debug.Print("Setting Tempo of Channel ID" + _soundStream + " (Tempo ID#" + _tempoStream + ")to " + value);
                Bass.BASS_ChannelSetAttribute(_tempoStream, BASSAttribute.BASS_ATTRIB_TEMPO, value);
                int CalculatedValue;
                CalculatedValue = TetrisGame.ClampValue((int)(value * 128), 0, 255);



               // Bass.BASS_ChannelSetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_MUSIC_SPEED, CalculatedValue);
            }

        }
        public bool Paused
        {
            get
            {
                return (Bass.BASS_ChannelIsActive(ActiveStream) == BASSActive.BASS_ACTIVE_PAUSED);
            }
            set
            {
                if (value)
                {
                    Bass.BASS_ChannelPause(ActiveStream);
                }
                else
                {
                    Bass.BASS_ChannelPlay(ActiveStream, false);
                }
            }
        }

        public void setVolume(float volumeset)
        {

            Bass.BASS_ChannelSetAttribute(ActiveStream, BASSAttribute.BASS_ATTRIB_MUSIC_VOL_CHAN, volumeset);
        }

        #endregion
    }
    class BASSDriver : iSoundEngineDriver
    {

        Un4seen.Bass.Bass _bassInstance;


        private void BasssoundStop(BASSSound soundstopped)
        {
            FireSoundStop(soundstopped);


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x86Path"></param>
        /// <param name="x64Path"></param>
        /// <returns>the path that was chosen</returns>
        private string LoadProperDLL(String x86Path, String x64Path)
        {
            String dlltoload;
            //C:\Users\BC_Programming\AppData\Roaming\BASeBlock\Lib\x86
            if (Utils.Is64Bit)
                dlltoload = x64Path;
            else
                dlltoload = x86Path;

            Debug.Print("Loading bass.net from:" + dlltoload);
            Bass.LoadMe(dlltoload);
            BassMix.LoadMe(dlltoload);
            //Don't forget BassFX too!
            Un4seen.Bass.AddOn.Fx.BassFx.LoadMe(dlltoload);
            return dlltoload;

        }
        private Dictionary<int, String> loadedbassplugs = null;
        public BASSDriver(String pluginFolder)
        {
            SupportedExtensions = ".MP3|.WAV|.OGG|.MID|.IT|.S3M|.XM";

            //C:\Users\BC_Programming\AppData\Roaming\BASeBlock\Lib\x86
            //or, the application data folder \Lib\x86...
            if(!Directory.Exists(TetrisGame.AppDataFolder))
            {

            }
            String x86DLL = Path.Combine(TetrisGame.AppDataFolder, "Lib\\x86");
            String x64DLL = Path.Combine(TetrisGame.AppDataFolder, "Lib\\x64");

            string pathuse = LoadProperDLL(x86DLL, x64DLL); //load the x64 or x86 version as needed
            loadedbassplugs = Bass.BASS_PluginLoadDirectory(pathuse);

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            //Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 10 + info.minbuf + 1);
            // default buffer size = update period + 'minbuf' + 1ms extra margin

            //buflen = BASS_GetConfig(BASS_CONFIG_BUFFER);



            /*
              if (PluginFolder.Length > 0)
              {
                  Dictionary<int,String> loadedbassplugs = Bass.BASS_PluginLoadDirectory(PluginFolder);
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
            : this(Path.Combine(TetrisGame.AppDataFolder, "SoundPlugin"))
        {


        }
        public override string ToString()
        {
            return Name;
        }
        public String Name
        {
            get { return "NBASS"; }

        }
        public static string DrvName = "NBASS";

        #region iSoundEngineDriver Members

        public event OnSoundStopDelegate OnSoundStop;

        public event OnSoundPlayDelegate OnSoundPlay;
        public void FireSoundPlay(iActiveSoundObject soundPlayed)
        {
            OnSoundPlayDelegate temp = OnSoundPlay;
            if (temp != null)
                temp(soundPlayed);



        }
        public void FireSoundStop(iActiveSoundObject soundStopped)
        {
            OnSoundStopDelegate temp = OnSoundStop;
            if (temp != null)
                temp(soundStopped);



        }
        public iSoundSourceObject LoadSound(Stream fromstream)
        {

            //int stmake = Bass.BASS_StreamCreateFile(
            return null;

        }

        public iSoundSourceObject LoadSound(byte[] data, string sName, string fileextension)
        {
            String extension = fileextension;
            if (extension.Equals(".xm", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Print("xm file...");


            }
            int stmake = 0;

            //we need to allocate some unmanaged memory.
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);



            if (new String[] { ".xm", ".it" }.Contains(extension.ToLower()))
            {

                stmake = Bass.BASS_MusicLoad(unmanagedPointer, 0, 0, BASSFlag.BASS_DEFAULT, 0);
            }
            else
            {
                stmake = Bass.BASS_StreamCreateFile(unmanagedPointer, 0, 0, BASSFlag.BASS_STREAM_DECODE);
            }

            if (stmake != 0)
            {
                //create the BASSSound object; we need to give it the unManagedPointer, so that it will be able to properly free that resource when it is destructed.
                BASSSound returnsound = new BASSSound(stmake, unmanagedPointer);
                returnsound.BASS_SoundStopped += BasssoundStop;
                return returnsound;

            }
            return null;
        }

        public iSoundSourceObject LoadSound(string filename)
        {
            String extension = Path.GetExtension(filename);
            if (extension.Equals(".xm", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Print("xm file...");


            }
            int stmake = 0;

            if (new String[] { ".xm", ".it", ".mod" }.Contains(extension.ToLower()))
            {
                stmake = Bass.BASS_MusicLoad(filename, 0, 0, BASSFlag.BASS_DEFAULT, 0);
            }
            else
            {
                stmake = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE);

            }

            if (stmake != 0)
            {
                BASSSound returnsound = new BASSSound(stmake);
                returnsound.BASS_SoundStopped += BasssoundStop;
                return returnsound;

            }
            return null;
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return SupportedExtensions.Split('|');
        }

        readonly String SupportedExtensions = null;
        #endregion

        public void Dispose()
        {
            // Bass.BASS_Free();
            Bass.FreeMe();
            BassMix.FreeMe();
            foreach (var disposeplugin in loadedbassplugs)
            {
                Bass.BASS_PluginFree(disposeplugin.Key);

            }
        }
    }
}
