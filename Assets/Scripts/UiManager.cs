using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject loaingPanel;
    [SerializeField] GameObject getStartedPanel;
    [SerializeField] GameObject homePanel;
    [SerializeField] GameObject portfolioPanel;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject learnPanel;
    [SerializeField] GameObject dailyChallengesPanel;
    [SerializeField] GameObject lessonDetailPanel;

    [Header("Buttons")]
    [SerializeField] Button getStartedBtn;

    [Header("Ui Elements")]
    [SerializeField] TextMeshProUGUI virtualBalanceText;

    private void Start()
    {
        ToggleAllPanels(false);
        ToggleLoadingPanel(true);

        GameManager.Instance.SetVirtualBalance(virtualBalanceText.text);
    }

    public void ToggleLoadingPanel(bool v) => loaingPanel.SetActive(v);
    public void ToggleGetStartedPanel(bool v) => getStartedPanel.SetActive(v);
    public void ToggleHomePanel(bool v) => homePanel.SetActive(v);
    public void ToggleLessonDetailPanel(bool v) => lessonDetailPanel.SetActive(v);
    public void ToggleTradePanel(bool v) => tradePanel.SetActive(v);
    public void ToggleLearnPanel(bool v) => learnPanel.SetActive(v);
    public void TogglePortfolioPanel(bool v) => portfolioPanel.SetActive(v);
    public void ToggleDailyChallengePanel(bool v) => dailyChallengesPanel.SetActive(v);

    public void ToggleAllPanels(bool v)
    {
        ToggleLoadingPanel(v);
        ToggleGetStartedPanel(v);
        ToggleHomePanel(v);
        ToggleLessonDetailPanel(v);
        ToggleTradePanel(v);
        ToggleLearnPanel(v);
        TogglePortfolioPanel(v);
        ToggleDailyChallengePanel(v);
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