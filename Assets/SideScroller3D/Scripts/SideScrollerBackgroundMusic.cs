using UnityEngine;

public class SideScrollerBackgroundMusic : MonoBehaviour
{
    private const string RuntimeObjectName = "SideScroller Background Music";
    private const string DefaultMusicPath = "Assets/Art/Sound/Copper_Heart_Meltdown.mp3";

    private static AudioSource sharedSource;

    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool persistAcrossScenes = true;

    private void OnValidate()
    {
        AssignDefaultMusicIfNeeded();
        volume = Mathf.Clamp01(volume);
    }

    private void Awake()
    {
        AssignDefaultMusicIfNeeded();

        if (playOnAwake)
        {
            Play();
        }
    }

    public void Play()
    {
        if (musicClip == null)
        {
            return;
        }

        SideScrollerSfxPlayer.RegisterMusicClip(musicClip);

        AudioSource source = EnsureSource();
        if (source == null)
        {
            return;
        }

        bool clipChanged = source.clip != musicClip;
        source.clip = musicClip;
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.enabled = true;

        if (clipChanged || !source.isPlaying)
        {
            source.Play();
        }
    }

    public void Stop()
    {
        if (sharedSource != null)
        {
            sharedSource.Stop();
        }
    }

    private AudioSource EnsureSource()
    {
        if (sharedSource != null)
        {
            EnsureAudioListener(sharedSource.gameObject);
            return sharedSource;
        }

        GameObject sourceObject = GameObject.Find(RuntimeObjectName);
        if (sourceObject == null)
        {
            sourceObject = new GameObject(RuntimeObjectName);
        }

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(sourceObject);
        }

        sharedSource = sourceObject.GetComponent<AudioSource>();
        if (sharedSource == null)
        {
            sharedSource = sourceObject.AddComponent<AudioSource>();
        }

        EnsureAudioListener(sourceObject);
        return sharedSource;
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

    private void AssignDefaultMusicIfNeeded()
    {
#if UNITY_EDITOR
        if (musicClip == null)
        {
            musicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultMusicPath);
        }
#endif
    }
}
