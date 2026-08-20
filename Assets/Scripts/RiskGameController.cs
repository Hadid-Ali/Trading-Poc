using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RiskGameController : MonoBehaviour
{
    // =========================================================
    // GAME SETTINGS
    // =========================================================

    [Header("Game Settings")]
    [SerializeField] private float startingStack = 1000f;

    [Tooltip("0.55 = 55% win chance")]
    [SerializeField, Range(0f, 1f)]
    private float winChance = 0.55f;

    [SerializeField] private int totalRounds = 100;

    [Tooltip("Recommended sizing shown on result screen.")]
    [SerializeField, Range(0f, 100f)]
    private float recommendedStakePercent = 5f;


    // =========================================================
    // GAMEPLAY SCREEN - TOP
    // =========================================================

    [Header("Gameplay - Top UI")]

    [SerializeField] private TMP_Text roundText;

    [SerializeField] private TMP_Text edgeText;


    // =========================================================
    // GAMEPLAY SCREEN - STACK
    // =========================================================

    [Header("Gameplay - Stack UI")]

    [SerializeField] private TMP_Text stackText;


    // =========================================================
    // STAKE SLIDER
    // =========================================================

    [Header("Stake Slider")]

    [SerializeField] private Slider stakeSlider;

    [SerializeField] private TMP_Text stakePercentText;

    [Tooltip("Optional. If you have a text showing actual money being staked.")]
    //[SerializeField] private TMP_Text stakeAmountText;

    [SerializeField] private TMP_Text stakeMessageText;


    // =========================================================
    // FLIP
    // =========================================================

    [Header("Flip")]

    [SerializeField] private Button flipButton;


    // =========================================================
    // QUICK WIN / LOSS FEEDBACK
    // =========================================================

    [Header("Round Feedback")]

    [SerializeField] private GameObject feedbackObject;

    [SerializeField] private TMP_Text feedbackText;

    [SerializeField] private float feedbackDuration = 0.7f;


    // =========================================================
    // PANEL NAVIGATION (Stake / Result / Licence tabs)
    // =========================================================

    [Header("Panel Navigation")]

    [Tooltip("The three panels the tab bar switches between.")]
    [SerializeField] private GameObject gameplayPanel;

    [SerializeField] private GameObject resultPanel;

    [SerializeField] private GameObject licencePanel;

    [Tooltip("Optional reference so the Licence panel repaints itself the moment it's shown.")]
    [SerializeField] private RiskLicenceScreenController licenceScreenController;

    [Tooltip("Always available - switches to the Stake (gameplay) panel.")]
    [SerializeField] private Button stakeTabButton;

    [Tooltip("Only works once the current run has finished (win or bust). Stays non-interactable before that.")]
    [SerializeField] private Button resultTabButton;

    [Tooltip("Always available - shows the Licence panel. The panel's own content shows locked/unlocked rows regardless.")]
    [SerializeField] private Button licenceTabButton;

    [Tooltip("Typically placed on the Result panel. Resets the run (round 1, stack back to starting value) WITHOUT touching licence progress.")]
    [SerializeField] private UiManager uiManager;
    //[SerializeField] private Button playAgainButton;

    private enum GamePanel
    {
        Stake,
        Result,
        Licence
    }


    // =========================================================
    // RESULT SCREEN
    // =========================================================

    [Header("Result Screen")]

    // Result screen top
    [SerializeField] private TMP_Text resultRoundText;

    [SerializeField] private TMP_Text resultTitleText;


    // Result screen avatar
    [SerializeField] private Image resultAvatarImage;

    [SerializeField] private Sprite bustAvatar;

    [SerializeField] private Sprite surviveAvatar;


    // Result graph
    [SerializeField] private Image resultGraphImage;

    [SerializeField] private Sprite bustGraph;

    [SerializeField] private Sprite surviveGraph;


    // Result texts
    [SerializeField] private TMP_Text resultRiskText;

    [SerializeField] private TMP_Text playersBustedText;

    [SerializeField] private TMP_Text resultMessageText;


    // =========================================================
    // RESULT SCREEN - STATIC COHORT DATA FOR NOW
    // =========================================================

    [Header("Result - Temporary Cohort Data")]

    [Tooltip("Temporary value until real daily cohort data is implemented.")]
    [SerializeField, Range(0f, 100f)]
    private float playersBustedPercent = 71f;


    // =========================================================
    // RUNTIME DATA
    // =========================================================

    private float currentStack;

    private float selectedStakePercent;

    private float currentStakeAmount;

    private int currentRound;

    private int totalWins;

    private int totalLosses;

    private float totalStakePercent;

    private bool roundProcessing;

    private bool gameFinished;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        InitializeGame();
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeGame()
    {
        currentStack = startingStack;

        currentRound = 1;

        selectedStakePercent = 0f;

        currentStakeAmount = 0f;

        totalWins = 0;

        totalLosses = 0;

        totalStakePercent = 0f;

        roundProcessing = false;

        gameFinished = false;


        // -----------------------------
        // Slider
        // -----------------------------

        stakeSlider.minValue = 0f;
        stakeSlider.maxValue = 100f;
        stakeSlider.wholeNumbers = false;
        stakeSlider.value = 0f;

        stakeSlider.onValueChanged.RemoveAllListeners();
        stakeSlider.onValueChanged.AddListener(OnStakeSliderChanged);


        // -----------------------------
        // Flip Button
        // -----------------------------

        flipButton.onClick.RemoveAllListeners();
        flipButton.onClick.AddListener(OnFlipClicked);


        // -----------------------------
        // Tab Buttons
        // -----------------------------

        if (stakeTabButton != null)
        {
            stakeTabButton.onClick.RemoveAllListeners();
            stakeTabButton.onClick.AddListener(() => ShowPanel(GamePanel.Stake));
        }

        if (resultTabButton != null)
        {
            resultTabButton.onClick.RemoveAllListeners();
            resultTabButton.onClick.AddListener(() => ShowPanel(GamePanel.Result));

            // No run has finished yet - Result tab starts locked.
            resultTabButton.interactable = false;
        }

        if (licenceTabButton != null)
        {
            licenceTabButton.onClick.RemoveAllListeners();
            licenceTabButton.onClick.AddListener(() => ShowPanel(GamePanel.Licence));
        }

        // -----------------------------
        // Feedback
        // -----------------------------

        if (feedbackObject != null)
            feedbackObject.SetActive(false);


        // Start on the Stake panel.
        ShowPanel(GamePanel.Stake);


        UpdateGameplayUI();
    }


    // =========================================================
    // PLAY AGAIN
    // =========================================================

    /// <summary>
    /// Resets the current run back to round 1 / starting stack and
    /// returns to the Stake panel. Does NOT touch RiskLicenceManager -
    /// licence progress (clean run count) is a separate, persisted
    /// track record and should survive across runs.
    /// </summary>
    public void RestartRun()
    {
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(false);
        licencePanel.SetActive(false);
        {
            if (uiManager != null)
            {
                uiManager.ToggleAllPanels(false);
                uiManager.ToggleMiniGamesPanel(true);
            }
        }
        flipButton.interactable = true;
        stakeSlider.interactable = true;

        InitializeGame();
    }


    // =========================================================
    // PANEL SWITCHING
    // =========================================================

    private void ShowPanel(GamePanel panel)
    {
        // Guard: can't open Result until the current run has actually
        // finished (win or bust). Button is also non-interactable, this
        // is just a safety check in case it's called from elsewhere.
        if (panel == GamePanel.Result && !gameFinished)
            return;

        if (gameplayPanel != null)
            gameplayPanel.SetActive(panel == GamePanel.Stake);

        if (resultPanel != null)
            resultPanel.SetActive(panel == GamePanel.Result);

        if (licencePanel != null)
            licencePanel.SetActive(panel == GamePanel.Licence);

        // Licence panel repaints its own locked/unlocked rows every time
        // it's shown, based on RiskLicenceManager progress.
        if (panel == GamePanel.Licence && licenceScreenController != null)
            licenceScreenController.Refresh();
    }


    // =========================================================
    // SLIDER
    // =========================================================

    private void OnStakeSliderChanged(float value)
    {
        if (gameFinished)
            return;

        selectedStakePercent = value;


        // Calculate actual money being risked.
        currentStakeAmount =
            currentStack * (selectedStakePercent / 100f);


        UpdateStakeUI();
    }


    // =========================================================
    // UPDATE GAMEPLAY UI
    // =========================================================

    private void UpdateGameplayUI()
    {
        // Example:
        // ROUND 3 / 100

        if (roundText != null)
        {
            roundText.text =
                $"ROUND {currentRound} / {totalRounds}";
        }


        // Example:
        // EDGE 55%

        if (edgeText != null)
        {
            edgeText.text =
                $"EDGE {winChance * 100f:0}%";
        }


        // Example:
        // 1,240

        if (stackText != null)
        {
            stackText.text =
                $"{currentStack:N0}";
        }


        UpdateStakeUI();
    }


    // =========================================================
    // UPDATE STAKE UI
    // =========================================================

    private void UpdateStakeUI()
    {
        // Example:
        // 18%

        if (stakePercentText != null)
        {
            stakePercentText.text =
                $"{selectedStakePercent:0}%";
        }


        // Optional:
        // $223

        //if (stakeAmountText != null)
        //{
        //    stakeAmountText.text =
        //        FormatMoney(currentStakeAmount);
        //}


        UpdateStakeMessage();
    }


    // =========================================================
    // STAKE MESSAGE
    // =========================================================

    private void UpdateStakeMessage()
    {
        if (stakeMessageText == null)
            return;


        if (selectedStakePercent <= 0f)
        {
            stakeMessageText.text =
                "No stake. You are sitting this round out.";
        }
        else if (selectedStakePercent <= 5f)
        {
            stakeMessageText.text =
                "Small stake. Your edge has room to compound.";
        }
        else if (selectedStakePercent <= 12f)
        {
            stakeMessageText.text =
                "Controlled sizing. You're giving the edge room to work.";
        }
        else if (selectedStakePercent <= 20f)
        {
            stakeMessageText.text =
                "Above 12% the edge stops compounding and starts eating you.";
        }
        else if (selectedStakePercent <= 50f)
        {
            stakeMessageText.text =
                "Aggressive sizing. A short losing streak can hurt badly.";
        }
        else
        {
            stakeMessageText.text =
                "Dangerous sizing. One loss can take a huge part of your stack.";
        }
    }


    // =========================================================
    // FLIP
    // =========================================================

    private void OnFlipClicked()
    {
        if (roundProcessing)
            return;

        if (gameFinished)
            return;


        StartCoroutine(ResolveRound());
    }


    // =========================================================
    // RESOLVE ROUND
    // =========================================================

    private IEnumerator ResolveRound()
    {
        roundProcessing = true;

        flipButton.interactable = false;
        stakeSlider.interactable = false;


        // -----------------------------------------
        // Calculate stake
        // -----------------------------------------

        currentStakeAmount =
            currentStack *
            (selectedStakePercent / 100f);


        // Save stake percentage for average.
        totalStakePercent += selectedStakePercent;


        // -----------------------------------------
        // Random 55/45 result
        // -----------------------------------------

        float randomValue = Random.value;

        bool playerWon =
            randomValue < winChance;


        // -----------------------------------------
        // WIN
        // -----------------------------------------

        if (playerWon)
        {
            HandleWin();
        }

        // -----------------------------------------
        // LOSS
        // -----------------------------------------

        else
        {
            HandleLoss();
        }


        // Update UI immediately.
        UpdateGameplayUI();


        // -----------------------------------------
        // If game ended, don't go to next round.
        // -----------------------------------------

        if (gameFinished)
        {
            yield break;
        }


        // -----------------------------------------
        // Show WIN / LOSS feedback briefly.
        // -----------------------------------------

        yield return new WaitForSeconds(feedbackDuration);


        if (gameFinished)
        {
            yield break;
        }


        // -----------------------------------------
        // Move to next round.
        // -----------------------------------------

        currentRound++;


        // Reset stake for next round.
        selectedStakePercent = 0f;
        currentStakeAmount = 0f;

        stakeSlider.value = 0f;


        if (feedbackObject != null)
            feedbackObject.SetActive(false);


        UpdateGameplayUI();


        flipButton.interactable = true;
        stakeSlider.interactable = true;

        roundProcessing = false;
    }


    // =========================================================
    // WIN
    // =========================================================

    private void HandleWin()
    {
        AudioManager.Instance.PlaySFX(SoundType.Correct);
        totalWins++;


        float profit =
            currentStakeAmount;


        currentStack += profit;


        ShowRoundFeedback(
            "WIN",
            $"+{FormatMoney(profit)}"
        );


        // Check if this was the final round.
        if (currentRound >= totalRounds)
        {
            FinishGame(true);
        }
    }


    // =========================================================
    // LOSS
    // =========================================================

    private void HandleLoss()
    {
        AudioManager.Instance.PlaySFX(SoundType.Wrong);
        totalLosses++;


        float loss =
            currentStakeAmount;


        currentStack -= loss;


        if (currentStack < 0.01f)
            currentStack = 0f;


        ShowRoundFeedback(
            "LOSS",
            $"-{FormatMoney(loss)}"
        );


        // Check bust first.
        if (currentStack <= 0f)
        {
            FinishGame(false);
            return;
        }


        // If it wasn't a bust, check final round.
        if (currentRound >= totalRounds)
        {
            FinishGame(true);
        }
    }


    // =========================================================
    // QUICK FEEDBACK
    // =========================================================

    private void ShowRoundFeedback(
        string result,
        string amount)
    {
        if (feedbackObject != null)
            feedbackObject.SetActive(true);


        if (feedbackText != null)
        {
            feedbackText.text =
                $"{result}\n{amount}";
        }
    }


    // =========================================================
    // FINISH GAME
    // =========================================================

    private void FinishGame(bool survived)
    {
        gameFinished = true;

        roundProcessing = false;


        flipButton.interactable = false;
        stakeSlider.interactable = false;


        if (feedbackObject != null)
            feedbackObject.SetActive(false);


        // Result tab is now unlocked - the run has actually ended.
        if (resultTabButton != null)
            resultTabButton.interactable = true;


        // Tell the licence system a run happened at all (win or bust).
        // This is what flips the Licence screen from "everything Locked"
        // to showing real per-feature progress.
        RiskLicenceManager.RegisterRunPlayed();


        // Only a survived run (finished all rounds without busting)
        // counts as "clean" toward licence progress. A bust does not
        // register progress, it just ends the run.
        if (survived)
        {
            RiskLicenceManager.RegisterCleanRun();
            ShowSurvivedResult();
        }
        else
        {
            ShowBustResult();
        }


        // Jump straight to the Result panel so the player sees the
        // outcome immediately. They can still use the tab bar afterward
        // to revisit Stake (next run, if you wire that up) or Licence.
        ShowPanel(GamePanel.Result);
    }


    // =========================================================
    // BUST RESULT
    // =========================================================

    private void ShowBustResult()
    {
        // -----------------------------
        // Top round
        // -----------------------------
        
        if (resultRoundText != null)
        {
            resultRoundText.text =
                $"ROUND {currentRound}";
        }


        // -----------------------------
        // Title
        // -----------------------------

        if (resultTitleText != null)
        {
            resultTitleText.text =
                "Bust";
        }


        // -----------------------------
        // Sad Avatar
        // -----------------------------

        if (resultAvatarImage != null)
        {
            resultAvatarImage.sprite =
                bustAvatar;

            resultAvatarImage.enabled =
                bustAvatar != null;
        }


        // -----------------------------
        // Bust Graph
        // -----------------------------

        if (resultGraphImage != null)
        {
            resultGraphImage.sprite =
                bustGraph;

            resultGraphImage.enabled =
                bustGraph != null;
        }


        // -----------------------------
        // Dashed graph text
        // -----------------------------

        if (resultRiskText != null)
        {
            resultRiskText.text =
                $"DASHED: THE SAME EDGE, RISKED AT {recommendedStakePercent:0}%";
        }


        // -----------------------------
        // Players busted
        // -----------------------------

        if (playersBustedText != null)
        {
            playersBustedText.text =
                $"{playersBustedPercent:0}%";
        }


        // -----------------------------
        // Bottom message
        // -----------------------------

        if (resultMessageText != null)
        {
            resultMessageText.text =
                "You had the better side of every single flip.";
        }
    }


    // =========================================================
    // SURVIVE RESULT
    // =========================================================

    private void ShowSurvivedResult()
    {
        // -----------------------------
        // Top round
        // -----------------------------
      
        if (resultRoundText != null)
        {
            resultRoundText.text =
                $"ROUND {totalRounds}";
        }


        // -----------------------------
        // Title
        // -----------------------------

        if (resultTitleText != null)
        {
            resultTitleText.text =
                "Survived";
        }


        // -----------------------------
        // Happy Avatar
        // -----------------------------

        if (resultAvatarImage != null)
        {
            resultAvatarImage.sprite =
                surviveAvatar;

            resultAvatarImage.enabled =
                surviveAvatar != null;
        }


        // -----------------------------
        // Survive Graph
        // -----------------------------

        if (resultGraphImage != null)
        {
            resultGraphImage.sprite =
                surviveGraph;

            resultGraphImage.enabled =
                surviveGraph != null;
        }


        // -----------------------------
        // Result risk text
        // -----------------------------

        float averageStake = 0f;

        if (currentRound > 0)
        {
            averageStake =
                totalStakePercent / currentRound;
        }


        if (resultRiskText != null)
        {
            resultRiskText.text =
                $"THE SAME EDGE, SIZED AT {averageStake:0}%";
        }


        // -----------------------------
        // Players busted
        // -----------------------------

        if (playersBustedText != null)
        {
            playersBustedText.text =
                $"{playersBustedPercent:0}%";
        }


        // -----------------------------
        // Bottom message
        // -----------------------------

        if (resultMessageText != null)
        {
            resultMessageText.text =
                "You kept your sizing under control.";
        }
    }


    // =========================================================
    // MONEY FORMAT
    // =========================================================

    private string FormatMoney(float value)
    {
        return $"${value:N0}";
    }
}