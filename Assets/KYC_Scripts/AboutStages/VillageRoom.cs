using UnityEngine;

public class VillageRoom : MonoBehaviour
{
    [Header("해당 room의 카메라 컨파이너 콜라이더")]
    public Collider2D cameraConfinerArea;

    [Header("마을에 입장 시, 플레이어가 위치할 포탈위치")]
    [SerializeField] Transform startPortalTransform;

    [Header("이 공간이 Camera의 Tracking을 필요로 하는가")]
    public bool isCameraTracking = true;

    public void EnterVillage()
    {
        Player.instance.transform.position = startPortalTransform.position;
    }
}