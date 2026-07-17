using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MarketDetectorControllor : MonoBehaviour
{
    [SerializeField] private RectTransform _optionParent;
    public List<MarketPredictionSO> PredictionScenario = new List<MarketPredictionSO>();
    public List<Prediction> userPredictions = new List<Prediction>();
    [Header("Button")]
    [SerializeField] private Button _submit;
    [SerializeField] private Button _next;
    [SerializeField] private Button _retry;
    [SerializeField] GameObject _resultPanel;
    [SerializeField] TMP_Text _resultDes;
    //[SerializeField] GameObject _WrongPanel;

    [SerializeField] TMP_Text _questiontext;
    int currentScenario;


    void Start()
    {
        _submit.onClick.RemoveAllListeners();
        _submit.onClick.AddListener(OnSubmit);
        _next.onClick.RemoveAllListeners();
        _next.onClick.AddListener(OnNext);
        _retry.onClick.RemoveAllListeners();
        _retry.onClick.AddListener(OnRetry);
        currentScenario = 0;

        LoadScenario(currentScenario);
        
    }
    void ShowOption()
    {
        _optionParent.localScale = Vector3.zero;
        _optionParent
     .DOScale(Vector3.one, 0.5f)
    .SetEase(Ease.OutBack);
    }
    void LoadScenario(int currentPrediction)
    {
        ShowOption();
        if (currentPrediction >= PredictionScenario.Count)
        {
            Debug.Log("All Scenarios Completed");
            return;
        }

        MarketPredictionSO scenario = PredictionScenario[currentPrediction];

        _questiontext.text = scenario.Question;
        userPredictions.Clear();
    }
    public void OnStockUp()
    {
        SelectPrediction(Prediction.StockUp);
    }
               
    public void OnStockDown()
    {
        SelectPrediction(Prediction.StockDown);
    }
    public void OnCryptoUp()
    {
        SelectPrediction(Prediction.CryptoUp);
    }
    public void OnCryptpDown()
    {
        SelectPrediction(Prediction.CryptoDown);
    }
    public void OnGoldUp()
    {
        SelectPrediction(Prediction.GoldUp);
    }
    public void OnGoldDown()
    {
        SelectPrediction(Prediction.GoldDown);
    }
    public void OnOilUp()
    {
        SelectPrediction(Prediction.OilUp);
    }
    public void OnOilDown()
    {
        SelectPrediction(Prediction.OilDown);
    }
    void SelectPrediction(Prediction prediction)
    {
        userPredictions.Remove(prediction);
        userPredictions.Add(prediction);
    }

    public void OnSubmit()
    {
        MarketPredictionSO scenario = PredictionScenario[currentScenario];

        bool isCorrect = false;

        foreach (Prediction prediction in scenario.CorrectPredictions)
        {
            if (userPredictions.Contains(prediction))
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("Correct");
            ShowResult(true);
        }
        else
        {
            Debug.Log("Wrong");
            ShowResult(false);
        }
    }
   
    public void OnNext()
    {
        HideAll();
        currentScenario++;

        if (currentScenario < PredictionScenario.Count)
        {
            LoadScenario(currentScenario);
        }
        else
        {
            Debug.Log("Game Completed");
        }
    }
    
        void ShowResult(bool isCorrect)
        {
        _resultPanel.SetActive(isCorrect);
        RectTransform rt = _resultPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -600);
        rt.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);

        MarketPredictionSO scenario = PredictionScenario[currentScenario];

            if (isCorrect)
            {
            //titleText.text = "Correct!";
            _resultDes.text = scenario.CorrectDes;

                _next.gameObject.SetActive(true);
               _retry.gameObject.SetActive(false);
            }
            else
            {
            //titleText.text = "Wrong!";
            _resultDes.text = scenario.WrongDes;

            _next.gameObject.SetActive(false);
            _retry.gameObject.SetActive(true);
        }
        }
    
    void HideAll()
    {
        ShowResult(false);
    }
    public void OnRetry()
    {
        HideAll();
        LoadScenario(currentScenario);
    }


}
