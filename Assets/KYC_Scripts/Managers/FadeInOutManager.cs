using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

//  기능 : 페이드 인 아웃 해주는 넘
public class FadeInOutManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.4f; // 페이드 시간

    //-----------------------------------------------------------------------------------------
    // 페이드 인 담당
    public IEnumerator FadeIn()
    {
        /*Debug.Log("FadeImage Null? " + (fadeImage == null));

        if (fadeImage != null)
        {
            Debug.Log("Image Scene : " + fadeImage.gameObject.scene.name);
            Debug.Log("Image Root : " + fadeImage.transform.root.name);
        }*/

        float timer = 0;
        while (timer <= fadeDuration)
        {
            //  페이드 인

            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }
    }

    //  페이드 아웃 담당
    public IEnumerator FadeOut()
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            //  페이드 아웃
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - timer / fadeDuration);
            yield return null;

        }
        fadeImage.color = new Color(0, 0, 0, 0);
    }






















    // -----------------------------------------------------------------------------------
    // 마을 및 스테이지 선택 씬으로 돌아갈 때 사용하는 fade
    /*public IEnumerator SceneFade(string sceneName)
    {
        float timer = 0;
        while (timer <= fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }
        fadeImage.color = new Color (0, 0, 0, 1);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        timer = 0;

        while (timer <= fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - timer / fadeDuration);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void EnterStage(string sceneName)
    {
        this.sceneName = sceneName;
        StartCoroutine(SceneFade(this.sceneName));
        this.sceneName = null;
    }

    public void GoStageSelectScene()
    {
        sceneName = "StageSelect Scene";
        StartCoroutine(SceneFade(sceneName));
        sceneName = null;
    }

    // 부활은 마을에서
    public void Revive()
    {
        sceneName = "Village";
        StartCoroutine(SceneFade(sceneName));
        sceneName = null;
    } */
}
