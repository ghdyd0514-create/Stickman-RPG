using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{

    public void LoadScene(string scenename)
    {
        SceneManager.LoadScene(scenename); 
    }

    public void QuitScene()
    {
        Application.Quit();
    }
}
