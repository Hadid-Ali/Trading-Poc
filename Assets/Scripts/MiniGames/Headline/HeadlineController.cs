using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HeadlineController : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Flat,
        Down
    }

    public enum Magnitude
    {
        Under1Percent,
        OneToThreePercent,
        Over3Percent
    }

    [Header("Headline Data")]
    [SerializeField] private HeadlineGameDataSO gameData;
    [SerializeField] private int headlineIndex = 0;

    private HeadlineDataSO currentHeadline;

    // =========================================================
    // DAILY SYSTEM
    // =========================================================

    private const string LastPlayedDateKey = "Headline_LastPlayedDate";
    private const string StreakKey = "Headline_Streak";

    private const string SavedAssetKey = "Headline_SavedAsset";
    private const string SavedDirectionKey = "Headline_SavedDirection";
    private const string SavedMagnitudeKey = "Headline_SavedMagnitude";

    private bool alreadyPlayedToday;

    public int CurrentStreak { get; private set; }

    // =========================================================
    // SCREENS
    // =========================================================

    [Header("Screens")]
    [SerializeField] private GameObject headlineScreen;
    [SerializeField] private GameObject predictionScreen;
    [SerializeField] private GameObject resultScreen;

    // =========================================================
    // NAVIGATION BUTTONS
    // =========================================================

    [Header("Navigation Buttons")]
    [SerializeField] private Button headlineNavButton;
    [SerializeField] private Button predictionNavButton;
    [SerializeField] private Button resultNavButton;

    // =========================================================
    // HEADLINE UI
    // =========================================================

    [Header("Headline UI")]
    [SerializeField] private TMP_Text headlineText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Daily Date UI")]
    [SerializeField] private TMP_Text dateText;

    // =========================================================
    // ASSET BUTTONS
    // =========================================================

    [Header("Asset Buttons")]
    [SerializeField] private Button goldButton;
    [SerializeField] private Button bankIndexButton;
    [SerializeField] private Button bitcoinButton;
    [SerializeField] private Button oilButton;
    [SerializeField] private Button techETFButton;
    [SerializeField] private Button dollarButton;

    // =========================================================
    // PREDICTION UI
    // =========================================================

    [Header("Prediction UI")]
    [SerializeField] private TMP_Text selectedAssetText;

    // =========================================================
    // DIRECTION BUTTONS
    // =========================================================

    [Header("Direction Buttons")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button flatButton;
    [SerializeField] private Button downButton;

    // =========================================================
    // MAGNITUDE BUTTONS
    // =========================================================

    [Header("Magnitude Buttons")]
    [SerializeField] private Button under1Button;
    [SerializeField] private Button oneToThreeButton;
    [SerializeField] private Button over3Button;

    // =========================================================
    // LOCK
    // =========================================================

    [Header("Lock Call")]
    [SerializeField] private Button lockCallButton;

    // =========================================================
    // COLORS
    // =========================================================

    [Header("Selection Colors")]
    [SerializeField]
    private Color selectedButtonColor = new Color(0.65f, 0.45f, 1f);

    [SerializeField]
    private Color normalButtonColor = Color.white;

    [SerializeField]
    private Color selectedTextColor = Color.black;

    [SerializeField]
    private Color normalTextColor = Color.black;

    // =========================================================
    // RESULT UI
    // =========================================================

    [Header("Result UI")]
    [SerializeField] private TMP_Text actualText;
    [SerializeField] private TMP_Text youSaidText;
    [SerializeField] private TMP_Text resultMessageText;

    // =========================================================
    // STREAK UI
    // =========================================================

    [Header("Daily Result UI")]
    [SerializeField] private TMP_Text playedInRowText;

    // =========================================================
    // RESULT AVATAR
    // =========================================================

    [Header("Result Avatar")]
    [SerializeField] private Image resultAvatar;
    [SerializeField] private Sprite happyAvatar;
    [SerializeField] private Sprite sadAvatar;

    // =========================================================
    // CROWD RESULT
    // =========================================================

    [Header("Crowd Result - Row 1")]
    [SerializeField] private TMP_Text crowdLabel1;
    [SerializeField] private TMP_Text crowdPercentage1;
    [SerializeField] private Slider crowdSlider1;

    [Header("Crowd Result - Row 2")]
    [SerializeField] private TMP_Text crowdLabel2;
    [SerializeField] private TMP_Text crowdPercentage2;
    [SerializeField] private Slider crowdSlider2;

    [Header("Crowd Result - Row 3")]
    [SerializeField] private TMP_Text crowdLabel3;
    [SerializeField] private TMP_Text crowdPercentage3;
    [SerializeField] private Slider crowdSlider3;

    // =========================================================
    // PLAYER SELECTION
    // =========================================================

    private HeadlineAsset selectedAsset;
    private Direction selectedDirection;
    private Magnitude selectedMagnitude;

    private bool assetSelected;
    private bool directionSelected;
    private bool magnitudeSelected;

    [SerializeField] private UiManager uiManager;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        SetupButtons();
        SetupNavigationButtons();
        InitializeDailyHeadline();
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void SetupNavigationButtons()
    {
        if (headlineNavButton != null)
        {
            headlineNavButton.onClick.AddListener(
                ShowHeadlineScreen
            );
        }

        if (predictionNavButton != null)
        {
            predictionNavButton.onClick.AddListener(
                OnPredictionNavClicked
            );
        }

        if (resultNavButton != null)
        {
            resultNavButton.onClick.AddListener(
                OnResultNavClicked
            );
        }
    }

    private void OnPredictionNavClicked()
    {
        // Before selecting an asset for today's game,
        // prediction screen should not open directly.
        if (!alreadyPlayedToday && !assetSelected)
            return;

        ShowPredictionScreen();
    }

    private void OnResultNavClicked()
    {
        // Before completing today's game,
        // result screen should not open directly.
        if (!alreadyPlayedToday)
            return;

        ShowResultScreen();
    }

    // =========================================================
    // DAILY INITIALIZATION
    // =========================================================

    private void InitializeDailyHeadline()
    {
        if (gameData == null)
        {
            Debug.LogError(
                "HeadlineController: Game Data is missing."
            );
            return;
        }

        if (gameData.headlines == null ||
            gameData.headlines.Count == 0)
        {
            Debug.LogError(
                "HeadlineController: No headlines found."
            );
            return;
        }

        string today = GetTodayKey();

        alreadyPlayedToday = IsPlayedToday(today);

        headlineIndex = GetDailyHeadlineIndex();

        Debug.Log(
            $"Headline Daily System | " +
            $"Date: {today} | " +
            $"Index: {headlineIndex} | " +
            $"Played Today: {alreadyPlayedToday}"
        );

        LoadHeadline(headlineIndex);

        if (alreadyPlayedToday)
        {
            RestoreCompletedState();
        }
    }

    // =========================================================
    // DATE
    // =========================================================

    private string GetTodayKey()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    private int GetDailyHeadlineIndex()
    {
        DateTime today = DateTime.Now.Date;

        // First headline starts on 12 August 2026.
        DateTime cycleStart =
            new DateTime(2026, 8, 12);

        int daysPassed =
            (today - cycleStart).Days;

        if (daysPassed < 0)
            daysPassed = 0;

        return daysPassed % gameData.headlines.Count;
    }

    // =========================================================
    // CHECK PLAYED
    // =========================================================

    private bool IsPlayedToday(string today)
    {
        string lastPlayedDate =
            PlayerPrefs.GetString(
                LastPlayedDateKey,
                ""
            );

        return lastPlayedDate == today;
    }

    // =========================================================
    // RESTORE COMPLETED STATE
    // =========================================================

    private void RestoreCompletedState()
    {
        Debug.Log(
            "Headline: Today's game is already completed."
        );

        RestoreSavedSelections();

        DisableGameplayButtons();

        UpdateStreakUI();

        // Open prediction screen by default
        // after restoring today's completed game.
        ShowPredictionScreen();
    }

    // =========================================================
    // RESTORE SAVED SELECTIONS
    // =========================================================

    private void RestoreSavedSelections()
    {
        selectedAsset =
            (HeadlineAsset)PlayerPrefs.GetInt(
                SavedAssetKey,
                0
            );

        selectedDirection =
            (Direction)PlayerPrefs.GetInt(
                SavedDirectionKey,
                0
            );

        selectedMagnitude =
            (Magnitude)PlayerPrefs.GetInt(
                SavedMagnitudeKey,
                0
            );

        assetSelected = true;
        directionSelected = true;
        magnitudeSelected = true;

        // Asset

        SetButtonSelected(
            goldButton,
            selectedAsset == HeadlineAsset.Gold
        );

        SetButtonSelected(
            bankIndexButton,
            selectedAsset == HeadlineAsset.BankIndex
        );

        SetButtonSelected(
            bitcoinButton,
            selectedAsset == HeadlineAsset.Bitcoin
        );

        SetButtonSelected(
            oilButton,
            selectedAsset == HeadlineAsset.Oil
        );

        SetButtonSelected(
            techETFButton,
            selectedAsset == HeadlineAsset.TechETF
        );

        SetButtonSelected(
            dollarButton,
            selectedAsset == HeadlineAsset.Dollar
        );

        // Direction

        SetButtonSelected(
            upButton,
            selectedDirection == Direction.Up
        );

        SetButtonSelected(
            flatButton,
            selectedDirection == Direction.Flat
        );

        SetButtonSelected(
            downButton,
            selectedDirection == Direction.Down
        );

        // Magnitude

        SetButtonSelected(
            under1Button,
            selectedMagnitude ==
            Magnitude.Under1Percent
        );

        SetButtonSelected(
            oneToThreeButton,
            selectedMagnitude ==
            Magnitude.OneToThreePercent
        );

        SetButtonSelected(
            over3Button,
            selectedMagnitude ==
            Magnitude.Over3Percent
        );

        if (selectedAssetText != null)
        {
            selectedAssetText.text =
                GetAssetDisplayName(selectedAsset);
        }
    }

    // =========================================================
    // DISABLE GAMEPLAY BUTTONS
    // =========================================================

    private void DisableGameplayButtons()
    {
        SetButtonsInteractable(
            false,

            goldButton,
            bankIndexButton,
            bitcoinButton,
            oilButton,
            techETFButton,
            dollarButton,

            upButton,
            flatButton,
            downButton,

            under1Button,
            oneToThreeButton,
            over3Button,

            lockCallButton
        );

        // Navigation buttons remain active.
    }

    private void SetButtonsInteractable(
        bool interactable,
        params Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }

    // =========================================================
    // ENABLE GAMEPLAY BUTTONS
    // =========================================================

    private void EnableGameplayButtons()
    {
        SetButtonsInteractable(
            true,

            goldButton,
            bankIndexButton,
            bitcoinButton,
            oilButton,
            techETFButton,
            dollarButton,

            upButton,
            flatButton,
            downButton,

            under1Button,
            oneToThreeButton,
            over3Button
        );

        CheckLockButton();
    }

    // =========================================================
    // SETUP GAME BUTTONS
    // =========================================================

    private void SetupButtons()
    {
        if (upButton != null)
        {
            upButton.onClick.AddListener(
                () => SelectDirection(Direction.Up)
            );
        }

        if (flatButton != null)
        {
            flatButton.onClick.AddListener(
                () => SelectDirection(Direction.Flat)
            );
        }

        if (downButton != null)
        {
            downButton.onClick.AddListener(
                () => SelectDirection(Direction.Down)
            );
        }

        if (under1Button != null)
        {
            under1Button.onClick.AddListener(
                () => SelectMagnitude(
                    Magnitude.Under1Percent
                )
            );
        }

        if (oneToThreeButton != null)
        {
            oneToThreeButton.onClick.AddListener(
                () => SelectMagnitude(
                    Magnitude.OneToThreePercent
                )
            );
        }

        if (over3Button != null)
        {
            over3Button.onClick.AddListener(
                () => SelectMagnitude(
                    Magnitude.Over3Percent
                )
            );
        }

        if (lockCallButton != null)
        {
            lockCallButton.onClick.AddListener(
                LockCall
            );
        }

        if (goldButton != null)
        {
            goldButton.onClick.AddListener(
                OnGoldClicked
            );
        }

        if (bankIndexButton != null)
        {
            bankIndexButton.onClick.AddListener(
                OnBankIndexClicked
            );
        }

        if (bitcoinButton != null)
        {
            bitcoinButton.onClick.AddListener(
                OnBitcoinClicked
            );
        }

        if (oilButton != null)
        {
            oilButton.onClick.AddListener(
                OnOilClicked
            );
        }

        if (techETFButton != null)
        {
            techETFButton.onClick.AddListener(
                OnTechETFClicked
            );
        }

        if (dollarButton != null)
        {
            dollarButton.onClick.AddListener(
                OnDollarClicked
            );
        }
    }

    // =========================================================
    // ASSET BUTTONS
    // =========================================================

    public void OnGoldClicked()
    {
        SelectAsset(HeadlineAsset.Gold);
    }

    public void OnBankIndexClicked()
    {
        SelectAsset(HeadlineAsset.BankIndex);
    }

    public void OnBitcoinClicked()
    {
        SelectAsset(HeadlineAsset.Bitcoin);
    }

    public void OnOilClicked()
    {
        SelectAsset(HeadlineAsset.Oil);
    }

    public void OnTechETFClicked()
    {
        SelectAsset(HeadlineAsset.TechETF);
    }

    public void OnDollarClicked()
    {
        SelectAsset(HeadlineAsset.Dollar);
    }

    // =========================================================
    // LOAD HEADLINE
    // =========================================================

    public void LoadHeadline(int index)
    {
        if (gameData == null)
        {
            Debug.LogError(
                "HeadlineController: Game Data is missing."
            );
            return;
        }

        if (gameData.headlines == null ||
            gameData.headlines.Count == 0)
        {
            Debug.LogError(
                "HeadlineController: No headlines found."
            );
            return;
        }

        headlineIndex =
            Mathf.Clamp(
                index,
                0,
                gameData.headlines.Count - 1
            );

        currentHeadline =
            gameData.headlines[headlineIndex];

        ResetSelections();

        UpdateHeadlineUI();
        UpdateDateUI();

        if (!alreadyPlayedToday)
            EnableGameplayButtons();

        ShowHeadlineScreen();
    }

    // =========================================================
    // DATE UI
    // =========================================================

    private void UpdateDateUI()
    {
        if (dateText == null)
            return;

        dateText.text =
            DateTime.Now.ToString("dd MMM");
    }

    // =========================================================
    // RESET
    // =========================================================

    private void ResetSelections()
    {
        assetSelected = false;
        directionSelected = false;
        magnitudeSelected = false;

        if (selectedAssetText != null)
            selectedAssetText.text = "SELECT ASSET";

        if (lockCallButton != null)
            lockCallButton.interactable = false;

        SetButtonSelected(upButton, false);
        SetButtonSelected(flatButton, false);
        SetButtonSelected(downButton, false);

        SetButtonSelected(
            under1Button,
            false
        );

        SetButtonSelected(
            oneToThreeButton,
            false
        );

        SetButtonSelected(
            over3Button,
            false
        );

        SetButtonSelected(
            goldButton,
            false
        );

        SetButtonSelected(
            bankIndexButton,
            false
        );

        SetButtonSelected(
            bitcoinButton,
            false
        );

        SetButtonSelected(
            oilButton,
            false
        );

        SetButtonSelected(
            techETFButton,
            false
        );

        SetButtonSelected(
            dollarButton,
            false
        );
    }

    // =========================================================
    // HEADLINE UI
    // =========================================================

    private void UpdateHeadlineUI()
    {
        if (headlineText != null)
            headlineText.text =
                currentHeadline.headline;

        if (descriptionText != null)
            descriptionText.text =
                currentHeadline.description;
    }

    // =========================================================
    // ASSET
    // =========================================================

    public void SelectAsset(HeadlineAsset asset)
    {
        if (alreadyPlayedToday)
            return;

        selectedAsset = asset;
        assetSelected = true;

        SetButtonSelected(
            goldButton,
            asset == HeadlineAsset.Gold
        );

        SetButtonSelected(
            bankIndexButton,
            asset == HeadlineAsset.BankIndex
        );

        SetButtonSelected(
            bitcoinButton,
            asset == HeadlineAsset.Bitcoin
        );

        SetButtonSelected(
            oilButton,
            asset == HeadlineAsset.Oil
        );

        SetButtonSelected(
            techETFButton,
            asset == HeadlineAsset.TechETF
        );

        SetButtonSelected(
            dollarButton,
            asset == HeadlineAsset.Dollar
        );

        if (selectedAssetText != null)
        {
            selectedAssetText.text =
                GetAssetDisplayName(asset);
        }

        // Selecting asset moves player to prediction.
        ShowPredictionScreen();

        CheckLockButton();
    }

    // =========================================================
    // DIRECTION
    // =========================================================

    public void SelectDirection(
        Direction direction)
    {
        if (alreadyPlayedToday)
            return;

        selectedDirection = direction;
        directionSelected = true;

        SetButtonSelected(
            upButton,
            direction == Direction.Up
        );

        SetButtonSelected(
            flatButton,
            direction == Direction.Flat
        );

        SetButtonSelected(
            downButton,
            direction == Direction.Down
        );

        CheckLockButton();
    }

    // =========================================================
    // MAGNITUDE
    // =========================================================

    public void SelectMagnitude(
        Magnitude magnitude)
    {
        if (alreadyPlayedToday)
            return;

        selectedMagnitude = magnitude;
        magnitudeSelected = true;

        SetButtonSelected(
            under1Button,
            magnitude ==
            Magnitude.Under1Percent
        );

        SetButtonSelected(
            oneToThreeButton,
            magnitude ==
            Magnitude.OneToThreePercent
        );

        SetButtonSelected(
            over3Button,
            magnitude ==
            Magnitude.Over3Percent
        );

        CheckLockButton();
    }

    // =========================================================
    // BUTTON VISUAL
    // =========================================================

    private void SetButtonSelected(
        Button button,
        bool selected)
    {
        if (button == null)
            return;

        Image background =
            button.GetComponent<Image>();

        if (background != null)
        {
            background.color =
                selected
                    ? selectedButtonColor
                    : normalButtonColor;
        }

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.color =
                selected
                    ? selectedTextColor
                    : normalTextColor;
        }
    }

    // =========================================================
    // LOCK
    // =========================================================

    private void CheckLockButton()
    {
        if (lockCallButton == null)
            return;

        lockCallButton.interactable =
            !alreadyPlayedToday &&
            assetSelected &&
            directionSelected &&
            magnitudeSelected;
    }

    public void LockCall()
    {
        if (alreadyPlayedToday)
            return;

        if (!assetSelected ||
            !directionSelected ||
            !magnitudeSelected)
            return;

        CalculateResult();
    }

    // =========================================================
    // RESULT
    // =========================================================

    private void CalculateResult()
    {
        if (currentHeadline == null)
            return;

        HeadlineAsset correctAsset =
            currentHeadline.correctAsset;

        float actualMovement =
            currentHeadline.actualMovement;

        Direction actualDirection =
            GetDirection(actualMovement);

        Magnitude actualMagnitude =
            GetMagnitude(actualMovement);

        bool assetCorrect =
            selectedAsset == correctAsset;

        bool directionCorrect =
            selectedDirection == actualDirection;

        bool magnitudeCorrect =
            selectedMagnitude == actualMagnitude;

        int score =
            CalculateScore(
                assetCorrect,
                directionCorrect,
                magnitudeCorrect
            );

        // IMPORTANT:
        // Result is shown immediately after Lock Call.
        // This is separate from Result navigation.
        ShowResultAfterLock(
            actualMovement,
            assetCorrect,
            directionCorrect,
            magnitudeCorrect,
            score
        );

        CompleteToday();
    }

    // =========================================================
    // COMPLETE TODAY
    // =========================================================

    private void CompleteToday()
    {
        string today = GetTodayKey();

        UpdateStreak(today);

        PlayerPrefs.SetString(
            LastPlayedDateKey,
            today
        );

        PlayerPrefs.SetInt(
            SavedAssetKey,
            (int)selectedAsset
        );

        PlayerPrefs.SetInt(
            SavedDirectionKey,
            (int)selectedDirection
        );

        PlayerPrefs.SetInt(
            SavedMagnitudeKey,
            (int)selectedMagnitude
        );

        PlayerPrefs.Save();

        alreadyPlayedToday = true;

        DisableGameplayButtons();

        UpdateStreakUI();

        Debug.Log(
            $"Headline completed | " +
            $"Date: {today} | " +
            $"Streak: {CurrentStreak}"
        );
    }

    // =========================================================
    // STREAK
    // =========================================================

    private void UpdateStreak(string today)
    {
        string lastPlayedDate =
            PlayerPrefs.GetString(
                LastPlayedDateKey,
                ""
            );

        int oldStreak =
            PlayerPrefs.GetInt(
                StreakKey,
                0
            );

        if (string.IsNullOrEmpty(lastPlayedDate))
        {
            CurrentStreak = 1;
        }
        else
        {
            DateTime lastDate;

            if (DateTime.TryParse(
                lastPlayedDate,
                out lastDate))
            {
                DateTime todayDate =
                    DateTime.Parse(today);

                int difference =
                    (
                        todayDate.Date -
                        lastDate.Date
                    ).Days;

                if (difference == 1)
                {
                    CurrentStreak =
                        oldStreak + 1;
                }
                else if (difference == 0)
                {
                    CurrentStreak =
                        oldStreak;
                }
                else
                {
                    CurrentStreak = 1;
                }
            }
            else
            {
                CurrentStreak = 1;
            }
        }

        PlayerPrefs.SetInt(
            StreakKey,
            CurrentStreak
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // STREAK UI
    // =========================================================

    private void UpdateStreakUI()
    {
        if (playedInRowText == null)
            return;

        CurrentStreak =
            PlayerPrefs.GetInt(
                StreakKey,
                0
            );

        // ONLY NUMBER
        playedInRowText.text =
            CurrentStreak.ToString();
    }

    // =========================================================
    // DIRECTION CALCULATION
    // =========================================================

    private Direction GetDirection(
        float movement)
    {
        if (movement > 0.05f)
            return Direction.Up;

        if (movement < -0.05f)
            return Direction.Down;

        return Direction.Flat;
    }

    // =========================================================
    // MAGNITUDE CALCULATION
    // =========================================================

    private Magnitude GetMagnitude(
        float movement)
    {
        float absoluteMovement =
            Mathf.Abs(movement);

        if (absoluteMovement < 1f)
            return Magnitude.Under1Percent;

        if (absoluteMovement <= 3f)
            return Magnitude.OneToThreePercent;

        return Magnitude.Over3Percent;
    }

    // =========================================================
    // SCORE
    // =========================================================

    private int CalculateScore(
        bool assetCorrect,
        bool directionCorrect,
        bool magnitudeCorrect)
    {
        if (!assetCorrect ||
            !directionCorrect)
            return 0;

        if (!magnitudeCorrect)
            return 100;

        switch (selectedMagnitude)
        {
            case Magnitude.Under1Percent:
                return 100;

            case Magnitude.OneToThreePercent:
                return 300;

            case Magnitude.Over3Percent:
                return 600;
        }

        return 0;
    }

    // =========================================================
    // SHOW RESULT AFTER LOCK
    // =========================================================

    private void ShowResultAfterLock(
        float actualMovement,
        bool assetCorrect,
        bool directionCorrect,
        bool magnitudeCorrect,
        int score)
    {
        // NO alreadyPlayedToday CHECK HERE.
        // This is called directly by Lock Call.

        ShowResultScreenDirect();

        if (actualText != null)
        {
            actualText.text =
                $"{actualMovement:+0.0;-0.0;0.0}%";
        }

        if (youSaidText != null)
        {
            youSaidText.text =
                GetMagnitudeDisplayName(
                    selectedMagnitude
                );
        }

        UpdateResultMessage(
            assetCorrect,
            directionCorrect,
            magnitudeCorrect
        );

        UpdateResultAvatar(
            assetCorrect,
            directionCorrect,
            magnitudeCorrect
        );

        UpdateCrowdUI();
    }

    // =========================================================
    // RESULT MESSAGE
    // =========================================================

    private void UpdateResultMessage(
        bool assetCorrect,
        bool directionCorrect,
        bool magnitudeCorrect)
    {
        if (resultMessageText == null)
            return;

        if (!assetCorrect)
        {
            resultMessageText.text =
                "Wrong asset. The move happened somewhere else.";
            return;
        }

        if (!directionCorrect)
        {
            resultMessageText.text =
                "Right asset, wrong direction.";
            return;
        }

        if (!magnitudeCorrect)
        {
            resultMessageText.text =
                "Right direction. Your magnitude was off. Partial score.";
            return;
        }

        resultMessageText.text =
            "Perfect call. You got the asset, direction and magnitude.";
    }

    // =========================================================
    // RESULT AVATAR
    // =========================================================

    private void UpdateResultAvatar(
        bool assetCorrect,
        bool directionCorrect,
        bool magnitudeCorrect)
    {
        if (resultAvatar == null)
            return;

        bool successfulCall =
            assetCorrect &&
            directionCorrect;

        if (successfulCall)
        {
            if (happyAvatar != null)
                resultAvatar.sprite =
                    happyAvatar;
        }
        else
        {
            if (sadAvatar != null)
                resultAvatar.sprite =
                    sadAvatar;
        }
    }

    // =========================================================
    // CROWD
    // =========================================================

    private void UpdateCrowdUI()
    {
        if (currentHeadline == null ||
            currentHeadline.crowdResults == null)
            return;

        if (currentHeadline.crowdResults.Count > 0)
        {
            SetCrowdRow(
                0,
                crowdLabel1,
                crowdPercentage1,
                crowdSlider1
            );
        }

        if (currentHeadline.crowdResults.Count > 1)
        {
            SetCrowdRow(
                1,
                crowdLabel2,
                crowdPercentage2,
                crowdSlider2
            );
        }

        if (currentHeadline.crowdResults.Count > 2)
        {
            SetCrowdRow(
                2,
                crowdLabel3,
                crowdPercentage3,
                crowdSlider3
            );
        }
    }

    private void SetCrowdRow(
        int index,
        TMP_Text labelText,
        TMP_Text percentageText,
        Slider slider)
    {
        CrowdResultData crowd =
            currentHeadline.crowdResults[index];

        if (labelText != null)
            labelText.text =
                crowd.label;

        if (percentageText != null)
            percentageText.text =
                $"{crowd.percentage:0}";

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value =
                crowd.percentage;
            slider.interactable = false;
        }
    }

    // =========================================================
    // SCREENS
    // =========================================================

    private void ShowHeadlineScreen()
    {
        if (headlineScreen != null)
            headlineScreen.SetActive(true);

        if (predictionScreen != null)
            predictionScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(false);
    }

    private void ShowPredictionScreen()
    {
        // Direct navigation is blocked before asset selection.
        // After completion it is allowed.
        if (!alreadyPlayedToday && !assetSelected)
            return;

        if (headlineScreen != null)
            headlineScreen.SetActive(false);

        if (predictionScreen != null)
            predictionScreen.SetActive(true);

        if (resultScreen != null)
            resultScreen.SetActive(false);
    }

    // Used ONLY by Result Nav
    private void ShowResultScreen()
    {
        if (!alreadyPlayedToday)
            return;

        if (headlineScreen != null)
            headlineScreen.SetActive(false);

        if (predictionScreen != null)
            predictionScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(true);
    }

    // Used ONLY after Lock Call
    private void ShowResultScreenDirect()
    {
        if (headlineScreen != null)
            headlineScreen.SetActive(false);

        if (predictionScreen != null)
            predictionScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(true);
    }

    // =========================================================
    // DISPLAY NAMES
    // =========================================================

    private string GetAssetDisplayName(
        HeadlineAsset asset)
    {
        switch (asset)
        {
            case HeadlineAsset.Gold:
                return "Gold";

            case HeadlineAsset.BankIndex:
                return "Bank Index";

            case HeadlineAsset.Bitcoin:
                return "Bitcoin";

            case HeadlineAsset.Oil:
                return "Oil";

            case HeadlineAsset.TechETF:
                return "Tech ETF";

            case HeadlineAsset.Dollar:
                return "Dollar";
        }

        return asset.ToString();
    }

    private string GetMagnitudeDisplayName(
        Magnitude magnitude)
    {
        switch (magnitude)
        {
            case Magnitude.Under1Percent:
                return "Under 1%";

            case Magnitude.OneToThreePercent:
                return "1–3%";

            case Magnitude.Over3Percent:
                return "Over 3%";
        }

        return "";
    }
    public void BackToMenu()
    {
        headlineScreen.SetActive(false); 
        predictionScreen.SetActive(false); 
        resultScreen.SetActive(false);

        if (uiManager != null)
        {
            uiManager.ToggleAllPanels(false);
            uiManager.ToggleMiniGamesPanel(true);
        }
    }
    // =========================================================
    // NEXT HEADLINE
    // =========================================================

    public void NextHeadline()
    {
        Debug.Log(
            "Daily system active. " +
            "Headline changes automatically with the date."
        );
    }
}