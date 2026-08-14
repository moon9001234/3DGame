using UnityEngine;
using UnityEngine.SceneManagement;

// \u73a9\u5bb6\u6b7b\u4ea1\u5f8c\u5ef6\u9072\u91cd\u65b0\u8f09\u5165\u76ee\u524d\u5834\u666f\uff0c\u4f5c\u70ba\u66ab\u4ee3\u91cd\u751f\u6d41\u7a0b\u3002
public class SideScrollerPlayerRespawn : MonoBehaviour
{
    [Header("\u91cd\u751f\u8a2d\u5b9a")]
    [Tooltip("\u73a9\u5bb6\u6b7b\u4ea1\u5f8c\uff0c\u7b49\u5f85\u5e7e\u79d2\u624d\u91cd\u65b0\u8f09\u5165\u76ee\u524d\u5834\u666f\u3002")]
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
