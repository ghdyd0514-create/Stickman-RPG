using System;
using System.Collections;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WarriorParry : MonoBehaviour
{
    Animator anim;
    CapsuleCollider2D playerCollider;
    PlayerHealth playerHealth;
    PlayerCombat playerCombat;
    PlayerSkillController skillController;

    [Header("비주얼 자식의 트랜스폼")]
    [SerializeField] Transform visualTransform;

    [Header("패리 가능 여부")]
    public bool canParry = true;

    [Header("패리 쿨타임")]
    [SerializeField] float parryCoolTime = 3.0f;

    [Header("패리 스킬 쿨다운 이미지와 연결")]
    [SerializeField] GameObject skillCooldownUI;

    [Header("현재 패리중인가?")]
    public bool onParring;

    [Header("몇초동안 패리할거냐")]
    [SerializeField] float parringTime = 0.5f;

    [Header("공격 이펙트")]
    [SerializeField] GameObject attackEffect;

    [Header("공격 히트박스 콜라이더")]
    [SerializeField] PolygonCollider2D hitBoxCollider;

    [Header("패리 성공 효과음")]
    [SerializeField] AudioClip parrySuccessSFX;
    [SerializeField] float volume;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();
        skillController = GetComponent<PlayerSkillController>();
        skillCooldownUI = FindAnyObjectByType<ParryIconFindScript>(FindObjectsInactive.Include).gameObject;
    }

    //  패리 쿨타임 코루틴.
    IEnumerator ParryCoolTime(float parryCoolTime)
    {
        canParry = false;

        //  패리 스킬 쿨다운 작동
        StartCoroutine(skillCooldownUI.GetComponent<SkillCooldownUI>().SkillCooldown(parryCoolTime));

        yield return new WaitForSeconds(parryCoolTime);
        canParry = true;
    }

    //  패리는 공격보다 우선순위가 높지만 대쉬보다는 낮다.
    IEnumerator StartParry()
    {
        skillController.LockSkill();

        //  패리 쿨타임을 돌려준다.
        StartCoroutine(ParryCoolTime(parryCoolTime));
        onParring = true;

        anim.SetTrigger("Parry");

        //  만약 공격을 하여 공격 이펙트와, 히트박스 콜라이더가 on 된 상태에서
        //  패리를 하여 공격을 끊는다면, 이 두 가지를 없애줘야함.
        hitBoxCollider.gameObject.SetActive(false);
        attackEffect.SetActive(false);

        float timer = 0;
        while (timer <= parringTime)
        {
            if (!onParring) yield break;

            timer += Time.deltaTime;

            //  만약 패리중에 피격받으면 패리 끊기게
            if (Player.instance.playerHealth.isDamaged)
            {
                anim.ResetTrigger("Parry");
                break;
            }

            yield return null;
        }

        onParring = false;
        skillController.UnlockSkill();
    }

    public bool Parry(Collider2D enemyAttackCollider)
    {
        //  패리를 오른쪽으로 했는지 왼쪽으로 했는지를 우선 구별. 오른쪽이면 1을, 왼쪽이면 -1을
        float parryDir = visualTransform.transform.localScale.x > 0 ? 1 : -1;

        switch (parryDir)
        {
            case 1:
                //  만약 오른쪽으로 패리 했는데, 적 공격 콜라이더가 플레이어 콜라이더보다 더 오른쪽에서 온다면... 그 때는 패리를 성공시킨다.
                if (enemyAttackCollider.bounds.center.x > playerCollider.bounds.center.x)
                {
                    Managers.instance.audioManager.PlaySFX(parrySuccessSFX, volume);
                }
                //  그게 아닌, 패리 방향과 반대 방향으로 공격을 받으면 그때는 데미지를 받게한다.
                else return false;

                break;

            case -1:
                //  만약 왼쪽으로 패리 했는데, 적 공격 콜라이더가 플레이어 콜라이더보다 더 왼쪽에서 온다면... 그 때는 패리를 성공시킨다.
                if (enemyAttackCollider.bounds.center.x < playerCollider.bounds.center.x)
                {
                    Managers.instance.audioManager.PlaySFX(parrySuccessSFX, volume);
                }
                //  그게 아닌, 패리 방향과 반대 방향으로 공격을 받으면 그때는 데미지를 받게한다.
                else return false;

                break;
        }
        return true;
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (!skillController.CanUseSkill())
            return;

        //  패리는 공격과 대쉬중에는 못 쓰게 막는다.
        if (context.performed && !onParring && canParry && !playerCombat.isAttacking && !Player.instance.warriorDash.onDash && !playerHealth.isDead)
        {
            StartCoroutine(StartParry());
        }
    }


    //  ------------ 타 스크립트에서 사용하기 위함-----------------
    //  대쉬중인걸 끊고, 애니메이션도 끊어버림
    public void CancelParry()
    {
        onParring = false;
        anim.ResetTrigger("Parry");
    }
}
