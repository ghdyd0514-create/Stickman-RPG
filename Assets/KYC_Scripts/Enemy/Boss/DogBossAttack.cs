using System.Collections;
using UnityEngine;

public class DogBossAttack : EnemyAttack
{
    Rigidbody2D rigid;
    Animator animator;
    EnemyMove enemyMove;
    EnemyHealth enemyHealth;
    PlayerDetector playerDetector;

    [Header("근접 공격")]
    [SerializeField] float nearAttackRange = 2f;

    [Header("돌진 공격")]
    [SerializeField] Collider2D dashAttackCollider;
    [SerializeField] float dashSpeed = 8f;
    [SerializeField] float dashDuration = 2f;
    [SerializeField] float dashJumpY = 1.0f;
    [SerializeField] float attackCooldown2 = 1.5f;

    [Header("뒤로 점프")]
    [Range(0f, 1f)]
    [SerializeField] float backJumpChance = 0.3f;

    [SerializeField] float backJumpPowerX = 4f;
    [SerializeField] float backJumpPowerY = 5f;
    [SerializeField] float backJumpDuration = 0.5f;

    [Header("잡몹 소환")]
    [SerializeField] GameObject summonMonsterPrefab;
    [SerializeField] Transform[] summonPoints = new Transform[2];

    // 소환 애니메이션 시작 후 몇 초 뒤에 실제 몬스터를 만들지
    [SerializeField] float summonSpawnDelay = 0.5f;

    [Header("공격 효과음")]
    [SerializeField] AudioClip howlingSFX;
    [SerializeField] float volume;

    // 지금까지 공격한 횟수
    int attackCount = 0;

    // 플레이어와 전투가 시작됐는가?
    bool battleStarted = false;

    // 현재 보스가 공격/점프/소환 등의 행동 중인가?
    bool isBusy = false;


    protected override void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
        enemyMove = GetComponent<EnemyMove>();
        enemyHealth = GetComponent<EnemyHealth>();

        // Visual 자식에 Animator가 있으므로
        animator = GetComponentInChildren<Animator>();

        // PlayerDetector도 자식에 있으므로
        playerDetector = GetComponentInChildren<PlayerDetector>();

        dashAttackCollider.gameObject.SetActive(false);
    }


    void Update()
    {
        // 죽었으면 아무것도 안 함
        if (enemyHealth != null && enemyHealth.isDead)
            return;

        if (playerDetector == null)
            return;

        // 플레이어 감지 못함
        if (!playerDetector.findPlayerDetected)
            return;

        // 플레이어 Transform이 없음
        if (playerDetector.DetectPlayer == null)
            return;


        // -------------------------------
        // 처음 전투 시작
        // -------------------------------
        if (!battleStarted)
        {
            battleStarted = true;

            // 첫 행동은 무조건 잡몹 소환
            StartCoroutine(FirstSummon());

            return;
        }


        // 다른 행동 중이면 새로운 행동 실행 X
        if (isBusy)
            return;

        if (!canAttack)
            return;

        Debug.Log("플레이어와의 거리 : " + (playerDetector.DetectPlayer.position.x - transform.position.x));
        Attack();
    }


    // =========================================================
    // 어떤 공격을 할지 결정
    // =========================================================

    public override void Attack()
    {
        animator.SetBool("DashEnd", false);
        if (playerDetector.DetectPlayer == null)
            return;

        float distance = Mathf.Abs(
            playerDetector.DetectPlayer.position.x - transform.position.x
        );


        // 플레이어가 가까움
        if (distance <= nearAttackRange)
        {
            float randomValue = Random.value;


            // 일정 확률로 뒤로 점프
            if (randomValue <= backJumpChance)
            {
                StartCoroutine(BackJump());
            }
            else
            {
                StartCoroutine(NearAttack());
            }
        }

        // 플레이어가 멀리 있음
        else
        {
            //  멀리있다고 대쉬만 하길 방지하기 위해
            //  멀리 있을 경우 일정확률로 대쉬를 안하고 그냥 걸어간다
            //  단 1.5초뒤에 다시 스킬을 사용할 수 있다.
            //  물론 1.5초 뒤에도 꽝이 뽑혀 대쉬를 안할 수 있다.

            int randomNum = Random.Range(0, 5);
            if (randomNum >= 2) //  2 3 4
            {
                StartCoroutine(CoolDown());
                return;
            }

            StartCoroutine(DashAttack());
        }
    }


    // =========================================================
    // 근접 할퀴기
    // =========================================================

    IEnumerator NearAttack()
    {
        isBusy = true;
        isAttacking = true;
        canAttack = false;

        StopMove();


        // NearAttack 애니메이션 재생
        animator.SetTrigger("NearAttack");


        yield break;
    }


    // =========================================================
    // 돌진 공격
    // =========================================================

    IEnumerator DashAttack()
    {
        isBusy = true;
        isAttacking = true;
        canAttack = false;

        StopMove();


        float playerX = playerDetector.DetectPlayer.position.x;

        // 플레이어가 있는 방향
        float dashDirection =
            Mathf.Sign(playerX - transform.position.x);


        animator.SetTrigger("Dash");


        // 돌진 시작
        rigid.linearVelocity =
            new Vector2(
                dashDirection * dashSpeed,
                dashJumpY
            );


        yield return new WaitForSeconds(dashDuration);


        // 돌진 종료
        rigid.linearVelocity =
            new Vector2(
                0f,
                rigid.linearVelocity.y
            );

        dashAttackCollider.gameObject.SetActive(false);

        animator.SetBool("DashEnd", true);

        isAttacking = false;


        // 이것도 공격 1회로 취급
        attackCount++;


        // 4번 공격했으면 소환
        if (attackCount >= 4)
        {
            attackCount = 0;

            yield return StartCoroutine(SummonMonster());
        }

        StartMove();

        isBusy = false;

        StartCoroutine(CoolDown());
    }


    // =========================================================
    // 뒤로 점프
    // =========================================================

    IEnumerator BackJump()
    {
        isBusy = true;
        canAttack = false;

        StopMove();


        float playerX = playerDetector.DetectPlayer.position.x;


        // 플레이어 방향
        float playerDirection =
            Mathf.Sign(playerX - transform.position.x);


        // 플레이어의 반대 방향
        float backDirection = -playerDirection;


        // 뒤 + 위로 점프
        rigid.linearVelocity =
            new Vector2(
                backDirection * backJumpPowerX,
                backJumpPowerY
            );


        yield return new WaitForSeconds(backJumpDuration);


        // 가로 이동만 멈춤
        rigid.linearVelocity =
            new Vector2(
                0f,
                rigid.linearVelocity.y
            );


        /*
         * 뒤로 점프는 "공격"이 아니므로
         * attackCount를 증가시키지 않는다.
         */


        yield return new WaitForSeconds(0.2f);


        StartMove();

        canAttack = true;
        isBusy = false;
    }


    // =========================================================
    // 전투 시작할 때 첫 소환
    // =========================================================

    IEnumerator FirstSummon()
    {
        isBusy = true;
        canAttack = false;

        StopMove();


        yield return StartCoroutine(SummonMonster());


        yield return new WaitForSeconds(attackCooldown);


        StartMove();

        canAttack = true;
        isBusy = false;
    }


    // =========================================================
    // 잡몹 소환
    // =========================================================

    IEnumerator SummonMonster()
    {
        StopMove();


        // 울부짖는 소환 애니메이션
        animator.SetTrigger("Wurlf");
        Managers.instance.audioManager.PlaySFX(howlingSFX, volume);

        yield break;
    }


    // =========================================================
    // 이동 멈춤
    // =========================================================

    void StopMove()
    {
        if (enemyMove != null)
        {
            enemyMove.canMove = false;
            enemyMove.canFlip = false;
        }


        if (rigid != null)
        {
            rigid.linearVelocity =
                new Vector2(
                    0f,
                    rigid.linearVelocity.y
                );
        }
    }


    // =========================================================
    // 다시 이동
    // =========================================================

    void StartMove()
    {
        if (enemyMove != null)
        {
            enemyMove.canMove = true;
            enemyMove.canFlip = true;
        }
    }


    // =========================================================
    // Animation Event에서 사용
    // =========================================================

    public void OnDashAttackCollider()
    {
        dashAttackCollider.gameObject.SetActive(true);
    }

    public void EndNearAttack()
    {
        isAttacking = false;

        attackCount++;

        StartMove();

        StartCoroutine(AfterAttack());
    }

    public void OnSummonMonster()
    {
        if (summonMonsterPrefab != null &&
            summonPoints != null)
        {
            Instantiate(
                summonMonsterPrefab,
                summonPoints[0].position,
                Quaternion.identity
            );

            Instantiate(
                summonMonsterPrefab,
                summonPoints[1].position,
                Quaternion.identity
            );
        }

        StartMove();
    }

    IEnumerator AfterAttack()
    {
        if (attackCount >= 4)
        {
            attackCount = 0;
            yield return StartCoroutine(SummonMonster());
        }
        StartMove();
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isBusy = false;
    }

    IEnumerator Cooldown2()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown2);
        canAttack = true;
    }
}