using UnityEngine;

public enum AugmentGrade
{
    C,
    B,
    A,
    S
}

public enum AugmentTargetClass
{
    All,
    Warrior,
    Archer,
    Magician
}

public enum AugmentEffectType
{
    UpgradeInvincibilityTime,    //  피격 후 무적시간 증가                                    1
    LowHpAttack1,         //  체력이 일정 수치 미만일 때, 공격력 15퍼센트 증가                   2
    LowHpAttack2,          //   체력이 일정 수치 미만일 때, 공격력 30퍼센트 증가 ㅁicon필요      3
    LifeSteal,          //  피흡                                                           4
    SpikeArmor,         //  가시갑옷 (플레이어가 피격 받을 시 소량의 데미지를 줌.)                5
    DashAttack,          //  대쉬에 데미지 주기. (단 본인도 데미지 조금 입음)                      6
    DashAttackHealth,   //  대쉬 어택으로 적 죽이면 hp 회복 (대쉬 어택으로 인한 자해 데미지 보단 적게해야할듯)     7
    DashAttackKnockBack,    //  대쉬 어택 맞은 적. 날라간다 ㅁ icon필요                              8

    UpperSlash,             //  올려치기                                                      9
    DownStrike,             //  아래찍기                                                    10
    ChargeAttack,           //  차징공격                                                    11
    DownAttack              //  아래공격                                                    12
}

[CreateAssetMenu(
    fileName = "NewAugment",
    menuName = "Game/Augment Data"
)]

//  해당 스크립트의 기능 : 증강에 넣을 내용물들을 모은 스크립트이다.
public class AugmentsData : ScriptableObject
{
    [Header("증강 ID")]
    public int ID;

    [Header("증강 기본 정보")]
    public Sprite augmentIcon;              //  해당 아이콘은 증강 아이템을 획득하고 카드에서 증강의 시각적인 면을 보여주는 아이콘
    public string augmentName;              //  증강 이름 (ex. 날카로운 검)
    public string augmentDescription;       //  증강 설명 (ex. 공격력을 5만큼 올립니다.)

    [Header("증강 등급")]
    public AugmentGrade augmentGrade;

    [Header("사용가능한 증강 클래스")]
    public AugmentTargetClass augmentTargetClass;

    [Header("증강 종류")]
    public AugmentEffectType effectType;

    [Header("수치")]
    public float value;

    [Header("해당 증강의 최대 출현 횟수")]
    [Min(1)]
    public int maximumAppear = 1;

    [Header("이 증강이 등장하기 위해 필요한 선행증강")]
    public AugmentsData requiredAugment;
}