using UnityEngine;

/// <summary>
/// Turns existing gameplay events into sound effects, without touching the
/// quiz, catch or UI scripts.
/// </summary>
public class GameAudioEvents : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CatchGameManager catchManager;
    [SerializeField] private DateSpawner spawner;

    private int lastQuizAnswered;
    private int lastQuizCorrect;

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;
        if (scoreManager != null) scoreManager.QuizScoreChanged += HandleQuizScoreChanged;
        if (spawner != null) spawner.DateCaught += HandleDateCaught;
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;
        if (scoreManager != null) scoreManager.QuizScoreChanged -= HandleQuizScoreChanged;
        if (spawner != null) spawner.DateCaught -= HandleDateCaught;
    }

    private void HandleQuizScoreChanged(int score)
    {
        // The event also fires when a quiz starts or resets; only a rise in
        // QuizAnswered means an answer was just checked.
        int answered = scoreManager.QuizAnswered;
        if (answered > lastQuizAnswered)
        {
            bool correct = scoreManager.QuizCorrect > lastQuizCorrect;
            PlaySound(correct ? SoundId.QuizCorrect : SoundId.QuizWrong);
        }
        lastQuizAnswered = answered;
        lastQuizCorrect = scoreManager.QuizCorrect;
    }

    private void HandleDateCaught(FallingDate date)
    {
        if (date == null || date.Data == null || catchManager == null) return;

        DateData target = catchManager.TargetData;
        bool correct = target != null && date.Data.Variety == target.Variety;
        PlaySound(correct ? SoundId.CatchCorrect : SoundId.CatchWrong);
    }

    private void HandleStateChanged(GameState previous, GameState next)
    {
        switch (next)
        {
            case GameState.QuizResult:
                if (scoreManager != null && scoreManager.GetQuizStars() > 0) PlaySound(SoundId.Win);
                break;

            case GameState.CatchResult:
                if (scoreManager != null && catchManager != null &&
                    scoreManager.GetCatchStars(catchManager.CaughtCount) > 0) PlaySound(SoundId.Win);
                break;

            case GameState.FinalResult:
                PlaySound(SoundId.Win);
                break;
        }
    }

    private static void PlaySound(SoundId id)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(id);
    }
}