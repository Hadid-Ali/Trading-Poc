using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Session phases for the Draft Day minigame.</summary>
public enum DraftDayPhase { Drafting, Weighting, PlayingWeek, Finished }

[Serializable]
public class DDCoachView
{
    public Image mascotImage;
    public TMP_Text dialogueText;
}

[Serializable]
public class DDConstraintView
{
    public Image background;
    public TMP_Text label;
}

/// <summary>Drives the whole Draft Day minigame: draft, weighting, week playout and standings.</summary>
public class DraftDayController : MonoBehaviour
{
    private const int RosterSize = 5;
    private const int DaysPerWeek = 7;
    private const int HappyRankThreshold = 3;
    private const float ConcentrationThreshold = 0.4f;
    private const float DayTickSeconds = 1.0f;

    [Header("Data")]
    [SerializeField] private DDDraftDaySO data;
    [SerializeField] private UiManager uiManager;

    [Header("Tabs")]
    [SerializeField] private GameObject draftBoardPanel;
    [SerializeField] private GameObject squadSheetPanel;
    [SerializeField] private GameObject leagueTablePanel;
    [SerializeField] private Button draftBoardTabButton;
    [SerializeField] private Button squadSheetTabButton;
    [SerializeField] private Button leagueTableTabButton;

    [Header("Coach")]
    [SerializeField] private DDCoachView draftBoardCoach;
    [SerializeField] private DDCoachView squadSheetCoach;
    [SerializeField] private DDCoachView leagueTableCoach;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite sadSprite;

    [Header("Draft Board")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float secondsPerPick = 45f;
    [SerializeField] private DDConstraintView[] constraintViews;
    [SerializeField] private Color constraintOpenColor = new(0.85f, 0.62f, 0.20f);
    [SerializeField] private Color constraintFilledColor = new(0.24f, 0.75f, 0.51f);
    [SerializeField] private Color constraintNeutralColor = new(0.30f, 0.32f, 0.36f);
    [SerializeField] private Color constraintMaxedColor = new(0.78f, 0.29f, 0.29f);
    [SerializeField] private Transform poolContent;
    [SerializeField] private DDPoolRow poolRowPrefab;
    [SerializeField] private Button confirmPickButton;

    [Header("Squad Sheet")]
    [SerializeField] private Transform sheetContent;
    [SerializeField] private DDSquadRow squadRowPrefab;
    [SerializeField] private TMP_Text spreadRatingText;
    [SerializeField] private Button lockRosterButton;

    [Header("League Table")]
    [SerializeField] private RectTransform leagueContent;
    [SerializeField] private DDLeagueRow leagueRowPrefab;
    [SerializeField] private TMP_Text infoText;

    [Header("Result")]
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button homeButton;

    private readonly List<DDPoolRow> poolRows = new();
    private readonly List<DDSquadRow> squadRows = new();
    private readonly List<DDLeagueEntry> leagueEntries = new();
    private readonly List<DDRosterEntry> playerRoster = new();

    private DraftDayPhase phase;
    private DDPoolRow selectedRow;
    private int currentRound;
    private float pickTimeRemaining;
    private bool isRebalancing;
    private Coroutine weekRoutine;

    private class DDLeagueEntry
    {
        public string displayName;
        public List<DDRosterEntry> roster;
        public bool isPlayer;
        public float score;
        public DDLeagueRow row;
    }

    private void Awake()
    {
        draftBoardTabButton.onClick.AddListener(() => ShowTab(draftBoardPanel));
        squadSheetTabButton.onClick.AddListener(() => ShowTab(squadSheetPanel));
        leagueTableTabButton.onClick.AddListener(() => ShowTab(leagueTablePanel));
        confirmPickButton.onClick.AddListener(ConfirmPick);
        lockRosterButton.onClick.AddListener(LockRoster);
        homeButton.onClick.AddListener(ReturnHome);
    }

    private void OnEnable()
    {
        BeginGame();
    }

    private void Update()
    {
        if (phase != DraftDayPhase.Drafting) return;

        pickTimeRemaining -= Time.deltaTime;
        if (pickTimeRemaining <= 0f)
        {
            AutoPick();
            return;
        }

        int seconds = Mathf.CeilToInt(pickTimeRemaining);
        timerText.text = $"{seconds / 60} : {seconds % 60:00}";
    }

    /// <summary>Resets all state and starts a fresh draft.</summary>
    public void BeginGame()
    {
        StopWeekRoutine();

        phase = DraftDayPhase.Drafting;
        currentRound = 1;
        selectedRow = null;
        playerRoster.Clear();

        resultPopup.SetActive(false);
        BuildPool();
        BuildLeague();
        RebuildSquadSheet();

        infoText.text = data.infoDrafting;
        lockRosterButton.interactable = false;
        spreadRatingText.text = "—";

        RefreshConstraints();
        RefreshPool();
        RefreshRoundLabel();
        ResetPickTimer();

        ShowCoach(draftBoardCoach, DDCoachSituation.ConstraintOpen);
        ShowTab(draftBoardPanel);
    }

    private void BuildPool()
    {
        foreach (DDPoolRow row in poolRows) Destroy(row.gameObject);
        poolRows.Clear();

        foreach (DDAssetSO asset in data.pool)
        {
            DDPoolRow row = Instantiate(poolRowPrefab, poolContent);
            row.Bind(asset, OnPoolRowClicked);
            poolRows.Add(row);
        }
    }

    private void BuildLeague()
    {
        foreach (DDLeagueEntry entry in leagueEntries)
        {
            if (entry.row != null)
            {
                Destroy(entry.row.gameObject);
            }
        }

        leagueEntries.Clear();

        foreach (DDOpponent opponent in data.opponents)
        {
            leagueEntries.Add(new DDLeagueEntry
            {
                displayName = opponent.displayName,
                roster = opponent.roster,
                isPlayer = false
            });
        }

        leagueEntries.Add(new DDLeagueEntry
        {
            displayName = data.playerEntryName,
            roster = playerRoster,
            isPlayer = true
        });

        VerticalLayoutGroup layoutGroup = leagueContent.GetComponent<VerticalLayoutGroup>();

        ContentSizeFitter sizeFitter = leagueContent.GetComponent<ContentSizeFitter>();

        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
        }

        if (sizeFitter != null)
        {
            sizeFitter.enabled = true;
        }

        for (int i = 0; i < leagueEntries.Count; i++)
        {
            DDLeagueRow row = Instantiate(leagueRowPrefab, leagueContent);

            row.Bind(leagueEntries[i].displayName, leagueEntries[i].isPlayer);

            row.SetRank(i + 1);
            leagueEntries[i].row = row;
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(leagueContent
        );
    }

    private void OnPoolRowClicked(DDPoolRow row)
    {
        if (phase != DraftDayPhase.Drafting) return;

        selectedRow = row;
        RefreshPool();
    }

    /// <summary>Commits the highlighted asset to the roster and advances the round.</summary>
    public void ConfirmPick()
    {
        if (phase != DraftDayPhase.Drafting || selectedRow == null) return;
        AddToRoster(selectedRow);
    }

    private void AutoPick()
    {
        DDPoolRow best = null;
        float bestScore = float.MinValue;

        foreach (DDPoolRow row in poolRows)
        {
            if (row.State == DDPoolRowState.Rostered || !IsPickable(row.Asset)) continue;

            List<DDRosterEntry> single = new()
            {
                new DDRosterEntry { asset = row.Asset, weight = DraftDayScoring.TotalWeightPercent }
            };

            float score = DraftDayScoring.CalculateScore(single, DaysPerWeek);
            if (score <= bestScore) continue;

            bestScore = score;
            best = row;
        }

        if (best != null) AddToRoster(best);
        else ResetPickTimer();
    }

    private void AddToRoster(DDPoolRow row)
    {
        row.SetState(DDPoolRowState.Rostered);
        selectedRow = null;

        playerRoster.Add(new DDRosterEntry { asset = row.Asset, weight = 0 });
        DraftDayScoring.ApplyEqualWeights(playerRoster);

        RefreshConstraints();
        RebuildSquadSheet();
        RefreshSpreadRating();

        if (playerRoster.Count >= RosterSize)
        {
            EnterWeighting();
            return;
        }

        currentRound++;
        RefreshRoundLabel();
        RefreshPool();
        ResetPickTimer();
        PlayNextPickAnimation();

        ShowCoach(draftBoardCoach,
            CountCategory(DDAssetCategory.Crypto) > 0 && row.Asset.category == DDAssetCategory.Crypto
                ? DDCoachSituation.CryptoFilled
                : DDCoachSituation.ConstraintOpen);
    }

    private void PlayNextPickAnimation()
    {
        for (int i = 0; i < poolRows.Count; i++)
        {
            if (poolRows[i].State == DDPoolRowState.Rostered)
            {
                continue;
            }

            poolRows[i].PlayRefreshAnimation(i * 0.025f);
        }

        RectTransform confirmRect = (RectTransform)confirmPickButton.transform;
        confirmRect.DOKill();
        confirmRect.DOPunchScale(Vector3.one * 0.05f, 0.3f, 1, 0.5f);

        RectTransform roundRect = roundText.rectTransform;
        roundRect.DOKill();
        roundRect.DOPunchScale(Vector3.one * 0.08f, 0.35f, 1, 0.5f);
    }

    private void EnterWeighting()
    {
        phase = DraftDayPhase.Weighting;
        RefreshPool();
        SetSquadInteractable(true);
        lockRosterButton.interactable = true;
        ShowCoach(squadSheetCoach, DDCoachSituation.RosterComplete);
        ShowTab(squadSheetPanel);
    }

    private void RebuildSquadSheet()
    {
        foreach (DDSquadRow row in squadRows) Destroy(row.gameObject);
        squadRows.Clear();

        for (int i = 0; i < playerRoster.Count; i++)
        {
            DDSquadRow row = Instantiate(squadRowPrefab, sheetContent);
            row.Bind(i, playerRoster[i].asset.displayName, playerRoster[i].weight, OnWeightChanged);
            row.SetInteractable(phase == DraftDayPhase.Weighting);
            squadRows.Add(row);
        }
    }

    private void OnWeightChanged(int index, int newWeight)
    {
        if (isRebalancing || phase != DraftDayPhase.Weighting) return;

        isRebalancing = true;
        DraftDayScoring.RebalanceAround(playerRoster, index, newWeight);

        for (int i = 0; i < squadRows.Count; i++) squadRows[i].SetWeight(playerRoster[i].weight);
        isRebalancing = false;

        RefreshSpreadRating();
        ShowCoach(squadSheetCoach,
            DraftDayScoring.LargestWeightShare(playerRoster) >= ConcentrationThreshold
                ? DDCoachSituation.WeightsConcentrated
                : DDCoachSituation.WeightsBalanced);
    }

    private void RefreshSpreadRating()
    {
        if (playerRoster.Count == 0)
        {
            spreadRatingText.text = "—";
            return;
        }

        float score = DraftDayScoring.CalculateScore(playerRoster, DaysPerWeek);
        spreadRatingText.text = DraftDayScoring.CalculateSpreadRating(score).ToString();
    }

    /// <summary>Freezes the weights and plays the week out day by day.</summary>
    public void LockRoster()
    {
        if (phase != DraftDayPhase.Weighting) return;

        phase = DraftDayPhase.PlayingWeek;
        SetSquadInteractable(false);
        lockRosterButton.interactable = false;
        infoText.text = data.infoPlayingWeek;

        ShowTab(leagueTablePanel);
        weekRoutine = StartCoroutine(PlayWeekRoutine());
    }

    private IEnumerator PlayWeekRoutine()
    {
        for (int day = 1; day <= DaysPerWeek; day++)
        {
            yield return new WaitForSeconds(DayTickSeconds);
            ResolveStandings(day);
        }

        phase = DraftDayPhase.Finished;
        Invoke(nameof(ShowResult), 1.5f);
    }

    private void ResolveStandings(int dayCount)
    {
        foreach (DDLeagueEntry entry in leagueEntries)
        {
            entry.score = DraftDayScoring.CalculateScore(
                entry.roster,
                dayCount
            );
        }

        // Day 1 has no usable volatility score yet.
        // Keep the initial order instead of sorting all zero scores.
        if (dayCount >= DraftDayScoring.MinimumDaysForScore)
        {
            leagueEntries.Sort((left, right) =>
            {
                int scoreComparison = right.score.CompareTo(left.score);

                if (scoreComparison != 0)
                {
                    return scoreComparison;
                }

                return string.CompareOrdinal(left.displayName, right.displayName);
            });
        }

        for (int i = 0; i < leagueEntries.Count; i++)
        {
            DDLeagueEntry entry = leagueEntries[i];

            entry.row.SetRank(i + 1);

            if (dayCount < DraftDayScoring.MinimumDaysForScore)
            {
                entry.row.SetPendingScore();
            }
            else
            {
                entry.row.SetScore(entry.score);
            }

            entry.row.transform.SetSiblingIndex(i);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(leagueContent);
    }

    private void ShowResult()
    {
        int rank = 1;
        for (int i = 0; i < leagueEntries.Count; i++)
        {
            if (leagueEntries[i].isPlayer) rank = i + 1;
        }

        float weekReturn = DraftDayScoring.CalculateWeekReturn(playerRoster, DaysPerWeek);
        float score = DraftDayScoring.CalculateScore(playerRoster, DaysPerWeek);

        infoText.text = string.Format(data.infoResolved, weekReturn.ToString("0.0"), score.ToString("0.00"));

        DDCoachSituation situation = rank <= HappyRankThreshold
            ? DDCoachSituation.ResultTop
            : rank >= leagueEntries.Count
                ? DDCoachSituation.ResultBottom
                : DDCoachSituation.ResultMid;

        ShowCoach(leagueTableCoach, situation);

        if (resultText != null)
            resultText.text = $"Rank {rank} of {leagueEntries.Count}\nWeek return {weekReturn:0.0}%   Score {score:0.00}\n" + leagueTableCoach.dialogueText.text;

        //resultPopup.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.GameComplete);
    }

    /// <summary>Closes the minigame and returns to the home panel.</summary>
    public void ReturnHome()
    {
        ResetGameState();

        uiManager.ToggleAllPanels(false);
        uiManager.ToggleMiniGamesPanel(true);
    }

    /// <summary>Clears all runtime Draft Day data so the next session starts from round one.</summary>
    private void ResetGameState()
    {
        StopWeekRoutine();

        phase = DraftDayPhase.Drafting;
        selectedRow = null;
        currentRound = 1;
        pickTimeRemaining = 0f;
        isRebalancing = false;

        playerRoster.Clear();

        foreach (DDPoolRow row in poolRows)
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        poolRows.Clear();

        foreach (DDSquadRow row in squadRows)
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        squadRows.Clear();

        foreach (DDLeagueEntry entry in leagueEntries)
        {
            if (entry.row != null)
            {
                Destroy(entry.row.gameObject);
            }
        }

        leagueEntries.Clear();

        resultPopup.SetActive(false);
    }

    private void RefreshPool()
    {
        foreach (DDPoolRow row in poolRows)
        {
            if (row.State == DDPoolRowState.Rostered) continue;

            if (phase != DraftDayPhase.Drafting)
            {
                row.SetState(DDPoolRowState.Locked);
                continue;
            }

            bool pickable = IsPickable(row.Asset);
            if (!pickable && row == selectedRow) selectedRow = null;
            row.SetState(!pickable ? DDPoolRowState.Locked
                : row == selectedRow ? DDPoolRowState.Selected
                : DDPoolRowState.Available);
        }

        confirmPickButton.interactable = phase == DraftDayPhase.Drafting && selectedRow != null;
    }

    private bool IsPickable(DDAssetSO asset)
    {
        foreach (DDCategoryRule rule in data.rules)
        {
            if (rule.ruleType != DDRuleType.Maximum) continue;
            if (rule.category == asset.category && CountCategory(rule.category) >= rule.amount) return false;
        }

        int slotsAfterPick = RosterSize - (playerRoster.Count + 1);
        int outstanding = 0;

        foreach (DDCategoryRule rule in data.rules)
        {
            if (rule.ruleType != DDRuleType.Minimum) continue;
            int projected = CountCategory(rule.category) + (rule.category == asset.category ? 1 : 0);
            outstanding += Mathf.Max(0, rule.amount - projected);
        }

        return outstanding <= slotsAfterPick;
    }

    private void RefreshConstraints()
    {
        for (int i = 0; i < constraintViews.Length && i < data.rules.Count; i++)
        {
            DDCategoryRule rule = data.rules[i];
            int count = CountCategory(rule.category);
            string categoryLabel = rule.category.ToString().ToUpperInvariant();

            if (rule.ruleType == DDRuleType.Minimum)
            {
                bool filled = count >= rule.amount;
                constraintViews[i].label.text = $"{categoryLabel} {(filled ? "FILLED" : "OPEN")}";
                constraintViews[i].background.color = filled ? constraintFilledColor : constraintOpenColor;
            }
            else
            {
                constraintViews[i].label.text = $"{categoryLabel} {count} / {rule.amount}";
                constraintViews[i].background.color = count >= rule.amount ? constraintMaxedColor : constraintNeutralColor;
            }
        }
    }

    private int CountCategory(DDAssetCategory category)
    {
        int count = 0;
        foreach (DDRosterEntry entry in playerRoster)
        {
            if (entry.asset != null && entry.asset.category == category) count++;
        }
        return count;
    }

    private void ShowCoach(DDCoachView view, DDCoachSituation situation)
    {
        if (!data.TryGetCoachLine(situation, out string line, out DDCoachMood mood)) return;

        view.dialogueText.text = line;
        Sprite sprite = mood == DDCoachMood.Happy ? happySprite : mood == DDCoachMood.Sad ? sadSprite : neutralSprite;

        draftBoardCoach.mascotImage.sprite = sprite;
        squadSheetCoach.mascotImage.sprite = sprite;
        leagueTableCoach.mascotImage.sprite = sprite;
    }

    private void SetSquadInteractable(bool value)
    {
        foreach (DDSquadRow row in squadRows) row.SetInteractable(value);
    }

    private void ShowTab(GameObject panel)
    {
        draftBoardPanel.SetActive(panel == draftBoardPanel);
        squadSheetPanel.SetActive(panel == squadSheetPanel);
        leagueTablePanel.SetActive(panel == leagueTablePanel);
    }

    private void RefreshRoundLabel()
    {
        roundText.text = $"Round {currentRound} of {RosterSize}";
    }

    private void ResetPickTimer()
    {
        pickTimeRemaining = secondsPerPick;
    }

    private void StopWeekRoutine()
    {
        if (weekRoutine == null) return;
        StopCoroutine(weekRoutine);
        weekRoutine = null;
    }

    /// <summary>Verification pass: logs every authored roster's score against the design targets.</summary>
    [ContextMenu("Log All Scores")]
    private void LogAllScores()
    {
        foreach (DDAssetSO asset in data.pool)
        {
            List<DDRosterEntry> single = new()
            {
                new DDRosterEntry { asset = asset, weight = DraftDayScoring.TotalWeightPercent }
            };
            Debug.Log($"[Asset] {asset.displayName}: week {DraftDayScoring.CalculateWeekReturn(single, DaysPerWeek):0.00}%  score {DraftDayScoring.CalculateScore(single, DaysPerWeek):0.000}");
        }

        foreach (DDOpponent opponent in data.opponents)
        {
            Debug.Log($"[Opponent] {opponent.displayName}: week {DraftDayScoring.CalculateWeekReturn(opponent.roster, DaysPerWeek):0.00}%  score {DraftDayScoring.CalculateScore(opponent.roster, DaysPerWeek):0.000}");
        }
    }
}
