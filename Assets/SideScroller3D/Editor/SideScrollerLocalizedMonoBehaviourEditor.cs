using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class SideScrollerLocalizedMonoBehaviourEditor : Editor
{
    private const string ScriptRoot = "Assets/SideScroller3D/Scripts/";

    private void OnEnable()
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();
    }

    public override void OnInspectorGUI()
    {
        if (!ShouldUseLocalizedInspector())
        {
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();
        DrawScriptField();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        string ownerTypeName = target.GetType().Name;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.propertyPath == "m_Script")
            {
                continue;
            }

            DrawSectionIfNeeded(ownerTypeName, property);
            GUIContent label = SideScrollerInspectorLabels.Content(
                ownerTypeName,
                property.name,
                ObjectNames.NicifyVariableName(property.name));
            EditorGUILayout.PropertyField(property, label, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool ShouldUseLocalizedInspector()
    {
        MonoBehaviour behaviour = target as MonoBehaviour;
        if (behaviour == null)
        {
            return false;
        }

        MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
        if (script == null)
        {
            return false;
        }

        string assetPath = AssetDatabase.GetAssetPath(script);
        return assetPath.StartsWith(ScriptRoot, StringComparison.Ordinal);
    }

    private void DrawSectionIfNeeded(string ownerTypeName, SerializedProperty property)
    {
        string section = SideScrollerInspectorLabels.Section(ownerTypeName + "." + property.name, string.Empty);
        if (string.IsNullOrEmpty(section))
        {
            return;
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(section, EditorStyles.boldLabel);
    }

    private void DrawScriptField()
    {
        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
        if (scriptProperty == null)
        {
            return;
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(scriptProperty);
        }
    }
}
