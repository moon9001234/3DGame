using UnityEditor;
using UnityEngine;

public class ToonTextureVerticalGradientShaderGUI : ShaderGUI
{
    private const string LabelOwnerName = "ToonTextureVerticalGradient";
    private const string UseXAxisName = "_GradientUseXAxis";
    private const string UseYAxisName = "_GradientUseYAxis";
    private const string UseZAxisName = "_GradientUseZAxis";

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();

        MaterialProperty useXAxis = FindProperty(UseXAxisName, properties);
        MaterialProperty useYAxis = FindProperty(UseYAxisName, properties);
        MaterialProperty useZAxis = FindProperty(UseZAxisName, properties);

        EnsureSingleAxis(useXAxis, useYAxis, useZAxis);

        for (int i = 0; i < properties.Length; i++)
        {
            MaterialProperty property = properties[i];
            if (property == null || IsAxisProperty(property))
            {
                continue;
            }

            GUIContent label = SideScrollerInspectorLabels.Content(
                LabelOwnerName,
                property.name,
                property.displayName);
            DrawSectionIfNeeded(property.name);
            materialEditor.ShaderProperty(property, label);

            if (property.name == "_GradientEndColor")
            {
                DrawAxisSelector(useXAxis, useYAxis, useZAxis);
            }
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

    private static void DrawAxisSelector(
        MaterialProperty useXAxis,
        MaterialProperty useYAxis,
        MaterialProperty useZAxis)
    {
        EditorGUILayout.LabelField(
            SideScrollerInspectorLabels.Text("ToonTextureVerticalGradient.gradientAxis", "Gradient Axis"),
            EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int selectedAxis = CurrentAxisIndex(useXAxis, useYAxis, useZAxis);
        selectedAxis = GUILayout.Toolbar(selectedAxis, new[]
        {
            SideScrollerInspectorLabels.Text("ToonTextureVerticalGradient.axisX", "X"),
            SideScrollerInspectorLabels.Text("ToonTextureVerticalGradient.axisY", "Y"),
            SideScrollerInspectorLabels.Text("ToonTextureVerticalGradient.axisZ", "Z")
        });
        if (EditorGUI.EndChangeCheck())
        {
            SetAxis(useXAxis, useYAxis, useZAxis, selectedAxis);
        }
    }

    private static void EnsureSingleAxis(
        MaterialProperty useXAxis,
        MaterialProperty useYAxis,
        MaterialProperty useZAxis)
    {
        float selectedCount = AxisValue(useXAxis) + AxisValue(useYAxis) + AxisValue(useZAxis);
        if (Mathf.Approximately(selectedCount, 1f))
        {
            return;
        }

        int selectedAxis = AxisValue(useXAxis) > 0.5f
            ? 0
            : (AxisValue(useYAxis) > 0.5f ? 1 : 2);
        SetAxis(useXAxis, useYAxis, useZAxis, selectedAxis);
    }

    private static int CurrentAxisIndex(
        MaterialProperty useXAxis,
        MaterialProperty useYAxis,
        MaterialProperty useZAxis)
    {
        if (AxisValue(useXAxis) > 0.5f)
        {
            return 0;
        }

        if (AxisValue(useYAxis) > 0.5f)
        {
            return 1;
        }

        return 2;
    }

    private static void SetAxis(
        MaterialProperty useXAxis,
        MaterialProperty useYAxis,
        MaterialProperty useZAxis,
        int selectedAxis)
    {
        useXAxis.floatValue = selectedAxis == 0 ? 1f : 0f;
        useYAxis.floatValue = selectedAxis == 1 ? 1f : 0f;
        useZAxis.floatValue = selectedAxis == 2 ? 1f : 0f;
    }

    private static float AxisValue(MaterialProperty property)
    {
        return property != null ? property.floatValue : 0f;
    }

    private static bool IsAxisProperty(MaterialProperty property)
    {
        return property.name == UseXAxisName
            || property.name == UseYAxisName
            || property.name == UseZAxisName;
    }
}
