/*using UnityEngine;

public class RandomEpic : MonoBehaviour
{
    EnemyMove enemyMove;
    FlyingMove flyingMove;
    EnemyAttack monsterAttack;

    [Header("희귀 개체")]

    public static bool rareExists = false;
    public bool isRare = false;
    public static int strongCount = 0; // 현재 강한 개체 마리 수 (모든 몬스터 공유)
    public int maxStrongCount = 3;      // 최대 몇 마리까지 허용할지 (Inspector에서 조절 가능)
    public bool isStrong = false;       // 이 몬스터가 강한 개체인지 표시

    [Header("확률 설정 (%)")]
    [Range(0, 100)]
    public float rareChance = 5f;      // 레어 개체 확률 (%)
    [Range(0, 100)]
    public float strongChance = 25f;   // 강한 개체 확률 (%) - rareChance 이후 구간

    private void Start()
    {
        RandomEpic.rareExists = false;
        RandomEpic.strongCount = 0;
    }

    void Awake()
    {
        enemyMove = GetComponent<EnemyMove>();
        flyingMove = GetComponent<FlyingMove>();
        monsterAttack = GetComponent<EnemyAttack>();
        float roll = Random.Range(0f, 100f);

        // 이름 뒤에 (1), (2) 같은 게 붙어도 원래 이름만 뽑아내기
        string cleanName = gameObject.name;
        int parenIndex = cleanName.IndexOf('(');
        if (parenIndex != -1)
        {
            cleanName = cleanName.Substring(0, parenIndex).Trim();
        }

        {
            if (roll < rareChance && !rareExists)
            {
                rareExists = true;
                isRare = true;

                switch (cleanName)
                {
                    case "bat":
                        Debug.Log("박쥐가 매우 사납게 날개짓한다!");
                        break;
                    case "Goblin":
                        Debug.Log("고블린정찰대 대장이 나타났다!");
                        break;
                    case "rat":
                        Debug.Log("쥐들의 대장이 나타났다!");
                        break;
                    case "slime":
                        Debug.Log("슬라임들이 더욱 많이 뭉치고있다!");
                        break;
                    case "Skeleton":
                        Debug.Log("스켈레톤이 뼈를 더욱 많이 뭉치고있다!");
                        break;
                    case "Golem":
                        Debug.Log("숲의 수호자가 숲의 힘을 크게 받았다");
                        break;
                    case "GoldGolem":
                        Debug.Log("깊은숲의 황금의 수호자가 숲의 힘을 크게 받았다");
                        break;
                    default:
                        Debug.Log("epic 개체!");
                        break;
                }

                if (enemyMove != null)
                {
                    enemyMove.speed = 5f;
                    transform.localScale *= 2f;
                    enemyMove.groundCheckDistance += 1.5f;
                    enemyMove.wallCheckDistance += 1.5f;
                    enemyMove.chaseSpeed = 5.5f;
                    EnemyHealth targetHealth = GetComponentInChildren<EnemyHealth>();
                    if (targetHealth != null)
                    {
                        targetHealth.hp += 30f;
                    }
                }
                else if (flyingMove != null)
                {
                    flyingMove.speed = 5f;
                    transform.localScale *= 2f;
                    flyingMove.wallCheckDistance *= 2f;
                    flyingMove.groundCheckDistance *= 2f;
                    EnemyHealth targetHealth = GetComponentInChildren<EnemyHealth>();
                    if (targetHealth != null)
                    {
                        targetHealth.hp += 30f;
                    }
                }

                // 몸집이 2배 커진 만큼 공격범위도 2배로 늘려줌
                if (monsterAttack != null)
                {
                    monsterAttack.attackRange *= 2f;
                }
            }

            else if (roll < rareChance + strongChance && strongCount < maxStrongCount)
            {
                strongCount++; // 마리 수 1 증가
                isStrong = true; // 나 자신 표시

                switch (cleanName)
                {
                    case "bat":
                        Debug.Log("박쥐가 사납게 날개짓한다!");
                        break;
                    case "Goblin":
                        Debug.Log("고블린정찰대 부대장이 나타났다!");
                        break;
                    case "rat":
                        Debug.Log("쥐들의 부대장이 나타났다!");
                        break;
                    case "slime":
                        Debug.Log("슬라임들이 뭉치고있다!");
                        break;
                    case "Skeleton":
                        Debug.Log("스켈레톤이 뼈를 뭉치고있다!");
                        break;
                    case "Golem":
                        Debug.Log("숲의 수호자가 숲의 힘을 받았다");
                        break;
                    case "GoldGolem":
                        Debug.Log("깊은숲의 황금의 수호자가 숲의 힘을 받았다");
                        break;
                    default:
                        Debug.Log("rare 개체!");
                        break;
                }

                if (enemyMove != null)
                {
                    enemyMove.speed = 3f;
                    transform.localScale *= 1.5f;
                    enemyMove.groundCheckDistance += 1f;
                    enemyMove.wallCheckDistance += 1f;
                    enemyMove.chaseSpeed = 3.5f;
                    EnemyHealth targetHealth = GetComponentInChildren<EnemyHealth>();
                    if (targetHealth != null)
                    {
                        targetHealth.hp += 20f;
                    }
                }
                else if (flyingMove != null)
                {
                    flyingMove.speed = 3f;
                    transform.localScale *= 1.5f;
                    flyingMove.wallCheckDistance *= 2f;
                    flyingMove.groundCheckDistance *= 2f;
                    EnemyHealth targetHealth = GetComponentInChildren<EnemyHealth>();
                    if (targetHealth != null)
                    {
                        targetHealth.hp += 20f;
                    }
                }

                // 몸집이 1.5배 커진 만큼 공격범위도 1.5배로 늘려줌
                if (monsterAttack != null)
                {
                    monsterAttack.attackRange *= 1.5f;
                }
            }
        }
    }
}*/