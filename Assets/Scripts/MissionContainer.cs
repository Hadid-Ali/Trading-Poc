using System.Collections.Generic;
using UnityEngine;

public class MissionContainer : MonoBehaviour
{
    [SerializeField] private List<string> _missions = new List<string>();

    [SerializeField] private Transform _container;
    [SerializeField] private MissionWidget _widgetPrefab;

    private readonly List<MissionWidget> _spawnedWidgets = new List<MissionWidget>();

    void Start()
    {
        BuildMissions();
    }

    public void BuildMissions()
    {
        ClearMissions();

        foreach (var mission in _missions)
        {
            var widget = Instantiate(_widgetPrefab, _container);
            widget.SetMission(mission);
            _spawnedWidgets.Add(widget);
        }
    }

    public void SetMissions(List<string> missions)
    {
        _missions = new List<string>(missions);
        BuildMissions();
    }

    public void ClearMissions()
    {
        foreach (var widget in _spawnedWidgets)
        {
            if (widget != null)
            {
                Destroy(widget.gameObject);
            }
        }

        _spawnedWidgets.Clear();
    }
}
