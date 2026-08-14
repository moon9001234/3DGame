using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// 偵測場上敵人是否全部被擊敗，顯示通關文字後重新開始目前場景。
public class GameClearManager3D : MonoBehaviour
{
    private const string RuntimeObjectName = "GameClearManager3D";
    private const string ClearCanvasName = "Game Clear Canvas";

    [Header("通關條件")]
    [Tooltip("被視為敵人的 Layer。預設會使用 Enemy Layer。")]
    [SerializeField] private LayerMask enemyMask;

    [Tooltip("開場後延遲幾秒才開始檢查，避免場景物件還沒完成初始化就誤判通關。")]
    [SerializeField] private float firstCheckDelay = 0.25f;

    [Tooltip("每隔幾秒重新掃描一次敵人，避免動態生成或 Destroy 時漏判。")]
    [SerializeField] private float checkInterval = 0.25f;

    [Header("通關顯示")]
    [Tooltip("擊敗所有敵人後顯示的文字。")]
    [SerializeField] private string clearMessage = "恭喜通關";

    [Tooltip("通關後是否自動重新開始目前場景。")]
    [SerializeField] private bool restartAfterClear;

    [Tooltip("通關文字顯示幾秒後重新開始遊戲。")]
    [SerializeField] private float restartDelay = 5f;

    [Tooltip("通關文字顏色。")]
    [SerializeField] private Color messageColor = new Color(1f, 0.92f, 0.35f, 1f);

    [Tooltip("通關文字背景顏色。")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.62f);

    private Canvas clearCanvas;
    private Text clearText;
    private bool hasFoundEnemy;
    private bool isClearing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeManager()
    {
        if (FindObjectOfType<GameClearManager3D>() != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(RuntimeObjectName);
        managerObject.AddComponent<GameClearManager3D>();
    }

    private void Awake()
    {
        if (FindObjectsOfType<GameClearManager3D>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        EnsureEnemyMask();
        EnsureClearUI();
    }

    private void Start()
    {
        StartCoroutine(CheckClearRoutine());
    }

    private IEnumerator CheckClearRoutine()
    {
        yield return new WaitForSeconds(firstCheckDelay);

        while (!isClearing)
        {
            if (BossClearOnDeath3D.HasShownClear)
            {
                isClearing = true;
                yield break;
            }

            int aliveEnemyCount = CountAliveEnemies();
            if (aliveEnemyCount > 0)
            {
                hasFoundEnemy = true;
            }
            else if (hasFoundEnemy)
            {
                TriggerGameClear();
                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private int CountAliveEnemies()
    {
        Health[] healthComponents = FindObjectsOfType<Health>();
        int aliveCount = 0;

        for (int i = 0; i < healthComponents.Length; i++)
        {
            Health health = healthComponents[i];
            if (health != null && !health.IsDead && IsEnemy(health.gameObject))
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    private bool IsEnemy(GameObject target)
    {
        return (enemyMask.value & (1 << target.layer)) != 0;
    }

    private void TriggerGameClear()
    {
        if (BossClearOnDeath3D.HasShownClear)
        {
            isClearing = true;
            return;
        }

        isClearing = true;
        EnsureClearUI();
        clearCanvas.gameObject.SetActive(true);
        clearText.text = clearMessage;
        if (restartAfterClear)
        {
            StartCoroutine(RestartRoutine());
        }
    }

    private IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(restartDelay);

        Scene activeScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(activeScene.path))
        {
            EditorSceneManager.LoadScene(activeScene.path);
            yield break;
        }
#endif

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        else
        {
            SceneManager.LoadScene(activeScene.name);
        }
    }

    private void EnsureEnemyMask()
    {
        if (enemyMask.value != 0)
        {
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        enemyMask = enemyLayer >= 0 ? LayerMask.GetMask("Enemy") : Physics.DefaultRaycastLayers;
    }

    private void EnsureClearUI()
    {
        if (clearCanvas != null && clearText != null)
        {
            return;
        }

        GameObject canvasObject = GameObject.Find(ClearCanvasName);
        if (canvasObject == null)
        {
            canvasObject = new GameObject(ClearCanvasName);
        }

        clearCanvas = canvasObject.GetComponent<Canvas>();
        if (clearCanvas == null)
        {
            clearCanvas = canvasObject.AddComponent<Canvas>();
        }

        clearCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        clearCanvas.sortingOrder = 1000;

        if (canvasObject.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        clearText = canvasObject.GetComponentInChildren<Text>(true);
        if (clearText == null)
        {
            CreateClearPanel(canvasObject.transform);
        }

        clearCanvas.gameObject.SetActive(isClearing);
    }

    private void CreateClearPanel(Transform canvasRoot)
    {
        GameObject panelObject = new GameObject("Clear Message Panel");
        panelObject.transform.SetParent(canvasRoot, false);

        Image panel = panelObject.AddComponent<Image>();
        panel.color = backgroundColor;
        panel.raycastTarget = false;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(420f, 120f);

        GameObject textObject = new GameObject("Clear Message Text");
        textObject.transform.SetParent(panelObject.transform, false);

        clearText = textObject.AddComponent<Text>();
        clearText.text = clearMessage;
        clearText.color = messageColor;
        clearText.font = ResolveFont();
        clearText.fontSize = 52;
        clearText.fontStyle = FontStyle.Bold;
        clearText.alignment = TextAnchor.MiddleCenter;
        clearText.horizontalOverflow = HorizontalWrapMode.Wrap;
        clearText.verticalOverflow = VerticalWrapMode.Truncate;
        clearText.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 8f);
        textRect.offsetMax = new Vector2(-16f, -8f);
    }

    private static Font ResolveFont()
    {
        Font font = Font.CreateDynamicFontFromOSFont(
            new[] { "Microsoft JhengHei", "Microsoft YaHei", "PMingLiU", "Arial" },
            32);

        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
