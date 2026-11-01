using System.Collections;
using UnityEngine;

public class WhereAmI : MonoBehaviour
{
    public CanvasGroup textGroup;

    public float fadeInSpeed = 2f;   // 나타나는 속도
    public float showTime = 2f;      // 유지 시간
    public float fadeOutSpeed = 2f;  // 사라지는 속도

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(ShowText());
        }
    }

    IEnumerator ShowText()
    {
        // 처음에는 안 보이게
        textGroup.alpha = 0;

        // 천천히 나타나기
        while (textGroup.alpha < 1)
        {
            textGroup.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        textGroup.alpha = 1;

        // 잠시 유지
        yield return new WaitForSeconds(showTime);

        // 천천히 사라지기
        while (textGroup.alpha > 0)
        {
            textGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        textGroup.alpha = 0;
    }
}