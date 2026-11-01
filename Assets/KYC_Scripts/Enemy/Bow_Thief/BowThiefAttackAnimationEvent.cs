using NUnit.Framework.Constraints;
using UnityEngine;

public class BowThiefAttackAnimationEvent : MonoBehaviour
{
    BowThiefAttack bowThiefAttack;
    Animator anim;

    [Header("화살")]
    [SerializeField] GameObject arrowPrefab;

    [Header("화살이 나가는 좌표")]
    [SerializeField] Transform arrowSpawnTransform;

    [Header("공격 효과음")]
    [SerializeField] AudioClip attackSFX;
    [SerializeField] float volume;

    void Awake()
    {
        anim = GetComponent<Animator>();
        bowThiefAttack = GetComponentInParent<BowThiefAttack>();
    }

    public void ShootAnArrow()
    {
        anim.SetBool("BowThiefAttackEnd", true);
        Instantiate(arrowPrefab, arrowSpawnTransform.position, Quaternion.identity);
        Managers.instance.audioManager.PlaySFX(attackSFX, volume);


        bowThiefAttack.EndAttack();
    }
}