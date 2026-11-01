using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] float followX = 1f;
    [SerializeField] float followY = 0.3f;

    Vector3 startPosition;
    Vector3 cameraStartPosition;

    void Start()
    {
        startPosition = transform.position;
        cameraStartPosition = cam.position;
    }

    void LateUpdate()
    {
        Vector3 cameraDelta = cam.position - cameraStartPosition;

        transform.position = new Vector3(
            startPosition.x + cameraDelta.x * followX,
            startPosition.y + cameraDelta.y * followY,
            transform.position.z
        );
    }
}