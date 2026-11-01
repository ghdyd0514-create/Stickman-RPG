using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [SerializeField] Boss boss;
    [SerializeField] Collider2D newCameraConfinerCollider;
    [SerializeField] BoxCollider2D bossMapWall; //  제한된 카메라 컨파이너 안에서 플레이어가 카메라 밖으로 못 빠져나가게끔, 투명벽 설치.    보스 잡으면 없어짐 ㅇㅇ
    [SerializeField] float cameraLens = 7.0f;

    bool playerHit = false;
    bool subscribed = false;

    void OnEnable()
    {
        if (boss != null && !subscribed)
        {
            boss.bossDie += BossDie;
            subscribed = true;
        }
    }

    void OnDisable()
    {
        if (boss != null && subscribed)
        {
            boss.bossDie -= BossDie;
            subscribed = false;
        }
    }

    void Update()
    {
        if (playerHit == false)
        {
            NewCameraConFiner();
        }
    }

    void NewCameraConFiner()
    {
        Collider2D hit = Physics2D.OverlapBox(newCameraConfinerCollider.bounds.center, newCameraConfinerCollider.bounds.size, 0, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            playerHit = true;
            Managers.instance.cameraConfinerManager.SetConfiner(newCameraConfinerCollider);
            StartCoroutine(Managers.instance.cameraConfinerManager.SetCameraLens(7f));
        }
    }

    void BossDie()
    {
        bossMapWall.gameObject.SetActive(false);
        ReturnCameraConfiner();
    }

    void ReturnCameraConfiner()
    {
        newCameraConfinerCollider.gameObject.SetActive(false);

        //  보스 처치 시 다시 원래 컨파이너로 되돌림
        Room roomCameraConfiner = GetComponentInParent<Room>();
        Managers.instance.cameraConfinerManager.SetConfiner(roomCameraConfiner.cameraConfinerArea);
        StartCoroutine(Managers.instance.cameraConfinerManager.SetCameraLens(6f));
    }
}