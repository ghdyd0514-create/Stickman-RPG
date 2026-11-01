using TMPro;
using UnityEngine;

public class HavingMoneyUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentMoneyText;
    [SerializeField] PlayerInventory playerInventory;

    void Awake()
    {
        playerInventory = FindAnyObjectByType<PlayerInventory>();
    }

    void Start()
    {
        playerInventory.getCashChangeEvent += UpdateMoneyText;
        currentMoneyText.text = "Money : " + playerInventory.GetMoney() + "G";
    }

    void OnDisable()
    { 
        playerInventory.getCashChangeEvent -= UpdateMoneyText;
    }

    void UpdateMoneyText()
    {
        currentMoneyText.text = "Money : " + playerInventory.GetMoney() + "G";
    }
}