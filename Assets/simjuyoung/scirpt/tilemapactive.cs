using UnityEngine;

public class tilemapactive : MonoBehaviour
{
    public GameObject tilemap;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌한 오브젝트: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 감지!");
            tilemap.SetActive(false);
        }
    }
}