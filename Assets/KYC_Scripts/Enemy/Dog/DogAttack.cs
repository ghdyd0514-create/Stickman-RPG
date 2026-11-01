using System.Collections;
using UnityEngine;

public class DogAttack : EnemyAttack
{
    Rigidbody2D rigid;
    PlayerDetector playerDetector;
    Animator animator;
    EnemyMove enemyMove;

    [Header("비주얼 자식의 트랜스폼")]
    [SerializeField] Transform visualTrans;

    [Header("공격 전 대기시간. 잠시 대기하는 시간")]
    [SerializeField] float waitingTime = 2.0f;

    [Header("최소 공격 시간")]
    [SerializeField] float minimumAttackTime = 0.15f;
    float attackTimer;

    [Header("멍뭉이가 공격할 때 점프 높이")]
    [SerializeField] float jumpPower = 4.0f;

    [Header("똥개샛기가 돌진하는 속도")]
    [SerializeField] float dashPower = 4.0f;

    [Header("들개의 몸통(플레이어와 접촉하는) 콜라이더")]
    [SerializeField] CapsuleCollider2D contactCollider;

    [Header("공격 시 on 될 작은 범위의 몸통 콜라이더(공격중에만 on 되는거임)")]
    [SerializeField] CapsuleCollider2D contactColliderWhenAttacking;

    [Header("레이 설정")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Collider2D boxCastOriginCollider;

    [Header("공격 효과음")]
    [SerializeField] AudioClip attackSFX;
    [SerializeField] float volume;

    bool useRay;

    protected override void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        enemyMove = GetComponent<EnemyMove>();
        playerDetector = GetComponentInChildren<PlayerDetector>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (useRay)
        {
            ActivateBaxCast();
        }

        if (!isAttacking)
        {
            contactCollider.enabled = true;
            contactColliderWhenAttacking.enabled = false;
            attackCollider.enabled = false;
        }

        if (playerDetector.DetectPlayer && canAttack)
        {
            Attack();
        }
    }

    //  플레이어 발견 시, 잠시 대기했다가 물려고 점프함.
    public override void Attack()
    {
        if (canAttack)
        {
            canAttack = false;
            StartCoroutine(DogAttackCoroutine());
        }
    }

    IEnumerator DogAttackCoroutine()
    {
        isAttacking = true;
        //  공격 준비 자세 하고
        enemyMove.canFlip = false;
        enemyMove.canMove = false;
        animator.SetBool("DogAttackReady", true);

        //  강아지가 경사면에서 공격 자세 준비 한다면 , 미끄러지지 않게 보정해준다
        if (enemyMove.isSlope && animator.GetBool("DogAttackReady"))
        {
            float timer = 0;
            while (timer < waitingTime)
            {
                timer += Time.deltaTime;
                rigid.linearVelocity = Vector2.zero;

                yield return null;
            }
        }
        else if (!enemyMove.isSlope && animator.GetBool("DogAttackReady"))
        {
            yield return new WaitForSeconds(waitingTime);
        }

        //  공격
        AttackDash();
        Managers.instance.audioManager.PlaySFX(attackSFX, volume);
    }

    void AttackDash()
    {
        

        attackTimer = 0f;
        rigid.linearVelocity = Vector2.zero;
        animator.SetBool("DogAttack", true);

        useRay = true;

        rigid.linearVelocity = new Vector2(-enemyMove.GetDirection() * dashPower, jumpPower);
        contactCollider.enabled = false;    //  몸통 접촉 콜라이더는 공격 콜라이더와 겹칠 수 있음으로 false시킨다.
        contactColliderWhenAttacking.enabled = true;    //  단, 공격 콜라이더만 있을 때, 플레이어가 들개와 접촉한다면 몸통 데미지를 입지 않으므로 작은 범위의 몸통 접촉 콜라이더를 추가한다.
        attackCollider.enabled = true;
    }


    //  공격 중일 때 아래로 쏟는 ray를 활성화 시킨다. 이유) 멍뭉이는 땅에 닿아야지 공격이 false가 되기 때문
    void ActivateBaxCast()
    {
        attackTimer += Time.deltaTime;

        Vector2 boxCastTransform = new Vector2(boxCastOriginCollider.bounds.center.x,
                                                boxCastOriginCollider.bounds.center.y);

        Vector2 boxCastSize = new Vector2(boxCastOriginCollider.bounds.size.x, boxCastOriginCollider.bounds.size.y);

        RaycastHit2D boxCastHit = Physics2D.BoxCast(boxCastTransform, boxCastSize, 0,
                                                    Vector2.down, 0, groundLayer);

        // 공격 직후에는 착지 판정 금지
        if (attackTimer < minimumAttackTime)
            return;

        //  강아지가 점프를 했는데 점프 바로 직전 ray가 땅을 인식할 경우를 예외처리하기 위함.
        if (rigid.linearVelocity.y > 0.01f)
        {
            if (boxCastHit.collider != null && useRay)
            {
                return;
            }
        }

        if (boxCastHit.collider != null && useRay)
        {
            EndAttack();
        }
    }

    // 강아지가 플레이어를 무는 데 성공했다면, 그 자리에서 내려온다.
    public void BiteSuccess()
    {
        if (!isAttacking) return;

        //  플레이어를 물게 되면.... 공격 콜라이더가 먼저 사라지게 되어서 데미지가 들어가지 않는 불상사가 생기게 된다.
        //  따라서 데미지는 들어갈 수 있도록 0.1초정도 보정을 해준다.
        StartCoroutine(BiteAdjust());
    }

    IEnumerator BiteAdjust()
    {
        yield return null;
        yield return null;
        rigid.linearVelocity = Vector2.zero;
        EndAttack();
    }

    void EndAttack()
    {

        useRay = false;

        isAttacking = false;
        enemyMove.canFlip = true;
        enemyMove.canMove = true;

        attackCollider.enabled = false;
        contactCollider.enabled = true;
        contactColliderWhenAttacking.enabled = false;

        animator.SetBool("DogAttackReady", false);
        animator.SetBool("DogAttack", false);

        StartCoroutine(CoolDown());
    }
}
