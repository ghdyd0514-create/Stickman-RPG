/*using UnityEngine;

public class FlyingMove : MonoBehaviour
{
    public enum MoveState { Fly, Stay }
    public MoveState CurrentMoveState = MoveState.Fly;

    Rigidbody2D rigid;
    EnemyAttack enemyAttack;
    EnemyHealth enemyHealth;

    [Header("이동")]
    public float speed = 2f;
    public float direction = -1f;
    public float chaseSpeed = 1.5f;

    [Header("비행 높이")]
    public float flyHeight = 1f;
    public float floatSpeed = 1f;

    [Header("위치지정")]
    float startY;

    [Header("땅")]
    public LayerMask ground;
    public float groundCheckDistance = 1f;

    [Header("플레이어 발견 콜라이더")]
    PlayerDetector playerDetector;

    [Header("벽")]
    public LayerMask wall;
    public float wallCheckDistance = 1f;

    [Header("랜덤 정지 (순찰 중에만 적용)")]
    public float checkInterval = 2f;              // 몇 초마다 멈출지 확률 체크할지
    [Range(0, 100)] public float stopChance = 20f; // 그 순간에 멈출 확률 (%)
    public float minStopTime = 1f;                 // 멈추면 최소 몇 초 있을지
    public float maxStopTime = 3f;                 // 멈추면 최대 몇 초 있을지
    [Range(0, 100)] public float directionChangeChance = 50f; // 다시 날 때 방향 바꿀 확률 (%)

    private float checkTimer = 0f;   // Fly 상태에서 흐른 시간
    private float stayTimer = 0f;    // Stay 상태에서 흐른 시간
    private float stayDuration = 0f; // 이번에 얼마나 멈춰있을지

    void Awake()
    {
        playerDetector = GetComponentInChildren<PlayerDetector>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyHealth = GetComponent<EnemyHealth>();
        rigid = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
    }

    void FixedUpdate()
    {
        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * flyHeight;
        float moveY = (newY - transform.position.y) * 10f;
        moveY = Mathf.Clamp(moveY, -3f, 3f);

        //  플레이어를 발견하면
        if (playerDetector.FindPlayerDetected == true)
        {
            Vector2 toPlayer = (playerDetector.DetectPlayer.position - transform.position).normalized;
            rigid.linearVelocity = new Vector2(toPlayer.x * chaseSpeed, toPlayer.y * chaseSpeed);

            if (toPlayer.x > 0 && direction < 0)
            {
                direction = 1f;
                Flip();
            }
            else if (toPlayer.x < 0 && direction > 0)
            {
                direction = -1f;
                Flip();
            }

            //CheckGround();
            CheckWall();
        }

        //  플레이어 발견 안했을 경우
        else
        {
            // ---- 순찰 중: Fly / Stay 상태로 나뉨 ----
            switch (CurrentMoveState)
            {
                case MoveState.Fly:
                    //CheckGround();
                    CheckWall();

                    float moveX = speed * direction;
                    rigid.linearVelocity = new Vector2(moveX, moveY);

                    // 일정 시간마다 멈출지 확률 체크
                    checkTimer += Time.fixedDeltaTime;
                    if (checkTimer >= checkInterval)
                    {
                        checkTimer = 0f;
                        float roll = Random.Range(0f, 100f);

                        if (roll < stopChance)
                        {
                            // 멈추기로 결정됨 -> Stay 상태로 전환 (좌우 이동만 멈춤, 위아래 둥실거림은 유지)
                            CurrentMoveState = MoveState.Stay;
                            stayDuration = Random.Range(minStopTime, maxStopTime);
                            stayTimer = 0f;
                        }
                    }
                    break;

                case MoveState.Stay:
                    // 좌우 이동은 0, 위아래 둥실거림은 계속
                    rigid.linearVelocity = new Vector2(0f, moveY);

                    stayTimer += Time.fixedDeltaTime;
                    if (stayTimer >= stayDuration)
                    {
                        // 멈춰있던 시간이 다 됨 -> 다시 날기 시작
                        float roll = Random.Range(0f, 100f);
                        if (roll < directionChangeChance)
                        {
                            direction = -direction;
                            Flip(); // 확률적으로 방향 전환
                        }

                        CurrentMoveState = MoveState.Fly;
                        checkTimer = 0f;
                    }
                    break;
            }
        }
    }

    /*void CheckGround()
    {
        Vector2 floorch = new Vector2(direction * 0.3f, -1f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, floorch, groundCheckDistance, ground);
        Debug.DrawRay(transform.position, floorch * groundCheckDistance,
                     hit.collider != null ? Color.blue : Color.red);
        if (hit.collider == null)
        {
            direction = -direction;
            Flip();
        }
    }

    void CheckWall()
    {
        Vector2 wallCheck = new Vector2(direction, 0f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, wallCheck, wallCheckDistance, wall);
        Debug.DrawRay(transform.position, wallCheck * wallCheckDistance,
                     hit.collider != null ? Color.green : Color.red);
        if (hit.collider != null)
        {
            direction = -direction;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}*/