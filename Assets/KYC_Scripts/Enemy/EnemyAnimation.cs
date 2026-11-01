using UnityEngine;

//  애니메이션 스크립트는 public으로 설정하여 원하는 애니메이션을 바로 참조해서 재생할 수 있도록
public class EnemyAnimation : MonoBehaviour
{
    Animator animator;
    EnemyMove enemyMove;
    EnemyAttack enemyAttack;
    EnemyHealth enemyHealth;

    void Awake()
    {
        animator = GetComponent<Animator>();
        enemyMove = GetComponentInParent<EnemyMove>();
        enemyAttack = GetComponentInParent<EnemyAttack>();
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    void Update()
    {
        if (enemyAttack != null && enemyAttack.isAttacking)
        {
            animator.SetBool("Walk", false);
            return;
        }

        if (enemyMove != null)
        {
            animator.SetBool("Walk", enemyMove.isWalking);
        }
    }

    //  몬스터 피격 당하면 재생
    public void PlayHit()
    {
        animator.SetTrigger("isHit");
    }

    //  몬스터 뒤1지면 재생
    public void PlayDeath()
    {
        animator.SetTrigger("isDead");
    }

    //  몬스터가 머무를 때 재생
    public void PlayStay()
    {
        animator.SetTrigger("Stay");
    }

    //  ㄱㄱㅈㅅ
    public void PlayWalk()
    {
        animator.SetTrigger("Walk");
    }

    //  공격
    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    //-------------------------------------------피격 당했을때 피격의 bool변수를 false로 만든다
    public void StaggerOff()
    {
        enemyHealth.isStagger = false;
    }





    // 맞았을 때 애니메이션 시작 지점에 Animation Event로 연결
    /*public void StopEnemy()
    {
        SetEnemyMovement(false);
    }

    // 맞았을 때 애니메이션 끝나는 지점에 Animation Event로 연결
    public void onEnemy()
    {
        SetEnemyMovement(true);
    }
    
    //  AllEnemyStop스크립트 내용 복붙한거임 --------------------------------------------- 사용이 필요할때만 사용
    private void SetEnemyMovement(bool isMoving)
    {
        EnemyMove enemyMove = GetComponentInParent<EnemyMove>();
        if (enemyMove != null)
            enemyMove.enabled = isMoving;

        FlyingMove flyingMove = GetComponentInParent<FlyingMove>();
        if (flyingMove != null)
            flyingMove.enabled = isMoving;

        if (isMoving == false)
        {
            Rigidbody2D rigid = GetComponentInParent<Rigidbody2D>();
            if (rigid != null)
            {
                bool wasChasing = flyingMove != null
                    && flyingMove.detector != null
                    && flyingMove.detector.FindPlayerDetected;

                if (wasChasing)
                {
                    // 추격 중이었으면 Y도 진짜 이동값이었으니 완전히 정지
                    rigid.linearVelocity = Vector2.zero;
                }
                else
                {
                    // 순찰 중이었으면 Y축(둥실거림/중력)은 유지
                    rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
                }
            }
        }
    }*/









    /*MonsterHealth monsterHealth;
    EnemyMove enemyMove;     // 지상 몬스터면 여기 들어있음 (없으면 null)
    FlyingMove flyingMove;   // 비행 몬스터면 여기 들어있음 (없으면 null)
    Animator animation; 

    float EnemyMaxHp;

    // 직전 프레임의 이동 상태 (지상/비행 각각 따로 비교용)
    EnemyMove.MoveState previousGroundState;
    FlyingMove.MoveState previousFlyState;

    private void Start()
    {
        monsterHealth = GetComponentInParent<MonsterHealth>();
        enemyMove = GetComponentInParent<EnemyMove>();
        flyingMove = GetComponentInParent<FlyingMove>();
        animation = GetComponent<Animator>();

        EnemyMaxHp = monsterHealth.maxHp;

        if (enemyMove != null)
            previousGroundState = enemyMove.CurrentMoveState;

        if (flyingMove != null)
            previousFlyState = flyingMove.CurrentMoveState;
    }

    void Update()
    {
        // ---- 체력 변화 감지 (기존, 지상/비행 공통) ----
        if (monsterHealth.hp < EnemyMaxHp)
        {
            if (monsterHealth.hp <= 0)
            {
                animation.SetTrigger("isDead");
            }
            else
            {
                animation.SetTrigger("isHit");
            }
        }

        EnemyMaxHp = monsterHealth.maxHp;

        // ---- 지상 몬스터 이동 상태 변화 감지 (Stay / Walk) ----
        if (enemyMove != null)
        {
            if (enemyMove.CurrentMoveState != previousGroundState)
            {
                switch (enemyMove.CurrentMoveState)
                {
                    case EnemyMove.MoveState.Stay:
                        animation.SetTrigger("Stay");
                        break;

                    case EnemyMove.MoveState.Walk:
                        animation.SetTrigger("Walk");
                        break;
                }

                previousGroundState = enemyMove.CurrentMoveState;
            }
        }

        // ---- 비행 몬스터 이동 상태 변화 감지 (Stay / Fly) ----
        if (flyingMove != null)
        {
            if (flyingMove.CurrentMoveState != previousFlyState)
            {
                switch (flyingMove.CurrentMoveState)
                {
                    case FlyingMove.MoveState.Stay:
                        animation.SetTrigger("Stay");
                        break;

                    case FlyingMove.MoveState.Fly:
                        animation.SetTrigger("Walk"); // 지상몹의 Walk랑 같은 트리거 이름으로 재사용
                        break;
                }

                previousFlyState = flyingMove.CurrentMoveState;
            }
        }
    }*/
}