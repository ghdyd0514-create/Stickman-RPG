using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

//  기능 : 숨겨진 길의 입구에 플레이어 콜라이더가 닿으면 시각적으로 가려진 길의 투명도를 낮추어 보여주게 한다.
public class HiddenPathEntrance : MonoBehaviour
{
    [Header("가려진 길이 열리거나 / 닫힐 때, 부드럽게 열고 닫힘을 몇초동안 보여줄건지")]
    [SerializeField] float smoothTime = 1;

    Tilemap hiddenPathSpriteRenderer;
    Coroutine alphaCoroutine;
    int faceCollider = 0;

    void Awake()
    {
        hiddenPathSpriteRenderer = GetComponent<Tilemap>();
        hiddenPathSpriteRenderer.color = new Color(1, 1, 1, 1);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            faceCollider++;

            //  현재 알파코루틴이 널이 아니라면(즉, 코루틴이 끝까지 다 돌지 못하고 중간에 새로운 이벤트가 발생했다면)
            //  현재 작동중인 코루틴은 멈추고, 새 코루틴을 실행시킨다.
            if (alphaCoroutine != null)
            {
                StopCoroutine(alphaCoroutine);
            }

            alphaCoroutine = StartCoroutine(MoveOverTime(0.32f));
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            faceCollider--;

            //  현재 알파코루틴이 널이 아니라면(즉, 코루틴이 끝까지 다 돌지 못하고 중간에 새로운 이벤트가 발생했다면)
            //  현재 작동중인 코루틴은 멈추고, 새 코루틴을 실행시킨다.
            if (alphaCoroutine != null)
            {
                StopCoroutine(alphaCoroutine);
            }

            if (faceCollider <= 0)
                alphaCoroutine = StartCoroutine(MoveOverTime(1));
        }
    }

    //  플레이어가 입구에 발 담구었을 때
    IEnumerator MoveOverTime(float targetAlpha)
    {
        float time = 0;

        while(time < smoothTime)
        {
            time += Time.deltaTime;

            float startAlpha = hiddenPathSpriteRenderer.color.a;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / smoothTime);
            hiddenPathSpriteRenderer.color = new Color(1, 1, 1, newAlpha);

            yield return null;
        }

        hiddenPathSpriteRenderer.color = new Color(1, 1, 1, targetAlpha);

        alphaCoroutine = null;
    }
}