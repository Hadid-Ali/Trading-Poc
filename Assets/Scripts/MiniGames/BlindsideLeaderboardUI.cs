using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 6 fixed rows matching the 6 entries in the SO, in the same order.
/// Whichever SO entry is named "You" gets its score overridden with the real
/// game score, and its row background (sprite + color) swapped to the
/// dedicated "You" look. No sorting, no spawning.
/// </summary>
public class BlindsideLeaderboardUI : MonoBehaviour
{
    [System.Serializable]
    public class LeaderboardRowRefs
    {
        public TMP_Text entryText;   // "name txt"
        public TMP_Text scoreText;   // "Score"
        public Image background;     // this row's background image
    }

    [Header("Data")]
    [SerializeField] private BS_LeaderboardDataSO leaderboardData;

    [Header("Rows (6 total, same order as SO entries)")]
    [SerializeField] private List<LeaderboardRowRefs> leaderboardRows;

    [Header("You Highlight")]
    [Tooltip("The background sprite to apply to whichever row is 'You'.")]
    [SerializeField] private Sprite youBackground;

    [Tooltip("The background color to apply to whichever row is 'You'.")]
    [SerializeField] private Color youColor = Color.white;
    [Header("Normal Row Look (reset target)")]
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("-1 = pick day from date, same as BlindsideController. MUST match BlindsideController's dayIndex when testing.")]
    [SerializeField] private int dayIndexOverride = -1;

    private const string YouName = "You";
    private static readonly DateTime Epoch = new DateTime(2026, 1, 1);

    private float yourScore = 0f;
    private bool hasYourScore = false;
    [Header("Avatar Reaction (single, reacts to YOUR rank)")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Sprite happyAvatar;
    [SerializeField] private Sprite normalAvatar;
    [SerializeField] private Sprite sadAvatar;

    public void SetYourScore(float score)
    {
        yourScore = score;
        hasYourScore = true;
    }

    public void BuildLeaderboard()
    {
        if (leaderboardData == null || leaderboardData.days == null || leaderboardData.days.Count == 0)
        {
            Debug.LogWarning("Leaderboard Data is missing or empty.");
            return;
        }

        if (leaderboardRows == null || leaderboardRows.Count == 0)
        {
            Debug.LogWarning("Leaderboard Rows are not assigned.");
            return;
        }

        int index = GetDayIndex();

        if (index < 0 || index >= leaderboardData.days.Count)
            return;

        var dayEntries = leaderboardData.days[index].entries;

        if (dayEntries == null)
            return;

        var combined = new List<BlindsideLeaderboardEntry>();

        foreach (var entry in dayEntries)
        {
            bool isYouEntry = entry.playerName == YouName;

            combined.Add(new BlindsideLeaderboardEntry
            {
                playerName = entry.playerName,
                percent = isYouEntry && hasYourScore ? yourScore : entry.percent
            });
        }

        combined.Sort((a, b) => b.percent.CompareTo(a.percent));

        int count = Mathf.Min(combined.Count, leaderboardRows.Count);
        int yourRankIndex = -1;   // 0-based

        for (int i = 0; i < count; i++)
        {
            var row = leaderboardRows[i];
            var entry = combined[i];

            bool isYou = entry.playerName == YouName;

            if (isYou)
                yourRankIndex = i;

            if (row.entryText != null)
                row.entryText.text = entry.playerName;

            if (row.scoreText != null)
                row.scoreText.text = FormatPercent(entry.percent);

            if (row.background != null)
            {
                if (isYou)
                {
                    if (youBackground != null)
                        row.background.sprite = youBackground;

                    row.background.color = youColor;
                }
                else
                {
                    if (normalBackground != null)
                        row.background.sprite = normalBackground;

                    row.background.color = normalColor;
                }
            }
        }

        // Single avatar reacts to where "You" landed.
        if (avatarImage != null && yourRankIndex >= 0)
            avatarImage.sprite = GetAvatarForRank(yourRankIndex, count);
    }

    /// <summary>
    /// Top 2 ranks = happy, bottom 2 ranks = sad, everyone in between = normal.
    /// </summary>
    private Sprite GetAvatarForRank(int zeroBasedIndex, int totalCount)
    {
        int rank = zeroBasedIndex + 1;
        int fromBottom = totalCount - zeroBasedIndex;

        if (rank <= 2)
            return happyAvatar;

        if (fromBottom <= 2)
            return sadAvatar;

        return normalAvatar;
    }
    private int GetDayIndex()
    {
        if (dayIndexOverride >= 0)
            return dayIndexOverride;

        int dayNumber = (int)(DateTime.Now.Date - Epoch.Date).TotalDays;
        int count = leaderboardData.days.Count;

        int idx = dayNumber % count;
        if (idx < 0) idx += count;

        return idx;
    }

    private static string FormatPercent(float value)
    {
        return $"{value:+0.#;-0.#;0.#}%";
    }
}