using System;
using TMPro;
using UnityEngine;

public class DailyRewardTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerTxt;

    [Tooltip("07:45:12"), SerializeField] private float startTime = 7 * 3600 + 45 * 60 + 12;

    private float remainingTime;
    private float disabledAt;

    private void Awake()
    {
        remainingTime = startTime;
    }

    private void OnEnable()
    {
        // If we were previously disabled, subtract the elapsed real time.
        if (disabledAt > 0f)
        {
            remainingTime -= Time.realtimeSinceStartup - disabledAt;
            disabledAt = 0f;
        }

        UpdateTimerText();
    }

    private void OnDisable()
    {
        disabledAt = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (remainingTime <= 0f)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime < 0f)
            remainingTime = 0f;

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        TimeSpan t = TimeSpan.FromSeconds(remainingTime);
        timerTxt.text = $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }
}
