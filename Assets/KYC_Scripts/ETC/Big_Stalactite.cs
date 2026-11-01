using System.Collections;
using UnityEngine;

//  거대 종유석의 작동 방식
//  1. 거대 종유석은 박스캐스트를 가지고 있으며 박스캐스트에 플레이어가 닿으면 떨어질 준비를 한다.
//  (거대 종유석의 경우 거대해서 종유석의 가운데 부분에 ray를 쏘게 만들면, 플레이어가 종유석의 안쪽까지 들어가야지만 작동이 되기에, 그 전에 박스캐스트로 넓게 범위를 만들고 그 범위 안에 들어오면 작동하는 걸로 할려 함.)
//  2. 거대 종유석은 떨어지면 부서지지 않고, 오히려 하나의 ground가 되어 이동할 수 잇는 발판이 된다.
//  3. 단, 낙하 도중에 플레이어가 종유석에 맞는다면 즉사한다.
public class Big_Stalactite : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rigid;
    CameraShake cameraShake;

    [Header("종유석 콜라이더")]
    [SerializeField] Collider2D stalactiteCollider;

    [Header("종유석 박스캐스트가 나오는 콜라이더")]
    [SerializeField] Collider2D boxCastCollider;

    [Header("박스 캐스트 길이")]
    [SerializeField] float boxCastLength;

    [Header("종유석 물리 콜라이더")]
    [SerializeField] Collider2D stalactitePhysicsCollider;

    [Header("충돌 후, 발판이 되는 콜라이더")]
    [SerializeField] Collider2D groundCollider;

    [Header("ground와 player를 식별하는 레이어")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("플레이어를 발견하였는가")]
    [SerializeField] bool isDetectPlayer = false;

    [Header("플레이어 발견 뒤 흔들거리는(낙하 준비) 시간")]
    [SerializeField] float fallingReadyTime = 1.0f;

    [Header("낙하 속도")]
    [SerializeField] float fallingSpeed = 10.0f;

    [Header("카메라 쉐이크 정도")]
    [SerializeField] float shakePower = 0.6f;

    [SerializeField] bool detectGround = false;

    [Header("효과음")]
    [SerializeField] AudioClip breakStalactiteSFX;
    [SerializeField] float volume;

    void Awake()
    {
        stalactiteCollider.enabled = false;
        groundCollider.enabled = false;
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        cameraShake = GetComponent<CameraShake>();
    }

    void OnEnable()
    {
        rigid.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (!isDetectPlayer)
            CheckPlayer();
    }

    void FixedUpdate()
    {
        if (isDetectPlayer && !detectGround)
        {
            CheckGround();
        }
    }

    void CheckGround()
    {
        RaycastHit2D groundHit = Physics2D.BoxCast(stalactitePhysicsCollider.bounds.center, stalactitePhysicsCollider.bounds.size,
            0f, Vector2.down, 0, groundLayer);

        if (groundHit.collider != null && !detectGround)
        {
            detectGround = true;

            rigid.linearVelocity = Vector2.zero;

            stalactiteCollider.enabled = false;

            groundCollider.enabled = true;

            rigid.bodyType = RigidbodyType2D.Kinematic;

            Managers.instance.audioManager.PlaySFX(breakStalactiteSFX, volume);
            cameraShake.Shake(shakePower);
        }
    }


    void CheckPlayer()
    {
        RaycastHit2D playerHit = Physics2D.BoxCast(boxCastCollider.bounds.center, boxCastCollider.bounds.size,
            0f, Vector2.down, boxCastLength, playerLayer);

        if (playerHit.collider != null)
        {
            isDetectPlayer = true;
            StartCoroutine(FallingReady());
        }
    }

    //  플레이어 발견 시 낙하 준비
    IEnumerator FallingReady()
    {
        anim.SetTrigger("fallingReady");

        yield return new WaitForSeconds(fallingReadyTime);

        anim.SetBool("isFalling", true);
        Fall();
    }

    void Fall()
    {
        rigid.bodyType = RigidbodyType2D.Dynamic;
        stalactiteCollider.enabled = true;

        rigid.linearVelocity = Vector2.down * fallingSpeed;
    }


    //  혹시 모른다. 낙하 하면서 플레이어가 연속으로 맞을 수도 있다. 한 번 맞으면 종유석 콜라이더를 다시 꺼버리게 하자
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player Hit Detector"))
        {
            StartCoroutine(ColliderEndCoroutine());
        }
    }

    //  콜라이더를 바로 없애면 데미지 들어가기 전에 콜라이더가 없어져서 그렇게 하면 아니된다. 그래서 이거 추가함.
    IEnumerator ColliderEndCoroutine()
    {
        yield return null;
        yield return null;

        stalactiteCollider.enabled = false;
    }



    void OnDrawGizmos()
    {
        // =========================
        // 1. 종유석 물리 콜라이더
        // =========================
        if (stalactitePhysicsCollider != null)
        {
            Vector2 physicsCenter = stalactitePhysicsCollider.bounds.center;
            Vector2 physicsSize = stalactitePhysicsCollider.bounds.size;

            // 물리 콜라이더 = 파란색
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(physicsCenter, physicsSize);
        }


        // =========================
        // 2. 플레이어 감지 BoxCast
        // =========================
        if (boxCastCollider != null)
        {
            Vector2 startCenter = boxCastCollider.bounds.center;
            Vector2 boxSize = boxCastCollider.bounds.size;

            Vector2 endCenter =
                startCenter + Vector2.down * boxCastLength;

            // BoxCast = 빨간색
            Gizmos.color = Color.red;

            // 시작 위치
            Gizmos.DrawWireCube(startCenter, boxSize);

            // BoxCast 끝 위치
            Gizmos.DrawWireCube(endCenter, boxSize);

            // 양쪽 끝을 연결해서 실제 BoxCast 범위 표시
            float halfWidth = boxSize.x / 2f;

            Vector3 startLeft =
                new Vector3(startCenter.x - halfWidth, startCenter.y, 0f);

            Vector3 startRight =
                new Vector3(startCenter.x + halfWidth, startCenter.y, 0f);

            Vector3 endLeft =
                new Vector3(endCenter.x - halfWidth, endCenter.y, 0f);

            Vector3 endRight =
                new Vector3(endCenter.x + halfWidth, endCenter.y, 0f);

            Gizmos.DrawLine(startLeft, endLeft);
            Gizmos.DrawLine(startRight, endRight);
        }
    }
}
