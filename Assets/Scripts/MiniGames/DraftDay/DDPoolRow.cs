using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Visual states for a row in the draft pool.</summary>
public enum DDPoolRowState { Available, Selected, Rostered, Locked }

/// <summary>One selectable asset row on the Draft Board.</summary>
public class DDPoolRow : MonoBehaviour
{
    private const string AvailableLabel = "Draft";
    private const string RosteredLabel = "Rostered";
    private const string LockedLabel = "Locked";

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image background;
    [SerializeField] private Button button;

    [Header("Colors")]
    [SerializeField] private Color availableColor = new(0.16f, 0.18f, 0.21f);
    [SerializeField] private Color selectedColor = new(0.24f, 0.75f, 0.51f);
    [SerializeField] private Color rosteredColor = new(0.12f, 0.13f, 0.15f);
    [SerializeField] private Color lockedColor = new(0.12f, 0.13f, 0.15f);
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color dimTextColor = new(1f, 1f, 1f, 0.35f);

    public DDAssetSO Asset { get; private set; }
    public DDPoolRowState State { get; private set; }

    /// <summary>Fills the row with an asset and routes clicks back to the controller.</summary>
    public void Bind(DDAssetSO asset, UnityAction<DDPoolRow> onClicked)
    {
        Asset = asset;
        nameText.text = asset.displayName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke(this));
        SetState(DDPoolRowState.Available);
    }

    /// <summary>Applies a visual state and the matching interactability.</summary>
    public void SetState(DDPoolRowState state)
    {
        State = state;

        switch (state)
        {
            case DDPoolRowState.Available:
                background.color = availableColor;
                statusText.text = AvailableLabel;
                statusText.color = activeTextColor;
                nameText.color = activeTextColor;
                button.interactable = true;
                break;

            case DDPoolRowState.Selected:
                background.color = selectedColor;
                statusText.text = AvailableLabel;
                statusText.color = activeTextColor;
                nameText.color = activeTextColor;
                button.interactable = true;
                break;

            case DDPoolRowState.Rostered:
                background.color = rosteredColor;
                statusText.text = RosteredLabel;
                statusText.color = dimTextColor;
                nameText.color = dimTextColor;
                button.interactable = false;
                break;

            case DDPoolRowState.Locked:
                background.color = lockedColor;
                statusText.text = LockedLabel;
                statusText.color = dimTextColor;
                nameText.color = dimTextColor;
                button.interactable = false;
                break;
        }
    }
}
