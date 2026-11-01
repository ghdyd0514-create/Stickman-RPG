using UnityEngine;

public class UpperSlashAnimEvent : MonoBehaviour
{
    UpperSlash upperSlash;

    void Awake()
    {
        upperSlash = GetComponentInParent<UpperSlash>();
    }

    public void OnEnableCollider()
    {
        upperSlash.OnEnableCollider();
    }

    public void OnEnableEffect()
    {
        upperSlash.OnEnableEffect();
    }
}