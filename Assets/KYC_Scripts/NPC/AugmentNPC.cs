using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

//  증강 상인 npc. 금액을 주고 증강을 살 수 있다. (랜덤한 증강 3장 중 하나 get.)
public class AugmentNPC : NPCText_Parent
{
    PlayerInput playerInput;

    [Header("증강 ui 캔버스")]
    [SerializeField] AugmentUICanvas augmentUICanvas;

    [Header("증강 가격")]
    [SerializeField] int augmentPrice = 3000;

    [Header("현재 대사 인덱스 번호")]
    [SerializeField] int textIndexNum = 0;

    [Header("증강상인 대사")]
    [SerializeField] List<string> doYouBuyAttackPowerText = new List<string>();
    [SerializeField] List<string> choiceText = new List<string>();
    [SerializeField] List<string> buySuccess = new List<string>();
    [SerializeField] List<string> refuse = new List<string>();
    [SerializeField] List<string> maxAugments = new List<string>();
    [SerializeField] List<string> poorText = new List<string>();

    [Header("선택지 예/아니오 불값")]
    [SerializeField] bool choiceBool = true;

    enum DialogueState
    {
        Start,
        Choice,
        BuySuccess,
        Refuse,
        MaxAugments,
        Poor,
        End
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        playerInput = Player.instance.GetComponent<PlayerInput>();
    }


    DialogueState state = DialogueState.Start;
    public override void OnInteract()
    {
        switch (state)
        {
            case DialogueState.Start:

                ShowTextByString(doYouBuyAttackPowerText[textIndexNum]);
                textIndexNum++;

                //  DoYouBuyPotionText의 대사가 출력되면 인덱스 번호를 초기화하고 선택 단계로 들어가게 함.
                if (textIndexNum >= doYouBuyAttackPowerText.Count)
                {
                    state = DialogueState.Choice;
                    textIndexNum = 0;
                }

                break;


            case DialogueState.Choice:

                //  선택이 가능하게끔 액션맵 변경
                playerInput.SwitchCurrentActionMap("DialogueActionMap");
                choiceBool = true;  //  선택지에서는 '예'를 기본으로 초기화시킨다.
                ShowTextByString(choiceText[textIndexNum]);

                break;


            case DialogueState.BuySuccess:

                ShowTextByString(buySuccess[textIndexNum]);
                textIndexNum++;

                if (textIndexNum >= buySuccess.Count)
                {
                    textIndexNum = 0;

                    //  비용 회수
                    Player.instance.playerInventory.SetMoney(-augmentPrice);

                    //  남은 증강 불러오기
                    augmentUICanvas.OpenCanvas(Player.instance);

                    state = DialogueState.End;
                }

                break;


            case DialogueState.Refuse:

                ShowTextByString(refuse[textIndexNum]);
                textIndexNum++;

                if (textIndexNum >= refuse.Count)
                {
                    textIndexNum = 0;

                    state = DialogueState.End;
                }

                break;


            case DialogueState.MaxAugments:

                ShowTextByString(maxAugments[textIndexNum]);
                textIndexNum++;

                if (textIndexNum >= maxAugments.Count)
                {
                    textIndexNum = 0;

                    state = DialogueState.End;
                }

                break;


            case DialogueState.Poor:

                ShowTextByString(poorText[textIndexNum]);
                textIndexNum++;

                if (textIndexNum >= poorText.Count)
                {
                    textIndexNum = 0;

                    state = DialogueState.End;
                }

                break;


            //  대화 종료
            case DialogueState.End:

                textIndexNum = 0;
                state = DialogueState.Start;

                EndInteraction();

                break;
        }
    }

    public override  void ChooseChoice(InputAction.CallbackContext context)
    {
        if (!isInteracting)
            return;

        if (context.performed)
        {

            Vector2 moveVec = context.ReadValue<Vector2>();

            //  오른쪽 방향키 누르면 '아니오' 선택지로 이동
            if (moveVec.x > 0)
            {
                //  불값이 현재 true일 경우에만 아니오 쪽으로 이동
                if (choiceBool)
                {
                    choiceBool = false;
                    ShowTextByString("산다     <b><size=120%>안산다</size></b>");
                }
            }

            //  왼쪽 방향키 누르면 '예' 선택지로 이동
            else
            {
                //  불값이 현재 false일 경우에만 예 쪽으로 이동
                if (!choiceBool)
                {
                    choiceBool = true;
                    ShowTextByString("<b><size=120%>산다</size></b>     안산다");
                }
            }
        }
    }

    public override void DecideChoice(InputAction.CallbackContext context)
    {
        if (!isInteracting)
            return;

        if (context.performed)
        {
            if (choiceBool)
            {
                //  '예' 를 선택햇는데 공격력이 꽉 찼을 때
                if (Player.instance.playerCombat.attackPower >= 99999)
                {
                    Managers.instance.audioManager.PlaySFX(buyRefuseSFX, volume);
                    state = DialogueState.MaxAugments;
                    Interact();
                }

                //  '예' 를 선택했는데 돈이 모자를 때
                else if (Player.instance.playerInventory.GetMoney() < augmentPrice)
                {
                    Managers.instance.audioManager.PlaySFX(buyRefuseSFX, volume);
                    state = DialogueState.Poor;
                    Interact();
                }

                //  '예'를 누르고 정상적인 경우
                else
                {
                    Managers.instance.audioManager.PlaySFX(buySuccessSFX, volume);
                    state = DialogueState.BuySuccess;
                    Interact();
                }
            }

            //  '아니오'를 눌렀을 때
            else
            {
                Managers.instance.audioManager.PlaySFX(buyRefuseSFX, volume);
                state = DialogueState.Refuse;
                Interact();
            }

            //  선택하면 다시 플레이어쪽으로 인풋액션 변경해서.... 대사를 계속 넘길 수 있게 해준다
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
}