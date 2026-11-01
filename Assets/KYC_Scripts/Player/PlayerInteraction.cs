using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//  플레이어와 상호작용 스크립트
public class PlayerInteraction : MonoBehaviour
{
    PlayerHealth playerHealth;
    PlayerInventory playerInventory;

    [Header("현재 상호작용 가능한 아이템이랑 붙어있는가")]
    [SerializeField] bool onAttachedItem = false;

    [Header("현재 npc와 붙어있는가")]
    [SerializeField] bool onAttachedNPC = false;

    [Header("현재 플레이어랑 붙어있는 상호작용 가능한 오브젝트의 콜라이더")]
    [SerializeField] Collider2D interactableObjectCollider = null;

    [Header("현재 포탈 위에 플레이어가 있는가")]
    [SerializeField] bool playerOnPortal = false;

    [Header("효과음")]
    [SerializeField] AudioClip getCoinSFX;
    [SerializeField] float getCoinVolume = 1.0f;

    [SerializeField] AudioClip interactNpcSFX;
    [SerializeField] float interactVolume = 1.0f;

    RoomPortal currentPortal;
    NPCText_Parent nearbyNPC;
    NPCText_Parent currentNPC;

    void Awake()
    {

        playerHealth = GetComponentInParent<PlayerHealth>();
        playerInventory = GetComponentInParent<PlayerInventory>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("InteractionItem") && !playerHealth.isDead)
        {
            onAttachedItem = true;
            interactableObjectCollider = collider;
        }

        else if (collider.CompareTag("Portal") && !playerHealth.isDead)
        {
            playerOnPortal = true;
            currentPortal = collider.GetComponent<RoomPortal>();
        }

        else if (collider.CompareTag("NPC") && !playerHealth.isDead)
        {
            nearbyNPC = collider.GetComponent<NPCText_Parent>();

            if (nearbyNPC == null)
                nearbyNPC = collider.GetComponentInParent<NPCText_Parent>();

            onAttachedNPC = nearbyNPC != null;
            //interactableObjectCollider = collider;
        }

        else if (collider.CompareTag("Potion") && !playerHealth.isDead)
        {
            if (playerInventory.potionAmount < playerInventory.potionMaxCount && playerInventory.potionAmount >= 0)
            {
                //  한 개 먹으니까 매개변수 1.
                playerInventory.GetPotionAmount(1);
                Managers.instance.audioManager.PlaySFX(getCoinSFX, getCoinVolume);
                collider.transform.parent.gameObject.SetActive(false);
            }
            else { return; }
        }

        else if (collider.CompareTag("Coin") && !playerHealth.isDead)
        {
            int cash = collider.GetComponentInParent<CoinCs>().GetMoney();
            Managers.instance.audioManager.PlaySFX(getCoinSFX, getCoinVolume);
            playerInventory.SetMoney(cash);
            collider.transform.parent.gameObject.SetActive(false);
        }
    }




    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("InteractionItem"))
        {
            onAttachedItem = false;
            interactableObjectCollider = null;
        }

        else if (collider.CompareTag("Portal"))
        {
            playerOnPortal = false;
            currentPortal = null;
        }

        else if (collider.CompareTag("NPC"))
        {
            NPCText_Parent exitingNPC = collider.GetComponent<NPCText_Parent>();

            if (exitingNPC == null)
                exitingNPC = collider.GetComponentInParent<NPCText_Parent>();

            // '근처 NPC'만 해제한다.
            if (nearbyNPC == exitingNPC)
            {
                nearbyNPC = null;
                onAttachedNPC = false;
            }

            //interactableObjectCollider = null;
        }
    }





    public void OnInteraction(InputAction.CallbackContext context)
    {
        //  사용 가능 조건) 1. 키를 눌렀을 때, 2. 상호작용 가능한 오브젝트와 가까이 있을 때 3. 플레이어가 사망하지 않았을 때.
        if (context.performed && onAttachedItem && !playerHealth.isDead)
        {
            interactableObjectCollider.GetComponent<Parent_InteractableObject>().Interact(GetComponentInParent<Player>());
        }

        else if (context.performed && onAttachedNPC && !playerHealth.isDead)
        {
            if (currentNPC != null)
            {
                Managers.instance.audioManager.PlaySFX(interactNpcSFX, interactVolume);

                // 이미 대화 중이면 현재 NPC의 다음 대사
                currentNPC.Interact();
                return;
            }

            if (nearbyNPC != null)
            {
                // 처음 대화 시작
                currentNPC = nearbyNPC;
                Managers.instance.audioManager.PlaySFX(interactNpcSFX, interactVolume);

                // 관성 제거
                Rigidbody2D rigid = GetComponentInParent<Rigidbody2D>();

                rigid.linearVelocity = Vector2.zero;

                currentNPC.Interact();
            }

            //interactableObjectCollider.GetComponent<NPCText_Parent>().Interact();
        }
    }





    public void OnChooseChoice(InputAction.CallbackContext context)
    {
        if (currentNPC == null)
            return;

        currentNPC.ChooseChoice(context);
    }

    public void OnDecideChoice(InputAction.CallbackContext context)
    {
        if (currentNPC == null)
            return;

        currentNPC.DecideChoice(context);
    }

    public void ClearCurrentNPC(NPCText_Parent npc)
    {
        if (currentNPC == npc)
            currentNPC = null;
    }





    public void OnUsePortal(InputAction.CallbackContext context)
    {
        if (context.performed && playerOnPortal && currentPortal != null && !playerHealth.isDead)
        {
            Debug.Log("키누름!");
            StartCoroutine(currentPortal.GoNextRoom());
        }
    }
}
