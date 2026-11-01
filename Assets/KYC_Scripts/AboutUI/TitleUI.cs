using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [Header("게임 시작/메뉴얼/게임 종료 패널")]
    [SerializeField] GameObject majorPanel;

    [Header("메뉴얼 패널")]
    [SerializeField] GameObject ManualPanel;
    [SerializeField] GameObject ManualPage1;
    [SerializeField] GameObject ManualPage2;
    [SerializeField] GameObject ManualPage3;

    [SerializeField] GameObject waringPanel;

    //[Header("주요 패널 버튼들")]
    //[SerializeField] Transform majorButtonText;

    //[SerializeField] float normalScale = 1f;
    //[SerializeField] float hoverScale = 1.1f;

    [Header("음향 패널")]
    [SerializeField] GameObject audioPanel;

    [Header("오디오 버튼")]
    [SerializeField] GameObject audioButton;

    [Header("패널 인덱스 번호")]
    [SerializeField] int panelIndex = 1;



    /*public void OnPointerEnter(PointerEventData eventData)
    {
        majorButtonText.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        majorButtonText.localScale = Vector3.one * normalScale;
    }*/

    //  ---------------------------------------------------
    //  음향 버튼
    //  ---------------------------------------------------
    public void AudioButton()
    {
        if (!audioPanel.gameObject.activeSelf)
        {
            audioPanel.gameObject.SetActive(true);
            majorPanel.gameObject.SetActive(false);
            audioButton.gameObject.SetActive(false);
        }
    }

    //  ---------------------------------------------------
    //  음향 닫기 버튼
    //  ---------------------------------------------------
    public void AudioCloseButton()
    {
        if (audioPanel.gameObject.activeSelf)
        {
            audioPanel.gameObject.SetActive(false);
            majorPanel.gameObject.SetActive(true);
            audioButton.gameObject.SetActive(true);
        }
    }

    //  ---------------------------------------------------
    //  게임시작 버튼
    //  ---------------------------------------------------
    public void StartButton()
    {
        //  사용자는 최초 1회, 헬프 버튼(조작키 메뉴얼)을 눌러야 게임 시작 할 수 있게 한다. 
        //  조작키는 익혀야제......
        if (Managers.instance.gameManager.isWatchingHelp)
            StartCoroutine(Managers.instance.sceneMoveManager.EnterVillage("Village"));

        //  아직 안 봤다면 경고 패널 뜸
        else
        {
            if (!waringPanel.gameObject.activeSelf)
            {
                waringPanel.gameObject.SetActive(true);
                majorPanel.gameObject.SetActive(false);
                audioButton.gameObject.SetActive(false);
            }
        }
    }

    //  ---------------------------------------------------
    //  경고패널 닫기 버튼
    //  ---------------------------------------------------
    public void waringPanelCloseButton()
    {
        if (waringPanel.gameObject.activeSelf)
        {
            waringPanel.gameObject.SetActive(false);
            majorPanel.gameObject.SetActive(true);
            audioButton.gameObject.SetActive(true);
        }
    }

    //  ---------------------------------------------------
    //  조작키 버튼
    //  ---------------------------------------------------
    public void ManualButton()
    {
        if (!ManualPanel.gameObject.activeSelf)
        {
            ManualPanel.gameObject.SetActive(true);
            majorPanel.gameObject.SetActive(false);
            audioButton.gameObject.SetActive(false);

            //  조작키 메뉴얼 봄.
            Managers.instance.gameManager.isWatchingHelp = true;
        }
    }

    //  ---------------------------------------------------
    //  조작키 닫기 버튼
    //  ---------------------------------------------------
    public void ManualCloseButton()
    {
        if (ManualPanel.gameObject.activeSelf)
        {
            ManualPanel.gameObject.SetActive(false);
            majorPanel.gameObject.SetActive(true);
            audioButton.gameObject.SetActive(true);
        }
    }

    //  --------------------------------------------------
    //  조작키 패널 화살표 버튼
    //  --------------------------------------------------
    public void PanelLeftArrowButton()  //  왼쪽 화살표
    {
        panelIndex--;

        if (panelIndex <= 1)
        {
            panelIndex = 1;
        }

        switch (panelIndex)
        {
            case 1:
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(true);
                break;

            case 2:
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(true);
                break;

            case 3:
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(true);
                break;
        }
    }

    public void PanelRightArrowButton()  //  오른쪽 화살표
    {
        panelIndex++;

        if (panelIndex >= 3)
        {
            panelIndex = 3;
        }

        switch (panelIndex)
        {
            case 1:
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(true);
                break;

            case 2:
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(true);
                break;

            case 3:
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(true);
                break;
        }
    }

    //  ---------------------------------------------------
    //  게임종료 버튼
    //  ---------------------------------------------------
    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 플레이 모드 종료
#else
                Application.Quit(); // 실제 빌드된 애플리케이션 종료
#endif
    }
}