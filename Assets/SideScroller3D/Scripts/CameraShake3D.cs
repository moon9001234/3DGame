using UnityEngine;

[DisallowMultipleComponent]
public class CameraShake3D : MonoBehaviour
{
    [Header("Shake")]
    [Tooltip("Default shake distance in local camera units.")]
    [SerializeField] private float defaultAmplitude = 0.08f;

    [Tooltip("Default shake duration in seconds.")]
    [SerializeField] private float defaultDuration = 0.08f;

    [Tooltip("Default shake frequency.")]
    [SerializeField] private float defaultFrequency = 35f;

    [Tooltip("Use unscaled time so shake still works during hit stop or slow motion.")]
    [SerializeField] private bool useUnscaledTime = true;

    private static CameraShake3D mainInstance;

    private float shakeEndTime;
    private float shakeStartTime;
    private float shakeAmplitude;
    private float shakeDuration;
    private float shakeFrequency;
    private float seedX;
    private float seedY;
    private Vector3 appliedOffset;

    private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;

    public static void Shake(float amplitude, float duration, float frequency)
    {
        CameraShake3D shaker = ResolveMainInstance();
        if (shaker != null)
        {
            shaker.Play(amplitude, duration, frequency);
        }
    }

    public void PlayDefault()
    {
        Play(defaultAmplitude, defaultDuration, defaultFrequency);
    }

    public void Play(float amplitude, float duration, float frequency)
    {
        amplitude = Mathf.Max(0f, amplitude);
        duration = Mathf.Max(0f, duration);
        if (amplitude <= 0f || duration <= 0f)
        {
            return;
        }

        float now = CurrentTime;
        shakeStartTime = now;
        shakeEndTime = Mathf.Max(shakeEndTime, now + duration);
        shakeDuration = Mathf.Max(0.001f, duration);
        shakeAmplitude = Mathf.Max(shakeAmplitude, amplitude);
        shakeFrequency = Mathf.Max(0.01f, frequency);
        seedX = Random.value * 100f;
        seedY = Random.value * 100f + 37.1f;
    }

    private void Awake()
    {
        if (mainInstance == null)
        {
            mainInstance = this;
        }
    }

    private void OnEnable()
    {
        if (mainInstance == null)
        {
            mainInstance = this;
        }
    }

    private void OnDisable()
    {
        RemoveAppliedOffset();
        if (mainInstance == this)
        {
            mainInstance = null;
        }
    }

    private void LateUpdate()
    {
        RemoveAppliedOffset();

        float now = CurrentTime;
        if (now >= shakeEndTime)
        {
            shakeAmplitude = 0f;
            return;
        }

        float elapsed = Mathf.Max(0f, now - shakeStartTime);
        float fade = 1f - Mathf.Clamp01(elapsed / shakeDuration);
        float sampleTime = now * shakeFrequency;
        float x = Mathf.PerlinNoise(seedX, sampleTime) * 2f - 1f;
        float y = Mathf.PerlinNoise(seedY, sampleTime) * 2f - 1f;
        appliedOffset = new Vector3(x, y, 0f) * shakeAmplitude * fade;
        transform.localPosition += appliedOffset;
    }

    private void RemoveAppliedOffset()
    {
        if (appliedOffset.sqrMagnitude <= 0f)
        {
            return;
        }

        transform.localPosition -= appliedOffset;
        appliedOffset = Vector3.zero;
    }

    private static CameraShake3D ResolveMainInstance()
    {
        if (mainInstance != null)
        {
            return mainInstance;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainInstance = mainCamera.GetComponent<CameraShake3D>();
            if (mainInstance == null)
            {
                mainInstance = mainCamera.gameObject.AddComponent<CameraShake3D>();
            }
        }

        return mainInstance;
    }
}
