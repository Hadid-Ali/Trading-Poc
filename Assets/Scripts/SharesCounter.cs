using TMPro;
using UnityEngine;

public class SharesCounter : MonoBehaviour
{
    [Header("Shares")]
    [SerializeField] private int sharesCount = 10;
    [SerializeField] private TextMeshProUGUI sharesCountText;

    [Header("Stock")]
    [SerializeField] private float stockPrice = 182.56f;
    [SerializeField] private TextMeshProUGUI estimatedCostText;

    private void OnEnable()
    {
        UpdateUI();
    }

    /// <summary>
    /// Increment or decrement the shares count.
    /// </summary>
    public void ChangeSharesCount(int amount)
    {
        sharesCount = Mathf.Max(0, sharesCount + amount);
        UpdateUI();
    }

    /// <summary>
    /// Sets the current stock price.
    /// </summary>
    public void SetStockPrice(float price)
    {
        stockPrice = Mathf.Max(0f, price);
        UpdateUI();
    }

    private void UpdateUI()
    {
        sharesCountText.text = sharesCount.ToString();

        float estimatedCost = sharesCount * stockPrice;
        estimatedCostText.text = $"${estimatedCost:N2}";
    }
}
