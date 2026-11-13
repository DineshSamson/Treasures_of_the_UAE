using System;
using UnityEngine;

/// <summary>
/// Tracks quiz and catch scores and calculates the final score.
/// Holds data only; other managers push values in and UI reads them out.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Points")]
    [SerializeField, Min(0)] private int pointsPerCorrectAnswer = 10;

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

    /// <summary>0 to 3 stars based on the share of correct answers.</summary>
    public int GetQuizStars()
    {
        if (QuizQuestionTotal <= 0 || QuizCorrect <= 0) return 0;

        float ratio = (float)QuizCorrect / QuizQuestionTotal;
        if (ratio >= 0.8f) return 3;
        if (ratio >= 0.5f) return 2;
        return 1;
    }

    // ---- Catch (used in Phase 5) ----

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

    // ---- Everything (used by "Play Again" in Phase 6) ----

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
}