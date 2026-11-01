using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//  기능
//  1. 몬스터의 체력을 hpBar를 통해 보여준다
//  2. 체력바는 몬스터가 데미지를 입으면 나타난다.
//  3. 체력바는 3초뒤에 스르륵 사라진다. 단, 3초 안에 몬스터가 데미지를 입는다면 3초라는 시간은 데미지를 입은 시점에서 다시 초기화된다.
public class EnemyHpBar : MonoBehaviour
{
    [SerializeField] Image hpBarBackground;
    [SerializeField] Image hpFill;
    [SerializeField] float hpBarKeepingTime = 2.0f;
    [SerializeField] float hpBarDisappearingTime = 0.5f;

    Coroutine hideCoroutine;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void UpdateHP(float currentHP, float maxHP)
    {
        hpFill.fillAmount = currentHP / maxHP;

        ShowUI();

    }

    //  데미지를 받으면 hp바를 보여준다
    void ShowUI()
    {
        gameObject.SetActive(true);

        Color backgroundColor = hpBarBackground.color;
        backgroundColor.a = 1f;
        hpBarBackground.color = backgroundColor;

        // Fill 알파만 복구
        Color fillColor = hpFill.color;
        fillColor.a = 1f;
        hpFill.color = fillColor;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideHPBar());
    }

    IEnumerator HideHPBar()
    {
        yield return new WaitForSeconds(hpBarKeepingTime);

        // 페이드 시작 시 알파값 저장
        float startAlpha = hpBarBackground.color.a; //  어차피 백그라운드나 fill이나 동시에 사라질거라 아무거나 지정해도 상관없

        float timer = 0;
        while (timer < hpBarDisappearingTime)
        {
            timer += Time.deltaTime;

            float newAlpha = Mathf.Lerp(startAlpha, 0, timer / hpBarDisappearingTime);

            Color backgroundColor = hpBarBackground.color;
            backgroundColor.a = newAlpha;
            hpBarBackground.color = backgroundColor;

            Color fillColor = hpFill.color;
            fillColor.a = newAlpha;
            hpFill.color = fillColor;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}