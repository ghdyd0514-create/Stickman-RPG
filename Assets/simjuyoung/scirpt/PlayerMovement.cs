using UnityEngine;


public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 8f;
    public float jumpForce = 12f;


    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;

    private float horizontalInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();   // 자동으로 가져옴
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 바닥 체크
        isGrounded = col.IsTouchingLayers(groundLayer);

        // 점프
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetButtonDown("Jump")) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        if (horizontalInput > 0)
            transform.localScale = Vector3.one;
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}