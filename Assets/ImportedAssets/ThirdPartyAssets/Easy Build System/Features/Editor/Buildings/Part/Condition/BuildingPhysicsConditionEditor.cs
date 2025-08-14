/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Editor.Buildings.Part.Condition
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

namespace EasyBuildSystem.Features.Editor.Buildings.Part.Condition
{
    [CustomEditor(typeof(BuildingPhysicsCondition))]
    public class BuildingPhysicsConditionEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingPhysicsCondition Target
        {
            get
            {
                return ((BuildingPhysicsCondition)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IsSleeping"),
                new GUIContent("Building Physics Is Sleeping", "Specifies if the physics condition is in a sleeping state."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FallingTime"),
                new GUIContent("Building Physics Falling Time", "The duration in seconds before the gameObject is destroyed after being affected by physics."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlaceOnlyIfStable"),
                new GUIContent("Building Physics Can Only Place When Stable", "Determines if the Building Part can only be placed when it is stable."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CheckStabilityInterval"),
                new GUIContent("Building Physics Check Stability Interval",
                "The frequency at which the CheckStability method is called. Lower values can impact performance, so it is recommended not to set it too low."));

            GUILayout.BeginHorizontal();
            GUILayout.Space(13);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Points"),
                new GUIContent("Building Physics Points",
                "The physics points that need to hit a collider for the condition to return a stable value."));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), new GUIContent("Show Debugs"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            GUI.enabled = !serializedObject.FindProperty("m_IsSleeping").boolValue && (Target.Points != null && Target.Points.Length != 0);
            if (GUILayout.Button("Check Physics Stability..."))
            {
                if (!Target.CheckStability())
                {
                    Debug.Log("<b>Easy Build System</b> : The Building Part is unstable.");
                }
                else
                {
                    Debug.Log("<b>Easy Build System</b> : The Building Part is stable.");
                }
            }
            GUI.enabled = true;

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