using UnityEngine;

public class WarriorSwordWave : MonoBehaviour
{   
    [Header("비주얼 자식의 트랜스폼")]
    [SerializeField] Transform visualTransform;

    [Header("검기 오브젝트")]
    [SerializeField] GameObject swordWave;

    [Header("검기 오브젝트 생성 위치")]
    [SerializeField] Transform swordWaveInstantiationPosition;

    [Header("검기가 나갈 확률")]
    [Min(1)]
    public int swordWaveChance = 15;

    [Header("검기 사이즈")]
    [SerializeField] float swordWaveSize = 1.0f;

    [Header("생성 후 사라지는 시간")]
    [SerializeField] float offTime = 2.0f;

    //  증강 상태에 따른 변수 여부
    [SerializeField] bool onSwordWave = false;
    bool onSwordWavePenetration = false;

    [Header("검기 공격력")]
    [SerializeField] int swordWaveAttackPower = 3;

    //  공격 애니메이션 프레임에 넣을거임 ㅇㅇ.
    public void SwordWaveWhileAttack()
    {
        if (onSwordWave && swordWaveChance >= Random.Range(1, 101))
        {
            GameObject swordWavePrefab = Instantiate(swordWave, swordWaveInstantiationPosition.position, swordWaveInstantiationPosition.rotation);

            //  검기 공격력 설정
            swordWavePrefab.GetComponent<SwordWave>().SetSwordWaveAttackPower(swordWaveAttackPower);

            if (onSwordWavePenetration)
            {
                //  검기 관통 설정
                swordWavePrefab.GetComponent<SwordWave>().SetSwordWavePenetration();
            }

            //  검기 방향 설정
            swordWavePrefab.transform.localScale = visualTransform.transform.localScale.x < 0 ? new Vector3(-1, swordWaveSize, 1) : new Vector3(1, swordWaveSize, 1);

            Destroy(swordWavePrefab, offTime);
        }
    }



    public void OnSwordWave()
    {
        onSwordWave = true;
    }
    public bool ReturnSwordWave()
    {
        return onSwordWave;
    }

    //  검기 확률 올리기
    public void SetSwordWaveChance(int value)
    {
        swordWaveChance += value;
    }

    //  검기 사이즈 키우기
    public void SetSwordWaveSize(float value)
    {
        swordWaveSize += value;
    }

    //  검기 관통하게 하기
    public void SwordWavePenetration()
    {
        onSwordWavePenetration = true;
    }

    //  검기 공격력 증가
    public void SetSwordWaveAttackPower(int value)
    {
        swordWaveAttackPower += value;
    }
}