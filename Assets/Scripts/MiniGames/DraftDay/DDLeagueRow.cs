using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One standings row on the League Table.</summary>
public class DDLeagueRow : MonoBehaviour
{
    private const string PendingScoreLabel = "—";

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color playerColor = new(0.24f, 0.75f, 0.51f);
    [SerializeField] private Color opponentColor = new(0.16f, 0.18f, 0.21f);
    [SerializeField] private Color playerTextColor = new(0.05f, 0.09f, 0.07f);
    [SerializeField] private Color opponentTextColor = Color.white;

    private string entryName;

    public RectTransform Rect => (RectTransform)transform;

    /// <summary>Sets the entry name and whether this row represents the player.</summary>
    public void Bind(string displayName, bool isPlayer)
    {
        entryName = displayName;
        background.color = isPlayer ? playerColor : opponentColor;
        nameText.color = isPlayer ? playerTextColor : opponentTextColor;
        valueText.color = isPlayer ? playerTextColor : opponentTextColor;
        SetRank(0);
        SetPendingScore();
    }

    /// <summary>Prefixes the entry name with its current standing.</summary>
    public void SetRank(int rank)
    {
        nameText.text = rank > 0 ? $"{rank}. {entryName}" : entryName;
    }

    /// <summary>Shows a resolved score to two decimals.</summary>
    public void SetScore(float score)
    {
        valueText.text = score.ToString("0.00");
    }

    /// <summary>Shows a dash while the week has not produced a score yet.</summary>
    public void SetPendingScore()
    {
        valueText.text = PendingScoreLabel;
    }

    /// <summary>Slides the row to a standings slot.</summary>
    public void MoveToSlot(Vector2 anchoredPosition, float duration)
    {
        Rect.DOKill();
        Rect.DOAnchorPos(anchoredPosition, duration).SetEase(Ease.OutCubic);
    }
}
