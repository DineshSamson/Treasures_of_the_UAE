using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls one catch round: countdown, timer, scoring, pause, restart, finish.
/// The target variety is configurable, so future rounds can ask for any date.
/// </summary>
public class CatchGameManager : MonoBehaviour
{
    public enum RoundState { Idle, Countdown, Playing, Paused, Finished }

    [Header("References")]
    [SerializeField] private DateDatabase database;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DateSpawner spawner;
    [SerializeField] private PlayerController player;
    [SerializeField] private CatchFeedback feedback;

    [Header("Round Settings")]
    [SerializeField] private DateVariety targetVariety = DateVariety.Ajwa;
    [Tooltip("Pick a random target each round instead of the one above.")]
    [SerializeField] private bool randomizeTargetEachRound = false;
    [SerializeField, Min(5f)] private float roundDuration = 45f;
    [SerializeField, Min(1)] private int pointsPerCatch = 10;
    [Tooltip("3-2-1-GO countdown length. 0 = start immediately.")]
    [SerializeField, Min(0)] private int countdownSeconds = 3;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text targetNameText;
    [SerializeField] private TMP_Text targetHintText;
    [SerializeField] private TMP_Text countdownText;

    [Header("Pause")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;

    public RoundState State { get; private set; } = RoundState.Idle;
    public DateData TargetData { get; private set; }
    public int CaughtCount { get; private set; }
    public int WrongCount { get; private set; }

    private DateVariety activeTarget;
    private float timeRemaining;
    private int shownSecond = -1;
    private Coroutine countdownRoutine;
    private WaitForSeconds oneSecond;
    private WaitForSeconds goPause;

    private void Awake()
    {
        oneSecond = new WaitForSeconds(1f);
        goPause = new WaitForSeconds(0.5f);
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;

        if (spawner != null) spawner.DateCaught += HandleDateCaught;
        if (scoreManager != null) scoreManager.CatchScoreChanged += HandleScoreChanged;

        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (homeButton != null) homeButton.onClick.AddListener(GoHome);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;

        if (spawner != null) spawner.DateCaught -= HandleDateCaught;
        if (scoreManager != null) scoreManager.CatchScoreChanged -= HandleScoreChanged;

        if (pauseButton != null) pauseButton.onClick.RemoveListener(Pause);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        if (homeButton != null) homeButton.onClick.RemoveListener(GoHome);

        StopCountdown();
    }

    private void HandleStateChanged(GameState previous, GameState next)
    {
        if (next == GameState.Catch) BeginRound();
        else if (previous == GameState.Catch) EndSession();
    }

    // ---- Round lifecycle ----

    private void BeginRound()
    {
        if (database == null || database.Count == 0 || spawner == null || player == null)
        {
            Debug.LogWarning("CatchGameManager: missing database, spawner or player reference.", this);
            return;
        }

        StopCountdown();
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (feedback != null) feedback.ClearAll();

        activeTarget = randomizeTargetEachRound
            ? database.GetAt(Random.Range(0, database.Count)).Variety
            : targetVariety;

        TargetData = database.Get(activeTarget);
        if (TargetData == null)
        {
            Debug.LogWarning($"CatchGameManager: no DateData for '{activeTarget}'.", this);
            return;
        }

        if (scoreManager != null) scoreManager.ResetCatch();
        CaughtCount = 0;
        WrongCount = 0;

        timeRemaining = roundDuration;
        shownSecond = -1;

        UpdateTargetUI();
        UpdateScoreText();
        UpdateTimerText(Mathf.CeilToInt(timeRemaining));

        spawner.PrepareRound(activeTarget);
        player.ResetToCenter();
        player.SetControlEnabled(false);
        if (pauseButton != null) pauseButton.interactable = false;

        countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        State = RoundState.Countdown;

        if (countdownText != null && countdownSeconds > 0)
        {
            countdownText.gameObject.SetActive(true);
            for (int s = countdownSeconds; s > 0; s--)
            {
                countdownText.text = s.ToString();
                yield return oneSecond;
            }
            countdownText.text = "GO!";
            yield return goPause;
            countdownText.gameObject.SetActive(false);
        }

        countdownRoutine = null;
        StartPlaying();
    }

    private void StartPlaying()
    {
        State = RoundState.Playing;
        player.SetControlEnabled(true);
        spawner.SetDifficulty(0f);
        spawner.StartSpawning();
        if (pauseButton != null) pauseButton.interactable = true;
    }

    private void Update()
    {
        if (State != RoundState.Playing) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText(0);
            FinishRound();
            return;
        }

        spawner.SetDifficulty(1f - timeRemaining / roundDuration);

        // Only touch the text when the displayed second changes (no per-frame UI rebuilds).
        int second = Mathf.CeilToInt(timeRemaining);
        if (second != shownSecond) UpdateTimerText(second);
    }

    private void FinishRound()
    {
        State = RoundState.Finished;

        spawner.ClearAll();
        if (feedback != null) feedback.ClearAll();
        player.SetControlEnabled(false);
        if (pauseButton != null) pauseButton.interactable = false;

        if (gameManager != null)
        {
            gameManager.MarkCatchCompleted();
            gameManager.SetState(GameState.CatchResult);
        }
    }

    /// <summary>Cleans up when leaving the Catch screen for any reason.</summary>
    private void EndSession()
    {
        StopCountdown();
        if (spawner != null) spawner.ClearAll();
        if (feedback != null) feedback.ClearAll();
        if (player != null) player.SetControlEnabled(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        State = RoundState.Idle;
    }

    // ---- Pause / restart / home ----

    private void Pause()
    {
        if (State != RoundState.Playing) return;

        State = RoundState.Paused;
        Time.timeScale = 0f;
        player.SetControlEnabled(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void Resume()
    {
        if (State != RoundState.Paused) return;

        Time.timeScale = 1f;
        State = RoundState.Playing;
        player.SetControlEnabled(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Restart()
    {
        if (State != RoundState.Paused && State != RoundState.Playing) return;

        spawner.ClearAll();
        BeginRound();
    }

    private void GoHome()
    {
        if (gameManager != null) gameManager.GoHome();
    }

    // ---- Scoring and feedback ----

    private void HandleDateCaught(FallingDate date)
    {
        if (State != RoundState.Playing || date == null || date.Data == null) return;

        // The date is already back in the pool, but its position is still where it was caught.
        Vector3 position = date.transform.position;

        if (date.Data.Variety == activeTarget)
        {
            CaughtCount++;
            if (scoreManager != null) scoreManager.AddCatchPoints(pointsPerCatch);
            if (feedback != null) feedback.PlayCorrect(position, pointsPerCatch);
            player.PlayReaction(true);
            // Phase 7: positive catch sound.
        }
        else
        {
            WrongCount++; // no penalty
            if (feedback != null) feedback.PlayWrong(position);
            player.PlayReaction(false);
            // Phase 7: soft "oops" sound.
        }
    }

    private void HandleScoreChanged(int score)
    {
        UpdateScoreText();
    }

    // ---- HUD ----

    private void UpdateTargetUI()
    {
        if (targetImage != null)
        {
            targetImage.sprite = TargetData.DisplaySprite;
            targetImage.preserveAspect = true;
            targetImage.enabled = TargetData.DisplaySprite != null;
        }
        if (targetNameText != null) targetNameText.text = TargetData.DisplayName.ToUpperInvariant();
        if (targetHintText != null) targetHintText.text = $"Catch only {TargetData.DisplayName} dates!";
    }

    private void UpdateScoreText()
    {
        if (scoreText != null && scoreManager != null) scoreText.text = $"Score: {scoreManager.CatchScore}";
    }

    private void UpdateTimerText(int seconds)
    {
        shownSecond = seconds;
        if (timerText != null) timerText.text = $"{seconds}s";
    }

    private void StopCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
    }

    /// <summary>For future rounds: change the target variety from code.</summary>
    public void SetTarget(DateVariety variety)
    {
        targetVariety = variety;
    }
}