using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpreadUnlockManager : MonoBehaviour
{
    [Header("UNLOCK STATUS TEXTS")]
    [Tooltip("Put unlock/status TMP texts here in the same order as levels.")]
    [SerializeField]
    private List<TMP_Text> unlockStatusTexts =
        new List<TMP_Text>();

    [Header("TEXT")]
    [SerializeField]
    private string lockedText = "Locked";

    [SerializeField]
    private string liveText = "Live";

    [Header("COLORS")]
    [SerializeField]
    private Color liveColor = Color.green;

    [SerializeField]
    private Color lockedColor = Color.white;

    private int completedLevels = 0;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        UpdateUnlockUI();
    }

    // =========================================================
    // LEVEL COMPLETED CORRECTLY
    // =========================================================

    public void LevelCompletedCorrectly(int levelIndex)
    {
        if (levelIndex < 0 ||
            levelIndex >= unlockStatusTexts.Count)
            return;

        // Only progress forward.
        if (levelIndex + 1 > completedLevels)
        {
            completedLevels =
                levelIndex + 1;
        }

        UpdateUnlockUI();
    }

    // =========================================================
    // UPDATE UI
    // =========================================================

    private void UpdateUnlockUI()
    {
        for (int i = 0;
             i < unlockStatusTexts.Count;
             i++)
        {
            TMP_Text statusText =
                unlockStatusTexts[i];

            if (statusText == null)
                continue;

            if (i < completedLevels)
            {
                // -----------------------------
                // UNLOCKED
                // -----------------------------

                statusText.text =
                    liveText;

                statusText.color =
                    liveColor;
            }
            else
            {
                // -----------------------------
                // LOCKED
                // -----------------------------

                statusText.text =
                    lockedText;

                statusText.color =
                    lockedColor;
            }
        }
    }
    public int GetNextLevel()
    {
        if (unlockStatusTexts.Count == 0)
            return 0;

        // All levels completed
        if (completedLevels >= unlockStatusTexts.Count)
        {
            completedLevels = 0;
            UpdateUnlockUI();

            return 0;
        }

        // Continue from next incomplete level
        return completedLevels;
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetUnlocks()
    {
        completedLevels = 0;

        UpdateUnlockUI();
    }
}