using UnityEngine;

/// <summary>
/// One ScriptableObject per date variety. Single source of truth for
/// the Learning, Quiz and Catch sections.
/// </summary>
[CreateAssetMenu(fileName = "Date_New", menuName = "Treasures of the UAE/Date Data")]
public class DateData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private DateVariety variety;
    [SerializeField] private string displayName;

    [Header("Sprites")]
    [Tooltip("Large image for Learn, Quiz and result screens (about 512px).")]
    [SerializeField] private Sprite displaySprite;
    [Tooltip("Small image for falling dates (about 256px). Falls back to the display sprite.")]
    [SerializeField] private Sprite iconSprite;

    [Header("Learning content (keep it child-friendly)")]
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private string origin;
    [SerializeField] private string appearance;
    [SerializeField] private string texture;
    [SerializeField] private string taste;
    [SerializeField, TextArea(2, 3)] private string funFact;
    [SerializeField] private string[] benefits;
    [SerializeField, TextArea(1, 3)] private string nutritionNote;

    [Header("Quiz")]
    [Tooltip("Shown after answering, e.g. 'Ajwa is a premium date variety.'")]
    [SerializeField, TextArea(1, 3)] private string quizFeedback;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip narrationClip;

    public DateVariety Variety => variety;
    public string DisplayName => displayName;
    public Sprite DisplaySprite => displaySprite;
    public Sprite IconSprite => iconSprite != null ? iconSprite : displaySprite;
    public string Description => description;
    public string Origin => origin;
    public string Appearance => appearance;
    public string Texture => texture;
    public string Taste => taste;
    public string FunFact => funFact;
    public string[] Benefits => benefits;
    public string NutritionNote => nutritionNote;
    public string QuizFeedback => quizFeedback;
    public AudioClip NarrationClip => narrationClip;

#if UNITY_EDITOR
    /// <summary>Used by the editor creator tool only.</summary>
    public void EditorSetup(DateVariety v, string name, string desc, string org, string app,
                            string tex, string tst, string fact, string[] ben, string nutrition, string quiz)
    {
        variety = v;
        displayName = name;
        description = desc;
        origin = org;
        appearance = app;
        texture = tex;
        taste = tst;
        funFact = fact;
        benefits = ben;
        nutritionNote = nutrition;
        quizFeedback = quiz;
    }
#endif
}