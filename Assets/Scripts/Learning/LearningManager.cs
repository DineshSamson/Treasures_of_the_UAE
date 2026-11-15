using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the reusable Learning panel from DateDatabase.
/// Button listeners are added/removed in code, so no OnClick setup is needed.
/// </summary>
public class LearningManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DateDatabase database;

    [Header("Flow")]
    [SerializeField] private GameManager gameManager;
    [Tooltip("On: Finish goes to the Quiz. Off: Finish returns to the Main Menu.")]
    [SerializeField] private bool openQuizOnFinish = true;

    [Header("Header")]
    [SerializeField] private TMP_Text dateNameText;
    [SerializeField] private TMP_Text progressText;

    [Header("Info Card")]
    [SerializeField] private Image dateImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private DetailRow originRow;
    [SerializeField] private DetailRow appearanceRow;
    [SerializeField] private DetailRow textureRow;
    [SerializeField] private DetailRow tasteRow;
    [SerializeField] private TMP_Text funFactText;
    [SerializeField] private TMP_Text nutritionText;
    [SerializeField] private Transform benefitsContainer;
    [Tooltip("Prefab with a TMP_Text somewhere inside (plus a check icon).")]
    [SerializeField] private GameObject benefitItemPrefab;

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button finishButton;
    [SerializeField] private Button readAloudButton;

    [Header("Narration (optional)")]
    [SerializeField] private AudioSource narrationSource;

    private readonly List<TMP_Text> benefitTexts = new List<TMP_Text>();
    private DateData currentData;
    private int currentIndex;

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;

        if (previousButton != null) previousButton.onClick.AddListener(ShowPrevious);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNext);
        if (finishButton != null) finishButton.onClick.AddListener(FinishLearning);
        if (readAloudButton != null) readAloudButton.onClick.AddListener(ReadAloud);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;

        if (previousButton != null) previousButton.onClick.RemoveListener(ShowPrevious);
        if (nextButton != null) nextButton.onClick.RemoveListener(ShowNext);
        if (finishButton != null) finishButton.onClick.RemoveListener(FinishLearning);
        if (readAloudButton != null) readAloudButton.onClick.RemoveListener(ReadAloud);

        StopNarration();
    }

    private void HandleStateChanged(GameState previous, GameState next)
    {
        if (next == GameState.Learning)
        {
            Show(0);
        }
        else if (previous == GameState.Learning)
        {
            StopNarration();
        }
    }

    // ---- Navigation ----

    public void ShowNext() => Show(currentIndex + 1);
    public void ShowPrevious() => Show(currentIndex - 1);

    public void FinishLearning()
    {
        StopNarration();
        if (gameManager == null) return;

        gameManager.MarkLearningCompleted();
        if (openQuizOnFinish) gameManager.OpenQuiz();
        else gameManager.GoHome();
    }

    // ---- Display ----

    private void Show(int index)
    {
        if (database == null || database.Count == 0)
        {
            Debug.LogWarning("LearningManager: DateDatabase is missing or empty.", this);
            return;
        }

        currentIndex = Mathf.Clamp(index, 0, database.Count - 1);
        currentData = database.GetAt(currentIndex);
        if (currentData == null)
        {
            Debug.LogWarning($"LearningManager: no DateData at index {currentIndex}.", this);
            return;
        }

        StopNarration();

        if (dateNameText != null) dateNameText.text = currentData.DisplayName;
        if (progressText != null) progressText.text = $"{currentIndex + 1}/{database.Count}";
        if (descriptionText != null) descriptionText.text = currentData.Description;
        if (funFactText != null) funFactText.text = currentData.FunFact;
        if (nutritionText != null) nutritionText.text = currentData.NutritionNote;

        if (dateImage != null)
        {
            dateImage.sprite = currentData.DisplaySprite;
            dateImage.preserveAspect = true;
            dateImage.enabled = currentData.DisplaySprite != null;
        }

        if (originRow != null) originRow.Set("Origin", currentData.Origin);
        if (appearanceRow != null) appearanceRow.Set("Looks", currentData.Appearance);
        if (textureRow != null) textureRow.Set("Feels", currentData.Texture);
        if (tasteRow != null) tasteRow.Set("Tastes", currentData.Taste);

        SetBenefits(currentData.Benefits);
        UpdateButtons();
    }

    private void SetBenefits(string[] benefits)
    {
        int needed = benefits != null ? benefits.Length : 0;

        // Reuse existing rows; create new ones only when we need more than we have.
        while (benefitTexts.Count < needed && benefitItemPrefab != null && benefitsContainer != null)
        {
            GameObject row = Instantiate(benefitItemPrefab, benefitsContainer);
            TMP_Text text = row.GetComponentInChildren<TMP_Text>(true);
            if (text == null)
            {
                Debug.LogError("LearningManager: Benefit prefab needs a TMP_Text inside.", benefitItemPrefab);
                Destroy(row);
                break;
            }
            benefitTexts.Add(text);
        }

        for (int i = 0; i < benefitTexts.Count; i++)
        {
            bool active = i < needed;
            // Activate the whole row (the text's parent holds the check icon).
            Transform row = benefitTexts[i].transform.parent == benefitsContainer
                ? benefitTexts[i].transform
                : benefitTexts[i].transform.parent;
            row.gameObject.SetActive(active);
            if (active) benefitTexts[i].text = benefits[i];
        }
    }

    private void UpdateButtons()
    {
        bool isFirst = currentIndex <= 0;
        bool isLast = currentIndex >= database.Count - 1;

        if (previousButton != null) previousButton.interactable = !isFirst;
        if (nextButton != null) nextButton.gameObject.SetActive(!isLast);
        if (finishButton != null) finishButton.gameObject.SetActive(isLast);
        if (readAloudButton != null)
        {
            readAloudButton.interactable = currentData != null && currentData.NarrationClip != null;
        }
    }

    // ---- Narration ----

    private void ReadAloud()
    {
        if (narrationSource == null || currentData == null || currentData.NarrationClip == null) return;

        narrationSource.Stop();
        narrationSource.clip = currentData.NarrationClip;
        narrationSource.Play();
    }

    private void StopNarration()
    {
        if (narrationSource != null && narrationSource.isPlaying) narrationSource.Stop();
    }
}