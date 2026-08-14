using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 讓玩家在可執行版中按 Esc 隨時關閉遊戲。
public class QuitGame3D : MonoBehaviour
{
    private const string RuntimeObjectName = "QuitGame3D";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeQuitHandler()
    {
        if (FindObjectOfType<QuitGame3D>() != null)
        {
            return;
        }

        GameObject handlerObject = new GameObject(RuntimeObjectName);
        DontDestroyOnLoad(handlerObject);
        handlerObject.AddComponent<QuitGame3D>();
    }

    private void Update()
    {
        if (WasQuitPressed())
        {
            Quit();
        }
    }

    private static bool WasQuitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
