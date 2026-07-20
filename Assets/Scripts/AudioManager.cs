using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource bgSource;

    [Header("Audio Assets")]
    [SerializeField] AudioClip btnClick;


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


    }
}

[Serializable]
public enum SoundType
{
    None,
    Bg,
    BtnClick,
}
