using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays pooled catch effects at a world position and animates them in one loop.
/// </summary>
public class CatchFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler pooler;
    [SerializeField] private string poolKey = "CatchEffect";

    [Header("Look")]
    [SerializeField] private Color correctColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color wrongColor = new Color(1f, 0.6f, 0.3f, 1f);
    [SerializeField] private string wrongText = "Oops!";
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, 0f);

    private readonly List<CatchEffect> active = new List<CatchEffect>(8);
    private string correctText = "+10";
    private int cachedPoints = 10;

    private void Awake()
    {
        enabled = false; // runs only while an effect is playing
    }

    public void PlayCorrect(Vector3 position, int points)
    {
        if (points != cachedPoints)
        {
            cachedPoints = points;
            correctText = "+" + points; // rebuilt only if the value changes
        }
        Spawn(position, correctText, correctColor, true);
    }

    public void PlayWrong(Vector3 position)
    {
        Spawn(position, wrongText, wrongColor, false);
    }

    private void Spawn(Vector3 position, string text, Color color, bool withStars)
    {
        if (pooler == null) return;

        GameObject instance = pooler.Get(poolKey);
        if (instance == null) return;

        CatchEffect effect = instance.GetComponent<CatchEffect>();
        if (effect == null)
        {
            Debug.LogError("CatchFeedback: pooled prefab has no CatchEffect component.", instance);
            pooler.Release(poolKey, instance);
            return;
        }

        effect.Play(position + offset, text, color, withStars);
        active.Add(effect);
        enabled = true;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            CatchEffect effect = active[i];
            if (effect.Tick(dt))
            {
                active.RemoveAt(i);
                pooler.Release(poolKey, effect.gameObject);
            }
        }

        if (active.Count == 0) enabled = false;
    }

    public void ClearAll()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (pooler != null) pooler.Release(poolKey, active[i].gameObject);
        }
        active.Clear();
        enabled = false;
    }
}