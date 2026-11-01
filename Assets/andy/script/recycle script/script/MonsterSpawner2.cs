using UnityEngine;

public class MonsterSpawner2 : MonoBehaviour
{
    [Header("소환 설정")]
    public GameObject monsterPrefab;   // 소환할 몬스터 프리팹
    public float spawnInterval = 3f;   // 몇 초마다 소환할지
    public bool spawnOnStart = true;   // 시작하자마자 소환할지 여부

    [Header("소환 위치 (비워두면 스포너 자신의 위치)")]
    public Transform[] spawnPoints;    // 여러 스폰 지점 (선택 사항)

    private float timer;

    void Start()
    {
        timer = spawnOnStart ? spawnInterval : 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMonster();
            timer = 0f;
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("몬스터 프리팹이 연결되지 않았습니다!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
    }

    Vector3 GetSpawnPosition()
    {
        // 스폰 포인트가 설정되어 있으면 그 중 랜덤으로 선택
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].position;
        }

        // 없으면 스포너 자신의 위치에서 소환
        return transform.position;
    }
}