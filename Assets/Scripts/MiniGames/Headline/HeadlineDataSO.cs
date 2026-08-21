using System.Collections.Generic;
using UnityEngine;

public enum HeadlineAsset
{
    Gold,
    BankIndex,
    Bitcoin,
    Oil,
    TechETF,
    Dollar
}

[CreateAssetMenu(
    fileName = "HeadlineData",
    menuName = "Headline/Headline Data"
)]
public class HeadlineDataSO : ScriptableObject
{
    [Header("Headline")]
    [TextArea(2, 5)]
    public string headline;

    [TextArea(2, 5)]
    public string description;

    [Header("Available Assets")]
    public List<HeadlineAsset> availableAssets = new List<HeadlineAsset>()
    {
        HeadlineAsset.Gold,
        HeadlineAsset.BankIndex,
        HeadlineAsset.Bitcoin,
        HeadlineAsset.Oil,
        HeadlineAsset.TechETF,
        HeadlineAsset.Dollar
    };

    [Header("Correct Result")]
    public HeadlineAsset correctAsset;

    [Tooltip("Example: +0.6 means +0.6%, -2.4 means -2.4%")]
    public float actualMovement;

    [Header("Crowd Result")]
    public List<CrowdResultData> crowdResults = new List<CrowdResultData>();
} 

[System.Serializable]
public class CrowdResultData
{
    public string label;

    [Range(0, 100)]
    public float percentage;
}