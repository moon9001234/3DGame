# 腳本職責速查表

這份文件用來快速判斷「我要改某個功能時，應該先看哪支腳本」。

## 玩家

| 功能 | 腳本 |
| --- | --- |
| 移動輸入、角色面向、Free3D / SideScroller 模式 | `PlayerMotor3D` |
| 跳躍、二段跳、跳躍緩衝、離地寬容 | `PlayerMotor3D` |
| 衝刺、空中衝刺、衝刺跳加成、衝刺動畫鎖定 | `PlayerMotor3D` |
| 地面偵測、單向跳板、貼牆修正 | `PlayerMotor3D` |
| 玩家受傷彈飛與操作鎖定 | `PlayerMotor3D` |
| 攻擊輸入、攻擊鎖定、連段流程 | `PlayerCombat3D` |
| 3 段攻擊的動畫、傷害、特效、音效、切換幀 | `PlayerWeaponAttackProfile` |
| 武器命中判定與投射物反彈 | `PlayerWeaponHitbox` |
| 衝刺殘影生成與淡出 | `PlayerDashAfterimage3D` |
| 玩家受傷閃爍 | `PlayerDamageFlash` |
| 玩家死亡或掉落後重生 | `SideScrollerPlayerRespawn` |

## 敵人

| 功能 | 腳本 |
| --- | --- |
| 敵人類型、巡邏、追擊、攻擊模式 | `EnemyPatrol3D` |
| 近戰攻擊範圍與傷害 | `EnemyPatrol3D` |
| 遠程投射物生成、速度、傷害、生命週期 | `EnemyPatrol3D` |
| Boss 近戰、接觸傷害、遠程攻擊、投射物種類 | `EnemyPatrol3D` |
| 敵人受傷擊退、落地恢復、死亡飛出、重生 | `EnemyPatrol3D` |
| 敵人 Animator 參數、視覺朝向 | `EnemyVisualAnimator` |
| 敵人受擊區 | `EnemyHurtbox3D` |
| 敵人受傷閃爍 | `EnemyDamageFlash` |
| 敵人血條 | `EnemyHealthBar3D` |
| 敵人貼地修正 | `EnemyGrounder3D` |
| 碰到玩家造成傷害 | `DamageOnTouch` |

## 戰鬥與傷害共用

| 功能 | 腳本 |
| --- | --- |
| 血量、受傷、死亡事件 | `Health` |
| 命中特效生成 | `DamageHitEffect3D` |
| Boss 死亡後通關 | `BossClearOnDeath3D` |
| 掉落死亡區 | `DeathZone3D` |
| 可反彈投射物 | `ReflectableProjectile3D` |

## 相機與畫面

| 功能 | 腳本 |
| --- | --- |
| 相機跟隨與視角控制 | `SideScrollerCamera` |
| 攝影機震動 | `CameraShake3D` |
| 遮擋物淡出或隱藏 | `CameraOcclusionHider` |
| 高度線與相機參考線 | `CameraLevelLine3D` |
| 轉角觸發與舊橫向卷軸轉向 | `CornerTurnTrigger3D` |
| 視差背景 | `ParallaxLayer3D` |

## UI、音效與流程

| 功能 | 腳本 |
| --- | --- |
| HUD、血量、訊息、控制提示 | `SideScrollerHUD` |
| 背景音樂 | `SideScrollerBackgroundMusic` |
| 共用音效播放 | `SideScrollerSfxPlayer` |
| 通關條件 | `GameClearManager3D` |
| 門或物件通過後顯示 | `DoorRevealAfterPass3D` |
| 離開遊戲 | `QuitGame3D` |

## 設定與 Editor

| 功能 | 腳本 |
| --- | --- |
| 集中讀取與同步數值 | `GameBalanceSettings3D` |
| 集中設定 Inspector | `GameBalanceSettings3DEditor` |
| 玩家移動 Inspector 收納 | `PlayerMotor3DEditor` |
| 敵人 AI Inspector 收納 | `EnemyPatrol3DEditor` |
| 武器攻擊 profile Inspector | `PlayerWeaponAttackProfileEditor` |
| 一般腳本中文化 Inspector | `SideScrollerLocalizedMonoBehaviourEditor` |
| 中文 label / tooltip 讀取 | `SideScrollerInspectorLabels` |
| 玩家 Animator 修復 | `PlayerAnimatorAttackRepair` |
| 場景/Prefab/美術自動配置 | `SideScrollerArtBinder` |

## 常見問題先看哪裡

| 問題 | 先看 |
| --- | --- |
| 玩家衝刺卡住、方向錯、空中衝刺動畫錯 | `PlayerMotor3D` |
| 跳躍上升/下降動畫切換不順 | `PlayerMotor3D`、`PlayerVisual.controller` |
| 攻擊沒有切到第 2 / 第 3 段 | `PlayerCombat3D`、武器 `PlayerWeaponAttackProfile` |
| 攻擊特效位置不對或方向不對 | `PlayerCombat3D`、武器 `PlayerWeaponAttackProfile`、`WeaponAnchor` |
| 命中特效或音效沒出現 | `PlayerWeaponHitbox`、`Health`、`SideScrollerSfxPlayer` |
| 敵人不攻擊、一直推玩家 | `EnemyPatrol3D` 的偵測、攻擊範圍、攻擊模式 |
| Boss 投射物要改種類或距離 | `EnemyPatrol3D > Boss 遠程投射物` |
| 攝影機震動太弱或太強 | `CameraShake3D`、武器 profile 的命中震動設定 |
| Inspector 中文顯示錯或太長 | `Editor/InspectorLabels` 或對應自訂 Editor |
