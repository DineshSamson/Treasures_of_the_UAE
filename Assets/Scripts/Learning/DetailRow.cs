using TMPro;
using UnityEngine;

/// <summary>
/// One "Label: value" row. Hides itself when there is no value.
/// </summary>
public class DetailRow : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;

    public void Set(string label, string value)
    {
        bool hasValue = !string.IsNullOrEmpty(value);
        gameObject.SetActive(hasValue);
        if (!hasValue) return;

        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;
    }
}