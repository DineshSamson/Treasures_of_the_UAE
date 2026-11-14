using UnityEngine;

public class ShineSweep : MonoBehaviour
{
    [SerializeField] Vector3 startPos = new Vector3(-6f, -3f, 0f);
    [SerializeField] Vector3 endPos = new Vector3(6f, 3f, 0f);
    [SerializeField] float duration = 1.2f;
    [SerializeField] float pause = 2.5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        transform.localPosition = Vector3.Lerp(startPos, endPos, t);
        if (timer >= duration + pause) timer = 0f;
    }
}