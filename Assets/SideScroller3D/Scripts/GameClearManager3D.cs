using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// \u5075\u6e2c\u5834\u4e0a\u6575\u4eba\u662f\u5426\u5168\u90e8\u88ab\u64ca\u6557\uff0c\u986f\u793a\u901a\u95dc\u6587\u5b57\u5f8c\u91cd\u65b0\u958b\u59cb\u76ee\u524d\u5834\u666f\u3002
public class GameClearManager3D : MonoBehaviour
{
    private const string RuntimeObjectName = "GameClearManager3D";
    private const string ClearCanvasName = "Game Clear Canvas";

    [Header("\u901a\u95dc\u689d\u4ef6")]
    [Tooltip("\u88ab\u8996\u70ba\u6575\u4eba\u7684 Layer\u3002\u9810\u8a2d\u6703\u4f7f\u7528 Enemy Layer\u3002")]
    [SerializeField] private LayerMask enemyMask;

    [Tooltip("\u958b\u5834\u5f8c\u5ef6\u9072\u5e7e\u79d2\u624d\u958b\u59cb\u6aa2\u67e5\uff0c\u907f\u514d\u5834\u666f\u7269\u4ef6\u9084\u6c92\u5b8c\u6210\u521d\u59cb\u5316\u5c31\u8aa4\u5224\u901a\u95dc\u3002")]
    [SerializeField] private float firstCheckDelay = 0.25f;

    [Tooltip("\u6bcf\u9694\u5e7e\u79d2\u91cd\u65b0\u6383\u63cf\u4e00\u6b21\u6575\u4eba\uff0c\u907f\u514d\u52d5\u614b\u751f\u6210\u6216 Destroy \u6642\u6f0f\u5224\u3002")]
    [SerializeField] private float checkInterval = 0.25f;

    [Header("\u901a\u95dc\u986f\u793a")]
    [Tooltip("\u64ca\u6557\u6240\u6709\u6575\u4eba\u5f8c\u986f\u793a\u7684\u6587\u5b57\u3002")]
    [SerializeField] private string clearMessage = "\u606d\u559c\u901a\u95dc";

    [Tooltip("\u901a\u95dc\u5f8c\u662f\u5426\u81ea\u52d5\u91cd\u65b0\u958b\u59cb\u76ee\u524d\u5834\u666f\u3002")]
    [SerializeField] private bool restartAfterClear;

    [Tooltip("\u901a\u95dc\u6587\u5b57\u986f\u793a\u5e7e\u79d2\u5f8c\u91cd\u65b0\u958b\u59cb\u904a\u6232\u3002")]
    [SerializeField] private float restartDelay = 5f;

    [Tooltip("\u901a\u95dc\u6587\u5b57\u984f\u8272\u3002")]
    [SerializeField] private Color messageColor = new Color(1f, 0.92f, 0.35f, 1f);

    [Tooltip("\u901a\u95dc\u6587\u5b57\u80cc\u666f\u984f\u8272\u3002")]
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
