using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.AssetManager
{
    public interface IActiveSound
    {
        bool Finished { get; }
        float Tempo { get; set; }
        float Pitch { get; set; }
        void Stop();
        void Pause();
        void UnPause();
        bool Paused { get; set; }
        void SetVolume(float volumeset);

        /// <summary>
        /// retrieves the current progress. This will be a ratio to Source.getLength.
        /// </summary>
        float Progress { get; }

        float Level { get; }
        ISoundSource Source { get; }

    }

    public interface ISoundSource
    {
        IActiveSound Play(bool playlooped);

        IActiveSound Play(bool playlooped, float volume, float tempo = 1f, float pitch = 0f);
        //float getLength();
    }

    public delegate void OnSoundStopDelegate(IActiveSound objstop);

    public delegate void OnSoundPlayDelegate(IActiveSound objplay);

    public interface ISoundEngineDriver : IDisposable
    {
        event OnSoundStopDelegate OnSoundStop;
        event OnSoundPlayDelegate OnSoundPlay;
        ISoundSource LoadSound(Byte[] data, String sName, String fileextension);
        ISoundSource LoadSound(String filename);
        String Name { get; }
        IEnumerable<string> GetSupportedExtensions();
        String ToString();
    }


    public interface IAudioHandler
    {
        ISoundSource GetPlayingMusic();
        IActiveSound GetPlayingMusic_Active();
        IActiveSound PlaySoundRnd(String key, float Volume);

        IActiveSound PlaySound(String key, bool playlooped);
        IActiveSound PlaySound(String key);

        IActiveSound PlaySound(String key, float volume);
        IActiveSound PlaySound(String key, bool playlooped, float volume);
        IActiveSound PlaySound(String key, AudioHandlerPlayDetails pDetails);
        IActiveSound PlayMusic();
        IActiveSound PlayMusic(String key, float volume, bool loop);

        IActiveSound PlayMusic(String key, AudioHandlerPlayDetails pDetails);
        IActiveSound PlayMusic(String[] key, cNewSoundManager.MultiMusicPlayMode mplaymode);
        IActiveSound PlayMusic(String[] key, cNewSoundManager.MultiMusicPlayMode mplaymode, out ISoundSource[] ssources);


        void PauseMusic();


        void PauseMusic(bool pausestate);



        void StopMusic();


    }
    public class AudioHandlerPlayDetails
    {
        
        public float Volume { get; set; } = 1f;
        public bool Playlooped { get; set; } = false;
        public float Pitch { get; set; } = 0f;
        public float Tempo { get; set; } = 1f;
        public AudioHandlerPlayDetails()
        {
        }
        public AudioHandlerPlayDetails(float pVolume,bool pLooped,float pPitch = 0f, float pTempo = 1f)
        {
            Volume = pVolume;
            Playlooped = pLooped;
            Pitch = pPitch;
            Tempo = pTempo;
        }
    }
}
