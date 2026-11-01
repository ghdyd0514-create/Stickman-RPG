using System;
using System.Collections;
using UnityEngine;

public class DashEffect : MonoBehaviour
{
    [Header("이펙트의 투명도 변경 시간")]
    [SerializeField] float alphaChangeTime;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        StartCoroutine(AlphaChange());
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    IEnumerator AlphaChange()
    {
        //  알파값 올리기
        float timer = 0;

        while (timer < alphaChangeTime)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(0f, 190f / 255f, timer / alphaChangeTime);
            spriteRenderer.color = new Color(1, 1, 1, alpha);

            yield return null;
        }

        spriteRenderer.color = new Color(1, 1, 1, 0.745f);

        //  알파값 내리기
        float timer2 = 0;

        while (timer2 < alphaChangeTime)
        {
            timer2 += Time.deltaTime;

            float alpha = Mathf.Lerp(190f / 255f, 0f, timer2 / alphaChangeTime);
            spriteRenderer.color = new Color(1, 1, 1, alpha);

            yield return null;
        }

        spriteRenderer.color = new Color(1, 1, 1, 0);

        gameObject.SetActive(false);

    }

    void OnDisable()
    {
        //  확실하게 알파값 0으로 하고 가버려라
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }
}