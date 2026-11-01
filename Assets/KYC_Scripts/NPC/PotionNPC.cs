using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PotionNPC : NPCText_Parent
{
    PlayerInput playerInput;

    [Header("포션 가격")]
    [SerializeField] int potionPrice = 500;

    [Header("포션 프리팹")]
    [SerializeField] GameObject potionPrefab;

    [Header("포션 던지는 힘")]
    [SerializeField] float potionThrowPower = 5.0f;

    [Header("현재 대사 인덱스 번호")]
    [SerializeField] int textIndexNum = 0;

    [Header("포션상인 대사")]
    [SerializeField] List<string> doYouBuyPotionText = new List<string>();
    [SerializeField] List<string> choiceText = new List<string>();
    [SerializeField] List<string> buySuccess = new List<string>();
    [SerializeField] List<string> refuse = new List<string>();
    [SerializeField] List<string> inventoryFull = new List<string>();
    [SerializeField] List<string> poorText = new List<string>();

    [Header("선택지 예/아니오 불값")]
    [SerializeField] bool choiceBool = true;

    enum DialogueState
    {
        Start,
        Choice,
        BuySuccess,
        Refuse,
        InventoryFull,
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

                ShowTextByString(doYouBuyPotionText[textIndexNum]);
                textIndexNum++;

                //  DoYouBuyPotionText의 대사가 출력되면 인덱스 번호를 초기화하고 선택 단계로 들어가게 함.
                if (textIndexNum >= doYouBuyPotionText.Count)
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
                    Player.instance.playerInventory.SetMoney(-potionPrice);

                    //  포션 주기
                    GameObject potion = Instantiate(potionPrefab, transform.position, Quaternion.identity);
                    Rigidbody2D potionRigid = potion.GetComponent<Rigidbody2D>();

                    //  포션을 플레이어 반대 방향으로 던짐
                    Vector2 throwPotionDir = (transform.position - Player.instance.transform.position).normalized;
                    throwPotionDir.y = 0;
                    if (Mathf.Abs(throwPotionDir.x) < 1)
                    {
                        if (throwPotionDir.x > 0) throwPotionDir.x = 1;
                        else throwPotionDir.x = -1;
                    }
                    // 위쪽 힘을 추가
                    potionRigid.AddForce(throwPotionDir * potionThrowPower + Vector2.up, ForceMode2D.Impulse);

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


            case DialogueState.InventoryFull:

                ShowTextByString(inventoryFull[textIndexNum]);
                textIndexNum++;

                if (textIndexNum >= inventoryFull.Count)
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

    public override void ChooseChoice(InputAction.CallbackContext context)
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
                    ShowTextByString("네     <b><size=120%>아니오</size></b>");
                }
            }

            //  왼쪽 방향키 누르면 '예' 선택지로 이동
            else
            {
                //  불값이 현재 false일 경우에만 예 쪽으로 이동
                if (!choiceBool)
                {
                    choiceBool = true;
                    ShowTextByString("<b><size=120%>네</size></b>     아니오");
                }
            }
        }
    }

    public override void DecideChoice(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isInteracting)
                return;

            if (choiceBool)
            {
                //  '예' 를 선택햇는데 인벤토리가 꽉 찼을 때
                if (Player.instance.playerInventory.potionAmount >= 5)
                {
                    Managers.instance.audioManager.PlaySFX(buyRefuseSFX, volume);
                    state = DialogueState.InventoryFull;
                    Interact();
                }

                //  '예' 를 선택했는데 돈이 모자를 때
                else if (Player.instance.playerInventory.potionAmount >= 0 && Player.instance.playerInventory.potionAmount < 10 &&
                            Player.instance.playerInventory.GetMoney() < potionPrice)
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