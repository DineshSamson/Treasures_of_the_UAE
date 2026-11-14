using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the main Canvas. At startup it gives every Button (including
/// inactive ones) a click sound and a press-scale effect.
/// Skips AnswerButtons for the scale effect (they manage their own scale).
/// </summary>
public class UIButtonFeedbackInstaller : MonoBehaviour
{
    [SerializeField] private bool addClickSound = true;
    [SerializeField] private bool addPressScale = true;

    private readonly List<Button> buttons = new List<Button>(32);

    private void Start()
    {
        GetComponentsInChildren(true, buttons);

        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            if (addClickSound) button.onClick.AddListener(PlayClick);

            if (addPressScale &&
                button.GetComponent<AnswerButton>() == null &&
                button.GetComponent<ButtonPressFeedback>() == null)
            {
                button.gameObject.AddComponent<ButtonPressFeedback>();
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null) buttons[i].onClick.RemoveListener(PlayClick);
        }
    }

    private static void PlayClick()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(SoundId.ButtonClick);
    }
}