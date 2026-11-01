using UnityEngine;

public class RainbowColor : MonoBehaviour
{
    [Header("무지개색 확률 설정")]
    [Range(0f, 100f)]
    public float rainbowChance = 10f; // 확률 (%) - 인스펙터에서 슬라이더로 조절 가능

    [Header("색 변화 속도")]
    public float speed = 1f;

    private float hue = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isRainbow = false; // 이번에 무지개색으로 당첨됐는지 여부

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 0~100 사이 랜덤값을 뽑아서 확률보다 낮으면 당첨
        float roll = Random.Range(0f, 100f);
        isRainbow = roll < rainbowChance;

        Debug.Log(gameObject.name + " 무지개색 당첨 여부: " + isRainbow + " (확률: " + rainbowChance + "%)");
    }

    void Update()
    {
        if (!isRainbow) return; // 당첨 안 됐으면 색 변화 없음

        hue += speed * Time.deltaTime;
        if (hue > 1f)
        {
            hue -= 1f;
        }

        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        spriteRenderer.color = rainbowColor;
    }
}