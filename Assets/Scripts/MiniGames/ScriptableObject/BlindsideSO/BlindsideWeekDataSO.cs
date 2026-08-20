using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BlindsideWeek",
    menuName = "Blindside/Week"
)]
public class BlindsideWeekDataSO : ScriptableObject
{
    public int weekNumber;
    public List<BlindsideDayData> days;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (days == null) return;

        foreach (var day in days)
        {
            if (day != null)
                day.SyncHeldPercent();
        }
    }
#endif
}

[System.Serializable]
public class BlindsideDayData
{
    public string dayName;
    public int chartId = 1;
    public GameObject chartPrefab;
    public int startingVisibleCandles = 8;
    public List<BlindsideDecisionData> decisions;

    [Header("Reveal Screen")]
    public BlindsideRevealData reveal = new BlindsideRevealData();

    /// <summary>
    /// Buy at the first candle, sell at the last. This is the HELD benchmark.
    /// </summary>
    public float CalculateHeldPercent()
    {
        if (decisions == null || decisions.Count == 0)
            return 0f;

        float first = decisions[0].currentPrice;
        float last = decisions[decisions.Count - 1].nextPrice;

        if (Mathf.Approximately(first, 0f))
            return 0f;

        return (last - first) / first * 100f;
    }

    /// <summary>
    /// Keeps reveal.heldPercent honest. Called from OnValidate, so editing any
    /// price in the inspector immediately refreshes the number.
    /// </summary>
    public void SyncHeldPercent()
    {
        if (reveal == null || reveal.overrideHeld)
            return;

        reveal.heldPercent = CalculateHeldPercent();
    }
}

[System.Serializable]
public class BlindsideDecisionData
{
    public float currentPrice;
    public float nextPrice;
}

[System.Serializable]
public class BlindsideRevealData
{
    [Tooltip("What it actually was. e.g. Brent crude")]
    public string asset = "";

    [Tooltip("Candle timeframe, NOT today's date. e.g. DAILY / 4H / WEEKLY")]
    public string timeframe = "DAILY";

    [Tooltip("When this actually happened. e.g. MARCH 2022")]
    public string period = "";

    [TextArea(2, 4)]
    [Tooltip("One line of context. e.g. A supply shock repriced the barrel in nine sessions.")]
    public string headline = "";

    [Header("HELD")]
    [Tooltip("Auto-filled from the prices. Leave Override Held off and do not type here.")]
    public float heldPercent;

    [Tooltip("Turn on only if you need a HELD number that ignores the prices.")]
    public bool overrideHeld = false;
}