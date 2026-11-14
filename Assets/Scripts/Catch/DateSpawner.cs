using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and moves falling dates. Handles lanes, random intervals/speeds,
/// difficulty, and fairness rules. Enabled only while a round is running.
/// </summary>
public class DateSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPooler pooler;
    [SerializeField] private string poolKey = "FallingDate";
    [SerializeField] private DateDatabase database;
    [Tooltip("Same lane markers the player uses (left, centre, right).")]
    [SerializeField] private Transform[] laneMarkers;

    [Header("Positions (world units)")]
    [SerializeField] private float spawnY = 6.2f;
    [SerializeField] private float missY = -6.2f;

    [Header("Difficulty: value at start -> value at end of round")]
    [SerializeField] private Vector2 startInterval = new Vector2(1.0f, 1.5f);
    [SerializeField] private Vector2 endInterval = new Vector2(0.5f, 0.9f);
    [SerializeField] private Vector2 startSpeed = new Vector2(3.0f, 4.0f);
    [SerializeField] private Vector2 endSpeed = new Vector2(5.5f, 7.0f);

    [Header("Variety")]
    [SerializeField, Range(0f, 1f)] private float targetChance = 0.4f;
    [Tooltip("Force a target date if this many non-target dates in a row have spawned.")]
    [SerializeField, Min(1)] private int maxSpawnsWithoutTarget = 4;

    [Header("Fairness")]
    [Tooltip("A lane is skipped until its last date has fallen this far from the spawn point.")]
    [SerializeField, Min(0.5f)] private float minLaneGap = 2.5f;

    public event Action<FallingDate> DateCaught;
    public event Action<FallingDate> DateMissed;

    private readonly List<FallingDate> active = new List<FallingDate>(16);
    private FallingDate[] laneLast;
    private float[] laneX;
    private int[] laneBuffer;

    private int targetIndex;
    private int spawnsSinceTarget;
    private float difficulty;
    private float spawnTimer;

    private void Awake()
    {
        enabled = false; // only runs during a round
    }

    // ---- Round control ----

    public void PrepareRound(DateVariety target)
    {
        ClearAll();

        int laneCount = laneMarkers != null ? laneMarkers.Length : 0;
        laneLast = new FallingDate[laneCount];
        laneBuffer = new int[laneCount];
        laneX = new float[laneCount];
        for (int i = 0; i < laneCount; i++) laneX[i] = laneMarkers[i].position.x;

        targetIndex = 0;
        for (int i = 0; i < database.Count; i++)
        {
            DateData data = database.GetAt(i);
            if (data != null && data.Variety == target)
            {
                targetIndex = i;
                break;
            }
        }

        difficulty = 0f;
        spawnsSinceTarget = 0;
        spawnTimer = 0.4f; // short pause before the first date
    }

    public void StartSpawning()
    {
        enabled = true;
    }

    public void StopSpawning()
    {
        enabled = false;
    }

    public void SetDifficulty(float value01)
    {
        difficulty = Mathf.Clamp01(value01);
    }

    /// <summary>Returns every active date to the pool and stops spawning.</summary>
    public void ClearAll()
    {
        enabled = false;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            FallingDate date = active[i];
            date.Deactivate();
            if (pooler != null) pooler.Release(poolKey, date.gameObject);
        }
        active.Clear();

        if (laneLast != null)
        {
            for (int i = 0; i < laneLast.Length; i++) laneLast[i] = null;
        }
    }

    // ---- Per-frame ----

    private void Update()
    {
        float dt = Time.deltaTime;

        // Move all dates (backwards so removal is safe).
        for (int i = active.Count - 1; i >= 0; i--)
        {
            FallingDate date = active[i];
            if (date.Tick(dt))
            {
                active.RemoveAt(i);
                date.Deactivate();
                pooler.Release(poolKey, date.gameObject);
                DateMissed?.Invoke(date);
            }
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            spawnTimer = TrySpawn() ? NextInterval() : 0.15f; // blocked: retry shortly
        }
    }

    private bool TrySpawn()
    {
        if (laneBuffer == null || laneBuffer.Length == 0) return false;

        // Collect lanes that are clear near the spawn point.
        int freeCount = 0;
        for (int i = 0; i < laneBuffer.Length; i++)
        {
            FallingDate last = laneLast[i];
            bool blocked = last != null && last.IsActive && last.LaneIndex == i && last.Y > spawnY - minLaneGap;
            if (!blocked) laneBuffer[freeCount++] = i;
        }
        if (freeCount == 0) return false;

        int lane = laneBuffer[UnityEngine.Random.Range(0, freeCount)];
        DateData data = PickData();
        if (data == null) return false;

        float speed = UnityEngine.Random.Range(
            Mathf.Lerp(startSpeed.x, endSpeed.x, difficulty),
            Mathf.Lerp(startSpeed.y, endSpeed.y, difficulty));

        // Never let a new date catch up with a slower one above the player in its lane.
        FallingDate previous = laneLast[lane];
        if (previous != null && previous.IsActive && previous.LaneIndex == lane)
        {
            speed = Mathf.Min(speed, previous.Speed);
        }

        GameObject instance = pooler.Get(poolKey);
        if (instance == null) return false;

        FallingDate date = instance.GetComponent<FallingDate>();
        if (date == null)
        {
            Debug.LogError("DateSpawner: pooled prefab has no FallingDate component.", instance);
            pooler.Release(poolKey, instance);
            return false;
        }

        date.Launch(this, data, speed, lane, new Vector3(laneX[lane], spawnY, 0f), missY);
        active.Add(date);
        laneLast[lane] = date;
        return true;
    }

    private DateData PickData()
    {
        int count = database.Count;
        if (count == 0) return null;

        bool forceTarget = spawnsSinceTarget >= maxSpawnsWithoutTarget;
        if (count == 1 || forceTarget || UnityEngine.Random.value < targetChance)
        {
            spawnsSinceTarget = 0;
            return database.GetAt(targetIndex);
        }

        spawnsSinceTarget++;
        int pick = UnityEngine.Random.Range(0, count - 1); // pick among the non-target dates
        if (pick >= targetIndex) pick++;
        return database.GetAt(pick);
    }

    private float NextInterval()
    {
        return UnityEngine.Random.Range(
            Mathf.Lerp(startInterval.x, endInterval.x, difficulty),
            Mathf.Lerp(startInterval.y, endInterval.y, difficulty));
    }

    // ---- Called by FallingDate on collision ----

    public void NotifyCaught(FallingDate date)
    {
        if (date == null || !date.IsActive) return;

        active.Remove(date);
        date.Deactivate();
        pooler.Release(poolKey, date.gameObject);
        DateCaught?.Invoke(date);
    }
}