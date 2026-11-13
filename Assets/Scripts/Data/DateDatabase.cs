using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all DateData assets in display order. Learning, Quiz and Catch
/// all read from this one asset.
/// </summary>
[CreateAssetMenu(fileName = "DateDatabase", menuName = "Treasures of the UAE/Date Database")]
public class DateDatabase : ScriptableObject
{
    [SerializeField] private DateData[] dates;

    public int Count => dates != null ? dates.Length : 0;
    public IReadOnlyList<DateData> All => dates;

    public DateData GetAt(int index)
    {
        if (dates == null || index < 0 || index >= dates.Length) return null;
        return dates[index];
    }

    public DateData Get(DateVariety variety)
    {
        if (dates == null) return null;
        for (int i = 0; i < dates.Length; i++)
        {
            if (dates[i] != null && dates[i].Variety == variety) return dates[i];
        }
        return null;
    }

#if UNITY_EDITOR
    public void EditorSetDates(DateData[] newDates)
    {
        dates = newDates;
    }
#endif
}