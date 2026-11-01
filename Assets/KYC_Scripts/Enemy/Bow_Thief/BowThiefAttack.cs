using System.Collections;
using UnityEngine;

//  활도적의 공격 로직
//  1. 플레이어가 몬스터의 PlayerDetector콜라이더 안에 들어옴
//  2. 공격 (단, 활도적의 경우 공격할 때 가만히 서서 공격하므로, PlayerDetector콜라이더가 플레이어를 찾아도 못 움직이게 속도를 0으로 해줘야함)
//  3. 단, 활도적의 경우에는 별도의 또 다른 PlayerDetector를 가지고 있어서, 이 범위안에 들어 올 경우, 뒤로 조금씩 이동한다. (뒤로 이동은 공격 쿨타임 중에만. 해당 콜라이더 안에 플레이어가 있다고 해도 공격은 함.)

public class BowThiefAttack : EnemyAttack
{
    Rigidbody2D rigid;
    Animator anim;
    EnemyMove enemyMove;
    PlayerDetector playerDetector;

    [Header("후퇴 전용 플레이어 감지 콜라이더")]
    [SerializeField] Collider2D playerDetectCollider;

    [Header("활도적의 애니메이션 이벤트 전용 스크립트")]
    [SerializeField] BowThiefAttackAnimationEvent bowThiefAttackAnimationEvent;

    [Header("후퇴 속도")]
    [SerializeField] float runPower = 1.0f;

    [Header("최소 후퇴 시간")]
    [SerializeField] float minRunAwayTime = 0.5f;
    [SerializeField] float maxRunAwayTime = 1.5f;

    [SerializeField] bool isRunningAway = false;

    protected override void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        enemyMove = GetComponent<EnemyMove>();
        playerDetector = GetComponentInChildren<PlayerDetector>();
    }

    void Update()
    {
        // 공격 가능 상태일 때는 공격을 해야하지만, 그 경우가 아니라면 활도적은 도망을 갈 수 있게 해줘야 한다.

        if (canAttack && !isAttacking && playerDetector.findPlayerDetected)
        {
            Attack();

            return;
        }

        // 공격 중이 아닐 때 후퇴 범위 확인
        if (!isAttacking)
        {
            RunColliderHit();
        }
    }

    //  후퇴 콜라이더 안에 플레이어가 들어올 경우의 함수
    void RunColliderHit()
    {
        Collider2D hit = Physics2D.OverlapBox(playerDetectCollider.bounds.center, playerDetectCollider.bounds.size, 0, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            // 공격 쿨타임 중이고 아직 도망 중이 아니라면
            if (!canAttack && !isRunningAway)
            {
                StartCoroutine(RunAwayCoroutine());
            }
        }
        else
        {
            // 도망 중이 아닐 때만 EnemyMove에게 이동권 반환
            if (!isAttacking && !isRunningAway)
            {
                enemyMove.canMove = true;
            }
        }
    }

    //  플레이어 반대 방향으로 도망
    IEnumerator RunAwayCoroutine()
    {
        isRunningAway = true;
        enemyMove.canFlip = false;
        enemyMove.canMove = false;

        float timer = 0f;

        while (!canAttack && !isAttacking)
        {
            timer += Time.deltaTime;

            Collider2D hit = Physics2D.OverlapBox(
                playerDetectCollider.bounds.center,
                playerDetectCollider.bounds.size,
                0f,
                LayerMask.GetMask("Player")
            );

            // 플레이어 자체를 완전히 놓쳤다면 종료
            if (!playerDetector.findPlayerDetected)
            {
                break;
            }

            // 최소 후퇴 시간이 지났고,
            // 플레이어가 근접 범위 밖으로 빠졌다면 후퇴 종료
            if (timer >= minRunAwayTime && hit == null)
            {
                break;
            }

            // 최대 후퇴 시간 전까지만 실제로 뒤로 이동
            if (timer < maxRunAwayTime)
            {
                float xDifference =
                    transform.position.x -
                    Player.instance.transform.position.x;

                float runDirection = Mathf.Sign(xDifference);

                rigid.linearVelocity = new Vector2(
                    runDirection * runPower,
                    rigid.linearVelocity.y
                );
            }
            else
            {
                // 최대시간이 지났는데 플레이어가 여전히 가까우면
                // 다시 접근하지 말고 그 자리에서 대기
                rigid.linearVelocity = new Vector2(
                    0f,
                    rigid.linearVelocity.y
                );
            }

            yield return null;
        }

        rigid.linearVelocity =
            new Vector2(0f, rigid.linearVelocity.y);

        isRunningAway = false;

        if (!isAttacking)
        {
            enemyMove.canFlip = true;
            enemyMove.canMove = true;
        }
    }

    //  화살 발사
    public override void Attack()
    {
        enemyMove.canFlip = false;
        enemyMove.canMove = false;
        isAttacking = true;
        canAttack = false;
        rigid.linearVelocity = Vector2.zero;

        //  기존 애니메이션 초기화
        anim.SetBool("BowThiefAttackEnd", false);

        anim.SetTrigger("BowThiefAttackReady"); //  활시위 당기는 애니메
    }

    //  공격 끝
    public void EndAttack()
    {
        enemyMove.canFlip = true;
        enemyMove.canMove = true;
        isAttacking = false;

        StartCoroutine(CoolDown());
    }
}