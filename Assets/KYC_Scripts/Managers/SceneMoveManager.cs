using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveManager : MonoBehaviour
{
    //  씬 이동
    public IEnumerator MoveScene(string sceneName)
    {
        yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeIn());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(FadeSystem.instance.fadeInOutManager.FadeOut());
    }

    //  타이틀로 이동하는 코루틴
    public IEnumerator EnterTitle()
    {
        yield return StartCoroutine(MoveScene("Title"));
    }

    //  스테이지에 입장하는 코루틴
    public IEnumerator EnterStage(string sceneName)
    {
        yield return StartCoroutine(MoveScene(sceneName));
    }

    //  스테이지 선택 씬으로 이동하는 코루틴
    public IEnumerator EnterStageSelectScene()
    {
        yield return StartCoroutine(MoveScene("StageSelectScene"));
    }

    //  마을로 이동하는 코루틴
    public IEnumerator EnterVillage(string sceneName)
    {
        yield return StartCoroutine(MoveScene(sceneName));
    }
}