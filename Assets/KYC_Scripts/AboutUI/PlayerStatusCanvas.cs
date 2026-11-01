using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//  스크립트 기능 : 1. 플레이어의 상태창을 킴. 2. 상태창 내에서 현 체력과 공격력 표시. 3. 현재 획득한 증강 간략하게 표시
public class PlayerStatusCanvas : MonoBehaviour
{
    [Header("플레이어 인풋")]
    [SerializeField] PlayerInput playerInput;

    [Header("플레이어 스크립트")]
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerCombat playerCombat;

    [Header("증강 매니저. 현재 가지고 있는 증강의 딕셔너리를 참조하기 위함")]
    [SerializeField] AugmentManager augmentManager;

    [Header("증강 아이콘 모음집을 담당하는 객체")]
    [SerializeField] GameObject iconContent;

    [Header("증강 아이콘이 들어갈 이미지 객체")]
    [SerializeField] GameObject imageObj;

    [Header("플레이어 공격력 텍스트와 체력 텍스트")]
    [SerializeField] TextMeshProUGUI HPText;
    [SerializeField] TextMeshProUGUI attackPowerText;

    [Header("증강 이름과 증강 내용")]
    [SerializeField] TextMeshProUGUI augmentTitleText;
    [SerializeField] TextMeshProUGUI augmentDescriptionText;

    [Header("체력바")]
    [SerializeField] GameObject hpBar;  //  체력바. 상태창 나올 때 체력바가 아주 거슬리거든요. 상태창 on일때는 체력바 없애기

    [Header("증강 아이콘 스크롤")]
    [SerializeField] RectTransform iconContentRect;

    [Header("현재 선택한 증강 번호")]
    [SerializeField] int currentIndex = 0;

    [Header("현재 화면 맨 왼쪽에 보이는 증강 번호")]
    [SerializeField] int firstVisibleIndex;

    [Header("슬롯에 보이는 아이콘의 개수")]
    [SerializeField] int visibleCount = 7;

    [Header("스크롤 이동 범위")]
    [SerializeField] float scrollMoveRange = 240;

    List<GameObject> acquiredAugments = new List<GameObject>(); //  얘는 증강 아이콘 리스트
    List<AugmentsData> acquiredAugmentsData = new List<AugmentsData>(); //  얘는 증강 설명 리스트

    public void OpenCanvas(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        this.playerHealth = playerHealth;
        this.playerCombat = playerCombat;

        //  이벤트를 끊고, 다시 구독해서 초기화 시켜준다.
        this.playerHealth.hpChanged -= RefreshStatusText;
        this.playerHealth.hpChanged += RefreshStatusText;
        this.playerCombat.attackPowerChanged -= RefreshStatusText;
        this.playerCombat.attackPowerChanged += RefreshStatusText;

        //  상태창 UI로 액션 맵 변경
        playerInput.SwitchCurrentActionMap("PlayerStatus");

        gameObject.SetActive(true);
        hpBar.gameObject.SetActive(false);
        SetText(playerHealth, playerCombat);
    }

    void OnEnable()
    {
        Debug.Log("현재 액션 맵 : " + playerInput.currentActionMap);

        //  리스트 초기화
        acquiredAugments.Clear();
        acquiredAugmentsData.Clear();
        currentIndex = 0;

        //  렉트 트랜스폼 초기화
        iconContentRect.anchoredPosition = new Vector2(0, 0);
        firstVisibleIndex = 0;

        //  혹시 모르니 enable할 때도 초기화
        for (int i = iconContent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(iconContent.transform.GetChild(i).gameObject);
        }

        BringAugmentsData();

        //  선택된 증강 초기화
        if (acquiredAugments.Count > 0)
        {
            acquiredAugments[currentIndex].GetComponent<AugmentIconSlotUI>().SetSelected(true);
        }
        if (acquiredAugmentsData.Count > 0)
        {
            SetAugmentText();
        }
    }

    void OnDisable()
    {
        playerHealth.hpChanged -= RefreshStatusText;
        playerCombat.attackPowerChanged -= RefreshStatusText;

        for (int i = iconContent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(iconContent.transform.GetChild(i).gameObject);
        }
    }

    //  현재 획득한 증강을 나타낸다.
    void BringAugmentsData()
    {
        foreach (int augmentID in Managers.instance.augmentManager.acquiredAugments)
        {
            AugmentsData augmentData =
                Managers.instance.augmentManager.allAugments.Find(x => x.ID == augmentID);

            if (augmentData == null)
                continue;

            GameObject imagePrefab =
                Instantiate(imageObj, iconContent.transform);

            imagePrefab
                .GetComponent<AugmentIconSlotUI>()
                .SetIconImage(augmentData.augmentIcon);

            // 이제 증강은 중첩되지 않으니까 x2, x3 표시 없음
            imagePrefab.GetComponentInChildren<TextMeshProUGUI>().text = null;

            acquiredAugments.Add(imagePrefab);
            acquiredAugmentsData.Add(augmentData);
        }
    }

    //  상태창의 텍스트를 설정한다.
    void SetText(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        //  추가체력이 존재할 경우
        if (playerHealth.plusMaxHP > 0) { HPText.text = "체력 : " + playerHealth.currentHP + " / " + playerHealth.maxHP + " (+" + playerHealth.plusMaxHP + ")"; }
        //  추가체력이 존재하지 않을 경우
        else { HPText.text = "체력 : " + playerHealth.currentHP + " / " + playerHealth.maxHP; }

        //  추가 공격력이 존재할 경우
        if (playerCombat.plusAttackPower > 0) { attackPowerText.text = "공격력 : " + playerCombat.attackPower + " (+" + playerCombat.plusAttackPower + ")"; }
        //  추가 공격력이 존재하지 않을 경우
        else { attackPowerText.text = "공격력 : " + playerCombat.attackPower; }
    }

    //  데미지 입거나 공격력이 상승하면 상태창을 키 상태로도 바로 확인 가능하도록 이벤트 설정
    void RefreshStatusText()
    {
        SetText(playerHealth, playerCombat);
    }

    //  아이콘이 슬롯을 넘으면 이동하게 한다.
    void MoveContentToSelectedIcon(bool isRightMove)
    {
        if (isRightMove)
        {
            firstVisibleIndex += 1;
            iconContentRect.anchoredPosition -= new Vector2(scrollMoveRange, 0);
        }
        else if (!isRightMove)
        {
            firstVisibleIndex -= 1;
            iconContentRect.anchoredPosition += new Vector2(scrollMoveRange, 0);
        }

    }

    //  현재 사용자가 선택한 증강의 정보를 보여준다
    void SetAugmentText()
    {
        augmentTitleText.text = "<" + acquiredAugmentsData[currentIndex].augmentName + ">";
        augmentDescriptionText.text = acquiredAugmentsData[currentIndex].augmentDescription;
    }

    //  증강 획득 목록을 좌우 방향키로 움직일 수 있도록
    public void OnMoveAugmentPage(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 moveVec = context.ReadValue<Vector2>();

            //  moveVec.x가 0보다 작다. 즉 왼쪽 방향키를 눌렀으면 왼쪽으로 스크롤 되게
            if (moveVec.x < 0)
            {
                if (currentIndex > 0)
                {
                    //  이전 증강 윤곽선 끄기
                    acquiredAugments[currentIndex].GetComponent<AugmentIconSlotUI>().SetSelected(false);
                    currentIndex--;

                    if (currentIndex < firstVisibleIndex)
                    {
                        MoveContentToSelectedIcon(false);
                    }

                    //  지금 선택된 윤곽선 키기
                    acquiredAugments[currentIndex].GetComponent<AugmentIconSlotUI>().SetSelected(true);
                    SetAugmentText();
                }

            }

            //  moveVec.x가 0보다 크다. 즉 오른쪽 방향키를 눌렀으면 오른쪽으로 스크롤 되게
            else if (moveVec.x > 0)
            {
                if (currentIndex < acquiredAugments.Count - 1)
                {
                    //  이전 증강 윤곽선 끄기
                    acquiredAugments[currentIndex].GetComponent<AugmentIconSlotUI>().SetSelected(false);
                    currentIndex++;

                    if (currentIndex > firstVisibleIndex + visibleCount - 1)
                    {
                        MoveContentToSelectedIcon(true);
                    }

                    //  지금 선택 된 윤곽선 키기
                    acquiredAugments[currentIndex].GetComponent<AugmentIconSlotUI>().SetSelected(true);

                    SetAugmentText();
                }
            }
        }
    }

    //  다시 tab키를 누르면 게임으로 돌아감
    public void OnTabKey(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //  Player로 액션 맵 변경
            playerInput.SwitchCurrentActionMap("Player");
            hpBar.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}


/*
        foreach (int augmentID in AugmentManager.instance.acquiredAugments)
        {
            AugmentsData augmentsData =
            GameObject imagePrefab = Instantiate(imageObj, iconContent.transform);

            //  게임 오브젝트의 이미지 컴포넌트에 접근해서 해당 이미지 컴포넌트에 생성한 iconSprite를 쳐 넣는다. 넣는다고 넣어.
            imagePrefab.GetComponent<AugmentIconSlotUI>().SetIconImage(augmentData[]);

            
            //  같은 증강을 2번 이상 획득할 경우. 증강 아이콘 프리팹을 참조해서.
            //  증강 아이콘 프리팹의 자식인 텍스에 접근해 획득 양을 보이게 한다.
            if (augmentData.Value >= 2)
            {
                imagePrefab.GetComponentInChildren<TextMeshProUGUI>().text = "x " + augmentData.Value;
            }
            else
            {
                imagePrefab.GetComponentInChildren<TextMeshProUGUI>().text = null;
            }

            //  정보가 담겨진 imagePrefab변수를 전역 리스트인 acquiredAugments에 넣어 정보를 넣는다.
            acquiredAugments.Add(imagePrefab);
            //  AugmentsData타입 그 자체를 리스트에 추가한다.
            acquiredAugmentsData.Add(augmentData.Key);
            */