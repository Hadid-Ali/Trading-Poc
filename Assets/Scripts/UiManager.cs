using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject getStartedPanel;
    [SerializeField] GameObject homePanel;
    [SerializeField] GameObject portfolioPanel;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject learnPanel;
    [SerializeField] GameObject dailyChallengesPanel;
    [SerializeField] GameObject lessonDetailPanel;
    [SerializeField] GameObject miniGamesPanel;
    [SerializeField] GameObject pbGamePanel;
    [SerializeField] GameObject MarketDetectorGamePanel;

    [Header("Buttons")]
    [SerializeField] Button getStartedBtn;

    [Header("Ui Elements")]
    [SerializeField] TextMeshProUGUI virtualBalanceText;

    [Header("Debug")]
    [SerializeField] GameObject lastActivePanel; // the panel which just got disbaled.

    private void Start()
    {
        ToggleAllPanels(false);
        ToggleLoadingPanel(true);

        GameManager.Instance.SetVirtualBalance(virtualBalanceText.text);
    }

    public void ToggleLoadingPanel(bool v)
    {
        if (v != loadingPanel.activeInHierarchy) loadingPanel.SetActive(v);
    }

    public void ToggleGetStartedPanel(bool v)
    {
        if (v != getStartedPanel.activeInHierarchy) getStartedPanel.SetActive(v);
    }

    public void ToggleHomePanel(bool v)
    {
        if (v != homePanel.activeInHierarchy) homePanel.SetActive(v);
    }

    public void TogglePortfolioPanel(bool v)
    {
        if (v != portfolioPanel.activeInHierarchy) portfolioPanel.SetActive(v);
    }

    public void ToggleTradePanel(bool v)
    {
        if (v != tradePanel.activeInHierarchy) tradePanel.SetActive(v);
    }

    public void ToggleLearnPanel(bool v)
    {
        if (v != learnPanel.activeInHierarchy) learnPanel.SetActive(v);
    }

    public void ToggleDailyChallengePanel(bool v)
    {
        if (v != dailyChallengesPanel.activeInHierarchy) dailyChallengesPanel.SetActive(v);
    }

    public void ToggleLessonDetailPanel(bool v)
    {
        if (v != lessonDetailPanel.activeInHierarchy) lessonDetailPanel.SetActive(v);
    }

    public void ToggleMiniGamesPanel(bool v)
    {
        if (v != miniGamesPanel.activeInHierarchy) miniGamesPanel.SetActive(v);
    }

    public void TogglePbGamePanel(bool v)
    {
        if (v != pbGamePanel.activeInHierarchy) pbGamePanel.SetActive(v);
    }
    public void ToggleMarketDetectorGamePanel(bool v)
    {
        if (v != MarketDetectorGamePanel.activeInHierarchy) MarketDetectorGamePanel.SetActive(v);
    }

    public void ToggleAllPanels(bool v)
    {
        ToggleLoadingPanel(v);
        ToggleGetStartedPanel(v);
        ToggleHomePanel(v);
        TogglePortfolioPanel(v);
        ToggleTradePanel(v);
        ToggleLearnPanel(v);
        ToggleDailyChallengePanel(v);
        ToggleLessonDetailPanel(v);
        ToggleMiniGamesPanel(v);
        TogglePbGamePanel(v);
        ToggleMarketDetectorGamePanel(v);
    }

    public void HideAllPanels(GameObject refGo)
    {
        // Avoid redundant SetActive calls
        if (refGo == loadingPanel && loadingPanel.activeInHierarchy ||
            refGo == getStartedPanel && getStartedPanel.activeInHierarchy ||
            refGo == homePanel && homePanel.activeInHierarchy ||
            refGo == portfolioPanel && portfolioPanel.activeInHierarchy ||
            refGo == tradePanel && tradePanel.activeInHierarchy ||
            refGo == learnPanel && learnPanel.activeInHierarchy ||
            refGo == dailyChallengesPanel && dailyChallengesPanel.activeInHierarchy ||
            refGo == lessonDetailPanel && lessonDetailPanel.activeInHierarchy ||
            refGo == miniGamesPanel && miniGamesPanel.activeInHierarchy)
            return;

        ToggleLoadingPanel(false);
        ToggleGetStartedPanel(false);
        ToggleHomePanel(false);
        TogglePortfolioPanel(false);
        ToggleTradePanel(false);
        ToggleLearnPanel(false);
        ToggleDailyChallengePanel(false);
        ToggleLessonDetailPanel(false);
        ToggleMiniGamesPanel(false);
        TogglePbGamePanel(false);
        ToggleMarketDetectorGamePanel(false);
    }

    public void ToggleVirtualBalance(bool v)
    {
        // get current text and replace all characters with asterisks & vice verse
        if (v)
        {
            // except replacing first char of $ (i.e: $10,450.25)
            virtualBalanceText.text = GameManager.Instance.GetVirtualBalance();
        }
        else
            virtualBalanceText.text = virtualBalanceText.text[0] + new string('*', virtualBalanceText.text.Length - 1);
    }
}

/* Hierarchy order:

        Canvas
        │
        ├── Screen Layer
        │   ├── Home
        │   ├── Trade
        │   ├── Learn
        │   ├── Portfolio
        │   └── Daily Challenges
        │
        ├── Persistent UI
        │   ├── Header
        │   └── Bottom Navigation
        │
        ├── Fullscreen Layer
        │   ├── Get Started
        │   ├── Lesson Detail
        │   ├── Quiz
        │   └── Complete Lesson
        │
        └── Overlay Layer
            ├── Blur
            ├── Loading
            ├── Reward Popup
            ├── Dialog
            └── Toast, etc

 */
