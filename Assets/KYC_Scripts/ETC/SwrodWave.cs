using UnityEngine;

public class SwordWave : MonoBehaviour
{
    Rigidbody2D rigid;

    [SerializeField] float swordWaveSpeed = 1200f;
    int swordWaveAttack;

    bool alreadyHit = false;
    bool SwordWavePenetration = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (transform.localScale.x > 0)
        {
            rigid.linearVelocity = new Vector2(swordWaveSpeed * Time.fixedDeltaTime, rigid.linearVelocity.y);
        }
        else
        {
            rigid.linearVelocity = new Vector2(-swordWaveSpeed * Time.fixedDeltaTime, rigid.linearVelocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyHitbox")) 
        {
            //  적은 한 명만 맞게 한다.
            if (alreadyHit && !SwordWavePenetration) return;

            alreadyHit = true;
            EnemyHurtbox enemyHitbox = collision.GetComponent<EnemyHurtbox>();

            enemyHitbox.TakeDamage(swordWaveAttack);

            if (alreadyHit && !SwordWavePenetration) Destroy(gameObject);
        }
    }

    public void SetSwordWavePenetration()
    {
        SwordWavePenetration = true;
    }

    public void SetSwordWaveAttackPower(int value)
    {
        swordWaveAttack += value;
    }
}