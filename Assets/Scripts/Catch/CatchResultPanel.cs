using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Great Catch!" screen. Reusable for any target date: it reads the target
/// and caught count from CatchGameManager.
/// </summary>
public class CatchResultPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CatchGameManager catchManager;
    [SerializeField] private GameManager gameManager;

    [Header("UI")]
    [SerializeField] private TMP_Text ribbonText;
    [SerializeField] private Image caughtDateImage;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button replayButton;

    [Header("Stars")]
    [SerializeField] private Image[] stars;
    [SerializeField] private Color earnedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);

    [Header("Animation")]
    [SerializeField, Min(0.1f)] private float countUpDuration = 0.8f;
    [SerializeField, Min(0.05f)] private float starDelay = 0.3f;

    [Header("Ribbon titles (index = stars earned, 0 to 3)")]
    [SerializeField] private string[] ribbonTitles = { "Nice Try!", "Good Catch!", "Great Catch!", "Amazing Catch!" };

    private Coroutine routine;
    private WaitForSeconds starWait;

    private void Awake()
    {
        starWait = new WaitForSeconds(starDelay);
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = GameManager.Instance;

        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (replayButton != null) replayButton.onClick.AddListener(OnReplay);

        Refresh();
    }

    private void OnDisable()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinue);
        if (replayButton != null) replayButton.onClick.RemoveListener(OnReplay);

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void Refresh()
    {
        if (scoreManager == null || catchManager == null) return;

        DateData target = catchManager.TargetData;
        int caught = catchManager.CaughtCount;
        int starCount = scoreManager.GetCatchStars(caught);

        if (ribbonText != null && ribbonTitles != null && ribbonTitles.Length > 0)
            ribbonText.text = ribbonTitles[Mathf.Clamp(starCount, 0, ribbonTitles.Length - 1)];

        if (caughtDateImage != null)
        {
            caughtDateImage.sprite = target != null ? target.DisplaySprite : null;
            caughtDateImage.preserveAspect = true;
            caughtDateImage.enabled = caughtDateImage.sprite != null;
        }

        if (summaryText != null && target != null)
        {
            summaryText.text = caught == 0
                ? "Keep practising. You'll catch more next time!"
                : $"You caught {caught} {target.DisplayName} {(caught == 1 ? "date" : "dates")}!";
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimateRoutine(scoreManager.CatchScore, starCount));
    }

    private IEnumerator AnimateRoutine(int score, int earned)
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

        float elapsed = 0f;
        int shown = -1;
        while (elapsed < countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int value = Mathf.RoundToInt(Mathf.Lerp(0f, score, elapsed / countUpDuration));
            if (value != shown)
            {
                shown = value;
                if (pointsText != null) pointsText.text = $"+{value} Points";
            }
            yield return null;
        }
        if (pointsText != null) pointsText.text = $"+{score} Points";

        if (stars != null)
        {
            for (int i = 0; i < earned && i < stars.Length; i++)
            {
                yield return starWait;
                if (stars[i] != null) stars[i].gameObject.SetActive(true);
            }
        }

        routine = null;
    }

    private void OnContinue()
    {
        if (gameManager != null) gameManager.SetState(GameState.FinalResult);
    }

    private void OnReplay()
    {
        if (gameManager != null) gameManager.OpenCatch();
    }
}