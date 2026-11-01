using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = FindAnyObjectByType<CinemachineImpulseSource>();
    }

    public void Shake(float shakePower)
    {
        impulseSource.GenerateImpulse(shakePower);
    }
}