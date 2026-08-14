using UnityEngine;
using UnityEngine.SceneManagement;

// 玩家死亡後延遲重新載入目前場景，作為暫代重生流程。
public class SideScrollerPlayerRespawn : MonoBehaviour
{
    [Header("重生設定")]
    [Tooltip("玩家死亡後，等待幾秒才重新載入目前場景。")]
    [SerializeField] private float reloadDelay = 1.2f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += ReloadScene;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= ReloadScene;
        }
    }

    private void ReloadScene()
    {
        Invoke(nameof(LoadActiveScene), reloadDelay);
    }

    private void LoadActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
