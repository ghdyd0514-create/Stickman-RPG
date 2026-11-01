using UnityEngine;
using System.Collections.Generic;
using System;

public enum PlayerClass
{
    Warrior,    //  전사
    Archer,     //  궁수
    Magician    //  법사
}

public class Player : MonoBehaviour
{
    public static Player instance;

    [Header("현재 직업")]
    public PlayerClass currentClass = PlayerClass.Warrior;    //  일단 프로토타입으로 전사만.

    //  직업 상관 없이 다들 가지는 조건
    public PlayerMove playerMove;
    public PlayerCombat playerCombat;
    public PlayerHealth playerHealth;
    public PlayerHitbox playerHitbox;
    public PlayerHitDetector playerHitDetector;
    public PlayerInteraction playerInteraction;
    public PlayerAugmentManager playerAugmentManager;
    public PlayerInventory playerInventory;
    public PlayerSkillController playerSkillController;

    [SerializeField] Transform aimPoint;

    //  전사가 가지는 특성
    public WarriorDash warriorDash;
    public WarriorParry warriorParry;
    public WarriorDashAttackHitbox warriorDashAttackHitbox;
    public WarriorSwordWave warriorSwordWave;

    //  궁수가 가지는 특성

    //  법사가 가지는 특성

    void Awake()
    {
        instance = this;
        playerMove = GetComponent<PlayerMove>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        playerHitbox = GetComponentInChildren<PlayerHitbox>();
        playerHitDetector = GetComponentInChildren<PlayerHitDetector>();
        playerInteraction = GetComponent<PlayerInteraction>();
        playerAugmentManager = GetComponent<PlayerAugmentManager>();
        playerInventory = GetComponent<PlayerInventory>();
        playerSkillController = GetComponent<PlayerSkillController>();

        if (currentClass == PlayerClass.Warrior)
        {
            warriorDash = GetComponent<WarriorDash>();
            warriorParry = GetComponent<WarriorParry>();
            warriorDashAttackHitbox = GetComponent<WarriorDashAttackHitbox>();
            warriorSwordWave = GetComponent<WarriorSwordWave>();
        }
    }

    //  플레이어가 획득했던 증강들에 다시 접근하게 해서, 씬이 바뀌어도 증강이 유지될 수 있도록 한다.
    void Start()
    {
        List<AugmentsData> obtainedAugments = Managers.instance.augmentManager.GetAcquiredAugments();

        for (int i = 0; i < obtainedAugments.Count; i++)
        {
            playerAugmentManager.ApplyAugment(obtainedAugments[i]);
        }
    }

    public Transform GetAimPoint()
    {
        return aimPoint;
    }
}
