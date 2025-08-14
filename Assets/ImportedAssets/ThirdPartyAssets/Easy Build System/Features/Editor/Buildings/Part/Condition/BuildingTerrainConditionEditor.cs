/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part.Condition
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part.Condition
{
    [CustomEditor(typeof(BuildingTerrainCondition))]
    public class BuildingTerrainConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassDetails"),
                new GUIContent("Building Terrain Clear Grass", "Clear grass details on the terrain at placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassRadius"),
                new GUIContent("Building Terrain Clear Grass Radius", "The radius within which to clear grass details on the terrain."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}