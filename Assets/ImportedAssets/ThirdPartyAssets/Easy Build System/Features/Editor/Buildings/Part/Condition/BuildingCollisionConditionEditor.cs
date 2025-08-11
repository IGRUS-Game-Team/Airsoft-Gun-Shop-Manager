/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part.Condition
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part.Condition
{
    [CustomEditor(typeof(BuildingCollisionCondition))]
    public class BuildingCollisionConditionEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingCollisionCondition Target
        {
            get
            {
                return ((BuildingCollisionCondition)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LayerMask"), new GUIContent("Building Collision Layers",
                "Layers that will be taken into account during collision detection."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequiredBuildingPartCollision"), new GUIContent("Required Building Part Collision",
                "Toggle to enable the required collision detection with other building parts, allowing for placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequireBuildingSurfaces"), new GUIContent("Required Building Surface Collision",
                "Requires that building parts be placed on designated building surfaces. When enabled, parts can only be placed where a valid building surface is detected."));

            if (serializedObject.FindProperty("m_RequireBuildingSurfaces").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingSurfaceTags"), new GUIContent("Required Building Surface By Tag(s)",
                    "Building surface tag(s) required to allows the placement."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Tolerance"), new GUIContent("Building Collision Tolerance",
                "Collision tolerance for allowing the placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreventOverlapping"), new GUIContent("Prevent Overlapping Collision",
                "Prevents overlapping between building parts."));

            if (serializedObject.FindProperty("m_PreventOverlapping").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OverlappingRaycastLength"), new GUIContent("Overlapping Raycast Length",
                    "Length of the ray used to detect the building parts below (or higher if negative)."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreOverlappingTypes"), new GUIContent("Ignore Overlapping Building Types",
                    "Ignore the overlap checks for specified building part types included in this array."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), new GUIContent("Show Debugs"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}