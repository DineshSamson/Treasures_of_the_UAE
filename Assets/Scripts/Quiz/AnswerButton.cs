using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One answer option. QuizManager sets its text and reads its clicks.
/// </summary>
public class AnswerButton : MonoBehaviour
{
    public enum VisualState { Normal, Selected, Correct, Wrong, Dimmed }

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image background;
    [Tooltip("Optional small check/cross image on the button.")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private Sprite correctIconSprite;
    [SerializeField] private Sprite wrongIconSprite;

    [Header("Look")]
    [SerializeField] private Color correctColor = new Color(0.25f, 0.8f, 0.35f);
    [SerializeField] private Color wrongColor = new Color(0.95f, 0.4f, 0.35f);
    [SerializeField, Range(1f, 1.3f)] private float selectedScale = 1.06f;

    private Color baseColor = Color.white;
    private Vector3 baseScale = Vector3.one;
    private bool initialised;
    private Action<AnswerButton> onClicked;

    public int Index { get; private set; }

    /// <summary>When true, clicks are ignored (after an answer is checked).</summary>
    public bool Locked { get; set; }

    private void Awake()
    {
        EnsureInitialised();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(HandleClick);
    }

    // Safe to call before Awake (the panel may still be inactive).
    private void EnsureInitialised()
    {
        if (initialised) return;
        initialised = true;

        if (button == null) button = GetComponent<Button>();
        if (background == null) background = GetComponent<Image>();

        if (background != null) baseColor = background.color;
        baseScale = transform.localScale;

        if (button != null) button.onClick.AddListener(HandleClick);
    }

    public void Setup(int index, string text, Action<AnswerButton> clickCallback)
    {
        EnsureInitialised();
        Index = index;
        onClicked = clickCallback;
        Locked = false;
        if (label != null) label.text = text;

        gameObject.SetActive(true);
        SetState(VisualState.Normal);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetState(VisualState state)
    {
        EnsureInitialised();

        transform.localScale = baseScale;
        if (background != null) background.color = baseColor;
        if (resultIcon != null) resultIcon.gameObject.SetActive(false);

        switch (state)
        {
            case VisualState.Selected:
                transform.localScale = baseScale * selectedScale;
                break;

            case VisualState.Correct:
                if (background != null) background.color = correctColor;
                ShowIcon(correctIconSprite);
                break;

            case VisualState.Wrong:
                if (background != null) background.color = wrongColor;
                ShowIcon(wrongIconSprite);
                break;

            case VisualState.Dimmed:
                if (background != null) background.color = Color.Lerp(baseColor, Color.gray, 0.5f);
                break;
        }
    }

    private void ShowIcon(Sprite sprite)
    {
        if (resultIcon == null || sprite == null) return;
        resultIcon.sprite = sprite;
        resultIcon.gameObject.SetActive(true);
    }

    private void HandleClick()
    {
        if (Locked) return;
        onClicked?.Invoke(this);
    }
}