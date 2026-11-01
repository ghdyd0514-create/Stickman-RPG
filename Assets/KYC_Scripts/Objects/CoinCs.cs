using UnityEngine;

public class CoinCs : MonoBehaviour
{
    int Money;

    public void SetMoney(int minMoney, int maxMoney)
    {
        int resultMoney = Random.Range(minMoney, maxMoney+1);
        Money = resultMoney;
    }

    public int GetMoney()
    {
        return Money;
    }
}