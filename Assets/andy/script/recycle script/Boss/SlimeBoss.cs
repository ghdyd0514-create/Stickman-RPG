using UnityEngine;
using System.Collections;

public class SlimeBoss : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Detect,
        Jumping,
        Landing,
        Cooldown,
        PhaseTwoPause
    }

    [Header("참조")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    [Header("보스 구역 설정 (좌우 이동만 제한)")]
    public BoxCollider2D bossArea;

    [Header("보스 체력 및 경직")]
    public float maxBossHealth = 300f;
    public float bossHealth;
    public float staggerDuration = 0.25f; // 경직 시간
    public bool isDead = false;
    public bool isStagger = false;        // 경직 상태 여부

    [Header("탐지 범위")]
    public float detectRange = 5f;
    public string playerTag = "Player";

    [Header("점프 이동 설정")]
    [Tooltip("점프하는 최고 높이")]
    public float jumpHeight = 4f;

    [Tooltip("점프가 시작해서 착지할 때까지 걸리는 시간")]
    public float jumpDuration = 1.0f;

    public float cooldownTime = 1.5f;

    [Header("점프 애니메이션 클립")]
    public AnimationClip clipStartUp;
    public AnimationClip clipUp;
    public AnimationClip clipToFall;
    public AnimationClip clipDown;
    public AnimationClip clipLand;
    public AnimationClip clipDeath;

    [Header("공격 히트박스")]
    public GameObject slamHitbox;

    [Header("페이즈2 설정 (체력 50% 이하)")]
    public int phase2MaxCombo = 3;
    public float phase2PauseDuration = 3f;
    private int phase2ComboCount = 0;

    private BossState currentState = BossState.Idle;
    private float stateTimer = 0f;

    private Vector2 jumpStartPos;
    private Vector2 jumpLandPos;
    private float flightDuration;
    private float jumpTimer;
    private bool waitingStartup;

    [Header("바닥 체크 (레이캐스트)")]
    public Transform groundCheck;
    public float rayDistance = 0.3f;
    public LayerMask groundLayer;
    private bool isGrounded = true;
    private float groundY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();

        bossHealth = maxBossHealth;

        if (slamHitbox != null) slamHitbox.SetActive(false);
    }

    void Update()
    {
        // 죽었거나 경직(Stagger) 상태일 때는 패턴 진행을 멈춥니다.
        if (isDead || isStagger) return;

        CheckGround();

        Transform detectedPlayer = DetectPlayerByTag();
        bool playerDetected = detectedPlayer != null;

        if (playerDetected)
        {
            player = detectedPlayer;
        }

        bool playerInArea = player != null && IsPositionInArea(player.position);

        if (currentState == BossState.Detect && (!playerDetected || !playerInArea))
        {
            ResetToIdle();
        }

        if (currentState == BossState.Idle && playerDetected && playerInArea)
        {
            currentState = BossState.Detect;
        }

        switch (currentState)
        {
            case BossState.Idle: break;
            case BossState.Detect: Detect(); break;
            case BossState.Jumping: Jumping(); break;
            case BossState.Landing: break;
            case BossState.Cooldown: Cooldown(); break;
            case BossState.PhaseTwoPause: PhaseTwoPause(); break;
        }
    }

    private Transform DetectPlayerByTag()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag(playerTag)) return hit.transform;
        }
        return null;
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayDistance, groundLayer);
        isGrounded = hit.collider != null;
        if (isGrounded) groundY = hit.point.y;
    }

    private bool IsPhaseTwo() => bossHealth <= maxBossHealth * 0.5f;

    private bool IsPositionInArea(Vector2 pos)
    {
        if (bossArea == null) return true;
        return pos.x >= bossArea.bounds.min.x && pos.x <= bossArea.bounds.max.x;
    }

    private Vector2 ClampToArea(Vector2 pos)
    {
        if (bossArea == null) return pos;
        Bounds b = bossArea.bounds;
        float clampedX = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        return new Vector2(clampedX, pos.y);
    }

    private void ResetToIdle()
    {
        StopAllCoroutines();
        phase2ComboCount = 0;
        stateTimer = 0f;
        jumpTimer = 0f;
        waitingStartup = false;
        if (slamHitbox != null) slamHitbox.SetActive(false);
        currentState = BossState.Idle;
    }

    void Detect()
    {
        Vector2 rawTarget = new Vector2(player.position.x, transform.position.y);
        jumpLandPos = ClampToArea(rawTarget);
        jumpStartPos = rb.position;

        if (anim != null) anim.SetTrigger("JumpAttack");

        flightDuration = jumpDuration;
        if (flightDuration < 0.1f) flightDuration = 0.1f;

        jumpTimer = 0f;
        waitingStartup = true;
        stateTimer = clipStartUp != null ? clipStartUp.length : 0.2f;

        currentState = BossState.Jumping;
    }

    void Jumping()
    {
        if (waitingStartup)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f) waitingStartup = false;
            return;
        }

        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / flightDuration);

        float x = Mathf.Lerp(jumpStartPos.x, jumpLandPos.x, t);
        float arc = jumpHeight * 4f * t * (1f - t);
        float y = Mathf.Lerp(jumpStartPos.y, jumpLandPos.y, t) + arc;

        Vector2 newPos = ClampToArea(new Vector2(x, y));
        rb.MovePosition(newPos);

        if (t >= 1f)
        {
            Vector2 snappedPos = new Vector2(rb.position.x, jumpLandPos.y);
            rb.MovePosition(snappedPos);

            currentState = BossState.Landing;
            StartCoroutine(OnSlamLanding());
        }
    }

    private IEnumerator OnSlamLanding()
    {
        float landDuration = clipLand != null ? clipLand.length : 0.3f;

        if (slamHitbox != null) slamHitbox.SetActive(true);
        yield return new WaitForSeconds(landDuration * 0.3f);

        if (slamHitbox != null) slamHitbox.SetActive(false);
        yield return new WaitForSeconds(landDuration * 0.7f);

        if (isDead) yield break;

        if (IsPhaseTwo())
        {
            phase2ComboCount++;
            if (phase2ComboCount >= phase2MaxCombo)
            {
                phase2ComboCount = 0;
                stateTimer = phase2PauseDuration;
                currentState = BossState.PhaseTwoPause;
            }
            else
            {
                currentState = BossState.Detect;
            }
        }
        else
        {
            stateTimer = cooldownTime;
            currentState = BossState.Cooldown;
        }
    }

    void Cooldown()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) currentState = BossState.Idle;
    }

    void PhaseTwoPause()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) currentState = BossState.Idle;
    }

    // ==========================================
    // 데미지 처리 및 사망 (EnemyHealth 로직 적용)
    // ==========================================
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        int prevPercentInt = Mathf.CeilToInt((bossHealth / maxBossHealth) * 100f);

        bossHealth -= damage;
        Debug.Log("보스 데미지 입음! 현재 체력: " + bossHealth);

        if (bossHealth <= 0f)
        {
            bossHealth = 0f;
            Die();
            return;
        }

        // 특정 체력 구간 진입 시 피격 애니메이션 재생 로직 (유지)
        int newPercentInt = Mathf.CeilToInt((bossHealth / maxBossHealth) * 100f);
        if (newPercentInt / 10 < prevPercentInt / 10)
        {
            if (anim != null) anim.SetTrigger("Hurt");
        }

    }

    private IEnumerator StaggerCoroutine()
    {
        isStagger = true;
        yield return new WaitForSeconds(staggerDuration);
        isStagger = false;
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("보스 사망");

        StopAllCoroutines(); // 진행 중인 점프나 공격 코루틴 중지

        // EnemyHealth처럼 태그를 변경하여 플레이어의 추가 타겟팅 방지
        gameObject.tag = "Untagged";

        // 공격 판정 및 충돌체 끄기
        if (slamHitbox != null) slamHitbox.SetActive(false);
        if (col != null) col.enabled = false;

        // 사망 애니메이션 재생
        if (anim != null) anim.SetTrigger("Death");

        // 사망 애니메이션 길이를 가져와서 해당 시간 뒤에 오브젝트 파괴 (Destroy)
        float deathAnimationTime = clipDeath != null ? clipDeath.length : 1.5f;
        Destroy(gameObject, deathAnimationTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        if (bossArea != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bossArea.bounds.center, bossArea.bounds.size);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * rayDistance);
        }
    }
}