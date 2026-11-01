using UnityEngine;

public class PullPlayer : MonoBehaviour
{
    public float launchForce = 15f;
    public float cooldown = 5f;

    private bool canUse = true;
    private Rigidbody2D playerRb;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void Update()
    {
        if (playerRb != null && canUse && Input.GetKeyDown(KeyCode.E))
        {
            // 플레이어에서 이 오브젝트 방향으로
            Vector2 direction = ((Vector2)transform.position - playerRb.position).normalized;

            // 기존 속도 제거
            playerRb.linearVelocity = Vector2.zero;

            // 점프대처럼 발사
            playerRb.AddForce(direction * launchForce, ForceMode2D.Impulse);

            canUse = false;
            Invoke(nameof(ResetCooldown), cooldown);
        }
    }

    private void ResetCooldown()
    {
        canUse = true;
    }
}