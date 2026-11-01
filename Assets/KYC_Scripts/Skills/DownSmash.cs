using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

//  내려찍기는
//  1. 공중에서만 사용 가능 (일정 높이 이상에서만 사용가능하게 해야 하나, 고민중)
//  2. 사용하면 플레이어는 잠시 경직되고(내려찍기 준비자세 취함) . 한 0.2초?
//  3. 밑으로 칼을 내려찍음.
//  4. 내려찍을 때는, 바닥에 닿기 전까지 모션이 안 끝나며, 내려가는 동안 대쉬, 기본공격, 다른 스킬, 패리 불가능
//  5. 내려가는 동안 x축 움직임 없음.
//  6. 거리에 따라 지면 충격파 적용
public class DownSmash : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;
    PlayerMove playerMove;
    DownAttack downAttack;
    ChargeAttack chargeAttack;
    PlayerHitbox playerHitbox;
    PlayerSkillController skillController;

    [Header("증강 얻엇니")]
    [SerializeField] bool areYouObtainAugment = false;


    [Header("내려찍기 속도")]
    [SerializeField] float FallingSpeed = 1.0f;

    [Header("내려찍기 시전 시간. (공중에서 경직되는 시간)")]
    [SerializeField] float delayTime = 0.2f;

    [Header("내려찍기 가능 여부")]
    public bool canDownSmash = false;

    [Header("내려찍기 콜라이더")]
    [SerializeField] Collider2D downSmashCollider;

    [Header("내려찍기 공격력 배율")]
    [SerializeField] float downSmashMultiplier = 2.0f;

    [Header("ray")]
    [SerializeField] Collider2D rayStartPos;

    [Header("카메라 쉐이크 정도")]
    [SerializeField] CameraShake cameraShake;   
    [SerializeField] float groundShakePower = 0.6f;
    [SerializeField] float enemyShakePower = 0.2f;

    [Header("충격파")]
    [SerializeField] GameObject shockWavePrefab;

    [SerializeField] float shockWave1Distance = 3f;
    [SerializeField] float shockWave2Distance = 7f;

    [SerializeField] Transform shockWavePoint;

    [Header("효과음")]
    [SerializeField] AudioClip downSmashSFX;
    [SerializeField] float volume;
    
    HashSet<EnemyHurtbox> hitEnemies = new HashSet<EnemyHurtbox>();

    float originGravityScale;

    void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        playerHitbox = GetComponent<PlayerHitbox>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        downAttack = GetComponent<DownAttack>();
        chargeAttack = GetComponent<ChargeAttack>();
        downSmashCollider.gameObject.SetActive(false);

        originGravityScale = rigid.gravityScale;
    }

//  -------------------------------------------------------------
//  아래찍기 코루틴
//  ---------------------------------------------------------
    IEnumerator StartDownSmash()
    {
        downAttack.CancelDownAttack();
        chargeAttack.CancelCharge();
        StartAttack();

        //  땅과의 거리 확인
        float rayDistance = CalculateRayDistance();

        //  딜레이
        yield return StartCoroutine(Delay());

        //  딜레이 끝나고 그때서야 콜라이더 on
        downSmashCollider.gameObject.SetActive(true);

        bool hitGround = false;

        //  이제 내려감
        while (!hitGround)
        {
            rigid.linearVelocity = new Vector2(0, Vector2.down.y * FallingSpeed);

            //  -----------
            //  땅 감지 hit
            //  ------------
            Collider2D groundHit = Physics2D.OverlapBox(downSmashCollider.bounds.center,
                                                    downSmashCollider.bounds.size, 0, LayerMask.GetMask("Ground"));

            hitGround = groundHit;

            //  --------------
            //  몬스터 감지 hit
            //  --------------
            CheckEnemyHit();

            yield return null;
        }

        //  땅에 도착
        if (hitGround)
        {
            rigid.linearVelocity = Vector2.zero;

            //  카메라 쉐이크
            cameraShake.Shake(groundShakePower);

            //  거리에 따른 충격파 발생
            OccurShockWave(rayDistance);

            //  효과음
            Managers.instance.audioManager.PlaySFX(downSmashSFX, volume);
        }

        EndAttack();
    }

//  -------------------------------------------------------------
//  내려찍기 실행 시, ray를 발사해, 거리 계산
//  ---------------------------------------------------------
    float CalculateRayDistance()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(rayStartPos.bounds.center.x, rayStartPos.bounds.min.y),
                                            Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));

        //  근데 만약 플레이어가 낭떠러지에서 내려찍기하면 hit.distance는 무한대가 나올텐데, 이러면 어떡하지. 버그 생기나?
        //  어차피 낭떠러지에 떨어지며 뒤지는데 상관없나
        float distance = hit.distance;
        return distance;
    }

//  -------------------------------------------------------------
//  내려찍기 시전 자세
//  ---------------------------------------------------------
    IEnumerator Delay()
    {
        //  속도 초기화
        rigid.linearVelocity = Vector2.zero;

        //  애니메이션 설정 (준비동작)
        anim.SetTrigger("DownSmashReady");

        float timer = 0;
        while (timer < delayTime)
        {
            timer += Time.deltaTime;
            rigid.linearVelocity = Vector2.zero;
            yield return null;
        }

        //  애니메이션 설정 (실제 행동)
        anim.SetBool("DownSmashOn", true);
    }
//  -------------------------------------------------------------
//  내려찍기 높이에 따른 충격파 설정
//  ---------------------------------------------------------
    void OccurShockWave(float fallDistance)
    {
        // 높은 높이
        if (fallDistance >= shockWave2Distance)
        {
            GameObject shockWaveObj = Instantiate(shockWavePrefab, shockWavePoint.position, Quaternion.identity);
            shockWaveObj.transform.localScale = new Vector3(1.5f, 1, 1);    //  살짝 더 길다.
        }

        // 중간 높이
        else if (fallDistance >= shockWave1Distance)
        {
            Instantiate(
                shockWavePrefab,
                shockWavePoint.position,
                Quaternion.identity
            );
        }

        // 낮은 높이
        else
        {
            // 충격파 없음
        }
    }

//  -------------------------------------------------------------
//  데미지 주기
//  ---------------------------------------------------------
    void CheckEnemyHit()
    {
        if (downSmashCollider != null && downSmashCollider.gameObject.activeSelf)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(downSmashCollider.bounds.center,
                                            downSmashCollider.bounds.size, 0, LayerMask.GetMask("EnemyHurtBox"));

            foreach (Collider2D hit in hits)
            {
                EnemyHurtbox enemyHurtbox = hit.GetComponentInChildren<EnemyHurtbox>();

                if (enemyHurtbox == null)
                    continue;

                // 이번 공격으로 이미 맞은 적이면 패스
                if (hitEnemies.Contains(enemyHurtbox))
                    continue;

                hitEnemies.Add(enemyHurtbox);

                int damage = (int)(playerCombat.attackPower * downSmashMultiplier);

                playerHitbox.EnemyHit(enemyHurtbox, damage, enemyShakePower, null, null, 0);
            }
        }
    }

    //--------------------------------------------------
    //  잡다한 것들
    //--------------------------------------------------

    //  e를 눌러서 작동
    public void OnDownSmash(InputAction.CallbackContext context)
    {
        if (context.performed && areYouObtainAugment &&
            canDownSmash && playerMove.isAir && !playerMove.isGround && skillController.CanUseSkill())
        {
            StartCoroutine(StartDownSmash());
        }
    }

    public void StartAttack()
    {
        //  조건 끄기
        canDownSmash = false;
        playerHealth.isSuperArmor = true;
        playerCombat.canAttack = false;
        playerMove.canMove = false;
        skillController.LockSkill();
        hitEnemies.Clear();

        //  애니메이션 초기화
        anim.SetBool("DownSmashEnd", false);
        anim.SetBool("DownSmashOn", false);

        rigid.gravityScale = 0;
    }

    void EndAttack()
    {
        //  조건 복구
        canDownSmash = true;
        playerCombat.canAttack = true;
        playerMove.canMove = true;
        playerHealth.isSuperArmor = false;
        skillController.UnlockSkill();

        //  콜라이더 off
        downSmashCollider.gameObject.SetActive(false);

        //  중력값 복구
        rigid.gravityScale = originGravityScale;


        //  애니메이션 복구
        anim.SetBool("DownSmashEnd", true);     //  DownSmash에서 Idle로 이동하는 트랜직션
    }

    public void SetAugmentObtain()
    {
        areYouObtainAugment = true;
        canDownSmash = true;
    }
}