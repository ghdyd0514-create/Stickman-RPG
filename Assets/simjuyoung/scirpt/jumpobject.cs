using UnityEngine;

public class jumpobject : MonoBehaviour
{
    public float jumpForce = 15f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 속도 초기화 (선택)
                rb.linearVelocity = Vector2.zero;

                // 오브젝트가 바라보는 방향으로 힘 주기
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}