using System.Collections.Generic;
using UnityEngine;

public enum SpreadTradeSide
{
    Buy,
    Sell
}

public enum SpreadOrderType
{
    Market,
    Limit,
    Stop
}

[System.Serializable]
public class SpreadOrderBookEntry
{
    public float price;
    public int quantity;
}

[System.Serializable]
public class SpreadTargetData
{
    [Header("Target")]
    public SpreadTradeSide side;

    [Tooltip("Total quantity the player must buy/sell.")]
    public int quantity;

    [Tooltip("For BUY: maximum acceptable price. For SELL: minimum acceptable price.")]
    public float targetPrice;
}

[System.Serializable]
public class SpreadScoringData
{
    [Header("Execution Cost Thresholds")]
    public float excellentThreshold = 0.02f;
    public float goodThreshold = 0.05f;
    public float averageThreshold = 0.10f;
}

[System.Serializable]
public class SpreadLevelData
{
    [Header("Level")]
    public int levelNumber;
    public string levelName;

    [Header("Target")]
    public SpreadTargetData target;

    [Header("Correct Order Type")]
    public SpreadOrderType correctOrderType;

    //[Header("Available Order Types")]
    //public List<SpreadOrderType> availableOrderTypes =
    //    new List<SpreadOrderType>
    //    {
    //        SpreadOrderType.Market,
    //        SpreadOrderType.Limit,
    //        SpreadOrderType.Stop
    //    };

    [Header("SELLERS")]
    [Tooltip("Lowest sell price should be first.")]
    public List<SpreadOrderBookEntry> sellers =
        new List<SpreadOrderBookEntry>();

    [Header("BUYERS")]
    [Tooltip("Highest buy price should be first.")]
    public List<SpreadOrderBookEntry> buyers =
        new List<SpreadOrderBookEntry>();

    [Header("Benchmark")]
    public float benchmarkPrice;

    [Header("Scoring")]
    public SpreadScoringData scoring;
}

[CreateAssetMenu(
    fileName = "SpreadGameData",
    menuName = "Mini Games/Spread Game/Spread Game Data"
)]
public class SpreadGameDataSO : ScriptableObject
{
    [Header("Spread Levels")]
    public List<SpreadLevelData> levels =
        new List<SpreadLevelData>();
}