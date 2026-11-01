using UnityEngine;

public class PlayerDetectorfly : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D playercollider;

    public bool FindPlayerDetectedfly = false;

    public Transform DetctefPlayer;

    public LayerMask playerLayer;

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player")
        {
            FindPlayerDetectedfly = true;
            DetctefPlayer = other.transform;

            Debug.Log("감지 감지 감지");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            FindPlayerDetectedfly = false;
            DetctefPlayer = null;

            Debug.Log("플레이어 놓침");
        }
    }

}