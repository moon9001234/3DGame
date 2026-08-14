using UnityEditor;
using UnityEngine;

public static class SideScrollerPhysicsLayerInstaller
{
    [MenuItem("Tools/3D 遊戲工具/啟用玩家與敵人碰撞")]
    public static void Install()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (playerLayer < 0 || enemyLayer < 0)
        {
            Debug.LogError("Player or Enemy layer is missing.");
            return;
        }

        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        AssetDatabase.SaveAssets();
        Debug.Log("Enabled physical collision between Player and Enemy layers.");
    }
}
