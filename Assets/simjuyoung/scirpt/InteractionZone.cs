using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public GameObject interactionUI;

    private bool playerInside;

    void Start()
    {
        interactionUI.SetActive(false);
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("interactive!");
            // 여기서 대화, 상점, 문 열기 등 실행
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            interactionUI.SetActive(false);
        }
    }
}