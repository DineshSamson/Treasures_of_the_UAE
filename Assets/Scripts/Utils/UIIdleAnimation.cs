using UnityEngine;

/// <summary>
/// Gentle looping animation for UI: Bob (up/down), Rotate (spin) or Pulse (scale).
/// Do NOT put it on objects driven by a Layout Group.
/// </summary>
public class UIIdleAnimation : MonoBehaviour
{
    public enum Mode { Bob, Rotate, Pulse }

    [SerializeField] private Mode mode = Mode.Bob;
    [Tooltip("Bob/Pulse: cycles speed. Rotate: degrees per second.")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Bob: pixels. Pulse: scale change (0.05 = 5%). Unused for Rotate.")]
    [SerializeField] private float amount = 10f;

    private RectTransform rect;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private float time;

    private void OnEnable()
    {
        if (rect == null) rect = transform as RectTransform;
        basePosition = rect.anchoredPosition;
        baseScale = rect.localScale;
        baseRotation = rect.localRotation;
        time = 0f;
    }

    private void OnDisable()
    {
        if (rect == null) return;
        rect.anchoredPosition = basePosition;
        rect.localScale = baseScale;
        rect.localRotation = baseRotation;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        time += dt;

        switch (mode)
        {
            case Mode.Bob:
                rect.anchoredPosition = basePosition + Vector2.up * (Mathf.Sin(time * speed) * amount);
                break;
            case Mode.Rotate:
                rect.Rotate(0f, 0f, -speed * dt);
                break;
            case Mode.Pulse:
                rect.localScale = baseScale * (1f + Mathf.Sin(time * speed) * amount);
                break;
        }
    }
}