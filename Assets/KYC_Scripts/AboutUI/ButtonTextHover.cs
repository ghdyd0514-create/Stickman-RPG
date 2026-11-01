using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTextHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] Transform buttonText;
    [SerializeField] float hoverScale = 1.1f;

    Vector3 originScale;

    void Awake()
    {
        originScale = buttonText.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.localScale = originScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.localScale = originScale;
    }
}