using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SideScrollerPrototypeBuilder
{
    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";
    private const string HitEffectPrefabPath = "Assets/Art/Prefab/FX/FX_Hit.prefab";

    [MenuItem("Tools/3D 遊戲工具/建立原型場景")]
    public static void CreatePrototypeScene()
    {
        EnsureFolder("Assets/SideScroller3D");
        EnsureFolder("Assets/SideScroller3D/Scenes");
        EnsureFolder("Assets/SideScroller3D/Materials");
        EnsureLayer("Player");
        EnsureLayer("Enemy");
        EnsureLayer("Ground");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material groundMaterial = CreateMaterial("Ground_Mat", new Color(0.18f, 0.19f, 0.2f));
        Material playerMaterial = CreateMaterial("Player_Mat", new Color(0.18f, 0.45f, 0.9f));
        Material enemyMaterial = CreateMaterial("Enemy_Mat", new Color(0.75f, 0.16f, 0.12f));
        Material platformMaterial = CreateMaterial("Platform_Mat", new Color(0.28f, 0.25f, 0.22f));

        GameObject ground = CreateCube("Ground", new Vector3(8f, -0.5f, 0f), new Vector3(18f, 1f, 4f), groundMaterial);
        ground.layer = LayerMask.NameToLayer("Ground");

        CreatePlatform("Platform_A", new Vector3(20f, 1.4f, 0f), new Vector3(5f, 0.5f, 4f), platformMaterial);
        CreatePlatform("Platform_B", new Vector3(30f, 3.1f, 0f), new Vector3(5f, 0.5f, 4f), platformMaterial);
        CreatePlatform("Upper_Walkway", new Vector3(43f, 0.2f, 0f), new Vector3(18f, 0.6f, 4f), platformMaterial);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(0f, 1.25f, 0f);
        player.GetComponent<Renderer>().sharedMaterial = playerMaterial;

        Rigidbody playerBody = player.AddComponent<Rigidbody>();
        playerBody.interpolation = RigidbodyInterpolation.Interpolate;
        playerBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Health playerHealth = player.AddComponent<Health>();
        PlayerMotor3D motor = player.AddComponent<PlayerMotor3D>();
        PlayerCombat3D combat = player.AddComponent<PlayerCombat3D>();
        player.AddComponent<SideScrollerPlayerRespawn>();

        Transform groundCheck = CreateChild(player.transform, "GroundCheck", new Vector3(0f, -1.05f, 0f));
        Transform weaponAnchor = CreateChild(player.transform, "WeaponAnchor", new Vector3(1.23f, 0.25f, 0f));
        PlayerWeaponHitbox weaponHitbox = CreateStarterWeapon(weaponAnchor);

        SetField(motor, "groundCheck", groundCheck);
        SetField(motor, "groundMask", LayerMask.GetMask("Ground"));
        SetField(combat, "weaponHitbox", weaponHitbox);
        SetField(combat, "enemyMask", LayerMask.GetMask("Enemy"));

        CreateEnemy("Enemy_A", new Vector3(12f, 1f, 0f), 9f, 15f, enemyMaterial);
        CreateEnemy("Enemy_B", new Vector3(27f, 4.35f, 0f), 24.5f, 31f, enemyMaterial);
        CreateEnemy("Enemy_C", new Vector3(44f, 1.5f, 0f), 37f, 50f, enemyMaterial);

        CreateCamera(player.transform);
        CreateLight();
        CreateHUD(playerHealth);

        Selection.activeGameObject = player;
        EditorSceneManager.SaveScene(scene, ScenePath);

        string message = $"Prototype scene saved at: {ScenePath}. Press Play to test movement, jump, attack, damage, and camera follow.";
        if (Application.isBatchMode)
        {
            Debug.Log(message);
        }
        else
        {
            EditorUtility.DisplayDialog("Prototype Created", message, "OK");
        }
    }

    private static void CreatePlatform(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject platform = CreateCube(name, position, scale, material);
        platform.layer = LayerMask.NameToLayer("Ground");
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static void CreateEnemy(string name, Vector3 position, float leftX, float rightX, Material material)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.transform.position = position;
        enemy.GetComponent<Renderer>().sharedMaterial = material;

        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;

        enemy.AddComponent<Health>();
        enemy.AddComponent<EnemyHealthBar3D>();
        enemy.AddComponent<EnemyDamageFlash>();
        ConfigureEnemyHitEffect(enemy);
        enemy.AddComponent<EnemyVisualAnimator>();
        CreateEnemyHurtbox(enemy.transform);
        EnemyPatrol3D patrol = enemy.AddComponent<EnemyPatrol3D>();
        DamageOnTouch touchDamage = enemy.AddComponent<DamageOnTouch>();

        Transform leftPoint = CreateChild(enemy.transform, "LeftPoint", new Vector3(leftX - position.x, 0f, 0f));
        Transform rightPoint = CreateChild(enemy.transform, "RightPoint", new Vector3(rightX - position.x, 0f, 0f));

        SetField(patrol, "leftPoint", leftPoint);
        SetField(patrol, "rightPoint", rightPoint);
        SetField(touchDamage, "targetMask", LayerMask.GetMask("Player"));
    }

    private static void ConfigureEnemyHitEffect(GameObject enemy)
    {
        DamageHitEffect3D hitEffect = enemy.AddComponent<DamageHitEffect3D>();
        Transform hitAnchor = CreateChild(enemy.transform, "EF_Hit", new Vector3(0f, 0.95f, 0f));
        hitEffect.SetEffectAnchor(hitAnchor);

        GameObject effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HitEffectPrefabPath);
        if (effectPrefab != null)
        {
            hitEffect.SetEffectPrefab(effectPrefab);
        }
    }

    private static void CreateEnemyHurtbox(Transform enemy)
    {
        GameObject hurtboxObject = new GameObject("Enemy_Hurtbox");
        hurtboxObject.layer = LayerMask.NameToLayer("Enemy");
        hurtboxObject.transform.SetParent(enemy, false);
        hurtboxObject.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        hurtboxObject.transform.localRotation = Quaternion.identity;
        hurtboxObject.transform.localScale = new Vector3(1.15f, 1.8f, 0.9f);

        EnemyHurtbox3D hurtbox = hurtboxObject.AddComponent<EnemyHurtbox3D>();
        hurtbox.RefreshParts();
    }

    private static PlayerWeaponHitbox CreateStarterWeapon(Transform weaponAnchor)
    {
        GameObject weapon = new GameObject("Starter_Weapon");
        weapon.transform.SetParent(weaponAnchor, false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        PlayerWeaponHitbox hitbox = weapon.AddComponent<PlayerWeaponHitbox>();
        weapon.AddComponent<PlayerWeaponAttackProfile>();
        hitbox.SetWorldSize(new Vector3(1.35f, 0.16f, 0.16f));
        hitbox.Configure(1, LayerMask.GetMask("Enemy"));
        return hitbox;
    }

    private static Camera CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 55f;
        cameraObject.transform.position = new Vector3(0f, 3f, -12f);
        cameraObject.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        SideScrollerCamera follow = cameraObject.AddComponent<SideScrollerCamera>();
        SetField(follow, "target", target);
        SetField(follow, "xBounds", new Vector2(-3f, 55f));
        SetField(follow, "yBounds", new Vector2(0f, 8f));
        SetField(follow, "zBounds", new Vector2(-3f, 55f));

        return camera;
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateHUD(Health playerHealth)
    {
        GameObject canvasObject = new GameObject("HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject sliderObject = new GameObject("Health Slider");
        sliderObject.transform.SetParent(canvasObject.transform, false);
        Slider slider = sliderObject.AddComponent<Slider>();

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

        slider.targetGraphic = fill.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.transition = Selectable.Transition.None;
        slider.direction = Slider.Direction.LeftToRight;

        SideScrollerHUD hud = canvasObject.AddComponent<SideScrollerHUD>();
        SetField(hud, "playerHealth", playerHealth);
        SetField(hud, "healthSlider", slider);
    }

    private static GameObject CreateUIBlock(string name, Transform parent, Color color)
    {
        GameObject block = new GameObject(name);
        block.transform.SetParent(parent, false);
        Image image = block.AddComponent<Image>();
        image.color = color;
        return block;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        child.transform.localPosition = localPosition;
        return child.transform;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"Assets/SideScroller3D/Materials/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static void EnsureLayer(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) >= 0)
        {
            return;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }

        Debug.LogWarning($"No empty layer slot found for {layerName}. Please create it manually.");
    }

    private static void SetField(Object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
        {
            Debug.LogWarning($"Field {fieldName} not found on {target.GetType().Name}.");
            return;
        }

        if (field.FieldType == typeof(LayerMask) && value is int layerMaskValue)
        {
            field.SetValue(target, new LayerMask { value = layerMaskValue });
        }
        else
        {
            field.SetValue(target, value);
        }

        EditorUtility.SetDirty(target);
    }
}
