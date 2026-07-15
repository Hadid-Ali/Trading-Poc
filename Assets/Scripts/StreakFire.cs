using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StreakFire : MonoBehaviour
{
    [SerializeField] private Sprite fire1;
    [SerializeField] private Sprite fire2;

    [SerializeField] private Image fireImage;
    private bool toggle;

    private void OnEnable()
    {
        InvokeRepeating(nameof(ChangeFire), 0f, 0.2f);

        fireImage.transform
            .DOScale(1.08f, 0.25f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        CancelInvoke();
        fireImage.transform.DOKill();
    }

    private void ChangeFire()
    {
        toggle = !toggle;
        fireImage.sprite = toggle ? fire1 : fire2;
    }
}