using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    [Header("이 구역의 적 부모")]
    [SerializeField] private Transform enemyParent;

    private BoxCollider2D zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // 시작 시 모든 적 비활성화
        SetEnemiesActive(false);

        // 플레이어가 이미 이 구역 안에서 시작한 경우
        Collider2D player = Physics2D.OverlapBox(
            (Vector2)transform.position + zoneCollider.offset,
            zoneCollider.size,
            transform.eulerAngles.z,
            LayerMask.GetMask("Player")
        );

        if (player != null)
        {
            SetEnemiesActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SetEnemiesActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SetEnemiesActive(false);
    }

    private void SetEnemiesActive(bool active)
    {
        if (enemyParent == null)
            return;

        foreach (Transform enemy in enemyParent)
        {
            enemy.gameObject.SetActive(active);
        }
    }

#if UNITY_EDITOR
    // Scene에서 범위 확인용
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = Color.green;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.offset, box.size);
        Gizmos.matrix = old;
    }
#endif
}