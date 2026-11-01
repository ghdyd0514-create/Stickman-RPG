using UnityEngine;

//  기능 1. 플레이어가 현재 있는 방의 몬스터들 수와 포탈을 관리한다. 2. 각 방은 고유의 id를 가지고 있다.
public class Room : MonoBehaviour
{
    [SerializeField] EnemyHealth bossEnemyHealth;

    [Header("아이템들 과 몬스터들의 root")]
    [SerializeField] GameObject itemsRoot, EnemiesRoot;

    [Header("스타트 room인가")]
    public bool isStartRoom;

    [Header("해당 room의 카메라 컨파이너 콜라이더")]
    public Collider2D cameraConfinerArea;

    [Header("camera가 tracking 필요로 하는 room인가?")]
    public bool isCameraTracking = true;

    [Header("보스방인가?")]
    [SerializeField] bool isBossRoom = false;

    [Header("강아지 보스냐? 이녀석은 별도 브금 씀")]
    [SerializeField] bool isDogBoss = false;

    [Header("일반보스 브금 볼륨")]
    [SerializeField] float normalBossBGMVolume = 0.7f;

    //  대충 시스템이 어떻게 현재 room의 아이디를 인식하게 하느냐를 말해보자면
    //  1. 우선 플레이어가 상호작용 할 포탈이, 다음 room의 ID를 확인한다
    //  2. 해당 ID를 게임 매니저가 참조하게 한다
    //  3. 게임 매니저는 ID를 통해 플레이어가 어느 room에 있는지 알고 있으므로, 다른 room의 오브젝트들을 비활성화시킨다.

    void Awake()
    {
        ExitRoom(); //  맨 처음, 모든 Room들을 비활성화 시켜서 최적화 도모.
    }

    //  플레이어가 방에 입장하면, 현재 방의 몬스터와 아이템은 활성화 시키고, 나머지 방의 몬스터 아이템은 비활성화 시켜 최적화를 도모한다.
    public void EnterRoom()
    {
        if (isBossRoom)
        {
            if (isDogBoss)
                Managers.instance.audioManager.PlayDogBossBGM();
            else
                Managers.instance.audioManager.PlayBossBGM(normalBossBGMVolume);
        }
        else
        {
            Managers.instance.audioManager.PlayStageBGM();
        }

        itemsRoot.SetActive(true);
        EnemiesRoot.SetActive(true);
    }
    public void ExitRoom()
    {
        itemsRoot.SetActive(false);
        EnemiesRoot.SetActive(false);
    }
}