using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//  이 스크립트의 기능
//  각 npc들마다 고유의 텍스트들을 가지고 있는데, 내가 직접 대사를 작성해서 string 리스트에 넣는다.
//  그럼 텍스트UI는 내가 적은 string 리스트를 참조하여 글자를 반환한다.
//  npc마다 대사가 다르고 대사 횟수도 다르니 각 npc들은 이 스크립트를 상속받게 하여, 각 npc에 맞게 대사 횟수를 지정해준다.
public abstract class NPCText_Parent : MonoBehaviour
{
    TextMeshProUGUI textUI;
    [SerializeField] GameObject textCanvas;

    [Header("대사들. (내가 인스펙터에서 적기만 하면 됨)")]
    [SerializeField] List<string> dialogueTexts = new List<string>();
    //  dialogueText는 0번 인덱스가 기본 대사. 1번 인덱스가 상호작용 대사

    [Header("지속적으로 나오는 대사 쿨타임")]
    [SerializeField] float standardTextCooldown;

    [Header("지속적으로 나오는 텍스트가 한 번 나올 때 몇 초동안 보여질건지")]
    [SerializeField] float standardTextExposureTime = 2.5f;

    [Header("기본대사가 있능가")]
    [SerializeField] bool isStandardText = true;

    [Header("현재 상호작용 대사 중인가")]
    [SerializeField] protected bool isInteracting = false;

    [Header("효과음")]
    public AudioClip buySuccessSFX;
    public AudioClip buyRefuseSFX;
    public float volume = 1.0f;

    protected virtual void Awake()
    {
        textUI = GetComponentInChildren<TextMeshProUGUI>();
        textCanvas = GetComponentInChildren<Canvas>().gameObject;

        //  캔버스 비활성화 진행 ㄱㄱ
        textCanvas.gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        if (isStandardText)
            StartCoroutine(StandardText());
    }

    //  딱히 상호작용 하지 않더라도, 지속적으로 나오는 텍스트 (ex. 포션 팔아요)
    IEnumerator StandardText()
    {
        while (true)
        {
            if (isInteracting)
            {
                yield return null;
                continue;
            }

            standardTextCooldown = Random.Range(4f, 7f);
            yield return new WaitForSeconds(standardTextCooldown);

            // 기다리는 동안 상호작용이 시작되었으면 이번 기본대사는 취소
            if (isInteracting)
                continue;

            textCanvas.SetActive(true);
            textUI.text = dialogueTexts[0];

            yield return new WaitForSeconds(standardTextExposureTime);

            // 상호작용 대사가 출력 중이라면 기본대사가 UI를 꺼버리지 않게 함
            if (isInteracting)
                continue;

            textUI.text = "";
            textCanvas.SetActive(false);
        }
    }

    //  상속 받는 애들이 이거 씀.
    //  1. 인덱스 번호를 참조해서 텍스트 출력하는 방법
    protected void ShowTextByIndexNum(int indexNum)
    {
        textCanvas.SetActive(true);
        textUI.text = dialogueTexts[indexNum];
    }

    //  2. 텍스트 매개변수를 통해 텍스트 출력하는 방법
    protected void ShowTextByString(string text)
    {
        textCanvas.SetActive(true);
        textUI.text = text;
    }

    protected void HideText()
    {
        textUI.text = "";
        textCanvas.SetActive(false);
    }

    public virtual void ChooseChoice(InputAction.CallbackContext context)
    {

    }

    public virtual void DecideChoice(InputAction.CallbackContext context)
    {

    }

    //  --------------------------------------------------
    //  대화 종료
    //  --------------------------------------------------
    protected void EndInteraction()
    {
        HideText();

        //  제어 해제
        if (!Player.instance.playerMove.canMove && !Player.instance.playerMove.canJump && !Player.instance.playerCombat.canAttack)
        {
            Player.instance.playerMove.canMove = true;
            Player.instance.playerMove.canJump = true;
            Player.instance.playerCombat.canAttack = true;
        }

        if (!Player.instance.playerSkillController.CanUseSkill())
        {
            Player.instance.playerSkillController.UnlockSkill();
        } 

        isInteracting = false;

        Player.instance.GetComponentInChildren<PlayerInteraction>().ClearCurrentNPC(this);
    }

    //  --------------------------------------------------
    //  상호작용(공통)
    //  --------------------------------------------------
    public void Interact()
    {
        //  움직임과 공격 제어
        if (Player.instance.playerMove.canMove &&
        Player.instance.playerMove.canJump && Player.instance.playerCombat.canAttack)
        {
            Player.instance.playerMove.canMove = false;
            Player.instance.playerMove.canJump = false;
            Player.instance.playerCombat.canAttack = false;

            // 기존 관성 제거
            Player.instance
                .GetComponent<Rigidbody2D>()
                .linearVelocity = Vector2.zero;
        }

        //  스킬 못쓰게 제어
        if (Player.instance.playerSkillController.CanUseSkill())
        {
            Player.instance.playerSkillController.LockSkill();
        }

        isInteracting = true;
        OnInteract();
    }

    public abstract void OnInteract();
}