using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

//  차지공격
//  1. 플레이어가 차지하는 동안에는 맞아도 경직되지 않는다
//  2. 차지공격은 총 3단계로 구성된다
//  3. 1단계 - 기본공격과 피해량 동일. 2단계 - 기본공격력보다 쌤. 적이 약간 뒤로 밀려남. 3단계 - 기본공격력보다 엄청 쌤. 적이 크게 뒤로 밀려남
//  4. 단, 차지공격을 끊고 다른 스킬이나, 대쉬, 패리 사용가능
//  5. 차지공격은 공격 중(차지 끝나고 공격 모션 취하는 동안) 에는 대쉬, 패리, 다른 스킬 사용 등 이 행동을 끊지 못한다.
//  6.  키를 홀드하면서 차지하고 키를 떼면 공격한다.
//  7. 차지 공격은 3초의 쿨타임 가짐 ㅇㅇ

public class ChargeAttack : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerMove playerMove;
    PlayerHealth playerHealth;
    DownAttack downAttack;
    PlayerHitbox playerHitbox;
    PlayerSkillController skillController;

    [Header("증강 획득 여부")]
    [SerializeField] bool areYouObtainAugment = false;

    [Header("차지 상태")]
    [SerializeField] bool isCharging = false;
    [SerializeField] bool isChargeAttacking = false;

    [Header("차지 시간")]
    [SerializeField] float playerChargingTime = 0f;

    [SerializeField] float middleChargeTime = 1.25f;
    [SerializeField] float maxChargeTime = 2.5f;
    [SerializeField] float overChargeTime = 4f;

    [Header("차지 공격 배율")]
    [SerializeField] float[] chargeDamageMultiplier = new float[3];

    [Header("차지 공격 넉백")]
    [SerializeField] float[] chargeKnockBack = new float[3];
    [SerializeField] float chargeKnockBackY = 1.0f;
    [SerializeField] float knockBackTime = 0.2f;

    [Header("차지 공격 시간")]
    [SerializeField] float chargeAttackTime = 0.5f;

    [Header("차지 공격 쿨타임")]
    [SerializeField] float chargeAttackCooldonw = 3.0f;
    [SerializeField] GameObject skillColldownUI;

    [Header("차지 공격 콜라이더")]
    [SerializeField] Collider2D[] chargeAttackCollider = new Collider2D[3];

    [Header("차지 공격 이펙트")]
    [SerializeField] GameObject[] chargeAttackEffect = new GameObject[3];

    [Header("차지공격 가능 여부")]
    public bool canChargeAttack = false;

    [Header("카메라 쉐이크 파워")]
    [SerializeField] float[] cameraShakePower = new float[3];

    Coroutine chargingCoroutine;
    HashSet<EnemyHurtbox> hitEnemies = new HashSet<EnemyHurtbox>();

    int currentChargeLevel = 1;

    //-------------------------------------------------
    //  효과음 변수
    //-----------------------------------------------
    [Header("차지중 효과음")]
    [SerializeField] AudioClip onCharging1SFX;
    [SerializeField] AudioClip onCharging2SFX;
    [SerializeField] AudioClip onCharging3SFX;

    [SerializeField] AudioClip Charge1AttackSFX;
    [SerializeField] AudioClip Charge2AttackSFX;
    [SerializeField] AudioClip Charge3AttackSFX;

    [SerializeField] float volume = 1.0f;

    void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        playerCombat = GetComponent<PlayerCombat>();
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        playerHitbox = GetComponent<PlayerHitbox>();

        downAttack = GetComponent<DownAttack>();

        for (int i = 0; i < chargeAttackCollider.Length; i++)
        {
            chargeAttackCollider[i].gameObject.SetActive(false);
            chargeAttackEffect[i].gameObject.SetActive(false);
        }
    }


    public void OnChargeAttack(InputAction.CallbackContext context)
    {
        // F 누름
        if (context.performed)
        {
            if (!areYouObtainAugment)
                return;

            if (!canChargeAttack)
                return;

            if (!skillController.CanUseSkill())
                return;

            if (isCharging || isChargeAttacking)
                return;

            chargingCoroutine = StartCoroutine(Charging());
        }

        // F 뗌
        else if (context.canceled)
        {
            if (isCharging)
            {
                ReleaseCharge();
            }
        }
    }

    //-----------------------
    //  차지중  (다른 행동 가능)
    //----------------------
    IEnumerator Charging()
    {
        downAttack.CancelDownAttack();

        isCharging = true;
        playerChargingTime = 0f;
        currentChargeLevel = 1;

        Managers.instance.audioManager.PlaySFX(onCharging1SFX, volume);
        StartChargeAttack();

        while (isCharging)
        {
            playerChargingTime += Time.deltaTime;

            // 3단계
            if (playerChargingTime >= maxChargeTime && currentChargeLevel == 2)
            {
                //  단계별 차징 애니메이션 변경
                currentChargeLevel = 3;
                anim.SetInteger("ChargeLevel", currentChargeLevel);

                //isSfxStart = true;
                Managers.instance.audioManager.PlaySFX(onCharging3SFX, volume);

            }

            // 2단계
            else if (playerChargingTime >= middleChargeTime && currentChargeLevel == 1)
            {
                //  단계별 차징 애니메이션 변경
                currentChargeLevel = 2;
                anim.SetInteger("ChargeLevel", currentChargeLevel);
                Managers.instance.audioManager.PlaySFX(onCharging2SFX, volume);
            }

            // 1단계
            else if(currentChargeLevel == 1)
            {
                //  얘는 어차피 SetTrigger되자마자 1단계라 없어도 됨
                currentChargeLevel = 1;
            }

            // 너무 오래 들고 있으면 강제로 발사
            if (playerChargingTime >= overChargeTime)
            {
                ReleaseCharge();
                yield break;
            }

            yield return null;
        }

        //  키를 떼면 발사
        ReleaseCharge();
    }

    //-----------------------
    //  차지 끝. 방출함
    //----------------------
    void ReleaseCharge()
    {
        if (!isCharging)
            return;

        isCharging = false;

        //  애니메이션 종료
        anim.SetBool("ChargingEnd", true);  //  차징 단계가 어떻든 ChargingEnd를 true시키고, 차지 공격 애니메이션으로 이동한다
        anim.SetBool("GoChargeAttack", true);   //  차지 어택 애니메이션으로 이동한다.
        //  차지 어택 애니메이션은 ChargingEnd가 true, GoChargeAttack이 true여야 만족한다.
        //  오직 ChargingEnd만 true면 Idle로 이동

        StartCoroutine(DoChargeAttack());
    }

    //-----------------------
    //  모은 만큼 공격을 함
    //----------------------
    IEnumerator DoChargeAttack()
    {
        isChargeAttacking = true;

        // 여기서부터는 공격 모션이 끝날 때까지 다른 행동 불가능
        LockActions();

        rigid.linearVelocity = Vector2.zero;

        //  공격 이펙트와 콜라이더 생성
        switch (currentChargeLevel)
        {
            case 1:
                chargeAttackEffect[0].gameObject.SetActive(true);
                chargeAttackCollider[0].gameObject.SetActive(true);
                Managers.instance.audioManager.PlaySFX(Charge1AttackSFX, volume);
                break;

            case 2:
                chargeAttackEffect[1].gameObject.SetActive(true);
                chargeAttackCollider[1].gameObject.SetActive(true);
                Managers.instance.audioManager.PlaySFX(Charge2AttackSFX, volume);
                break;

            case 3:
                chargeAttackEffect[2].gameObject.SetActive(true);
                chargeAttackCollider[2].gameObject.SetActive(true);
                Managers.instance.audioManager.PlaySFX(Charge3AttackSFX, volume);
                break;
        }

        //  쿨타임UI 흐르게
        StartCoroutine(skillColldownUI.GetComponent<SkillCooldownUI>().AppearActiveSkillTime(chargeAttackCooldonw));

        //  실제 쿨타임 기능
        StartCoroutine(ChargeAttackCooldown());

        // 공격 판정 유지시간
        yield return StartCoroutine(CheckEnemyHit(currentChargeLevel - 1));

        switch (currentChargeLevel)
        {
            case 1:
                chargeAttackEffect[0].gameObject.SetActive(false);
                chargeAttackCollider[0].gameObject.SetActive(false);
                break;

            case 2:
                chargeAttackEffect[1].gameObject.SetActive(false);
                chargeAttackCollider[1].gameObject.SetActive(false);
                break;

            case 3:
                chargeAttackEffect[2].gameObject.SetActive(false);
                chargeAttackCollider[2].gameObject.SetActive(false);
                break;
        }

        EndAttack();
    }

    //------------------------------------
    //  차지공격 데미지 계산 및 전달-
    //------------------------------------
    IEnumerator CheckEnemyHit(int index)
    {
        float timer = 0;
        while (timer < chargeAttackTime)
        {
            timer += Time.deltaTime;

            if (chargeAttackCollider[index] != null && chargeAttackCollider[index].gameObject.activeSelf)
            {
                Collider2D[] hits = Physics2D.OverlapBoxAll(chargeAttackCollider[index].bounds.center,
                                                chargeAttackCollider[index].bounds.size, 0, LayerMask.GetMask("EnemyHurtBox"));

                foreach (Collider2D hit in hits)
                {
                    EnemyHurtbox enemyHurtbox = hit.GetComponentInChildren<EnemyHurtbox>();

                    if (enemyHurtbox == null)
                        continue;

                    // 이번 공격으로 이미 맞은 적이면 패스
                    if (hitEnemies.Contains(enemyHurtbox))
                        continue;

                    hitEnemies.Add(enemyHurtbox);

                    int damage = (int)(playerCombat.attackPower * chargeDamageMultiplier[index]);

                    playerHitbox.EnemyHit(enemyHurtbox, damage, cameraShakePower[index], null, null, 0);

                    //  적 날려버리기
                    StartCoroutine(ChargeKnockBack(hit, index));
                }
            }

            yield return null;
        }
    }

    //  --------------------------------------------------
    //  적 날려버리는 함수
    //  -----------------------------------------------
    IEnumerator ChargeKnockBack(Collider2D collider, int index)
    {
        EnemyMove enemyMove = collider.GetComponentInParent<EnemyMove>();
        Rigidbody2D enemyRigid = collider.GetComponentInParent<Rigidbody2D>();

        enemyMove.isKnockBack = true;
        enemyRigid.linearVelocity = Vector2.zero;

        // 플레이어 기준으로 적이 어느 방향에 있는지
        float directionX = enemyRigid.position.x >= rigid.position.x ? 1f : -1f;

        //  x축 넉백
        enemyRigid.AddForce(Vector2.right * directionX * chargeKnockBack[index], ForceMode2D.Impulse);

        //  y축 띄우기
        enemyRigid.AddForce(Vector2.up * chargeKnockBackY, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);

        enemyMove.isKnockBack = false;
    }

    //-----------------------
    //  행동제한
    //----------------------
    void LockActions()
    {
        canChargeAttack = false;

        playerCombat.canAttack = false;
        playerMove.canMove = false;

        skillController.LockSkill();
    }

    void StartChargeAttack()
    {
        // 차지하면서 평타 / 이동만 막음.
        // 대쉬 / 패리 / 다른 스킬은 아직 가능.
        playerCombat.canAttack = false;
        playerMove.canMove = false;

        hitEnemies.Clear();

        //  슈퍼아머 부여
        playerHealth.isSuperArmor = true;

        //  X속도 초기화
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // 애니메이션 설정 및 초기화
        anim.SetInteger("ChargeLevel", 1);
        anim.SetBool("ChargingEnd", false);
        anim.SetBool("GoChargeAttack", false);
        anim.SetTrigger("ChargingOn");         //  시작과 동시에 1단계 차징 on
    }

    void EndAttack()
    {
        isChargeAttacking = false;
        playerChargingTime = 0f;
        currentChargeLevel = 1;

        playerCombat.canAttack = true;
        playerMove.canMove = true;
        playerHealth.isSuperArmor = false;

        skillController.UnlockSkill();

        anim.SetBool("ChargingEnd", true);
        anim.SetBool("GoChargeAttack", false);      //  Idle로 이동
    }

    // 차지중에 다른 스킬 / 대쉬 / 패리를 사용했을 때 호출
    public void CancelCharge()
    {
        if (!isCharging)
            return;

        isCharging = false;
        playerChargingTime = 0f;
        currentChargeLevel = 1;

        playerCombat.canAttack = true;
        playerMove.canMove = true;

        anim.SetBool("ChargingEnd", true);
        anim.SetBool("GoChargeAttack", false);

        if (chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
            chargingCoroutine = null;
        }

        playerHealth.isSuperArmor = false;
    }

    IEnumerator ChargeAttackCooldown()
    {
        canChargeAttack = false;
        yield return new WaitForSeconds(chargeAttackCooldonw);
        canChargeAttack = true;
    }


    public int GetChargeLevel()
    {
        return currentChargeLevel;
    }


    public void SetAugmentObtain()
    {
        areYouObtainAugment = true;
        canChargeAttack = true;
    }
}