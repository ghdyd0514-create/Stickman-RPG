using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

//  아래공격 특)
//  플레이어 캐릭터는 아래공격 모션을 취한다
//  아래공격으로 적을 피격 할 경우, 캐릭터는 위로 올라간다.
//  그리고 다시 아래공격을 감행한다. 이것은 총 3번까지 가능하다.
//  아래공격 3초 유지 시간 동안 적을 피격하지 않거나, 땅에 닿을 경우 아래공격 취소
//  아래공격은 공중 상태에서만 쓸 수 있다.
//  기본공격 제외 이동, 스킬, 대쉬, 패리 등 자유롭게 쓸 수 잇음
public class DownAttack : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerMove playerMove;
    PlayerHealth playerHealth;
    ChargeAttack chargeAttack;
    PlayerSkillController skillController;
    PlayerHitbox playerHitbox;

    [Header("증강 얻엇니")]
    [SerializeField] bool areYouObtainAugment = false;

    [Header("아래공격 공격력 배율")]
    [SerializeField] float downAttackMultiplier = 0.8f;

    [Header("아래공격 카운트")]
    [SerializeField] int downAttackCount = 3;

    [Header("아래공격 유지시간")]
    [SerializeField] float downAttackTime = 3.0f;   //  3초가 지나면 아래공격 풀림. 3초 안에 피격 입히면, 다시 3초 유지.

    [Header("아래공격 튕기는 정도")]
    [SerializeField] float downAttackBouncePower = 1.0f;

    [Header("아래공격 중인가요?")]
    [SerializeField] bool isDownAttacking = false;

    [Header("아래공격 가능 여부")]
    public bool canDownAttack = false;

    [Header("아래공격 콜라이더")]
    [SerializeField] Collider2D downAttackCollider;

    [Header("카메라 쉐이크")]
    [SerializeField] float cameraShakePower;

    [Header("아래공격 적 타격 효과음")]
    [SerializeField] AudioClip downAttackHitSFX;
    [SerializeField] float volume = 1.0f;

    Coroutine downAttackCoroutine;
    HashSet<EnemyHurtbox> hitEnemies = new HashSet<EnemyHurtbox>();

    void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMove = GetComponent<PlayerMove>();
        playerHitbox = GetComponent<PlayerHitbox>();
        playerHealth = GetComponent<PlayerHealth>();
        anim = GetComponentInChildren<Animator>();
        chargeAttack = GetComponent<ChargeAttack>();
        rigid = GetComponent<Rigidbody2D>();
        downAttackCollider.gameObject.SetActive(false);
    }

    IEnumerator StartDownAttack()
    {
        chargeAttack.CancelCharge(); 
        StartAttack();

        float timer = 0;
        int downAttackCountTotal = 0;
        bool isHit = false;
        while (timer < downAttackTime)
        {
            timer += Time.deltaTime;

            //  3번 이상 적을 피격 하면
            if (downAttackCountTotal >= downAttackCount)
            {
                break;
            }

            //  아래공격도중 적에게 피격 받는다면
            if (playerHealth.isDamaged)
            {
                anim.ResetTrigger("DownAttackOn");
                break;
            }

            //  아래공격도중 땅에 닿는다면
            if (playerMove.isGround)
            {
                break;
            }

            Collider2D hit = Physics2D.OverlapBox(downAttackCollider.bounds.center,
                                                downAttackCollider.bounds.size, 0, LayerMask.GetMask("EnemyHurtBox"));

            if (hit != null && !isHit)
            {
                isHit = true; //    한 프레임에 연속으로 적이 맞는 걸 제한하기 위함
                timer = 0;  //  적이 맞았으니까 다시 시간 초기화
                downAttackCountTotal++;

                EnemyHurtbox enemyHurtbox = hit.GetComponent<EnemyHurtbox>();

                int damage = (int)(playerCombat.attackPower * downAttackMultiplier);

                //  적에게 데미지 주기
                playerHitbox.EnemyHit(enemyHurtbox, damage, cameraShakePower, null, downAttackHitSFX, volume);

                //  반동으로 띄우기 전, y속도 초기화
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);

                //  적을 피격하면 반동으로 위로 뜬다.
                rigid.AddForce(Vector2.up * downAttackBouncePower, ForceMode2D.Impulse);
            }

            //  플레이어가 적을 맞추고 위로 올라가면 그 때 isHit을 다시 false화 시킴
            if (isHit && rigid.linearVelocity.y <= 0.0f)
            {
                isHit = false;
            }

            yield return null;
        }

        EndAttack();
    }


    //  w를 눌러서 작동
    public void OnDownAttack(InputAction.CallbackContext context)
    {
        if (!skillController.CanUseSkill())    
            return;
            
        if (context.performed && areYouObtainAugment && !isDownAttacking &&
            canDownAttack && playerMove.isAir && !playerMove.isGround)
        {
            downAttackCoroutine = StartCoroutine(StartDownAttack());
        }
    }


    //--------------------------------------------------
    //  잡다한 것들
    //--------------------------------------------------
    void StartAttack()
    {
        //  조건 끄기
        anim.SetBool("DownAttackEnd", false);
        playerCombat.canAttack = false;
        isDownAttacking = true;
        canDownAttack = false;

        //  애니메이션 설정
        anim.SetTrigger("DownAttackOn");


        downAttackCollider.gameObject.SetActive(true);
    }

    void EndAttack()
    {
        //  조건 복구
        downAttackCollider.gameObject.SetActive(false);

        isDownAttacking = false;
        playerCombat.canAttack = true;

        //  애니메이션에서 빠져나옴
        anim.SetBool("DownAttackEnd", true);    //  Trigger에서 Idle State로

        //  땅에 닿아야지 다시 쓸 수 있음.
        StartCoroutine(WaitLanding());
    }

    public void CancelDownAttack()
    {
        if (!isDownAttacking)
            return;

        if (downAttackCoroutine != null)
        {
            StopCoroutine(downAttackCoroutine);
            downAttackCoroutine = null;
        }
        
        EndAttack();
    }


    IEnumerator WaitLanding()
    {
        while (!playerMove.isGround)
        {
            yield return null;
        }

        canDownAttack = true;
    }

    public void SetAugmentObtain()
    {
        areYouObtainAugment = true;
        canDownAttack = true;
    }
}