using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingComponent : MonoBehaviour
{
    [SerializeField] UiManager uiManager;

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
            _loadingSpeed).SetEase(Ease.OutSine).OnComplete(OnLoadComplete).OnUpdate(Action);

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

    private void OnLoadComplete()
    {
        uiManager.ToggleLoadingPanel(false);
        uiManager.ToggleGetStartedPanel(true);
    }
}
