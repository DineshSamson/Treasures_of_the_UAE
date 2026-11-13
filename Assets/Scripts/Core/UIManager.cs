using System.Collections;
using UnityEngine;

/// <summary>
/// Shows exactly one screen panel at a time, with an optional fade transition.
/// Contains no game logic; GameManager tells it which state to show.
/// </summary>
public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public GameState state;
        public CanvasGroup panel;
        [Tooltip("Show the shared Home/Settings/Sound overlay on this screen.")]
        public bool showCommonOverlay = true;
    }

    [Header("Panels")]
    [SerializeField] private PanelEntry[] panels;

    [Header("Shared UI")]
    [Tooltip("Home / Settings / Sound buttons shown on most screens. Optional.")]
    [SerializeField] private GameObject commonOverlay;

    [Header("Fade Transition (optional)")]
    [Tooltip("Full-screen CanvasGroup with a black Image child. Leave empty for instant switching.")]
    [SerializeField] private CanvasGroup fadeGroup;
    [Tooltip("Duration of the fade OUT plus the fade IN, each at this length.")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;

    private PanelEntry current;
    private Coroutine transitionRoutine;

    public void ShowPanel(GameState state, bool animate = true)
    {
        PanelEntry target = FindEntry(state);
        if (target == null || target.panel == null)
        {
            Debug.LogWarning($"UIManager: no panel assigned for state '{state}'.", this);
            return;
        }

        // Cancel any running transition so rapid clicks can't stack coroutines.
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        bool canAnimate = animate && fadeGroup != null && current != null && isActiveAndEnabled;
        if (canAnimate)
        {
            transitionRoutine = StartCoroutine(TransitionRoutine(target));
        }
        else
        {
            SwitchImmediate(target);
            ResetFade();
        }
    }

    private IEnumerator TransitionRoutine(PanelEntry target)
    {
        yield return Fade(1f);
        SwitchImmediate(target);
        yield return Fade(0f);
        transitionRoutine = null;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        // Block clicks while fading so the player can't double-press buttons.
        fadeGroup.blocksRaycasts = true;

        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // works even if timeScale = 0 (pause)
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
        fadeGroup.blocksRaycasts = targetAlpha > 0f;
    }

    private void SwitchImmediate(PanelEntry target)
    {
        if (panels != null)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                PanelEntry entry = panels[i];
                if (entry == null || entry.panel == null) continue;

                bool isTarget = entry == target;
                entry.panel.gameObject.SetActive(isTarget);
                if (isTarget)
                {
                    entry.panel.alpha = 1f;
                    entry.panel.interactable = true;
                    entry.panel.blocksRaycasts = true;
                }
            }
        }

        if (commonOverlay != null)
        {
            commonOverlay.SetActive(target.showCommonOverlay);
        }

        current = target;
    }

    private PanelEntry FindEntry(GameState state)
    {
        if (panels == null) return null;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && panels[i].state == state) return panels[i];
        }
        return null;
    }

    private void ResetFade()
    {
        if (fadeGroup == null) return;
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }

    private void OnDisable()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }
        ResetFade();
    }
}