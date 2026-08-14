using UnityEditor;
using UnityEngine;

public class AdditiveTextureTransparentShaderGUI : ShaderGUI
{
    private const string LabelOwnerName = "AdditiveTextureTransparent";

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();

        for (int i = 0; i < properties.Length; i++)
        {
            MaterialProperty property = properties[i];
            if (property == null)
            {
                continue;
            }

            DrawSectionIfNeeded(property.name);
            GUIContent label = SideScrollerInspectorLabels.Content(
                LabelOwnerName,
                property.name,
                property.displayName);
            materialEditor.ShaderProperty(property, label);
        }
    }

    private static void DrawSectionIfNeeded(string propertyName)
    {
        string section = SideScrollerInspectorLabels.Section(LabelOwnerName + "." + propertyName, string.Empty);
        if (string.IsNullOrEmpty(section))
        {
            return;
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(section, EditorStyles.boldLabel);
    }
}
