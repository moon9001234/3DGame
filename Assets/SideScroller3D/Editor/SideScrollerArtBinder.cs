using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SideScrollerArtBinder
{
    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";
    private const string PlayerFbxPath = "Assets/Art/FBX/Player/TV_Man.fbx";
    private const string EnemyFbxPath = "Assets/Art/FBX/Monster/TV_Monster_01.fbx";
    private const string RangedEnemyFbxPath = "Assets/Art/FBX/Monster/TV_Monster_03.fbx";
    private const string WeaponPrefabPath = "Assets/Art/Prefab/Weapon/TV_Weapon_05.prefab";
    private const string HitEffectPrefabPath = "Assets/Art/Prefab/FX/FX_Hit.prefab";
    private const string AnimationFolder = "Assets/SideScroller3D/Animation";
    private const string PrefabFolder = "Assets/SideScroller3D/Prefabs";
    private const string PlayerControllerPath = AnimationFolder + "/PlayerVisual.controller";
    private const string EnemyControllerPath = AnimationFolder + "/EnemyVisual.controller";
    private const string RangedEnemyControllerPath = AnimationFolder + "/RangedEnemyVisual.controller";
    private const string PlayerVisualPrefabPath = PrefabFolder + "/Player_Model.prefab";
    private const string EnemyVisualPrefabPath = PrefabFolder + "/Enemy_Model.prefab";
    private const string RangedEnemyVisualPrefabPath = PrefabFolder + "/Ranged_Enemy_Model.prefab";
    private const string PlayerPrefabPath = PrefabFolder + "/Player.prefab";
    private const string EnemyPrefabPath = PrefabFolder + "/Enemy_Monster01.prefab";
    private const string RangedEnemyPrefabPath = PrefabFolder + "/Enemy_Monster03_Ranged.prefab";

    [MenuItem("Tools/3D 遊戲工具/套用美術模型")]
    public static void ApplyArtModels()
    {
        EnsureFolder("Assets/SideScroller3D");
        EnsureFolder(AnimationFolder);
        EnsureFolder(PrefabFolder);

        AnimatorController playerController = CreatePlayerController();
        AnimatorController enemyController = CreateEnemyController();
        AnimatorController rangedEnemyController = CreateRangedEnemyController();

        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            WeaponTransformSnapshot weaponSnapshot = CapturePlayerWeapon(player);
            EnsureVisualPrefab(PlayerVisualPrefabPath, PlayerFbxPath, playerController, "Player_Model", 2.05f, new Vector3(0f, -1f, 0f), 90f);
            BindVisual(player, PlayerVisualPrefabPath, playerController, "Player_Model");
            EnsurePlayerWeapon(player, weaponSnapshot);
            RemovePlayerHitEffect(player);
        }
        else
        {
            Debug.LogWarning("Player not found in Prototype scene.");
        }

        foreach (EnemyPatrol3D enemy in Object.FindObjectsByType<EnemyPatrol3D>(FindObjectsSortMode.None))
        {
            if (enemy.Mode == EnemyPatrol3D.AttackMode.Ranged)
            {
                continue;
            }

            EffectAnchorSnapshot hitAnchorSnapshot = CaptureEnemyHitAnchor(enemy.gameObject);
            EnsureVisualPrefab(EnemyVisualPrefabPath, EnemyFbxPath, enemyController, "Enemy_Model", 1.85f, new Vector3(0f, -0.95f, 0f), 90f);
            BindVisual(enemy.gameObject, EnemyVisualPrefabPath, enemyController, "Enemy_Model");
            Transform hitAnchor = EnsureEnemyHitAnchor(enemy.gameObject, hitAnchorSnapshot);

            if (enemy.GetComponent<EnemyVisualAnimator>() == null)
            {
                enemy.gameObject.AddComponent<EnemyVisualAnimator>();
            }

            if (enemy.GetComponent<EnemyHealthBar3D>() == null)
            {
                enemy.gameObject.AddComponent<EnemyHealthBar3D>();
            }

            if (enemy.GetComponent<EnemyDamageFlash>() == null)
            {
                enemy.gameObject.AddComponent<EnemyDamageFlash>();
            }

            EnsureEnemyHitEffect(enemy.gameObject, hitAnchor);
            EnsureEnemyHurtbox(enemy.gameObject);
        }

        foreach (EnemyPatrol3D enemy in Object.FindObjectsByType<EnemyPatrol3D>(FindObjectsSortMode.None))
        {
            if (enemy.Mode != EnemyPatrol3D.AttackMode.Ranged)
            {
                continue;
            }

            EffectAnchorSnapshot hitAnchorSnapshot = CaptureEnemyHitAnchor(enemy.gameObject);
            EffectAnchorSnapshot projectileSpawnSnapshot = CaptureProjectileSpawn(enemy.gameObject);
            EnsureVisualPrefab(RangedEnemyVisualPrefabPath, RangedEnemyFbxPath, rangedEnemyController, "Enemy_Model", 1.85f, new Vector3(0f, -0.95f, 0f), 90f);
            BindVisual(enemy.gameObject, RangedEnemyVisualPrefabPath, rangedEnemyController, "Enemy_Model");
            Transform hitAnchor = EnsureEnemyHitAnchor(enemy.gameObject, hitAnchorSnapshot);

            if (enemy.GetComponent<EnemyVisualAnimator>() == null)
            {
                enemy.gameObject.AddComponent<EnemyVisualAnimator>();
            }

            if (enemy.GetComponent<EnemyHealthBar3D>() == null)
            {
                enemy.gameObject.AddComponent<EnemyHealthBar3D>();
            }

            if (enemy.GetComponent<EnemyDamageFlash>() == null)
            {
                enemy.gameObject.AddComponent<EnemyDamageFlash>();
            }

            EnsureEnemyHitEffect(enemy.gameObject, hitAnchor);
            EnsureEnemyHurtbox(enemy.gameObject);
            EnsureProjectileSpawn(enemy.gameObject, projectileSpawnSnapshot);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Applied TV_Man to Player, TV_Monster_01 to melee enemies, and TV_Monster_03 to ranged enemies in Prototype scene.");
    }

    [MenuItem("Tools/3D 遊戲工具/從場景建立角色 Prefab")]
    public static void CreateCharacterPrefabsFromScene()
    {
        EnsureFolder("Assets/SideScroller3D");
        EnsureFolder(AnimationFolder);
        EnsureFolder(PrefabFolder);

        AnimatorController playerController = CreatePlayerController();
        AnimatorController enemyController = CreateEnemyController();
        AnimatorController rangedEnemyController = CreateRangedEnemyController();

        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            EnsurePlayerWeapon(player);
            RemovePlayerHitEffect(player);
        }

        Transform playerVisual = player != null ? player.transform.Find("Player_Model") : null;
        if (playerVisual != null)
        {
            SaveVisualPrefab(playerVisual.gameObject, PlayerVisualPrefabPath, playerController);
        }
        else
        {
            CreateVisualPrefabFromFbx(PlayerVisualPrefabPath, PlayerFbxPath, playerController, "Player_Model", 2.05f, new Vector3(0f, -1f, 0f), 90f);
        }

        if (player != null)
        {
            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
        }

        EnemyPatrol3D enemy = Object.FindObjectsByType<EnemyPatrol3D>(FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.Mode == EnemyPatrol3D.AttackMode.Melee);
        Transform enemyVisual = enemy != null ? enemy.transform.Find("Enemy_Model") : null;
        if (enemyVisual != null)
        {
            SaveVisualPrefab(enemyVisual.gameObject, EnemyVisualPrefabPath, enemyController);
        }
        else
        {
            CreateVisualPrefabFromFbx(EnemyVisualPrefabPath, EnemyFbxPath, enemyController, "Enemy_Model", 1.85f, new Vector3(0f, -0.95f, 0f), 90f);
        }

        if (enemy != null)
        {
            Transform hitAnchor = EnsureEnemyHitAnchor(enemy.gameObject, new EffectAnchorSnapshot());
            EnsureEnemyHitEffect(enemy.gameObject, hitAnchor);
            EnsureEnemyHurtbox(enemy.gameObject);
            PrefabUtility.SaveAsPrefabAsset(enemy.gameObject, EnemyPrefabPath);
        }

        EnemyPatrol3D rangedEnemy = Object.FindObjectsByType<EnemyPatrol3D>(FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.Mode == EnemyPatrol3D.AttackMode.Ranged);
        Transform rangedVisual = rangedEnemy != null ? rangedEnemy.transform.Find("Enemy_Model") : null;
        if (rangedVisual != null)
        {
            SaveVisualPrefab(rangedVisual.gameObject, RangedEnemyVisualPrefabPath, rangedEnemyController);
        }
        else
        {
            CreateVisualPrefabFromFbx(RangedEnemyVisualPrefabPath, RangedEnemyFbxPath, rangedEnemyController, "Enemy_Model", 1.85f, new Vector3(0f, -0.95f, 0f), 90f);
        }

        if (rangedEnemy != null)
        {
            Transform hitAnchor = EnsureEnemyHitAnchor(rangedEnemy.gameObject, new EffectAnchorSnapshot());
            EnsureEnemyHitEffect(rangedEnemy.gameObject, hitAnchor);
            EnsureEnemyHurtbox(rangedEnemy.gameObject);
            EnsureProjectileSpawn(rangedEnemy.gameObject);
            PrefabUtility.SaveAsPrefabAsset(rangedEnemy.gameObject, RangedEnemyPrefabPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Character prefabs saved:\n{PlayerVisualPrefabPath}\n{EnemyVisualPrefabPath}\n{RangedEnemyVisualPrefabPath}\n{PlayerPrefabPath}\n{EnemyPrefabPath}\n{RangedEnemyPrefabPath}");
    }

    [MenuItem("Tools/3D 遊戲工具/在場景建立遠程敵人")]
    public static void CreateRangedEnemyInScene()
    {
        EnsureFolder("Assets/SideScroller3D");
        EnsureFolder(AnimationFolder);
        EnsureFolder(PrefabFolder);

        AnimatorController rangedEnemyController = CreateRangedEnemyController();
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Ranged_Enemy";
        enemy.layer = GetLayerOrDefault("Enemy");
        enemy.transform.position = ResolveRangedEnemySpawnPosition();

        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        enemy.AddComponent<Health>();
        enemy.AddComponent<EnemyHealthBar3D>();
        enemy.AddComponent<EnemyDamageFlash>();
        enemy.AddComponent<EnemyVisualAnimator>();
        EnemyPatrol3D enemyAI = enemy.AddComponent<EnemyPatrol3D>();
        enemyAI.SetAttackMode(EnemyPatrol3D.AttackMode.Ranged);

        EnsureVisualPrefab(RangedEnemyVisualPrefabPath, RangedEnemyFbxPath, rangedEnemyController, "Enemy_Model", 1.85f, new Vector3(0f, -0.95f, 0f), 90f);
        BindVisual(enemy, RangedEnemyVisualPrefabPath, rangedEnemyController, "Enemy_Model");
        Transform hitAnchor = EnsureEnemyHitAnchor(enemy, new EffectAnchorSnapshot());
        EnsureEnemyHitEffect(enemy, hitAnchor);
        EnsureEnemyHurtbox(enemy);
        EnsureProjectileSpawn(enemy);

        Selection.activeGameObject = enemy;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Created a TV_Monster_03 ranged enemy. Press Play and enter its search range to test fireball reflection.");
    }

    private static void BindVisual(GameObject root, string prefabPath, RuntimeAnimatorController controller, string childName)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (asset == null)
        {
            Debug.LogError($"Missing visual prefab: {prefabPath}");
            return;
        }

        Transform existing = root.transform.Find(childName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        Renderer rootRenderer = root.GetComponent<Renderer>();
        if (rootRenderer != null)
        {
            rootRenderer.enabled = false;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
        instance.name = childName;

        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
    }

    private static void EnsureVisualPrefab(string prefabPath, string fbxPath, RuntimeAnimatorController controller, string prefabName, float targetHeight, Vector3 localPosition, float yRotation)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            return;
        }

        CreateVisualPrefabFromFbx(prefabPath, fbxPath, controller, prefabName, targetHeight, localPosition, yRotation);
    }

    private static void CreateVisualPrefabFromFbx(string prefabPath, string fbxPath, RuntimeAnimatorController controller, string prefabName, float targetHeight, Vector3 localPosition, float yRotation)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (asset == null)
        {
            Debug.LogError($"Missing model asset: {fbxPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.name = prefabName;
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        instance.transform.localScale = Vector3.one;

        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        FitToHeight(instance.transform, targetHeight);

        SaveVisualPrefab(instance, prefabPath, controller);
        Object.DestroyImmediate(instance);
    }

    private static void SaveVisualPrefab(GameObject visual, string prefabPath, RuntimeAnimatorController controller)
    {
        Animator animator = visual.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        PrefabUtility.SaveAsPrefabAsset(visual, prefabPath);
    }

    private static void EnsurePlayerWeapon(GameObject player)
    {
        EnsurePlayerWeapon(player, new WeaponTransformSnapshot());
    }

    private static void EnsurePlayerWeapon(GameObject player, WeaponTransformSnapshot weaponSnapshot)
    {
        PlayerCombat3D combat = player.GetComponent<PlayerCombat3D>();
        if (combat == null)
        {
            return;
        }

        Transform weaponParent = ResolvePlayerWeaponAnchor(player);

        PlayerWeaponHitbox hitbox = player.GetComponentInChildren<PlayerWeaponHitbox>(true);
        Transform weapon = hitbox != null ? hitbox.transform : null;
        bool createdWeapon = false;
        bool movedToWeaponParent = false;
        bool hadWeaponModel = weapon != null && weapon.Find("TV_Weapon_05") != null;
        bool wasUsingDefaultOriginPose = weapon != null
            && Vector3.Distance(weapon.localPosition, new Vector3(0.78f, 0f, 0f)) <= 0.001f
            && Quaternion.Angle(weapon.localRotation, Quaternion.identity) <= 0.1f;

        if (weapon == null)
        {
            GameObject weaponObject = new GameObject("Starter_Weapon");
            weaponObject.transform.SetParent(weaponParent, false);
            weapon = weaponObject.transform;
            createdWeapon = true;
        }
        else if (weapon.parent != weaponParent)
        {
            weapon.SetParent(weaponParent, false);
            movedToWeaponParent = true;
        }

        hitbox = weapon.GetComponent<PlayerWeaponHitbox>();
        if (hitbox == null)
        {
            hitbox = weapon.gameObject.AddComponent<PlayerWeaponHitbox>();
        }

        if (weapon.GetComponent<PlayerWeaponAttackProfile>() == null)
        {
            weapon.gameObject.AddComponent<PlayerWeaponAttackProfile>();
        }

        if (weaponSnapshot.HasPose)
        {
            weapon.localPosition = weaponSnapshot.LocalPosition;
            weapon.localRotation = weaponSnapshot.LocalRotation;
            weapon.localScale = weaponSnapshot.LocalScale;
        }
        else if (createdWeapon || wasUsingDefaultOriginPose || movedToWeaponParent)
        {
            weapon.localPosition = Vector3.zero;
            weapon.localRotation = Quaternion.identity;
        }

        bool createdWeaponModel = EnsurePlayerWeaponModel(weapon);
        if (createdWeaponModel && !hadWeaponModel)
        {
            weapon.localScale = Vector3.one;
        }

        hitbox.RefreshParts();
        hitbox.Configure(1, LayerMask.GetMask("Enemy"));
    }

    private static bool EnsurePlayerWeaponModel(Transform weapon)
    {
        Transform existingModel = weapon.Find("TV_Weapon_05");
        if (existingModel != null)
        {
            existingModel.gameObject.SetActive(true);
            return false;
        }

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPrefabPath);
        if (asset == null)
        {
            Debug.LogWarning($"Missing weapon prefab: {WeaponPrefabPath}. Temporary box visual will be used.");
            return false;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, weapon);
        instance.name = "TV_Weapon_05";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        return true;
    }

    private static void RemovePlayerHitEffect(GameObject player)
    {
        DamageHitEffect3D hitEffect = player.GetComponent<DamageHitEffect3D>();
        if (hitEffect != null)
        {
            Object.DestroyImmediate(hitEffect);
        }

        Transform generatedEffect = player.transform.Find("Damage_Hit_Effect");
        if (generatedEffect != null)
        {
            Object.DestroyImmediate(generatedEffect.gameObject);
        }
    }

    private static WeaponTransformSnapshot CapturePlayerWeapon(GameObject player)
    {
        PlayerWeaponHitbox hitbox = player.GetComponentInChildren<PlayerWeaponHitbox>(true);
        if (hitbox == null)
        {
            return new WeaponTransformSnapshot();
        }

        Transform weapon = hitbox.transform;
        return new WeaponTransformSnapshot
        {
            HasPose = true,
            LocalPosition = weapon.localPosition,
            LocalRotation = weapon.localRotation,
            LocalScale = weapon.localScale
        };
    }

    private struct WeaponTransformSnapshot
    {
        public bool HasPose;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    private static Transform ResolvePlayerWeaponAnchor(GameObject player)
    {
        Transform existingAnchor = FindChildByNames(player.transform, "WeaponAnchor", "Weapon Anchor", "weapon_anchor");
        if (existingAnchor != null)
        {
            return existingAnchor;
        }

        Transform parent = player.transform;
        Animator animator = player.GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (rightHand != null)
            {
                parent = rightHand;
            }
        }
        else
        {
            Transform namedHand = FindChildByNames(player.transform, "RightHand", "Right Hand", "mixamorig:RightHand", "Bip001 R Hand", "R_Hand", "Right_Hand", "hand_r");
            if (namedHand != null)
            {
                parent = namedHand;
            }
        }

        GameObject anchor = new GameObject("WeaponAnchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = parent == player.transform ? new Vector3(1.23f, 0.25f, 0f) : Vector3.zero;
        anchor.transform.localRotation = Quaternion.identity;
        return anchor.transform;
    }

    private static Transform FindChildByNames(Transform root, params string[] names)
    {
        foreach (Transform child in root)
        {
            foreach (string name in names)
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            Transform match = FindChildByNames(child, names);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static void EnsureEnemyHurtbox(GameObject enemy)
    {
        EnemyHurtbox3D hurtbox = enemy.GetComponentInChildren<EnemyHurtbox3D>(true);
        Transform hurtboxTransform = hurtbox != null ? hurtbox.transform : enemy.transform.Find("Enemy_Hurtbox");
        bool createdHurtbox = false;

        if (hurtboxTransform == null)
        {
            GameObject hurtboxObject = new GameObject("Enemy_Hurtbox");
            hurtboxObject.transform.SetParent(enemy.transform, false);
            hurtboxTransform = hurtboxObject.transform;
            createdHurtbox = true;
        }

        hurtboxTransform.gameObject.layer = LayerMask.NameToLayer("Enemy");

        if (createdHurtbox)
        {
            hurtboxTransform.localPosition = new Vector3(0f, 0.05f, 0f);
            hurtboxTransform.localRotation = Quaternion.identity;
            hurtboxTransform.localScale = new Vector3(1.15f, 1.8f, 0.9f);
        }

        hurtbox = hurtboxTransform.GetComponent<EnemyHurtbox3D>();
        if (hurtbox == null)
        {
            hurtbox = hurtboxTransform.gameObject.AddComponent<EnemyHurtbox3D>();
        }

        hurtbox.RefreshParts();
    }

    private static void EnsureProjectileSpawn(GameObject enemy)
    {
        EnsureProjectileSpawn(enemy, new EffectAnchorSnapshot());
    }

    private static void EnsureProjectileSpawn(GameObject enemy, EffectAnchorSnapshot snapshot)
    {
        Transform spawn = FindChildByNames(enemy.transform, "Shoot") ?? FindChildByNames(enemy.transform, "ProjectileSpawn");
        bool createdSpawn = false;
        if (spawn == null)
        {
            Transform enemyModel = enemy.transform.Find("Enemy_Model");
            Transform parent = enemyModel != null ? enemyModel : enemy.transform;
            GameObject spawnObject = new GameObject("Shoot");
            spawnObject.transform.SetParent(parent, false);
            spawn = spawnObject.transform;
            createdSpawn = true;
        }

        if (snapshot.HasPose)
        {
            spawn.localPosition = snapshot.LocalPosition;
            spawn.localRotation = snapshot.LocalRotation;
            spawn.localScale = snapshot.LocalScale;
        }
        else if (createdSpawn)
        {
            spawn.localPosition = new Vector3(0.65f, 0.95f, 0f);
            spawn.localRotation = Quaternion.identity;
            spawn.localScale = Vector3.one;
        }
    }

    private static Transform EnsureEnemyHitAnchor(GameObject enemy, EffectAnchorSnapshot snapshot)
    {
        Transform enemyModel = enemy.transform.Find("Enemy_Model");
        Transform parent = enemyModel != null ? enemyModel : enemy.transform;
        Transform anchor = FindChildByNames(parent, "EF_Hit");
        bool createdAnchor = false;

        if (anchor == null)
        {
            GameObject anchorObject = new GameObject("EF_Hit");
            anchorObject.transform.SetParent(parent, false);
            anchor = anchorObject.transform;
            createdAnchor = true;
        }

        if (snapshot.HasPose)
        {
            anchor.localPosition = snapshot.LocalPosition;
            anchor.localRotation = snapshot.LocalRotation;
            anchor.localScale = snapshot.LocalScale;
        }
        else if (createdAnchor)
        {
            anchor.localPosition = new Vector3(0f, 0.95f, 0f);
            anchor.localRotation = Quaternion.identity;
            anchor.localScale = Vector3.one;
        }

        return anchor;
    }

    private static EffectAnchorSnapshot CaptureEnemyHitAnchor(GameObject enemy)
    {
        Transform enemyModel = enemy.transform.Find("Enemy_Model");
        Transform searchRoot = enemyModel != null ? enemyModel : enemy.transform;
        Transform anchor = FindChildByNames(searchRoot, "EF_Hit");
        if (anchor == null)
        {
            return new EffectAnchorSnapshot();
        }

        return new EffectAnchorSnapshot
        {
            HasPose = true,
            LocalPosition = anchor.localPosition,
            LocalRotation = anchor.localRotation,
            LocalScale = anchor.localScale
        };
    }

    private static EffectAnchorSnapshot CaptureProjectileSpawn(GameObject enemy)
    {
        Transform spawn = FindChildByNames(enemy.transform, "Shoot") ?? FindChildByNames(enemy.transform, "ProjectileSpawn");
        if (spawn == null)
        {
            return new EffectAnchorSnapshot();
        }

        return new EffectAnchorSnapshot
        {
            HasPose = true,
            LocalPosition = spawn.localPosition,
            LocalRotation = spawn.localRotation,
            LocalScale = spawn.localScale
        };
    }

    private struct EffectAnchorSnapshot
    {
        public bool HasPose;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    private static void EnsureEnemyHitEffect(GameObject enemy, Transform hitAnchor)
    {
        DamageHitEffect3D hitEffect = enemy.GetComponent<DamageHitEffect3D>();
        if (hitEffect == null)
        {
            hitEffect = enemy.AddComponent<DamageHitEffect3D>();
        }

        hitEffect.SetEffectAnchor(hitAnchor);

        GameObject effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HitEffectPrefabPath);
        if (effectPrefab == null)
        {
            Debug.LogWarning($"Missing hit effect prefab: {HitEffectPrefabPath}. Generated fallback hit effect will be used.");
            return;
        }

        hitEffect.SetEffectPrefab(effectPrefab);
    }

    private static AnimatorController CreatePlayerController()
    {
        AnimatorController controller = GetOrCreateController(PlayerControllerPath);
        ClearController(controller);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Dashing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("InCombat", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack3", AnimatorControllerParameterType.Trigger);

        Dictionary<string, AnimationClip> clips = LoadClips(PlayerFbxPath);
        AnimatorState idle = AddState(controller, "Idle", clips, "Player_Idle", new Vector3(260f, 80f, 0f));
        AnimatorState attackIdle = AddState(controller, "Atk_Idle", clips, "Player_Atk_Idle", new Vector3(260f, 220f, 0f));
        AnimatorState run = AddState(controller, "Run", clips, "Player_Run", new Vector3(520f, 80f, 0f));
        AnimatorState jumpUp = AddState(controller, "Jump_Up", clips, "Player_Jump_UP", new Vector3(520f, -80f, 0f));
        AnimatorState jumpDown = AddState(controller, "Jump_Down", clips, "Player_Jump_Down", new Vector3(780f, -80f, 0f));
        AnimatorState dash = AddState(controller, "Dash", clips, "Player_Dash", new Vector3(780f, 80f, 0f));
        AnimatorState dashEnd = AddState(controller, "Dash_End", clips, "Player_Dash_End", new Vector3(1040f, 80f, 0f));
        AddState(controller, "Death", clips, "Player_Death", new Vector3(780f, 400f, 0f));
        AnimatorState attack = AddState(controller, "Attack_01", clips, "Player_Atk01_01", new Vector3(120f, 400f, 0f));
        AnimatorState attack2 = AddState(controller, "Attack_02", clips, "Player_Atk01_02", new Vector3(360f, 400f, 0f));
        AnimatorState attack3 = AddState(controller, "Attack_03", clips, "Player_Atk01_03", new Vector3(600f, 400f, 0f));

        controller.layers[0].stateMachine.defaultState = idle;
        AddFloatTransition(idle, run, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddFloatTransition(attackIdle, run, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddFloatBoolTransition(run, idle, "Speed", AnimatorConditionMode.Less, 0.1f, "InCombat", false);
        AddFloatBoolTransition(run, attackIdle, "Speed", AnimatorConditionMode.Less, 0.1f, "InCombat", true);
        AddBoolTransition(idle, attackIdle, "InCombat", true);
        AddBoolTransition(attackIdle, idle, "InCombat", false);
        AddFloatBoolTransition(jumpUp, jumpDown, "VerticalSpeed", AnimatorConditionMode.Less, -0.05f, "Grounded", false);
        AddGroundedMoveTransitions(jumpUp, idle, run, attackIdle);
        AddGroundedMoveTransitions(jumpDown, idle, run, attackIdle);
        AddFloatBoolBoolTransition(dash, run, "Speed", AnimatorConditionMode.Greater, 0.1f, "Grounded", true, "Dashing", false);
        AddFloatBoolBoolBoolTransition(dash, idle, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", false, "Dashing", false);
        AddFloatBoolBoolBoolTransition(dash, attackIdle, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", true, "Dashing", false);
        AddFloatBoolBoolTransition(dash, jumpDown, "VerticalSpeed", AnimatorConditionMode.Less, -0.05f, "Grounded", false, "Dashing", false);
        AddFloatBoolBoolTransition(dash, jumpUp, "VerticalSpeed", AnimatorConditionMode.Greater, -0.05f, "Grounded", false, "Dashing", false);
        AddExitGroundedMoveTransitions(dashEnd, idle, run, attackIdle, 0.8f);
        AddTriggerTransition(attack, attack2, "Attack2", 0.03f);
        AddTriggerTransition(attack2, attack3, "Attack3", 0.03f);
        AddExitTransition(attack, attackIdle, 0.98f);
        AddExitTransition(attack2, attackIdle, 0.98f);
        AddExitTransition(attack3, attackIdle, 0.98f);

        return controller;
    }

    private static AnimatorController CreateEnemyController()
    {
        AnimatorController controller = GetOrCreateController(EnemyControllerPath);
        ClearController(controller);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("InCombat", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        Dictionary<string, AnimationClip> clips = LoadClips(EnemyFbxPath);
        AnimatorState idle = AddState(controller, "Idle", clips, "Monster_01_Idle", new Vector3(260f, 80f, 0f));
        AnimatorState attackIdle = AddState(controller, "Atk_Idle", clips, "Monster_01_Atk_Idle", new Vector3(260f, 220f, 0f));
        AnimatorState run = AddState(controller, "Run", clips, "Monster_01_Run", new Vector3(520f, 80f, 0f));
        AnimatorState attack = AddState(controller, "Attack", clips, "Monster_01_Atk_01", new Vector3(650f, 260f, 0f));

        controller.layers[0].stateMachine.defaultState = idle;
        AddFloatTransition(idle, run, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddFloatTransition(run, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
        AddBoolTransition(idle, attackIdle, "InCombat", true);
        AddBoolTransition(run, attackIdle, "InCombat", true);
        AddBoolTransition(attackIdle, idle, "InCombat", false);
        AddTriggerTransition(controller.layers[0].stateMachine, attack, "Attack");
        AddExitTransition(attack, attackIdle, 0.85f);

        return controller;
    }

    private static AnimatorController CreateRangedEnemyController()
    {
        AnimatorController controller = GetOrCreateController(RangedEnemyControllerPath);
        ClearController(controller);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("InCombat", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        Dictionary<string, AnimationClip> clips = LoadClips(RangedEnemyFbxPath);
        AnimatorState idle = AddState(controller, "Idle", clips, "Monster_03_Idle", new Vector3(260f, 80f, 0f));
        AnimatorState attackIdle = AddState(controller, "Atk_Idle", clips, "Monster_03_Atk_Idle", new Vector3(260f, 220f, 0f));
        AnimatorState run = AddState(controller, "Run", clips, "Monster_03_Run", new Vector3(520f, 80f, 0f));
        AnimatorState attack = AddState(controller, "Attack", clips, "Monster_03_Atk_01", new Vector3(650f, 260f, 0f));

        controller.layers[0].stateMachine.defaultState = idle;
        AddFloatTransition(idle, run, "Speed", AnimatorConditionMode.Greater, 0.1f);
        AddFloatTransition(run, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
        AddBoolTransition(idle, attackIdle, "InCombat", true);
        AddBoolTransition(run, attackIdle, "InCombat", true);
        AddBoolTransition(attackIdle, idle, "InCombat", false);
        AddTriggerTransition(controller.layers[0].stateMachine, attack, "Attack");
        AddExitTransition(attack, attackIdle, 0.85f);

        return controller;
    }

    private static AnimatorController GetOrCreateController(string path)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null)
        {
            return controller;
        }

        return AnimatorController.CreateAnimatorControllerAtPath(path);
    }

    private static void ClearController(AnimatorController controller)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
        {
            controller.RemoveParameter(parameter);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState state in stateMachine.states)
        {
            stateMachine.RemoveState(state.state);
        }

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions.ToArray())
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }
    }

    private static Dictionary<string, AnimationClip> LoadClips(string fbxPath)
    {
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath)
            .OfType<AnimationClip>()
            .ToDictionary(clip => clip.name, clip => clip);
    }

    private static AnimatorState AddState(AnimatorController controller, string stateName, Dictionary<string, AnimationClip> clips, string clipName, Vector3 position)
    {
        AnimatorState state = controller.layers[0].stateMachine.AddState(stateName, position);
        if (clips.TryGetValue(clipName, out AnimationClip clip))
        {
            state.motion = clip;
        }
        else
        {
            Debug.LogWarning($"Clip {clipName} not found.");
        }

        return state;
    }

    private static void AddFloatTransition(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameter, bool expected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(expected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
    }

    private static void AddTriggerTransition(AnimatorState from, AnimatorState to, string parameter, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = Mathf.Max(0f, duration);
        transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
    }

    private static void AddFloatBoolTransition(AnimatorState from, AnimatorState to, string floatParameter, AnimatorConditionMode floatMode, float threshold, string boolParameter, bool expected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(floatMode, threshold, floatParameter);
        transition.AddCondition(expected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, boolParameter);
    }

    private static void AddFloatBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string floatParameter,
        AnimatorConditionMode floatMode,
        float threshold,
        string firstBoolParameter,
        bool firstExpected,
        string secondBoolParameter,
        bool secondExpected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(floatMode, threshold, floatParameter);
        transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolParameter);
        transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolParameter);
    }

    private static void AddFloatBoolBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string floatParameter,
        AnimatorConditionMode floatMode,
        float threshold,
        string firstBoolParameter,
        bool firstExpected,
        string secondBoolParameter,
        bool secondExpected,
        string thirdBoolParameter,
        bool thirdExpected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.08f;
        transition.AddCondition(floatMode, threshold, floatParameter);
        transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolParameter);
        transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolParameter);
        transition.AddCondition(thirdExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, thirdBoolParameter);
    }

    private static void AddExitFloatBoolTransition(
        AnimatorState from,
        AnimatorState to,
        float exitTime,
        string floatParameter,
        AnimatorConditionMode floatMode,
        float threshold,
        string boolParameter,
        bool expected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = 0.08f;
        transition.AddCondition(floatMode, threshold, floatParameter);
        transition.AddCondition(expected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, boolParameter);
    }

    private static void AddExitFloatBoolBoolTransition(
        AnimatorState from,
        AnimatorState to,
        float exitTime,
        string floatParameter,
        AnimatorConditionMode floatMode,
        float threshold,
        string firstBoolParameter,
        bool firstExpected,
        string secondBoolParameter,
        bool secondExpected)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = 0.08f;
        transition.AddCondition(floatMode, threshold, floatParameter);
        transition.AddCondition(firstExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, firstBoolParameter);
        transition.AddCondition(secondExpected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, secondBoolParameter);
    }

    private static void AddGroundedMoveTransitions(AnimatorState from, AnimatorState idle, AnimatorState run, AnimatorState attackIdle)
    {
        AddFloatBoolTransition(from, run, "Speed", AnimatorConditionMode.Greater, 0.1f, "Grounded", true);
        AddFloatBoolBoolTransition(from, idle, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", false);
        AddFloatBoolBoolTransition(from, attackIdle, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", true);
    }

    private static void AddExitGroundedMoveTransitions(AnimatorState from, AnimatorState idle, AnimatorState run, AnimatorState attackIdle, float exitTime)
    {
        AddExitFloatBoolTransition(from, run, exitTime, "Speed", AnimatorConditionMode.Greater, 0.1f, "Grounded", true);
        AddExitFloatBoolBoolTransition(from, idle, exitTime, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", false);
        AddExitFloatBoolBoolTransition(from, attackIdle, exitTime, "Speed", AnimatorConditionMode.Less, 0.1f, "Grounded", true, "InCombat", true);
    }

    private static void AddTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState to, string trigger)
    {
        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.04f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = 0.08f;
    }

    private static void FitToHeight(Transform visualRoot, float targetHeight)
    {
        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        if (bounds.size.y <= 0.001f)
        {
            return;
        }

        float scale = targetHeight / bounds.size.y;
        visualRoot.localScale *= scale;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static int GetLayerOrDefault(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        return layer >= 0 ? layer : 0;
    }

    private static Vector3 ResolveRangedEnemySpawnPosition()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            return player.transform.position + new Vector3(7f, 0f, 0f);
        }

        if (Selection.activeTransform != null)
        {
            return Selection.activeTransform.position + new Vector3(3f, 0f, 0f);
        }

        return new Vector3(18f, 1f, 0f);
    }
}
