using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers instance;

    public GameManager gameManager;
    public SceneMoveManager sceneMoveManager;
    public AugmentManager augmentManager;
    public CameraConfinerManager cameraConfinerManager;
    public AudioManager audioManager;

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