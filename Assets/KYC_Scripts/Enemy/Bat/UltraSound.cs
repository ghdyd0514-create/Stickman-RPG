using System.Collections;
using UnityEngine;

public class UltraSound : MonoBehaviour
{
    Rigidbody2D rigid;
    Vector2 ultraSoundVec;

    [Header("화살속도")]
    [SerializeField] float ultraSoundSpeed = 1.0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 10.0f);
    }

    void Start()
    {
        //  초음파가 나아갈 방향 설정
        ultraSoundVec = ((Vector2)Player.instance.GetAimPoint().position - (Vector2)gameObject.transform.position).normalized;

        float angle = Mathf.Atan2(ultraSoundVec.y, ultraSoundVec.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle + 180);
    }

    void FixedUpdate()
    {
        rigid.linearVelocity = ultraSoundVec * ultraSoundSpeed;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player Hit Detector"))
        {
            StartCoroutine(PlayerHitArrow());
        }
    }

    //  화살이 플레이어와 피격 당하면, 즉시 화살이 사라지게 한다. 단, 데미지를 입기 전에 사라질 수 있으니 데미지를 입을 수 있도록 시간을 보정시켜준다
    IEnumerator PlayerHitArrow()
    {
        yield return null;
        yield return null;

        gameObject.SetActive(false);
    }
}