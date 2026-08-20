using System.Collections.Generic;
using UnityEngine;

public class BlindsideChart : MonoBehaviour
{
    [Header("Hidden Candle Objects")]
    [SerializeField] private List<GameObject> hiddenCandles = new List<GameObject>();

    public int HiddenCandleCount => hiddenCandles.Count;

    private void Awake()
    {
        // Hidden objects start TRUE
        foreach (GameObject candle in hiddenCandles)
        {
            if (candle != null)
                candle.SetActive(true);
        }
    }

    public void Reveal(int index)
    {
        if (index < 0 || index >= hiddenCandles.Count)
        {
            Debug.LogError(
                $"Invalid candle index: {index}. Count: {hiddenCandles.Count}"
            );
            return;
        }

        GameObject hiddenObject = hiddenCandles[index];

        if (hiddenObject == null)
        {
            Debug.LogError($"Hidden object at index {index} is NULL.");
            return;
        }

        // THIS is what you want
        hiddenObject.SetActive(false);

        Debug.Log(
            $"REVEALED -> {hiddenObject.name} | Index: {index}"
        );
    }
}