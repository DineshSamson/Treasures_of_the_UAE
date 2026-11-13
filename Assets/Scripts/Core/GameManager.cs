using System;
using UnityEngine;

/// <summary>
/// Owns the current game state and progression flags.
/// Delegates all UI work to UIManager. Later phases hook into OnStateChanged.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [Tooltip("Root of the world-space catch gameplay. Active only in the Catch state. Optional for now.")]
    [SerializeField] private GameObject gameplayWorld;

    [Header("Startup")]
    [SerializeField] private GameState startState = GameState.Splash;

    public GameState CurrentState { get; private set; } = GameState.None;

    // Progression flags (revisiting completed sections is always allowed).
    public bool LearningCompleted { get; private set; }
    public bool QuizCompleted { get; private set; }
    public bool CatchCompleted { get; private set; }

    /// <summary>Args: previous state, new state.</summary>
    public event Action<GameState, GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetState(startState, false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetState(GameState newState, bool animate = true)
    {
        if (newState == CurrentState) return;

        GameState previous = CurrentState;
        CurrentState = newState;

        // Always restore normal time on a state change (safe with pausing later).
        Time.timeScale = 1f;

        if (gameplayWorld != null)
        {
            gameplayWorld.SetActive(newState == GameState.Catch);
        }

        if (uiManager != null)
        {
            uiManager.ShowPanel(newState, animate);
        }
        else
        {
            Debug.LogError("GameManager: UIManager reference is missing.", this);
        }

        OnStateChanged?.Invoke(previous, newState);
    }

    // ---- Public methods for UI Button OnClick events ----

    public void StartGame() => SetState(GameState.MainMenu);
    public void GoHome() => SetState(GameState.MainMenu);
    public void OpenLearning() => SetState(GameState.Learning);
    public void OpenQuiz() => SetState(GameState.Quiz);
    public void OpenCatch() => SetState(GameState.Catch);

    // ---- Progression hooks (called by later phases) ----

    public void MarkLearningCompleted() => LearningCompleted = true;
    public void MarkQuizCompleted() => QuizCompleted = true;
    public void MarkCatchCompleted() => CatchCompleted = true;

    public void ResetProgress()
    {
        LearningCompleted = false;
        QuizCompleted = false;
        CatchCompleted = false;
    }
}