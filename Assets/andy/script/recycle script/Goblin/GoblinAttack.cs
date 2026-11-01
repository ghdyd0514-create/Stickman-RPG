using UnityEngine;

public class Goblin : MonoBehaviour, All_in_One_Attack
{
    [SerializeField] private PlayerDetector detector; // Inspector에서 직접 연결
    private Rigidbody2D rigid;
    private EnemyMove enemyMove;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        enemyMove = GetComponent<EnemyMove>();
    }

    public void Attack()
    {
        Vector2 toPlayer = (detector.DetectPlayer.position - transform.position).normalized;
        rigid.linearVelocity = new Vector2(toPlayer.x * enemyMove.chaseSpeed, rigid.linearVelocity.y);

        if (toPlayer.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (toPlayer.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}