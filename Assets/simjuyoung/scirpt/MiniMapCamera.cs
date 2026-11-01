using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform player;
    public float height = -10f;

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            height
        );
    }
}