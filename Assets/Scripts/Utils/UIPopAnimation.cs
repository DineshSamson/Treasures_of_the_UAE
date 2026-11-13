using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a quick scale "pop" whenever the object becomes active.
/// Uses unscaled time, so it also works while the game is paused.
/// </summary>
public class UIPopAnimation : MonoBehaviour
{
    [SerializeField, Min(0.05f)] private float duration = 0.35f;
    [SerializeField, Range(0f, 1f)] private float startScale = 0.6f;
    [SerializeField]
    private AnimationCurve curve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.6f, 1.15f),
        new Keyframe(1f, 1f));

    private Vector3 baseScale;
    private bool cached;

    private void OnEnable()
    {
        if (!cached)
        {
            baseScale = transform.localScale;
            cached = true;
        }
        StartCoroutine(Play());
    }

    private void OnDisable()
    {
        if (cached) transform.localScale = baseScale;
    }

    private IEnumerator Play()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
            transform.localScale = baseScale * Mathf.LerpUnclamped(startScale, 1f, k);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}