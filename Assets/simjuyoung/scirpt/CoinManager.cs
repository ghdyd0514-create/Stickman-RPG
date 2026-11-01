using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public TextMeshProUGUI coinText;

    private int coinCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        coinText.text = "coin : " + coinCount;
    }
}