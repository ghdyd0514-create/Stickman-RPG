using UnityEngine;

//  이 스크립트의 기능 : 선택한 증강에 대하여 플레이어한테 반영함
public class PlayerAugmentManager : MonoBehaviour
{
    PlayerMove playerMove;
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;

    WarriorDash warriorDash;
    WarriorParry warriorParry;
    WarriorSwordWave warriorSwordWave;
    UpperSlash upperSlash;
    DownAttack downAttack;
    DownSmash downSmash;
    ChargeAttack chargeAttack;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();

        warriorSwordWave = GetComponent<WarriorSwordWave>();
        warriorDash = GetComponent<WarriorDash>();
        warriorParry = GetComponent<WarriorParry>();
        upperSlash = GetComponent<UpperSlash>();
        downAttack = GetComponent<DownAttack>();
        downSmash = GetComponent<DownSmash>();
        chargeAttack = GetComponent<ChargeAttack>();
    }

    public void ApplyAugment(AugmentsData augmentsDataParameter)
    {
        switch (augmentsDataParameter.effectType)
        {
            case AugmentEffectType.LifeSteal:
                playerCombat.OnHPSteal((int)augmentsDataParameter.value); break;

            //  체력이 일정수치 밑으로 떨어지면 공격력 증가
            case AugmentEffectType.LowHpAttack1:
                playerCombat.SetOnLowHpAttack((int)augmentsDataParameter.value); break;

            //  체력이 일정수치 밑으로 떨어지면 공격력 증가
            case AugmentEffectType.LowHpAttack2:
                playerCombat.SetOnLowHpAttack((int)augmentsDataParameter.value); break;

            //  가시갑옷
            case AugmentEffectType.SpikeArmor:
                playerHealth.SetSpikeArmor(); break;

            //  피격 후 무적시간 소폭 증가
            case AugmentEffectType.UpgradeInvincibilityTime:
                playerHealth.SetInvincibilityTime(augmentsDataParameter.value); break;

            //  대쉬------------------------------
            //  대쉬에 데미지 생겨요
            case AugmentEffectType.DashAttack:
                warriorDash.OnDashAttack(); break;

            //  대쉬어택 헬스
            case AugmentEffectType.DashAttackHealth :
                warriorDash.SetHealAfterDashAttackKill();     break;


            //  대쉬어택에 넉백 추가
            case AugmentEffectType.DashAttackKnockBack:
                warriorDash.SetDashAttackKnockBack(); break;

            //  올려치기
            case AugmentEffectType.UpperSlash :
                upperSlash.SetAugmentObtain(); break;

            //  아래공격
            case AugmentEffectType.DownAttack :
                downAttack.SetAugmentObtain(); break;

            //  내려찍기
            case AugmentEffectType.DownStrike :
                downSmash.SetAugmentObtain();   break;

            //  차지공격
            case AugmentEffectType.ChargeAttack :
                chargeAttack.SetAugmentObtain();    break;







            default:
                Debug.Log("왜 여기까지 왔냐"); break;
        }
    }
}