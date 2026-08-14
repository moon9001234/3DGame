using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class BossClearOnDeath3D : MonoBehaviour
{
    private const string CanvasName = "Boss Clear Canvas";
    private const string MessageName = "Boss Clear Message";
    private const string LegacyClearCanvasName = "Game Clear Canvas";

    [Header("Clear UI")]
    [SerializeField] private string clearMessage = "\u606d\u559c\u901a\u95dc";
    [SerializeField] private Color messageColor = new Color(1f, 0.92f, 0.35f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.62f);
    [SerializeField] private int fontSize = 64;
    [SerializeField] private Vector2 panelSize = new Vector2(520f, 150f);

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnClear;

    private Health health;
    private Canvas clearCanvas;
    private Text clearText;
    private Image background;
    private bool shown;

    public static bool HasShownClear { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetClearState()
    {
        HasShownClear = false;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        EnsureClearUI();
    }

    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.Died += ShowClear;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= ShowClear;
        }
    }

    private void OnValidate()
    {
        fontSize = Mathf.Max(12, fontSize);
        panelSize = new Vector2(Mathf.Max(120f, panelSize.x), Mathf.Max(60f, panelSize.y));
        if (Application.isPlaying && clearCanvas != null)
        {
            ApplyUiSettings();
        }
    }

    public void ShowClear()
    {
        if (shown)
        {
            return;
        }

        shown = true;
        HasShownClear = true;
        EnsureClearUI();
        ApplyUiSettings();
        HideLegacyClearCanvas();
        clearCanvas.gameObject.SetActive(true);

        if (pauseGameOnClear)
        {
            Time.timeScale = 0f;
        }
    }

    private static void HideLegacyClearCanvas()
    {
        GameObject legacyCanvas = GameObject.Find(LegacyClearCanvasName);
        if (legacyCanvas != null)
        {
            legacyCanvas.SetActive(false);
        }
    }

    private void EnsureClearUI()
    {
        if (clearCanvas != null && clearText != null)
        {
            return;
        }

        GameObject canvasObject = GameObject.Find(CanvasName);
        if (canvasObject == null)
        {
            canvasObject = new GameObject(CanvasName);
        }

        clearCanvas = canvasObject.GetComponent<Canvas>();
        if (clearCanvas == null)
        {
            clearCanvas = canvasObject.AddComponent<Canvas>();
        }

        clearCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        clearCanvas.sortingOrder = 1200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        Transform panelTransform = canvasObject.transform.Find(MessageName);
        GameObject panelObject = panelTransform != null ? panelTransform.gameObject : new GameObject(MessageName);
        panelObject.transform.SetParent(canvasObject.transform, false);

        background = panelObject.GetComponent<Image>();
        if (background == null)
        {
            background = panelObject.AddComponent<Image>();
        }

        Transform textTransform = panelObject.transform.Find("Text");
        GameObject textObject = textTransform != null ? textTransform.gameObject : new GameObject("Text");
        textObject.transform.SetParent(panelObject.transform, false);

        clearText = textObject.GetComponent<Text>();
        if (clearText == null)
        {
            clearText = textObject.AddComponent<Text>();
        }

        ApplyUiSettings();
        clearCanvas.gameObject.SetActive(shown);
    }

    private void ApplyUiSettings()
    {
        if (background != null)
        {
            background.color = backgroundColor;
            background.raycastTarget = false;

            RectTransform panelRect = background.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = panelSize;
        }

        if (clearText == null)
        {
            return;
        }

        clearText.text = clearMessage;
        clearText.color = messageColor;
        clearText.font = ResolveFont();
        clearText.fontSize = fontSize;
        clearText.fontStyle = FontStyle.Bold;
        clearText.alignment = TextAnchor.MiddleCenter;
        clearText.horizontalOverflow = HorizontalWrapMode.Wrap;
        clearText.verticalOverflow = VerticalWrapMode.Overflow;
        clearText.raycastTarget = false;

        RectTransform textRect = clearText.GetComponent<RectTransform>();
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
