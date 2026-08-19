using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Whether a category rule enforces a floor or a ceiling.</summary>
public enum DDRuleType { Minimum, Maximum }

/// <summary>Mascot expression, mapped to one of the three Coach Nadir sprites.</summary>
public enum DDCoachMood { Happy, Neutral, Sad }

/// <summary>Moments in the session that Coach Nadir reacts to.</summary>
public enum DDCoachSituation
{
    ConstraintOpen,
    CryptoFilled,
    RosterComplete,
    WeightsBalanced,
    WeightsConcentrated,
    ResultTop,
    ResultMid,
    ResultBottom
}

[Serializable]
public class DDCategoryRule
{
    public DDAssetCategory category;
    public DDRuleType ruleType;
    public int amount = 1;
}

[Serializable]
public class DDRosterEntry
{
    public DDAssetSO asset;
    [Range(0, 100)] public int weight;
}

[Serializable]
public class DDOpponent
{
    public string displayName;
    public List<DDRosterEntry> roster = new();
}

[Serializable]
public class DDCoachBucket
{
    public DDCoachSituation situation;
    public DDCoachMood mood;
    [TextArea] public string[] lines;
}

/// <summary>All authored content for the Draft Day minigame.</summary>
[CreateAssetMenu(fileName = "DDDraftDaySO", menuName = "ScriptableObjects/DraftDay/Config")]
public class DDDraftDaySO : ScriptableObject
{
    [Header("Pool")]
    public List<DDAssetSO> pool = new();
    public List<DDCategoryRule> rules = new();

    [Header("League")]
    public List<DDOpponent> opponents = new();
    public string playerEntryName = "You";

    [Header("Coach")]
    public List<DDCoachBucket> coachBuckets = new();

    [Header("Info line")]
    [TextArea] public string infoDrafting;
    [TextArea] public string infoPlayingWeek;
    [TextArea] public string infoResolved;

    /// <summary>Picks a random authored line for a situation. Returns false when none is authored.</summary>
    public bool TryGetCoachLine(DDCoachSituation situation, out string line, out DDCoachMood mood)
    {
        foreach (DDCoachBucket bucket in coachBuckets)
        {
            if (bucket.situation != situation || bucket.lines == null || bucket.lines.Length == 0) continue;
            line = bucket.lines[UnityEngine.Random.Range(0, bucket.lines.Length)];
            mood = bucket.mood;
            return true;
        }

        line = string.Empty;
        mood = DDCoachMood.Neutral;
        return false;
    }
}
