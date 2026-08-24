using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpreadController : MonoBehaviour
{
    // =========================================================
    // DATA
    // =========================================================

    [Header("DATA")]
    [SerializeField] private SpreadGameDataSO gameData;

    // =========================================================
    // UNLOCK
    // =========================================================

    [Header("UNLOCK")]
    [SerializeField] private SpreadUnlockManager unlockManager;

    // =========================================================
    // LEVEL
    // =========================================================

    [Header("LEVEL")]
    [SerializeField] private int currentLevel = 0;

    // =========================================================
    // SCREENS
    // =========================================================

    [Header("SCREENS")]
    [SerializeField] private GameObject gameplayScreen;
    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameObject ladderScreen;

    // =========================================================
    // NAVIGATION BUTTONS
    // =========================================================

    [Header("BOTTOM NAVIGATION")]
    [SerializeField] private Button gameplayNavButton;
    [SerializeField] private Button resultNavButton;
    [SerializeField] private Button ladderNavButton;

    // =========================================================
    // PLAYED LEVELS
    // =========================================================

    private List<bool> levelPlayed =
        new List<bool>();

    // =========================================================
    // COMPLETED LEVEL PROGRESS
    // =========================================================

    // Stores the next level that should be played.
    // Example:
    // 0 = Level 1
    // 1 = Level 2
    // 2 = Level 3
    private int completedLevels = 0;

    // =========================================================
    // GAMEPLAY UI
    // =========================================================

    [Header("GAMEPLAY UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text spreadText;

    // =========================================================
    // SELLERS
    // =========================================================

    [Header("SELLERS")]
    [SerializeField] private List<TMP_Text> sellerPriceTexts;
    [SerializeField] private List<TMP_Text> sellerQuantityTexts;

    // =========================================================
    // BUYERS
    // =========================================================

    [Header("BUYERS")]
    [SerializeField] private List<TMP_Text> buyerPriceTexts;
    [SerializeField] private List<TMP_Text> buyerQuantityTexts;

    // =========================================================
    // ORDER BUTTONS
    // =========================================================

    [Header("ORDER BUTTONS")]
    [SerializeField] private Button marketButton;
    [SerializeField] private Button limitButton;
    [SerializeField] private Button stopButton;

    // =========================================================
    // BUTTON COLORS
    // =========================================================

    [Header("BUTTON COLORS")]
    [SerializeField]
    private Color selectedButtonColor =
        new Color(0.20f, 0.75f, 0.85f);

    [SerializeField]
    private Color selectedTextColor =
        Color.black;

    [SerializeField]
    private Color normalButtonColor =
        Color.white;

    [SerializeField]
    private Color normalTextColor =
        Color.black;

    // =========================================================
    // RESULT UI
    // =========================================================

    [Header("RESULT UI")]
    [SerializeField] private TMP_Text resultLevelText;
    [SerializeField] private TMP_Text filledText;
    [SerializeField] private TMP_Text averagePriceText;
    [SerializeField] private TMP_Text benchmarkText;
    [SerializeField] private TMP_Text fedToNibbleText;
    [SerializeField] private TMP_Text fillQualityText;
    [SerializeField] private TMP_Text resultMessageText;

    // =========================================================
    // RESULT AVATAR
    // =========================================================

    [Header("RESULT AVATAR")]
    [SerializeField] private Image resultAvatar;
    [SerializeField] private Sprite happyAvatar;
    [SerializeField] private Sprite sadAvatar;

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    [Header("NEXT LEVEL")]
    [SerializeField] private Button nextLevelButton;

    // =========================================================
    // RUNTIME DATA
    // =========================================================

    private SpreadLevelData currentData;

    private List<SpreadOrderBookEntry> runtimeSellers;
    private List<SpreadOrderBookEntry> runtimeBuyers;

    private int filledQuantity;
    private float totalExecutionValue;

    private bool orderExecuted;

    private SpreadOrderType selectedOrderType;

    [SerializeField] private UiManager uiManager;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // ORDER BUTTONS

        if (marketButton != null)
        {
            marketButton.onClick.AddListener(
                () => SelectOrderType(
                    SpreadOrderType.Market
                )
            );
        }

        if (limitButton != null)
        {
            limitButton.onClick.AddListener(
                () => SelectOrderType(
                    SpreadOrderType.Limit
                )
            );
        }

        if (stopButton != null)
        {
            stopButton.onClick.AddListener(
                () => SelectOrderType(
                    SpreadOrderType.Stop
                )
            );
        }

        // NEXT BUTTON

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(
                NextLevel
            );
        }

        // BOTTOM NAVIGATION

        if (gameplayNavButton != null)
        {
            gameplayNavButton.onClick.AddListener(
                OpenGameplayFromNav
            );
        }

        if (resultNavButton != null)
        {
            resultNavButton.onClick.AddListener(
                OpenResultFromNav
            );
        }

        if (ladderNavButton != null)
        {
            ladderNavButton.onClick.AddListener(
                OpenLadderFromNav
            );
        }

        // Create played state for every level

        if (gameData != null &&
            gameData.levels != null)
        {
            levelPlayed.Clear();

            for (int i = 0;
                 i < gameData.levels.Count;
                 i++)
            {
                levelPlayed.Add(false);
            }
        }

        LoadLevel(currentLevel);
    }

    // =========================================================
    // LOAD LEVEL
    // =========================================================

    private void LoadLevel(int levelIndex)
    {
        if (gameData == null ||
            gameData.levels == null ||
            gameData.levels.Count == 0)
        {
            Debug.LogError(
                "Spread Game Data is missing."
            );

            return;
        }

        if (levelIndex < 0 ||
            levelIndex >= gameData.levels.Count)
        {
            return;
        }

        currentLevel = levelIndex;

        currentData =
            gameData.levels[levelIndex];

        filledQuantity = 0;
        totalExecutionValue = 0f;
        orderExecuted = false;

        runtimeSellers =
            CopyOrderBook(
                currentData.sellers
            );

        runtimeBuyers =
            CopyOrderBook(
                currentData.buyers
            );

        if (gameplayScreen != null)
            gameplayScreen.SetActive(true);

        if (resultScreen != null)
            resultScreen.SetActive(false);

        ResetButtonColors();

        UpdateOrderButtons();

        UpdateTargetUI();
        UpdateInstructionUI();
        UpdateOrderBookUI();
        UpdateSpreadUI();

        UpdateNavigationButtons();
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void OpenGameplayFromNav()
    {
        if (gameplayScreen != null)
            gameplayScreen.SetActive(true);

        if (resultScreen != null)
            resultScreen.SetActive(false);

        if (ladderScreen != null)
            ladderScreen.SetActive(false);

        UpdateOrderButtons();
    }

    // =========================================================
    // OPEN RESULT
    // =========================================================

    private void OpenResultFromNav()
    {
        if (!HasPlayedCurrentLevel())
        {
            Debug.Log(
                "Result is not available for this level yet."
            );

            return;
        }

        if (gameplayScreen != null)
            gameplayScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(true);

        if (ladderScreen != null)
            ladderScreen.SetActive(false);

        UpdateOrderButtons();
    }

    // =========================================================
    // OPEN LADDER
    // =========================================================

    private void OpenLadderFromNav()
    {
        if (gameplayScreen != null)
            gameplayScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(false);

        if (ladderScreen != null)
            ladderScreen.SetActive(true);
    }

    // =========================================================
    // CHECK PLAYED
    // =========================================================

    private bool HasPlayedCurrentLevel()
    {
        if (currentLevel < 0 ||
            currentLevel >= levelPlayed.Count)
        {
            return false;
        }

        return levelPlayed[currentLevel];
    }

    // =========================================================
    // NAV BUTTON STATE
    // =========================================================

    private void UpdateNavigationButtons()
    {
        if (resultNavButton != null)
        {
            resultNavButton.interactable =
                HasPlayedCurrentLevel();
        }
    }

    // =========================================================
    // TARGET UI
    // =========================================================

    private void UpdateTargetUI()
    {
        if (levelText != null)
        {
            levelText.text =
                $"LEVEL {currentData.levelNumber}";
        }

        if (targetText != null)
        {
            string side =
                currentData.target.side ==
                SpreadTradeSide.Buy
                    ? "Buy"
                    : "Sell";

            targetText.text =
                $"{side} " +
                $"{currentData.target.quantity} " +
                $"under {currentData.target.targetPrice:F2}";
        }
    }

    // =========================================================
    // INSTRUCTION
    // =========================================================

    private void UpdateInstructionUI()
    {
        if (instructionText == null)
            return;

        if (runtimeSellers.Count == 0 ||
            runtimeBuyers.Count == 0)
        {
            instructionText.text =
                "Choose how you want to execute the order.";

            return;
        }

        float bestSeller =
            runtimeSellers[0].price;

        float bestBuyer =
            runtimeBuyers[0].price;

        string side =
            currentData.target.side ==
            SpreadTradeSide.Buy
                ? "Buy"
                : "Sell";

        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            instructionText.text =
                $"{side} " +
                $"{currentData.target.quantity} units. " +
                $"Best seller: {bestSeller:F2}. " +
                $"Choose your order type.";
        }
        else
        {
            instructionText.text =
                $"{side} " +
                $"{currentData.target.quantity} units. " +
                $"Best buyer: {bestBuyer:F2}. " +
                $"Choose your order type.";
        }
    }

    // =========================================================
    // ORDER BOOK UI
    // =========================================================

    private void UpdateOrderBookUI()
    {
        for (int i = 0;
             i < sellerPriceTexts.Count;
             i++)
        {
            if (i < runtimeSellers.Count)
            {
                sellerPriceTexts[i].text =
                    runtimeSellers[i]
                        .price
                        .ToString("F2");

                sellerQuantityTexts[i].text =
                    runtimeSellers[i]
                        .quantity
                        .ToString();
            }
            else
            {
                sellerPriceTexts[i].text = "";
                sellerQuantityTexts[i].text = "";
            }
        }

        for (int i = 0;
             i < buyerPriceTexts.Count;
             i++)
        {
            if (i < runtimeBuyers.Count)
            {
                buyerPriceTexts[i].text =
                    runtimeBuyers[i]
                        .price
                        .ToString("F2");

                buyerQuantityTexts[i].text =
                    runtimeBuyers[i]
                        .quantity
                        .ToString();
            }
            else
            {
                buyerPriceTexts[i].text = "";
                buyerQuantityTexts[i].text = "";
            }
        }
    }

    // =========================================================
    // SPREAD
    // =========================================================

    private void UpdateSpreadUI()
    {
        if (spreadText == null)
            return;

        if (runtimeSellers.Count == 0 ||
            runtimeBuyers.Count == 0)
        {
            spreadText.text = "SPREAD --";
            return;
        }

        float bestSeller =
            runtimeSellers[0].price;

        float bestBuyer =
            runtimeBuyers[0].price;

        float spread =
            bestSeller - bestBuyer;

        spreadText.text =
            $"SPREAD {spread:F2}";
    }

    // =========================================================
    // SELECT ORDER TYPE
    // =========================================================

    private void SelectOrderType(
        SpreadOrderType orderType)
    {
        if (orderExecuted)
            return;

        selectedOrderType =
            orderType;

        SetSelectedOrderButton(
            orderType
        );

        switch (orderType)
        {
            case SpreadOrderType.Market:
                ExecuteMarketOrder();
                break;

            case SpreadOrderType.Limit:
                ExecuteLimitOrder();
                break;

            case SpreadOrderType.Stop:
                ExecuteStopOrder();
                break;
        }
    }

    // =========================================================
    // MARKET
    // =========================================================

    private void ExecuteMarketOrder()
    {
        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            ExecuteAgainstBook(runtimeSellers);
        }
        else
        {
            ExecuteAgainstBook(runtimeBuyers);
        }

        FinishOrder();
    }

    // =========================================================
    // LIMIT
    // =========================================================

    private void ExecuteLimitOrder()
    {
        float limitPrice =
            currentData.target.targetPrice;

        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            ExecuteLimitBuy(limitPrice);
        }
        else
        {
            ExecuteLimitSell(limitPrice);
        }

        FinishOrder();
    }

    private void ExecuteLimitBuy(float limitPrice)
    {
        int remaining =
            currentData.target.quantity;

        for (int i = 0;
             i < runtimeSellers.Count;
             i++)
        {
            if (remaining <= 0)
                break;

            SpreadOrderBookEntry level =
                runtimeSellers[i];

            if (level.price > limitPrice)
                break;

            int fill =
                Mathf.Min(
                    remaining,
                    level.quantity
                );

            AddFill(level.price, fill);

            level.quantity -= fill;
            remaining -= fill;
        }
    }

    private void ExecuteLimitSell(float limitPrice)
    {
        int remaining =
            currentData.target.quantity;

        for (int i = 0;
             i < runtimeBuyers.Count;
             i++)
        {
            if (remaining <= 0)
                break;

            SpreadOrderBookEntry level =
                runtimeBuyers[i];

            if (level.price < limitPrice)
                break;

            int fill =
                Mathf.Min(
                    remaining,
                    level.quantity
                );

            AddFill(level.price, fill);

            level.quantity -= fill;
            remaining -= fill;
        }
    }

    // =========================================================
    // STOP
    // =========================================================

    private void ExecuteStopOrder()
    {
        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            ExecuteAgainstBook(runtimeSellers);
        }
        else
        {
            ExecuteAgainstBook(runtimeBuyers);
        }

        FinishOrder();
    }

    // =========================================================
    // EXECUTE BOOK
    // =========================================================

    private void ExecuteAgainstBook(
        List<SpreadOrderBookEntry> book)
    {
        int remaining =
            currentData.target.quantity;

        for (int i = 0;
             i < book.Count;
             i++)
        {
            if (remaining <= 0)
                break;

            SpreadOrderBookEntry level =
                book[i];

            int fill =
                Mathf.Min(
                    remaining,
                    level.quantity
                );

            AddFill(level.price, fill);

            level.quantity -= fill;
            remaining -= fill;
        }
    }

    // =========================================================
    // ADD FILL
    // =========================================================

    private void AddFill(
        float price,
        int quantity)
    {
        if (quantity <= 0)
            return;

        filledQuantity += quantity;

        totalExecutionValue +=
            price * quantity;
    }

    // =========================================================
    // FINISH ORDER
    // =========================================================

    private void FinishOrder()
    {
        orderExecuted = true;

        if (currentLevel >= 0 &&
            currentLevel < levelPlayed.Count)
        {
            levelPlayed[currentLevel] = true;
        }

        ShowResult();

        if (gameplayScreen != null)
            gameplayScreen.SetActive(false);

        if (resultScreen != null)
            resultScreen.SetActive(true);

        UpdateOrderButtons();

        UpdateNavigationButtons();
    }

    // =========================================================
    // SHOW RESULT
    // =========================================================

    private void ShowResult()
    {
        if (resultLevelText != null)
        {
            resultLevelText.text =
                $"LEVEL {currentData.levelNumber}";
        }

        float averagePrice = 0f;

        if (filledQuantity > 0)
        {
            averagePrice =
                totalExecutionValue /
                filledQuantity;
        }

        float benchmark =
            currentData.benchmarkPrice;

        float executionCost =
            CalculateExecutionCost(
                averagePrice,
                benchmark
            );

        float fedToNibble =
            CalculateFedToNibble(
                averagePrice,
                benchmark
            );

        float fillPercentage = 0f;

        if (currentData.target.quantity > 0)
        {
            fillPercentage =
                ((float)filledQuantity /
                currentData.target.quantity)
                * 100f;
        }

        // =====================================================
        // CHECK CORRECT ANSWER
        // =====================================================

        bool correctChoice =
            selectedOrderType ==
            currentData.correctOrderType;

        // =====================================================
        // CORRECT LEVEL PROGRESS
        // =====================================================

        if (correctChoice)
        {
            // Move progress forward only
            if (currentLevel + 1 >
                completedLevels)
            {
                completedLevels =
                    currentLevel + 1;
            }

            // Update unlock UI
            if (unlockManager != null)
            {
                unlockManager.LevelCompletedCorrectly(
                    currentLevel
                );
            }
        }

        // =====================================================
        // GRADE
        // =====================================================

        string grade =
            CalculateGrade(
                fillPercentage,
                executionCost,
                correctChoice
            );

        if (filledText != null)
        {
            filledText.text =
                $"{filledQuantity}/" +
                $"{currentData.target.quantity}";
        }

        if (averagePriceText != null)
        {
            averagePriceText.text =
                filledQuantity > 0
                    ? averagePrice.ToString("F2")
                    : "--";
        }

        if (benchmarkText != null)
        {
            benchmarkText.text =
                benchmark.ToString("F2");
        }

        if (fedToNibbleText != null)
        {
            fedToNibbleText.text =
                fedToNibble.ToString("0");
        }

        if (fillQualityText != null)
        {
            fillQualityText.text =
                grade;
        }

        UpdateAvatar(grade);

        UpdateResultMessage(
            fillPercentage,
            executionCost
        );
    }

    // =========================================================
    // FED TO NIBBLE
    // =========================================================

    private float CalculateFedToNibble(
        float averagePrice,
        float benchmarkPrice)
    {
        if (filledQuantity <= 0)
            return 0f;

        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            return
                (benchmarkPrice -
                 averagePrice) *
                filledQuantity;
        }

        return
            (averagePrice -
             benchmarkPrice) *
            filledQuantity;
    }

    // =========================================================
    // EXECUTION COST
    // =========================================================

    private float CalculateExecutionCost(
        float averagePrice,
        float benchmarkPrice)
    {
        if (filledQuantity <= 0)
            return 0f;

        float difference;

        if (currentData.target.side ==
            SpreadTradeSide.Buy)
        {
            difference =
                averagePrice -
                benchmarkPrice;
        }
        else
        {
            difference =
                benchmarkPrice -
                averagePrice;
        }

        return Mathf.Max(
            0f,
            difference
        );
    }

    // =========================================================
    // GRADE
    // =========================================================

    private string CalculateGrade(
        float fillPercentage,
        float executionCost,
        bool correctChoice)
    {
        if (fillPercentage <= 0f)
            return "F";

        if (correctChoice &&
            fillPercentage >= 100f &&
            executionCost <=
            currentData.scoring.excellentThreshold)
        {
            return "A";
        }

        if (correctChoice &&
            fillPercentage >= 80f &&
            executionCost <=
            currentData.scoring.goodThreshold)
        {
            return "B";
        }

        if (fillPercentage >= 50f &&
            executionCost <=
            currentData.scoring.averageThreshold)
        {
            return "C";
        }

        if (fillPercentage >= 25f)
            return "D";

        return "F";
    }

    // =========================================================
    // RESULT MESSAGE
    // =========================================================

    private void UpdateResultMessage(
        float fillPercentage,
        float executionCost)
    {
        if (resultMessageText == null)
            return;

        switch (selectedOrderType)
        {
            case SpreadOrderType.Market:

                if (fillPercentage >= 100f &&
                    executionCost >
                    currentData.scoring.averageThreshold)
                {
                    resultMessageText.text =
                        "You used a market order and chased the price, increasing your execution cost.";
                }
                else if (fillPercentage >= 100f)
                {
                    resultMessageText.text =
                        "You used a market order and filled the order immediately.";
                }
                else
                {
                    resultMessageText.text =
                        "You used a market order, but available liquidity was not enough to fill the entire order.";
                }

                break;

            case SpreadOrderType.Limit:

                if (fillPercentage >= 100f)
                {
                    resultMessageText.text =
                        "Your limit order filled within the price you set.";
                }
                else if (fillPercentage > 0f)
                {
                    resultMessageText.text =
                        "Your limit order was too tight, so only part of the order was filled.";
                }
                else
                {
                    resultMessageText.text =
                        "Your limit price was too restrictive, so the order did not fill.";
                }

                break;

            case SpreadOrderType.Stop:

                if (fillPercentage >= 100f &&
                    executionCost >
                    currentData.scoring.averageThreshold)
                {
                    resultMessageText.text =
                        "Your stop order triggered as the market moved and resulted in a less favorable execution.";
                }
                else if (fillPercentage >= 100f)
                {
                    resultMessageText.text =
                        "Your stop order triggered and the order was executed.";
                }
                else if (fillPercentage > 0f)
                {
                    resultMessageText.text =
                        "Your stop order triggered, but there was not enough liquidity for a full fill.";
                }
                else
                {
                    resultMessageText.text =
                        "Your stop order triggered, but the available liquidity was not enough to execute the order.";
                }

                break;
        }
    }

    // =========================================================
    // AVATAR
    // =========================================================

    private void UpdateAvatar(string grade)
    {
        if (resultAvatar == null)
            return;

        if (grade == "A" ||
            grade == "B")
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
    // BUTTON COLORS
    // =========================================================

    private void SetSelectedOrderButton(
        SpreadOrderType selectedType)
    {
        SetButtonColor(
            marketButton,
            selectedType ==
            SpreadOrderType.Market
        );

        SetButtonColor(
            limitButton,
            selectedType ==
            SpreadOrderType.Limit
        );

        SetButtonColor(
            stopButton,
            selectedType ==
            SpreadOrderType.Stop
        );
    }

    private void SetButtonColor(
        Button button,
        bool selected)
    {
        if (button == null)
            return;

        Image buttonImage =
            button.GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.color =
                selected
                    ? selectedButtonColor
                    : normalButtonColor;
        }

        TMP_Text buttonText =
            button.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.color =
                selected
                    ? selectedTextColor
                    : normalTextColor;
        }
    }

    // =========================================================
    // RESET BUTTON COLORS
    // =========================================================

    private void ResetButtonColors()
    {
        SetButtonColor(
            marketButton,
            false
        );

        SetButtonColor(
            limitButton,
            false
        );

        SetButtonColor(
            stopButton,
            false
        );
    }

    // =========================================================
    // BUTTON STATE
    // =========================================================

    private void UpdateOrderButtons()
    {
        bool canPlay =
            !orderExecuted;

        if (marketButton != null)
        {
            marketButton.gameObject.SetActive(true);
            marketButton.interactable =
                canPlay;
        }

        if (limitButton != null)
        {
            limitButton.gameObject.SetActive(true);
            limitButton.interactable =
                canPlay;
        }

        if (stopButton != null)
        {
            stopButton.gameObject.SetActive(true);
            stopButton.interactable =
                canPlay;
        }
    }

    // =========================================================
    // COPY ORDER BOOK
    // =========================================================

    private List<SpreadOrderBookEntry>
        CopyOrderBook(
            List<SpreadOrderBookEntry> original)
    {
        List<SpreadOrderBookEntry> copy =
            new List<SpreadOrderBookEntry>();

        if (original == null)
            return copy;

        foreach (
            SpreadOrderBookEntry entry
            in original)
        {
            copy.Add(
                new SpreadOrderBookEntry
                {
                    price = entry.price,
                    quantity = entry.quantity
                }
            );
        }

        return copy;
    }

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    private void NextLevel()
    {
        if (gameData == null ||
            gameData.levels == null)
            return;

        if (currentLevel + 1 >=
            gameData.levels.Count)
        {
            Debug.Log(
                "All Spread levels completed!"
            );

            return;
        }

        currentLevel++;

        LoadLevel(currentLevel);
    }

    // =========================================================
    // BACK TO MENU
    // =========================================================

    public void BackToMenu()
    {
        if (gameData != null &&
            gameData.levels != null &&
            gameData.levels.Count > 0)
        {
            // All levels completed
            if (completedLevels >=
                gameData.levels.Count)
            {
                // Restart from Level 1
                completedLevels = 0;
                currentLevel = 0;
            }
            else
            {
                // Continue from next incomplete level
                currentLevel =
                    completedLevels;
            }
        }

        if (uiManager != null)
        {
            uiManager.ToggleAllPanels(false);
            uiManager.ToggleMiniGamesPanel(true);
        }
    }
}