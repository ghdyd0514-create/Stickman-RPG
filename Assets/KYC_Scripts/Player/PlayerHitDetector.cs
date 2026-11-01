using UnityEngine;
using System;

public class PlayerHitDetector : MonoBehaviour
{
    PlayerHealth playerHealth;
    WarriorParry warriorParry;

    [Header("전사직업군 한정. 패링 성공 시, 무적시간")]
    [SerializeField] float afterParry_InvincibilityTime = 0.5f;

    [Header("패리성공 시, 무적시간을 시각적으로 보여줌.")]
    [SerializeField] GameObject skillCooldownUI;

    public event Action playerActionCancel;  //  피격 받으면 플레이어의 진행중인 행동을 캔슬한다(행동에 따라 다름. 어떤 건 취소 안 함)

    void Awake()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
        warriorParry = GetComponentInParent<WarriorParry>();
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (playerHealth.currentHP <= 0)
        {
            return;
        }

        //  낭떠러지나 특정 장애물에 닿으면 즉사하게
        if (collider.CompareTag("DeadZone"))
        {
            playerHealth.SelfDamage(999999, collider);
            return;
        }

        if (playerHealth.invincibility)
        {
            return;
        }
        
        // 플레이어의 피격 조건 1. 태그가 Enemy 혹은 Boss여야 하고 2. 플레이어의 피가 0보다 커야하며 3. 현재 무적이 아니여야 한다.
        switch (Player.instance.currentClass)
        {
            //  단, 전사 직업군의 경우 일반적인 피격과 패링을 구분하여야 한다.
            case PlayerClass.Warrior:

                //  패링의 경우 조건 1. Enemy태그 or Obstacle태그 2. 플레이어 hp가 0보다 커야함 3. 현재 무적이 아니여야 함 4. 패링중이여야 함.
                if ((collider.CompareTag("Enemy") || collider.CompareTag("Obstacle")) && warriorParry != null && warriorParry.onParring)
                {
                    bool parrySuccess = warriorParry.Parry(collider);

                    //  패리 실패하면 데미지 입음
                    if (!parrySuccess)
                    {
                        playerHealth.OnDamaged(collider, collider.GetComponentInParent<Damage>().damage);
                    }
                    //  패리 성공하면 플레이어한테 무적시간 0.5초 부여
                    else
                    {
                        StartCoroutine(playerHealth.InvincibilityTime(afterParry_InvincibilityTime));
                        StartCoroutine(skillCooldownUI.GetComponent<SkillCooldownUI>().AppearActiveSkillTime(afterParry_InvincibilityTime));
                    }
                }
                //  패링 안쓰면 기존과 동일
                else if ((collider.CompareTag("Enemy") || collider.CompareTag("Obstacle")) && Player.instance.playerHealth.currentHP > 0 && !Player.instance.playerHealth.invincibility)
                {
                    playerHealth.OnDamaged(collider, collider.GetComponentInParent<Damage>().damage);
                }
                break;


            case PlayerClass.Archer:

                if (collider.CompareTag("Enemy") && Player.instance.playerHealth.currentHP > 0 && !Player.instance.playerHealth.invincibility)
                {
                    playerHealth.OnDamaged(collider, collider.GetComponent<Damage>().damage);
                }
                break;


            case PlayerClass.Magician:

                if (collider.CompareTag("Enemy") && Player.instance.playerHealth.currentHP > 0 && !Player.instance.playerHealth.invincibility)
                {
                    playerHealth.OnDamaged(collider, collider.GetComponent<Damage>().damage);
                }
                break;
        }

    }
}
