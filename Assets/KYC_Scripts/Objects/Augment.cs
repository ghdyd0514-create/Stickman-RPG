using UnityEngine;

public class Augment : Parent_InteractableObject
{
    [Header("증강 획득 후 UI표시")]
    public AugmentUICanvas augmentSelectionUI;

    void Start()
    {
        augmentSelectionUI = FindAnyObjectByType<AugmentUICanvas>(FindObjectsInactive.Include);

        augmentSelectionUI.gameObject.SetActive(false);
    }

    public override void Interact(Player player)
    {
        if (!Managers.instance.augmentManager.HasAvailableAugment())
        {
            gameObject.SetActive(false);
            Debug.Log("모든 증강 획득!");
            return;
        }

        //  그 상태에서 증강만 선택할 수 있도록 증강 선택 ui를 나오게 한다
        augmentSelectionUI.OpenCanvas(player);
        gameObject.SetActive(false);
    }
}