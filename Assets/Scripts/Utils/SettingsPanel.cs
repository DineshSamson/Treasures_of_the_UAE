using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings popup with Music and Sound toggles. Lives on CommonOverlay.
/// The popup always closes when the overlay is hidden.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject popup;
    [SerializeField] private Button closeButton;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;

    private void OnEnable()
    {
        if (openButton != null) openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggled);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxToggled);

        if (popup != null) popup.SetActive(false);
    }

    private void OnDisable()
    {
        if (openButton != null) openButton.onClick.RemoveListener(Open);
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (musicToggle != null) musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
        if (sfxToggle != null) sfxToggle.onValueChanged.RemoveListener(OnSfxToggled);

        if (popup != null) popup.SetActive(false);
    }

    private void Open()
    {
        if (popup == null) return;

        // Show the saved values without triggering the change events.
        if (settings != null)
        {
            if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(settings.MusicEnabled);
            if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(settings.SfxEnabled);
        }
        popup.SetActive(true);
    }

    private void Close()
    {
        if (popup != null) popup.SetActive(false);
    }

    private void OnMusicToggled(bool value)
    {
        if (settings != null) settings.SetMusicEnabled(value);
    }

    private void OnSfxToggled(bool value)
    {
        if (settings != null) settings.SetSfxEnabled(value);
    }
}