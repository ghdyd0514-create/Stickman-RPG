using NUnit.Framework.Constraints;
using UnityEngine;

public class BatAttackAnimationEvent : MonoBehaviour
{
    BatAttack batAttack;
    Animator anim;

    [Header("초음파")]
    [SerializeField] GameObject ultraSoundPrefab;

    [Header("초음파가 나가는 좌표")]
    [SerializeField] Transform ultraSoundTransform;

    [Header("공격 효과음")]
    [SerializeField] AudioClip attackSFX;
    [SerializeField] float volume;

    void Awake()
    {
        anim = GetComponent<Animator>();
        batAttack = GetComponentInParent<BatAttack>();
    }

    public void ShootAnUltraSound()
    {
        anim.SetBool("BatAttackEnd", true);
        Instantiate(ultraSoundPrefab, ultraSoundTransform.position, Quaternion.identity);
        Managers.instance.audioManager.PlaySFX(attackSFX, volume);

        batAttack.EndAttack();
    }
}