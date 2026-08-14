using UnityEngine;

// 讓背景層依照攝影機位移產生視差效果。Parallax Factor 越接近 1，越像固定在畫面上；越接近 0，卷動感越強。
[ExecuteAlways]
public class ParallaxLayer3D : MonoBehaviour
{
    [Header("視差設定")]
    [Tooltip("要追蹤的攝影機。通常指定 Main Camera；留空時會自動尋找 Camera.main。")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("背景跟隨攝影機的比例。X 控制橫向卷軸，Y 控制垂直卷軸。1 表示固定在畫面上，0 表示完全留在世界座標。")]
    [SerializeField] private Vector2 parallaxFactor = new Vector2(0.8f, 0.9f);

    [Tooltip("勾選後會在啟用時重新記錄目前背景與攝影機的位置，作為視差計算起點。")]
    [SerializeField] private bool recenterOnEnable = true;

    private Vector3 startCameraPosition;
    private Vector3 startLayerPosition;
    private bool initialized;

    private void OnEnable()
    {
        ResolveCamera();

        if (recenterOnEnable)
        {
            Recenter();
        }
    }

    private void LateUpdate()
    {
        ResolveCamera();
        if (cameraTransform == null)
        {
            return;
        }

        if (!initialized)
        {
            Recenter();
        }

        Vector3 cameraDelta = cameraTransform.position - startCameraPosition;
        transform.position = new Vector3(
            startLayerPosition.x + cameraDelta.x * parallaxFactor.x,
            startLayerPosition.y + cameraDelta.y * parallaxFactor.y,
            startLayerPosition.z);
    }

    public void Configure(Transform targetCamera, Vector2 factor)
    {
        cameraTransform = targetCamera;
        parallaxFactor = factor;
        Recenter();
    }

    public void Recenter()
    {
        ResolveCamera();
        startCameraPosition = cameraTransform != null ? cameraTransform.position : Vector3.zero;
        startLayerPosition = transform.position;
        initialized = true;
    }

    private void ResolveCamera()
    {
        if (cameraTransform != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
    }
}
