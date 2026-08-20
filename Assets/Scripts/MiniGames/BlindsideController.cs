using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lives on the parent Blindside panel, NOT on Gameplay. The parent stays
/// enabled while the panel is open so the countdown and the midnight check
/// keep running.
/// </summary>
public class BlindsideController : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private BlindsideWeekDataSO weekData;

    [Tooltip("-1 = pick the day from the date. 0..6 = force that day for testing.")]
    [SerializeField] private int dayIndex = -1;

    [Header("Daily Lock")]
    [Tooltip("Turn OFF while testing so you can replay the same day.")]
    [SerializeField] private bool lockAfterPlaying = true;

    [Header("Panels")]
    [SerializeField] private UiManager uiManager;
    [SerializeField] private GameObject gameplay;
    [SerializeField] private GameObject revealPanel;

    [Header("Chart")]
    [SerializeField] private Transform chartParent;

    [Header("Decision Buttons")]
    [SerializeField] private Button longButton;
    [SerializeField] private Button flatButton;
    [SerializeField] private Button shortButton;

    [Header("Live UI")]
    [SerializeField] private TMP_Text unrealisedText;
    [SerializeField] private TMP_Text streakText;      // "STREAK 6"
    [SerializeField] private TMP_Text countdownText;   // "14 : 02 : 11"

    [Header("Reveal Panel")]
    [SerializeField] private TMP_Text vegaLineText;    // "You flipped six times..."
    [SerializeField] private TMP_Text assetText;       // "Brent crude"
    [SerializeField] private TMP_Text subtitleText;    // "DAILY  ·  MARCH 2022"
    [SerializeField] private TMP_Text youText;         // "+4.2%"
    [SerializeField] private TMP_Text heldText;        // "+11.8%"
    [SerializeField] private TMP_Text headlineText;    // footer line

    private BlindsideChart currentChart;
    private BlindsideDayData today;

    private int decisionIndex;
    private float totalPL;

    private bool gameFinished;
    private bool initialised;

    // Which calendar day is currently loaded. Compared against TodayKey to
    // catch midnight, whether the panel was open, closed, or backgrounded.
    private string loadedDateKey = "";

    // Flip tracking. A segment is a run of taps spent in the same position.
    private DecisionType currentPosition;
    private bool hasOpenSegment;
    private float openSegmentPL;
    private readonly List<float> segmentResults = new List<float>();

    // Day 0 of the rotation. Do NOT change after launch or everyone's schedule shifts.
    private static readonly DateTime Epoch = new DateTime(2026, 1, 1);

    private const string StreakKey = "blindside_streak";
    private const string LastPlayedKey = "blindside_last_played";
    private const string ScoreKey = "blindside_score";
    private const string FlipsKey = "blindside_flips";
    private const string CostlyKey = "blindside_costly";

    private static string DateKey(DateTime d) { return d.ToString("yyyyMMdd"); }
    private static string TodayKey { get { return DateKey(DateTime.Now); } }
    private static string YesterdayKey { get { return DateKey(DateTime.Now.AddDays(-1)); } }

    private bool AlreadyPlayedToday
    {
        get { return PlayerPrefs.GetString(LastPlayedKey, "") == TodayKey; }
    }

    // =========================================================
    // LIFECYCLE
    // =========================================================

    private void Start()
    {
        if (longButton != null)
            longButton.onClick.AddListener(OnLongClicked);

        if (flatButton != null)
            flatButton.onClick.AddListener(OnFlatClicked);

        if (shortButton != null)
            shortButton.onClick.AddListener(OnShortClicked);

        StartDay();
    }

    /// <summary>
    /// Runs every time the Blindside panel is switched back on from the menu.
    /// </summary>
    private void OnEnable()
    {
        // OnEnable fires before Start on the first frame, when today is still null.
        if (!initialised)
            return;

        string before = loadedDateKey;

        CheckDayRollover();

        // A rollover means StartDay() already spawned the new chart and put the
        // gameplay panel up. Nothing left to do.
        if (before != loadedDateKey)
            return;

        if (gameFinished)
            ShowResultState();
        else
            ShowPlayingState();
    }

    private void Update()
    {
        // Countdown always runs, not just on the reveal screen.
        if (countdownText != null)
        {
            TimeSpan remaining = DateTime.Now.Date.AddDays(1) - DateTime.Now;

            countdownText.text =
                $"{(int)remaining.TotalHours:00} : {remaining.Minutes:00} : {remaining.Seconds:00}";
        }

        CheckDayRollover();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused) CheckDayRollover();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (focused) CheckDayRollover();
    }

    /// <summary>
    /// Compares the loaded calendar day against the real one. This is the single
    /// place midnight is detected — countdown text does not drive anything.
    /// </summary>
    private void CheckDayRollover()
    {
        if (!initialised) return;
        if (weekData == null || weekData.days == null || weekData.days.Count == 0) return;

        if (loadedDateKey != TodayKey)
        {
            Debug.Log($"DAY ROLLOVER: '{loadedDateKey}' -> '{TodayKey}'");
            StartDay();
        }
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    /// <summary>
    /// Hook this to the back button. Nothing is lost — the day's state lives in
    /// PlayerPrefs, not in memory.
    /// </summary>
    public void BackToMenu()
    {
        if (gameplay != null) gameplay.SetActive(false);
        if (revealPanel != null) revealPanel.SetActive(false);

        if (uiManager != null)
        {
            uiManager.ToggleAllPanels(false);
            uiManager.ToggleHomePanel(true);
        }
    }

    private void ShowPlayingState()
    {
        if (gameplay != null) gameplay.SetActive(true);
        if (revealPanel != null) revealPanel.SetActive(false);
    }

    private void ShowResultState()
    {
        if (gameplay != null) gameplay.SetActive(false);
        if (revealPanel != null) revealPanel.SetActive(true);
    }

    // =========================================================
    // SETUP
    // =========================================================

    private int GetDayIndex()
    {
        if (dayIndex >= 0)
            return Mathf.Clamp(dayIndex, 0, weekData.days.Count - 1);

        int dayNumber = (int)(DateTime.Now.Date - Epoch.Date).TotalDays;

        int index = dayNumber % weekData.days.Count;
        if (index < 0) index += weekData.days.Count;

        return index;
    }

    private void StartDay()
    {
        if (weekData == null)
        {
            Debug.LogError("Blindside Week Data is missing.");
            return;
        }

        if (weekData.days == null || weekData.days.Count == 0)
        {
            Debug.LogError("No days found in Blindside Week Data.");
            return;
        }

        int index = GetDayIndex();
        today = weekData.days[index];
        loadedDateKey = TodayKey;
        initialised = true;

        if (today.chartPrefab == null)
        {
            Debug.LogError($"{today.dayName}: Chart Prefab is missing.");
            return;
        }

        if (today.decisions == null || today.decisions.Count == 0)
        {
            Debug.LogError($"{today.dayName}: no decisions found.");
            return;
        }

        SpawnChart(today.chartPrefab);

        decisionIndex = 0;
        totalPL = 0f;
        gameFinished = false;

        // Reset flip tracking
        currentPosition = DecisionType.Flat;
        hasOpenSegment = false;
        openSegmentPL = 0f;
        segmentResults.Clear();

        UpdatePLUI();
        UpdateStreakUI(GetDisplayStreak());

        DateTime now = DateTime.Now;
        TimeSpan untilReset = now.Date.AddDays(1) - now;

        Debug.Log(
            $"===== BLINDSIDE STARTED =====\n" +
            $"Local time : {now:yyyy-MM-dd HH:mm:ss}\n" +
            $"Day number : {(int)(now.Date - Epoch.Date).TotalDays}  ->  index {index} of {weekData.days.Count}\n" +
            $"Playing    : {today.dayName} | Chart {today.chartId} | {today.decisions.Count} ticks\n" +
            $"Held       : {GetHeldPercent():0.##}%\n" +
            $"Next reset : in {(int)untilReset.TotalHours}h {untilReset.Minutes}m\n" +
            $"Last played: '{PlayerPrefs.GetString(LastPlayedKey, "(never)")}'  |  Today: '{TodayKey}'\n" +
            $"Streak     : {GetDisplayStreak()}\n" +
            $"Locked     : {(lockAfterPlaying && AlreadyPlayedToday)}"
        );

        // Already played today -> restore the finished result instead of the game.
        if (lockAfterPlaying && AlreadyPlayedToday)
        {
            RestoreFinishedState();
            return;
        }

        SetButtonsInteractable(true);
        ShowPlayingState();
    }

    /// <summary>
    /// Rebuilds the reveal from what was saved, so coming back from the menu
    /// shows the real score instead of zeros.
    /// </summary>
    private void RestoreFinishedState()
    {
        gameFinished = true;

        totalPL = PlayerPrefs.GetFloat(ScoreKey, 0f);
        int flips = PlayerPrefs.GetInt(FlipsKey, 0);
        int costly = PlayerPrefs.GetInt(CostlyKey, 0);

        // Reveal every candle without needing RevealAll on BlindsideChart.
        if (currentChart != null)
        {
            for (int i = 0; i < today.decisions.Count; i++)
                currentChart.Reveal(i);
        }

        decisionIndex = today.decisions.Count;

        SetButtonsInteractable(false);
        UpdatePLUI();
        ShowReveal(GetHeldPercent(), flips, costly);
    }

    private void SpawnChart(GameObject chartPrefab)
    {
        if (chartParent == null)
        {
            Debug.LogError("Chart Parent is missing.");
            return;
        }

        // Clear any chart left over from a previous run
        for (int i = chartParent.childCount - 1; i >= 0; i--)
            Destroy(chartParent.GetChild(i).gameObject);

        GameObject chartObject = Instantiate(chartPrefab, chartParent);
        currentChart = chartObject.GetComponent<BlindsideChart>();

        if (currentChart == null)
            Debug.LogError("Spawned chart does not contain BlindsideChart.");
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    public void OnLongClicked() { MakeDecision(DecisionType.Long); }
    public void OnFlatClicked() { MakeDecision(DecisionType.Flat); }
    public void OnShortClicked() { MakeDecision(DecisionType.Short); }

    // =========================================================
    // DECISION
    // =========================================================

    private void MakeDecision(DecisionType decision)
    {
        if (gameFinished)
            return;

        if (today == null)
            return;

        if (decisionIndex >= today.decisions.Count)
        {
            FinishGame();
            return;
        }

        if (currentChart == null)
        {
            Debug.LogError("CURRENT CHART IS NULL!");
            return;
        }

        // Tapping a different button closes the old segment and opens a new one.
        // Tapping the same button again just holds the position.
        if (!hasOpenSegment || decision != currentPosition)
        {
            if (hasOpenSegment)
                segmentResults.Add(openSegmentPL);

            currentPosition = decision;
            openSegmentPL = 0f;
            hasOpenSegment = true;
        }

        var decisionData = today.decisions[decisionIndex];

        float currentPrice = decisionData.currentPrice;
        float nextPrice = decisionData.nextPrice;

        // Movement as a percent of the very first price, so the player's total
        // sits on the same scale as HELD on the reveal screen.
        float basePrice = today.decisions[0].currentPrice;

        float percentMove = Mathf.Approximately(basePrice, 0f)
            ? 0f
            : (nextPrice - currentPrice) / basePrice * 100f;

        float result = CalculateResult(decision, percentMove);

        totalPL += result;
        openSegmentPL += result;

        Debug.Log(
            $"BLINDSIDE DECISION\n" +
            $"Decision: {decision}\n" +
            $"Index: {decisionIndex}\n" +
            $"Current Price: {currentPrice}\n" +
            $"Next Price: {nextPrice}\n" +
            $"Percent Move: {percentMove:0.##}%\n" +
            $"Result: {result:0.##}%\n" +
            $"Total P/L: {totalPL:0.##}%"
        );

        currentChart.Reveal(decisionIndex);
        decisionIndex++;

        UpdatePLUI();

        if (decisionIndex >= today.decisions.Count)
            FinishGame();
    }

    // =========================================================
    // CALCULATION
    // =========================================================

    private float CalculateResult(DecisionType decision, float percentMove)
    {
        switch (decision)
        {
            case DecisionType.Long: return percentMove;
            case DecisionType.Flat: return 0f;
            case DecisionType.Short: return -percentMove;
            default: return 0f;
        }
    }

    private float GetHeldPercent()
    {
        if (today == null)
            return 0f;

        if (today.reveal != null && today.reveal.overrideHeld)
            return today.reveal.heldPercent;

        return today.CalculateHeldPercent();
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdatePLUI()
    {
        if (unrealisedText == null)
            return;

        unrealisedText.text = FormatPercent(totalPL);
    }

    private void UpdateStreakUI(int streak)
    {
        if (streakText == null)
            return;

        streakText.text = $"STREAK {streak}";
    }

    private static string FormatPercent(float value)
    {
        return $"{value:+0.##;-0.##;0.##}%";
    }

    // =========================================================
    // STREAK
    // =========================================================

    /// <summary>
    /// What to show on screen. One missed day and the streak is dead.
    /// </summary>
    private int GetDisplayStreak()
    {
        string last = PlayerPrefs.GetString(LastPlayedKey, "");
        int streak = PlayerPrefs.GetInt(StreakKey, 0);

        if (last == TodayKey || last == YesterdayKey)
            return streak;

        return 0;
    }

    /// <summary>
    /// Called when a run finishes, not when the panel opens. Playing is what counts.
    /// </summary>
    private int RegisterPlay(int flips, int costlyFlips)
    {
        string last = PlayerPrefs.GetString(LastPlayedKey, "");
        int streak = PlayerPrefs.GetInt(StreakKey, 0);

        if (last != TodayKey)
        {
            streak = (last == YesterdayKey) ? streak + 1 : 1;

            PlayerPrefs.SetInt(StreakKey, streak);
            PlayerPrefs.SetString(LastPlayedKey, TodayKey);
        }

        // Saved so the reveal can be rebuilt after a trip to the menu.
        PlayerPrefs.SetFloat(ScoreKey, totalPL);
        PlayerPrefs.SetInt(FlipsKey, flips);
        PlayerPrefs.SetInt(CostlyKey, costlyFlips);
        PlayerPrefs.Save();

        return streak;
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void FinishGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        // Close the last open segment
        if (hasOpenSegment)
        {
            segmentResults.Add(openSegmentPL);
            hasOpenSegment = false;
        }

        SetButtonsInteractable(false);

        // How many times the player changed position, and how many of those
        // runs actually lost money.
        int flips = segmentResults.Count;
        int costlyFlips = 0;

        for (int i = 0; i < segmentResults.Count; i++)
        {
            if (segmentResults[i] < 0f)
                costlyFlips++;
        }

        float held = GetHeldPercent();

        int streak = RegisterPlay(flips, costlyFlips);
        UpdateStreakUI(streak);

        Debug.Log(
            $"BLINDSIDE FINISHED\n" +
            $"You: {totalPL:0.##}%\n" +
            $"Held: {held:0.##}%\n" +
            $"Flips: {flips} (costly: {costlyFlips})\n" +
            $"Streak: {streak}"
        );

        ShowReveal(held, flips, costlyFlips);
    }

    // =========================================================
    // REVEAL
    // =========================================================

    private void ShowReveal(float held, int flips, int costlyFlips)
    {
        ShowResultState();

        var reveal = today.reveal;

        if (vegaLineText != null)
            vegaLineText.text = BuildVegaLine(flips, costlyFlips);

        if (assetText != null)
            assetText.text = reveal != null ? reveal.asset : "";

        if (subtitleText != null)
        {
            string tf = reveal != null ? reveal.timeframe : "";
            string period = reveal != null ? reveal.period : "";

            subtitleText.text = $"{tf}  ·  {period}".ToUpperInvariant();
        }

        if (youText != null)
            youText.text = FormatPercent(totalPL);

        if (heldText != null)
            heldText.text = FormatPercent(held);

        if (headlineText != null)
            headlineText.text = reveal != null ? reveal.headline : "";
    }

    /// <summary>
    /// Built from how the player actually traded, so this line is never
    /// authored per chart.
    /// </summary>
    private string BuildVegaLine(int flips, int costlyFlips)
    {
        if (flips <= 0)
            return "You sat this one out. The chart moved anyway.";

        if (flips == 1)
        {
            return totalPL >= 0f
                ? "You picked once and sat still. That is harder than it looks."
                : "One call, held to the end. Wrong one, but you did not flinch.";
        }

        string line = $"You flipped {NumberWord(flips)} times.";

        if (costlyFlips == 0)
            return line + " Not one of them hurt you.";

        if (costlyFlips == 1)
            return line + " One of them cost you the move.";

        return line + $" {Capitalise(NumberWord(costlyFlips))} of them cost you the move.";
    }

    private static string NumberWord(int n)
    {
        switch (n)
        {
            case 1: return "one";
            case 2: return "two";
            case 3: return "three";
            case 4: return "four";
            case 5: return "five";
            case 6: return "six";
            case 7: return "seven";
            case 8: return "eight";
            case 9: return "nine";
            case 10: return "ten";
            default: return n.ToString();
        }
    }

    private static string Capitalise(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    private void SetButtonsInteractable(bool value)
    {
        if (longButton != null) longButton.interactable = value;
        if (flatButton != null) flatButton.interactable = value;
        if (shortButton != null) shortButton.interactable = value;
    }

    // =========================================================
    // DEV
    // =========================================================

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(StreakKey);
        PlayerPrefs.DeleteKey(LastPlayedKey);
        PlayerPrefs.DeleteKey(ScoreKey);
        PlayerPrefs.DeleteKey(FlipsKey);
        PlayerPrefs.DeleteKey(CostlyKey);
        PlayerPrefs.Save();

        Debug.Log("Blindside progress cleared.");
    }

    private enum DecisionType
    {
        Long,
        Flat,
        Short
    }
}