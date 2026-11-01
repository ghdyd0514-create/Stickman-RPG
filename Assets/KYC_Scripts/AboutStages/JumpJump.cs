using UnityEngine;

public class JumpJump : MonoBehaviour
{
    [Header("상승 수치")]
    [SerializeField] float jumpPower = 10.0f;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            collider.GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
    }
}