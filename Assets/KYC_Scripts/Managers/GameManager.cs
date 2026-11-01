using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum WhereIsThePlayer
{
    onVillage,
    onStage,
    onNull
}

//  기능 : 각 스테이지에 있는 몬스터들의 수 확인 및 처치 기록. 현재 진입한 스테이지 기록. 플레이어 사망 시, 안내문구 작성
//  게임을 플레이하는 필수적인 비가시적 시스템 부분을 담당함.
public class GameManager : MonoBehaviour
{
    [Header("플레이어의 플레이어 인풋")]
    PlayerInput playerInput;

    [Header("죽음 텍스트")]
    PlayerDieText playerDieText;

    [Header("죽고 몇 초 뒤에 텍스트 뜰거냐")]
    [SerializeField] float appearTextAfterPlayerDie = 2.0f;

    [Header("현재 몇 스테이지냐")]
    public int currentStage;

    [Header("현 스테이지는 Room이 몇 개가 있냐")]
    public int currentRoomNum;

    [Header("현재 플레이어가 스테이지에 있는지 아니면 마을에 있는지 아니면 스테이지 선택 씬에 잇는지")]
    public WhereIsThePlayer whereIsThePlayer;

    [Header("타이틀에서 사용자가 헬프 버튼을 눌렀는가")]
    public bool isWatchingHelp = false;

    /*void Start()
    {
        //  각 방에 있는 모든 오브젝트와 몬스터들을 비활성화시킨다.
        for (int i = 0; i < allRooms.Length; i++)
        {
            allRooms[i].GetComponent<Room>().ExitRoom();
        }

        //  단 첫 번째 방만큼은 다시 몬스터들과 아이템들을 활성화시킨다.
        SetCurrentRoom(startingRoom);
    }*/

    //  씬이 변경될때마다, OnSceneLoaded함수를 실행하여 플레이어가 필요한 씬. 아닌 씬을 구별
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //  씬이 변경될때마다, 실행된다. ----------------------------------------------------------------
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //  씬에 대하여 bgm 변경
        if (scene.name == "Title")
        {
            Managers.instance.audioManager.PlayTitleBGM();
        }
        else if (scene.name == "Village")
        {
            Managers.instance.audioManager.PlayVillageBGM();
        }
        else if (scene.name == "StageSelectScene")
        {
            Managers.instance.audioManager.PlayStageBGM();
        }
        else if (scene.name.Contains("Stage"))
        {
            Managers.instance.audioManager.PlayStageBGM();
        }
        
        //---------------
        //  씬에 대한 기능 
        //  ------------
        if (scene.name == "Title")
            return;

        //  바뀐 씬의 첫 번째 room 객체를 가져온다. 단, 스테이지 선택씬은 제외.
        if (scene.name != "StageSelectScene")
        {
            GameObject firstRoom = FindAnyObjectByType<FirstRoom>().gameObject;

            //  바뀐 씬의 cinemachineConfiner2D 를 찾는다.
            Managers.instance.cameraConfinerManager.SetCinemachineReference();
            Managers.instance.cameraConfinerManager.SetConfiner(firstRoom.GetComponent<BoxCollider2D>());
        }

        if (Player.instance != null)
        {
            //  플레이어가 있는 씬으로 바뀌면 플레이어 인풋을 기본설정으로 초기화.
            playerInput = Player.instance.GetComponent<PlayerInput>();
            playerInput.SwitchCurrentActionMap("Player");

            playerDieText = FindAnyObjectByType<PlayerDieText>();
            playerDieText.gameObject.SetActive(false);
        }

        if (scene.name.Contains("Village")) { whereIsThePlayer = WhereIsThePlayer.onVillage; }
        else if (scene.name.Contains("Stage"))
        {
            whereIsThePlayer = WhereIsThePlayer.onStage;

            Room[] rooms = FindObjectsByType<Room>();

            foreach (Room room in rooms)
            {
                if (room.isStartRoom)
                {
                    room.EnterRoom();
                    break;
                }
            }
        }
        else if (scene.name == "StageSelectScene") { whereIsThePlayer = WhereIsThePlayer.onNull; }    //  스테이지 선택 씬에는 플레이어 객체가 없음
        else { whereIsThePlayer = WhereIsThePlayer.onNull; }  //  타이틀씬이거나 엔딩씬이거나~
    }

    //  플레이어 죽음 관련 --------------------------------------------------------------------------
    public IEnumerator PlayerDie()
    {
        playerInput.SwitchCurrentActionMap("PlayerDieActionMap");

        yield return new WaitForSeconds(appearTextAfterPlayerDie);

        playerDieText.gameObject.SetActive(true);
    }
}