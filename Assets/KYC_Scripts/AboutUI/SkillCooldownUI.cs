using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("쿨다운 이미지")]
    [SerializeField] Image cooldownImage;

    [Header("개수가 존재하며 쿨타임도 있는 물체냐")]
    [SerializeField] bool isObject = false;

    [Header("화면에 보이는 스킬 아이콘인가")]
    [SerializeField] bool isShowingSkillIcon = false;

    void Awake()
    {
        if (!isObject)
        {
            if (!isShowingSkillIcon)
                gameObject.SetActive(false);
        }
    }

    //  스킬의 쿨타임을 나타내는 코루틴
    public IEnumerator SkillCooldown(float skillCoolTime)
    {
        //  스킬 쿨다운 이미지 on;
        cooldownImage.fillAmount = 1;

        float timer = 0;

        while(timer <= skillCoolTime)
        {
            timer += Time.deltaTime;
            
            //  서서히 채워진 이미지가 벗겨진다.
            cooldownImage.fillAmount = 1 - (timer / skillCoolTime);

            yield return null;
        }

        cooldownImage.fillAmount = 0;
    }

    //  활성화 된 스킬의 남은 시간을 보여주는 코루틴(if, 만약 패링 성공 시 무적시간을 시각적으로 0.5초 보여준다. 0.5초가 끝나면 SetActive를 false시킨다.)
    public IEnumerator AppearActiveSkillTime(float skillCoolTime)
    {
        //  아이콘을 true 시킨다.
        gameObject.SetActive(true);

        //  이미지 on;
        cooldownImage.fillAmount = 1;

        float timer = 0;

        while(timer <= skillCoolTime)
        {
            timer += Time.deltaTime;
            
            //  서서히 채워진 이미지가 벗겨진다.
            cooldownImage.fillAmount = 1 - (timer / skillCoolTime);

            yield return null;
        }

        cooldownImage.fillAmount = 0;

        //  아이콘을 다시 false시킨다.
        gameObject.SetActive(false);
    }
}