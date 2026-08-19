using DG.Tweening;
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
    [SerializeField] private Color draftStatusColor = new(0.24f, 0.75f, 0.51f);
    [SerializeField] private Color selectedStatusColor = Color.white;
    [SerializeField] private Color rosteredStatusColor = new(0.55f, 0.56f, 0.60f);
    [SerializeField] private Color lockedStatusColor = new(0.55f, 0.56f, 0.60f);

    public DDAssetSO Asset { get; private set; }
    public DDPoolRowState State { get; private set; }

    private Vector3 defaultScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
    }

    /// <summary>Animates the row when the next draft round becomes active.</summary>
    public void PlayRefreshAnimation(float delay)
    {
        RectTransform rectTransform = (RectTransform)transform;

        rectTransform.DOKill();
        rectTransform.localScale = defaultScale * 0.96f;

        rectTransform
            .DOScale(defaultScale, 0.28f)
            .SetDelay(delay)
            .SetEase(Ease.OutBack);
    }

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
                statusText.color = draftStatusColor;
                nameText.color = activeTextColor;
                button.interactable = true;
                break;

            case DDPoolRowState.Selected:
                background.color = selectedColor;
                statusText.text = AvailableLabel;
                statusText.color = selectedStatusColor;
                nameText.color = activeTextColor;
                button.interactable = true;
                break;

            case DDPoolRowState.Rostered:
                background.color = rosteredColor;
                statusText.text = RosteredLabel;
                statusText.color = rosteredStatusColor;
                nameText.color = dimTextColor;
                button.interactable = false;
                break;

            case DDPoolRowState.Locked:
                background.color = lockedColor;
                statusText.text = LockedLabel;
                statusText.color = lockedStatusColor;
                nameText.color = dimTextColor;
                button.interactable = false;
                break;
        }
    }
}
