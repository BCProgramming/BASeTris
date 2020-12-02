using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.AssetManager
{
    public interface iActiveSoundObject
    {
        bool Finished { get; }
        float Tempo { get; set; }
        void Stop();
        void Pause();
        void UnPause();
        bool Paused { get; set; }
        void setVolume(float volumeset);

        /// <summary>
        /// retrieves the current progress. This will be a ratio to Source.getLength.
        /// </summary>
        float Progress { get; }

        float Level { get; }
        iSoundSourceObject Source { get; }

    }

    public interface iSoundSourceObject
    {
        iActiveSoundObject Play(bool playlooped);

        iActiveSoundObject Play(bool playlooped, float volume);
        float getLength();
    }

    public delegate void OnSoundStopDelegate(iActiveSoundObject objstop);

    public delegate void OnSoundPlayDelegate(iActiveSoundObject objplay);

    public interface iSoundEngineDriver : IDisposable
    {
        event OnSoundStopDelegate OnSoundStop;
        event OnSoundPlayDelegate OnSoundPlay;
        iSoundSourceObject LoadSound(Byte[] data, String sName, String fileextension);
        iSoundSourceObject LoadSound(String filename);
        String Name { get; }
        IEnumerable<string> GetSupportedExtensions();
        String ToString();
    }


    public interface IAudioHandler
    {
        iSoundSourceObject GetPlayingMusic();
        iActiveSoundObject GetPlayingMusic_Active();
        iActiveSoundObject PlaySoundRnd(String key, float Volume);

        iActiveSoundObject PlaySound(String key, bool playlooped);
        iActiveSoundObject PlaySound(String key);

        iActiveSoundObject PlaySound(String key, float volume);
        iActiveSoundObject PlaySound(String key, bool playlooped, float volume);
        iActiveSoundObject PlayMusic();
        iActiveSoundObject PlayMusic(String key, float volume, bool loop);

        iActiveSoundObject PlayMusic(String[] key, cNewSoundManager.MultiMusicPlayMode mplaymode);
        iActiveSoundObject PlayMusic(String[] key, cNewSoundManager.MultiMusicPlayMode mplaymode, out iSoundSourceObject[] ssources);

        void PauseMusic();


        void PauseMusic(bool pausestate);



        void StopMusic();


    }
}
