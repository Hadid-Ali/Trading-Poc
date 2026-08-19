using System.Collections.Generic;
using UnityEngine;

/// <summary>Risk-adjusted scoring and weight arithmetic for Draft Day.</summary>
public static class DraftDayScoring
{
    public const int TotalWeightPercent = 100;
    public const int MinimumDaysForScore = 2;

    private const float VolatilityEpsilon = 0.0001f;
    private const float SpreadRatingMidpoint = 50f;
    private const float SpreadRatingBaseScore = 0.22f;
    private const float SpreadRatingSlope = 115f;

    /// <summary>Weighted daily portfolio returns for the first dayCount days.</summary>
    public static float[] BuildDailyPortfolioReturns(IReadOnlyList<DDRosterEntry> roster, int dayCount)
    {
        float[] dailyReturns = new float[Mathf.Max(0, dayCount)];
        if (roster == null) return dailyReturns;

        for (int day = 0; day < dailyReturns.Length; day++)
        {
            float total = 0f;
            foreach (DDRosterEntry entry in roster)
            {
                if (entry.asset == null) continue;
                total += entry.weight / (float)TotalWeightPercent * entry.asset.GetDailyReturn(day);
            }
            dailyReturns[day] = total;
        }

        return dailyReturns;
    }

    /// <summary>Mean daily return divided by its standard deviation. Zero until two days exist.</summary>
    public static float CalculateScore(IReadOnlyList<DDRosterEntry> roster, int dayCount)
    {
        if (roster == null || roster.Count == 0 || dayCount < MinimumDaysForScore) return 0f;

        float[] dailyReturns = BuildDailyPortfolioReturns(roster, dayCount);

        float mean = 0f;
        foreach (float value in dailyReturns) mean += value;
        mean /= dailyReturns.Length;

        float sumSquares = 0f;
        foreach (float value in dailyReturns)
        {
            float deviation = value - mean;
            sumSquares += deviation * deviation;
        }

        float volatility = Mathf.Sqrt(sumSquares / dailyReturns.Length);
        return volatility < VolatilityEpsilon ? 0f : mean / volatility;
    }

    /// <summary>Total percent return across the given number of days.</summary>
    public static float CalculateWeekReturn(IReadOnlyList<DDRosterEntry> roster, int dayCount)
    {
        float total = 0f;
        foreach (float value in BuildDailyPortfolioReturns(roster, dayCount)) total += value;
        return total;
    }

    /// <summary>Maps a risk-adjusted score onto the friendly 0-100 Spread Rating.</summary>
    public static int CalculateSpreadRating(float score)
    {
        float rating = SpreadRatingMidpoint + (score - SpreadRatingBaseScore) * SpreadRatingSlope;
        return Mathf.RoundToInt(Mathf.Clamp(rating, 0f, 100f));
    }

    /// <summary>Splits 100 points as evenly as possible, remainder going to the earliest slots.</summary>
    public static void ApplyEqualWeights(List<DDRosterEntry> roster)
    {
        if (roster == null || roster.Count == 0) return;

        int share = TotalWeightPercent / roster.Count;
        int remainder = TotalWeightPercent - share * roster.Count;

        for (int i = 0; i < roster.Count; i++)
        {
            roster[i].weight = share + (i < remainder ? 1 : 0);
        }
    }

    /// <summary>Sets one weight and rescales the others proportionally so the total is exactly 100.</summary>
    public static void RebalanceAround(List<DDRosterEntry> roster, int changedIndex, int newWeight)
    {
        if (roster == null || changedIndex < 0 || changedIndex >= roster.Count) return;

        newWeight = Mathf.Clamp(newWeight, 0, TotalWeightPercent);
        int otherCount = roster.Count - 1;

        if (otherCount <= 0)
        {
            roster[changedIndex].weight = TotalWeightPercent;
            return;
        }

        int previousOthersTotal = TotalWeightPercent - roster[changedIndex].weight;
        int budget = TotalWeightPercent - newWeight;
        roster[changedIndex].weight = newWeight;

        int assigned = 0;
        int largestIndex = -1;

        for (int i = 0; i < roster.Count; i++)
        {
            if (i == changedIndex) continue;

            int value = previousOthersTotal > 0
                ? Mathf.RoundToInt(roster[i].weight * (float)budget / previousOthersTotal)
                : Mathf.RoundToInt(budget / (float)otherCount);

            roster[i].weight = Mathf.Clamp(value, 0, TotalWeightPercent);
            assigned += roster[i].weight;

            if (largestIndex < 0 || roster[i].weight > roster[largestIndex].weight) largestIndex = i;
        }

        int drift = budget - assigned;
        if (drift != 0 && largestIndex >= 0)
        {
            roster[largestIndex].weight = Mathf.Clamp(roster[largestIndex].weight + drift, 0, TotalWeightPercent);
        }
    }

    /// <summary>Largest single holding as a 0-1 share, used to detect a concentrated squad.</summary>
    public static float LargestWeightShare(IReadOnlyList<DDRosterEntry> roster)
    {
        int largest = 0;
        if (roster == null) return 0f;
        foreach (DDRosterEntry entry in roster) largest = Mathf.Max(largest, entry.weight);
        return largest / (float)TotalWeightPercent;
    }
}
