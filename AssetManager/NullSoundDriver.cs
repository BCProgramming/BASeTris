using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.AssetManager
{
    public class NullSound : ISoundEngineDriver
    {
        #region iSoundEngineDriver implementation
        public event OnSoundStopDelegate OnSoundStop;


        public event OnSoundPlayDelegate OnSoundPlay;
        public NullSound()
        {


        }

        public NullSound(String pluginfolder)
        {
            //do nothing.

        }

        public ISoundSource LoadSound(byte[] data, String sName, string fileextension)
        {
            return new NullSoundObject(sName);
        }

        public ISoundSource LoadSound(string filename)
        {
            return new NullSoundObject(filename);
        }


        public IEnumerable<string> GetSupportedExtensions()
        {

            return new String[] { ".MP3", ".WAV", ".OGG", ".FLAC", ".MIDI", ".RMI" };
        }

        public static string DrvName = "NULL";
        public override string ToString()
        {
            return Name;
        }
        public string Name
        {
            get
            {
                return "NULL";
            }
        }

        #endregion
        public class NullSoundObject : ISoundSource, IActiveSound
        {
            string nullsoundfile = "";

            #region iSoundSourceObject implementation
            public NullSoundObject(String soundfile)
            {
                nullsoundfile = soundfile;


            }
            public IActiveSound Play(bool playlooped)
            {
                return this;
            }


            public IActiveSound Play(bool playlooped, float volume, float tempo = 1f, float pitch = 0f)
            {
                return this;
            }

            #endregion
            #region iActiveSoundObject implementation
            public float Progress { get { return 0; } }
            public float Tempo { get { return 1.0f; } set { } }
            public float Pitch { get { return 1.0f; } set { } }
            public ISoundSource Source
            {
                get { return this; }
            }

            public void Stop()
            {

            }
            public float getLength()
            {
                return 0;

            }

            public void Pause()
            {

            }


            public void UnPause()
            {

            }


            public bool Finished
            {
                get
                {
                    return true;
                }
            }


            public bool Paused
            {
                get
                {
                    return false;
                }
                set
                {
                    //
                }
            }

            public float Level { get { return 1.0f; } set { } }

            public void SetVolume(float volumeset)
            {
                //
            }

            #endregion


        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
