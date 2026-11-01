using System.Collections.Generic;
using UnityEngine;

public class DashEffectPool : MonoBehaviour
{
    [SerializeField] GameObject dashEffectPrefab;
    [SerializeField] int poolSize = 5;

    List<GameObject> dashEffectList = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject dashEffectInstant = Instantiate(dashEffectPrefab, transform);
            dashEffectInstant.SetActive(false);
            dashEffectList.Add(dashEffectInstant);
        }
    }

    public GameObject GetEffect()
    {
        foreach (GameObject effect in dashEffectList)
        {
            if (!effect.activeSelf)
            {
                return effect;
            }
        }

        return null;
    }
}