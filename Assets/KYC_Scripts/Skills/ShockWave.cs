using UnityEngine;

public class ShockWave : MonoBehaviour
{
    //PlayerHitbox playerHitbox;

    [SerializeField] float shockWaveMultiplier = 0.5f;  //  기본 공격력의 50퍼센트
    [SerializeField] float shockWaveDestroyTime = 1.0f;
    [SerializeField] AudioClip shockWaveHitSFX;
    [SerializeField] float volume = 1.0f;


    void Start()
    {
        //playerHitbox = GetComponentInParent<PlayerHitbox>();
        Destroy(gameObject, shockWaveDestroyTime);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("EnemyHitbox"))
        {
            int damage = (int)(Player.instance.playerCombat.attackPower * shockWaveMultiplier);

            Player.instance.playerHitbox.EnemyHit(collider.GetComponent<EnemyHurtbox>(), damage, 0, null, shockWaveHitSFX, volume);
        }
    }
}