using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraConfinerManager : MonoBehaviour
{
    [Header("참조할 시네머신 카메라")]
    [SerializeField] CinemachineConfiner2D cinemachineConfiner;
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] CinemachinePositionComposer cinemachinePositionComposer;

    [Header("카메라 축소/확대 속도")]
    [SerializeField] float cameraLensChangeSpeed = 1.0f;

    //  씬이 바뀔 때마다 그 씬에 있는 기본적인 씨네머신 카메라 찾기
    public void SetCinemachineReference()
    {
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        cinemachineConfiner = FindAnyObjectByType<CinemachineConfiner2D>();
        cinemachinePositionComposer = FindAnyObjectByType<CinemachinePositionComposer>();
    }

    //  화면이 전환되면(씬 전환 말하는 거 아님) 그 화면에 맞는 카메라 컨파이너를 설정.
    //  혹은 새로운 카메라 제한 콜라이더가 생기면 그 콜라이더를 카메라 컨파이너로 설정.
    public void SetConfiner(Collider2D collider)
    {
        cinemachineConfiner.BoundingShape2D = collider;
    }

    //  카메라가 트랙킹을 하지 않게 한다.
    public void StopCameraTracking()
    {
        cinemachineCamera.Follow = null;
    }

    //  카메라가 플레이어를 트랙킹 하게 한다
    public void OnCameraTracking()
    {
        cinemachineCamera.Follow = Player.instance.transform;
    }

    public void SnapCamera()
    {
        cinemachineCamera.PreviousStateIsValid = false;
    }

    //  카메라가 점점 확대/축소 되는 걸 연출하기 위해 코루틴으로
    public IEnumerator SetCameraLens(float lens)
    {
        while (Mathf.Abs(cinemachineCamera.Lens.OrthographicSize - lens) > 0.01f)
        {
            cinemachineCamera.Lens.OrthographicSize = Mathf.MoveTowards(cinemachineCamera.Lens.OrthographicSize, lens, cameraLensChangeSpeed * Time.deltaTime);

            // Confiner도 새로운 렌즈 크기로 다시 계산
            cinemachineConfiner.InvalidateLensCache();

            yield return null;
        }
        
        cinemachineCamera.Lens.OrthographicSize = lens;
        cinemachineConfiner.InvalidateLensCache();
    }
}