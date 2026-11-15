using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runs the image quiz: builds a no-repeat question order, builds four
/// randomized options per question, validates answers and shows feedback.
/// </summary>
public class QuizManager : MonoBehaviour
{
    [Header("Data & Flow")]
    [SerializeField] private DateDatabase database;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField, Min(1)] private int questionsPerQuiz = 6;
    [Tooltip("Pause after checking so the player sees green/red on the buttons first.")]
    [SerializeField, Min(0f)] private float feedbackDelay = 0.7f;

    [Header("Question UI")]
    [SerializeField] private Image questionImage;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private AnswerButton[] answerButtons;
    [SerializeField] private Button checkAnswerButton;

    [Header("Feedback Overlay")]
    [SerializeField] private GameObject feedbackRoot;
    [Tooltip("Optional: tinted green/orange per result.")]
    [SerializeField] private Image feedbackCardImage;
    [SerializeField] private Image feedbackIcon;
    [SerializeField] private Sprite correctIconSprite;
    [SerializeField] private Sprite wrongIconSprite;
    [SerializeField] private Image feedbackCharacter;
    [SerializeField] private Sprite happyCharacterSprite;
    [SerializeField] private Sprite oopsCharacterSprite;
    [SerializeField] private TMP_Text feedbackTitleText;
    [SerializeField] private TMP_Text feedbackMessageText;
    [SerializeField] private TMP_Text pointsEarnedText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Feedback Look & Messages")]
    [SerializeField] private Color correctCardColor = new Color(0.85f, 1f, 0.85f);
    [SerializeField] private Color wrongCardColor = new Color(1f, 0.92f, 0.82f);
    [SerializeField] private string[] correctTitles = { "Correct!", "Great job!", "You got it!" };
    [SerializeField] private string[] wrongTitles = { "Nice try!", "Almost there!", "Good try!" };

    // ---- Runtime state ----
    private int[] questionOrder;
    private int currentQuestion;
    private DateData correctData;
    private int optionCount;
    private int correctButtonIndex;
    private int selectedIndex = -1;
    private bool answered;

    private readonly DateData[] optionData = new DateData[8];
    private readonly List<int> indexPool = new List<int>(16);

    private WaitForSeconds feedbackWait;
    private Coroutine feedbackRoutine;
    private System.Action<AnswerButton> answerCallback;

    private void Awake()
    {
        feedbackWait = new WaitForSeconds(feedbackDelay);
        answerCallback = OnAnswerClicked; // cached so we don't allocate per question
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;

        if (checkAnswerButton != null) checkAnswerButton.onClick.AddListener(CheckAnswer);
        if (nextButton != null) nextButton.onClick.AddListener(NextQuestion);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;

        if (checkAnswerButton != null) checkAnswerButton.onClick.RemoveListener(CheckAnswer);
        if (nextButton != null) nextButton.onClick.RemoveListener(NextQuestion);

        StopFeedbackRoutine();
    }

    private void HandleStateChanged(GameState previous, GameState next)
    {
        if (next == GameState.Quiz)
        {
            StartQuiz();
        }
        else if (previous == GameState.Quiz)
        {
            // Left mid-quiz (Home) or finished: clean up.
            StopFeedbackRoutine();
            HideFeedback();
        }
    }

    // ---- Quiz lifecycle ----

    private void StartQuiz()
    {
        if (database == null || database.Count == 0)
        {
            Debug.LogWarning("QuizManager: DateDatabase is missing or empty.", this);
            return;
        }

        StopFeedbackRoutine();
        HideFeedback();
        BuildQuestionOrder();
        currentQuestion = 0;

        if (scoreManager != null) scoreManager.BeginQuiz(questionOrder.Length);
        ShowQuestion();
    }

    /// <summary>
    /// Shuffled passes through the database, so a variety never repeats until
    /// all have been asked. Avoids the same variety twice in a row across passes.
    /// </summary>
    private void BuildQuestionOrder()
    {
        int dbCount = database.Count;
        int total = Mathf.Max(1, questionsPerQuiz);
        questionOrder = new int[total];

        int filled = 0;
        int last = -1;
        while (filled < total)
        {
            indexPool.Clear();
            for (int i = 0; i < dbCount; i++) indexPool.Add(i);
            Shuffle(indexPool);

            if (dbCount > 1 && indexPool[0] == last)
            {
                int swapWith = indexPool.Count - 1;
                int temp = indexPool[0];
                indexPool[0] = indexPool[swapWith];
                indexPool[swapWith] = temp;
            }

            for (int i = 0; i < indexPool.Count && filled < total; i++)
            {
                questionOrder[filled++] = indexPool[i];
                last = indexPool[i];
            }
        }
    }

    private void ShowQuestion()
    {
        answered = false;
        selectedIndex = -1;

        int dbIndex = questionOrder[currentQuestion];
        correctData = database.GetAt(dbIndex);
        if (correctData == null)
        {
            Debug.LogWarning($"QuizManager: no DateData at index {dbIndex}.", this);
            return;
        }

        BuildOptions(dbIndex);

        if (questionImage != null)
        {
            questionImage.sprite = correctData.DisplaySprite;
            questionImage.preserveAspect = true;
            questionImage.enabled = correctData.DisplaySprite != null;
        }

        if (progressText != null) progressText.text = $"Question {currentQuestion + 1}/{questionOrder.Length}";
        UpdateScoreText();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            AnswerButton button = answerButtons[i];
            if (button == null) continue;

            if (i < optionCount) button.Setup(i, optionData[i].DisplayName, answerCallback);
            else button.Hide();
        }

        if (checkAnswerButton != null) checkAnswerButton.interactable = false;
    }

    private void BuildOptions(int correctDbIndex)
    {
        optionCount = Mathf.Min(Mathf.Min(answerButtons.Length, optionData.Length), database.Count);

        indexPool.Clear();
        for (int i = 0; i < database.Count; i++)
        {
            if (i != correctDbIndex) indexPool.Add(i);
        }

        optionData[0] = correctData;
        for (int k = 1; k < optionCount; k++)
        {
            int pick = Random.Range(0, indexPool.Count);
            optionData[k] = database.GetAt(indexPool[pick]);
            indexPool.RemoveAt(pick);
        }

        // Shuffle option positions (Fisher-Yates).
        for (int i = optionCount - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            DateData temp = optionData[i];
            optionData[i] = optionData[j];
            optionData[j] = temp;
        }

        correctButtonIndex = 0;
        for (int i = 0; i < optionCount; i++)
        {
            if (optionData[i] == correctData)
            {
                correctButtonIndex = i;
                break;
            }
        }
    }

    // ---- Player actions ----

    private void OnAnswerClicked(AnswerButton button)
    {
        if (answered) return;

        if (selectedIndex >= 0 && selectedIndex < answerButtons.Length && answerButtons[selectedIndex] != null)
        {
            answerButtons[selectedIndex].SetState(AnswerButton.VisualState.Normal);
        }

        selectedIndex = button.Index;
        button.SetState(AnswerButton.VisualState.Selected);

        if (checkAnswerButton != null) checkAnswerButton.interactable = true;
    }

    private void CheckAnswer()
    {
        if (answered || selectedIndex < 0) return;
        answered = true;

        if (checkAnswerButton != null) checkAnswerButton.interactable = false;

        bool correct = selectedIndex == correctButtonIndex;
        int points = scoreManager != null ? scoreManager.RegisterQuizAnswer(correct) : 0;

        for (int i = 0; i < optionCount; i++)
        {
            AnswerButton button = answerButtons[i];
            if (button == null) continue;

            button.Locked = true;
            if (i == correctButtonIndex) button.SetState(AnswerButton.VisualState.Correct);
            else if (i == selectedIndex) button.SetState(AnswerButton.VisualState.Wrong);
            else button.SetState(AnswerButton.VisualState.Dimmed);
        }

        UpdateScoreText();

        StopFeedbackRoutine();
        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(correct, points));
    }

    private void NextQuestion()
    {
        if (!answered) return;

        StopFeedbackRoutine();
        HideFeedback();

        currentQuestion++;
        if (currentQuestion >= questionOrder.Length) FinishQuiz();
        else ShowQuestion();
    }

    private void FinishQuiz()
    {
        if (gameManager == null) return;

        gameManager.MarkQuizCompleted();
        gameManager.SetState(GameState.QuizResult);
    }

    // ---- Feedback ----

    private IEnumerator ShowFeedbackRoutine(bool correct, int points)
    {
        if (feedbackDelay > 0f) yield return feedbackWait;

        ShowFeedback(correct, points);
        feedbackRoutine = null;
    }

    private void ShowFeedback(bool correct, int points)
    {
        if (feedbackRoot != null) feedbackRoot.SetActive(true);

        if (feedbackIcon != null)
        {
            Sprite icon = correct ? correctIconSprite : wrongIconSprite;
            feedbackIcon.gameObject.SetActive(icon != null);
            feedbackIcon.sprite = icon;
        }

        if (feedbackCharacter != null)
        {
            Sprite face = correct ? happyCharacterSprite : oopsCharacterSprite;
            if (face != null) feedbackCharacter.sprite = face;
        }

        //if (feedbackCardImage != null) feedbackCardImage.color = correct ? correctCardColor : wrongCardColor;

        if (feedbackTitleText != null) feedbackTitleText.text = PickRandom(correct ? correctTitles : wrongTitles);

        if (feedbackMessageText != null)
        {
            feedbackMessageText.text = correct
                ? correctData.QuizFeedback
                : $"It was {correctData.DisplayName}. {correctData.QuizFeedback}";
        }

        if (pointsEarnedText != null)
        {
            //pointsEarnedText.gameObject.SetActive(correct && points > 0);
            pointsEarnedText.text = $"Score: +{points}";
        }

        if (nextButtonLabel != null)
        {
            bool isLast = currentQuestion >= questionOrder.Length - 1;
            nextButtonLabel.text = isLast ? "See Results" : "Next";
        }
    }

    private void HideFeedback()
    {
        if (feedbackRoot != null) feedbackRoot.SetActive(false);
    }

    private void StopFeedbackRoutine()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }
    }

    // ---- Helpers ----

    private void UpdateScoreText()
    {
        if (scoreText != null && scoreManager != null) scoreText.text = $"Score: {scoreManager.QuizScore}";
    }

    private static string PickRandom(string[] options)
    {
        if (options == null || options.Length == 0) return string.Empty;
        return options[Random.Range(0, options.Length)];
    }

    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}