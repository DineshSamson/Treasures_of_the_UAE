using System;
using UnityEngine;

/// <summary>
/// Audio settings. Toggles are saved with PlayerPrefs (stored in the browser
/// on WebGL), so they survive a page reload.
/// </summary>
public class GameSettings : MonoBehaviour
{
    private const string MusicKey = "tou_music_enabled";
    private const string SfxKey = "tou_sfx_enabled";

    [Header("Default volumes (not exposed to the player)")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    public bool MusicEnabled { get; private set; } = true;
    public bool SfxEnabled { get; private set; } = true;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    /// <summary>Raised after any setting changes.</summary>
    public event Action Changed;

    private void Awake()
    {
        MusicEnabled = PlayerPrefs.GetInt(MusicKey, 1) == 1;
        SfxEnabled = PlayerPrefs.GetInt(SfxKey, 1) == 1;
    }

    public void SetMusicEnabled(bool value)
    {
        if (value == MusicEnabled) return;
        MusicEnabled = value;
        Commit();
    }

    public void SetSfxEnabled(bool value)
    {
        if (value == SfxEnabled) return;
        SfxEnabled = value;
        Commit();
    }

    /// <summary>Master switch used by the quick speaker button.</summary>
    public void SetAllAudio(bool value)
    {
        if (value == MusicEnabled && value == SfxEnabled) return;
        MusicEnabled = value;
        SfxEnabled = value;
        Commit();
    }

    private void Commit()
    {
        PlayerPrefs.SetInt(MusicKey, MusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(SfxKey, SfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}