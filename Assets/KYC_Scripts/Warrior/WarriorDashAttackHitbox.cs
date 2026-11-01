using System.Collections;
using UnityEngine;

public class WarriorDashAttackHitbox : MonoBehaviour
{
    WarriorDash warriorDash;
    PlayerHealth playerHealth;
    PlayerHitbox playerHitbox;
    PlayerCombat playerCombat;
    Rigidbody2D rigid;  //  리지드바디 키네마틱 사용

    [SerializeField] Collider2D playerHurtboxCollider;
    [SerializeField] float dashAttackMultiplier = 3.0f;
    [SerializeField] int selfDamage = 2;
    [SerializeField] float dashAttackKnockBackPower = 1;
    [SerializeField] float mySelfKnockBack = 1.0f, mySelfKnockBackY = 1.0f;
    [SerializeField] float knockBackTime = 0.2f;
    [SerializeField] float verticalKnockBackPower = 0.4f;

    [Header("대쉬 어택 이후 잠깐 무적시간을 줘서 보정하기")]
    [SerializeField] float invincibilityTime = 1.0f;

    [Header("돌진 공격 hit 이펙트")]
    [SerializeField] GameObject hitEffect;

    bool alreadyHit = false;

    [Header("증강 획득 유무에 따른 bool변수")]
    public bool healAfterDashAttackKill = false;
    public bool canDashAttackKnockBack = false;


    [Header("흔들림 정도")]
    [SerializeField] float shakePower = 0.5f;

    [Header("대쉬어택 효과음")]
    [SerializeField] AudioClip dashAttackSFX;
    [SerializeField] float volume = 1.0f;

    //  대쉬 어택으로 회복할 hp의 양
    [SerializeField] int healAmountAfterDashAttackKill = 5;

    void Awake()
    {
        warriorDash = GetComponentInParent<WarriorDash>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        playerHitbox = GetComponentInParent<PlayerHitbox>();
        playerCombat = GetComponentInParent<PlayerCombat>();
        rigid = GetComponentInParent<Rigidbody2D>();
    }

    public void StartDashAttack()
    {
        alreadyHit = false;
        playerHurtboxCollider.enabled = false;
    }

    public void EndDashAttack()
    {
        playerHurtboxCollider.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("EnemyHitbox"))
        {
            //  이미 충돌 한 번 했으면 더 이상 못하게
            if (alreadyHit)
            {
                return;
            }

            alreadyHit = true;


            //  찰나의 무적시간 넣기
            playerHealth.StartCoroutine(playerHealth.InvincibilityTime(invincibilityTime));

            int damage = Mathf.RoundToInt(playerCombat.attackPower * dashAttackMultiplier);
            playerHitbox.EnemyHit(collider.GetComponent<EnemyHurtbox>(), damage, shakePower, hitEffect, dashAttackSFX, volume);

            //  대쉬어택 넉백 증강으로 인해 변수가 활성화 되었다면 적을 날려버린다
            if (canDashAttackKnockBack)
            {
                StartCoroutine(DashAttackKnockBack(collider));
            }

            //  적이 죽엇다면
            if (collider.GetComponentInParent<EnemyHealth>().isDead && healAfterDashAttackKill)
            {
                GetComponentInParent<PlayerHealth>().HPRecovery(healAmountAfterDashAttackKill);
            }

            playerHealth.SelfDamage(selfDamage, collider);

            //  대쉬어택 캔슬
            warriorDash.CancelDash();

            SelfKnockBack(collider);
        }
    }

    void SelfKnockBack(Collider2D collider)
    {
        Rigidbody2D enemyRigid = collider.GetComponentInParent<Rigidbody2D>();

        rigid.linearVelocity = Vector2.zero;

        // 적이 플레이어 오른쪽에 있으면 플레이어는 왼쪽으로
        float directionX = enemyRigid.position.x >= rigid.position.x ? -1f : 1f;

        rigid.AddForce(
            Vector2.right * directionX * mySelfKnockBack,
            ForceMode2D.Impulse);

        rigid.AddForce(
            Vector2.up * mySelfKnockBackY,
            ForceMode2D.Impulse);
    }

    IEnumerator DashAttackKnockBack(Collider2D collider)
    {
        EnemyMove enemyMove = collider.GetComponentInParent<EnemyMove>();
        Rigidbody2D enemyRigid = collider.GetComponentInParent<Rigidbody2D>();

        enemyMove.isKnockBack = true;
        enemyRigid.linearVelocity = Vector2.zero;

        float directionX =
            enemyRigid.position.x >= rigid.position.x ? 1f : -1f;

        enemyRigid.AddForce(
            Vector2.right * directionX * dashAttackKnockBackPower,
            ForceMode2D.Impulse);

        enemyRigid.AddForce(
            Vector2.up * verticalKnockBackPower,
            ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);

        enemyMove.isKnockBack = false;
    }

    public void SetHealAfterDashAttackKill()
    {
        healAfterDashAttackKill = true;
    }


    public void SetCanDashAttackKnockBack()
    {
        canDashAttackKnockBack = true;
    }
}