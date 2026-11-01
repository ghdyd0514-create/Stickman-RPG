using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WarriorDash : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;
    PlayerMove playerMove;
    PlayerSkillController skillController;

    [Header("대쉬어택 히트박스")]
    [SerializeField] Collider2D dashAttackHitBox;

    [Header("대쉬 이펙트 풀")]
    [SerializeField] DashEffectPool dashEffectPool;

    [Header("비주얼 자식의 트랜스폼")]
    [SerializeField] Transform visualTransform;

    [Header("대쉬 파워")]
    public int dashPower = 5;

    [Header("대쉬 쿨타임")]
    [SerializeField] float dashCoolTime = 3.0f;
    public bool canDash = true;

    [Header("대쉬 스킬 쿨다운 이미지와 연결")]
    [SerializeField] GameObject skillCooldownUI;

    [Header("몇 초 동안 대쉬할거냐")]
    [SerializeField] float dashTime = 1.5f;

    [Header("대쉬 중인가? 대쉬 중이면 대쉬 애니메이션 써야 함")]
    public bool onDash = false;

    [Header("증강들")]
    [SerializeField]
    bool isDashAttackAugment = false, isDashCoolTimeReset = false;

    [Header("효과음")]
    [SerializeField] AudioClip dashSFX;
    [SerializeField] float volume = 1;

    void Awake()
    {
        rigid = GetComponentInParent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMove = GetComponent<PlayerMove>();
        skillController = GetComponent<PlayerSkillController>();
        skillCooldownUI = FindAnyObjectByType<DashIconFindScript>(FindObjectsInactive.Include).gameObject;

        dashAttackHitBox.enabled = false;
    }

    //  대쉬하는 동안 피격을 받았는지 확인하는 코루틴
    //  피격을 받았으면 대쉬를 중간에 끊는다.
    IEnumerator Dash()
    {
        skillController.LockSkill();
        onDash = true;
        playerCombat.canAttack = false;

        //  만약 공격을 하는 도중 대쉬를 하면 공격 잔상이나, 히트박스가 유지될 수 있으니 전부 false시켜버리기
        Player.instance.playerCombat.attackEffect.SetActive(false);
        Player.instance.playerCombat.attackHitBox.gameObject.SetActive(false);

        //  대쉬 이펙트를 on 시킨다
        DashEffectFunction();
        Managers.instance.audioManager.PlaySFX(dashSFX, volume);

        //  대쉬 어택 증강이 있다면 대쉬 어택 히트박스를 킨다.
        if (isDashAttackAugment)
        {
            dashAttackHitBox.enabled = true;

            dashAttackHitBox.GetComponent<WarriorDashAttackHitbox>().StartDashAttack();
        }

        float timer = 0;
        while (timer <= dashTime)
        {
            Player.instance.playerCombat.attackEffect.SetActive(false);
            Player.instance.playerCombat.attackHitBox.gameObject.SetActive(false);

            if (!onDash)
            {
                break;
            }

            timer += Time.deltaTime;

            //  만약 대쉬중에 피격받으면 대쉬 끊기게
            if (Player.instance.playerHealth.isDamaged)
            {
                anim.ResetTrigger("Dash");
                break;
            }

            //  플레이어 대쉬.
            //  단 현재 바라보는 방향에 따라 대쉬 방향을 다르게 함.
            float dir = visualTransform.transform.localScale.x < 0 ? -1 : 1;
            rigid.linearVelocity = new Vector2(dashPower * dir, rigid.linearVelocity.y);

            yield return null;
        }

        //  대쉬가 끝나면 속도 및 다른 조건들을 초기화 시킨다.
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
        dashAttackHitBox.enabled = false;
        dashAttackHitBox.GetComponent<WarriorDashAttackHitbox>().EndDashAttack();
        playerCombat.canAttack = true;
        onDash = false;
        skillController.UnlockSkill();
    }

    //  대쉬 이펙트 값 설정 함수
    void DashEffectFunction()
    {
        GameObject dashEffect = dashEffectPool.GetEffect();

        //  왼쪽 보고 있으면
        if (visualTransform.transform.localScale.x < 0)
        {
            dashEffect.GetComponent<SpriteRenderer>().flipX = true;
            dashEffect.transform.position = transform.position + new Vector3(2.07f, 1.25f, 0);
        }
        else
        {
            dashEffect.GetComponent<SpriteRenderer>().flipX = false;
            dashEffect.transform.position = transform.position + new Vector3(-2.07f, 1.25f, 0);
        }

        dashEffect.SetActive(true);
    }

    //  대쉬 쿨타임 관련 코루틴.
    //  대쉬 중 피격을 받아 대쉬가 끊기더라도 대쉬 쿨타임은 쿨타임대로 흘러가야 하기에 별도로 관리한다.
    IEnumerator DashCoolTime()
    {
        canDash = false;

        //  스킬 쿨다운 UI한테 쿨타임 전달
        //  isDashCoolTimeReset변수가 참이면 0.5초를, 그게 아니라면 dashCoolTime을 전달한다.
        StartCoroutine(isDashCoolTimeReset ? skillCooldownUI.GetComponent<SkillCooldownUI>().SkillCooldown(0.5f)
         : skillCooldownUI.GetComponent<SkillCooldownUI>().SkillCooldown(dashCoolTime));

        //  대쉬ing 코루틴 작동
        StartCoroutine(Dash());

        //  시스템 상 쿨타임 코루틴 작동
        yield return new WaitForSeconds(isDashCoolTimeReset ? 0.5f : dashCoolTime);
        canDash = true;
    }

    public void OnDashSkill(InputAction.CallbackContext context)
    {
        if (!skillController.CanUseSkill())
            return;

        //  대쉬 가능 조건 1. 키를 눌러야 함 2. 대쉬가 사용가능한 상태여야 함 3. 플레이어가 사망한 상태가 아니여야 함.
        if (context.performed && canDash && !playerHealth.isDead)
        {
            //  현재 플레이어가 왼쪽을 바라본다면(즉, scale이 -1이라면) 왼쪽 대쉬
            if (transform.localScale.x <= -1)
            {
                rigid.linearVelocity = Vector2.zero;
                anim.SetTrigger("Dash");
                StartCoroutine(DashCoolTime());
            }
            //  오른쪽 바라본다면 오른쪽 대쉬 ㅇㅇ
            else if (transform.localScale.x >= 1)
            {
                rigid.linearVelocity = Vector2.zero;
                anim.SetTrigger("Dash");
                StartCoroutine(DashCoolTime());
            }
        }
    }




    //  ------------ 타 스크립트에서 사용하기 위함-----------------
    //  대쉬중인걸 끊고, 애니메이션도 끊어버림
    public void CancelDash()
    {
        onDash = false;
        playerCombat.canAttack = true;
        dashAttackHitBox.enabled = false;
        dashAttackHitBox.GetComponent<WarriorDashAttackHitbox>().EndDashAttack();
        anim.ResetTrigger("Dash");
    }

    public void OnDashAttack()
    {
        isDashAttackAugment = true;
    }

    public bool ReturnDashAttack()
    {
        return isDashAttackAugment;
    }

    public void SetDashCoolTime(float value)
    {
        dashCoolTime -= value;
    }

    //  대쉬 어택으로 적 죽이면 피 회복하게 하는 기능 true로 해주기
    public void SetHealAfterDashAttackKill()
    {
        dashAttackHitBox.GetComponent<WarriorDashAttackHitbox>().SetHealAfterDashAttackKill();
    }

    //  대쉬 어택 넉백 설정
    public void SetDashAttackKnockBack()
    {
        dashAttackHitBox.gameObject.GetComponent<WarriorDashAttackHitbox>().SetCanDashAttackKnockBack();
    }

    //  대쉬 쿨타임 0.5초 고정
    public void SetDashCoolTimeReset()
    {
        isDashCoolTimeReset = true;
    }
}
