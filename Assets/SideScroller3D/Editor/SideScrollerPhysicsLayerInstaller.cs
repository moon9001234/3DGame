using UnityEditor;
using UnityEngine;

public static class SideScrollerPhysicsLayerInstaller
{
    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u555f\u7528\u73a9\u5bb6\u8207\u6575\u4eba\u78b0\u649e")]
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
