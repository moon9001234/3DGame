# SideScroller3D 專案整理索引

這份文件是專案的入口地圖。目標是讓你之後回來調整功能時，可以先從這裡判斷「要看哪個腳本」和「數值應該在哪裡改」。

目前建議是先整理入口與職責，不急著把行為拆成很多小腳本。等敵人種類、Boss 行為、玩家手感比較穩定後，再做中大型重構會比較划算。

## 主要資料夾

| 路徑 | 用途 |
| --- | --- |
| `Assets/SideScroller3D/Scripts` | 遊戲執行時腳本。玩家、敵人、相機、傷害、UI、音效都在這裡。 |
| `Assets/SideScroller3D/Editor` | Unity Editor 工具與自訂 Inspector。只在編輯器中使用，不會進入正式遊戲執行邏輯。 |
| `Assets/SideScroller3D/Editor/InspectorLabels` | Inspector 中文顯示文字與 tooltip。 |
| `Assets/SideScroller3D/Prefabs` | 原型 Player / Enemy prefab。 |
| `Assets/SideScroller3D/Animation` | Animator Controller。 |
| `Assets/SideScroller3D/Shaders` | 專案自訂 shader。 |
| `Assets/SideScroller3D/Materials` | SideScroller3D 原型材質。 |
| `Assets/SideScroller3D/Scenes` | 原型場景。 |
| `Assets/SideScroller3D/Docs` | 專案說明文件。 |

## 玩家相關

| 腳本 | 負責內容 | 主要調整位置 |
| --- | --- | --- |
| `PlayerMotor3D` | 移動、跳躍、二段跳、衝刺、衝刺跳、地面偵測、單向跳板、牆面修正、受傷彈飛。 | Player 物件上的 `PlayerMotor3D`，或 `GameBalanceSettings3D > Player Motor` 同步。 |
| `PlayerCombat3D` | 玩家攻擊輸入、攻擊鎖定、連段流程、攻擊動畫切換、武器 profile 讀取。 | Player 物件上的 `PlayerCombat3D`。攻擊數值主要看武器。 |
| `PlayerWeaponAttackProfile` | 武器攻擊資料：每段攻擊動畫、傷害、特效、音效、下一段切換幀。 | 武器 prefab 或武器物件上。 |
| `PlayerWeaponHitbox` | 玩家武器命中判定、命中音效規則、投射物反彈。 | 武器物件或武器子物件上。 |
| `PlayerDashAfterimage3D` | 衝刺殘影的生成、顏色、淡出。 | 通常由 `PlayerMotor3D` 控制，細部可看此腳本。 |
| `PlayerDamageFlash` | 玩家受傷閃爍。 | Player 視覺物件或 Player 物件。 |
| `SideScrollerPlayerRespawn` | 玩家掉落或死亡後重生。 | Player 物件。 |

## 敵人相關

| 腳本 | 負責內容 | 主要調整位置 |
| --- | --- | --- |
| `EnemyPatrol3D` | 敵人巡邏、追擊、近戰、遠程、Boss 行為、受傷擊退、死亡、重生。這是目前最值得之後拆分的腳本。 | 敵人物件上的 `EnemyPatrol3D`，或 `GameBalanceSettings3D > Enemies` 同步。 |
| `EnemyVisualAnimator` | 敵人視覺動畫參數與方向。 | 敵人視覺物件。 |
| `EnemyHurtbox3D` | 敵人被玩家武器打中的受擊區。 | 敵人 hurtbox 子物件。 |
| `EnemyDamageFlash` | 敵人受傷閃爍。 | 敵人視覺物件。 |
| `EnemyHealthBar3D` | 敵人血條顯示與面向相機。 | 敵人血條物件。 |
| `EnemyGrounder3D` | 敵人物理貼地/地面修正。 | 敵人物件。 |
| `DamageOnTouch` | 接觸造成傷害，例如敵人碰到玩家。 | 敵人或傷害觸發器。 |

## 傷害與生命

| 腳本 | 負責內容 |
| --- | --- |
| `Health` | 血量、受傷、死亡事件。玩家與敵人共用。 |
| `DamageHitEffect3D` | 受傷或命中時生成特效。 |
| `BossClearOnDeath3D` | Boss 死亡後觸發通關。 |
| `DeathZone3D` | 掉落死亡區與 runtime setup。 |

## 相機、場景與 UI

| 腳本 | 負責內容 |
| --- | --- |
| `SideScrollerCamera` | 相機跟隨、鏡頭區域、橫向卷軸舊邏輯。 |
| `CameraShake3D` | 攝影機震動。玩家/敵人命中可呼叫它加強打擊感。 |
| `CameraOcclusionHider` | 相機和角色之間遮擋物淡出或隱藏。 |
| `CameraLevelLine3D` | 相機高度線、轉角參考線。 |
| `CornerTurnTrigger3D` | 舊橫向卷軸轉角切換。 |
| `SideScrollerHUD` | 血量、訊息、控制提示等 UI。 |
| `GameClearManager3D` | 通關條件管理。 |
| `DoorRevealAfterPass3D` | 玩家通過後門或物件顯示。 |
| `ParallaxLayer3D` | 背景視差。 |
| `OneWayPlatform3D` | 單向跳板本體。 |

## 音效

| 腳本 | 負責內容 |
| --- | --- |
| `SideScrollerSfxPlayer` | 共用 one-shot 音效播放，避免背景音樂被當成命中特效重複播放。 |
| `SideScrollerBackgroundMusic` | 背景音樂管理。 |
| `PlayerHitSoundRule` | 玩家武器命中不同目標時的音效規則。 |

## 設定入口

| 腳本 | 用途 |
| --- | --- |
| `GameBalanceSettings3D` | 集中調整數值。可以從場景讀取目前值，再同步回 Player、敵人、武器、相機震動。 |
| `GameBalanceSettings3DEditor` | `GameBalanceSettings3D` 的自訂 Inspector 與同步按鈕。 |
| `PlayerMotor3DEditor` | `PlayerMotor3D` 摺疊式 Inspector。 |
| `EnemyPatrol3DEditor` | `EnemyPatrol3D` 摺疊式 Inspector。 |
| `PlayerWeaponAttackProfileEditor` | 武器攻擊 profile 的自訂 Inspector。 |
| `SideScrollerLocalizedMonoBehaviourEditor` | 一般腳本的中文化 Inspector fallback。 |
| `SideScrollerInspectorLabels` | 讀取 Inspector 中文標籤與 tooltip。 |

## Editor 工具

| 工具腳本 | 用途 |
| --- | --- |
| `SideScrollerArtBinder` | 綁定原型美術、建立 controller、配置場景物件。 |
| `PlayerAnimatorAttackRepair` | 修復玩家 Animator 的攻擊、Dash、Jump 相關狀態。 |
| `SideScrollerPrototypeBuilder` | 建立原型場景與基本物件。 |
| `CameraAnchorPrototypeInstaller` | 安裝相機錨點、相機相關原型配置。 |
| `SampleScene02CameraInstaller` | Sample 02 相機配置。 |
| `OneWayPlatformInstaller` | 單向跳板配置工具。 |
| `EnemyContactDamageInstaller` | 敵人接觸傷害配置工具。 |
| `CharacterVisibilityFixInstaller` | 修復角色 SkinnedMeshRenderer 顯示問題。 |
| `PrototypeShadowSettingsInstaller` | 原型陰影設定。 |
| `SideScrollerBackgroundBuilder` | 建立視差背景。 |
| `FlameFxBuilder` | 建立火焰特效原型。 |
| `DigitalRainSkyboxInstaller` | 建立數位雨 skybox。 |
| `WebGLMaterialCompatibilityInstaller` | WebGL 材質相容修正。 |
| `SideScrollerWindowsBuilder` / `SideScrollerWebGLBuilder` | 打包工具。 |

## Shaders

| Shader | 用途 |
| --- | --- |
| `ToonTextureVerticalGradient` | 可指定貼圖的 Toon shader，支援沿 X/Y/Z 軸由下往上的覆蓋漸層。 |
| `AdditiveTextureTransparent` | 特效用 Add 發亮 shader，支援透明貼圖與頂點色。 |
| `TransparentGlowCube` | 透明發光方塊。 |
| `ToonDotShadow` | Toon 陰影效果。 |
| `BoxFullEdgeOutline` | 盒狀外框效果。 |
| `DigitalRainSkybox` | 數位雨天空盒。 |

## 現在不急著拆的原因

目前玩家手感、敵人 AI、Boss、攻擊表演、特效都還在調整。現在如果把行為拆成很多小腳本，每次修改會需要跨更多檔案找流程，反而更容易忘記誰負責什麼。

比較好的順序是：

1. 先維持主腳本，但把 Inspector 收納整理好。
2. 把容易替換的數值放到 profile，例如武器攻擊資料。
3. 等規則穩定後，再拆大型行為腳本。

## 之後最適合拆的地方

| 優先度 | 目前腳本 | 建議拆法 | 觸發時機 |
| --- | --- | --- | --- |
| 高 | `EnemyPatrol3D` | `EnemyMovement3D`、`EnemyDetection3D`、`EnemyAttack3D`、`EnemyDamageReaction3D`、`EnemyRespawn3D`、`EnemyProfile` | 敵人種類增加、Boss 行為固定、同一份 AI 常常改到很痛時。 |
| 中 | `PlayerMotor3D` | `PlayerGroundCheck3D`、`PlayerDashController3D`、`PlayerJumpController3D` | 玩家移動手感穩定後。 |
| 中 | `PlayerWeaponHitbox` | 命中判定、投射物反彈、音效規則分離 | 武器種類變多後。 |
| 低 | 相機相關 | 相機跟隨、遮擋、震動、轉角工具保持分開即可 | 目前已經算清楚。 |

## 效能注意清單

這些不是現在一定有問題，但之後場景變大或敵人變多時要注意：

| 類型 | 目前常見位置 | 建議 |
| --- | --- | --- |
| 每幀 `Update` / `FixedUpdate` / `LateUpdate` | `PlayerMotor3D`、`PlayerCombat3D`、`EnemyPatrol3D`、相機/血條/投射物/殘影 | 敵人數量增加後，優先檢查 `EnemyPatrol3D.FixedUpdate`。 |
| runtime 搜尋物件 | `Camera.main`、`FindFirstObjectByType`、`FindObjectsOfType` | 每幀搜尋要避免；Awake/Start/Editor 工具裡可以接受。 |
| Physics 查詢 | 玩家地面偵測、敵人偵測、武器 hitbox、單向跳板 | 儘量使用 LayerMask、固定大小陣列、降低不必要查詢。 |
| 特效生成 | 攻擊特效、命中特效、殘影 | 特效很多時再考慮 object pool。現在先保持可調與可看懂。 |

## 存讀檔原則

不要直接存 MonoBehaviour。建議只存穩定資料：

- 目前場景或關卡 ID
- 玩家位置、血量、目前武器 ID
- 已取得道具、門是否開啟
- 敵人是否死亡或是否需要重生
- Boss 是否已擊敗
- 遊戲進度 flag

武器攻擊數值、敵人數值、shader 參數這類設計資料，應該留在 prefab/profile/scene 設定，不需要進玩家存檔。

## 下次要找功能時

| 你想改的東西 | 優先看 |
| --- | --- |
| 玩家移動、跳躍、衝刺、卡牆、單向跳板 | `PlayerMotor3D` |
| 玩家連段、攻擊動畫切換、攻擊鎖定 | `PlayerCombat3D` |
| 每段攻擊傷害、特效、音效、動畫 clip | 武器上的 `PlayerWeaponAttackProfile` |
| 武器命中範圍、反彈投射物 | `PlayerWeaponHitbox` |
| 敵人巡邏、追擊、攻擊、Boss 行為 | `EnemyPatrol3D` |
| 命中後震動 | `CameraShake3D` 與武器 profile 裡的震動設定 |
| 血量、死亡事件 | `Health` |
| Inspector 中文文字 | `Editor/InspectorLabels` |
| 集中調數值 | `GameBalanceSettings3D` |
