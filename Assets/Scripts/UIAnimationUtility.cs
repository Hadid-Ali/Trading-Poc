using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//TODO: Refactor this component
public class UIAnimationUtility : MonoBehaviour
{
    [SerializeField] private Image _imageComponent;
    [SerializeField] private float _timeDuration = 0.025f;
    [SerializeField] private Sprite[] _animationSequenceSprites;

    [Header("Loop Settings")]
    [SerializeField] private float _delayBetweenLoops = 2f;

    private Coroutine _animationCoroutine;
    private WaitForSecondsRealtime _frameDelay;

    private void Awake()
    {
        _frameDelay = new WaitForSecondsRealtime(_timeDuration);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        PlayAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void ResetState()
    {
        SetImage(_animationSequenceSprites[0]);
    }

    [ContextMenu("Play Animation")]
    public void PlayAnimation()
    {
        StopAnimation();
        _animationCoroutine = StartCoroutine(LoopAnimationSequence());
    }

    [ContextMenu("Stop Animation")]
    public void StopAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private IEnumerator LoopAnimationSequence()
    {
        while (true)
        {
            yield return PlayAnimationSequence();
            if(_delayBetweenLoops < 0)
                yield break;

            yield return new WaitForSeconds(_delayBetweenLoops);
        }
    }

    private IEnumerator PlayAnimationSequence()
    {
        for (int i = 0; i < _animationSequenceSprites.Length; i++)
        {
            SetImage(_animationSequenceSprites[i]);
            yield return _frameDelay;
        }
    }

    private void SetImage(Sprite sprite)
    {
        _imageComponent.sprite = sprite;
    }
}
