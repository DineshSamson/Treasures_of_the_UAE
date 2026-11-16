using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Three-lane player movement. Keyboard (A/D, Left/Right) and mouse/touch
/// (press a lane to move there). Actions are built in code, so no
/// .inputactions asset is needed. Also plays a small reaction on catches.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Lanes")]
    [Tooltip("Same lane markers the spawner uses (left, centre, right).")]
    [SerializeField] private Transform[] laneMarkers;

    [Header("Movement")]
    [SerializeField, Min(1f)] private float moveSpeed = 20f;

    [Header("Pointer")]
    [SerializeField] private Camera worldCamera;

    [Header("Reaction")]
    [Tooltip("Child object holding the character/basket sprite.")]
    [SerializeField] private Transform visual;
    [SerializeField, Min(0.05f)] private float reactionDuration = 0.25f;
    [SerializeField, Range(0f, 0.5f)] private float positivePop = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float wrongShake = 0.12f;

    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction pointerAction;

    private Transform cachedTransform;
    private int laneIndex;
    private float targetX;
    private bool controlEnabled;
    private bool pointerPending;
    private Vector2 pointerScreenPosition;

    private float reactionTimer;
    private bool reactionPositive;
    private Vector3 visualBaseScale = Vector3.one;
    private Vector3 visualBaseLocalPosition;

    private bool wasMoving;
    public Animator animator;

    private void Awake()
    {
        cachedTransform = transform;
        if (worldCamera == null) worldCamera = Camera.main;

        if (visual != null)
        {
            visualBaseScale = visual.localScale;
            visualBaseLocalPosition = visual.localPosition;
        }
        reactionTimer = reactionDuration; // no reaction playing

        leftAction = new InputAction("Left", InputActionType.Button);
        leftAction.AddBinding("<Keyboard>/leftArrow");
        leftAction.AddBinding("<Keyboard>/a");

        rightAction = new InputAction("Right", InputActionType.Button);
        rightAction.AddBinding("<Keyboard>/rightArrow");
        rightAction.AddBinding("<Keyboard>/d");

        // <Pointer>/press covers mouse and touch.
        pointerAction = new InputAction("Pointer", InputActionType.Button, "<Pointer>/press");
    }

    private void OnEnable()
    {
        leftAction.performed += OnLeft;
        rightAction.performed += OnRight;
        pointerAction.performed += OnPointer;

        leftAction.Enable();
        rightAction.Enable();
        pointerAction.Enable();
    }

    private void OnDisable()
    {
        leftAction.performed -= OnLeft;
        rightAction.performed -= OnRight;
        pointerAction.performed -= OnPointer;

        leftAction.Disable();
        rightAction.Disable();
        pointerAction.Disable();

        pointerPending = false;
        StopReaction();
    }

    private void OnDestroy()
    {
        leftAction?.Dispose();
        rightAction?.Dispose();
        pointerAction?.Dispose();
    }

    // ---- Public API ----

    public void SetControlEnabled(bool value)
    {
        controlEnabled = value;
        if (!value) pointerPending = false;
    }

    /// <summary>Snaps to the centre lane (used at the start of each round).</summary>
    public void ResetToCenter()
    {
        if (cachedTransform == null) cachedTransform = transform;
        StopReaction();

        if (laneMarkers == null || laneMarkers.Length == 0) return;

        laneIndex = laneMarkers.Length / 2;
        targetX = laneMarkers[laneIndex].position.x;

        Vector3 position = cachedTransform.position;
        position.x = targetX;
        cachedTransform.position = position;
    }

    /// <summary>Small pop on a good catch, gentle wobble on a wrong one.</summary>
    public void PlayReaction(bool positive)
    {
        if (visual == null) return;

        reactionPositive = positive;
        reactionTimer = 0f;
    }

    // ---- Input callbacks ----

    private void OnLeft(InputAction.CallbackContext context) { MoveLane(-1); }
    private void OnRight(InputAction.CallbackContext context) { MoveLane(1); }

    private void OnPointer(InputAction.CallbackContext context)
    {
        if (!controlEnabled) return;

        if (context.control.device is Pointer pointer)
        {
            // Handled in Update: IsPointerOverGameObject must not be called inside a callback.
            pointerScreenPosition = pointer.position.ReadValue();
            pointerPending = true;
        }
    }

    // ---- Movement ----

    public void MoveLane(int delta)
    {
        //if (!controlEnabled) return;

        //animator.SetFloat("MoveX", delta);

        //SetLane(laneIndex + delta);

        if (!controlEnabled) return;
        if (laneMarkers == null || laneMarkers.Length == 0) return;

        int newIndex = laneIndex + delta;

        // At the edge: A in the left lane, or D in the right lane, does nothing.
        if (newIndex < 0 || newIndex >= laneMarkers.Length) return;

        animator.SetFloat("MoveX", delta);

        SetLane(newIndex);
    }

    private void SetLane(int index)
    {
        if (laneMarkers == null || laneMarkers.Length == 0) return;

        laneIndex = Mathf.Clamp(index, 0, laneMarkers.Length - 1);
        targetX = laneMarkers[laneIndex].position.x;
    }

    private void Update()
    {
        if (pointerPending)
        {
            pointerPending = false;
            HandlePointer();
        }

        Vector3 position = cachedTransform.position;

        bool isMoving = !Mathf.Approximately(position.x, targetX);

        if (isMoving)
        {
            position.x = Mathf.MoveTowards(position.x, targetX, moveSpeed * Time.deltaTime);
            cachedTransform.position = position;
        }

        if (!isMoving && wasMoving)
        {
            animator.SetFloat("MoveX", 0);         // runs once when the player arrives
        }

        wasMoving = isMoving;

        UpdateReaction(Time.deltaTime);
    }

    private void UpdateReaction(float deltaTime)
    {
        if (visual == null || reactionTimer >= reactionDuration) return;

        reactionTimer += deltaTime;
        float t = Mathf.Clamp01(reactionTimer / reactionDuration);

        if (reactionPositive)
        {
            float scale = 1f + positivePop * Mathf.Sin(t * Mathf.PI);
            visual.localScale = visualBaseScale * scale;
        }
        else
        {
            float x = Mathf.Sin(t * Mathf.PI * 4f) * wrongShake * (1f - t);
            visual.localPosition = visualBaseLocalPosition + new Vector3(x, 0f, 0f);
        }

        if (t >= 1f) StopReaction();
    }

    private void StopReaction()
    {
        reactionTimer = reactionDuration;
        if (visual != null)
        {
            visual.localScale = visualBaseScale;
            visual.localPosition = visualBaseLocalPosition;
        }
    }

    private void HandlePointer()
    {
        if (!controlEnabled || worldCamera == null || laneMarkers == null) return;

        // Ignore presses that landed on a UI button (e.g. Pause).
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        float worldX = worldCamera.ScreenToWorldPoint(pointerScreenPosition).x;

        int nearest = 0;
        float best = float.MaxValue;
        for (int i = 0; i < laneMarkers.Length; i++)
        {
            float distance = Mathf.Abs(laneMarkers[i].position.x - worldX);
            if (distance < best)
            {
                best = distance;
                nearest = i;
            }
        }
        SetLane(nearest);
    }
}