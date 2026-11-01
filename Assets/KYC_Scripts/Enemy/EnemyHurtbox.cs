using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    EnemyHealth enemyHealth;
    [SerializeField] Transform hitEffectPoint;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public Transform GetHitEffectPoint()
    {
        return hitEffectPoint;
    }

    public void TakeDamage(float damage)
    {
        enemyHealth.TakeDamage(damage);
    }

    //  낭떠러지나, 혹은 장애물에 닿으면 죽게 한다 (몬스터와 플레이어 포함)
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("DeadZone"))
        {
            enemyHealth.Die();
        }
    }
}
