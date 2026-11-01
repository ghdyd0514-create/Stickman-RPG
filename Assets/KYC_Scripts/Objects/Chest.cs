using UnityEngine;

public class Chest : Parent_InteractableObject
{
    public override void Interact(Player player)
    {
        Debug.Log("상자 엶");
        gameObject.SetActive(false);
    }
}