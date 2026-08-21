using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "HeadlineGameData",
    menuName = "Headline/Game Data"
)]
public class HeadlineGameDataSO : ScriptableObject
{
    [Header("All Headlines")]
    public List<HeadlineDataSO> headlines = new List<HeadlineDataSO>();
}