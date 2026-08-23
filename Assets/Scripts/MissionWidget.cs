using TMPro;
using UnityEngine;

public class MissionWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _missionText;

    public void SetMission(string mission)
    {
        _missionText.text = mission;
    }

    public string GetMission()
    {
        return _missionText.text;
    }
}
