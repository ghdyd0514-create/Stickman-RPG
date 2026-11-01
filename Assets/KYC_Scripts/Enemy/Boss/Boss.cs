using System;
using UnityEngine;

//  기능 : 1. 보스가 있는 방에서는 포탈이 비활성화 된다
//  2. 보스를 죽이면 포탈이 다시 활성화 된다
//  3. 보스를 죽이면 이벤트를 발동시켜서 이벤트 변수를 구독중인 스크립트를 작동시키게 한다(ex. 보스용 투명 벽 삭제)
//  4. 보스가 생성되면 보스 전용 체력바UI가 화면 하단에 나타난다.
public class Boss : MonoBehaviour
{
    [Header("보스 체력바 캔버스")]
    [SerializeField] GameObject bossHpBar;

    public event Action bossDie;

    [SerializeField] RoomPortal[] portals = new RoomPortal[2];

    public void Die()
    {
        bossDie?.Invoke();

        portals[0].gameObject.SetActive(true);
        portals[1].gameObject.SetActive(true);

        bossHpBar.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        portals[0].gameObject.SetActive(false);
        portals[1].gameObject.SetActive(false);

        bossHpBar.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        Debug.Log("ㅅ라짐");
        bossHpBar.gameObject.SetActive(false);
    }
}