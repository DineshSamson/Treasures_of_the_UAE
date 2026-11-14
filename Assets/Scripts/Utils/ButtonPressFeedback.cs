using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Quick "squish" when a button is pressed. Uses unscaled time so it also
/// works on the Pause menu. Added automatically by UIButtonFeedbackInstaller.
/// </summary>
public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField, Range(0.8f, 1f)] private float pressedScale = 0.93f;
    [SerializeField, Min(0.01f)] private float duration = 0.08f;

    private Transform cachedTransform;
    private Button button;
    private Vector3 baseScale = Vector3.one;
    private bool cached;
    private Coroutine routine;

    private void CacheReferences()
    {
        if (cached) return;
        cached = true;
        cachedTransform = transform;
        button = GetComponent<Button>();
        baseScale = cachedTransform.localScale;
    }

    private void OnEnable()
    {
        CacheReferences();
        cachedTransform.localScale = baseScale;
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        if (cached) cachedTransform.localScale = baseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        AnimateTo(baseScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData) { AnimateTo(baseScale); }
    public void OnPointerExit(PointerEventData eventData) { AnimateTo(baseScale); }

    private void AnimateTo(Vector3 target)
    {
        if (!isActiveAndEnabled) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ScaleRoutine(target));
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = cachedTransform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cachedTransform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        cachedTransform.localScale = target;
        routine = null;
    }
}