using UnityEngine;

//  이 스크립트의 기능 : 증간 선택 캔버스UI가 활성화 되면 게임 일시정지 시키고, 증강 3개를 추첨시키게 한다.
public class AugmentUICanvas : MonoBehaviour
{
    PlayerAugmentManager playerAugmentManager;

    [Header("증강 선택 매니저")]
    [SerializeField] AugmentSelectionManager augmentSelectionManager;

    [Header("카드 세 개")]
    [SerializeField] AugmentCardUI[] augmentsCard = new AugmentCardUI[3];

    public void OpenCanvas(Player player)
    {
        playerAugmentManager = player.GetComponent<PlayerAugmentManager>();

        gameObject.SetActive(true);
    }

    public void SelectedCard(AugmentsData augment)
    {
        //  플레이어는 해당 증강을 획득한다.
        playerAugmentManager.ApplyAugment(augment);

        //  시스템은 플레이어가 방금 획득한 증강의 획득 횟수를 +1 시킨다.
        //augmentManager.AcquireAugment(augment);
        Managers.instance.augmentManager.AcquireAugment(augment);

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        //  증강 획득하면 게임 일시정지.
        Time.timeScale = 0f;

        //  3개의 증강을 미리 추첨한다(기리한다.)
        augmentSelectionManager.GetRandomAugments();

        //  각 카드에 추첨받은 3개의 증강 데이터를 부여한다.
        for (int i = 0; i < augmentsCard.Length; i++)
        {
            augmentsCard[i].SetUp(i);
        }
    }

    void OnDisable()
    {
        //  캔버스가 비활성화 되면 다시 시간을 흐르게 한다.
        Time.timeScale = 1.0f;
    }
}