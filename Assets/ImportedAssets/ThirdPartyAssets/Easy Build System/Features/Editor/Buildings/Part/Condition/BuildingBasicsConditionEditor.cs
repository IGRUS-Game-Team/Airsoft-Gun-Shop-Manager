/// <summary>
/// Project : Easy Build System
/// Class : BuildingBasicsConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part.Condition
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part.Condition
{
    [CustomEditor(typeof(BuildingBasicsCondition))]
    public class BuildingBasicsConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlacing"),
                new GUIContent("Building Can Placing", "Toggle to enable or disable the ability to place the Building Part."));

            if (serializedObject.FindProperty("m_CanPlacing").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequireArea"),
                    new GUIContent("Building Can Placing Only Area", "Toggle to require a Building Area for the Building Part to be placed."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequireSocket"),
                    new GUIContent("Building Can Placing Only Socket", "Toggle to require the Building Part to be snapped on a Building Socket for placement."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanEditing"),
                new GUIContent("Building Can Editing", "Toggle to enable or disable the ability to edit the Building Part."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanDestroying"),
                new GUIContent("Building Can Destroying", "Toggle to enable or disable the ability to destroy the Building Part."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreSocket"),
                new GUIContent("Building Ignore Snapping", "Toggle to ignore snapping with all Building Sockets."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"),
                new GUIContent("Show Debugs", "Toggle to show or hide debug information."));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Internal Methods

        #endregion
    }
}