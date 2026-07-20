using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] string virtualBalance = "";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetVirtualBalance() => virtualBalance;
    public void SetVirtualBalance(string balance)
    {
        virtualBalance = balance;
    }
}
