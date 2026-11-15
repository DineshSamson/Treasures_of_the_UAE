using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fills the quiz result screen each time it is shown.
/// </summary>
public class QuizResultPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;

    [Header("Texts")]
    [SerializeField] private TMP_Text correctCountText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text messageText;

    [Header("Stars")]
    [SerializeField] private Image[] stars;
    [SerializeField] private Color earnedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button retryButton;

    [Header("Messages (index = stars earned, 0 to 3)")]
    [SerializeField]
    private string[] messages =
    {
        "Keep learning, you can do it!",
        "Good try! Let's learn more.",
        "Great job!",
        "Amazing! You know your dates!"
    };

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;

        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);

        Refresh();
    }

    private void OnDisable()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinue);
        if (retryButton != null) retryButton.onClick.RemoveListener(OnRetry);
    }

    private void Refresh()
    {
        if (scoreManager == null) return;

        int starCount = scoreManager.GetQuizStars();

        if (correctCountText != null)
            correctCountText.text = $"Correct: {scoreManager.QuizCorrect} / {scoreManager.QuizQuestionTotal}";
        if (pointsText != null)
            pointsText.text = $"{scoreManager.QuizScore} points";

        if (messageText != null && messages != null && messages.Length > 0)
            messageText.text = messages[Mathf.Clamp(starCount, 0, messages.Length - 1)];

        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null) stars[i].color = i < starCount ? earnedColor : lockedColor;
            }
        }
    }

    private void OnContinue()
    {
        if (gameManager != null) gameManager.OpenCatch();
    }

    private void OnRetry()
    {
        if (gameManager != null) gameManager.OpenQuiz();
    }
}