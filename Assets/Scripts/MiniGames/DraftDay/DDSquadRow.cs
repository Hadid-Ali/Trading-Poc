using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>One weighted holding row on the Squad Sheet.</summary>
public class DDSquadRow : MonoBehaviour
{
    [SerializeField] private TMP_Text assetNameText;
    [SerializeField] private TMP_Text percentageText;
    [SerializeField] private Slider weightSlider;

    private int rosterIndex;

    /// <summary>Fills the row and routes slider drags back to the controller.</summary>
    public void Bind(int index, string assetName, int weight, UnityAction<int, int> onWeightChanged)
    {
        rosterIndex = index;
        assetNameText.text = assetName;

        weightSlider.wholeNumbers = true;
        weightSlider.minValue = 0f;
        weightSlider.maxValue = DraftDayScoring.TotalWeightPercent;
        weightSlider.onValueChanged.RemoveAllListeners();
        weightSlider.onValueChanged.AddListener(value => onWeightChanged?.Invoke(rosterIndex, Mathf.RoundToInt(value)));

        SetWeight(weight);
    }

    /// <summary>Updates the slider and label without firing the change callback.</summary>
    public void SetWeight(int weight)
    {
        weightSlider.SetValueWithoutNotify(weight);
        percentageText.text = $"{weight}%";
    }

    /// <summary>Enables or disables dragging, used to freeze weights after lock-in.</summary>
    public void SetInteractable(bool value)
    {
        weightSlider.interactable = value;
    }
}
