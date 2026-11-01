using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class Potion : MonoBehaviour
{
    [SerializeField] CircleCollider2D collider;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            collider.gameObject.SetActive(true);
        }
    }
}