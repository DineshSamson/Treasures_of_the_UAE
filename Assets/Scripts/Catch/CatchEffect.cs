using TMPro;
using UnityEngine;

/// <summary>
/// One pooled catch effect: a floating label ("+10" / "Oops!") and an optional
/// star burst. Moved by CatchFeedback (no Update of its own).
/// </summary>
public class CatchEffect : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;
    [SerializeField] private ParticleSystem stars;
    [SerializeField, Min(0.2f)] private float lifetime = 0.9f;
    [SerializeField] private float riseDistance = 1.3f;
    [SerializeField, Range(0f, 1f)] private float fadeStart = 0.55f;
    [SerializeField, Min(1f)] private float popScale = 1.25f;

    private Transform cachedTransform;
    private Vector3 startPosition;
    private float elapsed;

    private void Awake()
    {
        cachedTransform = transform;
    }

    public void Play(Vector3 position, string text, Color color, bool withStars)
    {
        if (cachedTransform == null) cachedTransform = transform;

        elapsed = 0f;
        startPosition = position;
        cachedTransform.position = position;
        cachedTransform.localScale = Vector3.one * 0.5f;

        if (label != null)
        {
            label.text = text;
            label.color = color;
        }

        if (stars != null)
        {
            stars.gameObject.SetActive(withStars);
            if (withStars)
            {
                stars.Clear();
                stars.Play();
            }
        }
    }

    /// <summary>Advances the animation. Returns true when finished.</summary>
    public bool Tick(float deltaTime)
    {
        elapsed += deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        // Rise with ease-out.
        float rise = 1f - (1f - t) * (1f - t);
        Vector3 position = startPosition;
        position.y += riseDistance * rise;
        cachedTransform.position = position;

        // Quick pop in, then settle.
        float scale = t < 0.2f
            ? Mathf.Lerp(0.5f, popScale, t / 0.2f)
            : Mathf.Lerp(popScale, 1f, Mathf.Clamp01((t - 0.2f) / 0.3f));
        cachedTransform.localScale = Vector3.one * scale;

        // Fade out near the end.
        if (label != null)
        {
            float alpha = t < fadeStart ? 1f : 1f - (t - fadeStart) / (1f - fadeStart);
            label.alpha = alpha;
        }

        return t >= 1f;
    }
}