using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressionBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField, Range(0f, 1f)] private float fillAmount = 0.65f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private void OnEnable()
    {
        fillImage.DOKill();

        fillImage.fillAmount = 0f;

        fillImage.DOFillAmount(fillAmount, duration).SetEase(ease);
    }

    public void SetProgress(float value)
    {
        fillAmount = Mathf.Clamp01(value);
    }
}
