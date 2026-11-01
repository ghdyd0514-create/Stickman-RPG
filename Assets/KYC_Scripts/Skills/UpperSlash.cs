using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpperSlash : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerMove playerMove;
    PlayerHealth playerHealth;
    DownAttack downAttack;
    ChargeAttack chargeAttack;
    PlayerHitbox playerHitbox;
    PlayerSkillController skillController;

    [Header("증강 얻엇니")]
    [SerializeField] bool areYouObtainAugment = false;


    [Header("올려치기 공격력 배율")]
    [SerializeField] float upperSlashMultiplier = 1.0f;

    [Header("올려치기 속도")]
    [SerializeField] float upperSlashSpeed = 1.0f;

    [Header("올려치기 할 때 약간 x축으로 이동함. 그 힘의 정도")]
    [SerializeField] float upperSlashMoveX = 0.3f;

    [Header("올려치기 시전 시간")]
    [SerializeField] float upperSlashTime = 1.0f;

    [Header("올려치기 준비 자세")]
    [SerializeField] float upperSlashReadyTime = 1.0f;

    [Header("올려치기 중인가요?")]
    [SerializeField] bool isUpperSlashing = false;

    [Header("올려치기 사용 했나? (한번 사용하면 착지 전까지 다시 사용 불가)")]
    public bool canUpperSlash = false;

    [Header("올려치기 콜라이더")]
    [SerializeField] Collider2D upperSlashCollider;

    [Header("올려치기 이펙트")]
    [SerializeField] GameObject upperSlashEffect;

    [Header("카메라 수ㅗㅔ이크")]
    [SerializeField] float cameraShake;

    [Header("올려치기 넉백")]
    [SerializeField] float knockBackX;
    [SerializeField] float knockBackY;
    [SerializeField] float knockBackTime;

    [Header("올려치기 시전 동작 효과음")]
    [SerializeField] AudioClip upperSlashActingSFX;
    [SerializeField] float volume = 1.0f;

    HashSet<EnemyHurtbox> hitEnemies = new HashSet<EnemyHurtbox>();

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
        upperSlashEffect.gameObject.SetActive(false);
        upperSlashCollider.gameObject.SetActive(false);
    }

    //  q를 눌러서 작동
    public void OnUpperSlash(InputAction.CallbackContext context)
    {
        if (context.performed && areYouObtainAugment && !isUpperSlashing && canUpperSlash && skillController.CanUseSkill())
        {
            StartCoroutine(StartUpperSlash());
        }
    }

    //  -------------------------------------
    //  올려치기 작동
    //  --------------------------------------
    IEnumerator StartUpperSlash()
    {
        chargeAttack.CancelCharge();
        downAttack.CancelDownAttack();

        //  애니메이션 이벤트로 StartUpperAttack()할 예정
        StartUpperAttack();

        float timer = 0;

        //  올려치기 시전 자세
        while (timer < upperSlashReadyTime)
        {
            //  시전 자세 도중 피격 받는다면 취소
            if (playerHealth.isDamaged)
            {
                anim.ResetTrigger("UpperAttackReady");
                EndAttack();
                StartCoroutine(UpperSlashRe_Use());
                yield break;
            }

            timer += Time.deltaTime;
            rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

            yield return null;
        }

        timer = 0;
        anim.SetBool("UpperAttackOn", true);
        Managers.instance.audioManager.PlaySFX(upperSlashActingSFX, volume);

        //  올려치기
        while (timer < upperSlashTime)
        {
            timer += Time.deltaTime;
            rigid.linearVelocity = new Vector2(playerMove.GetPlayerDirection() * upperSlashMoveX, upperSlashSpeed);

            CheckEnemyHit();

            yield return null;
        }

        EndAttack();


        StartCoroutine(UpperSlashRe_Use());
    }

    //  ----------------------------------------------
    //  올려치기 재사용 조건
    //  --------------------------------------------
    IEnumerator UpperSlashRe_Use()
    {
        //  올려치기 후 플레이어가 다시 땅을 밟기 전까지 반복문 돌게 해줌
        while (!playerMove.isGround)
        {
            yield return null;
        }

        //  땅에 다시 닿으면 착지 재사용 가능
        canUpperSlash = true;
    }

    //  ----------------------------------------------
    //  적 피격 조건 설정
    //  --------------------------------------------    
    void CheckEnemyHit()
    {
        if (upperSlashCollider != null && upperSlashCollider.gameObject.activeSelf)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(upperSlashCollider.bounds.center,
                                            upperSlashCollider.bounds.size, 0, LayerMask.GetMask("EnemyHurtBox"));

            foreach (Collider2D hit in hits)
            {
                EnemyHurtbox enemyHurtbox = hit.GetComponentInChildren<EnemyHurtbox>();

                if (enemyHurtbox == null)
                    continue;

                // 이번 공격으로 이미 맞은 적이면 패스
                if (hitEnemies.Contains(enemyHurtbox))
                    continue;

                hitEnemies.Add(enemyHurtbox);

                int damage = (int)(playerCombat.attackPower * upperSlashMultiplier);

                playerHitbox.EnemyHit(enemyHurtbox, damage, cameraShake, null, null, 0);

                StartCoroutine(UpperSlashKnockBack(hit));
            }
        }
    }

        //  --------------------------------------------------
    //  적 날려버리는 함수
    //  -----------------------------------------------
    IEnumerator UpperSlashKnockBack(Collider2D collider)
    {
        EnemyMove enemyMove = collider.GetComponentInParent<EnemyMove>();
        Rigidbody2D enemyRigid = collider.GetComponentInParent<Rigidbody2D>();

        enemyMove.isKnockBack = true;
        enemyRigid.linearVelocity = Vector2.zero;

        // 플레이어 기준으로 적이 어느 방향에 있는지
        float directionX = enemyRigid.position.x >= rigid.position.x ? 1f : -1f;

        //  x축 넉백
        enemyRigid.AddForce(Vector2.right * directionX * knockBackX, ForceMode2D.Impulse);

        //  y축 띄우기
        enemyRigid.AddForce(Vector2.up * knockBackY, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);

        enemyMove.isKnockBack = false;
    }

    //  ----------------------------------------------
    //  여러 조건 제한들 및 잡다한 넘들
    //  --------------------------------------------
    public void SetAugmentObtain()
    {
        areYouObtainAugment = true;
        canUpperSlash = true;
    }

    void StartUpperAttack()
    {
        //  조건 끄기
        canUpperSlash = false;
        isUpperSlashing = true;
        hitEnemies.Clear();
        skillController.LockSkill();

        playerCombat.canAttack = false;
        playerMove.canMove = false;
        anim.SetBool("UpperAttackOn", false);
        anim.SetBool("UpperAttackEnd", false);

        //  애니메이션 재생
        anim.SetTrigger("UpperAttackReady");

        //  속도 초기화
        rigid.linearVelocity = Vector2.zero;
    }

    //  콜라이더 우선 on
    public void OnEnableCollider()
    {
        if (!isUpperSlashing)
            return;
        //  콜라이더 온
        upperSlashCollider.gameObject.SetActive(true);
    }

    //  그 뒤에 이펙트 on
    public void OnEnableEffect()
    {
        if (!isUpperSlashing)
            return;
        //  이펙트 온
        upperSlashEffect.gameObject.SetActive(true);
    }

    public void EndAttack()
    {
        isUpperSlashing = false;
        rigid.linearVelocity = Vector2.zero;

        upperSlashCollider.gameObject.SetActive(false);
        upperSlashEffect.gameObject.SetActive(false);

        playerCombat.canAttack = true;
        playerMove.canMove = true;

        upperSlashEffect.gameObject.SetActive(false);
        skillController.UnlockSkill();

        anim.SetBool("UpperAttackEnd", true);
    }
}