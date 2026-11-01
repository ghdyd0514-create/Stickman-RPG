using UnityEngine;

public class DogAttackHitbox : MonoBehaviour
{
    DogAttack dogAttack;

    void Awake()
    {
        dogAttack = GetComponentInParent<DogAttack>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player Hit Detector"))
        {
            dogAttack.BiteSuccess();
        }
    }
}