using System;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress instance;

    public int cashData = 0;
    public int potionData = 0;
    public int playerAttackPower = 5;   //  초기값 5

    public Action cashChangeEvent;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}