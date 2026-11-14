using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic keyed object pool. Pre-creates objects so nothing is instantiated
/// or destroyed during gameplay.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [Serializable]
    public class Pool
    {
        public string key = "FallingDate";
        public GameObject prefab;
        [Min(0)] public int prewarmCount = 12;
        [Tooltip("Pooled objects are parented here.")]
        public Transform parent;
        [Tooltip("Create extra objects if the pool runs dry.")]
        public bool canGrow = true;
    }

    [SerializeField] private Pool[] pools;

    private readonly Dictionary<string, Stack<GameObject>> available = new Dictionary<string, Stack<GameObject>>();
    private readonly Dictionary<string, Pool> lookup = new Dictionary<string, Pool>();
    private bool built;

    private void Awake()
    {
        Build();
    }

    private void Build()
    {
        if (built) return;
        built = true;
        if (pools == null) return;

        for (int i = 0; i < pools.Length; i++)
        {
            Pool pool = pools[i];
            if (pool == null || pool.prefab == null || string.IsNullOrEmpty(pool.key))
            {
                Debug.LogWarning($"ObjectPooler: pool {i} is missing a key or prefab.", this);
                continue;
            }
            if (lookup.ContainsKey(pool.key))
            {
                Debug.LogWarning($"ObjectPooler: duplicate key '{pool.key}'.", this);
                continue;
            }

            lookup.Add(pool.key, pool);
            Stack<GameObject> stack = new Stack<GameObject>(Mathf.Max(4, pool.prewarmCount));
            available.Add(pool.key, stack);

            for (int n = 0; n < pool.prewarmCount; n++)
            {
                stack.Push(Create(pool));
            }
        }
    }

    private GameObject Create(Pool pool)
    {
        GameObject instance = Instantiate(pool.prefab, pool.parent);
        instance.SetActive(false);
        return instance;
    }

    /// <summary>Returns an ACTIVE pooled object, or null if the key is unknown / pool is full.</summary>
    public GameObject Get(string key)
    {
        Build();

        if (!available.TryGetValue(key, out Stack<GameObject> stack))
        {
            Debug.LogWarning($"ObjectPooler: unknown key '{key}'.", this);
            return null;
        }

        GameObject instance = null;
        while (stack.Count > 0 && instance == null)
        {
            instance = stack.Pop(); // skips anything destroyed externally
        }

        if (instance == null)
        {
            Pool pool = lookup[key];
            if (!pool.canGrow) return null;
            instance = Create(pool);
        }

        instance.SetActive(true);
        return instance;
    }

    public void Release(string key, GameObject instance)
    {
        if (instance == null) return;

        instance.SetActive(false);

        if (available.TryGetValue(key, out Stack<GameObject> stack))
        {
            Pool pool = lookup[key];
            if (pool.parent != null && instance.transform.parent != pool.parent)
            {
                instance.transform.SetParent(pool.parent, false);
            }
            stack.Push(instance);
        }
    }
}