using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-tap speaker button: turns all audio on or off.
/// With no sprites assigned, it dims the icon when audio is off.
/// </summary>
public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(Toggle);
        if (settings != null) settings.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(Toggle);
        if (settings != null) settings.Changed -= Refresh;
    }

    private void Toggle()
    {
        if (settings == null) return;
        bool anyOn = settings.MusicEnabled || settings.SfxEnabled;
        settings.SetAllAudio(!anyOn);
    }

    private void Refresh()
    {
        if (settings == null || icon == null) return;

        bool anyOn = settings.MusicEnabled || settings.SfxEnabled;
        if (onSprite != null && offSprite != null) icon.sprite = anyOn ? onSprite : offSprite;
        else icon.color = anyOn ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }
}