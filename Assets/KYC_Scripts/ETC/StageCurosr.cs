using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageCurosr : MonoBehaviour
{
    [Header("커서가 상/하로 움직이는 크기")]
    [SerializeField] float cursorMove = 1.0f;

    [Header("커서가 상/하로 움직이는 시간")]
    [SerializeField] float moveTime = 1.0f;

    [Header("커서 오프셋")]
    [SerializeField] Vector3 cursorOffset;

    [Header("전체 스테이지 배열")]
    [SerializeField] GameObject[] allStages;

    [Header("전체 스테이지들 중 현재 진입 가능한 스테이지 리스트")]
    [SerializeField] List<GameObject> stageList = new List<GameObject>();

    int cursorIndex = 0;
    bool stageEntering = false;     //  스테이지에 입장하면 페이드 인 되는데, 페이드 되는 찰나에 커서가 움직이면 뭔가 불편해서 그냥 추가함.
    Vector3 basePosition;

    void Awake()
    {
        for (int i = 0; i < allStages.Length; i++)
        {
            //  전체 스테이지 중, 현재 활성화가 되어진 스테이지만 리스트에 추가한다
            if (allStages[i].activeSelf)
            {
                stageList.Add(allStages[i]);
            }
        }

        stageEntering = false;
    }

    void Start()
    {
        basePosition = transform.localPosition;

        //  입장한 스테이지/마을에 대하여 인덱스를 기억하게 했다가, 다시 스테이지 선택 씬으로 돌아올 때 커서가 같은 인덱스에 위치하도록 한다.
        cursorIndex = Managers.instance.gameManager.currentStage;
        basePosition = stageList[cursorIndex].transform.position + cursorOffset;

        //  모든 스테이지 소개글 off
        foreach (GameObject list in stageList)
        {
            list.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
        }

        //  해당 스테이지 소개글 on
        stageList[cursorIndex].GetComponentInChildren<Canvas>(true).gameObject.SetActive(true);
    }

    void Update()
    {
        float y = Mathf.Abs(Mathf.Sin(Time.time * moveTime) * cursorMove);

        transform.localPosition = basePosition + Vector3.up * y;
    }

    //  커서가 현재 움직일 수 있는지 확인하는 코드.
    //  현재 입장가능한 스테이지들(마을 포함) 중 리스트의 0번째와 마지막 인덱스에 커서가 대응된다면, 왼쪽 / 오른쪽으로 못 움직이게 통제한다
    void CursorMoveCheck(Vector2 moveVec)
    {
        //  커서 인덱스가 0이고(즉, 0번째 리스트에 있고), 왼쪽으로 이동할려고 한다면
        if (cursorIndex <= 0 && moveVec.x < 0)
        {
            return;
        }

        //  커서 인덱스가 리스트이 마지막 인덱스와 같고(즉, 커서가 마지막 스테이지를 표시하고 있고), 오른쪽으로 이동하려고 한다면
        else if (cursorIndex >= stageList.Count - 1 && moveVec.x > 0)
        {
            return;
        }

        //  이 경우가 아니라면,
        CursorMove(moveVec);
    }

    void CursorMove(Vector2 moveVec)
    {
        if (moveVec.x < 0)
        {
            cursorIndex--;
            basePosition = stageList[cursorIndex].transform.position + cursorOffset;

            //  모든 스테이지 소개글 off
            foreach (GameObject list in stageList)
            {
                list.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
            }

            //  해당 스테이지 소개글 on
            stageList[cursorIndex].GetComponentInChildren<Canvas>(true).gameObject.SetActive(true);
        }
        else if (moveVec.x > 0)
        {
            cursorIndex++;
            basePosition = stageList[cursorIndex].transform.position + cursorOffset;

            //  모든 스테이지 소개글 off
            foreach (GameObject list in stageList)
            {
                list.GetComponentInChildren<Canvas>(true).gameObject.SetActive(false);
            }

            //  해당 스테이지 소개글 on
            stageList[cursorIndex].GetComponentInChildren<Canvas>(true).gameObject.SetActive(true);
        }
    }

    public void OnCursorMoveInput(InputAction.CallbackContext context)
    {
        if (context.performed && !stageEntering)
        {
            Vector2 moveVec = context.ReadValue<Vector2>();
            CursorMoveCheck(moveVec);
        }
    }

    public void OnStageEnterInput(InputAction.CallbackContext context)
    {
        //  게임 매니저한테 내가 선택한 스테이지 번호를 알려준다 
        // (나중에 스테이지에서 다시 선택 씬으로 돌아올 때, 커서가 입장한 스테이지를 가리키게 하기 위함.)
        if (context.performed)
        {
            stageEntering = true;
            Managers.instance.gameManager.currentStage = cursorIndex;

            switch (cursorIndex)
            {
                case 0:
                    StartCoroutine(Managers.instance.sceneMoveManager.EnterStage("Village"));
                    break;

                default:
                    StartCoroutine(Managers.instance.sceneMoveManager.EnterStage("Stage" + Managers.instance.gameManager.currentStage));
                    break;
            }
        }
    }
}