using UnityEngine;
using Unity.Cinemachine;

public class ZoomIn : MonoBehaviour
{
    public CinemachineCamera cam;

    public float normalSize = 5f;   // 기본 카메라 크기
    public float zoomSize = 2.5f;   // 확대 시 카메라 크기
    public float zoomSpeed = 5f;    // 확대/축소 속도

    private bool isZoom = false;    // 확대 여부

    void Update()
    {
        if (cam == null)
        {
            return;
        }

        LensSettings lens = cam.Lens;

        float target;

        if (isZoom)
        {
            target = zoomSize;      // 확대
        }
        else
        {
            target = normalSize;    // 원래 크기
        }

        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            target,
            Time.deltaTime * zoomSpeed
        );

        cam.Lens = lens;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isZoom = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isZoom = false;
        }
    }
}