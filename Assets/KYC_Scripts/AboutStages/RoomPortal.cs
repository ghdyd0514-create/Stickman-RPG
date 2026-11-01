using System;
using System.Collections;
using UnityEngine;

//  기능    1. 포탈을 이용하여 다음 방으로 넘어갈 수 있게 한다. 2. 다음 방의 id를 참조하여 게임 매니저한테 id를 보내준다.
public class RoomPortal : MonoBehaviour
{
    [Header("도착할 장소(포탈 위에 도착함)")]
    [SerializeField] RoomPortal nextRoomPortal;

    [Header("이 포탈은 처음 / 끝 부분 포탈이냐(맞다면 스테이지 선택 씬으로 돌아감)")]
    [SerializeField] bool isGoingToScenePortal;

    [Header("마을 전용 / 들어가는 문이냐, 나가는 문이냐")]
    [SerializeField] bool isEnterDoor = false, isCloseDoor = false;

    [Header("효과음")]
    [SerializeField] AudioClip enterDoorSFX, closeDoorSFX;
    [SerializeField] float volume;

    //  플레이어가 위 방향키 누르면 포탈 탐
    public IEnumerator GoNextRoom()
    {
        //  만약 씬으로 되돌아가는 포탈이라면 이 코드를 우선시 함.
        if (isGoingToScenePortal)
        {
            StartCoroutine(Managers.instance.sceneMoveManager.EnterStageSelectScene());
            yield break;
        }

        //  마을 전용 들어가거나, 나가는 문이면 효과음 주기 ㅎㅎ
        if (isEnterDoor)
        {
            if (enterDoorSFX == null) yield return null;
            Managers.instance.audioManager.PlaySFX(enterDoorSFX, volume);
        }
        else if (isCloseDoor)
        {
            if (closeDoorSFX == null) yield return null;
            Managers.instance.audioManager.PlaySFX(closeDoorSFX, volume);
        }

        //  마을에서는 그냥 페이드 인 아웃만
        if (Managers.instance.gameManager.whereIsThePlayer == WhereIsThePlayer.onVillage)
        {
            yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeIn()); //  페이드 인 해주고

            Managers.instance.cameraConfinerManager.SetConfiner(nextRoomPortal.GetComponentInParent<VillageRoom>().cameraConfinerArea);  //  다음 장소의 카메라 컨파이너 콜라이더를 가져온다

            Player.instance.transform.position = nextRoomPortal.transform.position;

            //  cinemachine의 이전 추적 상태 해제
            Managers.instance.cameraConfinerManager.SnapCamera();

            // Cinemachine이 새 위치를 계산할 최소 시간
            yield return null;
            yield return null;
            
            //  Cinemachine의 tracking이 필요한지 확인한다
            if (!nextRoomPortal.GetComponentInParent<VillageRoom>().isCameraTracking)
            {
                Managers.instance.cameraConfinerManager.StopCameraTracking();
            }
            else
            {
                Managers.instance.cameraConfinerManager.OnCameraTracking();
            }

            // Cinemachine이 플레이어를 follow 할 최소 시간
            yield return null;
            yield return null;

            yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeOut()); // 페이드 아웃 해주기
        }



        //  스테이지에서는 다음 방 몹과 오브젝트를 초기화한다.
        else if (Managers.instance.gameManager.whereIsThePlayer == WhereIsThePlayer.onStage)
        {
            //  다음 방으로 이동한다.
            yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeIn()); //  페이드 인 해주고

            //  있었던 방은 정리한다.
            Room currentRoom = gameObject.GetComponentInParent<Room>();
            currentRoom.ExitRoom();

            //  다음 방의 적과 오브젝트를 소환한다.
            Room nextRoom = nextRoomPortal.GetComponentInParent<Room>();
            nextRoom.EnterRoom();

            Managers.instance.cameraConfinerManager.SetConfiner(nextRoomPortal.GetComponentInParent<Room>().cameraConfinerArea);  //  다음 장소의 카메라 컨파이너 콜라이더를 가져온다

            Player.instance.transform.position = nextRoomPortal.transform.position;

            //  cinemachine의 이전 추적 상태 해제
            Managers.instance.cameraConfinerManager.SnapCamera();

            // Cinemachine이 새 위치를 계산할 최소 시간
            yield return null;
            yield return null;
            
            //  Cinemachine의 tracking이 필요한지 확인한다
            if (!nextRoomPortal.GetComponentInParent<Room>().isCameraTracking)
            {
                Managers.instance.cameraConfinerManager.StopCameraTracking();
            }
            else
            {
                Managers.instance.cameraConfinerManager.OnCameraTracking();
            }

            yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeOut()); // 페이드 아웃 해주기   
        }
    }
}