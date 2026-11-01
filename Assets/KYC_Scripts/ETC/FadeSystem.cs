using UnityEngine;

public class FadeSystem : MonoBehaviour
{
    public static FadeSystem instance;

    public FadeInOutManager fadeInOutManager;

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