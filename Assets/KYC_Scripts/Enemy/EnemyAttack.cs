using System.Collections;
using UnityEngine;

//  기능 : 개별 몬스터가 가지는 공통 된 공격 스크립트 내용.
//  몬스터마다의 공격력, 공격 쿨타임, 현재 공격중인지 여부 등등을 관리한다.
public abstract class EnemyAttack : MonoBehaviour
{
    //  공격 데미지는 damage스크립트를 사용해야한다.
    //  공격 판정을 담당하는 콜라이더 객체한테 damage스크립트를 주자.
    [Header("공격 쿨타임")]
    [SerializeField] protected float attackCooldown;

    [Header("공격이 가능한가")]
    [SerializeField] protected bool canAttack = true;

    [Header("현재 공격중인가")]
    [SerializeField] public bool isAttacking;

    [Header("공격 콜라이더(공격 범위)")]
    [SerializeField] protected Collider2D attackCollider;

    public abstract void Attack();

    protected virtual void Awake()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    protected IEnumerator CoolDown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    // 몬스터가 지금 뭘 하고 있는 상태인지 표시하는 것
    /*enum State
    {
        Move,      // 그냥 이동/추격 중
        Attack,    // 공격 애니메이션 재생 중 (스윙)
        Stay       // 공격 끝나고 쿨다운 대기 중 (멈춰서 대기)
    }

    [Header("공격 범위")]
    public float attackRange = 1.2f;      // 이 거리 안이면 멈추고 공격
    public float attackCooldown = 1.5f;   // 공격 시작부터 다음 이동까지 걸리는 총 시간

    [Header("타이밍")]
    public float attackAnimDuration = 0.5f; // 공격 애니메이션 총 길이 (Stay로 넘어갈 타이밍 계산용)

    [Header("참조")]
    public PlayerDetector playerDetector;   // 자식의 감지 스크립트
    public GameObject attackHitboxObject;   // 자식의 공격판정 콜라이더 오브젝트
    public MonoBehaviour movementScript;    // EnemyMove, FlyingMove 등

    private Animator animator;
    private Rigidbody2D rigid;

    private State currentState = State.Move; // 지금 상태 (처음엔 이동 상태)
    private float timer = 0f;                // 지금 상태에서 시간이 얼마나 지났는지

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();

        if (playerDetector == null)
            playerDetector = GetComponentInChildren<PlayerDetector>();

        if (attackHitboxObject != null)
            attackHitboxObject.SetActive(false); // 평소엔 꺼둠
    }

    void Update()
    {
        // 지금 상태가 뭔지에 따라 다른 함수 실행
        switch (currentState)
        {
            case State.Move:
                CheckAttackRange();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Stay:
                UpdateStay();
                break;
        }
    }

    // ===== Move 상태: 공격범위 안에 들어왔는지 매 프레임 확인 =====
    void CheckAttackRange()
    {
        if (playerDetector == null) return;
        if (!playerDetector.FindPlayerDetected) return;
        if (playerDetector.DetectPlayer == null) return;

        float dist = Vector2.Distance(transform.position, playerDetector.DetectPlayer.position);

        if (dist <= attackRange)
        {
            StartAttack();
        }
    }

    // 공격 시작할 때 한 번만 실행되는 부분
    void StartAttack()
    {
        currentState = State.Attack; // 상태를 Attack으로 변경
        timer = 0f;                  // 타이머 초기화

        if (movementScript != null)
            movementScript.enabled = false; // 이동 멈추기

        if (rigid != null)
            rigid.linearVelocity = Vector2.zero; // 속도도 0으로

        if (animator != null)
            animator.SetBool("isAttack", true); // 공격 애니메이션 재생
    }

    // ===== Attack 상태: 애니메이션 길이만큼 기다렸다가 Stay로 전환 =====
    // 히트박스 On/Off는 Animation Event가 EnableHitbox()/DisableHitbox()를 직접 호출해줌
    void UpdateAttack()
    {
        timer += Time.deltaTime; // 시간 계속 누적

        if (timer >= attackAnimDuration)
        {
            currentState = State.Stay; // 상태를 Stay로 변경
            timer = 0f;                // 타이머 다시 초기화

            if (animator != null)
                animator.SetBool("isAttack", false);
        }
    }

    // ===== Stay 상태: 쿨다운 끝날 때까지 대기 =====
    void UpdateStay()
    {
        timer += Time.deltaTime;

        // 공격 상태에서 이미 attackAnimDuration 만큼 썼으니, 남은 시간만 더 기다리면 됨
        float remaining = attackCooldown - attackAnimDuration;

        if (timer >= remaining)
        {
            // 쿨다운 끝 -> isStay 애니메이션 실행 + 다시 이동 상태로
            if (animator != null)
                animator.SetTrigger("isStay");

            currentState = State.Move;
            timer = 0f;

            if (movementScript != null)
                movementScript.enabled = true; // 이동 다시 켜기
        }
    }

    // 히트박스 켜기 (Animation Event로 직접 호출해도 됨)
    public void EnableHitbox()
    {
        if (attackHitboxObject == null) return;
        attackHitboxObject.SetActive(true);

        // 플레이어가 멈춰있어서 Rigidbody2D가 잠들어있으면 강제로 깨우기
        if (playerDetector != null && playerDetector.DetectPlayer != null)
        {
            Rigidbody2D playerRigid = playerDetector.DetectPlayer.GetComponent<Rigidbody2D>();
            if (playerRigid != null)
                playerRigid.WakeUp();
        }
    }

    // 히트박스 끄기
    public void DisableHitbox()
    {
        if (attackHitboxObject != null)
            attackHitboxObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }*/
}