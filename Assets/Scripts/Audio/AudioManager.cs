using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays music and sound effects. SFX use a small pool of AudioSources created
/// once at startup. Missing clips are ignored, so the game also runs silently.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class SoundEntry
    {
        public SoundId id;
        [Tooltip("One clip, or several for random variety.")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Random pitch range. (1,1) = no variation.")]
        public Vector2 pitchRange = new Vector2(1f, 1f);
    }

    public static AudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameSettings settings;
    [SerializeField] private GameManager gameManager;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip catchMusic;

    [Header("Sound effects")]
    [SerializeField] private SoundEntry[] sounds;
    [SerializeField, Range(2, 12)] private int sfxVoices = 6;

    private AudioSource musicSource;
    private AudioSource[] sfxSources;
    private int nextSource;
    private readonly Dictionary<int, SoundEntry> lookup = new Dictionary<int, SoundEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        sfxSources = new AudioSource[sfxVoices];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            sfxSources[i] = source;
        }

        if (sounds != null)
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i] != null && sounds[i].id != SoundId.None)
                {
                    lookup[(int)sounds[i].id] = sounds[i];
                }
            }
        }
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;
        if (settings != null) settings.Changed += ApplySettings;
        ApplySettings();
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;
        if (settings != null) settings.Changed -= ApplySettings;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---- Music ----

    private void HandleStateChanged(GameState previous, GameState next)
    {
        switch (next)
        {
            case GameState.Splash:
                break; 

            case GameState.Catch:
                PlayMusic(catchMusic != null ? catchMusic : menuMusic);
                break;

            default:
                PlayMusic(menuMusic);
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        bool enabled = settings == null || settings.MusicEnabled;
        if (musicSource.clip == clip && (musicSource.isPlaying || !enabled)) return; 

        musicSource.clip = clip;
        musicSource.volume = settings != null ? settings.MusicVolume : 0.05f;
        if (enabled) musicSource.Play();
    }

    private void ApplySettings()
    {
        if (musicSource == null) return;

        bool musicOn = settings == null || settings.MusicEnabled;
        musicSource.volume = settings != null ? settings.MusicVolume : 0.05f;

        if (!musicOn)
        {
            if (musicSource.isPlaying) musicSource.Stop();
        }
        else if (musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // ---- Sound effects ----

    public void Play(SoundId id)
    {
        if (id == SoundId.None || sfxSources == null) return;
        if (settings != null && !settings.SfxEnabled) return;
        if (!lookup.TryGetValue((int)id, out SoundEntry entry)) return;
        if (entry.clips == null || entry.clips.Length == 0) return;

        AudioClip clip = entry.clips.Length == 1
            ? entry.clips[0]
            : entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
        if (clip == null) return;

        AudioSource source = sfxSources[nextSource];
        nextSource = (nextSource + 1) % sfxSources.Length;

        source.clip = clip;
        source.pitch = UnityEngine.Random.Range(entry.pitchRange.x, entry.pitchRange.y);
        source.volume = entry.volume * (settings != null ? settings.SfxVolume : 1f);
        source.Play();
    }
}