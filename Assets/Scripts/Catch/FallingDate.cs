using UnityEngine;

/// <summary>
/// One falling date. It does NOT have its own Update: DateSpawner moves every
/// active date in a single loop. It only reports when the player touches it.
/// </summary>
public class FallingDate : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField] private SpriteRenderer spriteRenderer;

    private Transform cachedTransform;
    private DateSpawner owner;
    private float missY;

    public DateData Data { get; private set; }
    public int LaneIndex { get; private set; }
    public float Speed { get; private set; }
    public bool IsActive { get; private set; }

    public float Y
    {
        get
        {
            CacheReferences();
            return cachedTransform.position.y;
        }
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (cachedTransform == null) cachedTransform = transform;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Launch(DateSpawner spawner, DateData data, float speed, int lane, Vector3 startPosition, float bottomY)
    {
        CacheReferences();

        owner = spawner;
        Data = data;
        Speed = speed;
        LaneIndex = lane;
        missY = bottomY;
        IsActive = true;

        cachedTransform.position = startPosition;
        if (spriteRenderer != null) spriteRenderer.sprite = data.IconSprite;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Moves down. Returns true when the date has fallen past the bottom.</summary>
    public bool Tick(float deltaTime)
    {
        Vector3 position = cachedTransform.position;
        position.y -= Speed * deltaTime;
        cachedTransform.position = position;
        return position.y < missY;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive || owner == null) return;
        if (!other.CompareTag(PlayerTag)) return;

        owner.NotifyCaught(this);
    }
}