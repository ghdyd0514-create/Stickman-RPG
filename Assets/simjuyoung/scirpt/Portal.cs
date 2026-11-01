using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Portal : MonoBehaviour
{
    public Transform destination;
    public GameObject portarUI;

    // 추가
    public Collider2D destinationBounds;

    private static bool canTeleport = true;

    private bool playerInside = false;
    private Collider2D player;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
           Debug.Log("E 입력");
        }

        if (playerInside && canTeleport && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("텔레포트");
            StartCoroutine(Teleport());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("들어옴");

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("플레이어 들어옴");

        playerInside = true;
        player = other;

        if (portarUI != null)
            portarUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        player = null;

        if (portarUI != null)
            portarUI.SetActive(false);
    }

    IEnumerator Teleport()
    {
        canTeleport = false;

        if (portarUI != null)
            portarUI.SetActive(false);

        // 플레이어 이동
        player.transform.position = destination.position;

        // Confiner 변경
       //CinemachineConfiner2D confiner =
          //FindFirstObjectByType<CinemachineConfiner2D>();

        //Debug.Log(confiner);

        //confiner.BoundingShape2D = destinationBounds;
        //confiner.InvalidateBoundingShapeCache();

        yield return new WaitForSeconds(0.2f);

        canTeleport = true;
    }
}