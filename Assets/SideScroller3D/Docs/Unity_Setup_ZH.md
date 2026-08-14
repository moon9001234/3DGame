# Unity 3D 橫向卷軸原型設定

這包腳本是給「3D 畫面、橫向 2D 操作」的動作遊戲原型使用。角色和敵人都是 3D 物件，但移動鎖在 X/Y 平面，Z 軸固定。

## 建議 Unity 專案

- Template: 3D 或 Universal 3D
- Unity 版本: 2022 LTS 以上
- Input: 先使用舊版 Input Manager 即可

## 場景物件

### Player

建立一個 Capsule 或角色模型，加入：

- `Rigidbody`
- `CapsuleCollider`
- `Health`
- `PlayerMotor3D`
- `PlayerCombat3D`

設定：

- `Rigidbody > Use Gravity`: 開啟
- `Rigidbody > Interpolate`: Interpolate
- 建立子物件 `GroundCheck`，放在腳底
- 在手上建立子物件 `WeaponAnchor`
- 把武器 prefab 掛在 `WeaponAnchor` 底下，武器上需有 `PlayerWeaponHitbox` 與 `PlayerWeaponAttackProfile`
- `PlayerMotor3D > Ground Check`: 指到 `GroundCheck`

### Ground

建立地板 Cube，加入 Collider，並設定 Layer 為 `Ground`。

### Enemy

建立一個 Capsule 或怪物模型，加入：

- `Rigidbody`
- `CapsuleCollider`
- `Health`
- `EnemyPatrol3D`
- `DamageOnTouch`

另外建立兩個空物件：

- `LeftPoint`
- `RightPoint`

把它們指定給 `EnemyPatrol3D` 的巡邏範圍。

### Camera

主攝影機加入：

- `SideScrollerCamera`

把 `Target` 指到 Player。

## Layers

建議建立這些 Layer：

- `Player`
- `Enemy`
- `Ground`

設定：

- Player 物件 Layer: `Player`
- Enemy 物件 Layer: `Enemy`
- 地板 Layer: `Ground`
- `PlayerMotor3D > Ground Mask`: 選 `Ground`
- 武器的 `PlayerWeaponAttackProfile > Target Mask`: 選 `Enemy`
- `DamageOnTouch > Target Mask`: 選 `Player`

## 操作

- 左右移動: A/D 或方向鍵
- 跳躍: Space
- 攻擊: Ctrl 或滑鼠左鍵，依 Unity Input Manager 的 `Fire1`

## 下一步

先確認以下手感：

- 走路速度
- 跳躍高度
- 攻擊距離
- 攻擊冷卻
- 被打擊退

這些都穩了，再加入連段、空中攻擊、副武器、Boss 和關卡機關。
