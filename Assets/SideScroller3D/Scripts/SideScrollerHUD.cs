using UnityEngine;
using UnityEngine.UI;

// Manages the player's health UI and the basic control guide.
public class SideScrollerHUD : MonoBehaviour
{
    [Header("\u751f\u547d\u503c UI")]
    [Tooltip("\u8981\u986f\u793a\u751f\u547d\u503c\u7684\u73a9\u5bb6 Health\u3002")]
    [SerializeField] private Health playerHealth;

    [Tooltip("\u73a9\u5bb6\u751f\u547d\u503c\u6ed1\u687f\u3002")]
    [SerializeField] private Slider healthSlider;

    [Header("\u64cd\u4f5c\u8aaa\u660e")]
    [Tooltip("\u662f\u5426\u5728\u756b\u9762\u53f3\u4e0a\u89d2\u986f\u793a\u64cd\u4f5c\u8aaa\u660e\u3002")]
    [SerializeField] private bool showControlGuide = true;

    [Tooltip("\u53f3\u4e0a\u89d2\u64cd\u4f5c\u8aaa\u660e\u6587\u5b57\u3002")]
    [TextArea(4, 8)]
    [SerializeField] private string controlGuide =
        "\u64cd\u4f5c\u8aaa\u660e\n" +
        "A / D\uff1a\u5de6\u53f3\u79fb\u52d5\n" +
        "Shift\uff1a\u885d\u523a\n" +
        "Space\uff1a\u8df3\u8e8d\n" +
        "\u6ed1\u9f20\u5de6\u9375 / J / Ctrl\uff1a\u653b\u64ca\n" +
        "\u653b\u64ca\u77f3\u982d\uff1a\u53cd\u64ca\n" +
        "\u706b\u7403\uff1a\u7121\u6cd5\u53cd\u64ca";

    [Tooltip("\u64cd\u4f5c\u8aaa\u660e\u6846\u8ddd\u96e2\u756b\u9762\u53f3\u4e0a\u89d2\u7684\u504f\u79fb\u3002X \u5f80\u5de6\uff0cY \u5f80\u4e0b\u3002")]
    [SerializeField] private Vector2 guideOffset = new Vector2(-16f, -16f);

    [Tooltip("\u64cd\u4f5c\u8aaa\u660e\u6846\u7684\u5927\u5c0f\u3002")]
    [SerializeField] private Vector2 guideSize = new Vector2(230f, 170f);

    [Tooltip("\u64cd\u4f5c\u8aaa\u660e\u6587\u5b57\u984f\u8272\u3002")]
    [SerializeField] private Color guideTextColor = new Color(1f, 1f, 1f, 1f);

    [Tooltip("\u64cd\u4f5c\u8aaa\u660e\u80cc\u666f\u984f\u8272\u3002")]
    [SerializeField] private Color guideBackgroundColor = new Color(0f, 0f, 0f, 0.68f);

    [Header("\u6b7b\u4ea1\u63d0\u793a")]
    [Tooltip("\u73a9\u5bb6\u6b7b\u4ea1\u6642\uff0c\u756b\u9762\u4e2d\u592e\u986f\u793a\u7684\u6587\u5b57\u3002")]
    [SerializeField] private string gameOverMessage = "GAME OVER";

    [Tooltip("\u6b7b\u4ea1\u63d0\u793a\u6587\u5b57\u984f\u8272\u3002")]
    [SerializeField] private Color gameOverTextColor = new Color(1f, 0.08f, 0.04f, 1f);

    private const string ControlGuideName = "Controls Guide";
    private const string GameOverTextName = "Game Over Text";
    private const string HudCanvasName = "HUD Canvas";
    private const string HealthSliderName = "Health Slider";

    private Health subscribedHealth;
    private Text gameOverText;

    public static SideScrollerHUD EnsureRuntimeHud(Health playerHealth)
    {
        if (playerHealth == null)
        {
            return null;
        }

        SideScrollerHUD hud = FindFirstObjectByType<SideScrollerHUD>();
        if (hud == null)
        {
            GameObject canvasObject = new GameObject(HudCanvasName);
            hud = canvasObject.AddComponent<SideScrollerHUD>();
        }

        hud.playerHealth = playerHealth;
        hud.EnsureCanvas();
        hud.EnsureHealthSlider();
        hud.EnsureControlGuide();
        hud.EnsureGameOverText();
        hud.BindPlayerHealth();
        return hud;
    }

    private void Awake()
    {
        EnsureCanvas();
        EnsurePlayerHealth();
        EnsureHealthSlider();
        EnsureControlGuide();
        EnsureGameOverText();
    }

    private void OnEnable()
    {
        EnsureCanvas();
        EnsurePlayerHealth();
        EnsureHealthSlider();
        EnsureControlGuide();
        EnsureGameOverText();
        BindPlayerHealth();
    }

    private void OnDisable()
    {
        UnbindPlayerHealth();
    }

    private void BindPlayerHealth()
    {
        if (subscribedHealth == playerHealth)
        {
            if (playerHealth != null)
            {
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            return;
        }

        UnbindPlayerHealth();
        subscribedHealth = playerHealth;
        if (subscribedHealth == null)
        {
            return;
        }

        subscribedHealth.Changed += UpdateHealth;
        subscribedHealth.Died += ShowGameOver;
        UpdateHealth(subscribedHealth.CurrentHealth, subscribedHealth.MaxHealth);
        SetGameOverVisible(subscribedHealth.IsDead);
    }

    private void UnbindPlayerHealth()
    {
        if (subscribedHealth != null)
        {
            subscribedHealth.Changed -= UpdateHealth;
            subscribedHealth.Died -= ShowGameOver;
            subscribedHealth = null;
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = max;
        healthSlider.value = current;
        SetGameOverVisible(current <= 0);
    }

    private void EnsureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void EnsurePlayerHealth()
    {
        if (playerHealth != null)
        {
            return;
        }

        PlayerMotor3D player = FindFirstObjectByType<PlayerMotor3D>();
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInParent<Health>();
            }
        }
    }

    private void EnsureHealthSlider()
    {
        if (healthSlider != null)
        {
            return;
        }

        Transform existing = transform.Find(HealthSliderName);
        if (existing != null)
        {
            healthSlider = existing.GetComponent<Slider>();
            if (healthSlider != null)
            {
                return;
            }
        }

        GameObject sliderObject = new GameObject(HealthSliderName);
        sliderObject.transform.SetParent(transform, false);
        healthSlider = sliderObject.AddComponent<Slider>();

        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -24f);
        rect.sizeDelta = new Vector2(220f, 24f);

        GameObject background = CreateUIBlock("Background", sliderObject.transform, new Color(0.12f, 0.12f, 0.12f, 0.9f));
        StretchToParent(background.GetComponent<RectTransform>());

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        StretchToParent(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fill = CreateUIBlock("Fill", fillArea.transform, new Color(0.82f, 0.12f, 0.1f, 1f));
        StretchToParent(fill.GetComponent<RectTransform>());

        healthSlider.targetGraphic = fill.GetComponent<Image>();
        healthSlider.fillRect = fill.GetComponent<RectTransform>();
        healthSlider.transition = Selectable.Transition.None;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0f;
    }

    private void EnsureControlGuide()
    {
        Transform existing = transform.Find(ControlGuideName);
        if (!showControlGuide)
        {
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
            }

            return;
        }

        GameObject guideObject = existing != null ? existing.gameObject : CreateControlGuideObject();
        guideObject.SetActive(true);

        Image background = guideObject.GetComponent<Image>();
        if (background == null)
        {
            background = guideObject.AddComponent<Image>();
        }

        background.color = guideBackgroundColor;
        background.raycastTarget = false;

        RectTransform rect = guideObject.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = guideOffset;
        rect.sizeDelta = guideSize;

        Text guideText = guideObject.GetComponentInChildren<Text>(true);
        if (guideText == null)
        {
            guideText = CreateGuideText(guideObject.transform);
        }

        guideText.text = controlGuide;
        guideText.color = guideTextColor;
        guideText.font = ResolveFont();
        guideText.fontSize = 15;
        guideText.lineSpacing = 1.05f;
        guideText.alignment = TextAnchor.UpperLeft;
        guideText.horizontalOverflow = HorizontalWrapMode.Wrap;
        guideText.verticalOverflow = VerticalWrapMode.Overflow;
        guideText.raycastTarget = false;

        RectTransform textRect = guideText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 8f);
        textRect.offsetMax = new Vector2(-10f, -8f);
    }

    private GameObject CreateControlGuideObject()
    {
        GameObject guideObject = new GameObject(ControlGuideName);
        guideObject.transform.SetParent(transform, false);

        Image background = guideObject.AddComponent<Image>();
        background.raycastTarget = false;

        CreateGuideText(guideObject.transform);

        return guideObject;
    }

    private void EnsureGameOverText()
    {
        if (gameOverText == null)
        {
            Transform existing = transform.Find(GameOverTextName);
            if (existing != null)
            {
                gameOverText = existing.GetComponent<Text>();
            }
        }

        if (gameOverText == null)
        {
            GameObject textObject = new GameObject(GameOverTextName);
            textObject.transform.SetParent(transform, false);
            gameOverText = textObject.AddComponent<Text>();
            gameOverText.raycastTarget = false;
        }

        gameOverText.text = gameOverMessage;
        gameOverText.color = gameOverTextColor;
        gameOverText.font = ResolveFont();
        gameOverText.fontSize = 72;
        gameOverText.fontStyle = FontStyle.Bold;
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.horizontalOverflow = HorizontalWrapMode.Overflow;
        gameOverText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = gameOverText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 160f);

        SetGameOverVisible(playerHealth != null && playerHealth.IsDead);
    }

    private void ShowGameOver()
    {
        SetGameOverVisible(true);
    }

    private void SetGameOverVisible(bool visible)
    {
        if (gameOverText == null)
        {
            return;
        }

        gameOverText.gameObject.SetActive(visible);
    }

    private static GameObject CreateUIBlock(string name, Transform parent, Color color)
    {
        GameObject block = new GameObject(name);
        block.transform.SetParent(parent, false);
        Image image = block.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return block;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text CreateGuideText(Transform parent)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.raycastTarget = false;
        text.font = ResolveFont();
        return text;
    }

    private static Font ResolveFont()
    {
        Font font = Font.CreateDynamicFontFromOSFont(
            new[] { "Microsoft JhengHei", "Microsoft YaHei", "PMingLiU", "Arial" },
            16);
        if (font != null)
        {
            return font;
        }

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return font;
    }
}
