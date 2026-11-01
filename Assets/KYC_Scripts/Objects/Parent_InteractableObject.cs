using Unity.VisualScripting;
using UnityEngine;

public enum InteractableType
{
    Augment,        //  증강
    Chest,          //  상자
    SlotMachine,    //  슬롯머신
    NPC,             //  NPC
    NextStage       //  다음 스테이지로 이동하는 거에 대한 상호작용
}

//  모든 상호작용 가능한 것들에 대한 부모 스크립트.
public abstract class Parent_InteractableObject : MonoBehaviour
{
    [Header("상호작용 대상 종류")]
    [SerializeField] protected InteractableType interactableType;

    [Header("상호작용 안내 문구")]
    [SerializeField] protected string interactionTextInfo;

    public InteractableType GetInteractableType()
    {
        return interactableType;
    }

    public string GetInteractionText()
    {
        return interactionTextInfo;
    }

    public abstract void Interact(Player player);
}