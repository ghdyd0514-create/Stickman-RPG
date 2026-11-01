using TMPro;
using UnityEngine.UI;
using UnityEngine;

//  이 스크립트의 기능 : AugmentSelectionManager을 통해 추첨받은 3개의 증강을 실제 증강 선택 화면에서 부여한다.
public class AugmentCardUI : MonoBehaviour
{
    AugmentUICanvas augmentUICanvas;

    [Header("증강의 이름과 설명")]
    public TMP_Text augmentName;
    public TMP_Text augmentDescription;

    [Header("해당 카드에 부여된 증강")]
    AugmentsData currentAugment;

    [Header("증강 선택 매니저")]
    public AugmentSelectionManager augmentSelectionManager;

    [Header("현재 카드 번호")]
    [SerializeField] int augmentCardNum;

    [Header("이미지")]
    [SerializeField] Image augmentIcon;

    [Header("버튼")]
    [SerializeField] Button targetButton;

    void Start()
    {
        augmentUICanvas = GetComponentInParent<AugmentUICanvas>();
    }

    //  각 카드에 추첨받은 3개의 증강 데이터를 넣는다
    public void SetUp(int i)
    {
        currentAugment = null;

        //  augmentSelectionManger를 통해서 3개의 증강 추첨받음
        AugmentsData augmentData = augmentSelectionManager.GetOneAugment(augmentCardNum);

        //  증강의 개수가 카드 개수보다 적어서 문제가 생기면...
        //  증강 데이터를 받지 못한 카드는 SetActive를 false 시킨다.
        if (augmentData == null)
        {
            augmentName.text = "";
            augmentDescription.text = "";
            gameObject.SetActive(false);
            return;
        }

        //  이전에 비활성화 했던 카드라면 다시 카드 등장시키기
        gameObject.SetActive(true);

        augmentIcon.sprite = augmentData.augmentIcon;
        augmentName.text = augmentData.augmentName;
        augmentDescription.text = augmentData.augmentDescription;

        currentAugment = augmentData;

        ButtonColorChange();
    }

    void ButtonColorChange()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        //  현재 증강의 등급이 A등급이면 보라색으로 색 변경한당
        if (currentAugment.augmentGrade == AugmentGrade.A)
        {
            ColorBlock colorBlock = targetButton.colors;

            colorBlock.normalColor = new Color32(153, 153, 255, 255);
            colorBlock.highlightedColor = new Color32(153, 153, 255, 255);
            colorBlock.pressedColor = new Color32(118, 118, 197, 255);
            colorBlock.selectedColor = new Color32(153, 153, 255, 255);
            colorBlock.disabledColor = new Color32(118, 118, 197, 255);

            targetButton.colors = colorBlock;
        }

        //  현재 증강의 등급이 B등급이면 골드색으로 색 변경
        else if (currentAugment.augmentGrade == AugmentGrade.B)
        {
            ColorBlock colorBlock = targetButton.colors;

            colorBlock.normalColor = new Color32(255, 215, 0, 255);
            colorBlock.highlightedColor = new Color32(255, 215, 0, 255);
            colorBlock.pressedColor = new Color32(194, 164, 0, 255);
            colorBlock.selectedColor = new Color32(255, 215, 0, 255);
            colorBlock.disabledColor = new Color32(194, 164, 0, 255);

            targetButton.colors = colorBlock;
        }

        //  기본색(흰색)
        else if (currentAugment.augmentGrade == AugmentGrade.C)
        {
            ColorBlock colorBlock = targetButton.colors;

            colorBlock.normalColor = new Color32(255, 255, 255, 255);
            colorBlock.highlightedColor = new Color32(255, 255, 255, 255);
            colorBlock.pressedColor = new Color32(200, 200, 200, 255);
            colorBlock.selectedColor = new Color32(255, 255, 255, 255);
            colorBlock.disabledColor = new Color32(200, 200, 200, 255);

            targetButton.colors = colorBlock;
        }
    }

    //  증강 선택 ui 관리자한테 '나 이제 카드 선택했어요~' 하고 알려준다.
    public void OnSelectCard()
    {
        augmentUICanvas.SelectedCard(currentAugment);
    }
}