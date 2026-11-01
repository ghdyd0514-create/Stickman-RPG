using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class movefloor : MonoBehaviour
{
    public Rigidbody2D rigid;

    [Header("움직임")]
    public float speed = 2f;
    public float start = 1f;
    public float end = -1f;

    float direction = 1f;
    float minX;
    float maxX;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();

        // start와 end 중 더 작은 값을 minX, 큰 값을 maxX로 자동 정리
        if (start < end)
        {
            minX = start;
            maxX = end;
        }
        else
        {
            minX = end;
            maxX = start;
        }
    }

    void FixedUpdate()
    {
        rigid.linearVelocity = new Vector2(speed * direction, 0f);

        if (direction > 0 && transform.position.x >= maxX)
        {
            direction = -1f;
        }

        if (direction < 0 && transform.position.x <= minX)
        {
            direction = 1f;
        }
    }
}
