using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Licence screen shown in the mock:
///   - top progress readout ("2 OF 3" + pip bar)
///   - a status row per feature (Open / "N run away" / Locked)
///
/// Attach this to the Licence panel and hook up the fields in the
/// Inspector. Call Refresh() whenever the panel becomes visible
/// (OnEnable already does this) so it always shows current progress.
/// </summary>
public class RiskLicenceScreenController : MonoBehaviour
{
    [Header("Progress")]
    [Tooltip("Shows e.g. '2 OF 3'.")]
    [SerializeField] private TMP_Text progressText;

    [Tooltip("One Image per pip in the progress bar, left to right.")]
    [SerializeField] private Image[] progressSegments;

    [SerializeField] private Color segmentFilledColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color segmentEmptyColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Feature Rows")]
    [SerializeField] private LicenceRowUI spotTradingRow;
    [SerializeField] private LicenceRowUI stopLimitRow;
    [SerializeField] private LicenceRowUI margin2xRow;
    [SerializeField] private LicenceRowUI margin5xRow;

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// Re-reads RiskLicenceManager and repaints every row + the progress bar.
    /// Safe to call any time (e.g. after a new run finishes).
    /// </summary>
    public void Refresh()
    {
        int clean = RiskLicenceManager.CleanRuns;

        // The progress bar in the mock tracks toward the Margin 2x gate
        // (3 clean runs). Swap this for a different target if your
        // gate structure changes.
        int gateTarget = RiskLicenceManager.Margin2xRequirement;
        int progressShown = Mathf.Min(clean, gateTarget);

        if (progressText != null)
            progressText.text = $"{progressShown} OF {gateTarget}";

        if (progressSegments != null)
        {
            for (int i = 0; i < progressSegments.Length; i++)
            {
                bool filled = i < progressShown;
                progressSegments[i].color = filled ? segmentFilledColor : segmentEmptyColor;
            }
        }

        // Before the player has finished even one run (win or bust),
        // there's no track record yet - show everything as Locked no
        // matter what the individual thresholds say. As soon as the
        // first run ends, this flips and every row shows real progress.
        if (!RiskLicenceManager.HasPlayedAnyRun)
        {
            spotTradingRow?.SetLocked();
            stopLimitRow?.SetLocked();
            margin2xRow?.SetLocked();
            margin5xRow?.SetLocked();
            return;
        }

        UpdateRow(spotTradingRow, RiskLicenceManager.IsSpotTradingUnlocked,
            RiskLicenceManager.SpotTradingRequirement - clean);

        UpdateRow(stopLimitRow, RiskLicenceManager.IsStopLimitUnlocked,
            RiskLicenceManager.StopLimitRequirement - clean);

        UpdateRow(margin2xRow, RiskLicenceManager.IsMargin2xUnlocked,
            RiskLicenceManager.Margin2xRequirement - clean);

        UpdateRow(margin5xRow, RiskLicenceManager.IsMargin5xUnlocked,
            RiskLicenceManager.Margin5xRequirement - clean);
    }

    private void UpdateRow(LicenceRowUI row, bool unlocked, int runsRemaining)
    {
        if (row == null) return;

        if (unlocked)
            row.SetOpen();
        else if (runsRemaining <= 1)
            row.SetRunsAway(runsRemaining);
        else
            row.SetLocked();
    }
}

/// <summary>
/// One row in the licence list (e.g. "Margin 2x  —  1 run away").
/// Assign the status TMP_Text in the Inspector; this just repaints
/// its text + color.
/// </summary>
[System.Serializable]
public class LicenceRowUI
{
    public TMP_Text statusText;

    public Color openColor = new Color(0.3f, 0.8f, 0.4f);
    public Color neutralColor = Color.white;
    public Color lockedColor = new Color(1f, 1f, 1f, 0.4f);

    public void SetOpen()
    {
        if (statusText == null) return;
        statusText.text = "Open";
        statusText.color = openColor;
    }

    public void SetRunsAway(int runs)
    {
        if (statusText == null) return;
        statusText.text = runs <= 0 ? "Open" : $"{runs} run away";
        statusText.color = neutralColor;
    }

    public void SetLocked()
    {
        if (statusText == null) return;
        statusText.text = "Locked";
        statusText.color = lockedColor;
    }
}