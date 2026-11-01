using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    Vector2 dirVec;
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;
    Player player;

    //  전사 직업군 한정.
    WarriorParry warriorParry;
    WarriorDash warriorDash;

    [Header("비주얼 자식의 트랜스폼에 접근")]
    [SerializeField] Transform visualTransform;

    [Header("이동속도")]
    [SerializeField] public float baseSpeed = 2.0f;
    [SerializeField] public float currentSpeed = 0;
    [SerializeField] float acceleration = 17f;
    [SerializeField] float deceleration = 30f;

    [Header("공격 중 감속")]
    [SerializeField] float attackDeceleration = 20f;

    [Header("점프 파워")]
    [SerializeField] float jumpPower = 5.0f;
    [SerializeField] float jumpCutMultiplier = 0.3f;    //  점프 커팅하는 힘.

    //[Header("에어점프")]
    //[SerializeField] bool canAirJump = false;           //  공중에서 1회 점프 가능하게 해주는 변수. 더블점프라고도 볼 수 있음.

    [Header("현재 땅에 있는가")]
    public bool isGround = false;

    [Header("현재 점프 중인가")]
    [SerializeField] bool isJumping = false;

    [Header("현재 air상태인가")]
    public bool isAir = false;

    //[Header("현재 움직이는 키를 눌렀는가")]
    //[SerializeField] bool jumpKeyHolding = false;

    [Header("땅바닥 레이어 관련")]
    [SerializeField] BoxCollider2D groundCheckCollider;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float rayLength;

    [Header("레이가 바라보는 땅의 방향")]
    [SerializeField] Vector2 groundDirVec;

    [Header("경사")]
    [SerializeField] bool detectSlope = false;
    //[SerializeField] float maxSlopeAngle = 50f;
    //Vector2 slopeDirection;

    [Header("아래로 내려갈 수 있는 발판")]
    [SerializeField] LayerMask canDropPlatform;

    [Header("플레이어 본인의 캡슐 콜라이더")]
    [SerializeField] Collider2D playerCollider;

    [Header("점프 사운드")]
    [SerializeField] AudioClip jumpSoundSFX;
    [SerializeField] float volume = 1.0f;

    [Header("타 스크립트에서 움직임을 다를 수 있도록")]
    public bool canMove = true;
    public bool canJump = true;

    float playerDir;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        player = GetComponent<Player>();

        if (player.currentClass == PlayerClass.Warrior)
        {
            warriorParry = GetComponent<WarriorParry>();
            warriorDash = GetComponent<WarriorDash>();
        }

        currentSpeed = baseSpeed;
    }

    void Update()
    {
        //  플레이어가 죽으면 걷기, 대기, 공중 상태의 애니메이션을 초기화한다.
        if (playerHealth.isDead)
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Idle", false);
            anim.SetBool("Air", false);
            return;
        }

        //  레이를 사용하여 바닥 체크
        if (!playerHealth.isDead)
        {
            CheckGround();

            //  플레이어가 이동중이라면, 다음 ground에 대한 ray를 쏜다.
            if (Math.Abs(dirVec.x) > 0)
            {
                CheckNextGround();
            }
        }

        //  걷기 애니메이션 재생
        //  만약 직업군이 전사가 아니라면 일반적으로 움직인다.
        if (Player.instance.currentClass != PlayerClass.Warrior)
        {
            if (Math.Abs(rigid.linearVelocity.x) >= 0.5)
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
            }
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", true);
            }
        }
        //  하지만 직업군이 전사라면 대쉬중일 때는 대쉬 애니메이션을 쓰게 한다.
        else
        {
            if (Math.Abs(rigid.linearVelocity.x) >= 0.5 && !Player.instance.warriorDash.onDash)
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
            }
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", true);
            }
        }

    }

    void FixedUpdate()
    {
        // 이동 코드
        if (canMove)
        {
            float targetSpeed;
            float accel;

            if (playerCombat.isAttacking)
            {
                // 공격 중에는 이동 입력을 무시하고,
                // 현재 속도를 0 쪽으로 자연스럽게 감속시킨다.
                targetSpeed = 0f;
                accel = attackDeceleration;
            }
            //  전사 직업군 한정. 패리중이면 이동을 감속시킨다.
            else if (warriorParry != null && warriorParry.onParring)
            {
                targetSpeed = 0f;
                accel = attackDeceleration;
            }
            //  전사 직업군 한정. 대쉬중일때는 이동방향을 못 바꾸게 한다.
            else if (warriorDash.onDash) return;
            else
            {
                // 평소 이동
                targetSpeed = dirVec.x * currentSpeed;

                // 방향전환 코드
                if (!playerCombat.isAttacking)
                {
                    if (visualTransform.transform.localScale.x >= 1 && dirVec.x <= -1)
                    {
                        visualTransform.transform.localScale = new Vector3(-1 * visualTransform.transform.localScale.x, 1, 1);
                    }
                    else if (visualTransform.transform.localScale.x <= -1 && dirVec.x >= 1)
                    {
                        visualTransform.transform.localScale = new Vector3(-1 * visualTransform.transform.localScale.x, 1, 1);
                    }

                    playerDir = visualTransform.transform.localScale.x;
                }
                // 입력이 있으면 가속
                if (Mathf.Abs(targetSpeed) > 0.01f)
                    accel = acceleration;

                // 입력이 없으면 감속
                else
                    accel = deceleration;
            }

            //  공중에 있을 땐, 그냥 x축으로 이동할 수 있게금
            if (isAir)
            {
                float newX = Mathf.MoveTowards(rigid.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
                rigid.linearVelocity = new Vector2(newX, rigid.linearVelocity.y);
            }
            else
            {
                float currentGroundSpeed =
                    Vector2.Dot(rigid.linearVelocity, groundDirVec);

                float slopeTargetSpeed = targetSpeed;

                bool isUphill =
                    groundDirVec.y * dirVec.x > 0.01f;

                if (isUphill)
                {
                    float horizontalRatio = Mathf.Abs(groundDirVec.x);

                    horizontalRatio = Mathf.Max(horizontalRatio, 0.5f);

                    slopeTargetSpeed =
                        targetSpeed / horizontalRatio;
                }

                float newSpeed = Mathf.MoveTowards(
                    currentGroundSpeed,
                    slopeTargetSpeed,
                    accel * Time.fixedDeltaTime
                );

                rigid.linearVelocity = groundDirVec * newSpeed;
            }
            /*else
            {
                float currentSpeed = Vector2.Dot(rigid.linearVelocity, groundDirVec);
                float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.fixedDeltaTime);
                rigid.linearVelocity = groundDirVec * newSpeed;
            }*/
            /*
            if (playerCombat.isAttacking)    //  공격 중 감속하다가 속도가 일정 이하가 되면 속도를 0으로 함. 자연스럽게 멈추게 하기 위함.
            {
                if (Mathf.Abs(newX) < 0.1f)
                {
                    newX = 0f;
                }
            }
            */
        }
    }

    void NormalJump()
    {
        isJumping = true;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        Managers.instance.audioManager.PlaySFX(jumpSoundSFX, volume);
    }

    //  현재 땅에 닿고 있는지 레이가 확인해줌
    void CheckGround()
    {
        //  점프를 했을 때(혹은 날라갈 때) 원웨이 플랫폼에 안착하기 위함.
        if (isJumping && rigid.linearVelocity.y > 0.01f)
        {
            isGround = false;
            isAir = true;
            return;
        }

        RaycastHit2D rayHit = Physics2D.Raycast(new Vector2(groundCheckCollider.bounds.center.x, groundCheckCollider.bounds.min.y), Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(new Vector2(groundCheckCollider.bounds.center.x, groundCheckCollider.bounds.min.y), Vector2.down * rayLength, Color.red);

        bool groundDetect = rayHit.collider != null;

        //  점프를 했지만 ray가 아직 지면을 바라보고 있다면 ray 무시.
        if (isJumping && groundDetect && isGround)
        {
            isAir = true;
            return;
        }

        //  ray가 땅을 놓침
        else if (!groundDetect)
        {
            isAir = true;
            isGround = false;
            anim.SetBool("Air", true);

            return;
        }

        //  ray가 다시 땅을 읽음
        else if (groundDetect)
        {
            isAir = false;
            isJumping = false;
            isGround = true;
            //canAirJump = true;
            anim.SetBool("Air", false);

            if (isGround && !isJumping && !detectSlope) { GroundDirVec(rayHit); }
        }
    }

    //  플레이어가 이동하고 있는 방향의 끝에 ray를 쏴서, 다음 ground의 normal을 계산하게 한다.
    void CheckNextGround()
    {
        //  현재 플레이어가 이동하는 방향으로 rayPos를 설정~
        Vector2 rayPos = dirVec.x > 0.01f ?
            new Vector2(groundCheckCollider.bounds.max.x, groundCheckCollider.bounds.center.y) :
            new Vector2(groundCheckCollider.bounds.min.x, groundCheckCollider.bounds.center.y);

        RaycastHit2D rayHit = Physics2D.Raycast(rayPos, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(rayPos, Vector2.down * rayLength, Color.black);

        Vector2 nextGroundDir = new Vector2(rayHit.normal.y, -rayHit.normal.x).normalized;

        bool isUphill = nextGroundDir.y * dirVec.x > 0.01f;

        if (rayHit.collider != null && isUphill && !detectSlope)
        {
            detectSlope = true;
            Debug.Log("경사면 발견!");
            groundDirVec = nextGroundDir;
        }
        else if (rayHit.collider != null && !isUphill && detectSlope)
        {
            detectSlope = false;
            Debug.Log("경사면 종료. 평지 전환");
        }
    }

    //  현재 레이가 바라보는 땅의 방향 벡터를 구한다.
    void GroundDirVec(RaycastHit2D rayHit)
    {
        groundDirVec = new Vector2(rayHit.normal.y, -rayHit.normal.x).normalized;
    }

    //  일저시간동안 이동속도를 올려준다.
    public IEnumerator SpeedUp(float plusSpeed, float time)
    {
        currentSpeed += plusSpeed;
        yield return new WaitForSeconds(time);
        currentSpeed -= plusSpeed;
    }

    //  현재 땅이 dropPlatform인지 확인하게 하는 ray
    Collider2D GetCurrentDropPlatform()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(groundCheckCollider.transform.position, Vector2.down, rayLength, canDropPlatform);
        Debug.DrawRay(groundCheckCollider.transform.position, Vector2.down * rayLength, Color.green);

        if (rayHit.collider != null)
        {
            return rayHit.collider;
        }
        return null;
    }

    //  아래점프를 할 때 플랫폼을 관통하게 하는 코루틴
    IEnumerator DropThroughPlatform(Collider2D platformCollider)
    {
        // 플레이어와 현재 발판의 충돌을 잠시 무시한다.
        Physics2D.IgnoreCollision(
            playerCollider,
            platformCollider,
            true
        );

        //  충돌을 무시한 상태로 0.1초를 기다리게 해서,
        //  확실하게 플레이어 콜라이더와 발판 콜라이더가 서로 겹쳐질 수 있도록 함.
        yield return new WaitForSeconds(0.1f);

        //  플레이어 콜라이더와 발판 콜라이더가 겹쳐진다면 while문을 계속 수행한다.
        while (true)
        {
            ColliderDistance2D colliderDistance = playerCollider.Distance(platformCollider);

            if (!colliderDistance.isOverlapped)
            {
                break;
            }

            yield return null;
        }

        // 다시 충돌을 활성화한다.
        Physics2D.IgnoreCollision(
            playerCollider,
            platformCollider,
            false
        );
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && !playerHealth.isDead)
        {
            dirVec = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            dirVec = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && !playerHealth.isDead)
        {
            //  떨어질 수 있는 발판 위에서 떨어지게 만든다.
            if (dirVec.y < -0.5f && isGround)
            {
                Collider2D dropPlatformCollider = GetCurrentDropPlatform();
                if (dropPlatformCollider != null)
                {
                    StartCoroutine(DropThroughPlatform(dropPlatformCollider));
                }
                return;
            }

            //  땅에서 하는 일반 점프
            else if (canJump && isGround)
            //&& canAirJump)
            {
                NormalJump();
            }

            /*  공중에서 한 번 점프하는 에어 점프
            else if (canJump && !isGround && canAirJump)
            {
                AirJump();
            }*/

        }
        else if (context.canceled)
        {
            //  스페이스바를 떼면 점프 커팅 하기.
            if (rigid.linearVelocity.y > 0)
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y * jumpCutMultiplier);
        }
    }

    //  타 스크립트에서 플레이어가 현재 바라보는 방향을 가져옴
    public float GetPlayerDirection()
    {
        return playerDir;
    }


    //-------------------------------------------------------------------------------------------------

    // 공격력 추가욧
    public void AddSpeed(float plusSpeed)
    {
        currentSpeed += plusSpeed;
    }

    // 너 지금 땅에 있냐
    public bool AreYouOnGround()
    {
        return isGround;
    }

    public float CurrentSpeed()
    {
        return currentSpeed;
    }

    public float BaseSpeed()
    {
        return baseSpeed;
    }
}
