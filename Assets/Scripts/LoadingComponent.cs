using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingComponent : MonoBehaviour
{
    [SerializeField] private float _loadingSpeed;
    [SerializeField] private float _statusChangeTime;

    [SerializeField] private string _initialStatus;
    [SerializeField] private string _nextStatus;

    [SerializeField] private Image _loaderImage;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _loadingText;
    
    private float _currentLoadingValue;

    private void Start()
    {
        DOTween.To(() => _currentLoadingValue, x => _currentLoadingValue = x, 1,
            _loadingSpeed).SetEase(Ease.OutSine).OnComplete(LoadScene).OnUpdate(Action);

        Invoke(nameof(OnInitialLoadingComplete), _statusChangeTime);
        _statusText.text = _initialStatus;
    }

    private void Action()
    {
        _loadingText.text = $"{(int)(_currentLoadingValue * 100)} %";
        _loaderImage.fillAmount = _currentLoadingValue;
    }

    private void OnInitialLoadingComplete()
    {
        _statusText.text = _nextStatus;
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
