using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct GameAudio
{
    public enum AudioType
    {
        SFX,
        Music,
        Ambient,
    }

    public AudioClip Clip;
    public AudioType Type;
    public string Name;
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    string _currentSongName = "";

    [SerializeField] bool _playMusicOnLoop = true;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _sfxSource;
    [SerializeField] GameAudio[] _soundsLibrary;

    public delegate void SongChangedDelegate(GameAudio song);
    public static event SongChangedDelegate OnSongChanged;

    public float MusicVolume { get { return GetChannelVolume("MusicVolume"); } set { SetChannelVolume("MusicVolume", value); } }
    public float MusicCutoff { get { return GetChannelCutoff("MusicCutoff"); } set { SetChannelCutoff("MusicCutoff", value); } }
    public string CurrentSongName { get { return _currentSongName; } }


    private void Awake()
    {
        Instance = this;
        CheckMusicSource();
    }

    /// <summary>
    /// Ensures that the <see cref="AudioSource"/> component used for music playback is assigned and configured on the
    /// current <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>If the music source is not already set, this method attempts to retrieve the <see
    /// cref="AudioSource"/> component from the current <see cref="GameObject"/>.  If no <see cref="AudioSource"/> is
    /// found, a warning is logged and music playback may not function as expected.  The method also sets the looping
    /// behavior of the music source based on the configured value.</remarks>
    void CheckMusicSource() {
        if (_musicSource == null) {
            _musicSource = gameObject.GetComponent<AudioSource>();
        }
        else if (gameObject.GetComponent<AudioSource>() == null)
        {
            Debug.LogWarning("AudioManager: No AudioSource component found on the GameObject. Music playback may not work correctly.");
        }

        _musicSource.loop = _playMusicOnLoop;
    }

    /// <summary>
    /// Plays the audio clip associated with the specified sound name.
    /// </summary>
    /// <remarks>If the specified name matches a sound of type SFX, the sound effect is played once. If it
    /// matches a sound of type Music, the current music track is replaced and playback begins immediately. If no sound
    /// with the specified name exists, no action is taken.</remarks>
    /// <param name="name">The name of the sound to play. This value is case-sensitive and must match the name of a sound in the library.</param>
    public void PlaySoundByName(string name)
    {
        foreach (var sound in _soundsLibrary)
        {
            if (sound.Name == name)
            {
                switch (sound.Type)
                {
                    case GameAudio.AudioType.SFX:
                        _sfxSource.PlayOneShot(sound.Clip);
                        break;
                    case GameAudio.AudioType.Music:
                        _musicSource.clip = sound.Clip;
                        _musicSource.Play();
                        _currentSongName = sound.Name;
                        OnSongChanged?.Invoke(sound);
                        break;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Plays all available music tracks in sequence.
    /// </summary>
    /// <param name="loop"><see langword="true"/> to continuously repeat the sequence of music tracks; <see langword="false"/> to play the
    /// sequence once.</param>
    public void PlayAllMusic(bool loop)
    {
        StartCoroutine(PlayAllMusicCoroutine(loop));
    }

    public void SetChannelVolume(string mixerChannel, float linearVolume) // valor 0.0 a 1.0
    {
        float volumeInDb = Mathf.Log10(Mathf.Clamp(linearVolume, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(mixerChannel, volumeInDb);
    }
    public float GetChannelVolume(string mixerChannel)
    {
        float volumeInDb;
        mixer.GetFloat(mixerChannel, out volumeInDb);
        float linearVolume = Mathf.Pow(10f, volumeInDb / 20f);
        return linearVolume;
    }
    public float GetChannelCutoff(string mixerChannel)
    {
        float cutOff;
        mixer.GetFloat(mixerChannel, out cutOff);
        return cutOff;
    }
    public void SetChannelCutoff(string mixerChannel, float value)
    {
        mixer.SetFloat(mixerChannel, value);
    }

    IEnumerator PlayAllMusicCoroutine(bool loop)
    {
        foreach (var sound in _soundsLibrary)
        {
            if (sound.Type == GameAudio.AudioType.Music)
            {
                _musicSource.clip = sound.Clip;
                _musicSource.loop = false;
                _musicSource.Play();
                _currentSongName = sound.Name;
                OnSongChanged?.Invoke(sound);
                yield return new WaitUntil(() => !_musicSource.isPlaying);
            }
        }
        if (loop) StartCoroutine(PlayAllMusicCoroutine(loop));
    }
}
