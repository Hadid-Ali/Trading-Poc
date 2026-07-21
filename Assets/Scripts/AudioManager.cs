using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource bgSource;

    [Header("Audio Assets")]
    [SerializeField] AudioClip btnClick;
    [SerializeField] AudioClip correct;
    [SerializeField] AudioClip wrong;  
    [SerializeField] AudioClip gameComplete;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(SoundType type)
    {
        if (type == SoundType.BtnClick && btnClick != null) sfxSource.PlayOneShot(btnClick); 
        if (type == SoundType.Correct) sfxSource.PlayOneShot(correct);   
        if (type == SoundType.Wrong) sfxSource.PlayOneShot(wrong);
        if (type == SoundType.GameComplete) sfxSource.PlayOneShot(gameComplete);


    }
}

[Serializable]
public enum SoundType
{
    None,
    Bg,
    BtnClick,
    Correct,
    Wrong,
    GameComplete
}
