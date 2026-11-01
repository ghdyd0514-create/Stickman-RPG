using System;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private Collider2D playercollider;

    public bool findPlayerDetected = false;

    public Transform DetectPlayer;
    public LayerMask playerLayer;

    public Action findPlayerAction;

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player")
        {
            findPlayerDetected = true;
            DetectPlayer = other.transform;
        }

        if (findPlayerDetected)
        {
            findPlayerAction?.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            findPlayerDetected = false;
            DetectPlayer = null;

        }
    }

}