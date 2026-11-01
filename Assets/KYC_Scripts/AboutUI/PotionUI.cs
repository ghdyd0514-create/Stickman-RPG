using TMPro;
using UnityEngine;

//  기능 : 
//  1. 플레이어 스크립트에서 플레이어가 가지고 있는 포션의 양을 참조한다
//  2. 참조한 양만큼 ui로 표시한다
//  3. 만약 포션의 양이 0개라면 화면에서 포션 아이콘은 사라지게 한다.
//  4. 다시 포션의 양이 1개 이상이 되면 화면에서 포션 아이콘이 보이게 한다.
public class PotionUI : MonoBehaviour
{
    [Header("potion icon image")]
    [SerializeField] GameObject potionImage;

    [Header("potion가지고 있는 수. 텍스트로 표현")]
    [SerializeField] TextMeshProUGUI potionCountText;

    [Header("플레이어 인벤토리")]
    [SerializeField] PlayerInventory playerInventory;

    void Awake()
    {
        playerInventory = FindAnyObjectByType<PlayerInventory>();
    }

    //  포션 이벤트 구독
    void OnEnable()
    {
        playerInventory.getPotionAmountEvent += SetPotionUI;
    }

    void OnDisable()
    {
        playerInventory.getPotionAmountEvent -= SetPotionUI;
    }

    void SetPotionUI(int potionAmount)
    {
        //  포션이 0보다 작다면 아예 ui를 꺼버릴거임
        if (potionAmount <= 0)
        {
            potionImage.SetActive(false);
        }
        else
        {
            potionImage.SetActive(true);
            potionCountText.text = "X" + potionAmount;
        }
    }
}