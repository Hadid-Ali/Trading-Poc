using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FomoFirewallController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject _introScreen;
    [SerializeField] private GameObject _questionScreen;
    [SerializeField] private GameObject _optionScreen;
    [SerializeField] private GameObject _resultPanel;

    [Header("Buttons")]
    [SerializeField] private Button _startLesson;
    [SerializeField] private Button _questionNext;
    [SerializeField] private Button _resultNext;

    [SerializeField] private Button buyNowBtn;
    [SerializeField] private Button waitBtn;
    [SerializeField] private Button plannedBtn;
    [SerializeField] private Button ignoreBtn;
    [SerializeField] private Button researchBtn;

    [Header("Question UI")]
    [SerializeField] private TMP_Text _question;
    [SerializeField] private TMP_Text _desc;
    [SerializeField] private TMP_Text _comment1txt;
    [SerializeField] private TMP_Text _comment2txt;
    [SerializeField] private TMP_Text _comment3txt;

    [Header("Result UI")]
    [SerializeField] private TMP_Text _resultTitle;
    [SerializeField] private TMP_Text _happenText;
    [SerializeField] private TMP_Text _lessonText;
    [SerializeField] private Image _feedbackGraphImage;

    [Header("Scenario Data")]
    public List<FomoFirewallSO> FOMOScenario = new();

    private int currentScenario = 0;

    private void Start()
    {
        _startLesson.onClick.RemoveAllListeners();
        _startLesson.onClick.AddListener(OnStartLesson);

        _questionNext.onClick.RemoveAllListeners();
        _questionNext.onClick.AddListener(OnQuestionNext);

        _resultNext.onClick.RemoveAllListeners();
        _resultNext.onClick.AddListener(OnNextScenario);

        buyNowBtn.onClick.AddListener(() => SelectOption(FomoOption.BuyImmediately));
        waitBtn.onClick.AddListener(() => SelectOption(FomoOption.WaitConfirmation));
        plannedBtn.onClick.AddListener(() => SelectOption(FomoOption.PlannedEntry));
        ignoreBtn.onClick.AddListener(() => SelectOption(FomoOption.IgnoreTrade));
        researchBtn.onClick.AddListener(() => SelectOption(FomoOption.ResearchReason));

        LoadScenario(currentScenario);

        HideAll();
        _introScreen.SetActive(true);
    }

    private void HideAll()
    {
        _introScreen.SetActive(false);
        _questionScreen.SetActive(false);
        _optionScreen.SetActive(false);
        _resultPanel.SetActive(false);
    }

    private void LoadScenario(int index)
    {
        if (index >= FOMOScenario.Count)
        {
            Debug.Log("Game Completed");
            return;
        }

        FomoFirewallSO scenario = FOMOScenario[index];

        _question.text = scenario.Question;
        _desc.text = scenario.Desc;
        _comment1txt.text = scenario.cmt1;
        _comment2txt.text = scenario.cmt2;
        _comment3txt.text = scenario.cmt3;
    }

    private void OnStartLesson()
    {
        HideAll();
        LoadScenario(currentScenario);
        _questionScreen.SetActive(true);
    }

    private void OnQuestionNext()
    {
        HideAll();
        _optionScreen.SetActive(true);
    }

    public void SelectOption(FomoOption selectedOption)
    {
        HideAll();
        _resultPanel.SetActive(true);

        FomoFirewallSO scenario = FOMOScenario[currentScenario];

        if (selectedOption == scenario.CorrectOption)
        {
            _resultTitle.text = "Great Decision!";
            _resultTitle.color = Color.green;
            _happenText.text = scenario.CorrectWhatHappenDec;
            _lessonText.text = scenario.CorrectLesson;
            _feedbackGraphImage.sprite = scenario.CorrectGraph;
        }
        else
        {
            _resultTitle.text = "Think Again!";
            _resultTitle.color = Color.red;
            _happenText.text = scenario.WrongWhatHappenDec;
            _lessonText.text = scenario.WrongLesson;
            _feedbackGraphImage.sprite = scenario.WrongGraph;
        }
    }

    private void OnNextScenario()
    {
        currentScenario++;

        if (currentScenario >= FOMOScenario.Count)
        {
            Debug.Log("Game Completed");
            // Show Complete Panel if you have one
            return;
        }

        LoadScenario(currentScenario);

        HideAll();
        _questionScreen.SetActive(true);
    }
}

public enum FomoOption
{
    BuyImmediately,
    WaitConfirmation,
    PlannedEntry,
    IgnoreTrade,
    ResearchReason
}