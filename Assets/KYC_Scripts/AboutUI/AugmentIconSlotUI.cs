using UnityEngine.UI;
using UnityEngine;

public class AugmentIconSlotUI : MonoBehaviour
{
    [Header("윤곽선 이미지")]
    [SerializeField] Image outLine;

    [Header("아이콘 이미지")]
    [SerializeField] Image iconImage;

    public void SetSelected(bool enable)
    {
        outLine.gameObject.SetActive(enable);
    }

    public void SetIconImage(Sprite iconSprite)
    {
        iconImage.sprite = iconSprite;
    }
}