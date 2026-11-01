using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;
    CameraShake cameraShake;

    [Header("피해흡혈 회복량")]
    [SerializeField] int HPStealAmount = 1;

    [Header("타격 이펙트")]
    [SerializeField] GameObject hitEffect;

    [Header("기본 타격 사운드")]
    [SerializeField] AudioClip basicHitSFX;
    [SerializeField] float basicVolume = 1.0f;

    void Awake()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        cameraShake = GetComponent<CameraShake>();
    }

    public void EnemyHit(EnemyHurtbox enemyHurtbox, int damage, float cameraShakePower, GameObject hurtEffect, AudioClip audioClip, float volumeParameter)
    {
        enemyHurtbox.TakeDamage(damage);

        //  적 피격 이펙트 전용이 따로 없다면 기본 피격 이펙트를
        if (hurtEffect == null)
        {
            Instantiate(hitEffect, enemyHurtbox.GetHitEffectPoint().position, Quaternion.identity);
        }
        //  그게 아니면 전용 이펙트를
        else
        {
            Instantiate(hurtEffect, enemyHurtbox.GetHitEffectPoint().position, Quaternion.identity);
        }  

        if (audioClip == null)
        {
            Managers.instance.audioManager.PlaySFX(basicHitSFX, basicVolume);
        }
        else
        {
            Managers.instance.audioManager.PlaySFX(audioClip, volumeParameter);
        }

        cameraShake.Shake(cameraShakePower);

        //  피흡 증강이 있다면 적을 타격 했을 때 일정확률로 5%확률(기정)로 피흡시킨다.
        if (playerCombat.ReturnHPSteal())
        {
            int probability = Random.Range(1, 101);

            if (probability <= playerCombat.hpStealProbability)
            {
                Debug.Log("체력 흡혈 전 / 현재 체력 : " + playerHealth.currentHP);
                playerHealth.HPRecovery(HPStealAmount);
                Debug.Log("체력 흡혈 : " + playerHealth.currentHP);
            }
        }

    }
}