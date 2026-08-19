using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlindsideLeaderboardEntry
{
    public string playerName;
    public float percent;
}

[System.Serializable]
public class BlindsideLeaderboardDay
{
    public string dayName;
    public List<BlindsideLeaderboardEntry> entries;
}

[CreateAssetMenu(
    fileName = "BlindsideLeaderboard",
    menuName = "Blindside/Leaderboard"
)]
public class BS_LeaderboardDataSO : ScriptableObject
{
    public List<BlindsideLeaderboardDay> days;
}