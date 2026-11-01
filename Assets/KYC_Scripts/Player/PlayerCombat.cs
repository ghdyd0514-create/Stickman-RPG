using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    PlayerHealth playerHealth;
    PlayerMove playerMove;
    Animator anim;
    PlayerHitbox playerHitbox;

    [Header("기본 공격력")]
    public int attackPower;

    [Header("추가 공격력(추가 공격력이 몇인지 알기위함.)")]
    public int plusAttackPower;

    [Header("현재 공격중인가")]
    public bool isAttacking = false;

    [Header("공격 가능한가")]
    public bool canAttack = true;

    [Header("공격 키 홀딩중이니?")]
    [SerializeField] bool attackKeyHolding = false;

    [Header("공격 잔상 및 히트박스 콜라이더 오브젝트")]
    public GameObject attackEffect;
    public PolygonCollider2D attackHitBox;

    [Header("공격 속도")]
    [SerializeField] float attackSpeed = 1.0f;

    [Header("smaples")]
    [SerializeField] float samples;

    [Header("공격 애니메이션 프레임 수")]
    [SerializeField] float attackAnimFrame;

    [Header("카메라 쉐이크")]
    [SerializeField] float cameraShake = 1.0f;

    //  공격력 변화를 실시간으로 알려주는 이벤트
    public event Action attackPowerChanged;
    HashSet<EnemyHurtbox> hitEnemies = new HashSet<EnemyHurtbox>();

    //------------------------------------------------
    [Header("증강 상태에 따른 추가 효과 유무")]
    bool onHPSteal = false;
    bool lowHpAttack = false, onLowHpAttack = false;

    [Header("일정 체력 퍼센트 이하부터는 공격강화 증강 획득에 따라 공격력을 올린다. 체력 퍼센트를 몇으로 할거냐")]
    [SerializeField] float lowHpAttackHpPercent = 20;

    [Header("저체력 버프 추가 공격력")]
    [SerializeField] int lowHpBonusAttackPower;
    [SerializeField] int acquiredLowHpBonusAttackPowerCount = 0;

    [Header("피흡 확률")]
    public int hpStealProbability = 0;

    //  -------------------------------------------------
    //  효과음 변수
    //  ---------------------------------------------------
    [Header("공격 소리")]
    [SerializeField] AudioClip attackSFX;
    [SerializeField] float volume = 1.0f;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMove = GetComponent<PlayerMove>();
        playerHitbox = GetComponent<PlayerHitbox>();
        anim = GetComponentInChildren<Animator>();
        attackEffect.gameObject.SetActive(false);
        attackHitBox.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        playerHealth.onHpPercent += CheckHpPercent;
    }

    void Start()
    {
        attackPower = GameProgress.instance.playerAttackPower;
    }

    //  현재 체력 퍼센트가 얼마인지 확인하고, 일정 체력 이하라면 증강 조건을 발동시킨다.
    void CheckHpPercent(float hpPercent)
    {
        if (hpPercent <= lowHpAttackHpPercent && lowHpAttack && !onLowHpAttack)
        {
            onLowHpAttack = true;   //  추가적인 공격력 획득을 막기 위함.
            lowHpBonusAttackPower = Mathf.RoundToInt(attackPower * acquiredLowHpBonusAttackPowerCount * 0.15f);
            AddPlusAttackPower(lowHpBonusAttackPower);
        }

        else if (hpPercent > lowHpAttackHpPercent && onLowHpAttack)
        {
            onLowHpAttack = false;

            DownPlusAttackPower(lowHpBonusAttackPower);

            lowHpBonusAttackPower = 0;
        }
    }


    IEnumerator AttackLoop()
    {
        while (attackKeyHolding)
        {
            // 피격 중이라면 피격 애니메이션 끝날 때까지 기다림
            while (playerHealth.isDamaged)
            {
                yield return null;
            }

            // 공격 불가능 상태면 기다림
            while (!canAttack)
            {
                yield return null;
            }

            // 기다리는 동안 공격키를 뗐으면 종료
            if (!attackKeyHolding)
                yield break;

            yield return StartCoroutine(Attack());
        }

    }

    IEnumerator Attack()
    {
        isAttacking = true;

        // 이번 기본공격에서 맞은 적 기록 초기화
        hitEnemies.Clear();

        // 공격속도에 맞춰 애니메이션 속도 변경
        anim.SetFloat("AttackSpeed", attackSpeed);
        anim.SetTrigger("Attack");

        float timer = 0f;

        float attackDuration = attackAnimFrame / samples / attackSpeed;

        while (timer <= attackDuration)
        {
            timer += Time.deltaTime;

            // 피격받으면 공격 취소
            if (playerHealth.isDamaged)
            {
                anim.ResetTrigger("Attack");

                attackEffect.SetActive(false);
                attackHitBox.gameObject.SetActive(false);

                playerMove.canJump = true;
                isAttacking = false;

                yield break;
            }

            // 공격 콜라이더가 켜져 있을 때만 적중 검사
            if (attackHitBox.gameObject.activeSelf)
            {
                CheckEnemyHit();
            }

            // 지상 공격 중 점프 방지
            if (playerMove.AreYouOnGround())
            {
                playerMove.canJump = false;
            }

            yield return null;
        }

        // 혹시 Animation Event가 꼬여도 마지막에 확실하게 꺼줌
        attackEffect.SetActive(false);
        attackHitBox.gameObject.SetActive(false);

        playerMove.canJump = true;
        isAttacking = false;
    }

    void CheckEnemyHit()
    {
        if (attackHitBox != null && attackHitBox.gameObject.activeSelf)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackHitBox.bounds.center,
                                            attackHitBox.bounds.size, 0, LayerMask.GetMask("EnemyHurtBox"));

            foreach (Collider2D hit in hits)
            {
                EnemyHurtbox enemyHurtbox = hit.GetComponentInChildren<EnemyHurtbox>();

                if (enemyHurtbox == null)
                    continue;

                // 이번 공격으로 이미 맞은 적이면 패스
                if (hitEnemies.Contains(enemyHurtbox))
                    continue;

                hitEnemies.Add(enemyHurtbox);

                int damage = attackPower;

                playerHitbox.EnemyHit(enemyHurtbox, damage, cameraShake, null, null, 0);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !playerHealth.isDead)
        {
            attackKeyHolding = true;

            if (!isAttacking && canAttack)
            {
                StartCoroutine(AttackLoop());
            }
        }
        else if (context.canceled)
        {
            attackKeyHolding = false;
        }
    }

    public float AttackPower()
    {
        return attackPower;
    }

    //  ------------------------------------------애니메이션 이벤트로 넣을 함수들.
    public void EnableHitBox()
    {
        attackEffect.SetActive(true);
        attackHitBox.gameObject.SetActive(true);
        Managers.instance.audioManager.PlaySFX(attackSFX, volume);
    }

    public void DisableHitBox()
    {
        attackEffect.SetActive(false);
        attackHitBox.gameObject.SetActive(false);
    }

    //  ------------------------------------------------------
    //  기본 공격력 그 자체를 올린다
    //  ------------------------------------------------------------
    public void AddOriginAttackPower(int plusAttackPower)
    {
        //  공격력 증가
        attackPower += plusAttackPower;

        //  ddol한테 공격력 증가 알랴줌
        AttackPowerProperty = attackPower;
    }

    public int AttackPowerProperty
    {
        get => GameProgress.instance.playerAttackPower;
        set => GameProgress.instance.playerAttackPower = value;
    }


    //  -------------------------------------------------------
    //  추가 공격력 관한 함수
    //  ---------------------------------------------------
    public void AddPlusAttackPower(int plusAttackPower)
    {
        attackPower += plusAttackPower;
        this.plusAttackPower += plusAttackPower;
        attackPowerChanged?.Invoke();
    }

    public void DownPlusAttackPower(int downAttackPower)
    {
        attackPower -= downAttackPower;
        plusAttackPower -= downAttackPower;
        attackPowerChanged?.Invoke();
    }

    //  저체력 증강 획득
    public void SetOnLowHpAttack(int count)
    {
        lowHpAttack = true;
        acquiredLowHpBonusAttackPowerCount += count;
        //  증강 획득할 때 플레이어의 체력이 일정이하 일 수 있기 때문에... 바로 적용할 수 있도록
        CheckHpPercent((float)playerHealth.currentHP / playerHealth.maxHP * 100f);
        attackPowerChanged?.Invoke();
    }

    //  -------------------------------------------------------
    //  공격속도 추가
    //  -------------------------------------------------

    public void AddAttackSpeed(float plusAttackSpeed)
    {
        attackSpeed += plusAttackSpeed;
    }

    //  피흡 증강을 얻으면 피흡 할 수 있게 해준다.
    //  몬스터가 피격 당하면 일정확률로 피해흡혈이 가능하도록
    public void OnHPSteal(int hpStealProbability)
    {
        onHPSteal = true;
        this.hpStealProbability = hpStealProbability;
    }
    public bool ReturnHPSteal()
    {
        return onHPSteal;
    }


}
