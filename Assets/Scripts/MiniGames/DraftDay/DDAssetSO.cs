using UnityEngine;

/// <summary>Broad asset class used by the Draft Day roster constraints.</summary>
public enum DDAssetCategory { Crypto, Commodity, Etf, Stock }

/// <summary>A single draftable asset with an authored week of daily percent returns.</summary>
[CreateAssetMenu(fileName = "DDAsset", menuName = "ScriptableObjects/DraftDay/Asset")]
public class DDAssetSO : ScriptableObject
{
    public const int DaysPerWeek = 7;

    public string displayName;
    public DDAssetCategory category;
    public Sprite logo;

    [Tooltip("Authored percent return for each day of the week.")]
    public float[] dailyReturns = new float[DaysPerWeek];

    /// <summary>Returns the authored percent return for a zero-based day index.</summary>
    public float GetDailyReturn(int dayIndex)
    {
        return dayIndex >= 0 && dayIndex < dailyReturns.Length ? dailyReturns[dayIndex] : 0f;
    }
}
