using UnityEngine;

public static class SideScrollerSfxPlayer
{
    private const string RuntimeObjectName = "SideScroller SFX Player";
    private const float RepeatedClipCooldown = 0.03f;

    private static AudioSource sharedSource;
    private static AudioClip registeredMusicClip;
    private static AudioClip lastPlayedClip;
    private static float lastPlayedTime = -999f;

    public static void RegisterMusicClip(AudioClip clip)
    {
        registeredMusicClip = clip;
    }

    public static void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || volume <= 0f)
        {
            return;
        }

        if (clip == registeredMusicClip)
        {
            Debug.LogWarning("SideScrollerSfxPlayer ignored a one-shot request because the clip is registered as background music. Check weapon/enemy hit sound settings.");
            return;
        }

        if (clip == lastPlayedClip && Time.unscaledTime - lastPlayedTime < RepeatedClipCooldown)
        {
            return;
        }

        AudioSource source = EnsureSource();
        if (source == null)
        {
            return;
        }

        lastPlayedClip = clip;
        lastPlayedTime = Time.unscaledTime;
        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private static AudioSource EnsureSource()
    {
        if (sharedSource != null)
        {
            EnsureSourceReady(sharedSource);
            EnsureAudioListener(sharedSource.gameObject);
            return sharedSource;
        }

        GameObject sourceObject = GameObject.Find(RuntimeObjectName);
        if (sourceObject == null)
        {
            sourceObject = new GameObject(RuntimeObjectName);
            Object.DontDestroyOnLoad(sourceObject);
        }

        sharedSource = sourceObject.GetComponent<AudioSource>();
        if (sharedSource == null)
        {
            sharedSource = sourceObject.AddComponent<AudioSource>();
        }

        EnsureSourceReady(sharedSource);
        EnsureAudioListener(sourceObject);
        return sharedSource;
    }

    private static void EnsureSourceReady(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.gameObject.SetActive(true);
        source.enabled = true;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
    }

    private static void EnsureAudioListener(GameObject fallbackObject)
    {
        if (Object.FindFirstObjectByType<AudioListener>() != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        GameObject listenerObject = mainCamera != null ? mainCamera.gameObject : fallbackObject;
        if (listenerObject.GetComponent<AudioListener>() == null)
        {
            listenerObject.AddComponent<AudioListener>();
        }
    }
}
