using UnityEngine;

public class EnemyDieEffect : MonoBehaviour
{
    [Header("죽음이펙트 생성 되고 없어지는 시간")]
    [SerializeField] float time = 3;

    void Awake()
    {
       Destroy(gameObject, time);
    }
}