using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ESCMenu : MonoBehaviour
{
    public GameObject pausePanel;

    [Header("메뉴얼 패널")]
    [SerializeField] GameObject ManualPanel;
    [SerializeField] GameObject ManualPage1;
    [SerializeField] GameObject ManualPage2;
    [SerializeField] GameObject ManualPage3;

    [Header("패널 인덱스 번호")]
    [SerializeField] int panelIndex = 1;

    //  활성화 될 때, 일시정지
    void OnEnable()
    {
        Time.timeScale = 0;
    }

    //  다시 비활성화 되면 일시정지 풀림
    void OnDisable()
    {
        Time.timeScale = 1;        
    }

    //  타이틀로 이동하는 버튼
    public void QuitButton()
    {
        StartCoroutine(Managers.instance.sceneMoveManager.EnterTitle());
        Time.timeScale = 1; 
    }

    //  resume버튼. 그런데 esc눌러도 일시정지 캔버스 없어지긴 함
    public void ResumeButton()
    {
        gameObject.SetActive(false);
    }

        //  ---------------------------------------------------
    //  조작키 버튼
    //  ---------------------------------------------------
    public void ManualButton()
    {
        if (!ManualPanel.gameObject.activeSelf)
        {
            ManualPanel.gameObject.SetActive(true);
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
        }
    }

    //  --------------------------------------------------
    //  조작키 패널 화살표 버튼
    //  --------------------------------------------------
    public void PanelLeftArrowButton()  //  왼쪽 화살표
    {
        panelIndex --;

        if (panelIndex <= 1)
        {
            panelIndex = 1;
        }

        switch(panelIndex)
        {
            case 1 :
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(true);
                break;

            case 2 :
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(true);
                break;

            case 3 :
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(true);
                break;
        }
    }

    public void PanelRightArrowButton()  //  오른쪽 화살표
    {
        panelIndex ++;

        if (panelIndex >= 3)
        {
            panelIndex = 3;
        }

        switch(panelIndex)
        {
            case 1 :
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(true);
                break;

            case 2 :
                ManualPage3.gameObject.SetActive(false);
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(true);
                break;

            case 3 :
                ManualPage1.gameObject.SetActive(false);
                ManualPage2.gameObject.SetActive(false);
                ManualPage3.gameObject.SetActive(true);
                break;
        }
    }



    public void OnEnableCanvas(InputAction.CallbackContext context)
    {
        //  캔버스가 꺼져있는 상태에서 esc누르면 캔버스 활성화
        if (context.performed && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        else if (context.performed && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}