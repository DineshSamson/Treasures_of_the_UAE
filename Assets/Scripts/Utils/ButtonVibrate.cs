
using UnityEngine;

public class ButtonVibrate : MonoBehaviour
{
    public float duration = 2f;
    public float strength = 8f;
    public float frequency = 35f;

    private LTDescr shakeTween;
    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;

        Vibrate();
    }

    public void Vibrate()
    {
        //if (shakeTween != null)
        //    shakeTween.canc

        originalPosition = transform.localPosition;

        shakeTween = LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate((float value) =>
            {
                float amplitude = strength * (1f - value);
                float offset = Mathf.Sin(Time.time * frequency) * amplitude;

                transform.localPosition =
                    originalPosition + Vector3.right * offset;
            })
            .setOnComplete(() =>
            {
                transform.localPosition = originalPosition;
                shakeTween = null;
            });
    }
}
