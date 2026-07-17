using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PBEventsDataSO", menuName = "ScriptableObjects/PBEventsDataSO")]
public class PBEventsDataSO : ScriptableObject
{
    public List<EventData> eventsData;
}

[Serializable]
public class EventData
{
    [TextArea(1, 5)] public string statement;

    [Header("Correct Result Directions")]
    public AssetDirection[] directions = new AssetDirection[6];

    [Header("Result Descriptions")]
    [TextArea(1, 5)] public string correctAns;
    [TextArea(1, 5)] public string wrongAns;
    [TextArea(1, 5)] public string learningPoint;
}

[Serializable]
public class AssetDirection
{
    public AssetType assetType;
    public Direction direction;
}
