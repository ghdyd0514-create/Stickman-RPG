using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : MonoBehaviour
{
    [SerializeField] Image hpBarBackground;
    [SerializeField] Image hpFill;

    public void UpdateHP(float currentHP, float maxHP)
    {
        hpFill.fillAmount = currentHP / maxHP;
    }
}