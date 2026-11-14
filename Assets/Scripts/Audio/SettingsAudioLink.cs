using UnityEngine;

/// <summary>
/// Puts an AudioSource under GameSettings control (mute + volume).
/// Used for the Learning narration source.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SettingsAudioLink : MonoBehaviour
{
    public enum Channel { Music, Sfx }

    [SerializeField] private GameSettings settings;
    [SerializeField] private Channel channel = Channel.Sfx;

    private AudioSource source;

    private void OnEnable()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (settings != null) settings.Changed += Apply;
        Apply();
    }

    private void OnDisable()
    {
        if (settings != null) settings.Changed -= Apply;
    }

    private void Apply()
    {
        if (settings == null || source == null) return;

        bool on = channel == Channel.Music ? settings.MusicEnabled : settings.SfxEnabled;
        float volume = channel == Channel.Music ? settings.MusicVolume : settings.SfxVolume;

        source.mute = !on;
        source.volume = volume;
    }
}