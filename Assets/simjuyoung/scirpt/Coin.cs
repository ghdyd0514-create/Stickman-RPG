using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public int coinValue = 1;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"코인 획득! +{coinValue}");

        CoinManager.Instance.AddCoin(coinValue);

        Destroy(gameObject);
    }
}