using UnityEngine;

public class AttackAnimEvent : MonoBehaviour
{
    PlayerCombat combat;
    WarriorSwordWave warriorSwordWave;

    void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
        warriorSwordWave = GetComponentInParent<WarriorSwordWave>();
    }

    public void EnableHitBox()
    {
        combat.EnableHitBox();
    }

    public void DisableHitBox()
    {
        combat.DisableHitBox();
    }

    public void OnSwordWave()
    {
        warriorSwordWave.SwordWaveWhileAttack();
    }
}
