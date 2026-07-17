using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PredictionSO", menuName = "MarketDetectorData")]
public class MarketPredictionSO : ScriptableObject
{
    [TextArea(2,3)]
    public string Question;
    public List<Prediction> CorrectPredictions = new List<Prediction>();
    [TextArea(2, 3)]
    public string CorrectDes;
    [TextArea(2, 3)]
    public string WrongDes;
}

