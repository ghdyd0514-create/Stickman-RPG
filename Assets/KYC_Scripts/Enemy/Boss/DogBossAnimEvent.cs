using UnityEngine;

public class DogBossAnimEvent : MonoBehaviour
{
    DogBossAttack dogBossAttack;

    [Header("근접 공격 콜라이더")]
    [SerializeField] Collider2D nearAttackCollider;

    [Header("근접 공격 이펙트")]
    [SerializeField] GameObject nearAttackEffect;

    [SerializeField] AudioClip nearAttackSFX;
    [SerializeField] AudioClip dashAttackSFX;
    [SerializeField] float volume;

    void Awake()
    {
        nearAttackCollider.gameObject.SetActive(false);
        nearAttackEffect.gameObject.SetActive(false);

        dogBossAttack = GetComponentInParent<DogBossAttack>();
    }

    public void OnNearAttackCollider()
    {
        Managers.instance.audioManager.PlaySFX(nearAttackSFX, volume);
        nearAttackEffect.gameObject.SetActive(true);
        nearAttackCollider.gameObject.SetActive(true);
    }

    public void OnDashAttackCollider()
    {
        Managers.instance.audioManager.PlaySFX(dashAttackSFX, volume);
        dogBossAttack.OnDashAttackCollider();
    }

    public void EndNearAttack()
    {
        dogBossAttack.EndNearAttack();
        nearAttackEffect.gameObject.SetActive(false);
        nearAttackCollider.gameObject.SetActive(false);
    }

    public void OnSummonMonster()
    {
        dogBossAttack.OnSummonMonster();
    }
}