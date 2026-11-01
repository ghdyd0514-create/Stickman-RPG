using System.Collections;
using UnityEngine;

//  종유석의 작동 방식
//  1. 종유석이 레이를 발사함. 2. 레이에 플레이어 콜라이더가 닿으면 종유석이 2초?? 쯤 뒤에 떨어짐
//  3. 떨어질 때는 레이가 꺼지며(최적화), 종유석 콜라이더가 enable 되며 이 때 플레이어가 종유석에 닿으면 데미지를 입는다.
//  4. 종유석은 플레이어를 맞추든 말든 땅에 떨어져야 작동이 종료됨.
public class Stalactite : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rigid;
    CameraShake cameraShake;

    [Header("종유석 콜라이더")]
    [SerializeField] Collider2D stalactiteCollider;

    [Header("종유석 ray가 나오는 콜라이더")]
    [SerializeField] Collider2D rayCollider;

    [Header("종유석 물리 콜라이더")]
    [SerializeField] Collider2D stalactitePhysicsCollider;

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
        float checkDistance = Mathf.Abs(rigid.linearVelocity.y) * Time.fixedDeltaTime;

        RaycastHit2D groundHit = Physics2D.BoxCast(stalactitePhysicsCollider.bounds.center, stalactitePhysicsCollider.bounds.size,
            0f, Vector2.down, checkDistance, groundLayer);

        if (groundHit.collider != null && !detectGround)
        {
            detectGround = true;

            rigid.linearVelocity = Vector2.zero;

            stalactiteCollider.enabled = false;

            rigid.bodyType = RigidbodyType2D.Kinematic;

            Managers.instance.audioManager.PlaySFX(breakStalactiteSFX, volume);
            cameraShake.Shake(shakePower);
            anim.SetTrigger("break");
        }
    }


    void CheckPlayer()
    {
        //  플레이어가 닿기 전에는 레이가 ground 레이어를 식별해서 ray의 길이를 ground에 닿는 만큼으로 설정한다
        RaycastHit2D groundHit = Physics2D.Raycast(new Vector2(rayCollider.bounds.center.x, rayCollider.bounds.min.y), Vector2.down, Mathf.Infinity, groundLayer);
        if (groundHit.collider == null) { return; }

        float maxDetectDistance = groundHit.distance;

        //  그리고 그 길이만큼을 플레이어 확인용 ray의 길이로 선정한다
        RaycastHit2D playerHit = Physics2D.Raycast(new Vector2(rayCollider.bounds.center.x, rayCollider.bounds.min.y), Vector2.down, maxDetectDistance, playerLayer);
        Debug.DrawRay(new Vector2(rayCollider.bounds.center.x, rayCollider.bounds.min.y), Vector2.down * maxDetectDistance, Color.green);

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
        if (stalactitePhysicsCollider == null)
            return;

        // 플레이 중이 아니면 Rigidbody velocity를 못 쓰는 경우가 있으니 기본값 사용
        float checkDistance = 0f;

        if (Application.isPlaying && rigid != null)
        {
            checkDistance =
                Mathf.Abs(rigid.linearVelocity.y) * Time.fixedDeltaTime;
        }
        else
        {
            checkDistance =
                fallingSpeed * Time.fixedDeltaTime;
        }

        Vector2 startCenter = stalactitePhysicsCollider.bounds.center;
        Vector2 boxSize = stalactitePhysicsCollider.bounds.size;

        Vector2 endCenter =
            startCenter + Vector2.down * checkDistance;

        Gizmos.color = Color.green;

        // 시작 박스
        Gizmos.DrawWireCube(startCenter, boxSize);

        Gizmos.color = Color.red;

        // BoxCast가 끝나는 위치의 박스
        Gizmos.DrawWireCube(endCenter, boxSize);

        Gizmos.color = Color.yellow;

        // 가운데 이동 방향 표시
        Gizmos.DrawLine(startCenter, endCenter);
    }
}
