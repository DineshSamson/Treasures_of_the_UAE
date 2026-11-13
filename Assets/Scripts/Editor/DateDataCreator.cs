#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu: Tools > Treasures of the UAE > Create Date Data Assets.
/// Creates six DateData assets plus a DateDatabase. Safe to run again:
/// existing assets are kept, so your sprite assignments are not overwritten.
/// </summary>
public static class DateDataCreator
{
    private const string Root = "Assets/_Project";
    private const string DataFolder = "Assets/_Project/Data";
    private const string DatesFolder = "Assets/_Project/Data/Dates";

    private const string GenericNutrition =
        "Like most dates, it has natural sugars for energy, plus fibre and minerals such as potassium.";

    [MenuItem("Tools/Treasures of the UAE/Create Date Data Assets")]
    public static void Create()
    {
        EnsureFolder(Root, "Assets", "_Project");
        EnsureFolder(DataFolder, Root, "Data");
        EnsureFolder(DatesFolder, DataFolder, "Dates");

        DateData[] all = new DateData[6];

        all[0] = GetOrCreate(DateVariety.Ajwa, "Ajwa",
            "Ajwa dates are small, dark and soft. They are gently sweet and very special to many families.",
            "Madinah, Saudi Arabia",
            "Dark brown to almost black, with thin wrinkly skin",
            "Soft and a little chewy",
            "Gently sweet with a hint of fruity flavour",
            "Ajwa dates are famous in Madinah and are treasured as a special treat.",
            new[] { "Natural energy boost", "Good source of fibre", "A tasty, healthy snack" },
            "Ajwa is a premium date variety from Madinah.");

        all[1] = GetOrCreate(DateVariety.Khalas, "Khalas",
            "Khalas dates are popular in the UAE. They are golden brown, soft and very sweet.",
            "Al-Ahsa, Saudi Arabia, and grown widely in the UAE",
            "Golden brown and shiny",
            "Soft and smooth",
            "Very sweet, like caramel",
            "Khalas is one of the best-loved dates in the Gulf, often enjoyed with Arabic coffee.",
            new[] { "Natural energy boost", "Good source of fibre" },
            "Khalas dates are golden brown, soft and very sweet.");

        all[2] = GetOrCreate(DateVariety.Fard, "Fard",
            "Fard dates are dark and rich. They have a deep, sweet flavour.",
            "Oman and the UAE",
            "Dark brown to nearly black and glossy",
            "Firm and a little chewy",
            "Rich and sweet, like dark honey",
            "Fard dates keep well, which makes them great to store and share.",
            new[] { "Natural energy boost", "Good source of fibre" },
            "Fard dates are dark, firm and richly sweet.");

        all[3] = GetOrCreate(DateVariety.Khenaizi, "Khenaizi",
            "Khenaizi dates are reddish-brown and smooth. They are a favourite in many UAE homes.",
            "Popular in the UAE; also grown in Iraq",
            "Reddish to dark brown and smooth",
            "Soft and tender",
            "Lightly sweet with a smooth taste",
            "Khenaizi is a favourite date in many UAE homes.",
            new[] { "Natural energy boost", "Contains fibre" },
            "Khenaizi dates are reddish-brown and smooth.");

        all[4] = GetOrCreate(DateVariety.Dabbas, "Dabbas",
            "Dabbas dates are soft and sweet. You can find them on farms and in markets in the UAE.",
            "Grown in the UAE",
            "Light to reddish brown",
            "Soft and tender",
            "Sweet with a honey-like flavour",
            "Dabbas dates are a popular choice on UAE farms and in local markets.",
            new[] { "Natural energy boost", "Contains fibre" },
            "Dabbas dates are soft, sweet and honey-like.");

        all[5] = GetOrCreate(DateVariety.Barhi, "Barhi",
            "Barhi dates are special because you can eat them yellow and crunchy, or brown and soft.",
            "Originally from Iraq; now grown in many Gulf countries",
            "Yellow when fresh, turning brown when ripe",
            "Crunchy when yellow, soft when brown",
            "Sweet like honey and caramel",
            "Barhi can be eaten at two stages: crunchy and yellow, or soft and brown!",
            new[] { "Natural energy boost", "Contains fibre" },
            "Barhi dates are yellow and crunchy when fresh, and soft and brown when ripe.");

        string dbPath = DatesFolder + "/DateDatabase.asset";
        DateDatabase db = AssetDatabase.LoadAssetAtPath<DateDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<DateDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }
        db.EditorSetDates(all);
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = db;
        EditorGUIUtility.PingObject(db);
        Debug.Log("Date assets ready in " + DatesFolder + ". Now assign the sprites.");
    }

    private static DateData GetOrCreate(DateVariety variety, string name, string desc, string origin,
        string appearance, string texture, string taste, string fact, string[] benefits, string quiz)
    {
        string path = $"{DatesFolder}/Date_{name}.asset";
        DateData data = AssetDatabase.LoadAssetAtPath<DateData>(path);
        if (data != null) return data; // keep existing assets (and their sprites)

        data = ScriptableObject.CreateInstance<DateData>();
        data.EditorSetup(variety, name, desc, origin, appearance, texture, taste, fact,
                         benefits, GenericNutrition, quiz);
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static void EnsureFolder(string fullPath, string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif