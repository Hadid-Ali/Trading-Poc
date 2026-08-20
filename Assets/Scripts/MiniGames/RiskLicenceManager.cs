using UnityEngine;

/// <summary>
/// Tracks how many "clean" runs (a full run finished WITHOUT busting)
/// the player has completed, persists that count across sessions, and
/// exposes which licence tiers are currently unlocked.
///
/// This is deliberately a static class, not a MonoBehaviour on a scene
/// object. The Stake mini-game, the Ruin result screen, and the Licence
/// screen may all live in different scenes/panels — a static class (backed
/// by PlayerPrefs) means any of them can read/write the same progress
/// without you needing a DontDestroyOnLoad singleton object.
/// </summary>
public static class RiskLicenceManager
{
    private const string CleanRunsKey = "RiskGame_CleanRuns";
    private const string HasPlayedKey = "RiskGame_HasPlayedAnyRun";

    // -----------------------------------------------------------
    // Unlock thresholds — tune these to match your design doc.
    // Number = how many CLEAN runs are required to unlock the feature.
    // -----------------------------------------------------------
    public const int SpotTradingRequirement = 1; // needs at least 1 survived run - nothing opens on a bust
    public const int StopLimitRequirement = 2;
    public const int Margin2xRequirement = 3; // matches "3 clean runs" gate in the doc
    public const int Margin5xRequirement = 5;

    /// <summary>Total clean runs completed so far, persisted via PlayerPrefs.</summary>
    public static int CleanRuns
    {
        get => PlayerPrefs.GetInt(CleanRunsKey, 0);
        private set
        {
            PlayerPrefs.SetInt(CleanRunsKey, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// True as soon as the player has finished ONE run, win or bust.
    /// Before this is true, the Licence screen should show every row
    /// as Locked regardless of individual thresholds - there's simply
    /// no track record yet.
    /// </summary>
    public static bool HasPlayedAnyRun
    {
        get => PlayerPrefs.GetInt(HasPlayedKey, 0) == 1;
        private set
        {
            PlayerPrefs.SetInt(HasPlayedKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Call this every time a run ends, regardless of outcome (win or
    /// bust). In RiskGameController, call it at the top of FinishGame(),
    /// before the survived/bust branch.
    /// </summary>
    public static void RegisterRunPlayed()
    {
        HasPlayedAnyRun = true;
    }

    /// <summary>
    /// Call this exactly once, when a run ends WITHOUT busting.
    /// In RiskGameController, that's the `survived == true` branch
    /// inside FinishGame().
    /// </summary>
    public static void RegisterCleanRun()
    {
        CleanRuns++;
    }

    /// <summary>
    /// Optional: wire this to a debug/reset button if you want to
    /// re-test the unlock flow without clearing all PlayerPrefs.
    /// </summary>
    public static void ResetProgress()
    {
        CleanRuns = 0;
        HasPlayedAnyRun = false;
    }

    public static bool IsSpotTradingUnlocked => CleanRuns >= SpotTradingRequirement;
    public static bool IsStopLimitUnlocked => CleanRuns >= StopLimitRequirement;
    public static bool IsMargin2xUnlocked => CleanRuns >= Margin2xRequirement;
    public static bool IsMargin5xUnlocked => CleanRuns >= Margin5xRequirement;

    /// <summary>
    /// Runs remaining until the NEXT locked tier unlocks (0 if everything
    /// is unlocked already). Feeds the "1 run away" style copy.
    /// </summary>
    public static int RunsUntilNextUnlock()
    {
        if (!IsStopLimitUnlocked) return StopLimitRequirement - CleanRuns;
        if (!IsMargin2xUnlocked) return Margin2xRequirement - CleanRuns;
        if (!IsMargin5xUnlocked) return Margin5xRequirement - CleanRuns;
        return 0;
    }
}