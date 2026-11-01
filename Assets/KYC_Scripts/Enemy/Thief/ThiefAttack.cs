using System.Collections;
using UnityEngine;

//  도적의 공격 로직
//  1. 플레이어가 도적의 공격 범위 콜라이더 안에 들어옴. 
//  2. 공격 (딱히 공격 대기 자세 같은 건 없음.)

//  공격 방식 : 그냥 찌르기 공격하는 거임. 근데 찌를 때 플레이어 방향으로 살짝 나아가면서 찌르면 될듯.
public class ThiefAttack : EnemyAttack
{
    Rigidbody2D rigid;
    Animator anim;
    EnemyMove enemyMove;

    [Header("공격 전용 플레이어 감지 콜라이더")]
    [SerializeField] Collider2D playerDetectCollider;

    [Header("공격 전, 선딜 딜레이")]
    [SerializeField] float beforeAttackDelay = 2.0f;

    [Header("공격ing 시간")]
    [SerializeField] float attackingTime = 0.8f;

    [Header("찌를 때 앞으로 나아가는 속도")]
    [SerializeField] float thrustForwardPower = 1.0f;

    [Header("공격 범위 콜라이더에 플레이어가 보였나")]
    [SerializeField] bool hasPlayerIn = false;

    protected override void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        enemyMove = GetComponent<EnemyMove>();
    }

    void Update()
    {
        if (!hasPlayerIn && canAttack && !isAttacking)
        {
            CheckAttackHit();
        }
    }

    //  플레이어 감지 콜라이더에 플레이어가 들어왔나 확인한다.
    void CheckAttackHit()
    {
        Collider2D hit = Physics2D.OverlapBox(playerDetectCollider.bounds.center, playerDetectCollider.bounds.size, 0, LayerMask.GetMask("Player"));

        if (hit != null && !hasPlayerIn)
        {
            hasPlayerIn = true;
            StartCoroutine(BeforeAttackDelay());
        }  
    }

    //  선딜 딜레이 시간 코루틴
    IEnumerator BeforeAttackDelay()
    {
        enemyMove.canFlip = false;
        enemyMove.canMove = false;
        rigid.linearVelocity = Vector2.zero;

        //  기존 애니메이션들 초기화
        anim.SetBool("ThiefAttackEnd", false);
        anim.SetBool("ThiefAttack", false);


        anim.SetTrigger("ThiefAttackReady");

        yield return new WaitForSeconds(beforeAttackDelay);
        Attack();
    }

    //  공격
    public override void Attack()
    {
        isAttacking = true;
        canAttack = false;

        anim.SetBool("ThiefAttack", true);      //  ThiefAttackReady랑 이어지는 애니메이션 state

        attackCollider.enabled = true;

        StartCoroutine(AttackingTime());
    }

    IEnumerator AttackingTime()
    {
        float timer = 0;
        while(timer < attackingTime)
        {
            timer += Time.deltaTime;
            rigid.linearVelocity = new Vector2(-enemyMove.GetDirection() * thrustForwardPower, rigid.linearVelocity.y);

            yield return null;   
        }

        //  이 애니메이션은, Trigger를 통해 ThiefAttack 애니메이션이 재생될 때, 다시 Idle로 돌아갈 수 있게 해주는 조건이다.
        anim.SetBool("ThiefAttackEnd", true);

        EndAttack();
    }

    //  공격 끝
    void EndAttack()
    {
        attackCollider.enabled = false;
        enemyMove.canFlip = true;
        enemyMove.canMove = true;
        isAttacking = false;
        hasPlayerIn = false;

        StartCoroutine(CoolDown());
    }
}