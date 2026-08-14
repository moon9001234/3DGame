using UnityEngine;

// \u8b93\u80cc\u666f\u5c64\u4f9d\u7167\u651d\u5f71\u6a5f\u4f4d\u79fb\u7522\u751f\u8996\u5dee\u6548\u679c\u3002Parallax Factor \u8d8a\u63a5\u8fd1 1\uff0c\u8d8a\u50cf\u56fa\u5b9a\u5728\u756b\u9762\u4e0a\uff1b\u8d8a\u63a5\u8fd1 0\uff0c\u5377\u52d5\u611f\u8d8a\u5f37\u3002
[ExecuteAlways]
public class ParallaxLayer3D : MonoBehaviour
{
    [Header("\u8996\u5dee\u8a2d\u5b9a")]
    [Tooltip("\u8981\u8ffd\u8e64\u7684\u651d\u5f71\u6a5f\u3002\u901a\u5e38\u6307\u5b9a Main Camera\uff1b\u7559\u7a7a\u6642\u6703\u81ea\u52d5\u5c0b\u627e Camera.main\u3002")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("\u80cc\u666f\u8ddf\u96a8\u651d\u5f71\u6a5f\u7684\u6bd4\u4f8b\u3002X \u63a7\u5236\u6a6b\u5411\u5377\u8ef8\uff0cY \u63a7\u5236\u5782\u76f4\u5377\u8ef8\u30021 \u8868\u793a\u56fa\u5b9a\u5728\u756b\u9762\u4e0a\uff0c0 \u8868\u793a\u5b8c\u5168\u7559\u5728\u4e16\u754c\u5ea7\u6a19\u3002")]
    [SerializeField] private Vector2 parallaxFactor = new Vector2(0.8f, 0.9f);

    [Tooltip("\u52fe\u9078\u5f8c\u6703\u5728\u555f\u7528\u6642\u91cd\u65b0\u8a18\u9304\u76ee\u524d\u80cc\u666f\u8207\u651d\u5f71\u6a5f\u7684\u4f4d\u7f6e\uff0c\u4f5c\u70ba\u8996\u5dee\u8a08\u7b97\u8d77\u9ede\u3002")]
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
