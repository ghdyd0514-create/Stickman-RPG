using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    EnemyAttack enemyAttack;
    EnemyHealth enemyHealth;
    Boss boss;

    [Header("이동속도,방향")]
    [SerializeField] public float speed = 2f;
    private float direction = -1f;
    public float chaseSpeed = 3f;

    [Header("공중 몹이냐")]
    [SerializeField] bool isFlyingMonster = false;

    [Header("현재 이동 중인가")]
    public bool isWalking = false;

    [Header("이 녀석, 점프 가능하냐")]
    [SerializeField] bool canJump = false;           //  몇몇 몬스터는 점프 가능하게 ㄱㄱ. 아니면 모두 해야하나. 고민이다 흠

    [Header("점프 파워")]
    [SerializeField] float jumpPower = 5;

    [Header("점프 중인가")]
    [SerializeField] bool isJumping = false;

    [Header("넉백 중인가")]
    public bool isKnockBack = false;

    [Header("비주얼 자식 트랜스폼(flip변경을 위해서)")]
    [SerializeField] Transform visualChildren;

    [Header("땅 레이어")]
    [SerializeField] private LayerMask groundLayer;
    public float groundCheckDistance = 1f;

    [Header("현재 경사면인가")]
    public bool isSlope;

    [Header("플레이어")]
    public PlayerDetector playerDetector;

    [Header("벽 레이어")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 1f;

    [Header("레이가 바라보는 땅의 방향")]
    [SerializeField] Vector2 groundDirVec;

    [Header("바닥 감지 콜라이더(레이가 나오는 곳.)")]
    [SerializeField] Collider2D boxCastOriginCollider;

    [Header("타 스크립트에서 움직임 통제욧")]
    public bool canMove = true;
    public bool canFlip = true;

    [Header("보스냐? 보스면 경직 안 됨")]
    [SerializeField] bool isBoss;

    bool keepWalkAnimation = false;

    void Awake()
    {
        playerDetector = GetComponentInChildren<PlayerDetector>();
        rigid = GetComponent<Rigidbody2D>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        //  지상 몹만 땅 확인. 공중 몹은 날고 잇는데 바닥 확인할 필요가 굳이 있냐 ㅇㅇㅇ
        if (!isFlyingMonster)
            CheckGround();
    }

    void FixedUpdate()
    {
        //  넉백 중이면 넉백 시간동안 움직임을 제한한다. 그래야지 넉백이 되지 ㅇㅇ
        if (isKnockBack)
        {
            return;
        }
        
        //  몬스터가 피격 당해 경직이 되면 못 움직인다.
        if (enemyHealth.isStagger && !isBoss)
        {
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);

            isWalking = false;
            return;
        }

        //  지상 몬스터----------------------------
        if (!isFlyingMonster)
        {
            //  몬스터가 플레이어를 못 찾았는데 경사면에 있어서 중력을 받을 경우, 미끄러지지 않게 한다.
            if (!playerDetector.findPlayerDetected && isSlope)
            {
                if (enemyAttack != null && !enemyAttack.isAttacking)
                {
                    rigid.linearVelocity = Vector2.zero;
                    return;
                }
                rigid.linearVelocity = Vector2.zero;
                return;
            }

            if (playerDetector.findPlayerDetected)
            {
                float xDifference = playerDetector.DetectPlayer.position.x - transform.position.x;
                float deadZone = 0.2f;

                // 플레이어 방향으로 비주얼만 돌림 (canMove가 false여도 방향전환은 가능)
                if (canFlip)
                {
                    if (xDifference > deadZone && direction < 0)
                    {
                        Flip();
                    }
                    else if (xDifference < -deadZone && direction > 0)
                    {
                        Flip();
                    }
                }

                if (canMove)
                {
                    if (canJump)
                        CheckWall();

                    // 플레이어가 있는 방향
                    float chaseDirection = Mathf.Sign(xDifference);
                    rigid.linearVelocity = new Vector2(chaseDirection * chaseSpeed, rigid.linearVelocity.y);
                }

                // 플레이어 방향의 앞쪽에 땅이 있는지 확인
                /*bool hasGroundAhead = CheckGround(chaseDirection);

                if (hasGroundAhead)
                {
                    rigid.linearVelocity =
                        new Vector2(chaseDirection * chaseSpeed, rigid.linearVelocity.y);

                    isWalking = true;
                }
                else
                {
                    // 절벽 앞에서 멈춤
                    rigid.linearVelocity =
                        new Vector2(0f, rigid.linearVelocity.y);

                    // 실제로는 멈췄지만 걷기 애니메이션은 계속
                    isWalking = true;
                }*/
            }
        }

        //  공중 몬스터-----------------------------
        else
        {
            if (playerDetector.findPlayerDetected)
            {
                float xDifference = playerDetector.DetectPlayer.position.x - transform.position.x;
                float deadZone = 0.2f;

                // 플레이어 방향으로 비주얼만 돌림
                if (canFlip)
                {
                    if (xDifference > deadZone && direction < 0)
                    {
                        Flip();
                    }
                    else if (xDifference < -deadZone && direction > 0)
                    {
                        Flip();
                    }
                }

                if (canMove)
                {
                    Vector2 chaseDirection = ((Vector2)Player.instance.GetAimPoint().position - (Vector2)gameObject.transform.position).normalized;
                    rigid.linearVelocity = chaseDirection * chaseSpeed;
                }
            }
        }
        UpdateMoveAnimationState();
    }

    //  속도에 따라 walk를 할지 가만히 있을지
    private void UpdateMoveAnimationState()
    {
        if (keepWalkAnimation)
        {
            isWalking = true;
            return;
        }

        bool actuallyMoving = Mathf.Abs(rigid.linearVelocity.x) > 0.1f;

        isWalking = actuallyMoving;
    }

    void EnemyJump()
    {
        isJumping = true;
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    void CheckWall()
    {
        Vector2 wallCheck = new Vector2(direction, 0f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, wallCheck, wallCheckDistance, wallLayer);
        Debug.DrawRay(transform.position, wallCheck * wallCheckDistance, Color.purple);

        if (hit.collider != null && !isJumping)
        {
            EnemyJump();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }

    void CheckGround()
    {
        Vector2 boxCastTransform = new Vector2(boxCastOriginCollider.bounds.center.x,
                                                boxCastOriginCollider.bounds.center.y);

        Vector2 boxCastSize = new Vector2(boxCastOriginCollider.bounds.size.x, boxCastOriginCollider.bounds.size.y);

        //  몬스터의 왼쪽 구간을 확인하는 레이
        RaycastHit2D boxCastHit = Physics2D.BoxCast(boxCastTransform, boxCastSize, 0,
                                                    Vector2.down, groundCheckDistance, groundLayer);


        DrawBox(boxCastTransform, boxCastSize, Color.yellow);

        if (boxCastHit.collider != null)
        {
            GroundDirVec(boxCastHit);
        }
        else
        {
            isSlope = false;
        }
    }

    //  현재 레이가 바라보는 땅의 방향 벡터를 구한다.
    void GroundDirVec(RaycastHit2D rayHit)
    {
        groundDirVec = new Vector2(rayHit.normal.y, -rayHit.normal.x).normalized;

        //  경사면에 있을경우
        if (groundDirVec != new Vector2(1, 0))
        {
            isSlope = true;
        }
        //  평지에 있을경우
        else
        {
            isSlope = false;
        }
    }

    private void Flip()
    {
        direction *= -1;
        Vector3 visualScale = visualChildren.localScale;
        visualScale.x *= -1;
        visualChildren.localScale = visualScale;
    }

    public float GetDirection()
    {
        return visualChildren.localScale.x;
    }







    void DrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 half = size / 2f;

        Vector2 topLeft =
            center + new Vector2(-half.x, half.y);

        Vector2 topRight =
            center + new Vector2(half.x, half.y);

        Vector2 bottomLeft =
            center + new Vector2(-half.x, -half.y);

        Vector2 bottomRight =
            center + new Vector2(half.x, -half.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
}