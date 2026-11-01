using System;
using System.Collections;
using UnityEngine;

//  기능 : 몬스터 체력 관리 및 데미지 처리
public class EnemyHealth : MonoBehaviour
{
    [Header("죽음 이펙트")]
    [SerializeField] GameObject dieEffectPrefab;

    [Header("경직 시간")]
    [SerializeField] float staggerDuration = 0.25f;

    [Header("몬스터 허트박스 콜라이더")]
    [SerializeField] Collider2D hurtboxCollider;

    [Header("죽을 때 떨어뜨릴 돈")]
    [SerializeField] GameObject coinPrefab;

    [Header("코인 날라가는 정도")]
    [SerializeField] float coinThrowPower = 5f;

    [Header("비주얼 자식 Transform")]
    [SerializeField] Transform visualChildren;
    [SerializeField] Transform dieEffectPoint;

    [Header("몬스터가 죽을 때 떨어뜨릴 돈의 양")]
    [SerializeField] int minMoney, maxMoney;

    EnemyHurtbox enemyHurtbox;
    PlayerDetector playerDetector;
    EnemyMove enemyMove;
    EnemyAttack monsterAttack;
    SpriteRenderer spriteRenderer;
    EnemyHpBar enemyHpBar;
    BossHpBar bossHpBar;
    Boss boss;
    public float hp = 10f;
    public float maxHp = 10f;
    public float deathDelay = 1.5f;
    public bool isDead = false;
    public bool isStagger = false;  //  경직

    //  몬스터가 죽으면 현재 room의 몬스터가 죽었다고 알려주기 위한 이벤트
    //public event Action enemyDie;
    void Awake()
    {
        enemyMove = GetComponent<EnemyMove>();
        enemyHurtbox = GetComponentInChildren<EnemyHurtbox>();
        playerDetector = GetComponent<PlayerDetector>();
        monsterAttack = GetComponent<EnemyAttack>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyHpBar = GetComponentInChildren<EnemyHpBar>(true);
        bossHpBar = GetComponent<BossHpBar>();
        boss = GetComponent<Boss>();

        hp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        hp = hp - damage;

        if (hp <= 0)
        {
            hp = 0;
            Debug.Log("데미지 입어서 죽음");
            Die();
            return;
        }

        //  체력바 ui 전달
        if (!boss)
            enemyHpBar.UpdateHP(hp, maxHp);
        else
            bossHpBar.UpdateHP(hp, maxHp);

        DogAttack dogAttack = GetComponent<DogAttack>();

        //  경직    //  하드코딩좀할게
        if (dogAttack == null || !dogAttack.isAttacking)
        {
            StartCoroutine(StaggerCoroutine());
        }
    }

    IEnumerator StaggerCoroutine()
    {
        isStagger = true;

        yield return new WaitForSeconds(staggerDuration);

        isStagger = false;
    }

    public void Die()
    {
        isDead = true;
        //enemyDie?.Invoke();
        Boss boss = GetComponent<Boss>();
        if (boss != null)
        {
            boss.Die();
        }
        gameObject.tag = "Untagged";
        hurtboxCollider.enabled = false;
        spriteRenderer.enabled = false;
        Instantiate(dieEffectPrefab, dieEffectPoint.position, Quaternion.identity);
        if (enemyHurtbox != null) enemyHurtbox.enabled = false;
        if (enemyMove != null) enemyMove.enabled = false;
        if (playerDetector != null) playerDetector.enabled = false;
        if (monsterAttack != null) monsterAttack.enabled = false;

        DropCoin();

        Destroy(transform.gameObject);
    }

    void DropCoin()
    {
        GameObject coinObj = Instantiate(coinPrefab, dieEffectPoint.position, Quaternion.identity);
        CoinCs coinCs = coinObj.GetComponentInChildren<CoinCs>();

        coinCs.SetMoney(minMoney, maxMoney);

        /*GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        Vector2 coinThrowDir;

        //  몬스터의 반대 방향으로 돈이 날라가게 함.
        if (visualChildren.localScale.x > 0)
        {
            coinThrowDir = new Vector2(8, 6); // x축으로 4, y축으로 6정도의 비율을 줘서 돈이 날아가게끔.
        }
        else
        {
            coinThrowDir = new Vector2(8, 6); // x축으로 4, y축으로 6정도의 비율을 줘서 돈이 날아가게끔.
        }

        coin.GetComponent<Rigidbody2D>().AddForce(coinThrowDir.normalized * coinThrowPower, ForceMode2D.Impulse);*/
    }
}