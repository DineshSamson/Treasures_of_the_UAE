using System;
using UnityEngine;

/// <summary>
/// Tracks quiz and catch scores, calculates the final score and star ratings.
/// Holds data only; other managers push values in and UI reads them out.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Points")]
    [SerializeField, Min(0)] private int pointsPerCorrectAnswer = 10;

    [Header("Star ratings (ascending: 1 star, 2 stars, 3 stars)")]
    [Tooltip("Dates caught in one round needed for 1, 2 and 3 stars.")]
    [SerializeField] private int[] catchStarThresholds = { 3, 6, 10 };
    [Tooltip("Total score (quiz + catch) needed for 1, 2 and 3 stars.")]
    [SerializeField] private int[] finalStarThresholds = { 30, 90, 150 };

    public int QuizScore { get; private set; }
    public int CatchScore { get; private set; }
    public int QuizCorrect { get; private set; }
    public int QuizAnswered { get; private set; }
    public int QuizQuestionTotal { get; private set; }

    public int TotalScore => QuizScore + CatchScore;

    public event Action<int> QuizScoreChanged;
    public event Action<int> CatchScoreChanged;

    // ---- Quiz ----

    public void BeginQuiz(int totalQuestions)
    {
        QuizScore = 0;
        QuizCorrect = 0;
        QuizAnswered = 0;
        QuizQuestionTotal = Mathf.Max(0, totalQuestions);
        QuizScoreChanged?.Invoke(QuizScore);
    }

    /// <summary>Registers an answer and returns the points awarded.</summary>
    public int RegisterQuizAnswer(bool correct)
    {
        QuizAnswered++;
        int awarded = 0;
        if (correct)
        {
            QuizCorrect++;
            awarded = pointsPerCorrectAnswer;
            QuizScore += awarded;
        }
        QuizScoreChanged?.Invoke(QuizScore);
        return awarded;
    }

    /// <summary>0 to 3 stars based on the share of correct quiz answers.</summary>
    public int GetQuizStars()
    {
        if (QuizQuestionTotal <= 0 || QuizCorrect <= 0) return 0;

        float ratio = (float)QuizCorrect / QuizQuestionTotal;
        if (ratio >= 0.8f) return 3;
        if (ratio >= 0.5f) return 2;
        return 1;
    }

    // ---- Catch ----

    public void ResetCatch()
    {
        CatchScore = 0;
        CatchScoreChanged?.Invoke(CatchScore);
    }

    public void AddCatchPoints(int points)
    {
        if (points <= 0) return;
        CatchScore += points;
        CatchScoreChanged?.Invoke(CatchScore);
    }

    /// <summary>0 to 3 stars based on how many target dates were caught.</summary>
    public int GetCatchStars(int caughtCount)
    {
        return CountStars(caughtCount, catchStarThresholds);
    }

    /// <summary>0 to 3 stars based on the combined quiz + catch score.</summary>
    public int GetFinalStars()
    {
        return CountStars(TotalScore, finalStarThresholds);
    }

    // ---- Everything ----

    public void ResetAll()
    {
        QuizScore = 0;
        QuizCorrect = 0;
        QuizAnswered = 0;
        QuizQuestionTotal = 0;
        CatchScore = 0;
        QuizScoreChanged?.Invoke(QuizScore);
        CatchScoreChanged?.Invoke(CatchScore);
    }

    private static int CountStars(int value, int[] thresholds)
    {
        if (thresholds == null) return 0;

        int stars = 0;
        for (int i = 0; i < thresholds.Length; i++)
        {
            if (thresholds[i] > 0 && value >= thresholds[i]) stars = i + 1;
        }
        return Mathf.Min(stars, 3);
    }
}