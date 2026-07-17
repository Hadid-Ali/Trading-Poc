using TMPro;
using UnityEngine;

public class PBAsset : MonoBehaviour
{
    public AssetType assetType;

    [SerializeField] TextMeshProUGUI sharePriceTxt;
    [SerializeField] TextMeshProUGUI investedAmountTxt;
    [SerializeField] TextMeshProUGUI currentSharesCountTxt;


    // get till 2 decimal places 
    public float GetInvestedAmount() => float.Parse(investedAmountTxt.text.Replace("$", "").Replace(",", ""));

    public void OnValueChange(bool isAdded)
    {
        if (GetInvestedAmount() == 0 && !isAdded) return;

        float invested = float.Parse(investedAmountTxt.text.Replace("$", "").Replace(",", ""));
        float sharePrice = float.Parse(sharePriceTxt.text.Replace("$", "").Replace(",", ""));

        invested += isAdded ? sharePrice : -sharePrice;

        investedAmountTxt.text = $"${invested:N2}";

        PortfolioBuilderController.OnInvestedAmountChanged?.Invoke(assetType, GetInvestedAmount());
    }
}
