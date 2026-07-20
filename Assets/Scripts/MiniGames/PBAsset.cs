using TMPro;
using UnityEngine;

public class PBAsset : MonoBehaviour
{
    [SerializeField] PortfolioBuilderController pbController;

    public AssetType assetType;
    [SerializeField] float multiplicationFactor = 1.0f;

    [SerializeField] TextMeshProUGUI sharePriceTxt;
    [SerializeField] TextMeshProUGUI investedAmountTxt;
    [SerializeField] TextMeshProUGUI currentSharesCountTxt;

    // get till 2 decimal places 
    public float GetInvestedAmount() => float.Parse(investedAmountTxt.text.Replace("$", "").Replace(",", ""));

    public void OnValueChange(bool isAdded)
    {
        if (!isAdded && GetInvestedAmount() <= 0)
            return;

        float invested = float.Parse(investedAmountTxt.text.Replace("$", "").Replace(",", ""));
        float sharePrice = float.Parse(sharePriceTxt.text.Replace("$", "").Replace(",", ""));

        float amountChange = sharePrice * multiplicationFactor * (isAdded ? 1 : -1);

        // Prevent spending more money than available
        if (isAdded && pbController.GetTotalVirtualMoney() < amountChange)
            return;

        invested += amountChange;
        investedAmountTxt.text = $"${invested:N2}";

        // Update total virtual money
        PortfolioBuilderController.OnInvestedAmountChanged?.Invoke(assetType, GetInvestedAmount(), -amountChange);
        UpdateSharesCount(isAdded);
    }
    private void UpdateSharesCount(bool isAdded)
    {
        int sharesCount = int.Parse(currentSharesCountTxt.text);
        currentSharesCountTxt.text = Mathf.Max(0, sharesCount + (isAdded ? 1 : -1)).ToString();
    }
}
