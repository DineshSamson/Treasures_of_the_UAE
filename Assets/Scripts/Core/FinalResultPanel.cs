using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Final screen: total score, quiz and catch scores, stars and trophy.
/// Play Again and Home both reset the session's scores and progress.
/// </summary>
public class FinalResultPanel : MonoBehaviour
{
    public enum PlayAgainDestination { MainMenu, Learning, Quiz }

    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;

    [Header("Texts")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text quizScoreText;
    [SerializeField] private TMP_Text catchScoreText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Rewards")]
    [SerializeField] private Image[] stars;
    [SerializeField] private Color earnedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
    [Tooltip("Shown only when all stars are earned.")]
    [SerializeField] private GameObject trophy;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private PlayAgainDestination playAgainGoesTo = PlayAgainDestination.Quiz;

    [Header("Animation")]
    [SerializeField, Min(0.1f)] private float countUpDuration = 1.2f;
    [SerializeField, Min(0.05f)] private float starDelay = 0.3f;

    [Header("Messages (index = stars earned, 0 to 3)")]
    [SerializeField]
    private string[] subtitles =
    {
        "You completed the game!",
        "You completed the game!",
        "Great job, explorer!",
        "Amazing! You are a date expert!"
    };

    private Coroutine routine;
    private WaitForSeconds starWait;

    private void Awake()
    {
        starWait = new WaitForSeconds(starDelay);
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;

        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgain);
        if (homeButton != null) homeButton.onClick.AddListener(OnHome);

        Refresh();
    }

    private void OnDisable()
    {
        if (playAgainButton != null) playAgainButton.onClick.RemoveListener(OnPlayAgain);
        if (homeButton != null) homeButton.onClick.RemoveListener(OnHome);

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void Refresh()
    {
        if (scoreManager == null) return;

        int starCount = scoreManager.GetFinalStars();

        if (quizScoreText != null) quizScoreText.text = $"Quiz: {scoreManager.QuizScore}";
        if (catchScoreText != null) catchScoreText.text = $"Catch: {scoreManager.CatchScore}";
        if (subtitleText != null && subtitles != null && subtitles.Length > 0)
            subtitleText.text = subtitles[Mathf.Clamp(starCount, 0, subtitles.Length - 1)];

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimateRoutine(scoreManager.TotalScore, starCount));
    }

    private IEnumerator AnimateRoutine(int total, int earned)
    {
        // Locked stars show immediately; earned stars pop in one by one.
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                stars[i].color = i < earned ? earnedColor : lockedColor;
                stars[i].gameObject.SetActive(i >= earned);
            }
        }
        if (trophy != null) trophy.SetActive(false);

        // Score count-up (text is only rewritten when the number changes).
        float elapsed = 0f;
        int shown = -1;
        while (elapsed < countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int value = Mathf.RoundToInt(Mathf.Lerp(0f, total, elapsed / countUpDuration));
            if (value != shown)
            {
                shown = value;
                if (finalScoreText != null) finalScoreText.text = value.ToString();
            }
            yield return null;
        }
        if (finalScoreText != null) finalScoreText.text = total.ToString();

        if (stars != null)
        {
            for (int i = 0; i < earned && i < stars.Length; i++)
            {
                yield return starWait;
                if (stars[i] != null) stars[i].gameObject.SetActive(true);
            }
        }

        if (trophy != null && stars != null && earned >= stars.Length && stars.Length > 0)
        {
            yield return starWait;
            trophy.SetActive(true);
        }

        routine = null;
    }

    private void ResetSession()
    {
        if (scoreManager != null) scoreManager.ResetAll();
        if (gameManager != null) gameManager.ResetProgress();
    }

    private void OnPlayAgain()
    {
        if (gameManager == null) return;

        ResetSession();
        switch (playAgainGoesTo)
        {
            case PlayAgainDestination.Learning: gameManager.OpenLearning(); break;
            case PlayAgainDestination.Quiz: gameManager.OpenQuiz(); break;
            default: gameManager.GoHome(); break;
        }
    }

    private void OnHome()
    {
        if (gameManager == null) return;

        ResetSession();
        gameManager.GoHome();
    }
}