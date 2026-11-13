using UnityEngine;

/// <summary>
/// Refreshes the Main Menu each time it is shown (e.g. completion ticks).
/// </summary>
public class MainMenuPanel : MonoBehaviour
{
    [Header("Optional completion icons (checkmarks)")]
    [SerializeField] private GameObject learnDoneIcon;
    [SerializeField] private GameObject quizDoneIcon;
    [SerializeField] private GameObject catchDoneIcon;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        GameManager gm = GameManager.Instance;
        bool learn = gm != null && gm.LearningCompleted;
        bool quiz = gm != null && gm.QuizCompleted;
        bool catchDone = gm != null && gm.CatchCompleted;

        if (learnDoneIcon != null) learnDoneIcon.SetActive(learn);
        if (quizDoneIcon != null) quizDoneIcon.SetActive(quiz);
        if (catchDoneIcon != null) catchDoneIcon.SetActive(catchDone);
    }
}